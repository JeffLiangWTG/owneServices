using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class IE034GuaranteeReferenceProviderTest : Customs.Business.Testing.DataProviderTestCase<IE034GuaranteeReferenceProvider>
	{
		public void TestGrn()
		{
			guarantee.PW_BondNumber = "111";
			AssertEquals("Grn", "111", Provider.Grn);
		}

		public void TestGuaranteeQuery()
		{
			sendingAction.QueryIdentifier = "222";
			AssertEquals("QueryIdentifier", "222", Provider.GuaranteeQuery.QueryIdentifier);
		}

		public void TestOwnerIdentificationNumber()
		{
			AssertNull("OwnerIdentificationNumber", Provider.OwnerIdentificationNumber);
		}

		public void TestAccessCode()
		{
			guarantee.PW_Password = "pass";
			AssertEquals("AccessCode", "pass", Provider.AccessCode);
		}

		protected override IE034GuaranteeReferenceProvider GetProvider() => new IE034GuaranteeReferenceProvider(sendingAction, sendingObject);

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			sendingAction = new QueryOnGuaranteeSendingAction(nctsHeader);
			guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			sendingObject = new QueryOnGuaranteeSendingObject(guarantee);
		}
		NctsGuarantee guarantee;
		QueryOnGuaranteeSendingObject sendingObject;
		QueryOnGuaranteeSendingAction sendingAction;
		NctsHeader nctsHeader;
	}
}
