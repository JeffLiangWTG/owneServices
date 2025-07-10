using System;
using CargoWise.Common;
using CargoWise.Customs.CO.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.CO.Manifest.Business
{
	internal class PartyWrapper : IParty
	{
		internal PartyWrapper(AsycudaBill bill, ZString partyType)
		{
			this.bill = Argument.NotNull(bill, "AsycudaBill cannot be null");
			this.partyType = partyType;
			header = bill.Header;
			businessRegNo = COWrappersHelper.GetBusinessRegistrationNumber(header, partyType);
		}
		readonly AsycudaBill bill;
		readonly AsycudaManifestHeader header;
		readonly ZString partyType;
		readonly ZString businessRegNo;

		int IParty.DocumentType
		{
			get
			{
				switch (partyType)
				{
					case COWrappersConstants.Parties.CarrierCode:
						return (header?.Carrier?.Country.Code ?? ZString.Empty) == Core.Constants.CountryCodes.Colombia ? 31 : 43;
					case COWrappersConstants.Parties.HouseShipperCode:
						return (bill.Shipper?.Country.Code ?? ZString.Empty) == Core.Constants.CountryCodes.Colombia ? COWrappersHelper.GetPartyTypeCode(bill.Shipper?.Header?.CustomsCodes, bill.GetCountryCodeForCustomsRegNo(bill.Shipper)) : 43;
					case COWrappersConstants.Parties.MasterShipperCode:
						return (header?.ShippingAgent?.Country.Code ?? ZString.Empty) == Core.Constants.CountryCodes.Colombia ? 31 : 43;
					case COWrappersConstants.Parties.HouseConsigneeCode:
					case COWrappersConstants.Parties.MasterConsigneeCode:
						return (bill.Consignee?.Country.Code ?? ZString.Empty) == Core.Constants.CountryCodes.Colombia ? COWrappersHelper.GetPartyTypeCode(bill.Consignee?.Header?.CustomsCodes, bill.GetCountryCodeForCustomsRegNo(bill.Consignee)) : 43;
					default:
						return 31;
				}
			}
		}

		string IParty.ID
		{
			get
			{
				var id = ZString.Empty;
				if (businessRegNo.Length > 0)
				{
					switch (partyType)
					{
						case COWrappersConstants.Parties.MasterShipperCode:
						case COWrappersConstants.Parties.HouseShipperCode:
							id = businessRegNo;
							break;
						default:
							id = businessRegNo.Left(businessRegNo.Length - 1);
							break;
					}
				}
				return id.KeepNumericCharacters();
			}
		}

		int IParty.VerificationDigit => businessRegNo.Length > 0 ? Convert.ToInt16(businessRegNo.Right(1)) : 0;

		string IParty.CompanyName
		{
			get
			{
				switch (partyType)
				{
					case COWrappersConstants.Parties.CarrierCode:
						return header.Carrier?.Header?.OH_FullName ?? ZString.Empty;
					case COWrappersConstants.Parties.HouseShipperCode:
						return (bill.Shipper?.Country.Code ?? ZString.Empty) != Core.Constants.CountryCodes.Colombia ? bill.ABL_ShipperName : ZString.Empty;
					case COWrappersConstants.Parties.MasterShipperCode:
						return header.ShippingAgent?.Header?.OH_FullName ?? ZString.Empty;
					case COWrappersConstants.Parties.HouseConsigneeCode:
						return (bill.Consignee?.Country.Code ?? ZString.Empty) != Core.Constants.CountryCodes.Colombia ? bill.ABL_ConsigneeName : ZString.Empty;
					default:
						return ZString.Empty;
				}
			}
		}
	}
}
