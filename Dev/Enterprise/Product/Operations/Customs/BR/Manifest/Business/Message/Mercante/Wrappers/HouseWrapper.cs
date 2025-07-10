using System.Collections.Generic;
using CargoWise.Customs.BR.MessageContracts.Mercante.Outgoing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Manifest.Business
{
	internal class HouseWrapper : IHouse
	{
		internal HouseWrapper(AsycudaBill houseBill)
		{
			this.houseBill = houseBill;
		}
		readonly AsycudaBill houseBill;

		IHouse house => this;

		int IHouse.QuantityOfPackItems => houseBill.Packs.Count;

		string IHouse.HouseNumber => houseBill.ABL_BillNumber;

		bool IHouse.BLToTheOrder => houseBill.To_Order;

		bool IHouse.DeliveryPlaceAbroad => houseBill.ABL_RL_NKFinalDestination.Left(2) != Core.Constants.CountryCodes.Brazil;

		string IHouse.DeliveryCountryAbroadCode => house.DeliveryPlaceAbroad ? houseBill.ABL_RL_NKFinalDestination.Left(2) : ZString.Empty;

		bool IHouse.ForeignConsigneeIndicator => houseBill.ABL_RN_NKConsigneeCountry != Core.Constants.CountryCodes.Brazil;

		string IHouse.ConsigneeData => !house.ForeignConsigneeIndicator ? houseBill.ABL_ConsigneeName : ZString.Empty;

		string IHouse.ForeignConsigneePassportNumber => house.ForeignConsigneeIndicator && houseBill.ABL_ConsigneeRegNoType == OrgCusCode.CodeTypes.PassportID ? houseBill.ABL_ConsigneeRegNo : ZString.Empty;

		string IHouse.ForeignConsigneeName => house.ForeignConsigneeIndicator ? houseBill.ABL_ConsigneeName : ZString.Empty;

		string IHouse.ConsigneeRegNumber => houseBill.ABL_ConsigneeRegNo;

		string IHouse.ShipperID => houseBill.ABL_ShipperName;

		string IHouse.BillIsueDate => houseBill.Header.MasterBill?.ABL_BillIssueDate.ToString("yyyyMMdd");

		string IHouse.GoodsDescription => houseBill.ABL_GoodsDescription;

		decimal IHouse.Volume => houseBill.ABL_Volume;

		string IHouse.OriginPort => houseBill.ABL_RL_NKOrigin;

		string IHouse.DestinationPort => houseBill.ABL_RL_NKFinalDestination;

		string IHouse.NotifyRegNumber => houseBill.ABL_NotifyPartyRegNo;

		string IHouse.NotifyID => houseBill.ABL_NotifyPartyName;

		decimal IHouse.FreightCost => houseBill.ABL_FreightValue;

		string IHouse.CurrencyFreightCost => ZZRefCusMapCombined.MapCW1CodeToCustomsCode(houseBill.Factory, Core.Constants.CountryCodes.Brazil, RefCusMapTypeList.Codes.Currency, houseBill.ABL_RX_NKFreightValueCurrency, ZDateTime.Today);

		string IHouse.PaymentCode => !house.BLServiceIndicator ? houseBill.ABL_PrepaidCollect == Core.Constants.PaymentType.Prepaid ? MercanteConstants.Prepaid : MercanteConstants.Collect : string.Empty;

		string IHouse.DeliveryMode => !house.BLServiceIndicator ? house.ContainerIndicator ? MercanteConstants.NotApplicable : houseBill.FRTMode.ToString() : string.Empty;

		string IHouse.CargoClass => house.DeliveryPlaceAbroad ? MercanteConstants.CargoClassTransitChar : MercanteConstants.CargoClassImportChar;

		string IHouse.DestinationCountryState => houseBill.FinalDestination?.CountryStates?.RW_Code ?? ZString.Empty;

		string IHouse.DischargePortTerminalOperator => houseBill.ABL_GoodsLocation;

		string IHouse.OriginCountryCode => houseBill.ABL_RN_NKSellerCountry;

		bool IHouse.BLServiceIndicator => houseBill.BL_Service;

		string IHouse.OriginalCEMercanteNumber => houseBill.CustomsOwnNumber;

		bool IHouse.ContainerIndicator => houseBill.Header.AMA_ContainerMode == Core.Constants.ContainerModes.Containerised;

		IReadOnlyCollection<IContainer> IHouse.Containers
		{
			get
			{
				var result = new List<IContainer>();
				if (house.ContainerIndicator)
				{
					foreach (AsycudaPack pack in houseBill.Packs)
					{
						var container = pack.Container;
						if (container != null)
						{
							result.Add(new ContainerWrapper(container));
						}
					}
				}
				return result.ToArray();
			}
		}

		IReadOnlyCollection<IFreight> IHouse.Freights
		{
			get
			{
				var result = new List<IFreight>();

				foreach (AsycudaTax tax in houseBill.AsycudaTaxes)
				{
					result.Add(new FreightWrapper(tax));
				}

				return result.ToArray();
			}
		}

		IReadOnlyCollection<IPack> IHouse.Packs
		{
			get
			{
				var result = new List<IPack>();

				foreach (AsycudaPack pack in houseBill.Packs)
				{
					result.Add(new PackWrapper(pack));
				}

				return result.ToArray();
			}
		}
	}
}
