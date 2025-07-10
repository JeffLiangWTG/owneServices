using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestsSubclassesOf(typeof(ImportCommonSendMessageWrapper))]
	public abstract class ImportCommonSendMessageWrapperAbstractTest<T> : WrapperHelperTest<T>
		where T : ImportCommonSendMessageWrapper
	{
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

			entryHeader = declaration.CustomsEntryHeaders[0];

			wrapper = GetWrapper(entryHeader, Certificate);
		}
		protected JobDeclaration declaration;
		protected JobComInvoiceHeader invoiceHeader;
		protected JobComInvoiceLine invoiceLine;
		protected CusEntryHeader entryHeader;
		protected T wrapper;

		protected abstract T GetWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData);

		protected override T GetProvider() => wrapper;

		protected virtual ZString ExpectedMRNCode => MovementReferenceNumber;
	}
}
