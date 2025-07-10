using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class ProcessRelatedNumber : CusEntryNumber
	{
		public ProcessRelatedNumber(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore => Res.GetString("b8e73279-5a46-4428-b35b-88f140488d1a", "Process Related Number");

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CE_EntryIsSystemGenerated = false;
			CE_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;
			CE_Category = CusEntryNumber.Categories.DocumentsRelated;
		}

		public new ProcessRelatedNumberLookups Lookups => (ProcessRelatedNumberLookups)base.Lookups;

		protected override CusEntryNumLookups GetNewLookups() => new ProcessRelatedNumberLookups(this);

		public new ProcessRelatedNumberValidation Validation => (ProcessRelatedNumberValidation)base.Validation;

		protected override CusEntryNumValidation GetNewValidation() => new ProcessRelatedNumberValidation(this);
	}
}
