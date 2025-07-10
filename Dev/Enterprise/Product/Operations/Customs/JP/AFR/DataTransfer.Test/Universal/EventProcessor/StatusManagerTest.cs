using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2012_11;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal.Testing
{
	class StatusManagerTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestGetReleaseStatus()
		{
			CombineAssertions(() =>
			{
				TestGetReleaseStatusByDiscrepancyCode(MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouse, false, "1", ZString.Empty, AFRBillCustomsStatusList.Codes.NotRegistered);
				TestGetReleaseStatusByDiscrepancyCode(MessagingTypeList.Codes.UpdateAdvanceCargoInformationRegistrationHouse, false, "1", ZString.Empty, AFRBillCustomsStatusList.Codes.NotRegistered);
				TestGetReleaseStatusByDiscrepancyCode(MessagingTypeList.Codes.AdvanceCargoInformationRegistrationMaster, false, "1", ZString.Empty, AFRBillCustomsStatusList.Codes.NotRegistered);
				TestGetReleaseStatusByDiscrepancyCode(MessagingTypeList.Codes.UpdateRegisteredAdvanceCargoInformationMaster, false, "1", ZString.Empty, AFRBillCustomsStatusList.Codes.NotRegistered);

				TestGetReleaseStatusByDiscrepancyCode(MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouse, true, ZString.Empty, ZString.Empty, AFRBillCustomsStatusList.Codes.Registered);
				TestGetReleaseStatusByDiscrepancyCode(MessagingTypeList.Codes.UpdateAdvanceCargoInformationRegistrationHouse, true, ZString.Empty, ZString.Empty, AFRBillCustomsStatusList.Codes.Registered);
				TestGetReleaseStatusByDiscrepancyCode(MessagingTypeList.Codes.AdvanceCargoInformationRegistrationMaster, true, ZString.Empty, ZString.Empty, AFRBillCustomsStatusList.Codes.Registered);
				TestGetReleaseStatusByDiscrepancyCode(MessagingTypeList.Codes.UpdateRegisteredAdvanceCargoInformationMaster, true, ZString.Empty, ZString.Empty, AFRBillCustomsStatusList.Codes.Registered);

				TestGetReleaseStatusByDiscrepancyCode(MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouse, true, "1", ZString.Empty, AFRBillCustomsStatusList.Codes.NL1);
				TestGetReleaseStatusByDiscrepancyCode(MessagingTypeList.Codes.UpdateAdvanceCargoInformationRegistrationHouse, true, "1", ZString.Empty, AFRBillCustomsStatusList.Codes.NL1);
				TestGetReleaseStatusByDiscrepancyCode(MessagingTypeList.Codes.AdvanceCargoInformationRegistrationMaster, true, "1", ZString.Empty, AFRBillCustomsStatusList.Codes.NL2);
				TestGetReleaseStatusByDiscrepancyCode(MessagingTypeList.Codes.UpdateRegisteredAdvanceCargoInformationMaster, true, "1", ZString.Empty, AFRBillCustomsStatusList.Codes.NL2);

				TestGetReleaseStatusByDiscrepancyCode(MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouse, true, "1", AFRBillCustomsStatusList.Codes.Registered, AFRBillCustomsStatusList.Codes.NL1);
				TestGetReleaseStatusByDiscrepancyCode(MessagingTypeList.Codes.UpdateAdvanceCargoInformationRegistrationHouse, true, "1", AFRBillCustomsStatusList.Codes.Registered, AFRBillCustomsStatusList.Codes.NL1);
				TestGetReleaseStatusByDiscrepancyCode(MessagingTypeList.Codes.AdvanceCargoInformationRegistrationMaster, true, "1", AFRBillCustomsStatusList.Codes.Registered, AFRBillCustomsStatusList.Codes.NL2);
				TestGetReleaseStatusByDiscrepancyCode(MessagingTypeList.Codes.UpdateRegisteredAdvanceCargoInformationMaster, true, "1", AFRBillCustomsStatusList.Codes.Registered, AFRBillCustomsStatusList.Codes.NL2);

				TestGetReleaseStatusByDiscrepancyCode(MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouse, true, "X", ZString.Empty, ZString.Empty, "Warning - 'X' is not recognized as a valid Discrepancy Code");
				TestGetReleaseStatusByDiscrepancyCode(MessagingTypeList.Codes.UpdateAdvanceCargoInformationRegistrationHouse, true, "X", ZString.Empty, ZString.Empty, "Warning - 'X' is not recognized as a valid Discrepancy Code");
				TestGetReleaseStatusByDiscrepancyCode(MessagingTypeList.Codes.AdvanceCargoInformationRegistrationMaster, true, "X", ZString.Empty, ZString.Empty, "Warning - 'X' is not recognized as a valid Discrepancy Code");
				TestGetReleaseStatusByDiscrepancyCode(MessagingTypeList.Codes.UpdateRegisteredAdvanceCargoInformationMaster, true, "X", ZString.Empty, ZString.Empty, "Warning - 'X' is not recognized as a valid Discrepancy Code");

				TestGetReleaseStatusByDiscrepancyCode(MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouse, true, "X", AFRBillCustomsStatusList.Codes.Registered, AFRBillCustomsStatusList.Codes.Registered, "Warning - 'X' is not recognized as a valid Discrepancy Code");
				TestGetReleaseStatusByDiscrepancyCode(MessagingTypeList.Codes.UpdateAdvanceCargoInformationRegistrationHouse, true, "X", AFRBillCustomsStatusList.Codes.Registered, AFRBillCustomsStatusList.Codes.Registered, "Warning - 'X' is not recognized as a valid Discrepancy Code");
				TestGetReleaseStatusByDiscrepancyCode(MessagingTypeList.Codes.AdvanceCargoInformationRegistrationMaster, true, "X", AFRBillCustomsStatusList.Codes.Registered, AFRBillCustomsStatusList.Codes.Registered, "Warning - 'X' is not recognized as a valid Discrepancy Code");
				TestGetReleaseStatusByDiscrepancyCode(MessagingTypeList.Codes.UpdateRegisteredAdvanceCargoInformationMaster, true, "X", AFRBillCustomsStatusList.Codes.Registered, AFRBillCustomsStatusList.Codes.Registered, "Warning - 'X' is not recognized as a valid Discrepancy Code");

				TestGetReleaseStatusByEventReference(MessagingTypeList.Codes.RiskAssessmentResult, true, "HLD", ZString.Empty, AFRBillCustomsStatusList.Codes.HLD);
				TestGetReleaseStatusByEventReference(MessagingTypeList.Codes.RiskAssessmentResult, true, "DNL", ZString.Empty, AFRBillCustomsStatusList.Codes.DoNotLoad);
				TestGetReleaseStatusByEventReference(MessagingTypeList.Codes.RiskAssessmentResult, true, "DNU", ZString.Empty, AFRBillCustomsStatusList.Codes.DoNotUnload);

				TestGetReleaseStatusByEventReference(MessagingTypeList.Codes.RiskAssessmentResult, true, "HLD", AFRBillCustomsStatusList.Codes.HLD, AFRBillCustomsStatusList.Codes.HLD);
				TestGetReleaseStatusByEventReference(MessagingTypeList.Codes.RiskAssessmentResult, true, "DNL", AFRBillCustomsStatusList.Codes.HLD, AFRBillCustomsStatusList.Codes.DoNotLoad);
				TestGetReleaseStatusByEventReference(MessagingTypeList.Codes.RiskAssessmentResult, true, "DNU", AFRBillCustomsStatusList.Codes.HLD, AFRBillCustomsStatusList.Codes.DoNotUnload);

				TestGetReleaseStatusByEventReference(MessagingTypeList.Codes.RiskAssessmentResult, true, "XXX", AFRBillCustomsStatusList.Codes.HLD, AFRBillCustomsStatusList.Codes.HLD, "Warning - The Event Reference 'XXX' is not recognized as a valid Risk Assessment Result Code");

				TestGetReleaseStatusByEventReference(MessagingTypeList.Codes.RiskAssessmentCancellation, true, "HLD", ZString.Empty, AFRBillCustomsStatusList.Codes.ReleasedHold);
				TestGetReleaseStatusByEventReference(MessagingTypeList.Codes.RiskAssessmentCancellation, true, "DNL", ZString.Empty, AFRBillCustomsStatusList.Codes.ReleasedDoNotLoad);
				TestGetReleaseStatusByEventReference(MessagingTypeList.Codes.RiskAssessmentCancellation, true, "DNU", ZString.Empty, AFRBillCustomsStatusList.Codes.ReleasedDoNotUnload);

				TestGetReleaseStatusByEventReference(MessagingTypeList.Codes.RiskAssessmentCancellation, true, "HLD", AFRBillCustomsStatusList.Codes.ReleasedHold, AFRBillCustomsStatusList.Codes.ReleasedHold);
				TestGetReleaseStatusByEventReference(MessagingTypeList.Codes.RiskAssessmentCancellation, true, "DNL", AFRBillCustomsStatusList.Codes.ReleasedHold, AFRBillCustomsStatusList.Codes.ReleasedHold, "Warning - The Bill Customs Status doesn't match the Cancellation Detail. Current Bill Status is 'RHD' while the message is cancelling 'RDL'.");
				TestGetReleaseStatusByEventReference(MessagingTypeList.Codes.RiskAssessmentCancellation, true, "DNU", AFRBillCustomsStatusList.Codes.ReleasedHold, AFRBillCustomsStatusList.Codes.ReleasedHold, "Warning - The Bill Customs Status doesn't match the Cancellation Detail. Current Bill Status is 'RHD' while the message is cancelling 'RDU'.");

				TestGetReleaseStatusByEventReference(MessagingTypeList.Codes.RiskAssessmentCancellation, true, "HLD", AFRBillCustomsStatusList.Codes.HLD, AFRBillCustomsStatusList.Codes.ReleasedHold);
				TestGetReleaseStatusByEventReference(MessagingTypeList.Codes.RiskAssessmentCancellation, true, "DNL", AFRBillCustomsStatusList.Codes.DoNotLoad, AFRBillCustomsStatusList.Codes.ReleasedDoNotLoad);
				TestGetReleaseStatusByEventReference(MessagingTypeList.Codes.RiskAssessmentCancellation, true, "DNU", AFRBillCustomsStatusList.Codes.DoNotUnload, AFRBillCustomsStatusList.Codes.ReleasedDoNotUnload);

				TestGetReleaseStatusByEventReference(MessagingTypeList.Codes.RiskAssessmentCancellation, true, "XXX", AFRBillCustomsStatusList.Codes.HLD, AFRBillCustomsStatusList.Codes.HLD, "Warning - The Event Reference 'XXX' is not recognized as a valid Risk Assessment Cancellation Code");
				TestGetReleaseStatusByEventReference(MessagingTypeList.Codes.RiskAssessmentCancellation, true, "HLD", AFRBillCustomsStatusList.Codes.DoNotLoad, AFRBillCustomsStatusList.Codes.DoNotLoad, "Warning - The Bill Customs Status doesn't match the Cancellation Detail. Current Bill Status is 'DNL' while the message is cancelling 'RHD'.");
				TestGetReleaseStatusByEventReference(MessagingTypeList.Codes.RiskAssessmentCancellation, true, "HLD", AFRBillCustomsStatusList.Codes.DoNotUnload, AFRBillCustomsStatusList.Codes.DoNotUnload, "Warning - The Bill Customs Status doesn't match the Cancellation Detail. Current Bill Status is 'DNU' while the message is cancelling 'RHD'.");
			});
		}

		void TestGetReleaseStatusByDiscrepancyCode(ZString messageType, bool isSuccessResponse, ZString discrepancyCode, ZString oldStatus, ZString expectedNewStatus, string expectedLog = null)
		{
			var logger = new TestErrorLogger();
			var eventDataObject = new Event
			{
				DataContext = new DataContext
				{
					Workflow = new Workflow
					{
						ActionPurpose = new CodeDescriptionPair
						{
							Code = messageType
						}
					}
				},
				ContextCollection = new List<Context>(new[]
				{
					new Context { Type = "DiscrepancyCode", Value = discrepancyCode },
				})
			};

			var newStatus = StatusManager.GetReleaseStatus(eventDataObject, logger, oldStatus, isSuccessResponse);
			AssertEquals($"MessageType: {messageType}, IsSuccessResponse: {isSuccessResponse}, DiscrepancyCode: {discrepancyCode}, OldStaus: {oldStatus} -> NewStaus: {expectedNewStatus}", expectedNewStatus, newStatus);

			if (!string.IsNullOrEmpty(expectedLog))
			{
				AssertEquals(expectedLog, logger.Logs);
			}
		}

		void TestGetReleaseStatusByEventReference(ZString messageType, bool isSuccessResponse, ZString eventReference, ZString oldStatus, ZString expectedNewStatus, string expectedLog = null)
		{
			var logger = new TestErrorLogger();
			var eventDataObject = new Event
			{
				DataContext = new DataContext
				{
					Workflow = new Workflow
					{
						ActionPurpose = new CodeDescriptionPair
						{
							Code = messageType
						}
					}
				},
				EventReference = eventReference
			};

			var newStatus = StatusManager.GetReleaseStatus(eventDataObject, logger, oldStatus, isSuccessResponse);
			AssertEquals($"MessageType: {messageType}, IsSuccessResponse: {isSuccessResponse}, EventReference: {eventReference}, OldStaus: {oldStatus} -> NewStaus: {expectedNewStatus}", expectedNewStatus, newStatus);
			if (!string.IsNullOrEmpty(expectedLog))
			{
				AssertEquals(expectedLog, logger.Logs);
			}
		}
	}
}
