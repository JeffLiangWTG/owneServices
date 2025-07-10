using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Registry;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.MessageSending.Testing
{
	[TestedType(typeof(DeltaGJobDeclarationMessageSendingObject))]
	public class DeltaGJobDeclarationMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestTriggeringPointForValidation_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var sendingObject = new DeltaGJobDeclarationMessageSendingObject(entryHeader);

			sendingObject.MessageType = EntryActionCodeList.Codes.ANT;
			AssertEquals(true, sendingObject.TriggeringPointForValidationInfo.ReadOnly);

			sendingObject.MessageType = EntryActionCodeList.Codes.MDA;
			AssertEquals(true, sendingObject.TriggeringPointForValidationInfo.ReadOnly);

			sendingObject.MessageType = EntryActionCodeList.Codes.MAP;
			AssertEquals(true, sendingObject.TriggeringPointForValidationInfo.ReadOnly);

			sendingObject.MessageType = EntryActionCodeList.Codes.VAL;
			AssertEquals(true, sendingObject.TriggeringPointForValidationInfo.ReadOnly);
		}

		public void TestChangeAcknowledgementIndicatorReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var sendingObject = new DeltaGJobDeclarationMessageSendingObject(entryHeader);

			sendingObject.MessageType = EntryActionCodeList.Codes.INV;
			AssertEquals(false, sendingObject.ChangeAcknowledgementIndicatorInfo.ReadOnly);

			sendingObject.MessageType = EntryActionCodeList.Codes.REC;
			AssertEquals(false, sendingObject.ChangeAcknowledgementIndicatorInfo.ReadOnly);

			sendingObject.MessageType = EntryActionCodeList.Codes.MAP;
			Assert(sendingObject.ChangeAcknowledgementIndicatorInfo.ReadOnly);
		}

		public void TestDefaultinOfRegularJustification()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var sendingObject = new DeltaGJobDeclarationMessageSendingObject(entryHeader);
			AssertEquals(ZString.Empty, sendingObject.ChangeAcknowledgementIndicator);

			sendingObject.MessageType = EntryActionCodeList.Codes.REC;
			AssertEquals(Customs.FR.Business.ReasonCodeList.Codes.C173, sendingObject.ChangeAcknowledgementIndicator);

			entryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES060;
			sendingObject.MessageType = EntryActionCodeList.Codes.INV;
			AssertEquals(Customs.FR.Business.ReasonCodeList.Codes.C174, sendingObject.ChangeAcknowledgementIndicator);

			entryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			sendingObject.MessageType = EntryActionCodeList.Codes.INV;
			AssertEquals(Customs.FR.Business.ReasonCodeList.Codes.C148, sendingObject.ChangeAcknowledgementIndicator);
		}

		public void TestAvailableRegularJustificationCodes()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var sendingObject = new DeltaGJobDeclarationMessageSendingObject(entryHeader);

			sendingObject.MessageType = EntryActionCodeList.Codes.REC;
			AssertContainsExactElementsInAnyOrder(new string[] { Customs.FR.Business.ReasonCodeList.Codes.C173 }, sendingObject.ReasonCodeList.GetAllCodes());
			AssertContainsExactElementsInAnyOrder(new string[] { Customs.FR.Business.ReasonCodeList.Codes.C173 }, sendingObject.ReasonCodeShortDescriptionList.GetAllCodes());

			sendingObject.MessageType = EntryActionCodeList.Codes.INV;
			AssertContainsExactElementsInAnyOrder(new string[] { Customs.FR.Business.ReasonCodeList.Codes.C174, Customs.FR.Business.ReasonCodeList.Codes.C148 }, sendingObject.ReasonCodeList.GetAllCodes());
			AssertContainsExactElementsInAnyOrder(new string[] { Customs.FR.Business.ReasonCodeList.Codes.C174, Customs.FR.Business.ReasonCodeList.Codes.C148 }, sendingObject.ReasonCodeShortDescriptionList.GetAllCodes());
		}

		public void TestGetInitialMessageType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var sendingObject = new DeltaGJobDeclarationMessageSendingObject(entryHeader);
			AssertEquals("InitialMessageDefaultType should be empty when no entry instruction is set against the entry", ZString.Empty, sendingObject.GetInitialMessageType());

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			entryInstruction.CEI_SubStyle = "D";
			AssertEquals("InitialMessageDefaultType should be ANT when the entry instruction is prelodged", EntryActionCodeList.Codes.ANT, sendingObject.GetInitialMessageType());

			entryInstruction.CEI_SubStyle = "A";
			AssertEquals("InitialMessageDefaultType should be empty when the entry instruction is lodged", EntryActionCodeList.Codes.VAL, sendingObject.GetInitialMessageType());

			entryInstruction.CEI_SubStyle = "Y";
			AssertEquals("InitialMessageDefaultType should be empty when the entry instruction is not prelodged nor lodged", ZString.Empty, sendingObject.GetInitialMessageType());
		}

		public void TestMessageTypesList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var fallbackSettings = new FallbackSettings();
			fallbackSettings.End = ZDateTime.Empty;
			fallbackSettings.Start = ZDateTime.Today.AddDays(1);
			FRCustomsDataRegistry.Instance.DeltaGMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSettings);
			var sendingObject = new DeltaGJobDeclarationMessageSendingObject(entry);

			AssertEquals("Precondition", false, FRCustomsDataRegistry.DeltaGFallbackIsActive);
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G1, ZString.Empty, false, new string[] { "ANT", "VAL" });
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES010, false, new string[] { "ANT", "VAL" });
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES040, false, new string[] { "ANT", "VAL" });
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES050, false, new string[] { "MAP", "VAA", "ANA" });
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES055, false, new string[] { "EAV" });
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES060, false, new string[] { "INV", "REC", "CMP" });
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES061, false, new string[] { "INV", "REC", "CMP" });
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES070, false, new string[] { "INV", "REC", "CMP" });
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES075, false, new string[] { "INV", "REC", "CMP" });
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES080, false, new string[] { "INV", "REC", "CMP" });
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES083, false, new string[] { "INV", "REC", "CMP" });
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES085, false, new string[] { "INV", "REC", "CMP" });
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES090, false, Array.Empty<string>());
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES100, false, new string[] { "INV", "REC", "CMP" });
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES101, false, new string[] { "INV", "REC", "CMP" });
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES114, false, new string[] { "VAR", "ANR" });
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES115, false, Array.Empty<string>());
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES116, false, Array.Empty<string>());
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES117, false, Array.Empty<string>());
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES118, false, Array.Empty<string>());
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES119, false, Array.Empty<string>());
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES150, false, Array.Empty<string>());
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES151, false, Array.Empty<string>());
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G1, "XXX", false, new string[] { "ANA", "ANR", "ANT", "CMP", "EAV", "INV", "MAP", "REC", "RPS", "VAA", "VAL", "VAR" });

			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G2, ZString.Empty, false, new string[] { "ANT", "VAL" });
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES010, false, new string[] { "ANT", "VAL" });
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES040, false, new string[] { "ANT", "VAL" });
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES050, false, new string[] { "MDA", "VAA", "ANN" });
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES060, false, new string[] { "D2M", "INV", "REC", });
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES061, false, new string[] { "D2M", "INV", "REC" });
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES062, false, Array.Empty<string>());
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES063, false, Array.Empty<string>());
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES070, false, new string[] { "D2M", "INV", "REC" });
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES075, false, new string[] { "D2M", "INV", "REC" });
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES080, false, new string[] { "D2M", "INV", "REC" });
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES081, false, Array.Empty<string>());
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES082, false, new string[] { "D2M", "INV", "REC" });
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES083, false, new string[] { "D2M", "INV", "REC" });
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES085, false, new string[] { "D2M", "INV", "REC" });
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES090, false, Array.Empty<string>());
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES100, false, new string[] { "D2M", "INV", "REC" });
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES101, false, new string[] { "D2M", "INV", "REC" });
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES111, false, Array.Empty<string>());
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES112, false, Array.Empty<string>());
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES130, false, new string[] { "D2M", "INV", "REC" });
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES131, false, Array.Empty<string>());
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES132, false, Array.Empty<string>());
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES140, false, new string[] { "INV" });
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES150, false, Array.Empty<string>());
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES151, false, Array.Empty<string>());
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G2, "XXX", false, new string[] { "ANT", "ANN", "D2M", "INV", "MDA", "MDV", "REC", "RPS", "VAA", "VAL" });

			fallbackSettings.End = ZDateTime.Today.AddDays(1);
			fallbackSettings.Start = ZDateTime.Today.AddDays(-1);
			FRCustomsDataRegistry.Instance.DeltaGMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSettings);
			AssertEquals("Precondition", true, FRCustomsDataRegistry.DeltaGFallbackIsActive);
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G1, ZString.Empty, true, new string[] { "RPS" });
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES100, true, new string[] { "RPS" });
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G2, ZString.Empty, true, new string[] { "RPS" });
			MessageTypeListTestHelper(declaration, entry, sendingObject, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES130, true, new string[] { "RPS" });
		}

		void MessageTypeListTestHelper(JobDeclaration declaration, CusEntryHeader entry, DeltaGJobDeclarationMessageSendingObject sendingObject, string deltaMode, string entryStatus, bool setIsDeltaGFallbackInactiveAndNotRegularised, string[] expectedList)
		{
			entry.CH_EntryStatus = entryStatus;
			declaration.JE_DeltaMode = deltaMode;
			AssertContainsExactElementsInAnyOrder(expectedList, sendingObject.MessageTypesList.GetAllCodes());
		}

		public void TestProperties()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = MessageSubTypeList.Codes.IMC;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_JE = declaration.PK;
			var line = invoice.InvoiceLines.AddNew();
			line.JI_JZ = invoice.PK;

			Factory.Save();

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES060;
			entry.MovementReferenceNumberSetter("ABC", ZDateTime.BrettsBirthday);
			entry.CH_CustomsMessageRemarks = "Amendment";
			entry.CH_BGMReference = "1GB945390992000-B00001002";

			Factory.Save();

			var testItem = new DeltaGJobDeclarationMessageSendingObject(entry);

			AssertEquals(ZString.Empty, testItem.MessageType);
			AssertEquals(EntryStatusDescriptionCodeList.Codes.ES060, testItem.EntryStatus);
			AssertEquals("ABC", testItem.MovementReferenceNumber);
			AssertEquals("Amendment", testItem.VOCReason);
			AssertEquals("1GB945390992000-B00001002", testItem.LocalReferenceNumber);
		}

		public void TestDefaultMessageType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var fallbackSettings = new FallbackSettings();
			fallbackSettings.End = ZDateTime.Empty;
			fallbackSettings.Start = ZDateTime.Today.AddDays(1);
			FRCustomsDataRegistry.Instance.DeltaGMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSettings);
			AssertEquals("Precondition", false, FRCustomsDataRegistry.DeltaGFallbackIsActive);

			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G1, ZString.Empty, true, "RPS");
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G1, ZString.Empty, false, ZString.Empty);
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES010, false, ZString.Empty);
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES040, false, ZString.Empty);
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES050, false, "MAP");
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES055, false, "EAV");
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES060, false, "REC");
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES061, false, "REC");
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES070, false, "REC");
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES075, false, "REC");
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES080, false, "REC");
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES083, false, "REC");
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES085, false, "REC");
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES100, false, "REC");
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES100, true, "RPS");
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES101, false, "REC");
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES114, false, "VAR");
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES120, false, "REC");

			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G2, ZString.Empty, true, "RPS");
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G2, ZString.Empty, false, ZString.Empty);
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES010, false, ZString.Empty);
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES040, false, ZString.Empty);
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES050, false, "MDA");
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES060, false, "REC");
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES061, false, "REC");
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES070, false, "REC");
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES075, false, "REC");
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES080, false, "REC");
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES082, false, "REC");
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES083, false, "REC");
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES085, false, "REC");
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES100, false, "D2M");
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES101, false, "D2M");
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES130, false, "D2M");
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES130, true, "RPS");
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES140, false, "INV");

			fallbackSettings.End = ZDateTime.Today.AddDays(1);
			fallbackSettings.Start = ZDateTime.Today.AddDays(-1);
			FRCustomsDataRegistry.Instance.DeltaGMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSettings);
			AssertEquals("Precondition: When DeltaGFallbackIsActive, always return RPS type", true, FRCustomsDataRegistry.DeltaGFallbackIsActive);
			AssertEquals("Precondition: DeltaG Fallback Is Active", false, entry.IsDeltaGFallbackInactiveAndNotRegularised);
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G1, ZString.Empty, true, "RPS");
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES100, true, "RPS");
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G1, ZString.Empty, false, "RPS");
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G1, EntryStatusDescriptionCodeList.Codes.ES100, false, "RPS");

			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G2, ZString.Empty, true, "RPS");
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES130, true, "RPS");
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G2, ZString.Empty, false, "RPS");
			DefaultMessageTypeTestHelper(declaration, entry, OrgCusAccountDeltaGTypeList.Codes.G2, EntryStatusDescriptionCodeList.Codes.ES130, false, "RPS");
		}

		void DefaultMessageTypeTestHelper(JobDeclaration declaration, CusEntryHeader entry, string deltaMode, string entryStatus, bool setIsDeltaGFallbackInactiveAndNotRegularised, string expectedCode)
		{
			if (!FRCustomsDataRegistry.DeltaGFallbackIsActive)
			{
				if (setIsDeltaGFallbackInactiveAndNotRegularised)
				{
					entry.DeltaGFallbackStatus = "PDS";
					Assert(entry.IsDeltaGFallbackInactiveAndNotRegularised);
				}
				else
				{
					entry.DeltaGFallbackStatus = ZString.Empty;
					Assert(!entry.IsDeltaGFallbackInactiveAndNotRegularised);
				}
			}

			entry.CH_EntryStatus = entryStatus;
			declaration.JE_DeltaMode = deltaMode;
			var sendingObject = new DeltaGJobDeclarationMessageSendingObject(entry);
			AssertEquals(expectedCode, sendingObject.MessageType);

			if (deltaMode == OrgCusAccountDeltaGTypeList.Codes.G1)
			{
				var sendingObject2 = new DeltaGJobDeclarationMessageSendingObject(entry);
				AssertEquals(expectedCode, sendingObject2.MessageType);
			}
		}

		#region Overrides of BusinessObjectBaseTestCase

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			return new DeltaGJobDeclarationMessageSendingObject(entryHeader);
		}
		#endregion
	}
}
