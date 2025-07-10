using System.Linq;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using ValidationConstants = Enterprise.Customs.ASYCUDA.Business.ValidationConstants;

namespace Enterprise.Customs.GB.ICS.Business;

public class AsycudaBillSSValidationForMasterChild : ASYCUDA.Business.AsycudaBillValidationForMasterChild
{
	public AsycudaBillSSValidationForMasterChild(ASYCUDA.Business.AsycudaBill parent) : base(parent)
	{
	}

	protected override void CheckABL_BillNumber()
	{
		if (Parent.Header.AMA_TransportMode == GBSSTransportTypeList.Codes.AirFreight && Parent.ABL_BillNumber.IsEmpty)
		{
			Parent.ABL_BillNumberInfo.AddMessageError(ResString.GetMultilingualString("5BB1CC41-60AE-4D26-B045-7817829517B6", "Please enter Master Airway Bill (MAWB). This is a required field when Transport Mode = AIR."));
		}
	}

	protected override bool ShouldCheckHasAsycudaCountry => false;

	protected override void CheckABL_RL_NKPortOfDischarge()
	{
		if (Parent.Header.Bills.Count == 0 || Parent.Header.Bills.Any(x => x.ABL_RL_NKFinalDestination.IsEmpty))
		{
			if (NoSupportedManifestCountryCode)
			{
				Parent.ABL_RL_NKPortOfDischargeInfo.AddMessageError(ValidationConstants.ManifestMustGoThruSupportedCountries);
			}
			else
			{
				base.CheckABL_RL_NKPortOfDischarge();
			}
		}
		else if (!Parent.ABL_RL_NKPortOfDischarge.IsEmpty)
		{
			base.CheckABL_RL_NKPortOfDischarge();
		}
	}

	protected override void CheckABL_RL_NKPortOfLoading()
	{
		if (Parent.Header.Bills.Count == 0 || Parent.Header.Bills.Any(x => x.ABL_RL_NKOrigin.IsEmpty))
		{
			if (NoSupportedManifestCountryCode)
			{
				Parent.ABL_RL_NKPortOfLoadingInfo.AddMessageError(ValidationConstants.ManifestMustGoThruSupportedCountries);
			}
			else
			{
				base.CheckABL_RL_NKPortOfLoading();
			}
		}
		else if (!Parent.ABL_RL_NKPortOfLoading.IsEmpty)
		{
			base.CheckABL_RL_NKPortOfLoading();
		}
	}
}
