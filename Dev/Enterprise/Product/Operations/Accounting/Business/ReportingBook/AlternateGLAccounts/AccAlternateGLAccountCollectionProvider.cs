using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business
{
	internal class AccAlternateGLAccountCollectionProvider : CollectionProvider, Integration.IAccAlternateGLAccountCollectionProvider
	{
		public AccAlternateGLAccountCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.AlternateGLAccounts;

		protected override IBusinessObjectCollection CreateCollection()
		{
			return AlternateGLAccountCollection;
		}

		public override void AddValidationAndDefault(FilterField parentFilterField, ValidatorPack validatorPack)
		{
			base.AddValidationAndDefault(parentFilterField, validatorPack);
			parentFilterField.Validators.Add(new AccAlternateGLAccountLookupTypeValidator(GetFilterDescription()));
		}

		public override string GetFilterDescription()
		{
			return Res.GetString("B334992A-5E8D-4057-BB22-BDEA7E5B39DA", "Please choose the Alternate GL Account depending on the Alternate chart of Reporting book");
		}

		AlternateGLAccountCombineParentAccountCollection AlternateGLAccountCollection => alternateGLAccountCollection ??= new AlternateGLAccountCombineParentAccountCollection(BusinessObjectFactory);
		AlternateGLAccountCombineParentAccountCollection alternateGLAccountCollection;
	}

	class AccAlternateGLAccountLookupTypeValidator : FilterCollectionValidator
	{
		readonly string filterDescription;
		string errorMsgWithChart;

		public AccAlternateGLAccountLookupTypeValidator(string filterDescription)
		{
			this.filterDescription = filterDescription;
			this.errorMsgWithChart = filterDescription;
		}

		public override bool IsValid(FilterField filterToValidate)
		{
			var filter = filterToValidate as LookupField;
			var account = filter.Factory.Load<AccAlternateGLAccount>(filter.Value);
			var reportingBook = filter.Factory.Load<AccReportingBook>(ZGuid.ParseSafe(filter.DependencyValue));
			if (reportingBook == null || account == null || (account != null && reportingBook != null && account.AGA_AAC_AlternateChart == reportingBook.ARB_AAC_AlternateChart))
			{
				return true;
			}

			errorMsgWithChart = $"{filterDescription}: {reportingBook.AlternateChart.AAC_Code}-{reportingBook.AlternateChart.AAC_Description}";
			return false;
		}

		public override string GetErrorMessage(FilterField filterToValidate)
		{
			return errorMsgWithChart;
		}
	}
}
