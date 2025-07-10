using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(SupportingDocumentCollection))]
	public class SupportingDocumentCollectionTest : EU.Business.Declaration.MultiLineAddInfos.Testing.SupportingDocumentCollectionTest
	{
		protected override CusSupportingInfoCollection<EU.Business.Declaration.MultiLineAddInfos.SupportingDocument> GetCusSupportingInfoCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			return new SupportingDocumentCollection(declaration);
		}

		public override void TestAddNewInvoiceDocumentAndCopyDataFromInvoice()
		{
			SupportingDocumentTestHelper.CreateRefCusCodeWithAttributeToEnsurePropertyEditable(
				Factory,
				UniversalReferenceConstants.RefCusCodeListAttributes.Name.Reference,
				EU.Business.UniversalReferenceConstants.SupportingDocumentTypes.N380,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection);
			base.TestAddNewInvoiceDocumentAndCopyDataFromInvoice();
		}

		public void TestDESupportingDocumentCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			for (int i = 0; i < 20; i++)
			{
				invoiceHeader.SupportingDocuments.AddNew();
			}
			var supportingDocument21ForIncoiceHeader = invoiceHeader.SupportingDocuments.AddNew();
			AssertHasRowError(supportingDocument21ForIncoiceHeader, "The maximum number (20) of allowed Supporting Documents per Invoice Header has been exceeded.");

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var supportingDocument1ForIncoiceLine = invoiceLine.SupportingDocuments.AddNew();
			AssertEquals("Default Valus", AvailabilityList.Codes.J, supportingDocument1ForIncoiceLine.CSI_Status);
			for (int i = 1; i < 99; i++)
			{
				invoiceLine.SupportingDocuments.AddNew();
			}

			var supportingDocument100ForIncoiceLine = invoiceLine.SupportingDocuments.AddNew();
			AssertHasRowError(supportingDocument100ForIncoiceLine, "You are only allowed a maximum of 99 Supporting Documents here.");
		}
	}
}
