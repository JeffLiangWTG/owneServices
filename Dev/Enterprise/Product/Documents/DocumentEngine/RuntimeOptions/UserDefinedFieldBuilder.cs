using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	abstract class UserDefinedFieldBuilder
	{
		protected const string Type = FieldBuilderPropertyCodeDescriptionList.Codes.Type;
		protected const string Tab = FieldBuilderPropertyCodeDescriptionList.Codes.Tab;
		protected const string Default = FieldBuilderPropertyCodeDescriptionList.Codes.Default;
		protected const string Left = FieldBuilderPropertyCodeDescriptionList.Codes.Left;
		protected const string Top = FieldBuilderPropertyCodeDescriptionList.Codes.Top;
		protected const string Width = FieldBuilderPropertyCodeDescriptionList.Codes.Width;
		protected const string Height = FieldBuilderPropertyCodeDescriptionList.Codes.Height;
		protected const string DataContext = FieldBuilderPropertyCodeDescriptionList.Codes.DataContext;

		protected ValidatorPack validators;
		protected BusinessObjectFactory businessObjectFactory;
		protected MatchEvaluator evaluatorForDefaultValues;

		protected UserDefinedFieldBuilder(ValidatorPack validators, BusinessObjectFactory factory, MatchEvaluator evaluatorForDefaultValues)
		{
			ExpectedProperties.Add(Type);
			ExpectedProperties.Add(Tab);
			ExpectedProperties.Add(Default);
			ExpectedProperties.Add(Left);
			ExpectedProperties.Add(Top);
			ExpectedProperties.Add(Width);
			ExpectedProperties.Add(Height);
			ExpectedProperties.Add(DataContext);

			businessObjectFactory = factory;
			this.validators = validators;
			this.evaluatorForDefaultValues = evaluatorForDefaultValues;
		}

		public bool CanBuild(string filterType)
		{
			return Regex.IsMatch(filterType);
		}

		protected abstract Regex Regex { get; }

		protected BusinessObjectFactory Factory
		{
			get { return businessObjectFactory; }
		}

		/// <summary>
		/// Determines whether a property is valid for this type of field.
		/// </summary>
		/// <param name="propertyName">The name of the property, as it appears in the template</param>
		protected bool IsValidProperty(string propertyName)
		{
			return ExpectedProperties.Any((expectedProperty) => { return expectedProperty.Equals(propertyName, StringComparison.OrdinalIgnoreCase); });
		}

		internal List<string> ExpectedProperties = new List<string>();

		/// <summary>
		/// Creates a new FilterField of the concrete type appropriate for this UserDefinedFieldBuilder.
		/// </summary>
		/// <returns>A new FilterField</returns>
		public FilterField NewField(StringTreeNode fieldDef, bool isInRuntime = true)
		{
			return GetFilterField(fieldDef, isInRuntime);
		}

		protected abstract FilterField GetFilterField(StringTreeNode fieldDef, bool isInRuntime);

		public virtual FilterField Build(StringTreeNode fieldDef, string dataContext)
		{
			foreach (var child in fieldDef.Children)
			{
				if (!IsValidProperty(child.Value))
				{
					throw new TemplateDefinitionException(String.Format(CultureInfo.InvariantCulture, @"In the ""{0}"" field, the option ""{1}"" is not allowed in this type of field", fieldDef.Value, child.Value), child.CellReference);
				}
			}

			var newField = NewField(fieldDef);
			newField.DisplayName = fieldDef.Value;

			if (fieldDef.ChildExists(Tab))
			{
				foreach (StringTreeNode tabName in fieldDef.FindChild(Tab).Children)
				{
					newField.TabNames.Add(tabName.Value);
				}
			}
			if (fieldDef.ChildExists(Default))
			{
				newField.DefaultExpression = fieldDef.FindChild(Default).Child().Value;
			}
			else
			{
				newField.DefaultExpression = "";
			}

			if (fieldDef.ChildExists(Left))
			{
				ControlDpiScalingHelper.SetLeft(ref newField, ZArchitecture.Core.Utilities.ConvertToInt32(fieldDef.FindChild(Left).Child().Value, newField.Unspecified), true);
			}

			if (fieldDef.ChildExists(Top))
			{
				ControlDpiScalingHelper.SetTop(ref newField, ZArchitecture.Core.Utilities.ConvertToInt32(fieldDef.FindChild(Top).Child().Value, newField.Unspecified), true);
			}

			if (fieldDef.ChildExists(Width))
			{
				ControlDpiScalingHelper.SetWidth(ref newField, ZArchitecture.Core.Utilities.ConvertToInt32(fieldDef.FindChild(Width).Child().Value, newField.Unspecified), true);
			}

			if (fieldDef.ChildExists(Height))
			{
				ControlDpiScalingHelper.SetHeight(ref newField, ZArchitecture.Core.Utilities.ConvertToInt32(fieldDef.FindChild(Height).Child().Value, newField.Unspecified), true);
			}

			if (fieldDef.ChildExists(DataContext))
			{
				newField.DataContextValue = new DataContextValue(fieldDef.FindChild(DataContext).Child().Value);
			}
			else if (!string.IsNullOrEmpty(dataContext))
			{
				newField.DataContextValue = new DataContextValue(dataContext);
			}
			else
			{
				newField.DataContextValue = DataContextValue.None;
			}

			return newField;
		}

		#region Documentation

		IReportDocumenter documentation;
		public IReportDocumenter Documentation
		{
			get
			{
				if (documentation == null)
				{
					documentation = GetDocumentation(ExpectedProperties);
					documentation.ValueProviderDocumenters = NewField(new StringTreeNode(), false).ValueProviderDocumenters;
				}
				return documentation;
			}
		}
		protected abstract IReportDocumenter GetDocumentation(List<string> supportedProperties);

		#endregion

#if DEBUG
		internal BusinessObjectFactory GetFactoryForTest()
		{
			return this.Factory;
		}
#endif
	}
}
