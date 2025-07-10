using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	class SummaryDeclarationGoodsItemProviderTest : Customs.Business.Testing.DataProviderTestCase<SummaryDeclarationGoodsItemProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertNull("NULL", SummaryDeclarationGoodsItemProvider.NewOrNull(null));
				AssertNotNull("Not NULL", SummaryDeclarationGoodsItemProvider.NewOrNull(previousDocument));
			});
		}

		public void TestQuantity()
		{
			previousDocument.CSI_Quantity = 12345m;
			AssertEquals(12345, Provider.Quantity);
		}

		public void TestIdentificationByKeyKind()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty", ZString.Empty, Provider.IdentificationByKeyKind);

				previousDocument.CSI_SubType = PreviousDocSubTypeList.Codes.AWB;
				AssertEquals("Not empty", "AWB", Provider.IdentificationByKeyKind);
			});
		}

		public void TestIdentificationByKeyNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty", ZString.Empty, Provider.IdentificationByKeyNumber);

				previousDocument.CSI_ReferenceNumber = "REF12345";
				AssertEquals("Not empty", "REF12345", Provider.IdentificationByKeyNumber);
			});
		}

		public void TestIdentificationByKeyCustodianIdentifier()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty", ZString.Empty, Provider.IdentificationByKeyCustodianIdentifier);

				previousDocument.CSI_ReferenceNumber2 = "DE123456789";
				AssertEquals("Not empty", "DE123456789", Provider.IdentificationByKeyCustodianIdentifier);
			});
		}

		public void TestIdentificationByRegistrationReferencedRegistrationNumber_SubTypeREG()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty", ZString.Empty, Provider.IdentificationByRegistrationReferencedRegistrationNumber);

				previousDocument.CSI_ReferenceNumber = "REF12345";
				AssertEquals("REF12345", Provider.IdentificationByRegistrationReferencedRegistrationNumber);
			});
		}

		public void TestIdentificationByRegistrationReferencedSequenceNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Default", ZInt.Zero, Provider.IdentificationByRegistrationReferencedSequenceNumber);

				previousDocument.CSI_LineNo = 2;
				AssertEquals("2", 2, Provider.IdentificationByRegistrationReferencedSequenceNumber);
			});
		}

		protected override SummaryDeclarationGoodsItemProvider GetProvider() => SummaryDeclarationGoodsItemProvider.NewOrNull(previousDocument);

		protected override void SetUp()
		{
			base.SetUp();
			previousDocument = Factory.New<PreviousDocument>();
		}
		PreviousDocument previousDocument;
	}
}
