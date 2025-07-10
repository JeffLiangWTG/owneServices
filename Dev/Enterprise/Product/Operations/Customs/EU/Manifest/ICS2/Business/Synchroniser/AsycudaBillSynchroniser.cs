using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business;

public class AsycudaBillSynchroniser(ASYCUDA.Business.AsycudaBill destination, ForwardingShipment shipmentSource)
	: ASYCUDA.Business.AsycudaBillSynchroniser(destination, shipmentSource)
{
	protected override void AddConsignorSynchroniser()
	{
		AddAddressSynchorniserWithOverride(Destination.ABL_OA_ShipperInfo, Destination.ABL_ShipperNameInfo, Destination.ABL_ShipperStreet1Info, Destination.ABL_ShipperStreet2Info, Destination.ABL_ShipperCityInfo, Destination.ABL_ShipperStateInfo, Destination.ABL_ShipperPostcodeInfo, Destination.ABL_RN_NKShipperCountryInfo, () => Source.DocAddresses.FindByDocAddressType(DocAddressType.ConsignorDocumentaryAddress));
	}

	protected override void AddNotifyPartySynchroniser()
	{
		AddAddressSynchorniserWithOverride(Destination.ABL_OA_NotifyPartyInfo, Destination.ABL_NotifyPartyNameInfo, Destination.ABL_NotifyPartyStreet1Info, Destination.ABL_NotifyPartyStreet2Info, Destination.ABL_NotifyPartyCityInfo, Destination.ABL_NotifyPartyStateInfo, Destination.ABL_NotifyPartyPostcodeInfo, Destination.ABL_RN_NKNotifyPartyCountryInfo, () => Source.DocAddresses.FindByDocAddressType(DocAddressType.NotifyParty));
	}

	protected override void AddConsigneeSynchroniser()
	{
		AddAddressSynchorniserWithOverride(Destination.ABL_OA_ConsigneeInfo, Destination.ABL_ConsigneeNameInfo, Destination.ABL_ConsigneeStreet1Info, Destination.ABL_ConsigneeStreet2Info, Destination.ABL_ConsigneeCityInfo, Destination.ABL_ConsigneeStateInfo, Destination.ABL_ConsigneePostcodeInfo, Destination.ABL_RN_NKConsigneeCountryInfo, () => Source.DocAddresses.FindByDocAddressType(DocAddressType.ConsigneeDocumentaryAddress));
	}

	readonly List<FieldSynchroniser> addressSynchronisers = new();

	void AddAddressSynchorniserWithOverride(ZPropertyInfo addressPKInfo,
		ZPropertyInfo partyNameInfo, ZPropertyInfo partyStreet1Info, ZPropertyInfo partyStreet2Info,
		ZPropertyInfo partyCityInfo, ZPropertyInfo partyStateInfo, ZPropertyInfo partyPostcodeInfo,
		ZPropertyInfo partyCountryInfo, Func<JobDocAddress> source)
	{
		foreach (var fieldSynchroniser in GetAddressSynchorniserWithOverride(addressPKInfo, partyNameInfo, partyStreet1Info, partyStreet2Info, partyCityInfo, partyStateInfo, partyPostcodeInfo, partyCountryInfo, source))
		{
			addressSynchronisers.Add(fieldSynchroniser);
			Synchronisers.Add(fieldSynchroniser);
		}
	}

	IEnumerable<FieldSynchroniser> GetAddressSynchorniserWithOverride(ZPropertyInfo addressPKInfo, ZPropertyInfo partyNameInfo, ZPropertyInfo partyStreet1Info, ZPropertyInfo partyStreet2Info, ZPropertyInfo partyCityInfo, ZPropertyInfo partyStateInfo, ZPropertyInfo partyPostcodeInfo, ZPropertyInfo partyCountryInfo, Func<JobDocAddress> source)
	{
		yield return new FieldSynchroniser(addressPKInfo, () => source()?.RealAddress?.PK ?? ZGuid.Empty, () =>
		{
			var address = source();
			return address == null ? [] : new[] { address.E2_OA_AddressInfo, address.E2_AddressOverrideInfo, address.OrganisationPKInfo };
		});
		yield return new FieldSynchroniser(partyNameInfo, () => source()?.E2_CompanyName ?? ZString.Empty,
		() =>
		{
			var address = source();
			return address == null ? [] : new[] { address.E2_CompanyNameInfo, address.E2_OA_AddressInfo, address.E2_AddressOverrideInfo };
		});
		yield return new FieldSynchroniser(partyStreet1Info, () => source()?.E2_Address1 ?? ZString.Empty,
		() =>
		{
			var address = source();
			return address == null ? [] : new[] { address.E2_Address1Info, address.E2_OA_AddressInfo, address.E2_AddressOverrideInfo };
		});
		yield return new FieldSynchroniser(partyStreet2Info, () => source()?.E2_Address2 ?? ZString.Empty,
		() =>
		{
			var address = source();
			return address == null ? [] : new[] { address.E2_Address2Info, address.E2_OA_AddressInfo, address.E2_AddressOverrideInfo };
		});
		yield return new FieldSynchroniser(partyCityInfo, () => source()?.E2_City ?? ZString.Empty,
		() =>
		{
			var address = source();
			return address == null ? [] : new[] { address.E2_CityInfo, address.E2_OA_AddressInfo, address.E2_AddressOverrideInfo };
		});
		yield return new FieldSynchroniser(partyStateInfo, () => source()?.E2_State ?? ZString.Empty,
		() =>
		{
			var address = source();
			return address == null ? [] : new[] { address.E2_StateInfo, address.E2_OA_AddressInfo, address.E2_AddressOverrideInfo };
		});
		yield return new FieldSynchroniser(partyPostcodeInfo, () => source()?.E2_Postcode ?? ZString.Empty,
		() =>
		{
			var address = source();
			return address == null ? [] : new[] { address.E2_PostcodeInfo, address.E2_OA_AddressInfo, address.E2_AddressOverrideInfo };
		});
		yield return new FieldSynchroniser(partyCountryInfo, () => source()?.E2_RN_NKCountryCode ?? ZString.Empty,
		() =>
		{
			var address = source();
			return address == null ? [] : new[] { address.E2_RN_NKCountryCodeInfo, address.E2_OA_AddressInfo, address.E2_AddressOverrideInfo };
		});
	}

	protected override void HookSynchronisers()
	{
		base.HookSynchronisers();
		Source.DocAddresses.CountChanged -= DocAddressesOnCountChanged;
		Source.DocAddresses.CountChanged += DocAddressesOnCountChanged;
	}

	void DocAddressesOnCountChanged(object sender, CollectionCountChangedEventArgs e)
	{
		foreach (var synchroniser in addressSynchronisers)
		{
			synchroniser.UpdateInfoEventsAndReSynchronise();
		}
	}

	protected override void UnHookSynchronisers()
	{
		base.UnHookSynchronisers();
		Source.DocAddresses.CountChanged -= DocAddressesOnCountChanged;
		addressSynchronisers.Clear();
	}

	public new AsycudaBill Destination => (AsycudaBill)base.Destination;

	protected override ASYCUDA.Business.AsycudaPackCollectionSynchroniser GetAsycudaPackCollectionSynchroniser() => new AsycudaPackCollectionSynchroniser(Source, Destination);

	protected override IZType GetPaymentType() => Source.ShipmentJobHeader is { } job && job.JH_OA_LocalChargesAddr_ZAddress.OrgHeader is OrgHeader orgHeader
			? (ZString)MapToPayment(orgHeader.CompanyData.OB_ARCreditAgreedPaymentMethod)
			: ZString.Empty;

	string MapToPayment(string creditAgreedPaymentMethod)
	{
		var codeDescriptionPairList = OrganisationsDataRegistry.Instance.ReceivablesCreditAgreedPaymentMethodsList.Value.GetCodeDescriptionPairList();
		var description = codeDescriptionPairList.GetDescriptionFromCode(creditAgreedPaymentMethod);
		if (string.IsNullOrEmpty(description))
		{
			return string.Empty;
		}

		var possibleTokens = new[]
		{
			(tokens: (NoResString[])[(NoResString)"cash"], paymentMethod: EUICS2PaymentMethodList.Codes.A),
			(tokens: [(NoResString)"credit card"], paymentMethod: EUICS2PaymentMethodList.Codes.B),
			(tokens: [(NoResString)"cheque", (NoResString)"check"], paymentMethod: EUICS2PaymentMethodList.Codes.C),
			(tokens: [(NoResString)"bank transfer", (NoResString)"e-payment", (NoResString)"electronic funds transfer"], paymentMethod: EUICS2PaymentMethodList.Codes.H),
		};

		var lastFoundIndex = int.MaxValue;
		var lastFoundPaymentMethod = EUICS2PaymentMethodList.Codes.D;
		foreach (var (tokens, paymentMethod) in possibleTokens)
		{
			foreach (var token in tokens)
			{
				var index = description.IndexOf(token, StringComparison.InvariantCultureIgnoreCase);
				if (index != -1 && index < lastFoundIndex)
				{
					lastFoundIndex = index;
					lastFoundPaymentMethod = paymentMethod;
				}
			}
		}

		return lastFoundPaymentMethod;
	}

	protected override IEnumerable<ZPropertyInfo> GetPaymentInfo()
	{
		yield return Source.ShipmentJobHeaderPKInfo;
		if (Source.ShipmentJobHeader is { } jobHeader)
		{
			yield return jobHeader.JH_OA_LocalChargesAddr_ZAddress.OrgPKInfo;

			if (jobHeader.JH_OA_LocalChargesAddr_ZAddress.OrgHeader is OrgHeader orgHeader)
			{
				yield return orgHeader.CompanyData.OB_ARCreditAgreedPaymentMethodInfo;
			}
		}
	}
}
