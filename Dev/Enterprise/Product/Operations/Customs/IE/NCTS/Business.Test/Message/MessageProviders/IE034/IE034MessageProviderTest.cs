using System.Linq;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class IE034MessageProviderTest : Customs.Business.Testing.DataProviderTestCase<IE034MessageProvider>
	{
		public void TestRequesterIdentificationNumber()
		{
			SetPrincipal("0123456789000");
			AssertEquals("Id should be set when code type = EOR", "IE0123456789000", Provider.RequesterIdentificationNumber);
		}

		public void TestGuaranteeReferences()
		{
			var guarantee1 = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee1.PW_BondNumber = "111";
			var guarantee2 = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee2.PW_BondNumber = "222";
			AssertEquals("AllGuarantees", 2, sendingAction.AllGuarantees.Count);
			sendingAction.AllGuarantees[0].ShouldSend = false;
			sendingAction.AllGuarantees[1].ShouldSend = true;
			AssertEquals("GuaranteeReferences", 1, Provider.GuaranteeReferences.Count);
			AssertEquals("Grn", "222", Provider.GuaranteeReferences.ToArray()[0].Grn);
		}

		protected override IE034MessageProvider GetProvider() => new IE034MessageProvider(sendingAction);

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			sendingAction = new QueryOnGuaranteeSendingAction(nctsHeader);
		}
		QueryOnGuaranteeSendingAction sendingAction;
		NctsHeader nctsHeader;

		void SetPrincipal(string id)
		{
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "PC1", nctsHeader.Principal, string.Empty, "Test Company Limited", "123 Test Street", "A12B3C4", "City", "IEXX", "IE", id, "TIR123");
		}
	}
}
