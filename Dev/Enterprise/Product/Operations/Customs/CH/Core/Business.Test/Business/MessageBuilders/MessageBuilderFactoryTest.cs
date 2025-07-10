using CargoWise.Customs.CH.MessageContracts.Chartera.Outgoing;
using CargoWise.Customs.CH.MessageContracts.Ebd.Version0_2;
using CargoWise.Customs.CH.MessageContracts.Edec.Bordereau.Version1_0;
using CargoWise.Customs.CH.MessageContracts.Edec.ECom.Version1_0;
using CargoWise.Customs.CH.MessageContracts.Edec.Evv.Version3_0;
using CargoWise.Customs.CH.MessageContracts.Edec.GoodsDeclarations.Version4_0;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(MessageBuilderFactory))]
public sealed class MessageBuilderFactoryTest : TestCaseWithFactory
{
	public void TestNewMessageBuilder_Null()
	{
		AssertNull(MessageBuilderFactory.NewMessageBuilder(null));
	}

	public void TestNewMessageBuilder_EdecImportMessageBuilder()
	{
		var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
		var sendingObject = new ImportDeclarationMessageSendingObject(entryHeader);

		AssertType<EdecMessageBuilder>(MessageBuilderFactory.NewMessageBuilder(sendingObject));
	}

	public void TestNewMessageBuilder_EbdMessageBuilder()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var sendingObject = new SupportingDocSendingObject(declaration);

		AssertType<EbdMessageBuilder>(MessageBuilderFactory.NewMessageBuilder(sendingObject));
	}

	public void TestNewMessageBuilder_EComMessageBuilder()
	{
		var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
		var sendingObject = new EComplaintMessageSendingObject(entryHeader);

		AssertType<EdecComplaintRequestBuilder>(MessageBuilderFactory.NewMessageBuilder(sendingObject));
	}

	public void TestNewMessageBuilder_EvvMessageBuilder()
	{
		var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
		var sendingObject = new EvvRequestSendingObject(entryHeader, ZString.Empty, 0, "XXX");

		AssertType<EdecEvvRequestMessageBuilder>(MessageBuilderFactory.NewMessageBuilder(sendingObject));
	}

	public void TestNewMessageBuilder_BordereauListRequestMessageBuilder()
	{
		var sendingObject = new BordereauListRequestSendingObject();

		AssertType<EdecBordereauListRequestMessageBuilder>(MessageBuilderFactory.NewMessageBuilder(sendingObject));
	}

	public void TestNewMessageBuilder_CharteraDocumentSearchRequestMessageBuilder()
	{
		var sendingObject = new CharteraOutputDocumentSearchSendingObject(Factory);
		AssertType<DocumentSearchRequestV1MessageBuilder>(MessageBuilderFactory.NewMessageBuilder(sendingObject));
	}

	public void TestNewMessageBuilder_NC016() => AssertExportNewMessageBuilder<NC016_v1MessageBuilder>(PassarMessageTypeList.Codes.NC016);

	public void TestNewMessageBuilder_NE013v3() => AssertExportNewMessageBuilder<NE013_v3MessageBuilder>(PassarMessageTypeList.Codes.NE013, FunctionalityTypes.CHNE015V3, true);

	public void TestNewMessageBuilder_NE013v4() => AssertExportNewMessageBuilder<NE013_v4MessageBuilder>(PassarMessageTypeList.Codes.NE013, FunctionalityTypes.CHNE015V3, false);

	public void TestNewMessageBuilder_NE014() => AssertExportNewMessageBuilder<NE014_v3MessageBuilder>(PassarMessageTypeList.Codes.NE014);

	public void TestNewMessageBuilder_NE015v3() => AssertExportNewMessageBuilder<NE015_v3MessageBuilder>(PassarMessageTypeList.Codes.NE015, FunctionalityTypes.CHNE015V3, true);

	public void TestNewMessageBuilder_NE015v4() => AssertExportNewMessageBuilder<NE015_v4MessageBuilder>(PassarMessageTypeList.Codes.NE015, FunctionalityTypes.CHNE015V3, false);

	public void TestNewMessageBuilder_NE069() => AssertExportNewMessageBuilder<NE069_v1MessageBuilder>(PassarMessageTypeList.Codes.NE069);

	public void TestNewMessageBuilder_NE130() => AssertExportNewMessageBuilder<NE130_v1MessageBuilder>(PassarMessageTypeList.Codes.NE130);

	public void TestNewMessageBuilder_NC123() => AssertExportNewMessageBuilder<NC123_v1MessageBuilder>(PassarMessageTypeList.Codes.NC123);

	void AssertExportNewMessageBuilder<TMessageBuilder>(string messageType, string funcsCode, bool funcsCodeActive)
	{
		using (FuncsTestHelper.TemporarilySetFunctionality(funcsCode, funcsCodeActive))
		{
			AssertExportNewMessageBuilder<TMessageBuilder>(messageType);
		}
	}

	void AssertExportNewMessageBuilder<TMessageBuilder>(string messageType)
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var sendObjectParent = new ExportDeclarationMessageSendingObjectParent(declaration);
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		var sendingObject = new ExportDeclarationMessageSendingObject(sendObjectParent, entryHeader);
		sendingObject.MessageType = messageType;
		
		AssertType<TMessageBuilder>(MessageBuilderFactory.NewMessageBuilder(sendingObject));
	}
}
