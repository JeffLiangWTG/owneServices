using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.JP.Common;

namespace Enterprise.Customs.JP.Manifest.Business;

public class AsycudaManifestHeaderValidation : ASYCUDA.Business.AsycudaManifestHeaderValidation
{
	public AsycudaManifestHeaderValidation(AsycudaManifestHeader parent) : base(parent)
	{
	}

	public new AsycudaManifestHeader Parent => (AsycudaManifestHeader)base.Parent;

	public override void ValidateAll()
	{
		base.ValidateAll();
		Parent.MasterBill.Validation.ValidateABL_RL_NKPortOfLoading();
		Parent.MasterBill.Validation.ValidateABL_RL_NKPortOfDischarge();
		Parent.MasterBill.Validation.ValidateABL_GoodsLocation();
		ValidateAMA_BookingNumber();
		ValidateViaLocationCode();
		ValidatePortOfLoadingIATACode();
		ValidatePortOfDischargeIATACode();
	}

	public void ValidatePortOfLoadingIATACode()
	{
		ValidateCalculatedProperty(Parent.PortOfLoadingIATACodeInfo);
	}

	public void ValidatePortOfDischargeIATACode()
	{
		ValidateCalculatedProperty(Parent.PortOfDischargeIATACodeInfo);
	}

	public void ValidateAMA_CustomsAgentCredentialPK()
	{
		ValidateCalculatedProperty(Parent.AMA_CustomsAgentCredentialPKInfo);
	}

	public void ValidateAMA_BookingNumber()
	{
		ValidateCalculatedProperty(Parent.AMA_BookingNumberInfo);
	}

	public void ValidateViaLocationCode()
	{
		ValidateCalculatedProperty(Parent.ViaLocationCodeInfo);
	}

	protected void CheckPortOfLoadingIATACode()
	{
		if (Parent.IsAir)
		{
			ValidationHelper.CheckIATACode(Parent.Factory, Parent.PortOfLoadingIATACodeInfo, Parent.AMA_RL_NKPortOfLoadingInfo.HumanReadableName);
		}
	}

	protected void CheckPortOfDischargeIATACode()
	{
		if (Parent.IsAir)
		{
			ValidationHelper.CheckIATACode(Parent.Factory, Parent.PortOfDischargeIATACodeInfo, Parent.AMA_RL_NKPortOfDischargeInfo.HumanReadableName);
		}
	}

	protected void CheckViaLocationCode()
	{
		var parent = Parent;
		var viaLocationCode = parent.ViaLocationCode;
		if (!viaLocationCode.IsEmpty && parent.IsSea)
		{
			var targetInfo = parent.ViaLocationCodeInfo;
			ListValidation.MessageErrorIfInvalidCode(targetInfo, ResString.GetMultilingualString("8E6D01AA-74AB-49CA-A0F9-0EC2846108B1", "The entered Move-In Destination is invalid. Please select a value from the list."));

			var containers = parent.Containers.Cast<AsycudaContainer>().ToList();
			if (containers.Any(c => c.HasSealNumber))
			{
				targetInfo.AddMessageError(Res.GetString("7DEB96D2-F4D9-49FF-8BA1-0DCE6918353A", "Via location must be empty if at least one seal number is entered."));
			}

			if (containers.Any(x => x.VanningLocationCode == viaLocationCode))
			{
				targetInfo.AddMessageError(Res.GetString("7C905E6C-5020-43D2-901A-729B3D2278FA", "The same bonded area code is entered in the via location code and the vanning location code."));
			}
		}
	}

	protected void CheckAMA_BookingNumber()
	{
		var parent = Parent;
		if (parent.IsSea && parent.IsExport && parent.AMA_BookingNumber.IsEmpty)
		{
			parent.AMA_BookingNumberInfo.AddWarning(Res.GetString("30756095-9951-412E-A842-F31D350FD10D", "ZZZZ will be populated as the booking number if the booking number is not provided."));
		}
	}

	protected override void MandatoryCheckOfCustomsOffice()
	{
	}

	protected virtual void CheckAMA_CustomsAgentCredentialPK()
	{
		if (Parent.AMA_CustomsAgentCredentialPK == ZGuid.Empty)
		{
			Parent.AMA_CustomsAgentCredentialPKInfo.AddMessageError(Res.GetString("BAA0A2C8-9EE0-4315-A02F-87192CB70521", "You have not entered a NACCS Credential."));
		}
		else
		{
			if (!Parent.Lookups.NaccsCredentialList.Any(x => x.PK == Parent.AMA_CustomsAgentCredentialPK))
			{
				Parent.AMA_CustomsAgentCredentialPKInfo.AddMessageError(Res.GetString("6A5D50EF-2E72-49F4-90D1-56EAD9CB3124", "The entered value is not in the list."));
			}
		}
	}

	protected override void CheckAMA_GS_NKCustomsAgent()
	{
		if (string.IsNullOrWhiteSpace(Parent.AMA_GS_NKCustomsAgent))
		{
			Parent.AMA_GS_NKCustomsAgentInfo.AddMessageError(Res.GetString("D3045B37-4CA2-4507-80CA-EB0856FA5B5A", "You have not entered a Customs Agent."));
		}
	}

	protected override void CheckAMA_CarrierCode()
	{
		base.CheckAMA_CarrierCode();
		var parent = Parent;
		if (parent.IsSea && !parent.IsNVC)
		{
			var targetInfo = parent.AMA_CarrierCodeInfo;
			var carrierCode = parent.AMA_CarrierCode;
			var scacCode = parent.Carrier?.Header?.ShippingLineSCAC ?? ZString.Empty;
			if (!scacCode.IsEmpty && scacCode != carrierCode)
			{
				targetInfo.AddWarning(Res.GetString("D998E4F8-52D1-4796-9D5E-054D4A237EC1", "The entered Carrier Code is different from the one configured in the organization."));
			}
		}
	}

	protected override void CheckAMA_MasterBill()
	{
		var masterBill = Parent.MasterBill;
		if (masterBill != null)
		{
			masterBill.Validation.ValidateABL_BillNumber();
			Parent.AMA_MasterBillInfo.AddAllNotificationsFrom(masterBill.ABL_BillNumberInfo);
		}

		if (Parent.IsAir)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_MasterBillInfo);
			if (!Parent.AMA_MasterBill.IsEmpty)
			{
				var manifestNumber = Parent.AMA_MasterBill.Replace("-", "").Replace(" ", "");
				CheckManifestNumberWithAirWayBillValidator(manifestNumber);
			}
		}

		if (Parent.IsNVC01BondedLocationAmendmentSendingInProgress)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_MasterBillInfo);
		}
	}

	protected override void CheckManifestNumberWithAirWayBillValidator(string manifestNumber)
	{
		string message = null;
		if (Parent.IsImport && Parent.IsSubConsolidation && manifestNumber.Length > 16)
		{
			{
				message = Res.GetString("AB8A8FC9-DB83-4872-AFDC-D3F797385829", "The MAWB must be limited to 16 characters or fewer.");
			}
		}
		if (string.IsNullOrEmpty(message))
		{
			if (!Parent.IsSubConsolidation)
			{
				message = new JPAirWayBillValidator().GetWarningMessage(manifestNumber);
			}
		}
		if (!string.IsNullOrEmpty(message))
		{
			Parent.AMA_MasterBillInfo.AddMessageError(message);
		}
	}

	protected override void CheckAMA_RL_NKPortOfFinalDeparture()
	{
		base.CheckAMA_RL_NKPortOfFinalDeparture();
		var parent = Parent;
		if (parent.IsSea && !parent.IsNVC)
		{
			if (parent.AMA_RL_NKPortOfFinalDeparture.IsEmpty)
			{
				if (!parent.AMA_RadioCallSign.IsEmpty)
				{
					parent.AMA_RL_NKPortOfFinalDepartureInfo.AddWarning(Res.GetString("F664ABB3-C391-4DAB-A463-D189A0C0DA56", "If Move-In Destination is left blank, your vessel call sign will be sent as the Move-In Destination instead."));
				}
				else
				{
					parent.AMA_RL_NKPortOfFinalDepartureInfo.AddMessageError(Res.GetString("18D5EFBB-4EDD-42A3-B385-C17BD5A6F81A", "Please enter either Move-In Destination or Vessel Call Sign."));
				}
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(parent.AMA_RL_NKPortOfFinalDepartureInfo, ResString.GetMultilingualString("5A5BF9C4-9555-4DAD-8E32-232D913DD059", "The entered Move-In Destination is invalid. Please select a value from the list."));
			}
		}
	}

	protected override void CheckAMA_OA_CarrierMandatory()
	{
	}

	protected override void CheckAMA_VoyageMandatory()
	{
		if (Parent.IsHCH)
		{
			base.CheckAMA_VoyageMandatory();
		}
	}

	protected override void CheckAMA_OA_Consolidator()
	{
		base.CheckAMA_OA_Consolidator();
		var parent = Parent;
		if (parent.IsImport && parent.Consolidator != null && parent.ConsolidatorNACCSUserCode.IsEmpty)
		{
			parent.AMA_OA_ConsolidatorInfo.AddMessageError(Res.GetString("BFFB6C1A-B144-42B6-BBE7-714F761AD954", "The selected Consolidator address does not have a NACCS User Code. To add one, press F3 to visit the organization, go to Details > Config > Registration Number / Codes and add a new row where Country/Region of Issue is JP, Type is NUC, and Premises Address is the selected Consolidator address."));
		}
	}

	protected override bool ShouldValidateConveyanceCountry => false;
}
