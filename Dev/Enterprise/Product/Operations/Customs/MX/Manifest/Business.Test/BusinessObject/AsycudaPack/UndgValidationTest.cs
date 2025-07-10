using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	class UndgValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateUNDGSubstanceManager()
		{
			var bill = Factory.New<AsycudaBill>();
			var pack = bill.Packs.AddNew();
			var undg = pack.UNDGs.AddNew();
			undg.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			undg.UNDGSubstance.DG_FlashPoint = ZString.Empty;
			AssertHasMessageErrorContaining(pack.UNDGs.FirstItemForBinding[0].DI_DG_NKSubsInfo, "should have a flash point entered.");

			undg.UNDGSubstance.DG_FlashPoint = "TEST";
			AssertNoNotifications(pack.UNDGs.FirstItemForBinding[0].DI_DGInfo);
		}
	}
}
