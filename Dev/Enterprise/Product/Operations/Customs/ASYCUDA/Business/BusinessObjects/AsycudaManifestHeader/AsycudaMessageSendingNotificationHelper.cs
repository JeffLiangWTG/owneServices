using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeList;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaMessageSendingNotificationHelper : BaseMessageSendingNotificationHelper
	{
		public AsycudaMessageSendingNotificationHelper(AsycudaManifestHeader header) : base(header)
		{
		}

		public override ZString GetNotifications()
		{
			var countryCode = header.AMA_RN_NKCountry;
			if (IsCustomsOfficeValidationApplicable(countryCode))
			{
				var countryName = header.CountryName.IsEmpty ? countryCode : header.CountryName;
				return ValidationConstants.MissingCustomsOffice(countryName);
			}

			if (GlbStaff.CurrentUser.GS_EmailAddress.IsEmpty && !ZZDatabaseValidationHelper.IsMandatoryForOneCountry(header.Factory, countryCode, ManifestValidationRuleCodes.CurrentUserEmailAddress).IsEmpty)
			{
				return ValidationConstants.MissingEmailAddress;
			}
			return ZString.Empty;
		}

		protected virtual bool IsCustomsOfficeValidationApplicable(ZString countryCode) => header.AMA_CustomsOffice.IsEmpty && !ZZDatabaseValidationHelper.IsMandatoryForOneCountry(header.Factory, countryCode, ManifestValidationRuleCodes.OfficeCode).IsEmpty;

		public override IEnumerable<ZString> GetConfirmations()
		{
			var result = new List<ZString>();

			var confirmation = GetConfirmationForBangladeshManifestIfNoCCDAndCCCRegistrationNumber();
			if (!confirmation.IsEmpty)
			{
				result.Add(confirmation);
			}

			return result;
		}

		ZString GetConfirmationForBangladeshManifestIfNoCCDAndCCCRegistrationNumber()
		{
			var result = ZString.Empty;
			if (header.AMA_RN_NKCountry == Core.Constants.CountryCodes.Bangladesh)
			{
				var customsCodes = GlbBranch.CurrentBranch.OrgProxy.CustomsCodes;
				var bothCcdAndCccNotFound = customsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CustomsClientCode, Core.Constants.CountryCodes.Bangladesh).IsEmpty
					&& customsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierCode, Core.Constants.CountryCodes.Bangladesh).IsEmpty;

				if (bothCcdAndCccNotFound)
				{
					return Res.GetString("4C074105-F9A3-41A4-A567-8554CBFE22F8",
						"Your Branches Organization Proxy does not have a Customs Carrier Code (CCC) or a Customs Client Code (CCD) code configured for BD.  One of these codes are required to be included in the name of the files uploaded to ASYCUDA.");
				}
			}

			return result;
		}
	}
}
