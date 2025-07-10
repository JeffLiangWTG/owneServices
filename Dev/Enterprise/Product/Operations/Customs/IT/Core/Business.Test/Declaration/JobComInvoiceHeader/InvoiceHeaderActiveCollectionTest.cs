using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(InvoiceHeaderActiveCollection))]
sealed class InvoiceHeaderActiveCollectionTest : EU.Business.Declaration.Testing.InvoiceHeaderCollectionTest<InvoiceHeaderActiveCollection, JobComInvoiceHeader>
{
	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var dec = Factory.New<JobDeclaration>();
		var invoice = dec.Invoices.AddNew();
		return invoice;
	}

	public void TestSetDefaultsForNewElement_ShouldBringAEOCertificatesToSupportingDocuments()
	{
		var declaration = Factory.New<JobDeclaration>();

		var supplier = Factory.New<OrgHeader>();
		supplier.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "AEO022");
		declaration.JE_OH_Supplier = supplier.PK;

		var declarant = Factory.New<OrgHeader>();
		declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "AEO024");
		declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
		declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;

		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var invoice = declaration.Invoices.AddNew();
		AssertEquals(2, invoice.SupportingDocuments.Count);
		AssertEquals(UniversalReferenceConstants.SupportingDocumentTypes.Y022, invoice.SupportingDocuments[0].CSI_Code);
		AssertEquals("AEO022", invoice.SupportingDocuments[0].CSI_ReferenceNumber);
		AssertEquals(UniversalReferenceConstants.SupportingDocumentTypes.Y024, invoice.SupportingDocuments[1].CSI_Code);
		AssertEquals("AEO024", invoice.SupportingDocuments[1].CSI_ReferenceNumber);
	}

	public void TestGetDefaultSetterForInvoiceHeader()
	{
		var declaration = Factory.New<JobDeclaration>();
		var newJobComInvoiceHeader = Factory.New<JobComInvoiceHeader>();

		var invoiceHeaderActiveCollectionExtended = new InvoiceHeaderActiveCollectionExtendedTest(declaration);
		var defaultSetterForInvoiceHeader = invoiceHeaderActiveCollectionExtended.GetDefaultSetterForInvoiceHeaderExposed(newJobComInvoiceHeader, declaration);

		AssertNotNull(defaultSetterForInvoiceHeader);
		AssertType<DefaultSetterForInvoiceHeader>("Type", defaultSetterForInvoiceHeader);
	}

	class InvoiceHeaderActiveCollectionExtendedTest : InvoiceHeaderActiveCollection
	{
		public InvoiceHeaderActiveCollectionExtendedTest(JobDeclaration declaration) : base(declaration)
		{
		}

		public Customs.Business.DefaultSetterForInvoiceHeader GetDefaultSetterForInvoiceHeaderExposed(Customs.Business.BaseJobComInvoiceHeader newElement, Customs.Business.BaseJobDeclaration declaration)
		{
			return base.GetDefaultSetterForInvoiceHeader(newElement, declaration);
		}
	}
}
