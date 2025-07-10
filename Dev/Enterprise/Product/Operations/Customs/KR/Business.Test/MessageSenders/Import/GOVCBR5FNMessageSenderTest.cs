using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5FNMessageSenderTest : TestCaseWithFactory
	{
		IEnumerable<JobDeclarationMiscMessageSendingObject> GetMessageParents() => MessageSendingObjects;

		MessageSender GetMessageSender() => IsExceptionTest ? new GOVCBR5FNSenderForTest(MessageSendingObjects, Factory) : new GOVCBR5FNSender(MessageSendingObjects, Factory);

		IEnumerable<JobDeclarationMiscMessageSendingObject> Parents => parents ?? (parents = GetMessageParents());
		IEnumerable<JobDeclarationMiscMessageSendingObject> parents;

		IEnumerable<JobDeclarationMiscMessageSendingObject> MessageSendingObjects
		{
			get
			{
				if (messageSendingObjects == null)
				{
					var helper = new UniversalReferenceTestDataHelper(Factory);
					var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, ZZ.TariffTypes.DutyReductionExemption);
					var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "A093000004", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "관세법 제93조제4호 해당물품");
					helper.CreateTariffAttribute(Customs.KR.Messaging.Constants.ZZ.TariffAttributes.IsDutyExempt, YesNo.Yes, tariff1);
					Factory.Save();

					var entry = new TestDataSetupHelper(Factory).GetEntry929FullData(ZString.Empty, ZBool.True);
					entry.Messages.AddNew().EM_MessageType = ElectronicDocumentTypeList.Codes._929;

					var invoiceLine = entry.MergedLines[0].RandomLine;
					invoiceLine.JI_SpecificUseCodeDutyRatePermitNo = "";
					invoiceLine.JI_IsSpecificUseCode = false;
					invoiceLine.JI_SecondaryPreference = "A093000004";
					invoiceLine.JI_InstallmentCode = "";

					var sendingObjectParent = new JobDeclarationMiscMessageSendingObjectParent(entry.Declaration, ElectronicDocumentTypeList.Codes._5FN, MessageFunctions.MessageFunctionCode.Original);
					messageSendingObjects = sendingObjectParent.SendingObjectsCollection.Cast<JobDeclarationMiscMessageSendingObject>();
					messageSendingObjects.FirstOrDefault().MessageSendingEntryLines[0].ShouldSend = true;
				}
				return messageSendingObjects;
			}
		}

		IEnumerable<JobDeclarationMiscMessageSendingObject> messageSendingObjects;

		public void TestStatusIsUpdated()
		{
			GetMessageSender().Send();
			foreach (JobDeclarationMiscMessageSendingObject parent in Parents)
			{
				var entryLineObject = parent.MessageSendingEntryLines[0];
				var entryNum5FN = parent.Header.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5FN && ZInt.ParseEmptyAsZero(x.CE_EntryLineReference) == entryLineObject.EntryLineNo);
				AssertNotNull(entryNum5FN);
				AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalSent, entryNum5FN.CE_EntryStatus);
				AssertEquals(entryNum5FN.CE_EntryNum, parent.Header.EntryNumber);
				AssertNotNull(parent.Header.Messages.Cast<EDIMessage>().FirstOrDefault(x => ZInt.ParseSafe(x.EM_ApplicationReference, 0) == entryLineObject.EntryLineNo));
			}
		}

		public void TestMessagesReferHeader()
		{
			GetMessageSender().Send();
			foreach (JobDeclarationMiscMessageSendingObject parent in Parents)
			{
				AssertEquals(2, parent.Header.Messages.Count);
			}
		}

		public void TestSendException()
		{
			IsExceptionTest = ZBool.True;
			var messages = Parents.FirstOrDefault().Header.Messages;
			AssertEquals("One message exits before sending a message.", 1, messages.Count);
			AssertExceptionThrown<Exception>(() => GetMessageSender().Send());
			AssertEquals("Exception occurred when sending a message, and no new message has been created.", 1, messages.Count);
		}
		public ZBool IsExceptionTest;

		class GOVCBR5FNSenderForTest : GOVCBR5FNSender
		{
			public GOVCBR5FNSenderForTest(IEnumerable<JobDeclarationMiscMessageSendingObject> entries, BusinessObjectFactory factory)
				: base(entries, factory)
			{
			}

			protected override Import5FNLine GetMessageDataProvider(CusEntryHeader parent, ZString messageId) => throw new Exception();
		}
	}
}
