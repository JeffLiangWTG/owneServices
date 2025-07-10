using System;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Module.Testing
{
	class EntryStatusListProviderTest : Customs.Module.Testing.EntryStatusListProviderTest
	{
		public override void TestEntryStatusLists()
		{
			var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.SubmissionType = Customs.Business.DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, customsInterface))
			{
				RunStatusCodeListForVariousCountriesTester(Core.Constants.CountryCodes.Lithuania, new Common.EU.EntryStatusList().GetAllCodes(), new string[] { "WTO", "ROK", "CEO" });
			}
		}
	}
}
