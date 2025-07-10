using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.AR.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.AR.Manifest.Business
{
	internal class HouseConsignmentWrapper : IHouseConsignment
	{
		internal HouseConsignmentWrapper(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, "asycudaBill cannot be null");
			header = bill.Header;
		}
		readonly AsycudaBill bill;
		readonly AsycudaManifestHeader header;

		bool IHouseConsignment.NilCarriageValueIndicator => bill.ABL_TransportValue > 0;

		decimal IHouseConsignment.DeclaredValueForCarriageAmount => bill.ABL_TransportValue.Truncate(2);

		string IHouseConsignment.DeclaredValueForCarriageAmountCurrencyID => bill.ABL_RX_NKTransportValueCurrency;

		bool IHouseConsignment.NilCustomsValueIndicator => bill.ABL_CustomsValue > 0;

		decimal IHouseConsignment.DeclaredValueForCustomsAmount => bill.ABL_CustomsValue.Truncate(2);

		string IHouseConsignment.DeclaredValueForCustomsAmountCurrencyID => bill.ABL_RX_NKCustomsValueCurrency;

		bool IHouseConsignment.NilInsuranceValueIndicator => bill.ABL_InsuranceValue > 0;

		decimal IHouseConsignment.InsuranceValueAmount => bill.ABL_InsuranceValue.Truncate(2);

		string IHouseConsignment.InsuranceValueAmountCurrencyID => bill.ABL_RX_NKFreightValueCurrency;

		decimal IHouseConsignment.IncludedTareGrossWeightMeasure => ARHelperClass.WeightConvertion(bill.ABL_GrossWeightUQ, bill.ABL_GrossWeight);

		string IHouseConsignment.IncludedTareGrossWeightMeasureUnitCode => ARHelperClass.WeightUnitCodeCalculator(bill.ABL_GrossWeightUQ);

		decimal IHouseConsignment.GrossVolumeMeasure => ARHelperClass.VolumeConvertion(bill.ABL_VolumeUQ, bill.ABL_Volume);

		string IHouseConsignment.GrossVolumeMeasureUnitCode => ARHelperClass.VolumeUnitCodeCalculator(bill.ABL_VolumeUQ);

		decimal IHouseConsignment.TotalPieceQuantity => bill.Packs.Count;

		IParty IHouseConsignment.ConsignorParty => consignorParty ?? (consignorParty = new ConsignorPartyWrapper(bill));
		IParty consignorParty;

		IParty IHouseConsignment.ConsigneeParty => consigneeParty ?? (consigneeParty = new ConsigneePartyWrapper(bill));
		IParty consigneeParty;

		IParty IHouseConsignment.FreightForwarderParty => freightForwarderParty ?? (freightForwarderParty = new FreightForwarderPartyWrapper(MasterFiles.Business.GlbCompany.CurrentCompany));
		IParty freightForwarderParty;

		ILocation IHouseConsignment.OriginLocation => originLocation ?? (originLocation = new LocationWrapper(header.Factory, header.AMA_RL_NKPortOfLoading));
		ILocation originLocation;

		ILocation IHouseConsignment.FinalDestinationLocation => finalDestinationLocation ?? (finalDestinationLocation = new LocationWrapper(header.Factory, header.AMA_RL_NKPortOfDischarge));
		ILocation finalDestinationLocation;

		ILogisticsTransportMovement IHouseConsignment.SpecifiedLogisticsTransportMovement => logisticsTransportMovement ?? (logisticsTransportMovement = new LogisticsTransportMovementWrapper(header));
		ILogisticsTransportMovement logisticsTransportMovement;

		decimal IHouseConsignment.ValuationTotalChargeAmount => bill.ABL_FreightValue.Truncate(2);

		string IHouseConsignment.ValuationTotalChargeAmountCurrencyID => bill.ABL_RX_NKFreightValueCurrency;

		decimal IHouseConsignment.ConsignmentItemQuantity => (ZDecimal)(bill.ABL_ManifestQty);

		string IHouseConsignment.SummaryDescription => bill.ABL_GoodsDescription;

		IReadOnlyCollection<IParty> IHouseConsignment.AssociatedParty => new IParty[] { new AssociatedPartyWrapper(bill) };

		IReadOnlyCollection<IItem> IHouseConsignment.IncludedItems
		{
			get
			{
				var result = new List<IItem>();

				foreach (AsycudaPack pack in bill.Packs)
				{
					result.Add(new ItemWrapper(pack));
				}

				return result.ToArray();
			}
		}
	}
}
