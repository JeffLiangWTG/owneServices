using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class ReportingBookCollectionProvider : CollectionProvider
	{
		public ReportingBookCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new AccReportingBookCollection(BusinessObjectFactory);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.AccReportingBook;
	}

	internal class ReportingBookWithLocalCurrencyCollectionProvider : ReportingBookCollectionProvider
	{
		public ReportingBookWithLocalCurrencyCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		public override void AddValidationAndDefault(FilterField parentFilterField, ValidatorPack validatorPack)
		{
			base.AddValidationAndDefault(parentFilterField, validatorPack);
			parentFilterField.Validators.Add(new ReportingBookWithLocalCurrencyLookupTypeValidator());
		}
	}

	class ReportingBookWithLocalCurrencyLookupTypeValidator : FilterCollectionValidator
	{
		string errorMsgWithChart;

		public ReportingBookWithLocalCurrencyLookupTypeValidator()
		{
		}

		public override bool IsValid(FilterField filterToValidate)
		{
			var filter = filterToValidate as LookupField;
			var reportingBook = filter.Factory.Load<AccReportingBook>(filter.Value);

			if (string.IsNullOrEmpty(reportingBook?.ARB_RX_NKCurrency) || GlbCompany.CurrentCompany.LocalCurrency.RX_Code == reportingBook.ARB_RX_NKCurrency)
			{
				return true;
			}
			errorMsgWithChart = $"This report can only be generated for Reporting Books in Local Reporting Currency only.";
			return false;
		}

		public override string GetErrorMessage(FilterField filterToValidate)
		{
			return errorMsgWithChart;
		}
	}
}
