using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class CustomsOfficeOfGuaranteeWrapperTest : DataProviderTestCase<CustomsOfficeOfGuaranteeWrapper>
	{
		public void TestReferenceNumber()
		{
			AssertEquals("ReferenceNumber should equal guarantee PW_BondFiledPort", "FR000001", Provider.ReferenceNumber);
		}

		protected override CustomsOfficeOfGuaranteeWrapper GetProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var guarantee = entryInstruction.Guarantees.AddNew();
			guarantee.PW_BondFiledPort = "FR000001";
			return CustomsOfficeOfGuaranteeWrapper.New(guarantee);
		}
	}
}
