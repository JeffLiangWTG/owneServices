using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IE.Business.Testing
{
	class TransactionIDCreatorTest : TestCaseWithFactory
	{
		public void TestProcessingMessageAcknowledgement()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eun);
			helper.CreateNewOrGetExistingCusCodeType(Messaging.UniversalReferenceConstants.RefCusCodeListTypes.RevenueErrorType, "IEROS");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, Messaging.UniversalReferenceConstants.RefCusCodeListTypes.RevenueErrorType, "ROS-222000", "Test Error DESCRIPTION", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var testInterchange = Factory.New<EDIInterchange>();
			testInterchange.EI_InterchangeType = CommonInterchangeTypeList.Codes.TransactionID;
			testInterchange.EI_InterchangeNum = "IE342";
			testInterchange.EI_BodyText = @"<env:Envelope xmlns:env=""http://www.w3.org/2003/05/soap-envelope""><env:Header/><env:Body><ns2:MessageAcknowledgement xmlns:ns2=""http://www.ros.ie/schemas/customs/messageacknowledgement/v1""><ns2:ErrorReference><ns2:ErrorCode>ROS-222000</ns2:ErrorCode></ns2:ErrorReference></ns2:MessageAcknowledgement></env:Body></env:Envelope>";
			Factory.Save();

			var logger = new LoggingInformation();
			((IInboundMessageCreator)new TransactionIDCreator(logger,null,3, CusTransactionNumberTypeList.Codes.IECustoms)).CreateMessagesForInterchange(testInterchange);
			CombineAssertions(() =>
			{
				AssertEquals("No ediMessage generated", 0, testInterchange.ContainedMessages.Count);
				AssertEquals("EI_Status", EDIInterchange.Status.Error, testInterchange.EI_Status);
				var log = testInterchange.Logs.GetAllLogs()[0];

				AssertEquals("SL_SE_NKEvent", Events.ErrorReport.Code, log.SL_SE_NKEvent);
				AssertEquals("SL_Reference", "Error submitting transaction request. Error Code: ROS-222000 - Test Error DESCRIPTION", log.SL_Reference);
				AssertContains("Logs", "Processing Interchange (Type:TID, Number:IE342).\r\n\tInterchange #IE342: Status set to 'ERR' due to the following error: Error submitting transaction request. Error Code: ROS-222000 - Test Error DESCRIPTION", string.Join("\r\n", logger.UserLogStrings.Cast<string>()));
			});
		}
	}
}
