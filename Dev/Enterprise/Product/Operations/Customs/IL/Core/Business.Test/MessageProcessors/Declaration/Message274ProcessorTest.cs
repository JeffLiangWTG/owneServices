using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.IL.Business.MessageProcessors;
using Enterprise.Customs.IL.Business.Testing.MessageProcessors;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class Message274ProcessorTest : BaseILBranchCustomsApplicationTypeMessageProcessorTest<Message274Processor, ILDEC274ResponseMessage>
	{
		public void TestProcessMessage_Discarded_WhenCouldNotLocateEntryWithReference()
		{
			var factory = Factory;
			const string expectedNote = "Could not locate entry with reference 42430006402024";
			var message = CreateResponseMessage();
			entryHeader.CH_BGMReference = "unknown";
			factory.Save();

			var result = Processor.GetLinkedBusinessObjectMetaData(message, new LoggingInformation());

			CombineAssertions("When Entry Header Not Found", () =>
			{
				Assert("Discard message provided", !result.DiscardReason.IsEmpty);
				AssertEquals("The message has note with text", expectedNote, result.DiscardReason);
			});
		}

		public void TestProcessMessage_Discarded_WhenExternalDeclarationIDIsNull()
		{
			var factory = Factory;
			const string expectedNote = "The incoming message is invalid";
			var message = CreateResponseMessage("Null");
			factory.Save();

			var result = Processor.GetLinkedBusinessObjectMetaData(message, new LoggingInformation());

			CombineAssertions("When Entry Header Not Found", () =>
			{
				Assert("Discard message provided", !result.DiscardReason.IsEmpty);
				AssertEquals("The message has note with text", expectedNote, result.DiscardReason);
			});
		}

		public void TestProcessMessage_Discarded_WhenResponseIsNullAndEntryNotLinked()
		{
			const string expectedNote = "The incoming message is invalid";
			var factory = Factory;
			var message = CreateResponseMessage("ResponseIsNull");
			factory.Save();

			var result = Processor.GetLinkedBusinessObjectMetaData(message, new LoggingInformation());

			CombineAssertions("When Entry Header Not Found", () =>
			{
				Assert("Discard message provided", !result.DiscardReason.IsEmpty);
				AssertEquals("The message has note with text", expectedNote, result.DiscardReason);
			});
		}

		public void TestProcessMessage_Discarded_WhenLocateEntryWithReferenceButEntryLineFeesNotMatch()
		{
			const string expectedNote = "Customs feedback line numbers don't match the entry header line numbers";
			var factory = Factory;
			var message = CreateResponseMessage();
			factory.Save();

			var result = Processor.GetLinkedBusinessObjectMetaData(message, new LoggingInformation());

			CombineAssertions("When Entry Header Found, but UpdateEntryLineFees failed", () =>
			{
				Assert("Discard message provided", !result.DiscardReason.IsEmpty);
				AssertEquals("The message has note with text", expectedNote, result.DiscardReason);
			});
		}

		public void TestProcessMessage_ProcessedOKAndClearEntryLineFees_WhenLocateEntryWithReference()
		{
			var factory = Factory;
			var message = CreateResponseMessage();
			var declaration = entryHeader.Declaration;
			var invoiceHeader1 = declaration.Invoices.AddNew();
			invoiceHeader1.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader1.InvoiceLines.AddNew();

			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.InvoiceLines.AddNew();
			invoiceHeader2.InvoiceLines.AddNew();
			var cusEntryLine1 = entryHeader.MergedLines.AddNew();
			cusEntryLine1.CL_LineNumber = 1;
			var cusEntryLineFee1 = cusEntryLine1.ConfirmedFees.AddNew();
			cusEntryLineFee1.CF_ChargeType = "111";
			var cusEntryLine2 = entryHeader.MergedLines.AddNew();
			cusEntryLine2.CL_LineNumber = 2;
			factory.Save();

			var processor = new Message274Processor(new LoggingInformation());
			processor.ProcessMessage(message);
			factory.Save();

			message.Reload();
			CombineAssertions("When Entry Header Found", () =>
			{
				AssertEquals("The message status is ProcessedOK", "PRS", message.EM_Status);
				AssertEquals("The header CH_Status", "ACK", entryHeader.CH_Status);
				AssertEquals("The header CH_EntryStatus", "99", entryHeader.CH_EntryStatus);
				AssertEquals("The header CH_VersionID", (ZShort)300, entryHeader.CH_VersionID);
				AssertEquals("The CusEntryNumber(IMP) EntryNum", "24013304469780", entryHeader.CusEntryNumber.CE_EntryNum);
				foreach (var el in entryHeader.MergedLines)
				{
					AssertEquals("The entry line fees should be cleared", 0, el.ConfirmedFees.Count);
				}
			});
		}

		public void TestProcessMessage_ProcessedOKAndUpdateEntryLineFees_WhenLocateEntryWithReferenceWithMatchEntryLine()
		{
			var factory = Factory;
			var message = CreateResponseMessage("_ConfirmedFees");
			entryHeader.CH_BGMReference = "42450136632024";
			var declaration = entryHeader.Declaration;
			var invoiceHeader1 = declaration.Invoices.AddNew();
			invoiceHeader1.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader1.InvoiceLines.AddNew();

			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.InvoiceLines.AddNew();
			invoiceHeader2.InvoiceLines.AddNew();
			var cusEntryLine1 = entryHeader.MergedLines.AddNew();
			cusEntryLine1.CL_LineNumber = 1;
			var cusEntryLineFee1 = cusEntryLine1.ConfirmedFees.AddNew();
			cusEntryLineFee1.CF_ChargeType = "111";
			factory.Save();

			var processor = new Message274Processor(new LoggingInformation());
			processor.ProcessMessage(message);
			factory.Save();

			message.Reload();
			CombineAssertions("When Entry Header Found", () =>
			{
				AssertEquals("The message status is ProcessedOK", "PRS", message.EM_Status);
				AssertEquals("The header CH_Status", "ACK", entryHeader.CH_Status);
				AssertEquals("The header CH_EntryStatus", "13", entryHeader.CH_EntryStatus);
				AssertEquals("The header CH_VersionID", (ZShort)200, entryHeader.CH_VersionID);
				AssertEquals("The CusEntryNumber(IMP) EntryNum", "24033314228481", entryHeader.CusEntryNumber.CE_EntryNum);
			});

			var entryLine = entryHeader.MergedLines.Single();
			var confirmedFees = entryLine.ConfirmedFees.Cast<CusEntryLineFee>().ToList();
			AssertEquals("The entry line fees should hold 3 items", 3, confirmedFees.Count);
			AssertEntryLineFee(confirmedFees, expectedChargeType: "1", expectedBaseValue: 63235m, expectedChargeAmount: 4426m, expectedRate: 7m);
			AssertEntryLineFee(confirmedFees, expectedChargeType: "16", expectedBaseValue: 67661m, expectedChargeAmount: 37227m, expectedRate: 83m);
			AssertEntryLineFee(confirmedFees, expectedChargeType: "15", expectedBaseValue: 104888m, expectedChargeAmount: 17831m, expectedRate: 17m);

			var confirmedCharges = entryHeader.ConfirmedCharges;
			AssertEquals("ConfirmedCharges count should be 5 (Group by TypeCode.value)", 5, confirmedCharges.Count);
			var charges = confirmedCharges.Cast<CusEntryHeaderCharges>().ToList();
			AssertCharge(charges, "1", 4426m);
			AssertCharge(charges, "16", 37227m);
			AssertCharge(charges, "15", 17831m);
			AssertCharge(charges, "36", 42m);
			AssertCharge(charges, "50", 47m);
		}

		public void TestProcessMessage_ProcessedOK_WhenResponseIsNullAndEntryLinked()
		{
			const string expectedMessage = "Declaration data was not updated since message was rejected by customs";
			var factory = Factory;
			var message = CreateResponseMessage("ResponseIsNull");

			entryHeader.CH_Status = EDIMessageStatusList.Codes.Acknowledged;
			entryHeader.CH_EntryStatus = "13";
			entryHeader.CH_VersionID = 300;
			entryHeader.EntryNumber = "24013304469780";
			message.EM_LinkedObject = entryHeader;
			factory.Save();

			Processor.ProcessMessage(message);
			factory.Save();

			message.Reload();
			CombineAssertions("When Entry Header Not Found", () =>
			{
				AssertEquals("The message status is Processed", "PRS", message.EM_Status);
				var notes = (StmNoteCollection)message.Notes.GetAllNotes();
				AssertEquals("The message has one note", 1, notes.Count);
				AssertEquals("The message note text matches the expected message", expectedMessage, notes[0].ST_NoteText);

				AssertEquals("The header CH_Status remains ACK", "ACK", entryHeader.CH_Status);
				AssertEquals("The header CH_EntryStatus remains 13", "13", entryHeader.CH_EntryStatus);
				AssertEquals("The header CH_VersionID remains 300", (ZShort)300, entryHeader.CH_VersionID);
				AssertEquals("The CusEntryNumber(IMP) EntryNum remains 24013304469780", "24013304469780", entryHeader.CusEntryNumber.CE_EntryNum);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			entryHeader = CreateEntryHeader("42430006402024");
			glbBranch = entryHeader.Branch;

			Factory.Save();
		}

		protected override string ExpectedMessageFriendlyName => "IL Import Declaration Response Message";

		protected override string ExpectedMessageTypesToInclude => "DEC";

		protected override string ExpectedMessageSubTypesToInclude => ZString.Empty;

		protected override string BasicSuccessfulMessageText => new EmbeddedResourceRetriever().GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("ImportDeclarationResponse_2754_NoFees.xml"));

		protected override BusinessObject ExpectedLinkedObject => entryHeader;

		protected override ZGuid ExpectedBranchPk => glbBranch.PK;

		protected override Message274Processor CreateProcessor(LoggingInformation loggingInformation) => new Message274Processor(loggingInformation);

		CusEntryHeader CreateEntryHeader(string bgmReference)
		{
			var factory = Factory;
			var declaration = factory.New<JobDeclaration>();
			var ilCompany = factory.New<GlbCompany>();
			ilCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Israel;
			var ilBranch = ilCompany.Branches.AddNew();
			declaration.JE_GB = ilBranch.PK;

			declaration.JE_CustomsOffice = "IL123";
			declaration.JE_MessageType = "IMP";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = bgmReference;
			entryHeader.CH_JE = declaration.PK;
			var entryInstruction = factory.New<CusEntryInstruction>();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			declaration.CustomsEntryInstructions.Add(entryInstruction);

			return entryHeader;
		}

		ILDEC274ResponseMessage CreateResponseMessage(string suffix = null)
		{
			var message = Factory.New<ILDEC274ResponseMessage>();
			message.EM_ApplicationCode = "ILC";
			message.EM_MessageType = "DEC";
			message.EM_MessageSubType = "274";
			message.EM_Status = "QUE";
			message.EM_ReceiveTransmit = "RCV";

			message.EM_MessageText = new EmbeddedResourceRetriever().GetString(ILBusinessTestHelper.GetEmbeddedResourcePath($"ImportDeclarationResponse_2754{suffix}.xml"));
			return message;
		}

		static void AssertEntryLineFee(IEnumerable<CusEntryLineFee> confirmedFees, ZString expectedChargeType, decimal expectedBaseValue, decimal expectedChargeAmount, decimal expectedRate)
		{
			CombineAssertions($"entry Line fee with Charge Type: {expectedChargeType}", () =>
			{
				var entryLineFee = confirmedFees.FirstOrDefault(r => r.CF_ChargeType == expectedChargeType);
				AssertNotNull($"Fee with Charge Type: {expectedChargeType} should be", entryLineFee);
				AssertEquals("CF_BaseValue", expectedBaseValue, entryLineFee.CF_BaseValue);
				AssertEquals("CF_ChargeAmount", expectedChargeAmount, entryLineFee.CF_ChargeAmount);
				AssertEquals("CF_MethodOfCalculation", "%", entryLineFee.CF_MethodOfCalculation);
				AssertEquals("CF_Rate", expectedRate, entryLineFee.CF_Rate);
			});
		}

		static void AssertCharge(IEnumerable<CusEntryHeaderCharges> charges, string expectedChargeType, decimal expectedAmount)
		{
			CombineAssertions($"Charge with Type: {expectedChargeType}", () =>
			{
				var charge = charges.FirstOrDefault(c => c.C1_ChargeType == expectedChargeType);
				AssertNotNull($"Charge with Type: {expectedChargeType} should exist", charge);
				AssertEquals("C1_ChargeAmount", expectedAmount, charge.C1_ChargeAmount);
				AssertEquals("C1_Source", CusEntryHeaderChargesSourceCodeList.Codes.CUS, charge.C1_Source);
				AssertEquals("C1_IsLandedCostOnly", false, charge.C1_IsLandedCostOnly);
			});
		}

		GlbBranch glbBranch;
		CusEntryHeader entryHeader;
	}
}
