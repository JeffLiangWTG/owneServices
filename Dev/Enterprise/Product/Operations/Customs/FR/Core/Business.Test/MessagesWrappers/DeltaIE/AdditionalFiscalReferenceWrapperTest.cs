using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class AdditionalFiscalReferenceWrapperTest : Customs.Business.Testing.DataProviderTestCase<AdditionalFiscalReferenceWrapper>
	{
		protected override AdditionalFiscalReferenceWrapper GetProvider()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			var cusReference = entryInstruction.FiscalReferences.AddNew();
			cusReference.CFR_Reference = "ref";
			cusReference.CFR_Code = "A";
			return AdditionalFiscalReferenceWrapper.New(cusReference);
		}

		public void TestVATIdentificationNumber()
		{
			AssertEquals("VATIdentificationNumber should be equal to CFR_Reference.", "ref", Provider.VATIdentificationNumber);
		}

		public void TestRole()
		{
			AssertEquals("Role should be equal to CFR_Code.", "A", Provider.Role);
		}
	}
}
