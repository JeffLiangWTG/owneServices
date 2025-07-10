using System;
using System.Diagnostics;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Module
{
	public delegate ZQuery GetCustomsOfficeQueryDelegate(SQLComparisonOperator opperator, ZString purpose, ZString office);

	public class CustomsOfficeFilter : ModuleTextBaseFilter
	{
		public CustomsOfficeFilter(ZString description, GetCustomsOfficeQueryDelegate queryDelegate, CodeDescriptionPairList purposeListFromModule)
			: base(description, queryDelegate)
		{
			this.purposeListFromModule = purposeListFromModule;
		}
		readonly CodeDescriptionPairList purposeListFromModule;

		public CustomsOfficeFilter(FilterCategory category, ModuleFilterCollection parentCollection) : base(category, parentCollection)
		{
		}

		[List(nameof(PurposeList))]
		[MaxLength(3)]
		public virtual ZString Purpose
		{
			[DebuggerStepThrough]
			get { return purpose; }
			set
			{
				CheckMaximumLength(PurposeInfo, value);
				SetNonPersistentPropertyValue(PurposeInfo, ref purpose, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidatePurpose();
				}
			}
		}

		ZString purpose;

		public ZPropertyInfo PurposeInfo => GetZPropertyInfo(nameof(Purpose));

		public CodeDescriptionPairList PurposeList => purposeListFromModule;

		public new CustomsOfficeFilterValidation Validation => (CustomsOfficeFilterValidation)base.Validation;

		protected override ModuleFilterValidation GetNewValidation() => new CustomsOfficeFilterValidation(this);

		#region Implementation

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			base.CopyPersistantValuesFromFilter(filterToCopyFrom);
			CustomsOfficeFilter source = ((CustomsOfficeFilter)(filterToCopyFrom));
			source.purpose = purpose;
		}

		protected override void ClearCore()
		{
			base.ClearCore();
			Purpose = ZString.Empty;
		}

		protected override FilterCategory DefaultCategory => FilterCategories.Locations;

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection) => new CustomsOfficeFilter(category, parentCollection);

		protected override ZQuery GetQueryUsingFilterColumns() => throw new NotSupportedException();

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);
			writer.WriteElementString("Purpose", Purpose);
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);
			Purpose = reader.ReadElementString("Purpose");
		}

		protected override bool IsEmptyCore => base.IsEmptyCore && Purpose.IsEmpty;

		protected override object[] QueryDelegateParameters => new object[] { SqlComparisonOperator, Purpose, Property };

		#endregion
	}
}
