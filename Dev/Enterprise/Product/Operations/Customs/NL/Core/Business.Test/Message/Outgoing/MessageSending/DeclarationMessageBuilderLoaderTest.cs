using CargoWise.Customs.NL.MessageContracts.DMS;
using CargoWise.Customs.NL.MessageContracts.DMS.Export;
using CargoWise.Customs.NL.MessageContracts.DMS.Import;
using CargoWise.Customs.NL.MessageContracts.DMSAmendment.Export;
using CargoWise.Customs.NL.MessageContracts.DMSAmendment.Import;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;
using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class DeclarationMessageBuilderLoaderTest : TestCaseWithFactory
{
	public void TestImportMessageBuilders_AMD()
	{
		AssertImportAmendmentMessageBuilder(ImportSendMessageTypes.Codes.AMD);
	}

	public void TestImportMessageBuilders_CRI()
	{
		declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Import;
		sendingObject.MessageType = ImportSendMessageTypes.Codes.CRI;

		var builder = DeclarationMessageBuilderLoader.Instance.GetMessageBuilder(sendingObject);
		AssertType<CRIMessageBuilder>(builder);
	}

	public void TestImportMessageBuilders_DEC_H1()
	{
		declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Import;
		sendingObject.MessageType = ExportSendMessageTypes.Codes.DEC;
		entryInstruction.CEI_Style = NLConstants.EntryStyles.DeclarationForEndUse;

		var builder = DeclarationMessageBuilderLoader.Instance.GetMessageBuilder(sendingObject);
		AssertType<H1MessageBuilder>(builder);
	}

	public void TestImportMessageBuilders_DEC_H2()
	{
		declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Import;
		sendingObject.MessageType = ExportSendMessageTypes.Codes.DEC;
		entryInstruction.CEI_Style = NLConstants.EntryStyles.DeclarationForCustWarehouse;

		var builder = DeclarationMessageBuilderLoader.Instance.GetMessageBuilder(sendingObject);
		AssertType<H2MessageBuilder>(builder);
	}

	public void TestImportMessageBuilders_DEC_H3()
	{
		declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Import;
		sendingObject.MessageType = ExportSendMessageTypes.Codes.DEC;
		entryInstruction.CEI_Style = NLConstants.EntryStyles.DeclarationTemporaryAdmission;

		var builder = DeclarationMessageBuilderLoader.Instance.GetMessageBuilder(sendingObject);
		AssertType<H3MessageBuilder>(builder);
	}

	public void TestImportMessageBuilders_DEC_H4()
	{
		declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Import;
		sendingObject.MessageType = ExportSendMessageTypes.Codes.DEC;
		entryInstruction.CEI_Style = NLConstants.EntryStyles.DeclarationInwardProcessing;

		var builder = DeclarationMessageBuilderLoader.Instance.GetMessageBuilder(sendingObject);
		AssertType<H4MessageBuilder>(builder);
	}

	public void TestImportMessageBuilders_DEC_H5()
	{
		declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Import;
		sendingObject.MessageType = ExportSendMessageTypes.Codes.DEC;
		entryInstruction.CEI_Style = NLConstants.EntryStyles.ImportSpecialFiscalTerritoriesDeclaration;

		var builder = DeclarationMessageBuilderLoader.Instance.GetMessageBuilder(sendingObject);
		AssertType<H5MessageBuilder>(builder);
	}

	public void TestImportMessageBuilders_DEC_H6()
	{
		declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Import;
		sendingObject.MessageType = ExportSendMessageTypes.Codes.DEC;
		entryInstruction.CEI_Style = NLConstants.EntryStyles.DeclarationFreeCirculation;

		var builder = DeclarationMessageBuilderLoader.Instance.GetMessageBuilder(sendingObject);
		AssertType<H6MessageBuilder>(builder);
	}

	public void TestImportMessageBuilders_DEC_I1()
	{
		declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Import;
		sendingObject.MessageType = ExportSendMessageTypes.Codes.DEC;
		entryInstruction.CEI_Style = NLConstants.EntryStyles.ImportSimplifiedDeclaration;

		var builder = DeclarationMessageBuilderLoader.Instance.GetMessageBuilder(sendingObject);
		AssertType<I1MessageBuilder>(builder);
	}

	public void TestImportMessageBuilders_PRE()
	{
		declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Import;
		sendingObject.MessageType = ImportSendMessageTypes.Codes.PRE;

		var builder = DeclarationMessageBuilderLoader.Instance.GetMessageBuilder(sendingObject);
		AssertType<I2MessageBuilder>(builder);
	}

	public void TestImportMessageBuilders_CAN()
	{
		declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Import;
		entryInstruction.CEI_Style = NLConstants.EntryStyles.DeclarationForEndUse;
		sendingObject.MessageType = ImportSendMessageTypes.Codes.CAN;

		var builder = DeclarationMessageBuilderLoader.Instance.GetMessageBuilder(sendingObject);
		AssertType<ImportInvalidationMessageBuilder>(builder);
	}

	public void TestExportMessageBuilders_AMD()
	{
		AssertExportAmendmentMessageBuilder(ExportSendMessageTypes.Codes.AMD);
	}

	public void TestExportMessageBuilders_CRE()
	{
		declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Export;
		sendingObject.MessageType = ExportSendMessageTypes.Codes.CRE;

		var builder = DeclarationMessageBuilderLoader.Instance.GetMessageBuilder(sendingObject);
		AssertType<CREMessageBuilder>(builder);
	}

	public void TestExportMessageBuilders_DEC_B1()
	{
		declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Export;
		sendingObject.MessageType = ExportSendMessageTypes.Codes.DEC;
		entryInstruction.CEI_Style = NLConstants.EntryStyles.ExportReExport;

		var builder = DeclarationMessageBuilderLoader.Instance.GetMessageBuilder(sendingObject);
		AssertType<B1MessageBuilder>(builder);
	}

	public void TestExportMessageBuilders_DEC_B2()
	{
		declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Export;
		sendingObject.MessageType = ExportSendMessageTypes.Codes.DEC;
		entryInstruction.CEI_Style = NLConstants.EntryStyles.SpecialProcessing;

		var builder = DeclarationMessageBuilderLoader.Instance.GetMessageBuilder(sendingObject);
		AssertType<B2MessageBuilder>(builder);
	}

	public void TestExportMessageBuilders_DEC_B3()
	{
		declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Export;
		sendingObject.MessageType = ExportSendMessageTypes.Codes.DEC;
		entryInstruction.CEI_Style = NLConstants.EntryStyles.UnionGoods;

		var builder = DeclarationMessageBuilderLoader.Instance.GetMessageBuilder(sendingObject);
		AssertType<B3MessageBuilder>(builder);
	}

	public void TestExportMessageBuilders_DEC_B4()
	{
		declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Export;
		sendingObject.MessageType = ExportSendMessageTypes.Codes.DEC;
		entryInstruction.CEI_Style = NLConstants.EntryStyles.SpecialFiscalTerritory;

		var builder = DeclarationMessageBuilderLoader.Instance.GetMessageBuilder(sendingObject);
		AssertType<B4MessageBuilder>(builder);
	}

	public void TestExportMessageBuilders_DEC_C1()
	{
		declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Export;
		sendingObject.MessageType = ExportSendMessageTypes.Codes.DEC;
		entryInstruction.CEI_Style = NLConstants.EntryStyles.ExportDeclarationC1;

		var builder = DeclarationMessageBuilderLoader.Instance.GetMessageBuilder(sendingObject);
		AssertType<C1MessageBuilder>(builder);
	}

	public void TestExportMessageBuilders_DEC_C2()
	{
		declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Export;
		sendingObject.MessageType = ExportSendMessageTypes.Codes.DEC;
		entryInstruction.CEI_Style = NLConstants.EntryStyles.ExportDeclarationC2;

		var builder = DeclarationMessageBuilderLoader.Instance.GetMessageBuilder(sendingObject);
		AssertType<C2MessageBuilder>(builder);
	}

	public void TestExportMessageBuilders_DEC_H2()
	{
		declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Export;
		sendingObject.MessageType = ExportSendMessageTypes.Codes.DEC;
		entryInstruction.CEI_Style = NLConstants.EntryStyles.DeclarationForCustWarehouse;

		var builder = DeclarationMessageBuilderLoader.Instance.GetMessageBuilder(sendingObject);
		AssertType<H2MessageBuilder>(builder);
	}

	public void TestExportMessageBuilders_FBK_B1()
	{
		declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Export;
		sendingObject.MessageType = ExportSendMessageTypes.Codes.FBK;
		entryInstruction.CEI_Style = NLConstants.EntryStyles.ExportReExport;

		var builder = DeclarationMessageBuilderLoader.Instance.GetMessageBuilder(sendingObject);
		AssertType<B1MessageBuilder>(builder);
	}

	public void TestExportMessageBuilders_FBK_B2()
	{
		declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Export;
		sendingObject.MessageType = ExportSendMessageTypes.Codes.FBK;
		entryInstruction.CEI_Style = NLConstants.EntryStyles.SpecialProcessing;

		var builder = DeclarationMessageBuilderLoader.Instance.GetMessageBuilder(sendingObject);
		AssertType<B2MessageBuilder>(builder);
	}

	public void TestExportMessageBuilders_FBK_B3()
	{
		declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Export;
		sendingObject.MessageType = ExportSendMessageTypes.Codes.FBK;
		entryInstruction.CEI_Style = NLConstants.EntryStyles.UnionGoods;

		var builder = DeclarationMessageBuilderLoader.Instance.GetMessageBuilder(sendingObject);
		AssertType<B3MessageBuilder>(builder);
	}

	public void TestExportMessageBuilders_FBK_B4()
	{
		declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Export;
		sendingObject.MessageType = ExportSendMessageTypes.Codes.FBK;
		entryInstruction.CEI_Style = NLConstants.EntryStyles.SpecialFiscalTerritory;

		var builder = DeclarationMessageBuilderLoader.Instance.GetMessageBuilder(sendingObject);
		AssertType<B4MessageBuilder>(builder);
	}

	public void TestExportMessageBuilders_FBK_C1()
	{
		declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Export;
		sendingObject.MessageType = ExportSendMessageTypes.Codes.FBK;
		entryInstruction.CEI_Style = NLConstants.EntryStyles.ExportDeclarationC1;

		var builder = DeclarationMessageBuilderLoader.Instance.GetMessageBuilder(sendingObject);
		AssertType<C1MessageBuilder>(builder);
	}

	public void TestExportMessageBuilders_FBK_C2()
	{
		declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Export;
		sendingObject.MessageType = ExportSendMessageTypes.Codes.FBK;
		entryInstruction.CEI_Style = NLConstants.EntryStyles.ExportDeclarationC2;

		var builder = DeclarationMessageBuilderLoader.Instance.GetMessageBuilder(sendingObject);
		AssertType<C2MessageBuilder>(builder);
	}

	public void TestExportMessageBuilders_FBK_H2()
	{
		declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Export;
		sendingObject.MessageType = ExportSendMessageTypes.Codes.FBK;
		entryInstruction.CEI_Style = NLConstants.EntryStyles.DeclarationForCustWarehouse;

		var builder = DeclarationMessageBuilderLoader.Instance.GetMessageBuilder(sendingObject);
		AssertType<H2MessageBuilder>(builder);
	}

	public void TestExportMessageBuilders_EXT()
	{
		declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Export;
		sendingObject.MessageType = ExportSendMessageTypes.Codes.EXT;

		var builder = DeclarationMessageBuilderLoader.Instance.GetMessageBuilder(sendingObject);
		AssertType<ExitInfoMessageBuilder>(builder);
	}

	public void TestExportMessageBuilders_PRE()
	{
		declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Export;
		sendingObject.MessageType =	ExportSendMessageTypes.Codes.PRE;

		var builder = DeclarationMessageBuilderLoader.Instance.GetMessageBuilder(sendingObject);
		AssertType<I2MessageBuilder>(builder);
	}

	public void TestExportMessageBuilders_CAN()
	{
		declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Export;
		sendingObject.MessageType = ExportSendMessageTypes.Codes.CAN;
		entryInstruction.CEI_Style = NLConstants.EntryStyles.ExportReExport;

		var builder = DeclarationMessageBuilderLoader.Instance.GetMessageBuilder(sendingObject);
		AssertType<ExportInvalidationMessageBuilder>(builder);
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		entryInstruction = (CusEntryInstruction)declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_JE = declaration.PK;
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		sendingObject = new JobDeclarationMessageSendingObject(entryHeader);
	}
	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
	CusEntryHeader entryHeader;
	JobDeclarationMessageSendingObject sendingObject;

	void AssertExportAmendmentMessageBuilder(ZString messageType)
	{
		_ = WrapperTestHelper.CreateSentMessage(entryHeader, ExportSendMessageTypes.Codes.DEC, "EXA");

		declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Export;
		entryInstruction.CEI_Style = EntryStyles.ExportReExport;
		entryHeader.CH_EntryStatus = EntryStatus.AdvanceDeclarationReceived;
		sendingObject.MessageType = messageType;

		var builder = DeclarationMessageBuilderLoader.Instance.GetMessageBuilder(sendingObject);
		AssertType<ExportAmendmentMessageBuilder>(builder);
	}

	void AssertImportAmendmentMessageBuilder(ZString messageType)
	{
		_ = WrapperTestHelper.CreateSentMessage(entryHeader, ExportSendMessageTypes.Codes.DEC, "EXA");

		declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Import;
		entryInstruction.CEI_Style = EntryStyles.DeclarationForEndUse;
		entryHeader.CH_EntryStatus = EntryStatus.AdvanceDeclarationReceived;
		sendingObject.MessageType = messageType;

		var builder = DeclarationMessageBuilderLoader.Instance.GetMessageBuilder(sendingObject);
		AssertType<ImportAmendmentMessageBuilder>(builder);
	}
}
