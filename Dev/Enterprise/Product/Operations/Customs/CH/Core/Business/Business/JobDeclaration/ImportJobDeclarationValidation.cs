using CargoWise.EntityFramework;
using Enterprise.Customs.Business.CustomsLists;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.CH.Business;

public class ImportJobDeclarationValidation : JobDeclarationValidation
{
	public ImportJobDeclarationValidation(JobDeclaration parent)
		: base(parent)
	{
	}

	protected override void CheckJE_OH_Importer()
	{
		base.CheckJE_OH_Importer();

		var targetPropertyInfo = Parent.JE_OH_ImporterInfo;
		MandatoryValidation.MessageErrorIfNotEntered(targetPropertyInfo);

		var importerAddress = Parent.Importer?.MainAddress;
		PlausiValidation.CheckR121(targetPropertyInfo, importerAddress);
		PlausiValidation.CheckR168(targetPropertyInfo, Parent);
	}

	protected override void CheckJE_OH_Consignee()
	{
		base.CheckJE_OH_Consignee();

		var targetPropertyInfo = Parent.JE_OH_ConsigneeInfo;
		MandatoryValidation.MessageErrorIfNotEntered(targetPropertyInfo);

		var consigneeAddress = Parent.IntermConsignee?.MainAddress;
		PlausiValidation.CheckR121(targetPropertyInfo, consigneeAddress);
	}

	protected override void CheckJE_OA_Representative()
	{
		base.CheckJE_OA_Representative();

		var targetPropertyInfo = Parent.JE_OA_RepresentativeInfo;
		PlausiValidation.CheckR348(targetPropertyInfo, Parent);
		PlausiValidation.CheckR121(targetPropertyInfo, Parent.Representative);
	}

	protected override void CheckJE_RL_NKPortOfLoading()
	{
		base.CheckJE_RL_NKPortOfLoading();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_RL_NKPortOfLoadingInfo);
	}

	protected override void CheckJE_PaymentMethod()
	{
		base.CheckJE_PaymentMethod();

		var targetInfo = Parent.JE_PaymentMethodInfo;
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(targetInfo);

		if (!targetInfo.HasNotifications())
		{
			CheckPaidBy(targetInfo, Parent.JE_VATPaidByInfo, () => Parent.DutyPaidByAccountNo, OrgCusCode.SwissCodeTypes.CAD);
		}
	}

	protected override void CheckJE_VesselName()
	{
		base.CheckJE_VesselName();
		if (Parent.JE_VesselName.IsEmpty && Parent.IsRoad)
		{
			Parent.JE_VesselNameInfo.AddMessageError(Res.GetString("7B4EDE18-567D-48AC-80D3-CB614E7F300F", "You have not entered a {0}. {0} is mandatory when Transport Mode is {1}.", Parent.JE_VesselNameInfo.HumanReadableName, TransportTypeGenericList.Descriptions.Road));
		}
	}

	protected override void CheckJE_HouseBill()
	{
		base.CheckJE_HouseBill();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_HouseBillInfo);
	}

	protected override void CheckJE_LocationOfGoods()
	{
		base.CheckJE_LocationOfGoods();
		if (Parent.JE_LocationOfGoods.IsEmpty && Parent.IsImportDomicile)
		{
			Parent.JE_LocationOfGoodsInfo.AddMessageError(Res.GetString("7637C74C-1925-44C1-B9A5-CD1F04D08FFC", "You have not entered a Goods Location. Goods Location is mandatory when Clearance Location is {0}.", Parent.Lookups.ClearanceLocationList.GetDescriptionFromCode(UniversalReferenceConstants.ClearanceLocation.Domicile)));
		}
	}

	protected override void CheckJE_ShipmentIncoTerm()
	{
		base.CheckJE_ShipmentIncoTerm();
		if (Parent.JE_ShipmentIncoTerm == IncoTerms.FreeCarrierSeller || Parent.JE_ShipmentIncoTerm == IncoTerms.FreeCarrierBuyer)
		{
			Parent.JE_ShipmentIncoTermInfo.AddWarning(Res.GetString("6BE0F223-CA0A-44F7-A52B-6EA8AD44218F", "The internally used Incoterm {0} is not official and isn't accepted by Swiss customs. It will be converted to {1} in the declaration sending message to customs.", Parent.JE_ShipmentIncoTerm, IncoTerms.FreeCarrier));
		}
	}

	protected override void CheckDispatchCountryCode()
	{
		base.CheckDispatchCountryCode();
		PlausiValidation.CheckR167c(Parent.DispatchCountryCodeInfo, Parent);
	}

	protected override void CheckJE_RN_NKTransportNationality()
	{
		base.CheckJE_RN_NKTransportNationality();

		if (Parent.JE_RN_NKTransportNationality.IsEmpty && Parent.JE_TransportMode == TransportTypeGenericList.Codes.Road)
		{
			Parent.JE_RN_NKTransportNationalityInfo.AddMessageError(Res.GetString("D0D46C8A-E05F-4685-8ECE-AC73F9CADB9F", "You have not entered a {0}. {0} is mandatory when Transport Mode is {1}.", Parent.JE_RN_NKTransportNationalityInfo.HumanReadableName, TransportTypeGenericList.Descriptions.Road));
		}
	}

	protected override void CheckJE_GS_NKCusAgent()
	{
		base.CheckJE_GS_NKCusAgent();
		CheckJE_GS_NKCusAgent_Password();
	}
}
