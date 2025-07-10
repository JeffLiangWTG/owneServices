using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeList;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaManifestHeaderValidation : BaseAsycudaManifestHeaderValidation
	{
		public AsycudaManifestHeaderValidation(AsycudaManifestHeader parent)
			: base(parent)
		{
		}

		protected override void CheckAMA_RN_NKCountry()
		{
			base.CheckAMA_RN_NKCountry();

			var missingCusCodesOnOrgProxy = ZZValidationHelper.HasMandatoryCusCode(ManifestValidationRuleCodes.OrgProxy, GlbCompany.CurrentCompany.OrgProxy);
			if (!missingCusCodesOnOrgProxy.IsEmpty)
			{
				Parent.AMA_RN_NKCountryInfo.AddMessageError(missingCusCodesOnOrgProxy);
			}
		}

		protected override void CheckAMA_CustomsOffice()
		{
			base.CheckAMA_CustomsOffice();

			MandatoryCheckOfCustomsOffice();
		}

		protected virtual void MandatoryCheckOfCustomsOffice()
		{
			ZZValidationHelper.CheckIsMandatoryForOneCountryWhenTransportModeMatches(Parent.AMA_CustomsOfficeInfo, ManifestValidationRuleCodes.OfficeCode, Parent.AMA_TransportMode);
		}

		protected override void CheckAMA_OA_ShippingAgent()
		{
			base.CheckAMA_OA_ShippingAgent();
			if (Parent.AMA_OA_ShippingAgent.IsEmpty)
			{
				ZZValidationHelper.CheckIsMandatoryFor(Parent.AMA_OA_ShippingAgentInfo, ManifestValidationRuleCodes.ShippingAgent);
			}
		}

		protected override void CheckAMA_NatureCore()
		{
			base.CheckAMA_NatureCore();
			if (Parent.AMA_Nature.IsEmpty)
			{
				ZZValidationHelper.CheckIsMandatoryWhenTransportModeMatches(Parent.AMA_NatureInfo, ManifestValidationRuleCodes.Nature, Parent.AMA_TransportMode);
			}
		}

		protected override void CheckAMA_DateAtCustomsOffice()
		{
			base.CheckAMA_DateAtCustomsOffice();
			ZZValidationHelper.CheckIsMandatoryForOneCountryWhenTransportModeMatches(Parent.AMA_DateAtCustomsOfficeInfo, ManifestValidationRuleCodes.DateAtCustomsOffice, Parent.AMA_TransportMode);
		}

		protected override void CheckAMA_RL_NKPortOfFirstArrival()
		{
			base.CheckAMA_RL_NKPortOfFirstArrival();

			ZZValidationHelper.CheckIsMandatoryForOneCountryWhenTransportModeMatches(Parent.AMA_RL_NKPortOfFirstArrivalInfo, ManifestValidationRuleCodes.PortOfFirstArrival, Parent.AMA_TransportMode);

			if (!Parent.AMA_RL_NKPortOfFirstArrival.IsEmpty && Parent.IsAir && (Parent.PortOfFirstArrival?.RL_IATA ?? ZString.Empty).IsEmpty)
			{
				ZZValidationHelper.CheckIsMandatoryForValidationRuleWhenTheRelatedValueIsEmpty(Parent.AMA_RL_NKPortOfFirstArrivalInfo, ManifestValidationRuleCodes.IATAPortOfFirstArrival);
			}
		}

		protected override void CheckAMA_CarrierCode()
		{
			base.CheckAMA_CarrierCode();
			ZZValidationHelper.CheckIsMandatoryForOneCountryWhenTransportModeMatches(Parent.AMA_CarrierCodeInfo, ManifestValidationRuleCodes.CarrierCode, Parent.AMA_TransportMode);
		}

		ZZDatabaseValidationHelper ZZValidationHelper => zzValidationHelper ??= Parent.ZZValidationHelper;
		ZZDatabaseValidationHelper zzValidationHelper;
	}
}
