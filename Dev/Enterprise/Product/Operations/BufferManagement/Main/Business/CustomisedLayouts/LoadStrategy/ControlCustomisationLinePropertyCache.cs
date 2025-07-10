using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public class ControlCustomisationLinePropertyCache
	{
		public ControlCustomisationLinePropertyCache(BMControlCustomisationLine line, IEnumerable<ProcessTask> tasks, IEnumerable<ProcessHeader> headers)
		{
			propertySource = line.PropertySource;
			PropertyType = line.PropertyType;
			fieldValues = GetFieldValues(line, tasks, headers);
			bindingPath = line.GetBindingPath();
		}

		readonly Dictionary<ZGuid, IZType> fieldValues;
		readonly ZString propertySource;
		readonly string bindingPath;

		internal Type PropertyType { get; private set; }

		Dictionary<ZGuid, IZType> GetFieldValues(BMControlCustomisationLine line, IEnumerable<ProcessTask> tasks, IEnumerable<ProcessHeader> headers)
		{
			if (propertySource == PropertySourceList.Codes.ProcessTask)
			{
				return tasks.ToDictionary(task => task.PK, task => GetPropertyValue(task, line));
			}
			else if (propertySource == PropertySourceList.Codes.Workflow)
			{
				return headers.ToDictionary(workflow => workflow.PK, workflow => GetPropertyValue(workflow, line));
			}
			else if (propertySource == PropertySourceList.Codes.Job)
			{
				return headers.ToDictionary(job => job.PK, job => GetPropertyValueFromMacro(job, line));
			}
			else
			{
				throw new InvalidOperationException("Unknown Property Source");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1121:DoNotIncludeColumnValuesOrNamesInErrorReporterKey", Justification = "Baseline")]
		IZType GetPropertyValueFromMacro(BusinessObject bizo, BMControlCustomisationLine line)
		{
			var processHeader = bizo as ProcessHeader;

			if (processHeader != null)
			{
				var notification = new NotificationBuffer();
				var workflowProvider = processHeader.Parent;

				return new WorkflowMacroEvaluator(notification, null)
					.GetValue(
						(IBusiness)workflowProvider,
						Utilities.TrimExpression(line.PropertyName),
						out bool returnDefaultValue)?.FieldValue;
			}

			if (PropertyType != null)
			{
				return ZDataType.ZTypeToEmptyValue(PropertyType);
			}
			else
			{
				ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "Unable to determine a value for property [{0}] for bizo [{1}] on layout [{2}]", line.PropertyName, bizo.HumanReadableName, line.Parent.FM_Name));

				return ZString.Empty;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1121:DoNotIncludeColumnValuesOrNamesInErrorReporterKey", Justification = "Baseline")]
		IZType GetPropertyValue(BusinessObject bizo, BMControlCustomisationLine line)
		{
			var propertyInfo = bizo.ZPropertyInfoHash.GetPropertySafe(line.PropertyName);

			if (propertyInfo != null)
			{
				return propertyInfo.Value;
			}
			else if (recognisedErrorProperties == null || !IsRecognisedProblemLine(line.PropertyName, bizo.GetType()))
			{
				try
				{
					return (IZType)bizo[line.PropertyName];
				}
				catch (ArgumentException)
				{
					recognisedErrorProperties = recognisedErrorProperties ?? new HashSet<Tuple<ZString, Type>>();
					recognisedErrorProperties.Add(Tuple.Create(line.PropertyName, bizo.GetType()));
				}
			}

			if (PropertyType != null)
			{
				return ZDataType.ZTypeToEmptyValue(PropertyType);
			}
			else
			{
				ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "Unable to determine a value for property [{0}] for bizo [{1}] on layout [{2}]", line.PropertyName, bizo.HumanReadableName, line.Parent.FM_Name));

				return ZString.Empty;
			}
		}

		bool IsRecognisedProblemLine(ZString propertyName, Type type)
		{
			return recognisedErrorProperties.Contains(Tuple.Create(propertyName, type));
		}

		HashSet<Tuple<ZString, Type>> recognisedErrorProperties;

		public IZType GetValue(ICardContent cardContent)
		{
			switch (propertySource)
			{
				case PropertySourceList.Codes.ProcessTask:
					return fieldValues[cardContent.TaskIdentifier];

				case PropertySourceList.Codes.Workflow:
				case PropertySourceList.Codes.Job:
					return fieldValues[cardContent.WorkflowIdentifier];

				default:
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Invalid line load strategy configuration. Property source [{0}] unrecognised.", propertySource));
			}
		}

		public ICustomProperty GetCustomProperty(ICardContent content)
		{
			return new CustomProperty(this, content);
		}

		class CustomProperty : ICustomProperty
		{
			readonly ControlCustomisationLinePropertyCache lineLoadStrategy;
			readonly ICardContent cardContent;
			readonly DynamicBusinessObjectProperty propertyInfo;

			public CustomProperty(ControlCustomisationLinePropertyCache cache, ICardContent content)
			{
				lineLoadStrategy = cache;
				cardContent = content;
				propertyInfo = GetProperty();
			}

			#region ICustomProperty Members

			object ICustomProperty.GetValue(BusinessObject parent)
			{
				return GetValue();
			}

			DynamicBusinessObjectProperty ICustomProperty.Info
			{
				get { return propertyInfo; }
			}

			string ICustomProperty.Identifier
			{
				get { return lineLoadStrategy.bindingPath; }
			}

			bool ICustomProperty.TrySetValue(BusinessObject parent, object value)
			{
				throw new NotImplementedException("Why are you editing a read only card?");
			}

			void ICustomProperty.Validate(BusinessObject parent)
			{
			}

			IEnumerable<ICustomProperty> ICustomProperty.RelatedProperties => Enumerable.Empty<ICustomProperty>();

			ICustomColumnDefinition ICustomProperty.CustomColumnDefinition => null;

			bool ICustomProperty.IsDeleted => false;

			#endregion

			#region Implementation

			DynamicBusinessObjectProperty GetProperty()
			{
				var value = GetValue();
				var type = value != null ? value.GetType() : lineLoadStrategy.PropertyType;

				return new DynamicBusinessObjectProperty(type, readOnly: true);
			}

			object GetValue()
			{
				return lineLoadStrategy.GetValue(cardContent);
			}

			#endregion
		}
	}
}
