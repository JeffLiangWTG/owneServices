using System;
using System.Linq;
using Enterprise.Customs.AE.Business;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Edifact.D23A.Elements;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

sealed class ConsignmentDetailsProviderTest : Customs.Business.Testing.DataProviderTestCase<ConsignmentDetailsProvider>
{
	public void TestConstructor()
	{
		NUnit.Framework.Assert.Throws<ArgumentNullException>(() => new ConsignmentDetailsProvider(null), "Bill is null");
		AssertNoExceptionThrown("Bill is valid", () => new ConsignmentDetailsProvider(bill));
	}

	[ExpectNoExceptions]
	public void TestMonetaryAmounts()
	{
		bill.ABL_FreightValue = 1.2m;
		bill.ABL_RX_NKFreightValueCurrency = "FRT";
		bill.ABL_TransportValue = 2.3m;
		bill.ABL_RX_NKTransportValueCurrency = "TRA";
		bill.ABL_InsuranceValue = 3.4m;
		bill.ABL_RX_NKInsuranceValueCurrency = "INS";
		bill.DiscountValue = 4.5m;
		bill.DiscountValueCurrency = "DIS";
		bill.OtherChargesValue = 5.6m;
		bill.OtherChargesValueCurrency = "OTH";
		bill.ABL_CustomsValue = 6.7m;
		bill.ABL_RX_NKCustomsValueCurrency = "CUS";

		var monetaryAmounts = GetProvider().MonetaryAmounts;

		CombineAssertions(() =>
		{
			AssertMonetaryAmount(MonetaryAmountTypeCodeQualifierList.GoodsItemTotal, 1.2m, "FRT");
			AssertMonetaryAmount(MonetaryAmountTypeCodeQualifierList.TotalFreightDue, 2.3m, "TRA");
			AssertMonetaryAmount(MonetaryAmountTypeCodeQualifierList.InsuranceAmount, 3.4m, "INS");
			AssertMonetaryAmount(MonetaryAmountTypeCodeQualifierList.DiscountAmount, 4.5m, "DIS");
			AssertMonetaryAmount(MonetaryAmountTypeCodeQualifierList.OtherCharges, 5.6m, "OTH");
			AssertMonetaryAmount(MonetaryAmountTypeCodeQualifierList.GoodsItemForCustomsDeclaredValueAmount, 6.7m, "CUS");
		});

		void AssertMonetaryAmount(MonetaryAmountTypeCodeQualifierList type, decimal amount, string currency)
		{
			var monetaryAmount = monetaryAmounts.Single(x => x.AmountType == type);
			NUnit.Framework.Assert.That(monetaryAmount.Amount, Is.EqualTo(amount), $"{nameof(type)} Amount");
			NUnit.Framework.Assert.That(monetaryAmount.Currency, Is.EqualTo(currency), $"{nameof(type)} Currency");
		}
	}

	[ExpectNoExceptions]
	public void TestLocations()
	{
		header.AMA_RL_NKPortOfLoading = "POL";
		header.AMA_RL_NKPortOfDischarge = "POD";
		bill.ABL_RL_NKFinalDestination = "PFD";

		var locations = GetProvider().Locations;

		CombineAssertions(() =>
		{
			AssertLocation(LocationFunctionCodeQualifierList.PlaceOfReceipt, "POL");
			AssertLocation(LocationFunctionCodeQualifierList.PlaceOfLoading, "POL");
			AssertLocation(LocationFunctionCodeQualifierList.PortOfDischarge, "POD");
			AssertLocation(LocationFunctionCodeQualifierList.PlaceOfDelivery, "PFD");
		});

		void AssertLocation(LocationFunctionCodeQualifierList functionCode, string expectedIdentifier)
		{
			var location = locations.Single(x => x.LocationFunctionCode == functionCode);
			NUnit.Framework.Assert.That(location.LocationIdentifier, Is.EqualTo(expectedIdentifier), $"{nameof(functionCode)}");
		}
	}

	[ExpectNoExceptions]
	public void TestLocationsForTransport() => CombineAssertions(() =>
	{
		header.AMA_VesselName = "TEST VESSEL";
		bill.ABL_ShipmentType = ShipmentTypeList.Codes.Export22;
		var routingSupport = header as IRoutingSupport;
		var transport1 = routingSupport.TransportsIncludingRelated.AddNew();
		transport1.JW_VesselForBinding = "TEST VESSEL";
		transport1.JW_RL_NKLoadPortForBinding = "POL";
		var transport2 = routingSupport.TransportsIncludingRelated.AddNew();
		transport2.JW_VesselForBinding = "OTHER VESSEL";
		transport2.JW_RL_NKLoadPortForBinding = "PO1";

		NUnit.Framework.Assert.That(GetProvider().Locations.Count(x => x.LocationFunctionCode == LocationFunctionCodeQualifierList.PlaceOfTranshipment), Is.EqualTo(0), "Transport location count");

		bill.ABL_ShipmentType = ShipmentTypeList.Codes.Transhipment28;
		NUnit.Framework.Assert.That(GetProvider().Locations.Count(x => x.LocationFunctionCode == LocationFunctionCodeQualifierList.PlaceOfTranshipment), Is.EqualTo(1), "Transport location count");
		NUnit.Framework.Assert.That(GetProvider().Locations.Single(x => x.LocationFunctionCode == LocationFunctionCodeQualifierList.PlaceOfTranshipment).LocationIdentifier, Is.EqualTo("PO1"), "Transport location identifier");
	});

	[ExpectNoExceptions]
	public void TestParties()
	{
		bill.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;
		bill.ABL_OA_Shipper = CreateParty("Shipper").PK;
		bill.ABL_OA_Consignee = CreateParty("Consignee").PK;
		bill.ABL_OA_NotifyParty = CreateParty("NotifyParty").PK;
		bill.ABL_OA_ContainerAgent = CreateParty("ContainerAgent").PK;
		bill.ABL_OA_DeliveryAgent = CreateParty("DeliveryAgent").PK;

		var parties = GetProvider().Parties;

		CombineAssertions(() =>
		{
			AssertParty(PartyFunctionCodeQualifierList.Consignor, "Shipper");
			AssertParty(PartyFunctionCodeQualifierList.Consignee, "Consignee");
			AssertParty(PartyFunctionCodeQualifierList.NotifyParty, "NotifyParty");
			AssertParty(PartyFunctionCodeQualifierList.ConsignorsFreightForwarder, "ContainerAgent");
			AssertParty(PartyFunctionCodeQualifierList.DeliveryParty, "DeliveryAgent");

			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Export22;
			bill.ABL_OA_DeliveryAgent = CreateParty("DeliveryAgent").PK;
			parties = GetProvider().Parties;
			NUnit.Framework.Assert.That(!parties.Any(x => x.PartyFunctionCode == "DeliveryAgent"), "DeliveryAgent not set - should be [null]");
		});

		OrgAddress CreateParty(string partyName)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = partyName;
			return org.Addresses.AddNew();
		}

		void AssertParty(PartyFunctionCodeQualifierList functionCode, string expectedName)
		{
			var party = parties.Single(x => x.PartyFunctionCode == functionCode);
			NUnit.Framework.Assert.That(party.PartyName, Is.EqualTo(expectedName), $"{expectedName}");
		}
	}

	[ExpectNoExceptions]
	public void TestManifestNature()
	{
		CombineAssertions(() => {
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			NUnit.Framework.Assert.That(GetProvider().ManifestNature, Is.EqualTo(AEConstants.Messaging.ManifestNatureCodes.Import23));

			header.AMA_Nature = ShipmentTypeList.Codes.Transhipment28;
			NUnit.Framework.Assert.That(GetProvider().ManifestNature, Is.EqualTo(AEConstants.Messaging.ManifestNatureCodes.Transhipment28));

			header.AMA_Nature = ShipmentTypeList.Codes.Transit24;
			NUnit.Framework.Assert.That(GetProvider().ManifestNature, Is.EqualTo(AEConstants.Messaging.ManifestNatureCodes.Transit24));

			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			NUnit.Framework.Assert.That(GetProvider().ManifestNature, Is.Null.Or.Empty, "Export not set - should be [null] or [empty]");
		});
	}

	[ExpectNoExceptions]
	public void TestPacks()
	{
		bill.Packs.AddNew();
		bill.Packs.AddNew();
		CombineAssertions(() => {
			NUnit.Framework.Assert.That(GetProvider().Packs.Count, Is.EqualTo(2), "Packs count");
			NUnit.Framework.Assert.That(GetProvider().Packs.All(x => x is GoodsInfoProvider), Is.EqualTo(true), "Pack Type");
		});
	}

	protected override ConsignmentDetailsProvider GetProvider() => new ConsignmentDetailsProvider(bill);

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<AsycudaManifestHeader>();
		bill = header.Bills.AddNew();
	}
	AsycudaManifestHeader header;
	AsycudaBill bill;
}
