using System;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class ImportDocumentProviderTest : Customs.Business.Testing.DataProviderTestCase<ImportDocumentProvider>
	{
		public void TestNew()
		{
			AssertExceptionThrown<ArgumentNullException>("Argument == null", () => new ImportDocumentProvider(null));
		}

		public void TestType()
		{
			document.CSI_Code = "N380";
			AssertEquals("N380", Provider.Type);
		}

		public void TestReferenceNumber()
		{
			document.CSI_ReferenceNumber = "reference";
			AssertEquals("reference", Provider.ReferenceNumber);
		}

		public void TestIssuingDate()
		{
			var expectedDate = ZDate.Today;
			document.CSI_DateOfIssue = expectedDate;
			AssertEquals(expectedDate, Provider.IssuingDate);
		}

		protected override void SetUp()
		{
			base.SetUp();

			document = Factory.New<SupportingDocument>();
			dataProvider = new ImportDocumentProvider(document);
		}

		IImportDocument dataProvider;
		SupportingDocument document;

		protected override ImportDocumentProvider GetProvider() => (ImportDocumentProvider)dataProvider;
	}
}
