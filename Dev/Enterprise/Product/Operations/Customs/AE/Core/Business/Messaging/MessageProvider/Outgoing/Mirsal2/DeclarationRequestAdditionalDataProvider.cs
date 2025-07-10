using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AE.Business;

sealed class DeclarationRequestAdditionalDataProvider : IDeclarationRequestAdditionalDataProvider
{
	public int? GetBrokerCustomerCode(CusEntryHeader businessObject)
	{
		return int.TryParse(GetBrokerCode(businessObject), out var result) ? result : 0;
	}

	string GetBrokerCode(CusEntryHeader businessObject)
	{
		var orgHeader = businessObject?.Declaration?.JE_OA_DeclarantAddress_ZAddress?.OrgHeader as OrgHeader;
		return orgHeader?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CustomsClientCode, Core.Constants.CountryCodes.UnitedArabEmirates) ?? string.Empty;
	}

	public string GetCargoTypePackageCode(CusEntryHeader businessObject)
	{
		var declaration = businessObject?.Declaration;
		if(declaration == null)
		{
			return string.Empty;
		}

		if(declaration.JE_TransportMode == Core.Constants.TransportModes.Sea)
		{
			return declaration.JE_ContainerMode;
		}

		return CargoTypePackageCode;
	}

	public string GetConsigneeImporterTransfereeCode(CusEntryHeader businessObject)
	{
		return businessObject?.Declaration?.Importer?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CustomsClientCode, Core.Constants.CountryCodes.UnitedArabEmirates) ?? string.Empty;
	}

	public string GetConsignorExporterTransferorCode(CusEntryHeader businessObject)
	{
		return businessObject?.Declaration?.Supplier?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CustomsClientCode, Core.Constants.CountryCodes.UnitedArabEmirates) ?? string.Empty;
	}

	public string GetCTOCargoHandlerPremisesCode(CusEntryHeader businessObject)
	{
		var declaration = businessObject?.Declaration;
		if (declaration == null || !supportedTransportModesForCTOCargoHandlerPremises.Contains(declaration.JE_TransportMode))
		{
			return string.Empty;
		}
		return declaration.ContainerTerminalOperatorDocAddress?.Organisation?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierCode, Core.Constants.CountryCodes.UnitedArabEmirates) ?? string.Empty;
	}

	public string GetNotifyPartyCode(CusEntryHeader businessObject)
	{
		// TODO: Possibly something in JobDeclaration.JE_OH_NotifyParty Config
		return string.Empty;
	}

	public string GetShippingAirlineAgentBusinessCode(CusEntryHeader businessObject)
	{
		var declaration = businessObject?.Declaration;
		if (declaration == null || !supportedTransportModesForShippingAndAirline.Contains(declaration.JE_TransportMode))
		{
			return string.Empty;
		}

		return declaration.ShippingLine?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierCode, Core.Constants.CountryCodes.UnitedArabEmirates) ?? string.Empty;
	}

	public decimal? GetRegimeType(CusEntryHeader businessObject)
	{
		var declaration = businessObject?.Declaration;
		if(declaration == null)
		{
			return null;
		}
		return decimal.TryParse(declaration.JE_MessageType, out var result) ? result : 0;
	}

	public short? GetDeclarationType(CusEntryHeader businessObject)
	{
		return short.TryParse(businessObject?.EntryInstruction.CEI_Style, out var result) ? result : (short)0;
	}

	public short? GetDeclarationPurpose(CusEntryHeader businessObject)
	{
		return short.TryParse(businessObject?.EntryInstruction.CEI_DeclarationPurpose, out var result) ? result : (short)0;
	}

	public string GetBrokerBusinessCode(CusEntryHeader businessObject) => GetBrokerCode(businessObject);

	public short? GetTradeType(CusEntryHeader businessObject)
	{
		return 0;
	}

	public string GetContainerType(Customs.Business.BaseCusContainer businessObject)
	{
		var codeMap = businessObject?.Container?.CodeMapCollection.Cast<RefContainerCodeMap>().FirstOrDefault(x => x.RCM_RN_NKCountry == Core.Constants.CountryCodes.UnitedArabEmirates);
		return codeMap?.RCM_Code ?? string.Empty;
	}

	public short? GetPaymentInstrumentType(JobComInvoiceHeader businessObject)
	{
		return short.TryParse(businessObject?.JZ_PaymentMethod, out var result) ? result : (short)0;
	}

	public short? GetValuationMethod(JobComInvoiceHeader businessObject)
	{
		return short.TryParse(businessObject?.JZ_ValuationCode, out var result) ? result : (short)0;
	}

	public int? GetVehicleBrand(CusVehicle businessObject)
	{
		return int.TryParse(businessObject.CVH_BrandName, out var result) ? result : 0;
	}

	public decimal? GetTransportMode(CusEntryHeader businessObject)
	{
		var declaration = businessObject?.Declaration;
		if (declaration == null)
		{
			return null;
		}
		return decimal.TryParse(declaration.JE_TransportMode, out var result) ? result : 0;
	}

	public string GetVehicleCondition(CusVehicle businessObject)
	{
		var isUsed = businessObject?.CVH_IsUsed;
		if (isUsed == null)
		{
			return string.Empty;
		}
		return isUsed.Value ? "O" : "N";
	}

	readonly HashSet<string> supportedTransportModesForCTOCargoHandlerPremises = new HashSet<string> { Core.Constants.TransportModes.Sea, Core.Constants.TransportModes.Air, AETransportTypeList.Codes.CourierAir, AETransportTypeList.Codes.Courier, AETransportTypeList.Codes.CourierLand };

	readonly HashSet<string> supportedTransportModesForShippingAndAirline = new HashSet<string> { Core.Constants.TransportModes.Sea, Core.Constants.TransportModes.Air, AETransportTypeList.Codes.CourierAir };

	const string CargoTypePackageCode = "General";
}
