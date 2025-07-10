using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(CustomsAndExciseReportOutboundMessageValidation))]
	public class CustomsAndExciseReportOutboundMessageValidationTest : BusinessObjectValidationTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();

			reportMessage = Factory.New<CustomsAndExciseReportOutboundMessage>();
		}
		protected CustomsAndExciseReportOutboundMessage reportMessage;

		public void TestCheckEM_MessageTypeValidationStrategy()
		{
			const string message = "Report Type must be provided.";
			var validation = new CustomsAndExciseReportOutboundMessageValidation(reportMessage);

			CombineAssertions(() =>
			{
				reportMessage.EM_MessageType = null;
				validation.ValidateEM_MessageType();
				AssertHasError("With applicable error", reportMessage.EM_MessageTypeInfo, message);

				reportMessage.EM_MessageType = CustomsAndExciseReportTypeList.Codes.PSR;
				validation.ValidateEM_MessageType();
				AssertNoError("Without applicable error", reportMessage.EM_MessageTypeInfo, message);
			});
		}

		public void TestCheckMessageDate()
		{
			const string message = "Date must be provided when Message Type is not UDR or BAL.";
			var validation = new CustomsAndExciseReportOutboundMessageValidation(reportMessage);

			CombineAssertions(() =>
			{
				reportMessage.EM_MessageType = CustomsAndExciseReportTypeList.Codes.UDR;
				validation.ValidateMessageDate();
				AssertNoError("Without applicable error: UDR", reportMessage.MessageDateInfo, message);

				reportMessage.EM_MessageType = CustomsAndExciseReportTypeList.Codes.PSR;
				validation.ValidateMessageDate();
				AssertHasError("With applicable error: PSR", reportMessage.MessageDateInfo, message);

				reportMessage.EM_MessageType = CustomsAndExciseReportTypeList.Codes.BAL;
				validation.ValidateMessageDate();
				AssertNoError("Without applicable error: BAL", reportMessage.MessageDateInfo, message);
			});
		}
	}
}
