using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(MessageManagerFactory))]
sealed class MessageManagerFactoryTest : TestCaseWithFactory
{
	public void TestCreateNew_Null()
	{
		var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
		var sendingObject = new MessageSendingObjectForTest(entryHeader);

		CombineAssertions(() =>
		{
			AssertNull(MessageManagerFactory.CreateNew(null));
			AssertNull(MessageManagerFactory.CreateNew(sendingObject));
		});
	}

	public void TestCreateNew_ImportDeclarationMessageManager()
	{
		var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
		var sendingObject = new ImportDeclarationMessageSendingObject(entryHeader);

		AssertType<ImportDeclarationMessageManager>(MessageManagerFactory.CreateNew(sendingObject));
	}

	public void TestCreateNew_EbdDeclarationMessageManager()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var sendingObject = new SupportingDocSendingObject(declaration);

		AssertType<EbdMessageManager>(MessageManagerFactory.CreateNew(sendingObject));
	}

	public void TestCreateNew_EComplaintMessageManager()
	{
		var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
		var sendingObject = new EComplaintMessageSendingObject(entryHeader);

		AssertType<EComplaintMessageManager>(MessageManagerFactory.CreateNew(sendingObject));
	}

	public void TestCreateNew_EvvMessageManager()
	{
		var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
		var sendingObject = new EvvRequestSendingObject(entryHeader, ZString.Empty, 0, ZString.Empty);

		AssertType<EvvMessageManager>(MessageManagerFactory.CreateNew(sendingObject));
	}

	public void TestCreateNew_NC016MessageManager() => AssertExportCreateNewMessageManager<NC016MessageManager>(PassarMessageTypeList.Codes.NC016);

	public void TestCreateNew_NC123MessageManager() => AssertExportCreateNewMessageManager<NC123MessageManager>(PassarMessageTypeList.Codes.NC123);

	public void TestCreateNew_NE014MessageManager() => AssertExportCreateNewMessageManager<NE014MessageManager>(PassarMessageTypeList.Codes.NE014);

	public void TestCreateNew_NE013MessageManager() => AssertExportCreateNewMessageManager<NE013MessageManager>(PassarMessageTypeList.Codes.NE013);

	public void TestCreateNew_NE015MessageManager() => AssertExportCreateNewMessageManager<NE015MessageManager>(PassarMessageTypeList.Codes.NE015);

	public void TestCreateNew_NE069MessageManager() => AssertExportCreateNewMessageManager<NE069MessageManager>(PassarMessageTypeList.Codes.NE069);

	public void TestCreateNew_NE130MessageManager() => AssertExportCreateNewMessageManager<NE130MessageManager>(PassarMessageTypeList.Codes.NE130);

	void AssertExportCreateNewMessageManager<TMessageManager>(string messageType)
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var sendingObjectParent = new ExportDeclarationMessageSendingObjectParent(declaration);
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		var sendingObject = new ExportDeclarationMessageSendingObject(sendingObjectParent, entryHeader);
		sendingObject.MessageType = messageType;

		AssertType<TMessageManager>(MessageManagerFactory.CreateNew(sendingObject));
	}
}

public class MessageSendingObjectForTest : DeclarationMessageSendingObject
{
	public MessageSendingObjectForTest(CusEntryHeader header) : base(header)
	{
	}

	public override ZString FriendlyNameForMessageManager => "Class is for testing purpose only";

	public override ZGuid GetCredentialPK() => ZGuid.Empty;
}
