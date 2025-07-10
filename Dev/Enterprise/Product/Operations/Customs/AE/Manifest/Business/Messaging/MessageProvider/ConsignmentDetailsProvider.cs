using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AE.Business;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Edifact.D23A.Elements;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AE.Manifest.Business;

sealed class ConsignmentDetailsProvider : IConsignmentDetailsProvider
{
	public ConsignmentDetailsProvider(AsycudaBill bill)
	{
		Bill = Argument.NotNull(bill, nameof(bill));
		Header = bill.Header;
	}
	AsycudaBill Bill { get; }
	AsycudaManifestHeader Header { get; }

	public IReadOnlyCollection<IMonetaryAmountProvider> MonetaryAmounts => monetaryAmounts ??= GetMonetaryAmounts();
	IReadOnlyCollection<IMonetaryAmountProvider> monetaryAmounts;

	public IReadOnlyCollection<ILocationProvider> Locations => locations ??= GetLocations();
	IReadOnlyCollection<ILocationProvider> locations;

	public IReadOnlyCollection<IPartyFromOrgAddressProvider> Parties => parties ??= GetParties();
	IReadOnlyCollection<IPartyFromOrgAddressProvider> parties;

	public IReadOnlyCollection<IGoodsInfoProvider> Packs => packs ??= GetPacks();
	IReadOnlyCollection<IGoodsInfoProvider> packs;

	public string ManifestNature => CachedValueHelper.GetValue(ref manifestNature, () => GetManifestNature());
	CachedValue<string> manifestNature;

	public string GetManifestNature() => (string)Header.AMA_Nature switch
	{
		ShipmentTypeList.Codes.Import23 => AEConstants.Messaging.ManifestNatureCodes.Import23,
		ShipmentTypeList.Codes.Transhipment28 => AEConstants.Messaging.ManifestNatureCodes.Transhipment28,
		ShipmentTypeList.Codes.Transit24 => AEConstants.Messaging.ManifestNatureCodes.Transit24,
		_ => null
	};

	List<MonetaryAmountProvider> GetMonetaryAmounts()
	{
		var monetaryAmounts = new List<MonetaryAmountProvider>();
		AddIfPresent(MonetaryAmountTypeCodeQualifierList.GoodsItemTotal, Bill.ABL_FreightValue, Bill.ABL_RX_NKFreightValueCurrency);
		AddIfPresent(MonetaryAmountTypeCodeQualifierList.TotalFreightDue, Bill.ABL_TransportValue, Bill.ABL_RX_NKTransportValueCurrency);
		AddIfPresent(MonetaryAmountTypeCodeQualifierList.InsuranceAmount, Bill.ABL_InsuranceValue, Bill.ABL_RX_NKInsuranceValueCurrency);
		AddIfPresent(MonetaryAmountTypeCodeQualifierList.DiscountAmount, Bill.DiscountValue, Bill.DiscountValueCurrency);
		AddIfPresent(MonetaryAmountTypeCodeQualifierList.OtherCharges, Bill.OtherChargesValue, Bill.OtherChargesValueCurrency);
		AddIfPresent(MonetaryAmountTypeCodeQualifierList.GoodsItemForCustomsDeclaredValueAmount, Bill.ABL_CustomsValue, Bill.ABL_RX_NKCustomsValueCurrency);

		return monetaryAmounts;

		void AddIfPresent(string type, ZDecimal amount, ZString currency)
		{
			if (!amount.IsEmpty)
			{
				monetaryAmounts.Add(new MonetaryAmountProvider(type, amount, currency));
			}
		}
	}

	List<LocationProvider> GetLocations()
	{
		var locations = new List<LocationProvider>
		{
			new (LocationFunctionCodeQualifierList.PlaceOfReceipt, Header.AMA_RL_NKPortOfLoading),
			new (LocationFunctionCodeQualifierList.PlaceOfLoading, Header.AMA_RL_NKPortOfLoading),
		};

		if (Bill.IsTSS && Header is IRoutingSupport routingSupport)
		{
			var vesselName = Header.AMA_VesselName;
			foreach (Transport transport in routingSupport.TransportsIncludingRelated)
			{
				if (transport.JW_VesselForBinding != vesselName)
				{
					locations.Add(new LocationProvider(LocationFunctionCodeQualifierList.PlaceOfTranshipment, transport.JW_RL_NKLoadPortForBinding));
				}
			}
		}
		locations.Add(new LocationProvider(LocationFunctionCodeQualifierList.PortOfDischarge, Header.AMA_RL_NKPortOfDischarge));
		locations.Add(new LocationProvider(LocationFunctionCodeQualifierList.PlaceOfDelivery, Bill.ABL_RL_NKFinalDestination));
		return locations;
	}

	List<PartyFromOrgAddressProvider> GetParties()
	{
		var parties = new List<PartyFromOrgAddressProvider>();

		AddIfPresent(Bill.Shipper, PartyFunctionCodeQualifierList.Consignor);
		AddIfPresent(Bill.Consignee, PartyFunctionCodeQualifierList.Consignee);
		AddIfPresent(Bill.NotifyParty, PartyFunctionCodeQualifierList.NotifyParty);
		AddIfPresent(Bill.ContainerAgent, PartyFunctionCodeQualifierList.ConsignorsFreightForwarder);
		if (Bill.IsImport)
		{
			AddIfPresent(Bill.DeliveryAgent, PartyFunctionCodeQualifierList.DeliveryParty);
		}

		return parties;

		void AddIfPresent(OrgAddress orgAddress, string functionCode)
		{
			if (orgAddress != null)
			{
				parties.Add(new PartyFromOrgAddressProvider(orgAddress, functionCode));
			}
		}
	}

	List<GoodsInfoProvider> GetPacks() => Bill.Packs.Cast<AsycudaPack>().Select(x => new GoodsInfoProvider(x)).ToList();
}
