using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CargoWise.Types;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal
{
	public static class StatusManager
	{
		public static ZString GetReleaseStatus(IXmlEventValueObject eventDataObject, IXmlImportLogger logger, ZString oldStatus, bool isSuccessResponse)
		{
			var dataContext = eventDataObject.DataContext;
			var messageType = dataContext?.ActionPurposeCode ?? ZString.Empty;

			var result = oldStatus;
			var newStatus = GetNewReleaseStatus(messageType, logger, eventDataObject, isSuccessResponse);

			var newStatusPriority = GetReleaseStatusPriority(newStatus);
			var oldStatusPriority = GetReleaseStatusPriority(oldStatus);

			if (oldStatusPriority > newStatusPriority || newStatus == oldStatus)
			{
				result = oldStatus;
			}
			else if (oldStatusPriority < newStatusPriority)
			{
				result = newStatus;
			}
			else if (oldStatusPriority == newStatusPriority)
			{
				if (oldStatusPriority < ReleaseStatusPriority.High)
				{
					result = newStatus;
				}
				else if (oldStatusPriority == ReleaseStatusPriority.High)
				{
					if (AreHighStatusesExchengeable(oldStatus, newStatus))
					{
						result = newStatus;
					}
					else
					{
						logger.Log(Integration.LogType.Warning, string.Format(CultureInfo.InvariantCulture, "The Bill Customs Status doesn't match the Cancellation Detail. Current Bill Status is '{0}' while the message is cancelling '{1}'.", oldStatus, newStatus));
					}
				}
			}

			return result;
		}

		static bool AreHighStatusesExchengeable(ZString oldStatus, ZString newStatus)
		{
			bool result;
			switch (newStatus)
			{
				case AFRBillCustomsStatusList.Codes.ReleasedHold:
					result = oldStatus == AFRBillCustomsStatusList.Codes.HLD;
					break;
				case AFRBillCustomsStatusList.Codes.ReleasedDoNotLoad:
					result = oldStatus == AFRBillCustomsStatusList.Codes.DoNotLoad;
					break;
				case AFRBillCustomsStatusList.Codes.ReleasedDoNotUnload:
					result = oldStatus == AFRBillCustomsStatusList.Codes.DoNotUnload;
					break;
				default:
					result = true;
					break;
			}
			return result;
		}

		static ZString GetNewReleaseStatus(ZString messageType, IXmlImportLogger logger, IXmlEventValueObject eventDataObject, bool isSuccessResponse)
		{
			var newStatus = AFRBillCustomsStatusList.Codes.NotRegistered;
			if (isSuccessResponse)
			{
				newStatus = AFRBillCustomsStatusList.Codes.Registered;

				ZString discrepancyCode;
				switch (messageType)
				{
					case MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouse:
					case MessagingTypeList.Codes.UpdateAdvanceCargoInformationRegistrationHouse:
						discrepancyCode = eventDataObject.Context.DiscrepancyCode;
						if (!discrepancyCode.IsEmpty)
						{
							newStatus = GetNVOCCReleaseStatusFromDiscrepancyCode(logger, discrepancyCode);
						}
						break;
					case MessagingTypeList.Codes.AdvanceCargoInformationRegistrationMaster:
					case MessagingTypeList.Codes.UpdateRegisteredAdvanceCargoInformationMaster:
						discrepancyCode = eventDataObject.Context.DiscrepancyCode;
						if (!discrepancyCode.IsEmpty)
						{
							newStatus = GetVOCCReleaseStatusFromDiscrepancyCode(logger, discrepancyCode);
						}
						break;
					case MessagingTypeList.Codes.RiskAssessmentResult:
						newStatus = GetRiskAssessmentResultStatus(logger, eventDataObject.EventReference);
						break;
					case MessagingTypeList.Codes.RiskAssessmentCancellation:
						newStatus = GetRiskAssessmentCancellationStatus(logger, eventDataObject.EventReference);
						break;
				}
			}
			return newStatus;
		}

		static ZString GetNVOCCReleaseStatusFromDiscrepancyCode(IXmlImportLogger logger, ZString discrepancyCode)
		{
			var result = ZString.Empty;
			switch (discrepancyCode)
			{
				case "1":
					result = AFRBillCustomsStatusList.Codes.NL1;
					break;
				case "2":
					result = AFRBillCustomsStatusList.Codes.NL2;
					break;
				default:
					logger.Log(Integration.LogType.Warning, string.Format(CultureInfo.InvariantCulture, "'{0}' is not recognized as a valid Discrepancy Code", discrepancyCode));
					break;
			}
			return result;
		}

		static ZString GetVOCCReleaseStatusFromDiscrepancyCode(IXmlImportLogger logger, ZString discrepancyCode)
		{
			var result = ZString.Empty;
			switch (discrepancyCode)
			{
				case "1":
					result = AFRBillCustomsStatusList.Codes.NL2;
					break;
				case "2":
					result = AFRBillCustomsStatusList.Codes.NL1;
					break;
				default:
					logger.Log(Integration.LogType.Warning, string.Format(CultureInfo.InvariantCulture, "'{0}' is not recognized as a valid Discrepancy Code", discrepancyCode));
					break;
			}
			return result;
		}

		static ZString GetRiskAssessmentResultStatus(IXmlImportLogger logger, ZString eventReference)
		{
			var result = ZString.Empty;
			switch (eventReference)
			{
				case AFRBillCustomsStatusList.Codes.DoNotLoad:
				case AFRBillCustomsStatusList.Codes.DoNotUnload:
				case AFRBillCustomsStatusList.Codes.HLD:
					result = eventReference;
					break;
				default:
					logger.Log(Integration.LogType.Warning, string.Format(CultureInfo.InvariantCulture, "The Event Reference '{0}' is not recognized as a valid Risk Assessment Result Code", eventReference));
					break;
			}
			return result;
		}

		static ZString GetRiskAssessmentCancellationStatus(IXmlImportLogger logger, ZString eventReference)
		{
			var result = ZString.Empty;
			switch (eventReference)
			{
				case AFRBillCustomsStatusList.Codes.DoNotLoad:
					result = AFRBillCustomsStatusList.Codes.ReleasedDoNotLoad;
					break;
				case AFRBillCustomsStatusList.Codes.DoNotUnload:
					result = AFRBillCustomsStatusList.Codes.ReleasedDoNotUnload;
					break;
				case AFRBillCustomsStatusList.Codes.HLD:
					result = AFRBillCustomsStatusList.Codes.ReleasedHold;
					break;
				default:
					logger.Log(Integration.LogType.Warning, string.Format(CultureInfo.InvariantCulture, "The Event Reference '{0}' is not recognized as a valid Risk Assessment Cancellation Code", eventReference));
					break;
			}
			return result;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		static ReleaseStatusPriority GetReleaseStatusPriority(ZString releaseStatus)
		{
			var result = ReleaseStatusPriority.Low;
			switch (releaseStatus)
			{
				case "":
				case AFRBillCustomsStatusList.Codes.NotRegistered:
					result = ReleaseStatusPriority.Low;
					break;
				case AFRBillCustomsStatusList.Codes.Registered:
				case AFRBillCustomsStatusList.Codes.NL1:
				case AFRBillCustomsStatusList.Codes.NL2:
					result = ReleaseStatusPriority.Medium;
					break;
				case AFRBillCustomsStatusList.Codes.HLD:
				case AFRBillCustomsStatusList.Codes.DoNotLoad:
				case AFRBillCustomsStatusList.Codes.DoNotUnload:
				case AFRBillCustomsStatusList.Codes.ReleasedHold:
				case AFRBillCustomsStatusList.Codes.ReleasedDoNotLoad:
				case AFRBillCustomsStatusList.Codes.ReleasedDoNotUnload:
					result = ReleaseStatusPriority.High;
					break;
			}
			return result;
		}

		enum ReleaseStatusPriority
		{
			Low = 0,
			Medium = 1,
			High = 2
		}
	}
}
