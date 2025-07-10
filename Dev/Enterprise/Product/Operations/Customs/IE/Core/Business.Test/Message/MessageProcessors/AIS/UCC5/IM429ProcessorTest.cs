using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM429;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Customs.IE.Messaging.UCC5;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(IM429Processor))]
	class IM429ProcessorTest : EntryHeaderMessageProcessorTest<IM429Processor, AISUCC5InboundEDIMessage, AISUCC5OutboundEDIMessage, IM429Provider>
	{
		protected override ZString MessageType => Messaging.AISInterchangeTypeList.Codes.IM429;

		protected override ZString MessageText => AISInterchangeProcessorTestHelper.GetStandardUCC5IM429Text();

		protected override ZString MessageFriendlyName => "IM429: Release Notification";

		protected override IM429Processor Processor => new IM429Processor(logger, typeof(Im429));

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISUCC5InboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_Status", LogicalStatusList.Codes.Accepted, messageAttachee.CH_Status);
			AssertEquals("CH_EntryStatus", AISEntryStatusList.Codes.Released, messageAttachee.CH_EntryStatus);

			MessageProcessorNotificationTestHelper.AssertEmail(CommonResStrings.IM429MessageFriendlyName,
				new string[] { "A Release Notification (IM429) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });

			AssertNotNull("CusEntryHeader should have a RLS event logged", messageAttachee.Logs.Find(x => x.SL_SE_NKEvent == Events.ReleasedCode).SingleOrDefault());

			AssertConfirmedDutiesAndTaxesAdded(messageAttachee);
		}

		void AssertConfirmedDutiesAndTaxesAdded(CusEntryHeader cusEntryHeader)
		{
			var entryLine1 = cusEntryHeader.MergedLines[0];
			AssertEquals(1, entryLine1.ConfirmedFees.Count);
			var confirmedFee1 = entryLine1.ConfirmedFees[0];
			AssertEquals("Entry Line 1, Confirmed Fee 1 - CF_ChargeType", "A00", confirmedFee1.CF_ChargeType);
			AssertEquals("Entry Line 1, Confirmed Fee 1 - CF_MethodOfCalculation", "Q", confirmedFee1.CF_MethodOfCalculation);
			AssertEquals("Entry Line 1, Confirmed Fee 1 - CF_BaseValue", 200m, confirmedFee1.CF_BaseValue);
			AssertEquals("Entry Line 1, Confirmed Fee 1 - CF_Rate", 1m, confirmedFee1.CF_Rate);
			AssertEquals("Entry Line 1, Confirmed Fee 1 - CF_ChargeAmount", 2m, confirmedFee1.CF_ChargeAmount);
			AssertEquals("Entry Line 1, Confirmed Fee 1 - CF_MethodOfPayment", "A", confirmedFee1.CF_MethodOfPayment);

			var entryLine2 = cusEntryHeader.MergedLines[1];
			AssertEquals(2, entryLine2.ConfirmedFees.Count);
			var confirmedFee2 = entryLine2.ConfirmedFees[0];
			var confirmedFee3 = entryLine2.ConfirmedFees[1];
			AssertEquals("Entry Line 2, Confirmed Fee 1 - CF_ChargeType", "C00", confirmedFee2.CF_ChargeType);
			AssertEquals("Entry Line 2, Confirmed Fee 1 - CF_MethodOfCalculation", "F", confirmedFee2.CF_MethodOfCalculation);
			AssertEquals("Entry Line 2, Confirmed Fee 1 - CF_BaseValue", 310m, confirmedFee2.CF_BaseValue);
			AssertEquals("Entry Line 2, Confirmed Fee 1 - CF_Rate", 1m, confirmedFee2.CF_Rate);
			AssertEquals("Entry Line 2, Confirmed Fee 1 - CF_ChargeAmount", 3m, confirmedFee2.CF_ChargeAmount);
			AssertEquals("Entry Line 2, Confirmed Fee 1 - CF_MethodOfPayment", "C", confirmedFee2.CF_MethodOfPayment);

			AssertEquals("Entry Line 2, Confirmed Fee 2 - CF_ChargeType", "B00", confirmedFee3.CF_ChargeType);
			AssertEquals("Entry Line 2, Confirmed Fee 2 - CF_MethodOfCalculation", "X", confirmedFee3.CF_MethodOfCalculation);
			AssertEquals("Entry Line 2, Confirmed Fee 2 - CF_BaseValue", 425m, confirmedFee3.CF_BaseValue);
			AssertEquals("Entry Line 2, Confirmed Fee 2 - CF_Rate", 10m, confirmedFee3.CF_Rate);
			AssertEquals("Entry Line 2, Confirmed Fee 2 - CF_ChargeAmount", 42.5m, confirmedFee3.CF_ChargeAmount);
			AssertEquals("Entry Line 2, Confirmed Fee 2 - CF_MethodOfPayment", "B", confirmedFee3.CF_MethodOfPayment);

			AssertEquals("EntryHeader Updated", 47.5m, cusEntryHeader.CH_TotalPaid);
		}

		protected override (JobDeclaration declaration, CusEntryHeader messageAttachee, EDIMessage outgoingMessage, AISUCC5InboundEDIMessage incomingMessage) CreateSetupData()
		{
			var result = base.CreateSetupData();

			var entryHeader = result.messageAttachee;
			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			var existingConfirmedDutiesAndTaxes = entryLine2.ConfirmedFees.AddNew();
			existingConfirmedDutiesAndTaxes.CF_ChargeType = "XXX";

			return result;
		}
	}
}
