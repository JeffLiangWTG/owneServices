using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class PreviousDocumentWrapperTest : Customs.Business.Testing.DataProviderTestCase<PreviousDocumentWrapper>
	{
		public void TestPreviousDocumentType()
		{
			previousDocument.CSI_Code = "Code";
			AssertEquals("Code", wrapper.PreviousDocumentType);
		}

		public void TestPreviousDocumentReference()
		{
			previousDocument.CSI_ReferenceNumber = "123456";
			AssertEquals("123456", wrapper.PreviousDocumentReference);
		}

		public void TestPreviousDocumentReference_WithDateOfIssue()
		{
			previousDocument.CSI_ReferenceNumber = "123456";
			previousDocument.CSI_DateOfIssue = new ZDateTime(2019, 01, 01);
			AssertEquals("123456-01/01/2019", wrapper.PreviousDocumentReference);
		}

		public void TestPreviousDocumentReferenceLanguage()
		{
			AssertEquals(ZString.Empty, wrapper.PreviousDocumentReferenceLanguage);
		}

		public void TestComplementOfInformation()
		{
			previousDocument.CSI_Description = "descriptionTEST";
			AssertEquals("descriptionTEST", wrapper.ComplementOfInformation);
		}

		public void TestComplementOfInformationLanguage()
		{
			AssertEquals(ZString.Empty, wrapper.ComplementOfInformation);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var departureCargoDesc = header.Bills.AddNew().GoodsItems.AddNew();
			previousDocument = departureCargoDesc.PreviousDocuments.AddNew();
			wrapper = new PreviousDocumentWrapper(previousDocument);
		}
		NctsPreviousDocument previousDocument;
		PreviousDocumentWrapper wrapper;
		protected override PreviousDocumentWrapper GetProvider() => wrapper;
	}
}
