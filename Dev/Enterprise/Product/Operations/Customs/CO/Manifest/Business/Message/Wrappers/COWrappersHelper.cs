using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CO.Manifest.Business
{
	internal static class COWrappersHelper
	{
		internal static double GetDocumentID(CusTransactionNumber transactionNumber)
		{
			double result = 0;
			var id = transactionNumber?.TN_TransactionReference ?? ZString.Empty;
			double.TryParse(id, out result);
			return result;
		}

		internal static ZString GetCustomsStateCode(RefUNLOCO refUNLOCO, BusinessObjectFactory factory, ZDateTime effectiveDate)
		{
			var result = ZString.Empty;
			if (refUNLOCO != null)
			{
				var stateCode = refUNLOCO.CountryStates?.RW_Code ?? ZString.Empty;
				result = ZZRefCusMapCombined.MapCW1CodeToCustomsCode(factory, Core.Constants.CountryCodes.Colombia, COWrappersConstants.StateMapType, stateCode, effectiveDate);
			}
			return result;
		}

		internal static ZString GetCustomsCityCode(RefUNLOCO refUNLOCO, BusinessObjectFactory factory, ZDateTime effectiveDate)
		{
			var result = ZString.Empty;
			if (refUNLOCO != null)
			{
				result = ZZRefCusMapCombined.MapCW1CodeToCustomsCode(factory, Core.Constants.CountryCodes.Colombia, COWrappersConstants.CityMapType, refUNLOCO.RL_Code, effectiveDate);
			}
			return result;
		}

		internal static ZString GetBusinessRegistrationNumber(AsycudaManifestHeader header, ZString partyType)
		{
			var bill = header.Bills[0];
			switch (partyType)
			{
				case COWrappersConstants.Parties.CarrierCode:
					return header.Carrier?.Header?.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(c => c.OK_CodeType == ColombiaOrgCusCodeInfo.OrgCusCodes.NIT && c.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Colombia)?.OK_CustomsRegNo ?? ZString.Empty;
				case COWrappersConstants.Parties.HouseShipperCode:
					return (bill.Shipper?.Country.Code ?? ZString.Empty) == Core.Constants.CountryCodes.Colombia ? bill.ABL_ShipperRegNo : ZString.Empty;
				case COWrappersConstants.Parties.MasterShipperCode:
					return header.ShippingAgent?.Header?.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(c => c.OK_CodeType == ColombiaOrgCusCodeInfo.OrgCusCodes.NIT && c.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Colombia)?.OK_CustomsRegNo ?? ZString.Empty;
				case COWrappersConstants.Parties.HouseConsigneeCode:
					return (bill.Consignee?.Country.Code ?? ZString.Empty) == Core.Constants.CountryCodes.Colombia ? bill.ABL_ConsigneeRegNo : ZString.Empty;
				case COWrappersConstants.Parties.MasterConsigneeCode:
					return GlbCompany.CurrentCompany.GC_BusinessRegNo;
				default:
					return ZString.Empty;
			}
		}

		internal static int GetPartyTypeCode(OrgCusCodeCollection customsCodes, ZString country)
		{
			if (!customsCodes.GetCustomsRegNo(ColombiaOrgCusCodeInfo.OrgCusCodes.NIT, country).IsEmpty)
			{
				return 31;
			}
			else if (!customsCodes.GetCustomsRegNo(ColombiaOrgCusCodeInfo.OrgCusCodes.CID, country).IsEmpty)
			{
				return 13;
			}
			else if (!customsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.PassportID, country).IsEmpty)
			{
				return 41;
			}
			else
			{
				return 43;
			}
		}

		internal static ZString GetLoadType(AsycudaManifestHeader header)
		{
			var loadType = 5;
			switch (header.AMA_ContainerMode)
			{
				case Core.Constants.ContainerModes.BreakBulk:
					loadType = 1;
					break;
				case Core.Constants.ContainerModes.Containerised:
					loadType = 2;
					break;
				case Core.Constants.ContainerModes.Bulk:
				case Core.Constants.ContainerModes.Liquid:
					loadType = 3;
					break;
			}
			return loadType.ToString();
		}

		internal static ZString GetContainerType(ZString containerType)
		{
			var cType = ZString.Empty;
			switch (containerType)
			{
				case Core.Constants.ContainerTypes.DryStorage:
				case Core.Constants.ContainerTypes.Other:
					cType = "1";
					break;
				case Core.Constants.ContainerTypes.FlatRack:
				case Core.Constants.ContainerTypes.Bolster:
					cType = "2";
					break;
				case Core.Constants.ContainerTypes.OpenTop:
					cType = "3";
					break;
				case Core.Constants.ContainerTypes.Tank:
					cType = "6";
					break;
				case Core.Constants.ContainerTypes.Refrigerated:
					cType = "8";
					break;
			}
			return cType.ToString();
		}
	}
}
