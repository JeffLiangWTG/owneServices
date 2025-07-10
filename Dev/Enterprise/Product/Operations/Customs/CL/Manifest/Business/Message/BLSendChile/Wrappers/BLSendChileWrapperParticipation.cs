using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.CL.MessageContracts;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CL.Manifest.Business
{
	internal class BLSendChileWrapperParticipation : IParticipationDocument
	{
		internal BLSendChileWrapperParticipation(AsycudaBill bill, ZString participationName)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
			this.participationName = participationName;
			participationCountry = GetParticipationCountry();
		}
		readonly AsycudaBill bill;
		readonly string participationName;
		readonly string participationCountry;

		string IParticipationDocument.Name => participationName;

		string IParticipationDocument.IDType
		{
			get
			{
				if (participationCountry == Constants.CountryCodes.Chile)
				{
					var type = ChileOrgCusCodeInfo.OrgCusCodes.RUT;
					switch (participationName)
					{
						case WrappersConstants.ParticipationName.Cons:
							type = bill.ABL_ConsigneeRegNoType;
							break;
						case WrappersConstants.ParticipationName.Noti:
							type = bill.ABL_NotifyPartyRegNoType;
							break;
					}
					return type;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		string IParticipationDocument.IDValue
		{
			get
			{
				var id = ZString.Empty;
				if (participationCountry == Constants.CountryCodes.Chile)
				{
					switch (participationName)
					{
						case WrappersConstants.ParticipationName.Alm:
							id = bill.GoodsLocation?.Header?.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(c => c.OK_CodeType == ChileOrgCusCodeInfo.OrgCusCodes.RUT && c.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Chile)?.OK_CustomsRegNo ?? ZString.Empty;
							break;
						case WrappersConstants.ParticipationName.Emido:
							id = bill.Header.Carrier?.Header?.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(c => c.OK_CodeType == ChileOrgCusCodeInfo.OrgCusCodes.RUT && c.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Chile)?.OK_CustomsRegNo ?? ZString.Empty;
							break;
						case WrappersConstants.ParticipationName.Emb:
							id = bill.ABL_ShipperRegNo;
							break;
						case WrappersConstants.ParticipationName.Cons:
							id = bill.ABL_ConsigneeRegNo;
							break;
						case WrappersConstants.ParticipationName.Noti:
							id = bill.ABL_NotifyPartyRegNo;
							break;
						default:
							id = GlbCompany.CurrentCompany.GC_BusinessRegNo;
							break;
					}
				}
				return id;
			}
		}

		string IParticipationDocument.Names
		{
			get
			{
				var name = ZString.Empty;
				switch (participationName)
				{
					case WrappersConstants.ParticipationName.Alm:
						name = bill.GoodsLocation?.Header?.OH_FullName ?? ZString.Empty;
						break;
					case WrappersConstants.ParticipationName.Emido:
						name = bill.Header.Carrier?.Header?.OH_FullName ?? ZString.Empty;
						break;
					case WrappersConstants.ParticipationName.Emb:
						name = bill.ABL_ShipperName;
						break;
					case WrappersConstants.ParticipationName.Cons:
						name = bill.ABL_ConsigneeName;
						break;
					case WrappersConstants.ParticipationName.Noti:
						name = bill.ABL_NotifyPartyName;
						break;
					default:
						name = GlbCompany.CurrentCompany.GC_Name;
						break;
				}
				return name;
			}
		}

		string IParticipationDocument.CountryID => participationCountry;

		string IParticipationDocument.WarehouseCode
		{
			get
			{
				var warehouseCode = ZString.Empty;
				if (participationName == WrappersConstants.ParticipationName.Alm)
				{
					warehouseCode = bill.GoodsLocation?.Header?.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(c => c.OK_CodeType == OrgCusCode.CodeTypes.ControlledPremisesID && c.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Chile)?.OK_CustomsRegNo ?? ZString.Empty;
				}
				return warehouseCode;
			}
		}

		string IParticipationDocument.Address
		{
			get
			{
				var address = string.Empty;
				switch (participationName)
				{
					case WrappersConstants.ParticipationName.Emb:
						address = string.Concat(bill.ABL_ShipperStreet1, " ", bill.ABL_ShipperCity, " ", bill.ABL_ShipperPostcode);
						break;
					case WrappersConstants.ParticipationName.Cons:
						address = string.Concat(bill.ABL_ConsigneeStreet1, " ", bill.ABL_ConsigneeCity, " ", bill.ABL_ConsigneePostcode);
						break;
					case WrappersConstants.ParticipationName.Noti:
						address = string.Concat(bill.ABL_NotifyPartyStreet1, " ", bill.ABL_NotifyPartyCity, " ", bill.ABL_NotifyPartyPostcode);
						break;
				}
				return address;
			}
		}

		string IParticipationDocument.Phone
		{
			get
			{
				var phone = ZString.Empty;
				switch (participationName)
				{
					case WrappersConstants.ParticipationName.Emb:
						phone = bill.ABL_ShipperPhone;
						break;
					case WrappersConstants.ParticipationName.Cons:
						phone = bill.ABL_ConsigneePhone;
						break;
					case WrappersConstants.ParticipationName.Noti:
						phone = bill.ABL_NotifyPartyPhone;
						break;
				}
				return phone;
			}
		}

		string IParticipationDocument.Email
		{
			get
			{
				var email = ZString.Empty;
				switch (participationName)
				{
					case WrappersConstants.ParticipationName.Emb:
						email = bill.Shipper?.OA_Email ?? ZString.Empty;
						break;
					case WrappersConstants.ParticipationName.Cons:
						email = bill.Consignee?.OA_Email ?? ZString.Empty;
						break;
					case WrappersConstants.ParticipationName.Noti:
						email = bill.NotifyParty?.OA_Email ?? ZString.Empty;
						break;
				}
				return email;
			}
		}

		string GetParticipationCountry()
		{
			var countryID = Constants.CountryCodes.Chile;
			switch (participationName)
			{
				case WrappersConstants.ParticipationName.Cons:
					countryID = bill.ABL_RN_NKConsigneeCountry;
					break;
				case WrappersConstants.ParticipationName.Noti:
					countryID = bill.ABL_RN_NKNotifyPartyCountry;
					break;
				case WrappersConstants.ParticipationName.Emido:
					countryID = bill.Header.Carrier?.OA_RN_NKCountryCode ?? ZString.Empty;
					break;
				case WrappersConstants.ParticipationName.Emb:
					countryID = bill.ABL_RN_NKShipperCountry;
					break;
			}
			return countryID;
		}
	}
}
