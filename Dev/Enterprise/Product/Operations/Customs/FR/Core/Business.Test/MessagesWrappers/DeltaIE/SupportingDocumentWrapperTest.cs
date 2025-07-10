using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class SupportingDocumentWrapperTest : Customs.Business.Testing.DataProviderTestCase<SupportingDocumentWrapper>
	{
		protected override SupportingDocumentWrapper GetProvider()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var doc1 = invoiceLine.SupportingDocuments.AddNew();
			doc1.CSI_ReferenceNumber = "ref1";
			doc1.CSI_ReferenceNumber2 = "ref2";
			doc1.CSI_ItemNumber = 1;
			doc1.CSI_DateOfExpiry = new ZDate(2025, 05, 01);
			doc1.CSI_LineNo = 2;
			doc1.CSI_Code = "Code";
			doc1.CSI_Value = 12m;
			doc1.CSI_Quantity = 23m;
			doc1.CSI_RX_NKCurrency = "EUR";
			doc1.CSI_Description = "Description";
			doc1.CSI_UnitOfQuantity = "KGM";
			doc1.CSI_AdditionalDescription = "AdditionalDescription";

			return SupportingDocumentWrapper.New(doc1, string.Empty);
		}

		internal static void AssertSupportingDocumentWrapper(ISupportingDocument supportingDocWrapper, string dateOfValidity = "", string documentLineItemNumber = "0", string issuingAuthorityName = "", string ccQualifier = "FR", string referenceNumber = "", string type = "", double amount = 0, string currency = "", double quantity = 0)
		{
			AssertEquals("SupportingDocumentWrapper: Date Of Validity", dateOfValidity, supportingDocWrapper.DateOfValidity);
			AssertEquals("SupportingDocumentWrapper: Document Line Item Number", documentLineItemNumber, supportingDocWrapper.DocumentLineItemNumber);
			AssertEquals("SupportingDocumentWrapper: Issuing Authority Name", issuingAuthorityName, supportingDocWrapper.IssuingAuthorityName);
			AssertEquals("SupportingDocumentWrapper: CcQualifier", ccQualifier, supportingDocWrapper.CcQualifier);
			AssertEquals("SupportingDocumentWrapper: Reference Number", referenceNumber, supportingDocWrapper.ReferenceNumber);
			AssertEquals("SupportingDocumentWrapper: Type", type, supportingDocWrapper.Type);
			AssertEquals("SupportingDocumentWrapper: Amount", amount, supportingDocWrapper.Amount);
			AssertEquals("SupportingDocumentWrapper: Currency", currency, supportingDocWrapper.Currency);
			AssertEquals("SupportingDocumentWrapper: Date Of Validity", quantity, supportingDocWrapper.Quantity);			
		}

		public void TestDateOfValidity()
		{
			AssertEquals("DateOfValidity should be equal to document CSI_DateOfExpiry.", "2025-05-01", Provider.DateOfValidity);
		}

		public void TestDocumentLineItemNumber()
		{
			AssertEquals("DocumentLineItemNumber should be equal to document CSI_LineNo.", "2", Provider.DocumentLineItemNumber);
		}

		public void TestIssuingAuthorityName()
		{
			AssertEquals("IssuingAuthorityName should be equal to document CSI_AdditionalDescription.", "AdditionalDescription", Provider.IssuingAuthorityName);
		}

		public void TestCcQualifier()
		{
			AssertEquals("CcQualifier should be equal to procedure FR as customs office is empty.", Core.Constants.CountryCodes.France, Provider.CcQualifier);

			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var doc1 = invoiceLine.SupportingDocuments.AddNew();
			doc1.CSI_ReferenceNumber = "ref1";
			doc1.CSI_ReferenceNumber2 = "ref2";
			doc1.CSI_ItemNumber = 1;
			doc1.CSI_DateOfExpiry = ZDateTime.Today;
			doc1.CSI_LineNo = 2;
			doc1.CSI_Code = "Code";
			doc1.CSI_Value = 12m;
			doc1.CSI_Quantity = 23m;
			doc1.CSI_RX_NKCurrency = "EUR";

			var wrapper = SupportingDocumentWrapper.New(doc1, Core.Constants.CountryCodes.France);
			AssertEquals("SupportingDocumentWrapper: CcQualifier should be empty as customsOffice starts with FR", string.Empty, wrapper.CcQualifier);

			wrapper = SupportingDocumentWrapper.New(doc1, Core.Constants.CountryCodes.Ukraine);
			AssertEquals("SupportingDocumentWrapper: CcQualifier should be equal to FR as customsOffice doesn't starts with FR", Core.Constants.CountryCodes.France, wrapper.CcQualifier);
		}

		public void TestReferenceNumber()
		{
			AssertEquals("ReferenceNumber should be equal to document CSI_ReferenceNumber.", "ref1", Provider.ReferenceNumber);
		}

		public void TestType()
		{
			AssertEquals("Type should be equal to document CSI_Code.", "Code", Provider.Type);
		}

		public void TestAmount()
		{
			AssertEquals("Amount should be equal to document CSI_Value.", 12d, Provider.Amount);
		}

		public void TestCurrency()
		{
			AssertEquals("Currency should be equal to document CSI_RX_NKCurrency.", "EUR", Provider.Currency);
		}

		public void TestQuantity()
		{
			AssertEquals("Quantity should be equal to document CSI_Quantity.", 23d, Provider.Quantity);
		}

		public void TestMeasurementUnitAndQualifier()
		{
			AssertEquals("MeasurementUnitAndQualifier should be equal to document CSI_UnitOfQuantity.", "KGM", Provider.MeasurementUnitAndQualifier);
		}

		public void TestComplementOfInformation()
		{
			AssertEquals("ComplementOfInformation should be equal to document CSI_Description.", "Description", Provider.ComplementOfInformation);
		}
	}
}
