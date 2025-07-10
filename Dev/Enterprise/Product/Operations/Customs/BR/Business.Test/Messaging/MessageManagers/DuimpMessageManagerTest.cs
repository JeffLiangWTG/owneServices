using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using CargoWise.Customs.BR.MessageDefinitions.Duimp;
using CargoWise.Types;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestDate(2021, 7, 19)]
	public class DuimpMessageManagerTest : DeclarationMessageManagerTest
	{
		protected override ZString ExpectedMessageFriendlyName => MessageTypeList.Descriptions.CDD;

		protected override ZString JobDeclarationMessageType => BRJobMessageTypeList.Codes.Import;

		protected override ZString ExpectedEM_MessageType => MessageTypeList.Codes.CIH;

		protected override string OriginalMessageType => ImportEntryActionCodeList.Codes.ORI;

		protected override string AmendmentMessageType => null;

		protected override string WithdrawalMessageType => null;

		protected override string ExpectedCH_StatusWhenNoMessages => ZString.Empty;

		protected override DeclarationMessageSendingObject CreateMessageSendingObject(CusEntryHeader entryHeader) => new DuimpMessageSendingObject(entryHeader);

		protected override void DoChangesRequiresAmendment(JobDeclaration declaration)
		{
			declaration.CustomsEntryInstructions[0].AdditionalInformation = "Additional Information TEST 2";
		}

		protected override bool ShouldSendMessageWhenEntryNumberHasValue(ZString messageType) => messageType != EDIMessageSubTypeList.Codes.Original;

		public void TestGenerateMessages_CVH()
		{
			AssertMessagesGenerated(CreateEntryHeader(2), ImportEntryActionCodeList.Codes.CVH, MessageTypeList.Codes.CDD, "21BR0000022649|111", 1, expectedMessageText: "");
		}

		public void TestGenerateMessages_DEL()
		{
			AssertMessagesGenerated(CreateEntryHeader(2), ImportEntryActionCodeList.Codes.DEL, MessageTypeList.Codes.DOR, "21BR0000022649|111", 1, expectedMessageText: "");
		}

		public void TestGenerateMessages_DIA()
		{
			AssertMessagesGenerated(CreateEntryHeader(2), ImportEntryActionCodeList.Codes.DIA, MessageTypeList.Codes.CIH, "21BR0000022649|111", 1, expectedMessageText: "{\"totalItem\":2}");
		}

		public void TestGenerateMessages_COM()
		{
			AssertMessagesGenerated(CreateEntryHeader(2), EDIMessageSubTypeList.Codes.CompleteConsult, MessageTypeList.Codes.CIH, "21BR0000022649|111", 1, expectedMessageText: "");
		}

		public void TestGenerateMessages_REG()
		{
			AssertMessagesGenerated(CreateEntryHeader(2), ImportEntryActionCodeList.Codes.REG, MessageTypeList.Codes.CIH, "21BR0000022649|111", 1, expectedMessageText: "{\"totalItem\":2,\"pagamentos\":[{\"principal\":{\"tributo\":{\"tipo\":\"II\"},\"valor\":20.2}}]}");
		}

		public void TestGenerateMessages_RET()
		{
			AssertMessagesGenerated(CreateEntryHeader(2), ImportEntryActionCodeList.Codes.RET, MessageTypeList.Codes.CDD, "21BR0000022649|111", 1);
		}

		public void TestGenerateMessages_ORI_NoEntryNumber()
		{
			using (BRCustomsDataRegistry.Instance.MaxNumberOfEntryLineInDuimpMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 7))
			{
				var entryHeader = CreateEntryHeader(7, false);

				AssertMessagesGenerated(entryHeader, ImportEntryActionCodeList.Codes.ORI, MessageTypeList.Codes.CIH, "", 1,	expectedMessageSubType: EDIMessageSubTypeList.Codes.Original);
			}
		}

		public void TestGenerateMessages_ORI_HasEntryNumber_HeaderHasNoUpdate()
		{
			using (BRCustomsDataRegistry.Instance.MaxNumberOfEntryLineInDuimpMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 7))
			{
				var entryHeader = CreateEntryHeader(0);
				entryHeader.CH_CustomsPostedStatus = CustomsPostedStatusList.Codes.Accepted;

				CreateEntryLines(entryHeader, 3, CustomsPostedStatusList.Codes.Active);
				CreateEntryLines(entryHeader, 3, CustomsPostedStatusList.Codes.Accepted);
				CreateEntryLines(entryHeader, 5, CustomsPostedStatusList.Codes.UpdatePending);
				CreateEntryLines(entryHeader, 3, CustomsPostedStatusList.Codes.DeletePending);
				CreateEntryLines(entryHeader, 7, CustomsPostedStatusList.Codes.Deleted);

				AssertMessagesGenerated(entryHeader, ImportEntryActionCodeList.Codes.ORI, MessageTypeList.Codes.CIH, "21BR0000022649|111", 0,
					expectedLineMessagesCountForEachMessageSubType: new[]
					{
						(EDIMessageSubTypeList.Codes.Addition, 1),
						(EDIMessageSubTypeList.Codes.Update, 1),
						(EDIMessageSubTypeList.Codes.Deletion, 3),
					});
			}
		}

		public void TestGenerateMessages_ORI_HasEntryNumber_HeaderUpdated()
		{
			using (BRCustomsDataRegistry.Instance.MaxNumberOfEntryLineInDuimpMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			{
				var entryHeader = CreateEntryHeader(0);
				entryHeader.CH_CustomsPostedStatus = CustomsPostedStatusList.Codes.UpdatePending;

				CreateEntryLines(entryHeader, 3, CustomsPostedStatusList.Codes.Active);
				CreateEntryLines(entryHeader, 3, CustomsPostedStatusList.Codes.Accepted);
				CreateEntryLines(entryHeader, 5, CustomsPostedStatusList.Codes.UpdatePending);
				CreateEntryLines(entryHeader, 7, CustomsPostedStatusList.Codes.DeletePending);
				CreateEntryLines(entryHeader, 1, CustomsPostedStatusList.Codes.Deleted);

				AssertMessagesGenerated(entryHeader, ImportEntryActionCodeList.Codes.ORI, MessageTypeList.Codes.CIH, "21BR0000022649|111", 1,
					expectedMessageSubType: EDIMessageSubTypeList.Codes.Update,
					expectedLineMessagesCountForEachMessageSubType: new[]
					{
						(EDIMessageSubTypeList.Codes.Addition, 2),
						(EDIMessageSubTypeList.Codes.Update, 3),
						(EDIMessageSubTypeList.Codes.Deletion, 7),
					});
			}
		}

		public new void TestHasActiveMessages()
		{
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			entryHeader.CH_Status = BRMessageStatusList.Codes.AwaitingResponse;
			entryHeader.EntryNumber = "123";
			entryHeader.CH_EntryStatus = Constants.EntryStatus.Registered;

			var messageObject = new DuimpMessageManager(new DuimpMessageSendingObject(entryHeader));

			Assert("HasActiveMessages should be True", messageObject.HasActiveMessages);

			entryHeader.EntryNumber = ZString.Empty;
			entryHeader.CH_EntryStatus = ZString.Empty;

			Assert("HasActiveMessages should be True", messageObject.HasActiveMessages);

			entryHeader.CH_Status = ZString.Empty;

			AssertEquals("HasActiveMessages should be false", false, messageObject.HasActiveMessages);

			entryHeader.EntryNumber = "123";
			entryHeader.CH_EntryStatus = Constants.EntryStatus.Registered;

			Assert("HasActiveMessages should be true", messageObject.HasActiveMessages);
		}

		CusEntryHeader CreateEntryHeader(int countOfEntryLines, bool setMRNAndVersion = true)
		{
			var entryHeader = base.CreateEntryHeader();
			if (setMRNAndVersion)
			{
				entryHeader.MovementReferenceNumberSetter("21BR0000022649");
				entryHeader.CH_AuthorityVersion = "111";
			}
			entryHeader.Declaration.Invoices.AddNew();

			CreateEntryLines(entryHeader, countOfEntryLines, CustomsPostedStatusList.Codes.Active);

			return entryHeader;
		}

		void CreateEntryLines(CusEntryHeader entryHeader, int countOfEntryLines, string customsPostedStatus)
		{
			for (var i = 0; i < countOfEntryLines; i++)
			{
				var entryLine = entryHeader.AllEntryLines.AddNew();
				entryLine.CL_CustomsPostedStatus = customsPostedStatus;
				entryLine.CL_LineNumber = 3;

				if (entryLine.CL_CustomsPostedStatus != CustomsPostedStatusList.Codes.DeletePending && entryLine.CL_CustomsPostedStatus != CustomsPostedStatusList.Codes.Deleted)
				{
					entryHeader.Declaration.Invoices[0].InvoiceLines.AddNew().JI_CL = entryLine.PK;
					entryLine.Fees.AddOrUpdate(ChargeTypesList.Codes.DTY, 10.1m);
				}
			}
		}

		void AssertMessagesGenerated(CusEntryHeader entryHeader, ZString entryActionCode, ZString expectedMessageType, ZString expectedApplicationReference,
			int expectedMessagesCount, string expectedMessageSubType = null, string expectedMessageText = null,
			IEnumerable<(string, int)> expectedLineMessagesCountForEachMessageSubType = null)
		{
			var sendingMessage = new DuimpMessageSendingObject(entryHeader);
			sendingMessage.MessageType = entryActionCode;

			var allMessages = new DuimpMessageManager(sendingMessage).GenerateMessages();
			var messages = allMessages.Where(w => w.EM_MessageType == expectedMessageType).ToArray();
			AssertEquals($"{expectedMessageType} Messages count", expectedMessagesCount, messages.Length);

			if (expectedMessagesCount > 0)
			{
				CombineAssertions($"{expectedMessageType} Message", () =>
				{
					AssertGenerateMessages(allMessages[0], expectedMessageType, expectedMessageSubType ?? entryActionCode, expectedApplicationReference, entryHeader, expectedMessageText);
				});
			}

			var lineMessages = allMessages.Where(w => w.EM_MessageType == MessageTypeList.Codes.CIL).ToArray();
			AssertEquals($"{MessageTypeList.Codes.CIL} Messages count", expectedLineMessagesCountForEachMessageSubType?.Sum(x => x.Item2) ?? 0, lineMessages.Length);

			if (expectedLineMessagesCountForEachMessageSubType != null)
			{
				foreach (var (messageSubType, expectedLineMessagesCount) in expectedLineMessagesCountForEachMessageSubType)
				{
					CombineAssertions($"{MessageTypeList.Codes.CIL}|{messageSubType} Message", () =>
					{
						var lineMessagesWithSubType = lineMessages.Where(x => x.EM_MessageSubType == messageSubType);
						AssertEquals($"{MessageTypeList.Codes.CIL}|{messageSubType} Messages count", expectedLineMessagesCount, lineMessagesWithSubType.Count());

						foreach (var lineMessage in lineMessagesWithSubType)
						{
							if (messageSubType == EDIMessageSubTypeList.Codes.Deletion)
							{
								AssertGenerateMessages(lineMessage, MessageTypeList.Codes.CIL, messageSubType, "21BR0000022649|111|3", entryHeader, ZString.Empty);
							}
							else
							{
								AssertGenerateMessages(lineMessage, MessageTypeList.Codes.CIL, messageSubType, expectedApplicationReference.IsEmpty ? "|" : "21BR0000022649|111", entryHeader);
								AssertLessThanOrEqualTo(JsonSerializer.Deserialize<ItemCover[]>(lineMessage.EM_MessageText).Length, BRCustomsDataRegistry.Instance.MaxNumberOfEntryLineInDuimpMessage.Value);
							}
						}
					});
				}
			}
		}
	}
}
