using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestsSubclassesOf(typeof(DVDCommonSendMessageWrapper))]
	public abstract class DVDCommonSendMessageWrapperAbstractTest<T> : WrapperHelperTest<T>
		where T : DVDCommonSendMessageWrapper
	{
		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "11";

			AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

			entryHeader = declaration.CustomsEntryHeaders[0];

			wrapper = GetWrapper(entryHeader, Certificate);
		}

		protected JobDeclaration declaration;
		protected JobComInvoiceHeader invoiceHeader;
		protected CusEntryInstruction entryInstruction;
		protected JobComInvoiceLine invoiceLine;
		protected CusEntryHeader entryHeader;
		protected T wrapper;

		protected abstract T GetWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData);

		protected override T GetProvider() => wrapper;

		protected virtual ZString ExpectedMRNCode => MovementReferenceNumber;
	}
}
