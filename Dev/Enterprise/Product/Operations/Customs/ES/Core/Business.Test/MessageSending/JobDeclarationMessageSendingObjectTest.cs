using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.EDIMessages;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.Testing;

[TestedType(typeof(MessageSending.JobDeclarationMessageSendingObject))]
public class JobDeclarationMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
{
	protected override BusinessObject GetNewBusinessObject()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		return new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
	}

	public void TestReadOnlyPropertiesShouldSendFalseExport()
	{
		CombineAssertions(() =>
		{
			testItem.ShouldSend = false;
			AssertEquals("MessageType is readonly when is Send? is not ticked", true, testItem.MessageTypeInfo.ReadOnly);
			AssertEquals("Sub Type  is readonly when is Send? is not ticked", true, testItem.MessageSubTypeInfo.ReadOnly);
		});
	}

	public void TestReadOnlyPropertiesShouldSendTrueExport()
	{
		entryHeader.ZG_UCC6Version = 0;
		CombineAssertions(() =>
		{
			testItem.ShouldSend = true;
			AssertEquals("Message Type is readonly when is Send? is ticked and dec is Export", true, testItem.MessageTypeInfo.ReadOnly);
			AssertEquals("Message Sub Type is readonly when is Send? is ticked and dec is Export, entry instruction substyle is not in (B,C) and entry status is not CLP or not T2L and CLR", true, testItem.MessageSubTypeInfo.ReadOnly);
		});
	}

	public void TestReadOnlyPropertiesShouldSendTrueExportSubTypeBEntryStatusCLP()
	{
		CombineAssertions(() =>
		{
			testItem.ShouldSend = true;
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
			AssertEquals("Message Sub Type is readonly when is Send? is ticked and dec is Export, entry instruction substyle is in B but entry status is not CLP", true, testItem.MessageSubTypeInfo.ReadOnly);

			entryHeader.ZG_UCC6Version = 0;
			entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
			AssertEquals("Message Sub Type is not readonly when is Send? is ticked and dec is Export, entry instruction substyle is in B, entry status is CLP and Ucc6Version = 0", false, testItem.MessageSubTypeInfo.ReadOnly);

			entryHeader.ZG_UCC6Version = 1;
			AssertEquals("Message Sub Type is not readonly when is Send? is ticked and dec is Export, entry instruction substyle is B, entry status is CLP and Ucc6Version > 0", false, testItem.MessageSubTypeInfo.ReadOnly);
		});
	}

	public void TestReadOnlyPropertiesShouldSendTrueExportSubTypeCEntryStatusCLP()
	{
		CombineAssertions(() =>
		{
			testItem.ShouldSend = true;
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.C;
			AssertEquals("Message Sub Type is readonly when is Send? is ticked and dec is Export, entry instruction substyle is in C but entry status is not CLP", true, testItem.MessageSubTypeInfo.ReadOnly);

			entryHeader.ZG_UCC6Version = 0;
			entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
			AssertEquals("Message Sub Type is not readonly when is Send? is ticked and dec is Export, entry instruction substyle is in C, entry status is CLP and Ucc6Version = 0", false, testItem.MessageSubTypeInfo.ReadOnly);

			entryHeader.ZG_UCC6Version = 1;
			AssertEquals("Message Sub Type is not readonly when is Send? is ticked and dec is Export, entry instruction substyle is in C, entry status is CLP and Ucc6Version > 0", false, testItem.MessageSubTypeInfo.ReadOnly);
		});
	}

	public void TestReadOnlyPropertiesShouldSendTrueExportT2L()
	{
		testItem.ShouldSend = true;
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
		AssertEquals("Message Sub Type is readonly when is Send? is ticked and dec is Export and entry instruction substyle is T2L", true, testItem.MessageSubTypeInfo.ReadOnly);
	}

	public void TestReadOnlyPropertiesShouldSendTrueImportT2LNoPousEntryStatusCLR()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		testItem.ShouldSend = true;
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
		entryHeader.ZG_POUSVersion = 0;
		entryHeader.CH_EntryStatus = EntryStatusCodes.Cleared;
		AssertEquals("Message Sub Type is not readonly when is Send? is ticked and dec is Import and entry instruction substyle is T2L no Pous and Entry status is CLR", false, testItem.MessageSubTypeInfo.ReadOnly);
	}

	public void TestReadOnlyPropertiesShouldSendTrueImportT2LPousEntryStatusCLR()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		testItem.ShouldSend = true;
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
		entryHeader.ZG_POUSVersion = 1;
		entryHeader.CH_EntryStatus = EntryStatusCodes.Cleared;
		AssertEquals("Message Sub Type is not readonly when is Send? is ticked and dec is Import and entry instruction substyle is T2L Pous and Entry status is CLR", true, testItem.MessageSubTypeInfo.ReadOnly);
	}

	public void TestReadOnlyPropertiesShouldSendTrueExportSubTypeEXS()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			testItem.ShouldSend = true;
			entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
			AssertEquals("Message Sub Type is readonly when is Send? is ticked and dec is Export, entry instruction substyle is EXS and Entry Status is empty", true, testItem.MessageSubTypeInfo.ReadOnly);

			entryHeader.CH_EntryStatus = EntryStatusCodes.Cleared;
			AssertEquals("Message Sub Type is readonly when is Send? is ticked and dec is Export, entry instruction substyle is EXS and Entry Status is CLR", false, testItem.MessageSubTypeInfo.ReadOnly);
		});
	}

	public void TestReadOnlyPropertiesShouldSendTrueExportUcc6SubTypeABCYZEntryStatusPDA()
	{
		CombineAssertions(() =>
		{
			testItem.ShouldSend = true;
			entryHeader.ZG_UCC6Version = 1;
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.C;
			AssertEquals("Message Sub Type is readonly when is Send? is ticked and dec is Export, entry instruction substyle is in (A, B, C, Y, Z) and Ucc6Version > 0 but entry status is not PDA", true, testItem.MessageSubTypeInfo.ReadOnly);

			entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;
			AssertEquals("Message Sub Type is not readonly when is Send? is ticked and dec is Export, entry instruction substyle is in (A, B, C, Y, Z) and Ucc6Version > 0 and entry status is PDA", false, testItem.MessageSubTypeInfo.ReadOnly);
		});
	}

	public void TestReadOnlyPropertiesShouldSendTrueExportUcc6SubTypeABCYZEntryStatusCLR()
	{
		CombineAssertions(() =>
		{
			testItem.ShouldSend = true;
			entryHeader.ZG_UCC6Version = 1;
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Z;
			AssertEquals("Message Sub Type is readonly when is Send? is ticked and dec is Export, entry instruction substyle is in (A, B, C, Y, Z) and Ucc6Version > 0 but entry status is not CLR", true, testItem.MessageSubTypeInfo.ReadOnly);

			entryHeader.CH_EntryStatus = EntryStatusCodes.Cleared;
			AssertEquals("Message Sub Type is not readonly when is Send? is ticked and dec is Export, entry instruction substyle is in (A, B, C, Y, Z) and Ucc6Version > 0 and entry status is CLR", false, testItem.MessageSubTypeInfo.ReadOnly);
		});
	}

	public void TestReadOnlyPropertiesShouldSendTrueExportUcc6SubTypeABCYZEntryStatusCLP()
	{
		CombineAssertions(() =>
		{
			testItem.ShouldSend = true;
			entryHeader.ZG_UCC6Version = 1;
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			AssertEquals("Message Sub Type is readonly when is Send? is ticked and dec is Export, entry instruction substyle is in (A, B, C, Y, Z) and Ucc6Version > 0 but entry status is not CLP", true, testItem.MessageSubTypeInfo.ReadOnly);

			entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
			AssertEquals("Message Sub Type is not readonly when is Send? is ticked and dec is Export, entry instruction substyle is in (A, B, C, Y, Z) and Ucc6Version > 0 and entry status is CLP", false, testItem.MessageSubTypeInfo.ReadOnly);
		});
	}

	public void DeclarationImport()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		Factory.Save();
	}

	public void TestReadOnlyPropertiesShouldSendTrueImportSubTypeEmpty()
	{
		DeclarationImport();
		entryInstruction.CEI_SubStyle = ZString.Empty;
		testItem.MessageSubType = ZString.Empty;
		AssertEquals("Message Type is readonly when is Send? is ticked and dec is Import and Message Sub Type is not ORG", true, testItem.MessageTypeInfo.ReadOnly);
	}

	public void TestReadOnlyPropertiesShouldSendTrueImportNotH2SubTypeORG_NoUcc6()
	{
		DeclarationImport();
		entryInstruction.CEI_SubStyle = ZString.Empty;
		testItem.ShouldSend = true;
		testItem.MessageSubType = DeclarationMessageSubTypeList.Codes.OriginalDeclaration;
		entryHeader.ZG_UCC6Version = 0;
		AssertEquals("Message Type not readonly when is Send? is ticked and dec is Import, is not Ucc6, entry instruction declaration type is not H2 and entry status is empty and Message Sub Type is ORG", false, testItem.MessageTypeInfo.ReadOnly);
	}

	public void TestReadOnlyPropertiesShouldSendTrueImportNotH2SubTypeORG_Ucc6()
	{
		DeclarationImport();
		entryInstruction.CEI_SubStyle = ZString.Empty;
		testItem.ShouldSend = true;
		testItem.MessageSubType = DeclarationMessageSubTypeList.Codes.OriginalDeclaration;
		entryHeader.ZG_UCC6Version = 1;
		AssertEquals("Message Type is readonly when is Send? is ticked and dec is Import, is Ucc6, entry instruction declaration type is not H2 and entry status is empty and Message Sub Type is ORG", true, testItem.MessageTypeInfo.ReadOnly);
	}

	public void TestReadOnlyPropertiesShouldSendTrueImportH2SubTypeORG()
	{
		DeclarationImport();
		entryInstruction.CEI_SubStyle = ZString.Empty;
		entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
		testItem.ShouldSend = true;
		testItem.MessageSubType = DeclarationMessageSubTypeList.Codes.OriginalDeclaration;
		AssertEquals("Message Type not readonly when is Send? is ticked and dec is Import, entry instruction declaration type is H2 and entry status is empty and Message Sub Type is ORG", true, testItem.MessageTypeInfo.ReadOnly);
	}

	public void TestReadOnlyPropertiesShouldSendTrueImportH2SubTypeAMD()
	{
		DeclarationImport();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Z;
		entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
		entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;
		testItem.ShouldSend = true;
		testItem.MessageSubType = DeclarationMessageSubTypeList.Codes.Amendment;
		AssertEquals("Message Type not readonly when is Send? is ticked and dec is Import, entry instruction declaration type is H2 and entry status is PDA and Message Sub Type is AMD", true, testItem.MessageTypeInfo.ReadOnly);
	}

	public void TestReadOnlyPropertiesShouldSendTrueImportH2SubTypeCAN()
	{
		DeclarationImport();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Z;
		entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
		entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;
		testItem.ShouldSend = true;
		testItem.MessageSubType = DeclarationMessageSubTypeList.Codes.Cancellation;
		AssertEquals("Message Type not readonly when is Send? is ticked and dec is Import, entry instruction declaration type is H2 and entry status is PDA and Message Sub Type is CAN", true, testItem.MessageTypeInfo.ReadOnly);
	}

	public void TestReadOnlyPropertiesShouldSendTrueImportSubTypeORGEntryStatusPDI()
	{
		DeclarationImport();
		entryInstruction.CEI_SubStyle = ZString.Empty;
		entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
		AssertEquals("Message Type is readonly when is Send? is ticked and dec is Import and entry status is not empty and Message Sub Type is ORG", true, testItem.MessageTypeInfo.ReadOnly);
	}

	public void TestReadOnlyPropertiesShouldSendTrueImportEntryStatusEmpty()
	{
		DeclarationImport();
		entryInstruction.CEI_SubStyle = ZString.Empty;
		entryHeader.CH_EntryStatus = ZString.Empty;
		AssertEquals("Message Sub Type is readonly when is Send? is ticked and dec is Import, entry instruction substyle is not in (A,B,C,Y,Z) or entry status is not PDI or PDA", true, testItem.MessageSubTypeInfo.ReadOnly);
	}

	public void TestReadOnlyPropertiesShouldSendTrueImportEntryStatusEmptySubStyleZ()
	{
		DeclarationImport();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Z;
		entryHeader.CH_EntryStatus = ZString.Empty;
		AssertEquals("Message Sub Type is readonly when is Send? is ticked and dec is Import, entry instruction substyle is not in (A,B,C,Y,Z) or entry status is not PDI or PDA", true, testItem.MessageSubTypeInfo.ReadOnly);
	}

	public void TestReadOnlyPropertiesShouldSendTrueImportNotH2EntryStatusPDIorPDA_NoUcc6()
	{
		DeclarationImport();
		testItem.ShouldSend = true;
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
		entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
		entryHeader.ZG_UCC6Version = 0;
		AssertEquals("Message Sub Type is not readonly when is Send? is ticked and dec is Import, is not Ucc6, entry instruction substyle is in (A,B,C,Y,Z), entry instruction declaration type is not H2 and entry status is PDI or PDA", false, testItem.MessageSubTypeInfo.ReadOnly);
	}

	public void TestReadOnlyPropertiesShouldSendTrueImportNotH2EntryStatusPDIorPDA_Ucc6()
	{
		DeclarationImport();
		testItem.ShouldSend = true;
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
		entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
		entryHeader.ZG_UCC6Version = 1;
		AssertEquals("Message Sub Type is readonly when is Send? is ticked and dec is Import, is Ucc6, entry instruction substyle is in (A,B,C,Y,Z), entry instruction declaration type is not H2 and entry status is PDI or PDA", true, testItem.MessageSubTypeInfo.ReadOnly);
	}

	public void TestReadOnlyPropertiesShouldSendTrueImportNotH2EntryStatusPDIorPDASubStyleZ_NoUcc6()
	{
		DeclarationImport();
		testItem.ShouldSend = true;
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Z;
		entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;
		entryHeader.ZG_UCC6Version = 0;
		AssertEquals("Message Sub Type is not readonly when is Send? is ticked and dec is Import, is not Ucc6, entry instruction substyle is in (A,B,C,Y,Z), entry instruction declaration type is not H2 and entry status is PDI or PDA", false, testItem.MessageSubTypeInfo.ReadOnly);
	}

	public void TestReadOnlyPropertiesShouldSendTrueImportH2EntryStatusPDA_NoUcc6()
	{
		DeclarationImport();
		testItem.ShouldSend = true;
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Z;
		entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
		entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;
		entryHeader.ZG_UCC6Version = 0;
		AssertEquals("Message Sub Type is not readonly when is Send? is ticked and dec is Import, is not Ucc6, entry instruction substyle is in (A,B,C,Y,Z), entry instruction declaration type is H2 and entry status is PDA", false, testItem.MessageSubTypeInfo.ReadOnly);
	}

	public void TestReadOnlyPropertiesShouldSendTrueImportEntryStatusPDISubStyleZSubtypeAMD()
	{
		DeclarationImport();
		testItem.ShouldSend = true;
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Z;
		entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;
		testItem.MessageSubType = DeclarationMessageSubTypeList.Codes.Amendment;
		AssertEquals("Message Type not readonly when is Send? is ticked and dec is Import, entry instruction substyle is in (A,B,Y,Z), entry status is PDA and Message Sub Type is AMD", false, testItem.MessageTypeInfo.ReadOnly);
	}

	public void TestReadOnlyPropertiesShouldSendTrueImportEntryStatusPDISubStyleT2LSubtypeORG()
	{
		DeclarationImport();
		testItem.MessageSubType = DeclarationMessageSubTypeList.Codes.OriginalDeclaration;
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
		CombineAssertions(() =>
		{
			AssertEquals("Message Sub Type is readonly when is Send? is ticked and dec is Import, entry instruction substyle is T2L but entry status is not CLP or CLR", true, testItem.MessageSubTypeInfo.ReadOnly);
			AssertEquals("Message Type is readonly when is Send? is ticked and dec is Import and entry instruction substyle is T2L", true, testItem.MessageTypeInfo.ReadOnly);
		});
	}

	public void TestReadOnlyPropertiesShouldSendTrueImportEntryStatusCLPSubStyleT2LSubtypeORG()
	{
		DeclarationImport();
		testItem.ShouldSend = true;
		testItem.MessageSubType = DeclarationMessageSubTypeList.Codes.OriginalDeclaration;
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
		entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
		CombineAssertions(() =>
		{
			AssertEquals("Message Sub Type is not readonly when is Send? is ticked and dec is Import, entry instruction substyle is T2L and entry status is CLP or CLR", false, testItem.MessageSubTypeInfo.ReadOnly);
			AssertEquals("Message Type is readonly when is Send? is ticked and dec is Import and entry instruction substyle is T2L", true, testItem.MessageTypeInfo.ReadOnly);
		});
	}

	public void TestReadOnlyPropertiesShouldSendTrueImportSubStyleT2CSubtypeORG()
	{
		DeclarationImport();
		testItem.MessageSubType = DeclarationMessageSubTypeList.Codes.OriginalDeclaration;
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
		AssertEquals("Message Type is readonly when is Send? is ticked and dec is Import and entry instruction substyle is T2C", true, testItem.MessageTypeInfo.ReadOnly);
	}

	public void TestReadOnlyPropertiesShouldSendTrueUCC6SubStyleABCAndNotPDAorCLPorCLR()
	{
		entryHeader.ZG_UCC6Version = 1;
		testItem.ShouldSend = true;

		CombineAssertions(() =>
		{
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;
			AssertEquals("Message Type is readonly when is Send? is ticked and dec is Export UCC6 and entry instruction is A but entry status is PDA", true, testItem.MessageTypeInfo.ReadOnly);

			entryHeader.CH_EntryStatus = ZString.Empty;
			AssertEquals("Message Type is not readonly when is Send? is ticked and dec is Export UCC6 and entry instruction is A", false, testItem.MessageTypeInfo.ReadOnly);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
			AssertEquals("Message Type is not readonly when is Send? is ticked and dec is Export UCC6 and entry instruction is B", false, testItem.MessageTypeInfo.ReadOnly);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.C;
			AssertEquals("Message Type is not readonly when is Send? is ticked and dec is Export UCC6 and entry instruction is C", false, testItem.MessageTypeInfo.ReadOnly);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Z;
			AssertEquals("Message Type is readonly when is Send? is ticked and dec is Export UCC6 and entry instruction is Z", true, testItem.MessageTypeInfo.ReadOnly);

			entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
			AssertEquals("Message Type is readonly when is Send? is ticked and dec is Export UCC6 and entry instruction is B but EntryStatus is CLP", true, testItem.MessageTypeInfo.ReadOnly);

			entryHeader.CH_EntryStatus = EntryStatusCodes.Cleared;
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.C;
			AssertEquals("Message Type is readonly when is Send? is ticked and dec is Export UCC6 and entry instruction is C but EntryStatus is CLR", true, testItem.MessageTypeInfo.ReadOnly);
		});
	}

	public void TestSendColumnNoEntryInstruction()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		var testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
		AssertEquals("There is no entry instruction so it cannot be sent", true, testItem.ShouldSendInfo.ReadOnly);
	}

	public void TestSendColumnNoLastWaitingForResponse()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		AssertEquals("There is no last message, so it is not waiting for response", false, testItem.ShouldSendInfo.ReadOnly);
	}

	public void TestSendColumnMockWaitingForResponse()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		var message = Factory.New<TestEDIMessage>();
		entryHeader.Messages.Add(message);
		message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		message.EM_Status = EDIMessage.Status.Sent;
		entryHeader.CH_Status = Common.EU.MessageStatusList.Codes.AwaitingResponse;
		testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
		CombineAssertions(() =>
		{
			AssertEquals(entryHeader.Messages.LastMessage, message);
			AssertEquals("There is a mock of the last message waiting for a response", true, testItem.ShouldSendInfo.ReadOnly);
		});
	}

	public void TestSendColumnMockRejected()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		var message = Factory.New<TestEDIMessage>();
		entryHeader.Messages.Add(message);
		message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		entryHeader.CH_Status = "";
		message.EM_Status = EDIMessage.Status.Rejected;
		testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
		CombineAssertions(() =>
		{
			AssertEquals(entryHeader.Messages.LastMessage, message);
			AssertEquals("There is a mock of the last message rejected", false, testItem.ShouldSendInfo.ReadOnly);
		});
	}

	public void TestSendColumnMockForT2lWaitingForResponse()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		var message2 = Factory.New<TestEDIMessage>();
		entryHeader.Messages.Add(message2);
		message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		message2.EM_Status = EDIMessage.Status.Sent;
		var docPivot = Factory.NewWithValidTestData<CusStorageDocPivot>();
		entryHeader.EDocPivotCollection.Add(docPivot);
		var messagePivot = Factory.New<GenPivot>();
		messagePivot.XX_RelationType = GenPivotTypes.CusStorageDocPivotEdiMessage;
		messagePivot.XX_Relation1ID = docPivot.PK;
		messagePivot.XX_Relation1TableCode = docPivot.TablePrefix;
		messagePivot.XX_Relation2ID = message2.PK;
		messagePivot.XX_Relation2TableCode = message2.TablePrefix;

		testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);

		CombineAssertions(() =>
		{
			AssertEquals(entryHeader.Messages.LastMessage, message2);
			AssertEquals("There is a mock of the last message for the t2l annex is waiting for a response", true, testItem.ShouldSendInfo.ReadOnly);
		});
	}

	public void TestProperties()
	{
		entryHeader.CH_EntryStatus = "CAN";
		entryHeader.CH_Status = "ACC";
		entryHeader.CH_BGMReference = "12345678916";

		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("ACC", testItem.MessageStatus);
			AssertEquals("CAN", testItem.EntryStatus);
			AssertEquals("12345678916", testItem.CH_BGMReference);
		});
	}

	public void TestMessageTypeListForExport()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		Factory.Save();
		var typeList = testItem.MessageTypesList;
		CombineAssertions(() =>
		{
			AssertEquals("Message Type List is CodeDescriptionPairList", typeof(CodeDescriptionPairList), typeList.GetType());
			AssertEquals("Message Type List is empty for Export", 0, typeList.Count);
		});
	}

	public void TestMessageTypeListForImportNotH2EntryStatusPDASubTypeAMD_NoUcc6()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Z;
		entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;
		entryHeader.ZG_UCC6Version = 0;
		testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
		{
			MessageSubType = DeclarationMessageSubTypeList.Codes.Amendment,
			ShouldSend = true
		};
		var typeList = testItem.MessageTypesList;
		CombineAssertions(() =>
		{
			AssertEquals("Message Type List is CodeDescriptionPairList", typeof(CodeDescriptionPairList), typeList.GetType());
			AssertContainsExactElementsInAnyOrder("Message Type List for Import, is not Ucc6, and entry instruction substyle is in (A,B,Y,Z), entry status is PDA and messagesubtype is AMD contains C40 and PDC", new ZString[] { "C40", "PDC" }, typeList.GetAllCodesZString());
			AssertEquals("Message Type List for Import, is not Ucc6, and entry instruction substyle is in (A,B,Y,Z), entry status is PDA and messagesubtype is AMD contains only 2 values", 2, typeList.Count);
		});
	}

	public void TestMessageTypeListForImportNotH2SubTypeCAN()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Z;
		entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;
		testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
		{
			MessageSubType = DeclarationMessageSubTypeList.Codes.Cancellation,
			ShouldSend = true
		};
		var typeList = testItem.MessageTypesList;
		CombineAssertions(() =>
		{
			AssertEquals("Message Type List is CodeDescriptionPairList", typeof(CodeDescriptionPairList), typeList.GetType());
			AssertEquals("Message Type List is empty for Import and messagesubtype is CAN", 0, typeList.Count);
		});
	}

	public void TestMessageTypeListForImportNotH2SubTypeORG_NoUcc6()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Z;
		entryHeader.ZG_UCC6Version = 0;
		testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
		{
			MessageSubType = DeclarationMessageSubTypeList.Codes.OriginalDeclaration,
			ShouldSend = true
		};
		var typeList = testItem.MessageTypesList;
		CombineAssertions(() =>
		{
			AssertEquals("Message Type List is CodeDescriptionPairList", typeof(CodeDescriptionPairList), typeList.GetType());
			AssertContainsExactElementsInAnyOrder("Message Type List for Import, is not Ucc6, and entry instruction substyle is in (A,B,Y,Z) and messagesubtype is ORG contains PDI and PDC", new ZString[] { "PDI", "PDC" }, typeList.GetAllCodesZString());
			AssertEquals("Message Type List for Import, is not Ucc6, and entry instruction substyle is in (A,B,Y,Z) and messagesubtype is ORG contains only 2 values", 2, typeList.Count);
		});
	}

	public void TestMessageTypeListForImportSubTypeORGSubStyleA_MultipleItems_NoUcc6()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
		entryHeader.ZG_UCC6Version = 0;
		testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
		{
			MessageSubType = DeclarationMessageSubTypeList.Codes.OriginalDeclaration,
			ShouldSend = true
		};

		CombineAssertions(() =>
		{
			var typeList = testItem.MessageTypesList;
			AssertContainsExactElementsInAnyOrder("Message Type List for Import, is not Ucc6, and entry instruction substyle is A and messagesubtype is ORG contains PDI and PDC", new ZString[] { "PDI", "PDC" }, typeList.GetAllCodesZString());
			AssertSame("Message Type List should be cached (for Import)", typeList, testItem.MessageTypesList);

			var invoice2 = declaration.Invoices.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.A;
			entryInstruction2.CEI_Style = IMPDeclarationTypeList.Codes.H2;
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "11";
			invoiceLine2.JI_CEI = entryInstruction2.PK;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entryHeader2 = declaration.CustomsEntryHeaders[1];
			var testItem2 = new MessageSending.JobDeclarationMessageSendingObject(entryHeader2)
			{
				MessageSubType = DeclarationMessageSubTypeList.Codes.OriginalDeclaration,
				ShouldSend = true
			};
			var typeList2 = testItem2.MessageTypesList;
			AssertEquals("Message Type List for Import and entry instruction substyle is A, entry instruction style is H2 and messagesubtype is ORG is empty", 0, typeList2.Count);
			AssertSame("Message Type List should be cached (for Import H2)", typeList2, testItem2.MessageTypesList);
		});
	}

	public void TestMessageTypeListForImportNotH2SubTypeORGSubStyleC_NoUcc6()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.C;
		entryHeader.ZG_UCC6Version = 0;
		testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
		{
			MessageSubType = DeclarationMessageSubTypeList.Codes.OriginalDeclaration,
			ShouldSend = true
		};

		Factory.Save();
		var typeList = testItem.MessageTypesList;
		CombineAssertions(() =>
		{
			AssertEquals("Message Type List is CodeDescriptionPairList", typeof(CodeDescriptionPairList), typeList.GetType());
			AssertContainsExactElementsInAnyOrder("Message Type List for Import, is not Ucc6, and entry instruction substyle C contains PDI and PDS", new ZString[] { "PDI", "PDS" }, typeList.GetAllCodesZString());
			AssertEquals("Message Type List for Import, is not Ucc6, and entry instruction substyle C contains only 2 values", 2, typeList.Count);
		});
	}

	public void TestMessageTypeListForImportNotH2SubTypeORGSubStyleEmpty()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryInstruction.CEI_SubStyle = ZString.Empty;
		testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
		{
			MessageSubType = DeclarationMessageSubTypeList.Codes.OriginalDeclaration,
			ShouldSend = true
		};

		Factory.Save();
		var typeList = testItem.MessageTypesList;
		CombineAssertions(() =>
		{
			AssertEquals("Message Type List is CodeDescriptionPairList", typeof(CodeDescriptionPairList), typeList.GetType());
			AssertEquals("Message Type List is empty when entry instruction substyle is empty", 0, typeList.Count);
		});
	}

	public void TestMessageTypeListForImportSubTypeCMPSubStyleT2L_NoPOUS()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
		entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;
		entryHeader.ZG_POUSVersion = 0;

		testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
		{
			MessageSubType = DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration,
			ShouldSend = true
		};
		var typeList = testItem.MessageTypesList;
		CombineAssertions(() =>
		{
			AssertEquals("Message Type List is CodeDescriptionPairList", typeof(CodeDescriptionPairList), typeList.GetType());
			AssertContainsExactElementsInAnyOrder("Message Type List for Import NoPOUS and entry instruction substyle is T2L and messagesubtype is CMP contains T2A and T2C", new ZString[] { "T2A", "T2C" }, typeList.GetAllCodesZString());
		});
	}

	public void TestMessageTypeList_ClearedForT2lAndT2lPous()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
		entryHeader.CH_EntryStatus = EntryStatusCodes.Cleared;
		entryHeader.ZG_POUSVersion = 0;

		CombineAssertions(() =>
		{
			testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
			{
				MessageSubType = DeclarationMessageSubTypeList.Codes.Amendment,
			};
			var typeList = testItem.MessageTypesList;
			AssertEquals("Message Type List for T2L Request and entry instruction substyle is T2l, contains ADM", DeclarationMessageTypeList.Codes.T2lExpeditionAmendment, testItem.MessageType);

			entryHeader.ZG_POUSVersion = 1;
			Factory.Save();
			testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
			{
				MessageSubType = DeclarationMessageSubTypeList.Codes.Amendment,
			};
			typeList = testItem.MessageTypesList;
			AssertEquals("Message Type List for T2LPOUS Request and entry instruction substyle is T2l, is empty", ZString.Empty, testItem.MessageType);
		});
	}

	public void TestMessageSubTypeList_ClearedForT2lAndT2lPous()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
		entryHeader.CH_EntryStatus = EntryStatusCodes.Cleared;
		entryHeader.ZG_POUSVersion = 0;

		var typeList = testItem.MessageSubTypesList;
		CombineAssertions(() =>
		{
			AssertContainsExactElementsInAnyOrder("Message SubType List for T2L Request and entry instruction substyle is T2l, contains AMD and CAN", new ZString[] { "AMD", "CAN" }, typeList.GetAllCodesZString());

			entryHeader.ZG_POUSVersion = 1;
			typeList = testItem.MessageSubTypesList;
			AssertContainsExactElementsInAnyOrder("Message SubType List for T2LPOUS Request and entry instruction substyle is T2l, no contains elements", System.Array.Empty<ZString>(), typeList.GetAllCodesZString());
		});
	}

	public void TestMessageTypeListForImportSubTypeCMPSubStyleT2L_POUS()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
		entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;
		entryHeader.ZG_POUSVersion = 1;

		testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
		{
			MessageSubType = DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration,
			ShouldSend = true
		};
		var typeList = testItem.MessageTypesList;
		CombineAssertions(() =>
		{
			AssertEquals("Message Type List is CodeDescriptionPairList", typeof(CodeDescriptionPairList), typeList.GetType());
			AssertEquals("Message Type List for Import POUS and entry instruction substyle is T2L and messagesubtype is CMP is empty", 0, typeList.Count);
		});
	}

	public void TestMessageTypeList_Export_UCC6_SubStyleABC_NotPDA()
	{
		var expectedListMessageTypes = new ZString[] { DeclarationMessageTypeList.Codes.ExportUcc6, DeclarationMessageTypeList.Codes.ExportPreDeclaration };
		entryHeader.ZG_UCC6Version = 1;

		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
		testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
		{
			MessageSubType = DeclarationMessageSubTypeList.Codes.OriginalDeclaration,
			ShouldSend = true
		};
		var typeList = testItem.MessageTypesList;

		CombineAssertions(() =>
		{
			AssertEquals("Message Type List is CodeDescriptionPairList", typeof(CodeDescriptionPairList), typeList.GetType());
			AssertContainsExactElementsInAnyOrder("Message Type List for Export UCC6 and entry instruction substyle is A contains EDP and PDE", expectedListMessageTypes, typeList.GetAllCodesZString());

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
			typeList = testItem.MessageTypesList;
			AssertContainsExactElementsInAnyOrder("Message Type List for Export UCC6 and entry instruction substyle is B contains EDP and PDE", expectedListMessageTypes, typeList.GetAllCodesZString());

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.C;
			typeList = testItem.MessageTypesList;
			AssertContainsExactElementsInAnyOrder("Message Type List for Export UCC6 and entry instruction substyle is C contains EDP and PDE", expectedListMessageTypes, typeList.GetAllCodesZString());

			entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;
			typeList = testItem.MessageTypesList;
			AssertEquals("Message Type List for Export UCC6 and entry instruction substyle is C but entry status is PDA", 0, typeList.Count);
		});
	}

	public void TestMessageSubTypeList_Export_CLP()
	{
		entryHeader.ZG_UCC6Version = 0;

		entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.C;
		Factory.Save();
		AssertMessageSubTypeList("Export, entry status CLP and entry instruction C", new ZString[] { "AMD", "CMP" });

		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
		Factory.Save();
		AssertMessageSubTypeList("Export, entry status CLP and entry instruction B", new ZString[] { "AMD", "CMP" });
	}

	public void TestMessageSubTypeList_Import_T2L_NoPous()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
		entryHeader.CH_EntryStatus = EntryStatusCodes.Cleared;
		entryHeader.ZG_POUSVersion = 0;
		Factory.Save();
		AssertMessageSubTypeList("Import, no T2LPous, entry status CLR and entry instruction T2L", new ZString[] { "AMD", "CMP" });

		entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
		Factory.Save();
		AssertMessageSubTypeList("Import, no T2LPous, entry status CLP and entry instruction T2L", new ZString[] { "AMD", "CMP" });
	}

	public void TestMessageSubTypeList_Import_T2L_Pous()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
		entryHeader.CH_EntryStatus = EntryStatusCodes.Cleared;
		entryHeader.ZG_POUSVersion = 1;
		Factory.Save();
		AssertMessageSubTypeList("Import, T2LPous, entry status CLR and entry instruction T2L", System.Array.Empty<ZString>());

		entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
		Factory.Save();
		AssertMessageSubTypeList("Import, T2LPous, entry status CLP and entry instruction T2L", new ZString[] { "AMD", "CMP" });
	}

	public void TestMessageSubTypeList_Import_NotH2_PDA_NoUcc6()
	{
		entryHeader.ZG_UCC6Version = 0;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Z;
		Factory.Save();
		AssertMessageSubTypeList("Import, is not Ucc6, entry status PDA and entry instruction in (A, B, C, Y, Z)", new ZString[] { "AMD", "CAN" });
	}

	public void TestMessageSubTypeList_Import_H2_PDA()
	{
		entryHeader.ZG_UCC6Version = 0;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
		entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;
		Factory.Save();
		AssertMessageSubTypeList("Import, is not Ucc6, entry status PDA and entry instruction declaration type is H2", new ZString[] { "AMD", "CAN" });
	}

	public void TestMessageSubTypeList_Import_PDI_NoUcc6()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Z;
		entryHeader.ZG_UCC6Version = 0;
		Factory.Save();
		AssertMessageSubTypeList("Import, is not Ucc6, entry status PDA and entry instruction in (A, B, C, Y, Z)", new ZString[] { "AMD", "CAN", "ORG" });
	}

	public void TestMessageSubTypeList_EmptyEntryInstruction()
	{
		entryInstruction.CEI_SubStyle = ZString.Empty;
		Factory.Save();
		AssertMessageSubTypeList(" when entry instruction substyle is empty", System.Array.Empty<ZString>());
	}

	public void TestMessageSubTypeList_Export_EXS_CLR()
	{
		entryHeader.CH_EntryStatus = EntryStatusCodes.Cleared;
		entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
		Factory.Save();
		AssertMessageSubTypeList("Export, entry status CLR and entry instruction EXS", new ZString[] { "AMD", "CAN" });
	}

	public void TestMessageSubTypeList_Export_Ucc6_CLP_EntryInstructionC()
	{
		entryHeader.ZG_UCC6Version = 1;

		entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.C;
		Factory.Save();
		AssertMessageSubTypeList("Export, entry status CLP, entry instruction C and Ucc6Version > 0", new ZString[] { "AMD", "CAN", "CMP" });
	}

	public void TestMessageSubTypeList_Export_Ucc6_CLP_EntryInstructionB()
	{
		entryHeader.ZG_UCC6Version = 1;

		entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
		Factory.Save();
		AssertMessageSubTypeList("Export, entry status CLP, entry instruction B and Ucc6Version > 0", new ZString[] { "AMD", "CAN", "CMP" });
	}

	public void TestMessageSubTypeList_Export_Ucc6_CLP_NotEntryInstructionC()
	{
		entryHeader.ZG_UCC6Version = 1;

		entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
		Factory.Save();
		AssertMessageSubTypeList("Export, entry status CLP, entry instruction not C and Ucc6Version > 0", new ZString[] { "AMD", "CAN" });
	}

	public void TestMessageSubTypeList_Export_Ucc6_PDA_EntryInstructionABC()
	{
		entryHeader.ZG_UCC6Version = 1;

		entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.C;
		Factory.Save();
		AssertMessageSubTypeList("Export, entry status PDA, entry instruction in (A, B, C) and Ucc6Version > 0", new ZString[] { "AMD", "CAN", "ORG" });
	}

	public void TestMessageSubTypeList_Export_Ucc6_PDA_NotEntryInstructionABC()
	{
		entryHeader.ZG_UCC6Version = 1;

		entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Z;
		Factory.Save();
		AssertMessageSubTypeList("Export, entry status PDA, entry instruction not in (A, B, C) and Ucc6Version > 0", new ZString[] { "AMD", "CAN" });
	}

	public void TestMessageSubTypeList_Export_Ucc6_CLR()
	{
		entryHeader.ZG_UCC6Version = 1;

		entryHeader.CH_EntryStatus = EntryStatusCodes.Cleared;
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
		Factory.Save();
		AssertMessageSubTypeList("Export, entry status CLR, entry instruction in any entry instruction and Ucc6Version > 0", new ZString[] { "AMD", "CAN" });
	}

	void AssertMessageSubTypeList(ZString declarationType, ZString[] expectedCodes)
	{
		CombineAssertions(() =>
		{
			var typeList = testItem.MessageSubTypesList;
			AssertEquals("Message SubType List for " + declarationType + " is CodeDescriptionPairList", typeof(CodeDescriptionPairList), typeList.GetType());
			AssertContainsExactElementsInAnyOrder("Message SubType List for " + declarationType + " contains", expectedCodes, typeList.GetAllCodesZString());
		});
	}

	public void TestMessageSubTypeImportAndExport()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Message SubType is ORG when no incoming rejected response", DeclarationMessageSubTypeList.Codes.OriginalDeclaration, testItem.MessageSubType);

			var outgoingMessage = entryHeader.Messages.AddNew();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = "MT1";
			outgoingMessage.EM_MessageSubType = "MST";

			var incomingMessage = entryHeader.Messages.AddNew();
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageSubType = DeclarationMessageSubTypeList.Codes.AcceptedResponse;

			testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
			AssertEquals("Message SubType is ORG when no incoming rejected response", DeclarationMessageSubTypeList.Codes.OriginalDeclaration, testItem.MessageSubType);

			incomingMessage.EM_MessageSubType = DeclarationMessageSubTypeList.Codes.RejectedResponse;
			testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
			AssertEquals("Message SubType is MST when incoming rejected response", "MST", testItem.MessageSubType);
			entryHeader.Messages.RemoveAndDeleteAll();
		});
	}

	public void TestMessageSubTypeImport()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
		AssertEquals("Message SubType is ORG when EntryStatus is empty", DeclarationMessageSubTypeList.Codes.OriginalDeclaration, testItem.MessageSubType);
	}

	public void TestMessageSubTypeImportEntryStatusERR()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryHeader.CH_EntryStatus = EntryStatusCodes.Error;
		testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
		AssertEquals("Message SubType is ORG when EntryStatus is error", DeclarationMessageSubTypeList.Codes.OriginalDeclaration, testItem.MessageSubType);
	}

	public void TestMessageSubTypeImportEntryStatusFFT()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryHeader.CH_EntryStatus = Common.EU.MessageStatusList.Codes.FailedFromTransmission;
		testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
		AssertEquals("Message SubType is ORG when EntryStatus is failed from transmission", DeclarationMessageSubTypeList.Codes.OriginalDeclaration, testItem.MessageSubType);
	}

	public void TestMessageSubTypeImportEntryStatusCLR()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryHeader.CH_EntryStatus = EntryStatusCodes.Cleared;
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
		testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
		AssertEquals("Message SubType is empty when EntryStatus is CLR and entry instruction substyle is T2L", ZString.Empty, testItem.MessageSubType);
	}

	public void TestMessageSubTypeImportEntryStatusCLP()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
		testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
		AssertEquals("Message SubType is empty when EntryStatus is CLP and entry instruction substyle is T2L", ZString.Empty, testItem.MessageSubType);
	}

	public void TestMessageSubTypeImportEntryStatusCDA()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			entryHeader.CH_EntryStatus = EntryStatusCodes.CustomsDeclarationAccepted;
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
			AssertEquals("Message SubType is CMP for import when EntryStatus is CDA and entry instruction substyle is in (A,B,C,Y,Z)", DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration, testItem.MessageSubType);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
			testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
			AssertEquals("Message SubType is empty for import when EntryStatus is CDA but entry instruction substyle is not in (A,B,C,Y,Z)", ZString.Empty, testItem.MessageSubType);

			entryHeader.ZG_POUSVersion = 1;
			testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
			AssertEquals("Message SubType is CMP for import when EntryStatus is CDA and entry instruction substyle is T2L or T2C and POUSVersion > 0", DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration, testItem.MessageSubType);
		});
	}

	public void TestMessageSubTypeImportEntryStatusCLPSubStyleAB()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
			testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
			AssertEquals("Message SubType is CMP for import when EntryStatus is CLP and entry instruction substyle is in (B,C,Z)", DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration, testItem.MessageSubType);
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
			AssertEquals("Message SubType is empty for import when EntryStatus is CLP but entry instruction substyle is not in (B,C,Z)", ZString.Empty, testItem.MessageSubType);
		});
	}

	public void TestMessageSubTypeImportEntryStatusCAN()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryHeader.CH_EntryStatus = EntryStatusCodes.Cancelled;
		testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
		AssertEquals("Message SubType is empty for import when EntryStatus is not CDA or CLP", ZString.Empty, testItem.MessageSubType);
	}

	public void TestMessageSubTypeImportEntryStatusCDP()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingDocuments;
		entryInstruction.CEI_SubStyle = ZString.Empty;
		testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
		AssertEquals("Message SubType is CMP for import when EntryStatus is CDP", "CMP", testItem.MessageSubType);
	}

	public void TestMessageSubTypeImportH2EntryStatusPDA()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
		entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;
		testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
		AssertEquals("Message SubType is Empty for import when EntryStatus is PDA and entry instruction declaration type is H2", ZString.Empty, testItem.MessageSubType);
	}

	public void TestMessageSubTypeImportEntryStatusPDIEntryInstructionABC_UCC6()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
			entryHeader.ZG_UCC6Version = 1;
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
			AssertEquals("Message SubType is AMD for import when EntryStatus is PDI and entry instruction substyle is A", DeclarationMessageSubTypeList.Codes.Amendment, testItem.MessageSubType);
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Z;
			testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
			AssertEquals("Message SubType is empty for import when EntryStatus is PDI and entry instruction substyle is Z", ZString.Empty, testItem.MessageSubType);
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
			testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
			AssertEquals("Message SubType is AMD for import when EntryStatus is PDI and entry instruction substyle is B", DeclarationMessageSubTypeList.Codes.Amendment, testItem.MessageSubType);
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.C;
			testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
			AssertEquals("Message SubType is AMD for import when EntryStatus is PDI and entry instruction substyle is C", DeclarationMessageSubTypeList.Codes.Amendment, testItem.MessageSubType);
		});
	}

	public void TestMessageSubTypeExportEntryStatusEmpty()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryHeader.CH_EntryStatus = ZString.Empty;
		testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
		AssertEquals("Message SubType is ORG when EntryStatus is empty", DeclarationMessageSubTypeList.Codes.OriginalDeclaration, testItem.MessageSubType);
	}

	public void TestMessageSubTypeExportEntryStatusERR()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryHeader.CH_EntryStatus = EntryStatusCodes.Error;
		testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
		AssertEquals("Message SubType is ORG when EntryStatus is error", DeclarationMessageSubTypeList.Codes.OriginalDeclaration, testItem.MessageSubType);
	}

	public void TestMessageSubTypeExportEntryStatusFFT()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryHeader.CH_EntryStatus = Common.EU.MessageStatusList.Codes.FailedFromTransmission;
		testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
		AssertEquals("Message SubType is ORG when EntryStatus is failed from transmission", DeclarationMessageSubTypeList.Codes.OriginalDeclaration, testItem.MessageSubType);
	}

	public void TestMessageSubTypeExportEntryStatusCLP_T2L()
	{
		entryHeader.ZG_UCC6Version = 0;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
		testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
		AssertEquals("Message SubType is CMP when EntryStatus is CLP and entry instruction substyle is T2L", DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration, testItem.MessageSubType);
	}

	public void TestMessageSubTypeExportEntryStatusCLP_UCC6EntryInstructionC()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryHeader.ZG_UCC6Version = 1;
		entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.C;
		testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
		AssertEquals("Message SubType is empty when Export, EntryStatus is CLP, entry instruction substyle is C and UCC6Version > 0", ZString.Empty, testItem.MessageSubType);
	}

	public void TestMessageSubTypeExportEntryStatusCLP_UCC6NoEntryInstructionC()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryHeader.ZG_UCC6Version = 1;
		entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
		testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
		AssertEquals("Message SubType is empty when Export, EntryStatus is CLP, entry instruction substyle is not C and UCC6Version > 0", ZString.Empty, testItem.MessageSubType);
	}

	public void TestMessageSubTypeExportEntryStatusCLR_UCC6()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryHeader.ZG_UCC6Version = 1;
		entryHeader.CH_EntryStatus = EntryStatusCodes.Cleared;
		testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
		AssertEquals("Message SubType is empty when Export, EntryStatus is CLR and UCC6Version > 0", ZString.Empty, testItem.MessageSubType);
	}

	public void TestMessageSubTypeExportEntryStatusPDA_UCC6()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			entryHeader.ZG_UCC6Version = 1;
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Y;
			entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;
			testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
			AssertEquals("Message SubType is empty when Export, EntryStatus is PDA and UCC6Version > 0 but entry instruction substyle is not in (A,B,C)", ZString.Empty, testItem.MessageSubType);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
			AssertEquals("Message SubType is empty when Export, EntryStatus is PDA, UCC6Version > 0 and entry instruction substyle is in (A,B,C)", ZString.Empty, testItem.MessageSubType);
		});
	}

	public void TestMessageSubTypeExportEntryStatusCDA_UCC6()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryHeader.ZG_UCC6Version = 1;
		entryHeader.CH_EntryStatus = EntryStatusCodes.CustomsDeclarationAccepted;
		testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
		AssertEquals("Message SubType is CMP when EntryStatus is CDA and UCC6Version > 0", DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration, testItem.MessageSubType);
	}

	public void TestMessageSubTypeExportEntryStatusCDA_POUS()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryHeader.ZG_POUSVersion = 1;
		entryHeader.CH_EntryStatus = EntryStatusCodes.CustomsDeclarationAccepted;
		testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
		AssertEquals("Message SubType is CMP when EntryStatus is CDA and POUSVersion > 0", DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration, testItem.MessageSubType);
	}

	public void TestMessageSubTypeExportEntryStatusCLR()
	{
		entryHeader.ZG_UCC6Version = 0;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryHeader.CH_EntryStatus = EntryStatusCodes.Cleared;
		testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
		AssertEquals("Message SubType is AMD when EntryStatus is CLR and entry instruction substyle is T2L", DeclarationMessageSubTypeList.Codes.Amendment, testItem.MessageSubType);
	}

	public void TestMessageSubTypeExportEntryStatusPDA()
	{
		entryHeader.ZG_UCC6Version = 0;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
		testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
		AssertEquals("Message SubType is AMD for export when EntryStatus is PDA (or CLR or CLP) and entry instruction substyle is in (A,B,C,Z)", DeclarationMessageSubTypeList.Codes.Amendment, testItem.MessageSubType);
	}

	public void TestMessageSubTypeExportEntryStatusCLRSubStyleBY()
	{
		entryHeader.ZG_UCC6Version = 0;
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			entryHeader.CH_EntryStatus = EntryStatusCodes.Cleared;
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
			testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
			AssertEquals("Message SubType is AMD for export when EntryStatus is PDA or CLR and entry instruction substyle is in (A,B,C,Z)", DeclarationMessageSubTypeList.Codes.Amendment, testItem.MessageSubType);
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Y;
			testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
			AssertEquals("Message SubType is empty for export when EntryStatus is PDA or CLR but entry instruction substyle is not in (A,B,C,Z)", ZString.Empty, testItem.MessageSubType);
		});
	}

	public void TestMessageSubTypeExportEntryStatusCLPSubStyleZX()
	{
		entryHeader.ZG_UCC6Version = 0;
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Z;
			testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
			AssertEquals("Message SubType is AMD for export when EntryStatus is CLP (or CLR or PDA) and entry instruction substyle is in (A,B,C,Z)", DeclarationMessageSubTypeList.Codes.Amendment, testItem.MessageSubType);
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.X;
			testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
			AssertEquals("Message SubType is empty for export when EntryStatus is PDA or CLR but entry instruction substyle is not in (A,B,C,Z) and not T2L", ZString.Empty, testItem.MessageSubType);
		});
	}

	public void TestMessageSubTypeExportEntryStatusCAN()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryHeader.CH_EntryStatus = EntryStatusCodes.Cancelled;
		testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
		AssertEquals("Message SubType is empty for export when EntryStatus is not PDA, CLR or CLP", ZString.Empty, testItem.MessageSubType);
	}

	public void TestMessageSubTypeExport_EXS()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
		testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
		AssertEquals("Message SubType is ORG when EntryStatus is empty", DeclarationMessageSubTypeList.Codes.OriginalDeclaration, testItem.MessageSubType);
	}

	public void TestMessageSubTypeExport_EXS_EntryStatusCLR()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryHeader.CH_EntryStatus = EntryStatusCodes.Cleared;
		entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
		testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);

		AssertEquals("Message SubType is empty when EntryStatus is CLR and entry instruction substyle is EXS", ZString.Empty, testItem.MessageSubType);
	}

	public void TestGetDefaultMessageTypeImportAndExportAcceptedResponse()
	{
		entryHeader.ZG_UCC6Version = 0;
		CombineAssertions(() =>
		{
			testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
			AssertEquals("MessageType is EXP when no incoming rejected response", DeclarationMessageTypeList.Codes.Export, testItem.MessageType);

			var outgoingMessage = entryHeader.Messages.AddNew();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = "MT1";
			outgoingMessage.EM_MessageSubType = "MST";

			var incomingMessage = entryHeader.Messages.AddNew();
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageSubType = DeclarationMessageSubTypeList.Codes.AcceptedResponse;

			testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
			AssertEquals("MessageType is EXP when only incoming response is accepted", DeclarationMessageTypeList.Codes.Export, testItem.MessageType);

			entryHeader.Messages.RemoveAndDeleteAll();
		});
	}

	public void TestGetDefaultMessageTypeImportAndExportRejectedResponse()
	{
		CombineAssertions(() =>
		{
			var outgoingMessage = entryHeader.Messages.AddNew();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = "MT1";
			outgoingMessage.EM_MessageSubType = "MST";

			var incomingMessage = entryHeader.Messages.AddNew();
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageSubType = DeclarationMessageSubTypeList.Codes.AcceptedResponse;

			incomingMessage.EM_MessageSubType = DeclarationMessageSubTypeList.Codes.RejectedResponse;
			testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
			AssertEquals("MessageType is MT1 when incoming rejected response", "MT1", testItem.MessageType);

			entryHeader.Messages.RemoveAndDeleteAll();
		});
	}

	public void TestGetDefaultMessageTypeImportAndExportMessageTypeMSC()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
		testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
		AssertEquals("Message Type is empty for not import or export declaration", ZString.Empty, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeImportMessageSubTypeEmpty()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		CombineAssertions(() =>
		{
			TestItemSetEntryInstructionAndMesageSubStyle(messageSubStyle: ZString.Empty);
			AssertEquals("Message Type is empty when MessageSubType is empty", ZString.Empty, testItem.MessageType);
		});
	}

	public void TestGetDefaultMessageTypeImportMessageSubTypeORGSubStyleT2L_NoPOUS()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryHeader.ZG_POUSVersion = 0;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.T2L);
		AssertEquals("Message Type is T2R when entry instruction substyle is T2L and messagesubtype is ORG", DeclarationMessageTypeList.Codes.T2lReception, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeImportMessageSubTypeORGSubStyleT2L_POUS()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryHeader.ZG_POUSVersion = 1;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.T2L);
		AssertEquals("Message Type is T2R when entry instruction substyle is T2L and messagesubtype is ORG", DeclarationMessageTypeList.Codes.T2lReception, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeImportMessageSubTypeORGSubStyleT2L_POUS2()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryHeader.ZG_POUSVersion = 2;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.T2L);
		AssertEquals("Message Type is T2I when entry instruction substyle is T2L and messagesubtype is ORG", DeclarationMessageTypeList.Codes.T2lReceptionPous, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeImportMessageSubTypeAMDSubStyleT2L()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.T2L, DeclarationMessageSubTypeList.Codes.Amendment);
		AssertEquals("Message Type is TRM when entry instruction substyle is T2L and messagesubtype is AMD", DeclarationMessageTypeList.Codes.T2lReceptionAmendment, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeImportMessageSubTypeCMPSubStyleT2L_NoPOUS()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryHeader.ZG_POUSVersion = 0;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.T2L, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);
		AssertEquals("Message Type is empty when entry instruction substyle is T2L and messagesubtype is not ORG or AMD", ZString.Empty, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeImportMessageSubTypeORGSubStyleT2CNoPOUS()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.T2C);
		AssertEquals("Message Type is T2C when entry instruction substyle is T2C and messagesubtype is ORG", DeclarationMessageTypeList.Codes.T2lClearance, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeImportMessageSubTypeORGSubStyleT2CPOUS()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryHeader.ZG_POUSVersion = 1;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.T2C);
		AssertEquals("Message Type is T2J when entry instruction substyle is T2C and messagesubtype is ORG", DeclarationMessageTypeList.Codes.T2lPresentationPous, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeImportMessageSubTypeAMDEntryStatusPDISubStyleEmpty()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
		TestItemSetEntryInstructionAndMesageSubStyle(ZString.Empty, DeclarationMessageSubTypeList.Codes.Amendment);
		AssertEquals("Message Type is PDI when entry status is PDI and messagesubtype is AMD", DeclarationMessageTypeList.Codes.ImportIncompletePreDeclaration, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeImportMessageSubTypeORGEntryStatusPDISubStyleB()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.B);
		AssertEquals("Message Type is PDC when entry status is PDI, entry instruction substyle is in (A,B,Y,Z) and messagesubtype is ORG", DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeImportMessageSubTypeORGEntryStatusPDISubStyleC()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.C);
		AssertEquals("Message Type is PDS when entry status is PDI, entry instruction substyle is C and messagesubtype is ORG", DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeImportMessageSubTypeAMDEntryStatusPDASubStyleB()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.B, DeclarationMessageSubTypeList.Codes.Amendment);
		AssertEquals("Message Type is empty when entry status is PDA, entry instruction substyle is in (A,B,Y,Z) and messagesubtype is AMD", ZString.Empty, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeImportMessageSubTypeAMDEntryStatusPDASubStyleC()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.C, DeclarationMessageSubTypeList.Codes.Amendment);
		AssertEquals("Message Type is PDS when entry status is PDA, entry instruction substyle is C and messagesubtype is AMD", DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeImportMessageSubTypeORGEntryStatusPDASubStyleC()
	{
		entryHeader.ZG_UCC6Version = 0;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.C);
		AssertEquals("Message Type is empty when is not Ucc6, entry status is PDA, entry instruction substyle is in (A,B,C,Y,Z) and messagesubtype is not AMD", ZString.Empty, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeImportMessageSubTypeAMDEntryStatusPDASubStyleX()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.X, DeclarationMessageSubTypeList.Codes.Amendment);
		AssertEquals("Message Type is empty when entry status is PDA, entry instruction substyle is not in (A,B,C,Y,Z) and messagesubtype is AMD", ZString.Empty, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeImportMessageSubTypeCMPEntryStatusCLPSubStyleC()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.C, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);
		AssertEquals("Message Type is PDC when entry status is CLP, entry instruction substyle is C and messagesubtype is CMP", DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeImportNotH2MessageSubTypeCMPEntryStatusCLPSubStyleB()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.B, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);
		AssertEquals("Message Type is DJP when entry status is CLP, entry instruction substyle is B and messagesubtype is CMP", DeclarationMessageTypeList.Codes.PendingSupportingDocuments, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeImportH2MessageSubTypeCMPEntryStatusCLPSubStyleB()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
		entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.B, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);
		AssertEquals("Message Type is DVX when entry status is CLP, entry instruction substyle is B, entry isntruction declaration type is H2 and messagesubtype is CMP", DeclarationMessageTypeList.Codes.TypeXDvdH2, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeImportMessageSubTypeCMPEntryStatusCLPSubStyleZ()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.Z, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);
		AssertEquals("Message Type is DJC when entry status is CLP, entry instruction substyle is in (B,Z) and messagesubtype is CMP", DeclarationMessageTypeList.Codes.PendingSupportingDocuments, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeImportMessageSubTypeAMDEntryStatusCLPSubStyleZ()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.Z, DeclarationMessageSubTypeList.Codes.Amendment);
		AssertEquals("Message Type is Empty when entry status is CLP, entry instruction substyle is in (B,Z) and messagesubtype is not CMP", ZString.Empty, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeImportMessageSubTypeCMPEntryStatusCLPSubStyleX()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.X, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);
		AssertEquals("Message Type is empty when entry status is CLP, entry instruction substyle is not in (B,C,Z) and messagesubtype is CMP", ZString.Empty, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeImportMessageSubTypeCMPEntryStatusCDP()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingDocuments;
		TestItemSetEntryInstructionAndMesageSubStyle(messageSubStyle: DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);
		AssertEquals("Message Type is C44 when entry status is CDP, and messagesubtype is CMP", DeclarationMessageTypeList.Codes.Box44Documents, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeImportMessageSubTypeORGEntryStatusCDA()
	{
		entryHeader.ZG_UCC6Version = 0;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryHeader.CH_EntryStatus = EntryStatusCodes.CustomsDeclarationAccepted;
		TestItemSetEntryInstructionAndMesageSubStyle();
		AssertEquals("Message Type is empty when is not Ucc6, entry status is CDA, and messagesubtype is not CMP", ZString.Empty, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeImportNotH2MessageSubTypeCAN()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		TestItemSetEntryInstructionAndMesageSubStyle(messageSubStyle: DeclarationMessageSubTypeList.Codes.Cancellation);
		AssertEquals("Message Type is PCN when messagesubtype is CAN", DeclarationMessageTypeList.Codes.ImportPreDeclarationCancellation, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeImportH2MessageSubTypeCAN()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		TestItemSetEntryInstructionAndMesageSubStyle(messageSubStyle: DeclarationMessageSubTypeList.Codes.Cancellation, entryInstructionStyle: IMPDeclarationTypeList.Codes.H2);
		AssertEquals("Message Type is DVC when messagesubtype is CAN and entry instruction declaration type is H2", DeclarationMessageTypeList.Codes.DvdH2Cancellation, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeImportMessageSubTypeORGSubStyleBDeclTypeH2()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.B, entryInstructionStyle: IMPDeclarationTypeList.Codes.H2);
		AssertEquals("Message Type is DVD when entry status is empty, entry instruction substyle is B, entry isntruction declaration type is H2 and messagesubtype is ORG", DeclarationMessageTypeList.Codes.DvdH2, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeImportMessageSubTypeORGSubStyleADeclTypeH2()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.A, entryInstructionStyle: IMPDeclarationTypeList.Codes.H2);
		AssertEquals("Message Type is DVD when entry status is empty, entry instruction substyle is A entry isntruction declaration type is H2 and messagesubtype is ORG", DeclarationMessageTypeList.Codes.DvdH2, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeImportMessageSubTypeORGSubStyleZDeclTypeH2()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.Z, entryInstructionStyle: IMPDeclarationTypeList.Codes.H2);
		AssertEquals("Message Type is DVD when entry status is empty, entry instruction substyle is Z, entry isntruction declaration type is H2 and messagesubtype is ORG", DeclarationMessageTypeList.Codes.DvdH2, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeImportH2MessageSubTypeAmendmentEntryStatusPDA()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;
		TestItemSetEntryInstructionAndMesageSubStyle(messageSubStyle: DeclarationMessageSubTypeList.Codes.Amendment, entryInstructionStyle: IMPDeclarationTypeList.Codes.H2);
		AssertEquals("Message Type is DVD when entry status is PDA, entry instruction style is H2 and messagesubtype is AMD", DeclarationMessageTypeList.Codes.DvdH2, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeImportSubTypeCMPEntryStatusCDASubStyleT2L_POUS()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryHeader.CH_EntryStatus = EntryStatusCodes.CustomsDeclarationAccepted;
		entryHeader.ZG_POUSVersion = 1;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.T2L, messageSubStyle: DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);
		AssertEquals("Message Type is T2D when entry status is CDA, entry instruction substyle is T2L, messagesubtype is CMP and POUSVersion > 0", DeclarationMessageTypeList.Codes.T2lDocumentationPous, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeImportSubTypeCMPEntryStatusCDASubStyleT2C_POUS()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryHeader.CH_EntryStatus = EntryStatusCodes.CustomsDeclarationAccepted;
		entryHeader.ZG_POUSVersion = 1;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.T2C, messageSubStyle: DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);
		AssertEquals("Message Type is T2D when entry status is CDA, entry instruction substyle is T2C, messagesubtype is CMP and POUSVersion > 0", DeclarationMessageTypeList.Codes.T2lDocumentationPous, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeImportSubTypeORGSubABC_UCC6()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryHeader.ZG_UCC6Version = 1;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.A);
		CombineAssertions(() =>
		{
			AssertEquals("Message Type is IPD when entry instruction substyle is A, messagesubtype is ORG and UCC6Version > 0", DeclarationMessageTypeList.Codes.ImportIncompletePreDeclarationH1, testItem.MessageType);

			TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.B);
			AssertEquals("Message Type is IPD when entry instruction substyle is B, messagesubtype is ORG and UCC6Version > 0", DeclarationMessageTypeList.Codes.ImportIncompletePreDeclarationH1, testItem.MessageType);

			TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.C);
			AssertEquals("Message Type is IPD when entry instruction substyle is C, messagesubtype is ORG and UCC6Version > 0", DeclarationMessageTypeList.Codes.ImportIncompletePreDeclarationH1, testItem.MessageType);
		});
	}

	public void TestGetDefaultMessageTypeImportSubTypeAMDEntryStatusPDISubABC_UCC6()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;
		entryHeader.ZG_UCC6Version = 1;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.A, DeclarationMessageSubTypeList.Codes.Amendment);
		CombineAssertions(() =>
		{
			AssertEquals("Message Type is IPD when entry instruction substyle is A, messagesubtype is AMD and UCC6Version > 0", DeclarationMessageTypeList.Codes.ImportIncompletePreDeclarationH1, testItem.MessageType);

			TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.B, DeclarationMessageSubTypeList.Codes.Amendment);
			AssertEquals("Message Type is IPD when entry instruction substyle is B, messagesubtype is AMD and UCC6Version > 0", DeclarationMessageTypeList.Codes.ImportIncompletePreDeclarationH1, testItem.MessageType);

			TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.C, DeclarationMessageSubTypeList.Codes.Amendment);
			AssertEquals("Message Type is IPD when entry instruction substyle is C, messagesubtype is AMD and UCC6Version > 0", DeclarationMessageTypeList.Codes.ImportIncompletePreDeclarationH1, testItem.MessageType);
		});
	}

	public void TestGetDefaultMessageTypeExportSubTypeEmpty()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		TestItemSetEntryInstructionAndMesageSubStyle(messageSubStyle: ZString.Empty);
		AssertEquals("Message Type is empty when MessageSubType is empty", ZString.Empty, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeExportSubTypeORGSubStyleT2LNoPOUS()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryHeader.ZG_POUSVersion = 0;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.T2L);
		AssertEquals("Message Type is T2E when entry instruction substyle is T2L and messagesubtype is ORG and POUSVersion <= 0", DeclarationMessageTypeList.Codes.T2lExpedition, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeExportSubTypeORGSubStyleT2LPOUS()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryHeader.ZG_POUSVersion = 1;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.T2L);
		AssertEquals("Message Type is T2L when entry instruction substyle is T2L and messagesubtype is ORG and POUSVersion > 0", DeclarationMessageTypeList.Codes.T2lRequestPous, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeExportSubTypeAMDSubStyleT2L()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.T2L, DeclarationMessageSubTypeList.Codes.Amendment);
		AssertEquals("Message Type is TEM when entry instruction substyle is T2L and messagesubtype is AMD", DeclarationMessageTypeList.Codes.T2lExpeditionAmendment, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeExportSubTypeCANSubStyleT2L()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.T2L, DeclarationMessageSubTypeList.Codes.Cancellation);
		AssertEquals("Message Type is empty when entry instruction substyle is T2L and messagesubtype is not ORG, AMD or CMD", ZString.Empty, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeExportSubTypeORGSubStyleT2C()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryHeader.ZG_POUSVersion = 0;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.T2C);
		AssertEquals("Message Type is T2C when entry instruction substyle is T2C and messagesubtype is ORG", DeclarationMessageTypeList.Codes.T2lClearance, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeExportSubtypeORGSubStyleEmpty_NotUCC6()
	{
		entryHeader.ZG_UCC6Version = 0;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		TestItemSetEntryInstructionAndMesageSubStyle(ZString.Empty);
		AssertEquals("Message Type is EXP when entry instruction substyle is not T2L and messagesubtype is ORG", DeclarationMessageTypeList.Codes.Export, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeExportSubTypeORGSubStyleEXS()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		TestItemSetEntryInstructionAndMesageSubStyle(ExsEntrySubStyleList.Codes.EXS);
		AssertEquals("Message Type is Export when entry instruction substyle is EXS and messagesubtype is ORG", DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeExportSubTypeAMDSubStyleEXS()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		TestItemSetEntryInstructionAndMesageSubStyle(ExsEntrySubStyleList.Codes.EXS, DeclarationMessageSubTypeList.Codes.Amendment);
		AssertEquals("Message Type is Export when entry instruction substyle is EXS and messagesubtype is AMD", DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeExportSubTypeCANSubStyleEXS()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		TestItemSetEntryInstructionAndMesageSubStyle(ExsEntrySubStyleList.Codes.EXS, DeclarationMessageSubTypeList.Codes.Cancellation);
		AssertEquals("Message Type is Export when entry instruction substyle is EXS and messagesubtype is CAN", DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeExportSubtypeORGSubY_UCC6()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryHeader.ZG_UCC6Version = 1;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.Y);
		AssertEquals("Message Type is EDP when entry instruction substyle is Y, messagesubtype is ORG and UCC6Version > 0", DeclarationMessageTypeList.Codes.ExportUcc6, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeExportSubtypeORGSubZ_UCC6()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryHeader.ZG_UCC6Version = 1;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.Z);
		AssertEquals("Message Type is EDP when entry instruction substyle is Z, messagesubtype is ORG and UCC6Version > 0", DeclarationMessageTypeList.Codes.ExportUcc6, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeExportSubtypeORGSubABC_UCC6()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryHeader.ZG_UCC6Version = 1;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.A);
		CombineAssertions(() =>
		{
			AssertEquals("Message Type is Empty when entry instruction substyle is A, messagesubtype is ORG and UCC6Version > 0", ZString.Empty, testItem.MessageType);

			TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.B);
			AssertEquals("Message Type is Empty when entry instruction substyle is B, messagesubtype is ORG and UCC6Version > 0", ZString.Empty, testItem.MessageType);

			TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.C);
			AssertEquals("Message Type is Empty when entry instruction substyle is C, messagesubtype is ORG and UCC6Version > 0", ZString.Empty, testItem.MessageType);
		});
	}

	public void TestGetDefaultMessageTypeExportSubtypeORGSubABCEntryStatusPDA_UCC6()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;
		entryHeader.ZG_UCC6Version = 1;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.B);
		AssertEquals("Message Type is EDN when entry status is PDA, entry instruction substyle is (A, B or C), messagesubtype is ORG and UCC6Version > 0", DeclarationMessageTypeList.Codes.ExportNotification, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeExportSubTypeCMPEntryStatusCLPSubStyleB()
	{
		entryHeader.ZG_UCC6Version = 0;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.B, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);
		AssertEquals("Message Type is EXX when entry status is CLP, entry instruction substyle is B and messagesubtype is CMP", DeclarationMessageTypeList.Codes.TypeXExport, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeExportSubTypeCMPEntryStatusCLPSubStyleC_NotUCC6()
	{
		entryHeader.ZG_UCC6Version = 0;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.C, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);
		AssertEquals("Message Type is EXP when entry status is CLP, entry instruction substyle is C and messagesubtype is CMP", DeclarationMessageTypeList.Codes.Export, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeExportSubTypeCMPEntryStatusCLPSubStyleC_UCC6()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
		entryHeader.ZG_UCC6Version = 1;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.C, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);
		AssertEquals("Message Type is EDP when entry status is CLP, entry instruction substyle is C, messagesubtype is CMP and UCC6Version > 0", DeclarationMessageTypeList.Codes.ExportUcc6, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeExportSubTypeCMPEntryStatusCDA_UCC6()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryHeader.CH_EntryStatus = EntryStatusCodes.CustomsDeclarationAccepted;
		entryHeader.ZG_UCC6Version = 1;
		TestItemSetEntryInstructionAndMesageSubStyle(messageSubStyle: DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);
		AssertEquals("Message Type is EDA when entry status is CDA, messagesubtype is CMP and UCC6Version > 0", DeclarationMessageTypeList.Codes.ExportAnnexes, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeExportSubTypeCMPEntryStatusCDASubStyleT2L_POUS()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryHeader.CH_EntryStatus = EntryStatusCodes.CustomsDeclarationAccepted;
		entryHeader.ZG_POUSVersion = 1;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.T2L, messageSubStyle: DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);
		AssertEquals("Message Type is T2D when entry status is CDA, entry instruction substyle is T2L, messagesubtype is CMP and POUSVersion > 0", DeclarationMessageTypeList.Codes.T2lDocumentationPous, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeExportSubTypeCMPEntryStatusCDASubStyleT2C_POUS()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryHeader.CH_EntryStatus = EntryStatusCodes.CustomsDeclarationAccepted;
		entryHeader.ZG_POUSVersion = 1;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.T2C, messageSubStyle: DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);
		AssertEquals("Message Type is T2D when entry status is CDA, entry instruction substyle is T2C, messagesubtype is CMP and POUSVersion > 0", DeclarationMessageTypeList.Codes.T2lDocumentationPous, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeExportSubTypeCMPEntryStatusCLPSubStyleA()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.A, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);
		AssertEquals("Message Type is empty when entry status is CLP, entry instruction substyle is not in (B,C) and messagesubtype is CMP", ZString.Empty, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeExportSubTypeAMDEntryStatusCLPSubStyleC()
	{
		entryHeader.ZG_UCC6Version = 0;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.C, DeclarationMessageSubTypeList.Codes.Amendment);
		AssertEquals("Message Type is EXM when entry status is in (CLP, PDA, CLR), entry instruction substyle is in (A,B,C,Z) and messagesubtype is AMD", DeclarationMessageTypeList.Codes.ExportAmendment, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeExportSubTypeAMDEntryStatusCLP_UCC6()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
		entryHeader.ZG_UCC6Version = 1;
		TestItemSetEntryInstructionAndMesageSubStyle(messageSubStyle: DeclarationMessageSubTypeList.Codes.Amendment);
		AssertEquals("Message Type is EDM when entry status is CLP, messagesubtype is AMD and UCC6Version > 0", DeclarationMessageTypeList.Codes.ExportAmendmentUcc6, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeExportSubTypeAMDEntryStatusCLR_UCC6()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryHeader.CH_EntryStatus = EntryStatusCodes.Cleared;
		entryHeader.ZG_UCC6Version = 1;
		TestItemSetEntryInstructionAndMesageSubStyle(messageSubStyle: DeclarationMessageSubTypeList.Codes.Amendment);
		AssertEquals("Message Type is EDM when entry status is CLR, messagesubtype is AMD and UCC6Version > 0", DeclarationMessageTypeList.Codes.ExportAmendmentUcc6, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeExportSubTypeAMDEntryStatusPDA_UCC6()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;
		entryHeader.ZG_UCC6Version = 1;
		TestItemSetEntryInstructionAndMesageSubStyle(messageSubStyle: DeclarationMessageSubTypeList.Codes.Amendment);
		AssertEquals("Message Type is EDM when entry status is PDA, messagesubtype is AMD and UCC6Version > 0", DeclarationMessageTypeList.Codes.ExportAmendmentUcc6, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeExportSubtypeAMDEntryStatusCANSubStyleC()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryHeader.CH_EntryStatus = EntryStatusCodes.Cancelled;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.C, DeclarationMessageSubTypeList.Codes.Amendment);
		AssertEquals("Message Type is empty when entry status is not in (CLP, PDA, CLR), entry instruction substyle is in (A,B,C,Z) and messagesubtype is AMD", ZString.Empty, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeExportSubTypeAMDEntryStatusCLPSubStyleX()
	{
		entryHeader.ZG_UCC6Version = 0;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.X, DeclarationMessageSubTypeList.Codes.Amendment);
		AssertEquals("Message Type is empty when entry status is in (CLP, PDA, CLR), entry instruction substyle is not in (A,B,C,Z) and messagesubtype is AMD", ZString.Empty, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeExportSubTypeCANEntryStatusCLPSubStyleZ()
	{
		entryHeader.ZG_UCC6Version = 0;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.Z, DeclarationMessageSubTypeList.Codes.Cancellation);
		AssertEquals("Message Type is empty when entry status is in (CLP, PDA, CLR), entry instruction substyle is in (A,B,C,Z) and messagesubtype is not AMD", ZString.Empty, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeExportSubTypeCANntryStatusCLP_UCC6()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
		entryHeader.ZG_UCC6Version = 1;
		TestItemSetEntryInstructionAndMesageSubStyle(messageSubStyle: DeclarationMessageSubTypeList.Codes.Cancellation);
		AssertEquals("Message Type is EDC when entry status is CLP, messagesubtype is CAN and UCC6Version > 0", DeclarationMessageTypeList.Codes.ExportCancellation, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeExportSubTypeCANEntryStatusCLR_UCC6()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryHeader.CH_EntryStatus = EntryStatusCodes.Cleared;
		entryHeader.ZG_UCC6Version = 1;
		TestItemSetEntryInstructionAndMesageSubStyle(messageSubStyle: DeclarationMessageSubTypeList.Codes.Cancellation);
		AssertEquals("Message Type is EDC when entry status is CLR, messagesubtype is CAN and UCC6Version > 0", DeclarationMessageTypeList.Codes.ExportCancellation, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeExportSubTypeCANEntryStatusPDA_UCC6()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;
		entryHeader.ZG_UCC6Version = 1;
		TestItemSetEntryInstructionAndMesageSubStyle(messageSubStyle: DeclarationMessageSubTypeList.Codes.Cancellation);
		AssertEquals("Message Type is EDC when entry status is PDA, messagesubtype is CAN and UCC6Version > 0", DeclarationMessageTypeList.Codes.ExportCancellation, testItem.MessageType);
	}

	public void TestGetDefaultMessageTypeExportSubTypeCMPEntryStatusCLPSubStyleB_UCC6()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
		entryHeader.ZG_UCC6Version = 1;
		TestItemSetEntryInstructionAndMesageSubStyle(EntrySubStyleList.Codes.B, DeclarationMessageSubTypeList.Codes.ComplementaryDeclaration);
		AssertEquals("Message Type is EDX when entry status is CLP, messagesubtype is CMP, messageSubstyle is B and UCC6Version > 0", DeclarationMessageTypeList.Codes.TypeXExportUcc6, testItem.MessageType);
	}

	public void TestSetValidationModes()
	{
		CombineAssertions(() =>
		{
			testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
			{
				MessageType = "PDI"
			};
			AssertEquals("For Message Type PDI, validation mode is PDI", ValidationModes.PDI, entryHeader.ValidationMode);

			testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
			{
				MessageType = "PDS"
			};
			AssertEquals("For Message Type PDS, validation mode is PDS", ValidationModes.PDS, entryHeader.ValidationMode);

			testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
			{
				MessageType = "PDC"
			};
			AssertEquals("For Message Type not in (PDI, PDS), validation mode is None", ValidationModes.None, entryHeader.ValidationMode);
		});
	}

	public void TestActivateByOperatorFlagList()
	{
		CombineAssertions(() =>
		{
			var activateByOperatorFlagList = testItem.ActivateByOperatorFlagList;
			AssertEquals("ActivateByOperatorFlagList is CodeDescriptionPairList", typeof(ActivateByOperatorCodeList), activateByOperatorFlagList.GetType());
			AssertContainsExactElementsInAnyOrder("ActivateByOperatorFlagList contains the correct values", new ZString[] { "0", "1" }, activateByOperatorFlagList.GetAllCodesZString());
		});
	}

	public void TestActivateByOperatorFlagVisible()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);

		CombineAssertions(() =>
		{
			entryHeader.ZG_UCC6Version = 1;

			testItem.MessageType = ZString.Empty;
			AssertEquals("When ZG_UCC6Version > 0 but MessageType is empty, ActivateByOperatorFlag is not visible", false, testItem.ActivateByOperatorFlagVisible);

			testItem.MessageType = "DCP";
			AssertEquals("When ZG_UCC6Version > 0 and MessageType is DCP, ActivateByOperatorFlag is visible", true, testItem.ActivateByOperatorFlagVisible);

			testItem.MessageType = "JPB";
			AssertEquals("When ZG_UCC6Version > 0 but MessageType is JPB (not DCP or DSP), ActivateByOperatorFlag is not visible", false, testItem.ActivateByOperatorFlagVisible);

			testItem.MessageType = "DSP";
			AssertEquals("When ZG_UCC6Version > 0 and MessageType is DSP, ActivateByOperatorFlag is visible", true, testItem.ActivateByOperatorFlagVisible);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			testItem.MessageType = "DCP";
			AssertEquals("When ZG_UCC6Version > 0 and MessageType is DCP but declaration is Export, ActivateByOperatorFlag is not visible", false, testItem.ActivateByOperatorFlagVisible);

			entryHeader.ZG_UCC6Version = 0;

			testItem.MessageType = ZString.Empty;
			AssertEquals("When ZG_UCC6Version = 0 and MessageType is empty (not DCP or DSP), ActivateByOperatorFlag is not visible", false, testItem.ActivateByOperatorFlagVisible);

			testItem.MessageType = "JPB";
			AssertEquals("When ZG_UCC6Version = 0 and MessageType is JPB, ActivateByOperatorFlag is not visible", false, testItem.ActivateByOperatorFlagVisible);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			testItem.MessageType = "DCP";
			AssertEquals("When ZG_UCC6Version = 0 and MessageType is DCP and declaration is Import, ActivateByOperatorFlag is not visible", false, testItem.ActivateByOperatorFlagVisible);
		});
	}

	public void TestDefaultActivateByOperatorFlag()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryInstruction.ZG_ActivateByOperator = false;

		CombineAssertions(() =>
		{
			entryHeader.ZG_UCC6Version = 0;
			var testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
			AssertEquals("When ZG_UCC6Version = 0, ActivateByOperatorFlag is defaulted to empty", ZString.Empty, testItem.ActivateByOperatorFlag);

			entryHeader.ZG_UCC6Version = 1;
			testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
			AssertEquals("When ZG_UCC6Version > 0 and ZG_ActivateByOperator if false, ActivateByOperatorFlag is defaulted to 0", "0", testItem.ActivateByOperatorFlag);

			entryInstruction.ZG_ActivateByOperator = true;
			testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
			AssertEquals("When ZG_UCC6Version > 0 and ZG_ActivateByOperator if true, ActivateByOperatorFlag is defaulted to 1", "1", testItem.ActivateByOperatorFlag);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
			AssertEquals("When ZG_UCC6Version > 0 and ZG_ActivateByOperator if true but declaration is not import, ActivateByOperatorFlag is defaulted to empty", ZString.Empty, testItem.ActivateByOperatorFlag);
		});
	}

	public void TestSecurityFlagList()
	{
		CombineAssertions(() =>
		{
			var securityFlagList = testItem.SecurityFlagList;
			AssertEquals("SecurityFlagList is CodeDescriptionPairList", typeof(SecurityFlagCodeList), securityFlagList.GetType());
			AssertContainsExactElementsInAnyOrder("SecurityFlagList contains the correct values", new ZString[] { "0", "2" }, securityFlagList.GetAllCodesZString());
		});
	}

	public void TestSecurityFlagVisible()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);

		CombineAssertions(() =>
		{
			entryHeader.ZG_UCC6Version = 1;

			testItem.MessageType = ZString.Empty;
			AssertEquals("When ZG_UCC6Version > 0 but MessageType is empty, SecurityFlag is not visible", false, testItem.SecurityFlagVisible);

			testItem.MessageType = "EDP";
			AssertEquals("When ZG_UCC6Version > 0 and MessageType is EDP, SecurityFlag is visible", true, testItem.SecurityFlagVisible);

			testItem.MessageType = "EDC";
			AssertEquals("When ZG_UCC6Version > 0 but MessageType is EDC (not EDP, PDE or EDM), SecurityFlag is not visible", false, testItem.SecurityFlagVisible);

			testItem.MessageType = "PDE";
			AssertEquals("When ZG_UCC6Version > 0 and MessageType is PDE, SecurityFlag is visible", true, testItem.SecurityFlagVisible);

			testItem.MessageType = "EDX";
			AssertEquals("When ZG_UCC6Version > 0 but MessageType is EDX (not EDP, PDE or EDM), SecurityFlag is not visible", false, testItem.SecurityFlagVisible);

			testItem.MessageType = "EDM";
			AssertEquals("When ZG_UCC6Version > 0 and MessageType is EDM, SecurityFlag is visible", true, testItem.SecurityFlagVisible);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			testItem.MessageType = "EDM";
			AssertEquals("When ZG_UCC6Version > 0 and MessageType is EDM but declaration is Import, SecurityFlag is not visible", false, testItem.SecurityFlagVisible);

			entryHeader.ZG_UCC6Version = 0;

			testItem.MessageType = ZString.Empty;
			AssertEquals("When ZG_UCC6Version = 0 and MessageType is empty (not EDP, PDE or EDM), SecurityFlag is not visible", false, testItem.SecurityFlagVisible);

			testItem.MessageType = "EDP";
			AssertEquals("When ZG_UCC6Version = 0 and MessageType is EDP, SecurityFlag is not visible", false, testItem.SecurityFlagVisible);
		});
	}

	public void TestDefaultSecurityFlag()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.ZG_IsSecurityDeclaration = false;

		CombineAssertions(() =>
		{
			entryHeader.ZG_UCC6Version = 0;
			var testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
			AssertEquals("When ZG_UCC6Version = 0, SecurityFlag is defaulted to empty", ZString.Empty, testItem.SecurityFlag);

			entryHeader.ZG_UCC6Version = 1;
			declaration.JE_MessageSubType = "EX";
			testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
			AssertEquals("When ZG_UCC6Version > 0, ZG_IsSecurityDeclaration if false and JE_MessageSubType is not CO, SecurityFlag is defaulted to 0", "0", testItem.SecurityFlag);

			declaration.JE_MessageSubType = "CO";
			testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
			AssertEquals("When ZG_UCC6Version > 0, ZG_IsSecurityDeclaration if false and JE_MessageSubType is CO, SecurityFlag is defaulted to empty", ZString.Empty, testItem.SecurityFlag);

			declaration.ZG_IsSecurityDeclaration = true;
			declaration.JE_MessageSubType = "EX";
			testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
			AssertEquals("When ZG_UCC6Version > 0, ZG_IsSecurityDeclaration if true and JE_MessageSubType is not CO, SecurityFlag is defaulted to 2", "2", testItem.SecurityFlag);

			declaration.JE_MessageSubType = "CO";
			testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
			AssertEquals("When ZG_UCC6Version > 0, ZG_IsSecurityDeclaration if true and JE_MessageSubType is CO, SecurityFlag is defaulted to empty", ZString.Empty, testItem.SecurityFlag);

			declaration.JE_MessageSubType = "EU";
			testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
			AssertEquals("When ZG_UCC6Version > 0, ZG_IsSecurityDeclaration if true and JE_MessageSubType is not CO (EU), SecurityFlag is defaulted to 2", "2", testItem.SecurityFlag);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
			AssertEquals("When ZG_UCC6Version > 0, ZG_IsSecurityDeclaration if true and JE_MessageSubType is not CO (EU) but declaration is not export, SecurityFlag is defaulted to empty", ZString.Empty, testItem.SecurityFlag);
		});
	}

	public void TestRequestDispatchList()
	{
		CombineAssertions(() =>
		{
			var requestDispatchList = testItem.RequestDispatchList;
			AssertEquals("RequestDispatchList is CodeDescriptionPairList", typeof(YesNoList), requestDispatchList.GetType());
			AssertContainsExactElementsInAnyOrder("RequestDispatchList contains the correct values", new ZString[] { "Y", "N" }, requestDispatchList.GetAllCodesZString());
		});
	}

	public void TestRequestDispatchVisible_AES()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);

		CombineAssertions(() =>
		{
			entryHeader.ZG_UCC6Version = 1;
			entryHeader.MovementReferenceNumber = "MRN-TEST";

			var docPivot = Factory.NewWithValidTestData<CusStorageDocPivot>();
			entryHeader.EDocPivotCollection.Add(docPivot);

			testItem.MessageType = ZString.Empty;
			AssertEquals("When ZG_UCC6Version > 0, mrn is declared, entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot but MessageType is empty, RequestDispatch is not visible", false, testItem.RequestDispatchVisible);

			testItem.MessageType = "EDA";
			AssertEquals("When ZG_UCC6Version > 0, mrn is declared, entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot and MessageType is EDA, RequestDispatch is visible", true, testItem.RequestDispatchVisible);

			entryHeader.ZG_UCC6Version = 0;
			AssertEquals("When ZG_UCC6Version = 0, mrn is declared, entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot and MessageType is EDA, RequestDispatch is not visible", false, testItem.RequestDispatchVisible);

			entryHeader.ZG_UCC6Version = 1;
			AssertEquals("When ZG_UCC6Version > 0, mrn is declared, entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot and MessageType is EDA, RequestDispatch is visible", true, testItem.RequestDispatchVisible);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			AssertEquals("When CEI_SubStyle is T2C, ZG_UCC6Version > 0, mrn is declared, entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot and MessageType is EDA, RequestDispatch is not visible", false, testItem.RequestDispatchVisible);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
			AssertEquals("When CEI_SubStyle is B, ZG_UCC6Version > 0, mrn is declared, entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot and MessageType is EDA, RequestDispatch is visible", true, testItem.RequestDispatchVisible);

			entryHeader.MovementReferenceNumber = ZString.Empty;
			AssertEquals("When ZG_UCC6Version = 0, mrn is empty, entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot and MessageType is EDA, RequestDispatch is not visible", false, testItem.RequestDispatchVisible);

			entryHeader.MovementReferenceNumber = "MRN-TEST";
			AssertEquals("When ZG_UCC6Version > 0, mrn is not empty, entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot and MessageType is EDA, RequestDispatch is visible", true, testItem.RequestDispatchVisible);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			testItem.MessageType = "EDA";
			AssertEquals("When ZG_UCC6Version > 0, mrn is declared, there is a sendable annex and MessageType is EDA but declaration is Import, RequestDispatch is not visible", false, testItem.RequestDispatchVisible);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var message = Factory.New<ESEDIMessage>();
			entryHeader.Messages.Add(message);
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Sent;
			var messagePivot = Factory.New<GenPivot>();
			messagePivot.XX_RelationType = GenPivotTypes.CusStorageDocPivotEdiMessage;
			messagePivot.XX_Relation1ID = docPivot.PK;
			messagePivot.XX_Relation1TableCode = docPivot.TablePrefix;
			messagePivot.XX_Relation2ID = message.PK;
			messagePivot.XX_Relation2TableCode = message.TablePrefix;
			Factory.Save();
			AssertEquals("When ZG_UCC6Version > 0, mrn is declared, entryHeader has pivots in EDocPivotCollection and at least one transmit EDIMessage associated to the pivot but the message is awaiting response and MessageType is EDA, RequestDispatch is not visible", false, testItem.RequestDispatchVisible);

			message.EM_Status = EDIMessage.Status.Rejected;
			Factory.Save();
			AssertEquals("When ZG_UCC6Version > 0, mrn is declared, entryHeader has pivots in EDocPivotCollection and at least one transmit EDIMessage associated to the pivot and the message is not awaiting response and MessageType is EDA, RequestDispatch is visible", true, testItem.RequestDispatchVisible);

			message.EM_Status = EDIMessage.Status.Received;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();
			AssertEquals("When ZG_UCC6Version > 0, mrn is declared, entryHeader has pivots in EDocPivotCollection and at least one receive EDIMessage associated to the pivot but the message has status RCV and MessageType is EDA, RequestDispatch is not visible", false, testItem.RequestDispatchVisible);
		});
	}

	public void TestRequestDispatchVisible_T2LPOUS()
	{
		using (RegistryTemporarySetterHelper.SetEST2LMessageVersion(T2LVersionNumberList.Codes.ProofOfUnionStatus))
		{
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			var testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);

			CombineAssertions(() =>
			{
				entryHeader.MovementReferenceNumber = "MRN-TEST";

				var docPivot = Factory.NewWithValidTestData<CusStorageDocPivot>();
				entryHeader.EDocPivotCollection.Add(docPivot);

				testItem.MessageType = ZString.Empty;
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals("When mrn is declared, entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot but MessageType is empty, RequestDispatch is not visible", false, testItem.RequestDispatchVisible);

				testItem.MessageType = "T2D";
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals("When mrn is declared, entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot and MessageType is T2D, RequestDispatch is visible", true, testItem.RequestDispatchVisible);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals("When CEI_SubStyle is A, mrn is declared, entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot and MessageType is T2D, RequestDispatch is not visible", false, testItem.RequestDispatchVisible);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals("When CEI_SubStyle is T2L, mrn is declared, entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot and MessageType is T2D, RequestDispatch is visible", true, testItem.RequestDispatchVisible);

				entryHeader.MovementReferenceNumber = ZString.Empty;
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals("When mrn is empty, entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot and MessageType is T2D, RequestDispatch is not visible", false, testItem.RequestDispatchVisible);

				entryHeader.MovementReferenceNumber = "MRN-TEST";
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals("When mrn is not empty, entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot and MessageType is T2D, RequestDispatch is visible", true, testItem.RequestDispatchVisible);

				var message = Factory.New<ESEDIMessage>();
				entryHeader.Messages.Add(message);
				message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				message.EM_Status = EDIMessage.Status.Sent;
				var messagePivot = Factory.New<GenPivot>();
				messagePivot.XX_RelationType = GenPivotTypes.CusStorageDocPivotEdiMessage;
				messagePivot.XX_Relation1ID = docPivot.PK;
				messagePivot.XX_Relation1TableCode = docPivot.TablePrefix;
				messagePivot.XX_Relation2ID = message.PK;
				messagePivot.XX_Relation2TableCode = message.TablePrefix;
				Factory.Save();
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals("When mrn is declared, entryHeader has pivots in EDocPivotCollection and at least one transmit EDIMessage associated to the pivot but the message is awaiting response and MessageType is T2D, RequestDispatch is not visible", false, testItem.RequestDispatchVisible);

				message.EM_Status = EDIMessage.Status.Rejected;
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				Factory.Save();
				AssertEquals("When mrn is declared, entryHeader has pivots in EDocPivotCollection and at least one transmit EDIMessage associated to the pivot and the message is not awaiting response and MessageType is T2D, RequestDispatch is visible", true, testItem.RequestDispatchVisible);

				message.EM_Status = EDIMessage.Status.Received;
				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				Factory.Save();
				AssertEquals("When mrn is declared, entryHeader has pivots in EDocPivotCollection and at least one receive EDIMessage associated to the pivot but the message has status RCV and MessageType is T2D, RequestDispatch is not visible", false, testItem.RequestDispatchVisible);
			});
		}
	}

	public void TestRequestDispatchVisible_T2LPOUS2()
	{
		using (RegistryTemporarySetterHelper.SetEST2LMessageVersion(T2LVersionNumberList.Codes.RequestJecAndReceptionPous))
		{
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			var testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);

			CombineAssertions(() =>
			{
				entryHeader.MovementReferenceNumber = "MRN-TEST";

				var docPivot = Factory.NewWithValidTestData<CusStorageDocPivot>();
				entryHeader.EDocPivotCollection.Add(docPivot);

				testItem.MessageType = ZString.Empty;
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals("When mrn is declared, entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot but MessageType is empty, RequestDispatch is not visible", false, testItem.RequestDispatchVisible);

				testItem.MessageType = "T2D";
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals("When mrn is declared, entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot and MessageType is T2D, RequestDispatch is visible", true, testItem.RequestDispatchVisible);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals("When CEI_SubStyle is A, mrn is declared, entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot and MessageType is T2D, RequestDispatch is not visible", false, testItem.RequestDispatchVisible);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals("When CEI_SubStyle is T2L, mrn is declared, entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot and MessageType is T2D, RequestDispatch is visible", true, testItem.RequestDispatchVisible);

				entryHeader.MovementReferenceNumber = ZString.Empty;
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals("When mrn is empty, entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot and MessageType is T2D, RequestDispatch is not visible", false, testItem.RequestDispatchVisible);

				entryHeader.MovementReferenceNumber = "MRN-TEST";
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals("When mrn is not empty, entryHeader has pivots in EDocPivotCollection and no EDIMessages associated to the pivot and MessageType is T2D, RequestDispatch is visible", true, testItem.RequestDispatchVisible);

				var message = Factory.New<ESEDIMessage>();
				entryHeader.Messages.Add(message);
				message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				message.EM_Status = EDIMessage.Status.Sent;
				var messagePivot = Factory.New<GenPivot>();
				messagePivot.XX_RelationType = GenPivotTypes.CusStorageDocPivotEdiMessage;
				messagePivot.XX_Relation1ID = docPivot.PK;
				messagePivot.XX_Relation1TableCode = docPivot.TablePrefix;
				messagePivot.XX_Relation2ID = message.PK;
				messagePivot.XX_Relation2TableCode = message.TablePrefix;
				Factory.Save();
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals("When mrn is declared, entryHeader has pivots in EDocPivotCollection and at least one transmit EDIMessage associated to the pivot but the message is awaiting response and MessageType is T2D, RequestDispatch is not visible", false, testItem.RequestDispatchVisible);

				message.EM_Status = EDIMessage.Status.Rejected;
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				Factory.Save();
				AssertEquals("When mrn is declared, entryHeader has pivots in EDocPivotCollection and at least one transmit EDIMessage associated to the pivot and the message is not awaiting response and MessageType is T2D, RequestDispatch is visible", true, testItem.RequestDispatchVisible);

				message.EM_Status = EDIMessage.Status.Received;
				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				Factory.Save();
				AssertEquals("When mrn is declared, entryHeader has pivots in EDocPivotCollection and at least one receive EDIMessage associated to the pivot but the message has status RCV and MessageType is T2D, RequestDispatch is not visible", false, testItem.RequestDispatchVisible);
			});
		}
	}

	public void TestIsImport()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = "EXP";
			AssertEquals("IsImport is false when JE_MessageType is not IMP", false, testItem.IsImport);

			declaration.JE_MessageType = "IMP";
			AssertEquals("IsImport is true when JE_MessageType is IMP", true, testItem.IsImport);
		});
	}

	public void TestIsDeclarationH2()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Without H2 is false", false, testItem.IsDeclarationH2);
			entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
			AssertEquals("With H2 is true", true, testItem.IsDeclarationH2);
		});
	}

	public void TestIsEntryInstructionABZ()
	{
		CombineAssertions(() =>
		{
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			AssertEquals("With EntryInstruction A", true, testItem.IsEntryInstructionABZ);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.X;
			AssertEquals("With EntryInstruction X", false, testItem.IsEntryInstructionABZ);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
			AssertEquals("With EntryInstruction B", true, testItem.IsEntryInstructionABZ);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Y;
			AssertEquals("With EntryInstruction Y", false, testItem.IsEntryInstructionABZ);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Z;
			AssertEquals("With EntryInstruction Z", true, testItem.IsEntryInstructionABZ);
		});
	}

	public void TestIsExport()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = "IMP";
			AssertEquals("IsExport is false when JE_MessageType is not EXP", false, testItem.IsExport);

			declaration.JE_MessageType = "EXP";
			AssertEquals("IsExport is true when JE_MessageType is EXP", true, testItem.IsExport);
		});
	}

	public void TestIsUcc6()
	{
		CombineAssertions(() =>
		{
			entryHeader.ZG_UCC6Version = 0;
			AssertEquals("IsUcc6 is false when Z_UCC6Version = 0", false, testItem.IsUcc6);

			entryHeader.ZG_UCC6Version = 1;
			AssertEquals("IsUcc6 is true when Z_UCC6Version > 0", true, testItem.IsUcc6);
		});
	}

	public void TestIsPOUS()
	{
		CombineAssertions(() =>
		{
			entryHeader.ZG_POUSVersion = 0;
			AssertEquals("IsPOUS is false when ZG_POUSVersion = 0", false, testItem.IsPOUS);

			entryHeader.ZG_POUSVersion = 1;
			AssertEquals("IsPOUS is true when ZG_POUSVersion > 0", true, testItem.IsPOUS);
		});
	}

	public void TestIsEntryInstructionT2LOrT2C()
	{
		CombineAssertions(() =>
		{
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
			AssertEquals("With EntryInstruction T2L", true, testItem.IsEntryInstructionT2LOrT2C);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.X;
			AssertEquals("With EntryInstruction X", false, testItem.IsEntryInstructionT2LOrT2C);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			AssertEquals("With EntryInstruction T2C", true, testItem.IsEntryInstructionT2LOrT2C);
		});
	}

	void TestItemSetEntryInstructionAndMesageSubStyle(string entryInstructionSubStyle = null, string messageSubStyle = DeclarationMessageSubTypeList.Codes.OriginalDeclaration, string entryInstructionStyle = null)
	{
		if (entryInstructionSubStyle != null)
		{
			entryInstruction.CEI_SubStyle = entryInstructionSubStyle;
		}
		if (entryInstructionStyle != null)
		{
			entryInstruction.CEI_Style = entryInstructionStyle;
		}
		testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader)
		{
			MessageSubType = messageSubStyle
		};
	}

	#region Automation

	public void TestMessageSubTypeForAutomation()
	{
		CombineAssertions("For automation purpose, initial message SubType should be defaulted depending on declaration direction, instruction sub style and declaration type.", () =>
		{
			AssertDefaultMessageSubType(JobMessageTypeList.Codes.Export, EntrySubStyleList.Codes.A, ZString.Empty, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
			AssertDefaultMessageSubType(JobMessageTypeList.Codes.Export, EntrySubStyleList.Codes.B, ZString.Empty, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
			AssertDefaultMessageSubType(JobMessageTypeList.Codes.Export, EntrySubStyleList.Codes.C, ZString.Empty, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
			AssertDefaultMessageSubType(JobMessageTypeList.Codes.Export, EntrySubStyleList.Codes.X, ZString.Empty, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
			AssertDefaultMessageSubType(JobMessageTypeList.Codes.Export, EntrySubStyleList.Codes.Y, ZString.Empty, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
			AssertDefaultMessageSubType(JobMessageTypeList.Codes.Export, EntrySubStyleList.Codes.Z, ZString.Empty, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
			AssertDefaultMessageSubType(JobMessageTypeList.Codes.Export, EntrySubStyleList.Codes.T2L, ZString.Empty, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
			AssertDefaultMessageSubType(JobMessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.EXS, ZString.Empty, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
			AssertDefaultMessageSubType(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, ZString.Empty, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
			AssertDefaultMessageSubType(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.B, ZString.Empty, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
			AssertDefaultMessageSubType(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.Y, ZString.Empty, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
			AssertDefaultMessageSubType(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.Z, ZString.Empty, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
			AssertDefaultMessageSubType(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.C, ZString.Empty, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
			AssertDefaultMessageSubType(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.T2C, ZString.Empty, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
			AssertDefaultMessageSubType(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.T2L, ZString.Empty, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
			AssertDefaultMessageSubType(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, IMPDeclarationTypeList.Codes.H2, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
			AssertDefaultMessageSubType(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.B, IMPDeclarationTypeList.Codes.H2, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
			AssertDefaultMessageSubType(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.X, IMPDeclarationTypeList.Codes.H2, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
			AssertDefaultMessageSubType(JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.Z, IMPDeclarationTypeList.Codes.H2, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
		});
	}

	void AssertDefaultMessageSubType(string direction, string instructionSubStyle, string declarationType, string expectedMessageSubType)
	{
		declaration.JE_MessageType = direction;
		entryHeader.CH_EntryStatus = ZString.Empty;
		entryInstruction.CEI_SubStyle = instructionSubStyle;
		entryInstruction.CEI_Style = declarationType;
		var sendingObject = new MessageSending.JobDeclarationMessageSendingObject(entryHeader, true);
		AssertEquals(expectedMessageSubType, sendingObject.MessageSubType);
	}

	public void TestShouldSendDefaultValue()
	{
		CombineAssertions("ShouldSend should be forced to true only in auto send mode when entry has no status, .", () =>
		{
			AssertShouldSend(true, true, false);
			AssertShouldSend(true, false, false);
			AssertShouldSend(false, false, false);
			AssertShouldSend(false, true, true);
		});
	}

	void AssertShouldSend(bool hasEntryStatus, bool isAutoSend, bool expectedShouldSendDefaultValue)
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_EntryStatus = hasEntryStatus ? "ZZZ" : ZString.Empty;
		var sendingObject = new MessageSending.JobDeclarationMessageSendingObject(entryHeader, isAutoSend);
		AssertEquals(expectedShouldSendDefaultValue, sendingObject.ShouldSend);
	}

	public void TestMessageTypeInAutoSendMode()
	{
		CombineAssertions("In auto send mode, UCC6 message type should be defaulted depending on declaration direction, instruction sub style and declaration type as well as entry POUS version.", () =>
		{
			AssertMessageTypeInAutoSendMode(false, false, JobMessageTypeList.Codes.Export, EntrySubStyleList.Codes.A, ZString.Empty, DeclarationMessageTypeList.Codes.ExportUcc6);
			AssertMessageTypeInAutoSendMode(false, false, JobMessageTypeList.Codes.Export, EntrySubStyleList.Codes.B, ZString.Empty, DeclarationMessageTypeList.Codes.ExportUcc6);
			AssertMessageTypeInAutoSendMode(false, false, JobMessageTypeList.Codes.Export, EntrySubStyleList.Codes.C, ZString.Empty, DeclarationMessageTypeList.Codes.ExportUcc6);
			AssertMessageTypeInAutoSendMode(false, false, JobMessageTypeList.Codes.Export, EntrySubStyleList.Codes.X, ZString.Empty, DeclarationMessageTypeList.Codes.ExportUcc6);
			AssertMessageTypeInAutoSendMode(false, false, JobMessageTypeList.Codes.Export, EntrySubStyleList.Codes.Y, ZString.Empty, DeclarationMessageTypeList.Codes.ExportUcc6);
			AssertMessageTypeInAutoSendMode(false, false, JobMessageTypeList.Codes.Export, EntrySubStyleList.Codes.Z, ZString.Empty, DeclarationMessageTypeList.Codes.ExportUcc6);
			AssertMessageTypeInAutoSendMode(true, false, JobMessageTypeList.Codes.Export, EntrySubStyleList.Codes.T2L, ZString.Empty, DeclarationMessageTypeList.Codes.T2lRequestPous);
			AssertMessageTypeInAutoSendMode(false, false, JobMessageTypeList.Codes.Export, EntrySubStyleList.Codes.T2L, ZString.Empty, ZString.Empty);
			AssertMessageTypeInAutoSendMode(false, false, JobMessageTypeList.Codes.Export, ExsEntrySubStyleList.Codes.EXS, ZString.Empty, DeclarationMessageTypeList.Codes.ExitSummaryDeclaration);
			AssertMessageTypeInAutoSendMode(false, false, JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, ZString.Empty, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration);
			AssertMessageTypeInAutoSendMode(false, false, JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.B, ZString.Empty, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration);
			AssertMessageTypeInAutoSendMode(false, false, JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.Y, ZString.Empty, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration);
			AssertMessageTypeInAutoSendMode(false, false, JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.Z, ZString.Empty, DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration);
			AssertMessageTypeInAutoSendMode(false, false, JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.C, ZString.Empty, DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration);
			AssertMessageTypeInAutoSendMode(false, false, JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.T2C, ZString.Empty, ZString.Empty);
			AssertMessageTypeInAutoSendMode(true, false, JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.T2C, ZString.Empty, DeclarationMessageTypeList.Codes.T2lPresentationPous);
			AssertMessageTypeInAutoSendMode(false, false, JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.T2L, ZString.Empty, ZString.Empty);
			AssertMessageTypeInAutoSendMode(true, false, JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.T2L, ZString.Empty, DeclarationMessageTypeList.Codes.T2lReception);
			AssertMessageTypeInAutoSendMode(false, true, JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.T2L, ZString.Empty, DeclarationMessageTypeList.Codes.T2lReceptionPous);
			AssertMessageTypeInAutoSendMode(false, false, JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.A, IMPDeclarationTypeList.Codes.H2, DeclarationMessageTypeList.Codes.DvdH2);
			AssertMessageTypeInAutoSendMode(false, false, JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.B, IMPDeclarationTypeList.Codes.H2, DeclarationMessageTypeList.Codes.DvdH2);
			AssertMessageTypeInAutoSendMode(false, false, JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.X, IMPDeclarationTypeList.Codes.H2, DeclarationMessageTypeList.Codes.DvdH2);
			AssertMessageTypeInAutoSendMode(false, false, JobMessageTypeList.Codes.Import, EntrySubStyleList.Codes.Z, IMPDeclarationTypeList.Codes.H2, DeclarationMessageTypeList.Codes.DvdH2);
		});
	}

	void AssertMessageTypeInAutoSendMode(bool isPous1, bool isPous2, string direction, string instructionSubStyle, string declarationType, string expectedMessageType)
	{
		declaration.JE_MessageType = direction;
		entryHeader.CH_EntryStatus = ZString.Empty;
		entryHeader.ZG_UCC6Version = expectedMessageType == DeclarationMessageTypeList.Codes.ExportUcc6 ? 1 : 0;
		entryHeader.ZG_POUSVersion = isPous2 ? 2 : isPous1 ? 1 : 0;
		entryInstruction.CEI_SubStyle = instructionSubStyle;
		entryInstruction.CEI_Style = declarationType;
		var sendingObject = new MessageSending.JobDeclarationMessageSendingObject(entryHeader, true);
		AssertEquals(expectedMessageType, sendingObject.MessageType);
	}

	#endregion

	public class TestEDIMessage : ESEDIMessage
	{
		public TestEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override string GetMessageReferenceNumber() => "test";
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		var invoice = declaration.Invoices.AddNew();
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
		invoice.InvoiceLines.AddNew();
		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		entryHeader = declaration.CustomsEntryHeaders[0];
		Factory.Save();

		testItem = new MessageSending.JobDeclarationMessageSendingObject(entryHeader);
	}
	MessageSending.JobDeclarationMessageSendingObject testItem;
	Declaration.CusEntryHeader entryHeader;
	Declaration.CusEntryInstruction entryInstruction;
	JobDeclaration declaration;
}
