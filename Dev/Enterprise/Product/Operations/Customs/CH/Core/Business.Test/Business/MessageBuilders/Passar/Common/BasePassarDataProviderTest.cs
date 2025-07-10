using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.CustomsLists;

namespace Enterprise.Customs.CH.Business.Testing;

public abstract class BasePassarDataProviderTest<TDataProvider> : TestCaseWithFactory
														where TDataProvider : class
{
	protected TDataProvider DataProvider => dataProvider ??= CreateDataProvider();
	TDataProvider dataProvider;

	protected abstract TDataProvider CreateDataProvider();

	protected void ResetDataProvider() => dataProvider = null;

	protected void AddDocument<T>(CusSupportingInfoCollection<T> documents, string type, string reference) where T : CusSupportingInfo
	{
		var document = documents.AddNew();
		document.CSI_Code = type;
		document.CSI_ReferenceNumber = reference;
	}

	protected JobDeclaration Declaration => declaration ??= CreateDeclaration();
	JobDeclaration declaration;

	JobDeclaration CreateDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.CustomsEntryInstructions.AddNew();
		declaration.JE_TransportMode = TransportTypeGenericList.Codes.Road;
		CreateInvoiceHeader(declaration);
		declaration.DoMergeForTesting();
		return declaration;
	}

	protected JobComInvoiceHeader InvoiceHeader => Declaration.Invoices.Cast<JobComInvoiceHeader>().First();

	protected JobComInvoiceHeader CreateInvoiceHeader() => CreateInvoiceHeader(Declaration);

	JobComInvoiceHeader CreateInvoiceHeader(JobDeclaration declaration)
	{
		var invoice = declaration.Invoices.AddNew();
		CreateInvoiceLine(invoice);
		return invoice;
	}

	protected JobComInvoiceLine InvoiceLine => InvoiceHeader.InvoiceLines.Cast<JobComInvoiceLine>().First();

	protected JobComInvoiceLine CreateInvoiceLine(JobComInvoiceHeader invoiceHeader)
	{
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = invoiceHeader.JobDeclaration.CustomsEntryInstructions[0].PK;
		return invoiceLine;
	}

	protected CusEntryInstruction EntryInstruction => Declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().First();

	protected CusEntryHeader EntryHeader => Declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().First();

	protected CusEntryLine EntryLine => EntryHeader.AllEntryLines.Cast<CusEntryLine>().First();

	protected ExportDeclarationMessageSendingObjectParent SendingObjectParent => sendingObjectParent ??= new ExportDeclarationMessageSendingObjectParent(Declaration);
	ExportDeclarationMessageSendingObjectParent sendingObjectParent;

	protected ExportDeclarationMessageSendingObject SendingObject => sendingObject ??= (ExportDeclarationMessageSendingObject)SendingObjectParent.SendingObjectsCollection.First();
	ExportDeclarationMessageSendingObject sendingObject;
}
