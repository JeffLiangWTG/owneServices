using CargoWise.Types;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	public class ReceptacleValidationTest : Customs.Business.Testing.CusCodeDataValidationTest
	{
		public new void TestCheckCY_Code()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var receptacle = header.Receptacles.AddNew();
			receptacle.CY_Code = ZString.Empty;

			AssertNoNotifications(receptacle.CY_CodeInfo);
		}
	}
}
