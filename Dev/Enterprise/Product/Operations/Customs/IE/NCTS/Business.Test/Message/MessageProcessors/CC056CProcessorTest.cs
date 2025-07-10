using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC056C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC056CProcessor))]
	class CC056CProcessorTest : NCTSDepartureWithGuaranteeMessageProcessorAbstractTest<CC056CProcessor, NCTSInboundEDIMessage, NCTSOutboundEDIMessage, CC056CProvider>
	{
		public void TestGuaranteeTransactionStatus_013()
		{
			TestGuaranteeTransactionStatusNotUpdated(NCTSOutgoingDepartureMessageTypeList.Codes.DeclarationAmendment);
		}

		public void TestGuaranteeTransactionStatus_014()
		{
			TestGuaranteeTransactionStatusNotUpdated(NCTSOutgoingDepartureMessageTypeList.Codes.DeclarationInvalidationRequest);
		}

		void TestGuaranteeTransactionStatusNotUpdated(string businessRejectionType)
		{
			var messageText = InterchangeProcessorTestHelper.GetMailboxItemText("5B625BFB-BF2A-491C-8C23-BDBB2DECA438", InterchangeProcessorTestHelper.GetStandardCC056CText(businessRejectionType: businessRejectionType), "ce45c655-c780-43be-94f8-69ef936ea871", includeResponseWrap: false);
			var (nctsHeader, _, _, incomingMessage) = CreateSetupData(messageText);
			var movementHeader = nctsHeader.MovementHeader;
			using (incomingMessage.Factory.AddDisposableService())
			{
				ResponseDetail responseDetail = ResponseMessageDetails.GetResponseDetail(incomingMessage.EM_ApplicationCode, incomingMessage.EM_MessageType, incomingMessage.EM_MessageSubType);
				Type processorType = responseDetail.ProcessorType;
				var val2 = (CC056CProcessor)Activator.CreateInstance(processorType, logger, responseDetail.XmlObjectType);
				val2.PreProcessMessage(incomingMessage);
				val2.ProcessMessage(incomingMessage);
				AssertTransaction(movementHeader, 2, -25000m, -25000m, Customs.Business.PermitTransactionStatusList.Codes.Pending);
			}
		}

		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE056;

		protected override ZString MessageText => InterchangeProcessorTestHelper.GetStandardCC056CText();

		protected override ZString MessageFriendlyName => "CC056C: REJECTION FROM OFFICE OF DEPARTURE";

		protected override CC056CProcessor Processor => new CC056CProcessor(logger, typeof(Cc056CType));

		protected override void AssertProcessResultCore(NctsDepartureMovementHeader movementHeader, NCTSInboundEDIMessage incomingMessage)
		{
			AssertEquals("BM_MessageStatus", LogicalStatusList.Codes.Invalid, movementHeader.BM_MessageStatus);
			AssertTransaction(movementHeader, 2, -25000m, -25000m, Customs.Business.PermitTransactionStatusList.Codes.Deleted);
			AssertMessageInterpretation(incomingMessage, @"A Rejection From Office of Departure (IE056) message has been received for Job B00001000.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>19MRNCC056C0123456</td></tr><tr><td>Rejection Date and Time</td><td>31-Jan-23 10:22</td></tr><tr><td>Rejection Code</td><td>7 - Rejection Code 7</td></tr><tr><td>Rejection Reason</td><td>Guarantee not valid for this customs territory</td></tr><tr><td>Error Pointer</td><td>Error Pointer 1</td></tr><tr><td>Error Code</td><td>12</td></tr><tr><td>Error Reason</td><td>REASON1</td></tr></table>");
			MessageProcessorNotificationTestHelper.AssertEmail(
				incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Rejection From Office of Departure (IE056) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}

		protected override (NctsHeader declaration, NctsDepartureMovementHeader messageAttachee, EDIMessage outgoingMessage, NCTSInboundEDIMessage incomingMessage) CreateSetupData(string incomingMessageText = null)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eun);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL226, "CL226");
			helper.CreateNewOrGetExistingCusCodeList(
				dataGroupingCode: Core.Constants.CountryCodes.Ireland,
				codeType: UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL226,
				code: "7",
				description: "Rejection Code 7",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);

			return base.CreateSetupData(incomingMessageText);
		}
	}
}
