using CargoWise.EntityFramework;
using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.Customs.EU.TemporaryStorage.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Module.TemporaryStorage.Testing
{
	[TestedType(typeof(TempStoragePremisesController))]
	sealed class TempStoragePremisesControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.EU.TempStoragePremises;

		protected override string CountryCode => Core.Constants.CountryCodes.Latvia;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var premises = Factory.New<CusTempStorageRegPremises>();
			premises.SRP_Code = "X";
			premises.SRP_Description = "DESC";

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "AAA";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address";
			premises.SRP_OA_PremisesAddress = orgAddress.PK;

			Factory.Save();
			return premises;
		}
	}
}
