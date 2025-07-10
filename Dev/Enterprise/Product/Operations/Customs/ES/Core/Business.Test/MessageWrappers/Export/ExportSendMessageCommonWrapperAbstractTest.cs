using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestsSubclassesOf(typeof(ExportSendMessageCommonWrapper))]
	public abstract class ExportSendMessageCommonWrapperAbstractTest<T> : WrapperHelperTest<T>
		where T : ExportSendMessageCommonWrapper
	{
		public void TestLocalReferenceNumber()
		{
			entryHeader.CH_BGMReference = "Reference";
			AssertEquals("Expected filled LocalReferenceNumber", ExpectedLocalReferenceNumber, wrapper.LocalReferenceNumber);
		}

		protected abstract ZString ExpectedLocalReferenceNumber { get; }

		protected abstract T GetWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData);

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge", true, mergeResult);

			entryHeader = declaration.CustomsEntryHeaders[0];

			wrapper = GetWrapper(entryHeader, Certificate);
		}
		protected JobDeclaration declaration;
		protected CusEntryInstruction entryInstruction;
		protected JobComInvoiceHeader invoiceHeader;
		protected JobComInvoiceLine invoiceLine;
		protected CusEntryHeader entryHeader;
		protected T wrapper;
	}
}
