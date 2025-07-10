using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public static class DeltaIEDeclarationValidationHelper
	{
		public static void CheckEORIOrFullAddress(ZPropertyInfo info, JobDocAddress address)
		{
			if (TraderEoriInvalid(address) && MissingAddressFields(address))
			{
				AddMessageError(info, Res.GetString("F1AC1E8C-66DC-41C2-BF31-2A17359BAA69", "Please enter an organization with an EORI or enter full address details."));
			}
		}

		public static void CheckEORIOrFullAddress(ZPropertyInfo info, OrgAddress address)
		{
			if (TraderEoriInvalid(address) && MissingAddressFields(address))
			{
				AddMessageError(info, Res.GetString("F1AC1E8C-66DC-41C2-BF31-2A17359BAA69", "Please enter an organization with an EORI or enter full address details."));
			}
		}

		public static void CheckEORI(ZPropertyInfo info, JobDocAddress address, string ruleCode = "")
		{
			if (TraderEoriInvalid(address))
			{
				AddMessageError(info, Res.GetString("76C2AE3C-B297-47E5-B210-C7FE2158423C", "[{0}] The selected organization does not have an EORI.", ruleCode));
			}
		}

		public static void CheckEORI(ZPropertyInfo info, OrgAddress address, string ruleCode = "")
		{
			if (TraderEoriInvalid(address))
			{
				AddMessageError(info, Res.GetString("6D04E043-EF59-473C-B57B-FE2512200A44", "[{0}] The selected organization does not have an EORI.", ruleCode));
			}
		}

		public static bool HasMandatoryAdditionalInfos(AdditionalInfoCollection additionalInfos)
		{
			return additionalInfos
				.Cast<AdditionalInfo>()
				.Any(x => x.IsAnAdditionalInformation && x.CSI_Code == UniversalReferenceConstants.RefCusCodeList.AdditionalInformationCodes.FretCargo);
		}

		public static bool HasFallbackProcedureReferenceAdditionalInfos(AdditionalInfoCollection additionalInfos)
		{
			var fallbackSettings = (FallbackSettings)FRCustomsDataRegistry.Instance.DeltaIMode.Value;
			return additionalInfos
				.Cast<AdditionalInfo>()
				.Any(x => x.IsAnAdditionalReference && x.CSI_Code == UniversalReferenceConstants.RefCusCodeList.AdditionalReferenceCodes.FallbackProcedure && x.CSI_ReferenceNumber == fallbackSettings.InvocationReason);
		}

		public static bool HasFallbackProcedureInformationAdditionalInfos(AdditionalInfoCollection additionalInfos)
		{
			return additionalInfos
				.Cast<AdditionalInfo>()
				.Any(x => x.IsAnAdditionalInformation && x.CSI_Code == UniversalReferenceConstants.RefCusCodeList.AdditionalInformationCodes.FallbackProcedure);
		}

		public static AdditionalInfo GetPortCodeAdditionalReference(AdditionalInfoCollection additionalInfos)
		{
			return additionalInfos.OfType<AdditionalInfo>().FirstOrDefault(x => x.IsAnAdditionalReference && x.CSI_Code == UniversalReferenceConstants.RefCusCodeList.AdditionalReferenceCodes.PortCode);
		}

		static bool MissingAddressFields(JobDocAddress address)
		{
			return address?.Address != null && (address.Address.Header == null
				|| address.Address.Header.OH_FullNameTruncated.IsEmpty
				|| address.Address.Header.CountryCode.IsEmpty
				|| address.E2_Address1.IsEmpty
				|| address.E2_Postcode.IsEmpty
				|| address.E2_City.IsEmpty);
		}

		static bool MissingAddressFields(OrgAddress address)
		{
			return address != null && (address.Header == null
				|| address.Header.OH_FullNameTruncated.IsEmpty
				|| address.Header.CountryCode.IsEmpty
				|| address.OA_Address1.IsEmpty
				|| address.OA_PostCode.IsEmpty
				|| address.OA_City.IsEmpty);
		}

		static bool TraderEoriInvalid(JobDocAddress address)
		{
			var eoriCode = address?.Organisation?.GetEuIdentificationNumber() ?? ZString.Empty;
			return eoriCode.IsEmpty;
		}

		static bool TraderEoriInvalid(OrgAddress address)
		{
			var eoriCode = address?.Header?.GetEuIdentificationNumber() ?? ZString.Empty;
			return eoriCode.IsEmpty;
		}

		static void AddMessageError(ZPropertyInfo info, ZString error)
		{
			if (!error.IsEmpty && !info.HasMessageError(error))
			{
				info.AddMessageError(error);
			}
		}
	}
}
