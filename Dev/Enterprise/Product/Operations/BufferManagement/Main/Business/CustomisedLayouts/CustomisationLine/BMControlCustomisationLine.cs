using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class BMControlCustomisationLine : ControlCustomisationBase, IRootTypeProvider
	{
		public BMControlCustomisationLine(BMControlCustomisation parent)
			: base(parent)
		{
		}

		#region Properties

		#region PropertySource

		[XmlColumnProperty]
		[List("Lookups.PropertySources")]
		[ResourceStringData("BMControlCustomisationLine.PropertySource", Caption = "Source", FullDescription = "The source of the data this property will draw from.")]
		[MaxLength(3)]
		public ZString PropertySource
		{
			get { return GetXmlColumnPropertyValue<ZString>(PropertySourceInfo); }
			set
			{
				using (this.LayoutConfigUpdatedOnDisposeIfValueChanged(PropertySource, value))
				{
					SetXmlColumnPropertyValue(PropertySourceInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidatePropertySource();
					}

					if (!PropertySourceInfo.HasErrors())
					{
						PropertyName = ZString.Empty;
					}
				}
			}
		}

		public ZPropertyInfo PropertySourceInfo
		{
			get { return GetZPropertyInfo(nameof(PropertySource)); }
		}

		#endregion

		#region PropertySourceDescription

		[List("Lookups.PropertySourceDescriptions")]
		[BusinessObjectTestExclude] // We filter values based on valid codes
		[ResourceStringData("BMControlCustomisationLine.PropertySourceDescription", Caption = "Source", FullDescription = "The source of the data this property will draw from.")]
		[MaxLength(10)]
		public ZString PropertySourceDescription
		{
			get { return Lookups.PropertySources.GetDescriptionFromCode(PropertySource); }
			set
			{
				PropertySource = Lookups.PropertySources.GetCodeFromDescription(value);
				PropertySourceDescriptionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo PropertySourceDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(PropertySourceDescription)); }
		}

		#endregion

		#region PropertyName

		[XmlColumnProperty]
		[List("Lookups.PropertyNames")]
		[ResourceStringData("BMControlCustomisationLine.PropertyName", Caption = "Property Name", ShortCaption = "Property")]
		public ZString PropertyName
		{
			get { return GetXmlColumnPropertyValue<ZString>(PropertyNameInfo); }
			set
			{
				using (this.LayoutConfigUpdatedOnDisposeIfValueChanged(PropertyName, value))
				{
					SetXmlColumnPropertyValue(PropertyNameInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidatePropertyName();
						Validation.ValidateControlType();
					}

					if (!PropertyNameInfo.HasErrors())
					{
						if (IsCustomField)
						{
							Label = CustomFieldPropertyName;
							var controlType = CalculateCustomPropertyType();
							if (!controlType.IsEmpty)
							{
								ControlType = controlType;
							}
						}
						else
						{
							Label = Lookups.PropertyNames.GetDescriptionFromCode(value);
							var controlType = CalculatePropertyType();
							if (!controlType.IsEmpty)
							{
								ControlType = controlType;
							}
						}
					}
				}
			}
		}

		public ZPropertyInfo PropertyNameInfo
		{
			get { return GetZPropertyInfo(nameof(PropertyName)); }
		}

		#endregion

		#region PropertyNameDescription

		[List("Lookups.PropertyNameDescriptions")]
		[ResourceStringData("BMControlCustomisationLine.PropertyNameDescription", Caption = "Property Name", ShortCaption = "Property")]
		public ZString PropertyNameDescription
		{
			get
			{
				if (PropertySource == PropertySourceList.Codes.Job)
				{
					return PropertyName;
				}
				else
				{
					return Lookups.PropertyNames.GetDescriptionFromCode(PropertyName);
				}
			}
			set
			{
				if (PropertySource == PropertySourceList.Codes.Job)
				{
					PropertyName = value;
				}
				else
				{
					PropertyName = Lookups.PropertyNames.GetCodeFromDescription(value);
				}
				PropertyNameDescriptionInfo.RefreshBinding();
				PropertyNameInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo PropertyNameDescriptionInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(PropertyNameDescription), _ => PropertyNameInfo); }
		}

		[List("Lookups.PropertyNameDescriptions")]
		[ReadOnlyMember(nameof(PropertyNameDescriptionForDropEditBinding_ReadOnly))]
		[ResourceStringData("BMControlCustomisationLine.PropertyNameDescriptionForDropEditBinding", Caption = "Property Name", ShortCaption = "Property")]
		public ZString PropertyNameDescriptionForDropEditBinding
		{
			get { return PropertyNameDescription; }
			set { PropertyNameDescription = value; }
		}

		protected bool PropertyNameDescriptionForDropEditBinding_ReadOnly => PropertySource == PropertySourceList.Codes.Job;

		#endregion

		#region ControlType

		[XmlColumnProperty]
		[ResourceStringData("BMControlCustomisationLine.PropertyType", Caption = "Property Type", ShortCaption = "Type")]
		public override ZString ControlType
		{
			get => base.ControlType;
			set
			{
				base.ControlType = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateControlType();
					Validation.ValidateBackgroundColor();
				}
			}
		}

		ZString CalculatePropertyType()
		{
			var property = Property;
			if (property != null)
			{
				if (property.PropertyType == typeof(ZString))
				{
					return PropertyTypeList.Codes.Text;
				}
				else if (property.PropertyType == typeof(ZDateTime))
				{
					if (property.Name.Contains((NoResString)"Duration")) // This is a schema column name convention.
					{
						return PropertyTypeList.Codes.Duration;
					}
					else
					{
						return PropertyTypeList.Codes.DateTime;
					}
				}
				else if (property.PropertyType == typeof(ZBool))
				{
					return PropertyTypeList.Codes.Boolean;
				}
				else if (typeof(INumericZType).IsAssignableFrom(property.PropertyType))
				{
					return PropertyTypeList.Codes.Number;
				}
			}

			return ZString.Empty;
		}

		ZString CalculateCustomPropertyType()
		{
			if (CustomFieldPropertyType != null)
			{
				if (CustomFieldPropertyType == typeof(ZString))
				{
					return PropertyTypeList.Codes.Text;
				}
				else if (CustomFieldPropertyType == typeof(ZDateTime))
				{
					return PropertyTypeList.Codes.DateTime;
				}
				else if (CustomFieldPropertyType == typeof(ZBool))
				{
					return PropertyTypeList.Codes.Boolean;
				}
				else if (typeof(INumericZType).IsAssignableFrom(CustomFieldPropertyType))
				{
					return PropertyTypeList.Codes.Number;
				}
			}

			return ZString.Empty;
		}

		#endregion

		#region AutoSize

		[XmlColumnProperty]
		[ResourceStringData("ControlCustomisationBase.AutoSize", Caption = "Auto Size", FullDescription = "Causes the field to expand to fit its content.")]
		public ZBool AutoSize
		{
			get { return GetXmlColumnPropertyValue<ZBool>(AutoSizeInfo); }
			set
			{
				using (this.LayoutConfigUpdatedOnDisposeIfValueChanged(AutoSize, value))
				{
					SetXmlColumnPropertyValue(AutoSizeInfo, value);
				}
			}
		}

		public ZPropertyInfo AutoSizeInfo
		{
			get { return GetZPropertyInfo(nameof(AutoSize)); }
		}

		#endregion

		#endregion

		#region New Properties

		public Type SourceType
		{
			get
			{
				switch (PropertySource)
				{
					case PropertySourceList.Codes.ProcessTask:
						return typeof(ProcessTask);

					case PropertySourceList.Codes.Workflow:
						return typeof(ProcessHeader);

					case PropertySourceList.Codes.Job:
						var workflowDescriptor = WorkflowDescriptors.Instance.TryGetValueSafe(Parent.FM_JobType);
						return workflowDescriptor != null ? workflowDescriptor.WorkflowProviderType : null;

					default:
						return null;
				}
			}
		}

		public PropertyInfo Property
		{
			get
			{
				if (SourceType != null)
				{
					switch (PropertySource)
					{
						case PropertySourceList.Codes.Job:
							return new WorkflowMacroEvaluator(null, null).GetFinalPropertyInfo(SourceType, PropertyName);

						default:
							return SourceType.GetProperty(PropertyName);
					}
				}
				return null;
			}
		}

		public Type PropertyType
		{
			get
			{
				var property = Property;

				if (property != null)
				{
					return property.PropertyType;
				}
				else
				{
					switch (ControlType)
					{
						case PropertyTypeList.Codes.Text:
							return typeof(ZString);

						case PropertyTypeList.Codes.Duration:
						case PropertyTypeList.Codes.DateTime:
						case PropertyTypeList.Codes.Date:
							return typeof(ZDateTime);

						case PropertyTypeList.Codes.Boolean:
							return typeof(ZBool);

						case PropertyTypeList.Codes.Number:
							return typeof(ZDecimal);
					}
				}

				return null;
			}
		}

		public Type CustomFieldPropertyType
		{
			get
			{
				var customField = Factory.LoadTop1<GenCustomColumnDefinition>(new ZQuery(GenCustomColumnDefinitionSchema.XC_Name, CustomFieldPropertyName));
				if (customField != null)
				{
					switch (customField.XC_Type)
					{
						case AddOnColumnDataType.Codes.String:
							return typeof(ZString);

						case AddOnColumnDataType.Codes.Datetime:
							return typeof(ZDateTime);

						case AddOnColumnDataType.Codes.Boolean:
							return typeof(ZBool);

						case AddOnColumnDataType.Codes.Integer:
						case AddOnColumnDataType.Codes.Decimal:
							return typeof(ZDecimal);

						case AddOnColumnDataType.Codes.ComboBox:
							return typeof(ComboBoxCustomFieldType);
					}
				}

				return null;
			}
		}

		public ZString CustomFieldPropertyName
		{
			get
			{
				var propertyName = Utilities.TrimExpression(PropertyName);
				if (propertyName.StartsWith(MacroHelper.GetCustomFieldMacroName, StringComparison.CurrentCultureIgnoreCase))
				{
					var customFieldNameLength = propertyName.Length - MacroHelper.GetCustomFieldMacroName.Length - 1; // Minus macro name and parenthesis
					return propertyName.Substring(MacroHelper.GetCustomFieldMacroName.Length, customFieldNameLength).Trim();
				}

				return ZString.Empty;
			}
		}
		protected override IEnumerable<ControlCustomisationBase> GetParentCustomisationRecords()
		{
			return Parent.CustomisationLines.Cast<ControlCustomisationBase>();
		}

		#endregion

		#region Lookups

		public new BMControlCustomisationLineLookups Lookups
		{
			get { return (BMControlCustomisationLineLookups)base.Lookups; }
		}

		protected override ControlCustomisationBaseLookups GetNewLookups()
		{
			return new BMControlCustomisationLineLookups(this);
		}

		protected internal override CodeDescriptionPairList ControlTypes
		{
			get { return Factory.GetCachedValue<PropertyTypeList>(); }
		}

		#endregion

		#region Validation

		public new BMControlCustomisationLineValidation Validation
		{
			get { return (BMControlCustomisationLineValidation)GetNewValidation(); }
		}

		protected override ControlCustomisationValidationBase GetNewValidation()
		{
			return new BMControlCustomisationLineValidation(this);
		}

		#endregion

		#region IRootTypeProvider members

		WorkflowDescriptor WorkflowDescriptor => WorkflowDescriptors.Instance.TryGetValueSafe(Parent.FM_JobType);

		Type[] IRootTypeProvider.RootTypes
		{
			get
			{
				var rootTypes = new List<Type>(1);

				if (WorkflowDescriptor?.WorkflowProviderType != null)
				{
					rootTypes.Add(WorkflowDescriptor.WorkflowProviderType);
				}
				return rootTypes.ToArray();
			}
		}

		BusinessObject[] IRootTypeProvider.Roots => Array.Empty<BusinessObject>();

		public bool IsCustomField => Utilities.TrimExpression(PropertyName).StartsWith(MacroHelper.GetCustomFieldMacroName, StringComparison.CurrentCultureIgnoreCase);
		public bool IsFirstOrFirstOrDefaultMacro => PropertyName.Contains(".First(", StringComparison.CurrentCultureIgnoreCase) ||
													PropertyName.Contains(".FirstOrDefault(", StringComparison.CurrentCultureIgnoreCase);

		#endregion

		internal override void SetDefaultsAfterAddedToCollection()
		{
			base.SetDefaultsAfterAddedToCollection();

			if (Parent.FM_ControlType == CustomisedControlTypeList.Codes.TaskCard)
			{
				IsReadOnly = true;
			}
		}

		public void RestoreDefaultLabel()
		{
			Label = IsCustomField ? CustomFieldPropertyName : PropertyNameDescription;
		}

		#region BindingPath

		public string GetBindingPath(ICardContent cardContent = null, bool showJobCards = false)
		{
			var bindable = cardContent?.Bindable;
			var propertyName = Utilities.TrimExpression(PropertyName);

			if (bindable == null || bindable is CardDto)
			{
				var replacementCharacter = "_";

				if (propertyName.Contains(".", StringComparison.CurrentCultureIgnoreCase))
				{
					propertyName = propertyName.Replace(".", replacementCharacter);
				}

				if (propertyName.Contains("+", StringComparison.CurrentCultureIgnoreCase))
				{
					propertyName = propertyName.Replace("+", replacementCharacter);
				}

				return string.Format(CultureInfo.InvariantCulture, "{0}_{1}", PropertySource, propertyName);
			}
			else if (bindable is ProcessTask task)
			{
				return GetBindingPath(task, showJobCards);
			}
			else
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Unrecognized type for binding [{0}]", bindable.GetType().FullName));
			}
		}

		string GetBindingPath(ProcessTask task, bool showJobCards)
		{
			var propertyName = Utilities.TrimExpression(PropertyName);

			switch (PropertySource)
			{
				case PropertySourceList.Codes.ProcessTask:
					return propertyName;
				case PropertySourceList.Codes.Workflow:
					if (showJobCards)
					{
						return string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}", nameof(task.ProcessHeader), nameof(task.ProcessHeader.ParentHeader), propertyName);
					}
					else
					{
						return string.Format(CultureInfo.InvariantCulture, "{0}.{1}", nameof(task.ProcessHeader), propertyName);
					}
				case PropertySourceList.Codes.Job:
					if (DoesTaskParentHaveProperty(task, propertyName))
					{
						return string.Format(CultureInfo.InvariantCulture, "{0}.{1}", nameof(task.Parent), propertyName);
					}
					else
					{
						return GetCustomPropertyBindingPath(task, CustomFieldPropertyName);
					}
			}

			return string.Empty;
		}

		static string GetCustomPropertyBindingPath(ProcessTask task, ZString propertyName)
		{
			if (!propertyName.IsEmpty
				&& task.Parent is ICustomFieldProvider c
				&& c.GetCustomBusinessObject() is IDynamicBusinessObject customBizo)
			{
				var customPropertyNames = customBizo.PropertyNames;
				var filterName = "__" + propertyName + "__";
				var customProperty = customPropertyNames.Where(p => p.Contains(filterName, StringComparison.OrdinalIgnoreCase) && !p.EndsWith((NoResString)"Info", StringComparison.OrdinalIgnoreCase)).Min(); // This is a custom property name

				if (customProperty != null)
				{
					return customProperty;
				}
			}

			return string.Empty;
		}

		bool DoesTaskParentHaveProperty(ProcessTask task, string propertyName)
		{
			var parentProperty = CustomisationLineReflectionHelper.GetPropertyInfo(task.GetType(), nameof(task.Parent));
			var parentType = TypeDecider.GetTypeForBinding(parentProperty.PropertyType);
			var hasProperty = GetProperty(parentType, propertyName) != null;

			if (!hasProperty && task.Parent != null)
			{
				parentType = task.Parent.GetType();
				var property = GetProperty(parentType, propertyName);
				if (property != null)
				{
					var taskTypeName = task.GetType().FullName;
					var key = (NoResString)"416a4126-f189-4046-826a-6e8b548a9bf9 - " + taskTypeName; // This is a GUID
					var message = string.Format(CultureInfo.InvariantCulture, (NoResString)"We were unable to bind to property [{0}] of task type [{1}] for parent type [{2}] even though the property is present. This could mean that the Parent property is being accessed through an interface rather than an actual business object.", // Developer exception
						propertyName,
						taskTypeName,
						parentType.FullName);

					ErrorReporter.ReportOnce(key, message);
				}
			}

			return hasProperty;
		}

		static PropertyInfo GetProperty(Type type, string propertyName)
		{
			if (!propertyName.Contains('.'))
			{
				var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => string.Equals(x.Name, propertyName, StringComparison.OrdinalIgnoreCase)).ToArray();

				return properties.Length > 1 ? GetMostOverriddenProperty(properties, type) : properties.SingleOrDefault();

				PropertyInfo GetMostOverriddenProperty(PropertyInfo[] propertyInfos, Type typeToCheck)
				{
					var result = propertyInfos.SingleOrDefault(x => x.DeclaringType == typeToCheck);

					return result ?? GetMostOverriddenProperty(propertyInfos, typeToCheck.BaseType);
				}
			}

			var indexOfDot = propertyName.IndexOf('.');
			var nextPropertyName = propertyName.Substring(0, indexOfDot);
			var nextProperty = GetProperty(type, nextPropertyName);
			var nextPropertyType = nextProperty?.PropertyType;

			if (nextPropertyType == null)
			{
				return null;
			}

			if (typeof(IBusinessObjectCollection).IsAssignableFrom(nextPropertyType))
			{
				nextPropertyType = BusinessObjectCollection.GetElementTypeFromCollectionType(nextPropertyType);
			}

			return GetProperty(nextPropertyType, propertyName.Substring(indexOfDot + 1));
		}

		#endregion
	}
}

// An empty type created for the sole purpose of dealing with combo boxes in custom fields.
internal struct ComboBoxCustomFieldType { }
