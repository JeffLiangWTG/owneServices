using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public static class CargoHelper
	{
		public const string CanberraUNLOCO = "AUCBR";

		public static ZString GetACSAQISStatusReason(ZString statusText)
		{
			var details = statusText.Replace("\n", "").Split('\r').Where(s => s.Contains("ACS/AQIS", StringComparison.OrdinalIgnoreCase)).ToList();
			if (!details.IsNullOrEmpty())
			{
				var detailText = new ZStringBuilder();
				foreach (var detail in details)
				{
					detailText.Append(detail.Substring(detail.IndexOf(":", StringComparison.OrdinalIgnoreCase) + 2));
				}
				return detailText.ToStringWithDelimiterBetweenAppends(" ");
			}
			return ZString.Empty;
		}

		public static ZString GetConsigneeBusinessNumber(OrgHeader consignee)
		{
			var result = ZString.Empty;

			if (consignee != null && !consignee.IsUnmatchedOrganisation())
			{
				var abnRegNo = consignee.CustomsCodes.GetCustomsRegNo(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, Core.Constants.CountryCodes.Australia).Replace(" ", "").Replace("/", "").Replace("\\", "");
				var abn = abnRegNo.Left(11);
				var cac = consignee.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CreditAgencyCode, Core.Constants.CountryCodes.Australia).Left(3);
				if (cac.IsEmpty)
				{
					cac = abnRegNo.SubstringSafe(11, 3);
				}

				result = !abn.IsEmpty && !cac.IsEmpty ? new ZString(abn + "/" + cac) : abn;
			}

			return result;
		}

		public static ZString GetIdentifier(OrgHeader orgHeader, OrgAddress orgAddress)
		{
			var result = ZString.Empty;

			if (orgHeader != null && !orgHeader.IsUnmatchedOrganisation())
			{
				result = orgHeader.GetCustomsClientID(orgAddress).Left(11);
			}

			return result;
		}

		public static ZString GetTraderIdentificationNumber(OrgHeader orgHeader)
		{
			var result = ZString.Empty;

			if (orgHeader != null && !orgHeader.IsUnmatchedOrganisation())
			{
				result = orgHeader.CustomsCodes.GetCustomsRegNo(OrgCusCode.AustraliaCodeTypes.TraderIdentificationNumber, Core.Constants.CountryCodes.Australia);
			}

			return result;
		}

		public static ZString GetConsignorVendor(OrgHeader consignor)
		{
			var result = ZString.Empty;

			if (consignor != null && !consignor.IsUnmatchedOrganisation())
			{
				result = consignor.CustomsCodes.GetCustomsRegNo(OrgCusCode.AustraliaCodeTypes.ARN, Core.Constants.CountryCodes.Australia).Left(12);
				if (result.IsEmpty)
				{
					result = consignor.CustomsCodes.GetCustomsRegNo(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, Core.Constants.CountryCodes.Australia).KeepChars(ABNValidation.ValidChars).Left(14);
				}
			}

			return result;
		}

		public static ZString GetConsignorVendorId(OrganizationAddress consignorAddressData)
		{
			return consignorAddressData?.RegistrationNumberCollection?.FirstOrDefault(IsAUARNVendorIdentifier)?.Value.GetValueOrDefault()
				?? consignorAddressData?.RegistrationNumberCollection?.FirstOrDefault(IsAUABNVendorIdentifier)?.Value.GetValueOrDefault()
				?? ZString.Empty;
		}

		static bool IsAUARNVendorIdentifier(RegistrationNumber x)
		{
			return (x.Type.GetCodeAsUpperCase() == OrgCusCode.AustraliaCodeTypes.ARN) && x.CountryOfIssue.GetCodeAsUpperCase() == Core.Constants.CountryCodes.Australia;
		}

		static bool IsAUABNVendorIdentifier(RegistrationNumber x)
		{
			return (x.Type.GetCodeAsUpperCase() == OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber) && x.CountryOfIssue.GetCodeAsUpperCase() == Core.Constants.CountryCodes.Australia;
		}

		public static void CheckBusinessNumberOrIdentifierShouldBeEntered(ZPropertyInfo propertyInfo, ZString businessNumber, ZString identifier)
		{
			if (!businessNumber.IsEmpty && !identifier.IsEmpty)
			{
				propertyInfo.AddMessageError(OnlyOneOfABNCACCombinationORImporterIdentifierShouldBeEntered);
			}
		}

		public static ZBool IsUnmatchedOrganisation(this OrgHeader orgHeader)
		{
			return orgHeader != null && orgHeader.PK == OrgHeader.UnmatchedOrganisationPK;
		}

		public static ZDateTime CalculateScheduledMessagesSendTime(BusinessObjectFactory factory, ZString transportMode, ZString dischargePort, ZDateTime arrivalDate)
		{
			var scheduledSendTime = ZDateTime.Empty;

			if (arrivalDate.IsValid)
			{
				int whiteTimeStartHour = 6; // 6AM
				int whiteTimeEndHour = 19;  // 7PM
				int lateTimeframe = transportMode == Core.Constants.TransportCodes.Air ? AUCustomsDataRegistry.Instance.AirMandatoryLatestCargoReportingTimeframe.Value : AUCustomsDataRegistry.Instance.SeaMandatoryLatestCargoReportingTimeframe.Value;

				var portCBR = factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, CanberraUNLOCO);
				var currentCBRTime = portCBR.TimeZoneSet.GetCalculationTimeZone().ToLocalTime(ZDateTime.UtcNow.ToDateTime()); // portCBR.LocationDateTime is cached so it will return the same time over and over
				var arrivalCBRTime = EnvProxy.Instance.Time.GetTimeInOneZoneFromTimeInAnotherZone(dischargePort, arrivalDate.ToDateTime(), CanberraUNLOCO);

				if (currentCBRTime.AddHours(lateTimeframe) < arrivalCBRTime)
				{
					scheduledSendTime = currentCBRTime;

					int maxHoursBeforeArrival = transportMode == Core.Constants.TransportCodes.Air ? 48 : 0;
					if (maxHoursBeforeArrival > 0)
					{
						var earliestSendTime = arrivalCBRTime.AddHours(-maxHoursBeforeArrival);
						if (scheduledSendTime < earliestSendTime)
						{
							scheduledSendTime = earliestSendTime;
						}
					}

					if (scheduledSendTime.Hour < whiteTimeEndHour
					 && scheduledSendTime.Hour >= whiteTimeStartHour
					 && scheduledSendTime.DayOfWeek != DayOfWeek.Saturday
					 && scheduledSendTime.DayOfWeek != DayOfWeek.Sunday)
					{
						scheduledSendTime = scheduledSendTime.Date.AddHours(whiteTimeEndHour);
					}

					if (scheduledSendTime.AddHours(lateTimeframe) > arrivalCBRTime)
					{
						scheduledSendTime = ZDateTime.Empty;
					}
				}
			}

			return scheduledSendTime;
		}

		public static OrgAddress GetMatchingOrgAddressUsingCodeOrAddress1(this OrganizationAddress orgAddressData, BusinessObjectFactory factory)
		{
			OrgAddress result = null;
			ZString orgCode = orgAddressData.OrganizationCode.GetValueOrDefault();
			if (!orgCode.IsEmpty)
			{
				var orgHeader = factory.LoadFromUniqueKey<OrgHeader>(OrgHeaderSchema.OH_Code, orgCode);
				if (orgHeader != null)
				{
					var addressCode = orgAddressData.AddressShortCode.GetValueOrDefault();
					if (!addressCode.IsEmpty)
					{
						var addressQuery = new ZQuery(OrgAddressSchema.OA_OH, orgHeader.PK);
						addressQuery.AddToFilter(OrgAddressSchema.OA_Code, addressCode);
						result = factory.LoadTop1<OrgAddress>(addressQuery);
					}

					var address1 = orgAddressData.Address1.GetValueOrDefault();
					if (result == null && !address1.IsEmpty)
					{
						var addressQuery = new ZQuery(OrgAddressSchema.OA_OH, orgHeader.PK);
						addressQuery.AddToFilter(OrgAddressSchema.OA_Address1, address1);
						result = factory.LoadTop1<OrgAddress>(addressQuery);
					}

					if (result == null && addressCode.IsEmpty && address1.IsEmpty)
					{
						result = orgHeader.MainAddress;
					}
				}
			}
			return result;
		}

		public static string ScheduledMessagesConfirmationText(ZString deferredScheduledMessagesDateTimeString, string reportType)
		{
			if (deferredScheduledMessagesDateTimeString == "IMMEDIATELY")
			{
				return string.Format("{0} are within the late cargo reporting time frame, (or it is currently out of hours), so will be sent IMMEDIATELY, for all house bills on this job that have not already been reported.", reportType);
			}
			else
			{
				return string.Format("{0} will be sent in the background by the process controller {1}, for all house bills of this job that have not already been reported.", reportType, deferredScheduledMessagesDateTimeString);
			}
		}

		public static ZString DeferredScheduledMessagesDateTimeString(ZDateTime deferredScheduledMessagesDateTime)
		{
			return deferredScheduledMessagesDateTime.IsEmpty ? "IMMEDIATELY" : "at " + deferredScheduledMessagesDateTime.ToString("dd-MMM-yyyy HH:mm");
		}

		public static JobDocAddress GetDefaultConsigneeAddress(JobDocAddress consigneeAddress, JobDocAddress deliveryAddress, bool isAir)
		{
			JobDocAddress defaultAddress = null;
			var miscServ = consigneeAddress.Organisation?.MiscServ;
			var consigneeDefaultOption = (isAir ? miscServ?.OM_IMAirCargoReportDefaultConsignee : miscServ?.OM_IMSeaCargoReportDefaultConsignee) ?? ConsigneeDefaultOptionList.Codes.Default;
			var registryDefaultOption = isAir ? new ZString(AUCustomsDataRegistry.Instance.DefaultAirCargoConsignee.Value)
									: new ZString(AUCustomsDataRegistry.Instance.DefaultSeaCargoConsignee.Value);
			var defaultOption = consigneeDefaultOption == ConsigneeDefaultOptionList.Codes.Default ? registryDefaultOption : consigneeDefaultOption;
			switch (defaultOption)
			{
				case ConsigneeDefaultOptionList.Codes.Consignee:
					defaultAddress = consigneeAddress;
					break;
				case ConsigneeDefaultOptionList.Codes.DeliverTo:
					defaultAddress = deliveryAddress;
					break;
			}
			return defaultAddress;
		}

		const string OnlyOneOfABNCACCombinationORImporterIdentifierShouldBeEntered = "Only one of ABN/CAC combination OR Importer Identifier should be entered.";
	}
}
