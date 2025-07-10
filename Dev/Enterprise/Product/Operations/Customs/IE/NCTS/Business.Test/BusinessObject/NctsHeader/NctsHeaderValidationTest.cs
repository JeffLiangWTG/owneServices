using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class NctsHeaderValidationTest : BusinessObjectLookupsTestCase
	{
		public void TestCheckBH_RL_NKImportLoadPort()
		{
			header.BH_RL_NKImportLoadPort = ZString.Empty;
			header.Validation.ValidateBH_RL_NKImportLoadPort();
			AssertNoNotifications(header.BH_RL_NKImportLoadPortInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = NctsHeaderTest.GetNewBusinessObject(Factory);
		}
		NctsHeader header;
	}
}
