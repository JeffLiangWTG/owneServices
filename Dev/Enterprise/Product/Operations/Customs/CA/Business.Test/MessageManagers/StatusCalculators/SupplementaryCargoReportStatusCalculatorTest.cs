using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageProcessors.Testing;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.MessageBuilders.Testing;
using Enterprise.Customs.Common.Shared;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	[TestedType(typeof(SupplementaryCargoReportStatusCalculator))]
	sealed class SupplementaryCargoReportStatusCalculatorTest : EDIFACTMessageStatusCalculatorTestCase
	{
		public void TestMainEntryStatusCodesAreSame()
		{
			AssertEquals("Clear", EntryStatusList.Codes.Clear, SupplementaryCargoReportJobStatusList.Codes.Clear);
			AssertEquals("Error", EntryStatusList.Codes.Error, SupplementaryCargoReportJobStatusList.Codes.Error);
			AssertEquals("Cancelled", EntryStatusList.Codes.Cancelled, SupplementaryCargoReportJobStatusList.Codes.Cancelled);
		}

		public void TestGetImpedementJobStatus()
		{
			var calculator1 = (SupplementaryCargoReportStatusCalculatorForTesting)calculator;
			AssertEquals("ImpedementJobStatus", SupplementaryCargoReportJobStatusList.Codes.DoNotLoad, calculator1.GetImpedementJobStatus_Exposed("5"));
			AssertEquals("ImpedementJobStatus", SupplementaryCargoReportJobStatusList.Codes.Hold, calculator1.GetImpedementJobStatus_Exposed("6"));
			AssertEquals("ImpedementJobStatus", SupplementaryCargoReportJobStatusList.Codes.DoNotUnload, calculator1.GetImpedementJobStatus_Exposed("7"));
			AssertEquals("ImpedementJobStatus", SupplementaryCargoReportJobStatusList.Codes.RACleared, calculator1.GetImpedementJobStatus_Exposed("1"));
			AssertEquals("ImpedementJobStatus", SupplementaryCargoReportJobStatusList.Codes.Unknown, calculator1.GetImpedementJobStatus_Exposed(""));
		}

		public override void TestIsClear()
		{
			Assert("IsClear", calculator.IsClear(SupplementaryCargoReportJobStatusList.Codes.Clear));
			Assert("IsClear", calculator.IsClear(SupplementaryCargoReportJobStatusList.Codes.RACleared));
			Assert("IsClear", !calculator.IsClear(SupplementaryCargoReportJobStatusList.Codes.Cancelled));
			Assert("IsClear", !calculator.IsClear(""));
		}

		public override void TestIsLodged()
		{
			Assert("IsLodged", calculator.IsLodged(SupplementaryCargoReportJobStatusList.Codes.Clear));
			Assert("IsLodged", calculator.IsLodged(SupplementaryCargoReportJobStatusList.Codes.RACleared));
			Assert("IsLodged", calculator.IsLodged(SupplementaryCargoReportJobStatusList.Codes.DoNotLoad));
			Assert("IsLodged", calculator.IsLodged(SupplementaryCargoReportJobStatusList.Codes.Hold));
			Assert("IsLodged", calculator.IsLodged(SupplementaryCargoReportJobStatusList.Codes.Unknown));
			Assert("IsLodged", calculator.IsLodged(SupplementaryCargoReportJobStatusList.Codes.RACleared));
			Assert("IsLodged", !calculator.IsLodged(SupplementaryCargoReportJobStatusList.Codes.Cancelled));
			Assert("IsLodged", !calculator.IsLodged(""));
		}

		#region TestCalculatedJobStatus

		public override void TestCalculatedJobStatus()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse house = oceanBill.HouseBills.AddNew();
			AssertEquals("No status", ZString.Empty, calculator.CalculatedJobStatus(house));

			AddEDIMessage(house, "50", SupplementaryCargoReportResponseMessageProcessorTest.ValidationErrorMessageText);
			AssertEquals("Error", SupplementaryCargoReportJobStatusList.Codes.Error, calculator.CalculatedJobStatus(house));

			AddEDIMessage(house, "60", SupplementaryCargoReportResponseMessageProcessorTest.ContentAcceptedMessageText);
			AssertEquals("Validated", SupplementaryCargoReportJobStatusList.Codes.Validated, calculator.CalculatedJobStatus(house));

			AddEDIMessage(house, "100", SupplementaryCargoReportResponseMessageProcessorTest.MatchedMessageText);
			AssertEquals("Matched", SupplementaryCargoReportJobStatusList.Codes.Clear, calculator.CalculatedJobStatus(house));

			AddEDIMessage(house, "110", SupplementaryCargoReportResponseMessageProcessorTest.NotMatchedMessageText);
			AssertEquals("Not Matched", SupplementaryCargoReportJobStatusList.Codes.NotMatched, calculator.CalculatedJobStatus(house));

			AddEDIMessage(house, "120", SupplementaryCargoReportResponseMessageProcessorTest.RiskAssessmentNoticeMessageText);
			AssertEquals("RA", SupplementaryCargoReportJobStatusList.Codes.DoNotLoad, calculator.CalculatedJobStatus(house));

			AddEDIMessage(house, "130", SupplementaryCargoReportResponseMessageProcessorTest.ContentAcceptedMessageText);
			AssertEquals("RA Still", SupplementaryCargoReportJobStatusList.Codes.DoNotLoad, calculator.CalculatedJobStatus(house));

			AddEDIMessage(house, "140", SupplementaryCargoReportResponseMessageProcessorTest.ContentAcceptedMessageText, MessageSubTypeCodes.Codes.Cancellation);
			AssertEquals("Cancelled", SupplementaryCargoReportJobStatusList.Codes.Cancelled, calculator.CalculatedJobStatus(house));

			AddEDIMessage(house, "150", SupplementaryCargoReportResponseMessageProcessorTest.ContentAcceptedMessageText);
			AssertEquals("Validated", SupplementaryCargoReportJobStatusList.Codes.Validated, calculator.CalculatedJobStatus(house));

			AddEDIMessage(house, "160", SupplementaryCargoReportResponseMessageProcessorTest.MatchedMessageText);
			AssertEquals("Matched", SupplementaryCargoReportJobStatusList.Codes.Clear, calculator.CalculatedJobStatus(house));

			AddEDIMessage(house, "170", SupplementaryCargoReportResponseMessageProcessorTest.RiskAssessmentNoticeMessageText);
			AssertEquals("RA", SupplementaryCargoReportJobStatusList.Codes.DoNotLoad, calculator.CalculatedJobStatus(house));

			AddEDIMessage(house, "180", SupplementaryCargoReportResponseMessageProcessorTest.ContentAcceptedMessageText);
			AssertEquals("RA Still", SupplementaryCargoReportJobStatusList.Codes.DoNotLoad, calculator.CalculatedJobStatus(house));

			AddEDIMessage(house, "190", SupplementaryCargoReportResponseMessageProcessorTest.ContentAcceptedMessageText, MessageSubTypeCodes.Codes.Cancellation);
			AssertEquals("Cancelled", SupplementaryCargoReportJobStatusList.Codes.Cancelled, calculator.CalculatedJobStatus(house));

			AddEDIMessage(house, "200", SupplementaryCargoReportResponseMessageProcessorTest.NotMatchedMessageText);
			AssertEquals("Cancelled", SupplementaryCargoReportJobStatusList.Codes.Cancelled, calculator.CalculatedJobStatus(house));

			AddEDIMessage(house, "210", SupplementaryCargoReportResponseMessageProcessorTest.ContentAcceptedMessageText);
			AssertEquals("Validated", SupplementaryCargoReportJobStatusList.Codes.Validated, calculator.CalculatedJobStatus(house));

			AddEDIMessage(house, "220", SupplementaryCargoReportResponseMessageProcessorTest.ContentAcceptedMessageText, MessageSubTypeCodes.Codes.Cancellation);
			AssertEquals("Cancelled", SupplementaryCargoReportJobStatusList.Codes.Cancelled, calculator.CalculatedJobStatus(house));

			AddEDIMessage(house, "220", SupplementaryCargoReportResponseMessageProcessorTest.ContentAcceptedMessageText);
			AssertEquals("Validated", SupplementaryCargoReportJobStatusList.Codes.Validated, calculator.CalculatedJobStatus(house));

			AddEDIMessage(house, "230", SupplementaryCargoReportResponseMessageProcessorTest.NotMatchedMessageText);
			AssertEquals("Not Matched", SupplementaryCargoReportJobStatusList.Codes.NotMatched, calculator.CalculatedJobStatus(house));
		}

		void AddEDIMessage(CusSCAHouse house, string messageNum, string messageText)
		{
			AddEDIMessage(house, messageNum, messageText, MessageSubTypeCodes.Codes.Original);
		}

		void AddEDIMessage(CusSCAHouse house, string messageNum, string messageText, string messageSubType)
		{
			EDIMessage result = (EDIMessage)house.Messages.AddNew();
			result.EM_ApplicationCode = Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.CAACI;
			result.EM_MessageType = MessageTypeList.Codes.SupplementaryCargoReport;
			result.EM_MessageSubType = messageSubType;
			result.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Receive;
			result.EM_MessageNum = messageNum;
			result.EM_MessageText = messageText.Replace("\r\n", "");
		}

		#endregion

		public override void TestMessageTypeDescription()
		{
			AssertEquals("MessageTypeDescription", MessageTypeList.Descriptions.SupplementaryCargoReport, calculator.MessageTypeDescription);
		}

		protected override EDIFACTMessageStatusCalculator GetCalculator()
		{
			return new SupplementaryCargoReportStatusCalculatorForTesting();
		}

		#region SupplementaryCargoReportStatusCalculatorForTesting

		class SupplementaryCargoReportStatusCalculatorForTesting : SupplementaryCargoReportStatusCalculator
		{
			public ZString GetImpedementJobStatus_Exposed(ZString impedementReasonCode)
			{
				return GetImpedementJobStatus(impedementReasonCode);
			}
		}

		#endregion
	}
}
