using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test;

[TestedType(typeof(AsycudaBillSynchroniser))]
sealed class AsycudaBillSynchroniserTest : Customs.Business.Testing.ManifestBillSynchroniserTest
{
	public void TestConsignorSynchroniser()
	{
		var header = Factory.New<AsycudaManifestHeader>();

		header.AMA_ParentId = Consol.PK;
		header.AMA_ParentTableCode = Consol.TablePrefix;

		Shipment.ConsignorDocumentaryAddress.E2_OA_Address = Address.PK;

		header.Synchroniser.Synchronise(true);

		var bill = header.Bills[0];
		AssertEquals(Address.PK, bill.ABL_OA_Shipper);

		Shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
		AssertEquals(ZGuid.Empty, bill.ABL_OA_Shipper);
		AssertEquals(ZGuid.Empty, bill.ShipperOrgPK);

		Shipment.ConsignorDocumentaryAddress.E2_CompanyName = "Company Name";
		AssertEquals("Company Name", bill.ABL_ShipperName);

		Shipment.ConsignorDocumentaryAddress.E2_Address1 = "Address 1";
		AssertEquals("Address 1", bill.ABL_ShipperStreet1);

		Shipment.ConsignorDocumentaryAddress.E2_Address2 = "Address 2";
		AssertEquals("Address 2", bill.ABL_ShipperStreet2);

		Shipment.ConsignorDocumentaryAddress.E2_City = "My City";
		AssertEquals("My City", bill.ABL_ShipperCity);

		Shipment.ConsignorDocumentaryAddress.E2_State = "BY";
		AssertEquals("BY", bill.ABL_ShipperState);

		Shipment.ConsignorDocumentaryAddress.E2_Postcode = "12345";
		AssertEquals("12345", bill.ABL_ShipperPostcode);

		Shipment.ConsignorDocumentaryAddress.E2_RN_NKCountryCode = "DE";
		AssertEquals("DE", bill.ABL_RN_NKShipperCountry);

		Shipment.ConsignorDocumentaryAddress.E2_AddressOverride = false;
		Shipment.ConsignorDocumentaryAddress.E2_OA_Address = Address.PK;
		Factory.Save();

		AssertEquals(Address.PK, bill.ABL_OA_Shipper);
		AssertEquals("A Company Name", bill.ABL_ShipperName);
		AssertEquals("A Address 1", bill.ABL_ShipperStreet1);
		AssertEquals("A Address 2", bill.ABL_ShipperStreet2);
		AssertEquals("A City", bill.ABL_ShipperCity);
		AssertEquals("15", bill.ABL_ShipperState);
		AssertEquals("23456", bill.ABL_ShipperPostcode);
		AssertEquals("LV", bill.ABL_RN_NKShipperCountry);

		Shipment.ConsignorDocumentaryAddress.OrganisationPK = ZGuid.Empty;
		Factory.Save();

		AssertEquals(ZGuid.Empty, bill.ABL_OA_Shipper);
		AssertEquals(ZString.Empty, bill.ABL_ShipperName);
		AssertEquals(ZString.Empty, bill.ABL_ShipperStreet1);
		AssertEquals(ZString.Empty, bill.ABL_ShipperStreet2);
		AssertEquals(ZString.Empty, bill.ABL_ShipperCity);
		AssertEquals(ZString.Empty, bill.ABL_ShipperState);
		AssertEquals(ZString.Empty, bill.ABL_ShipperPostcode);
		AssertEquals(ZString.Empty, bill.ABL_RN_NKShipperCountry);
	}

	public void TestConsigneeSynchroniser()
	{
		var header = Factory.New<AsycudaManifestHeader>();

		header.AMA_ParentId = Consol.PK;
		header.AMA_ParentTableCode = Consol.TablePrefix;

		Shipment.ConsigneeDocumentaryAddress.E2_OA_Address = Address.PK;

		header.Synchroniser.Synchronise(true);

		var bill = header.Bills[0];
		AssertEquals(Address.PK, bill.ABL_OA_Consignee);

		Shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
		AssertEquals(ZGuid.Empty, bill.ABL_OA_Consignee);
		AssertEquals(ZGuid.Empty, bill.ConsigneeOrgPK);

		Shipment.ConsigneeDocumentaryAddress.E2_CompanyName = "Company Name";
		AssertEquals("Company Name", bill.ABL_ConsigneeName);

		Shipment.ConsigneeDocumentaryAddress.E2_Address1 = "Address 1";
		AssertEquals("Address 1", bill.ABL_ConsigneeStreet1);

		Shipment.ConsigneeDocumentaryAddress.E2_Address2 = "Address 2";
		AssertEquals("Address 2", bill.ABL_ConsigneeStreet2);

		Shipment.ConsigneeDocumentaryAddress.E2_City = "My City";
		AssertEquals("My City", bill.ABL_ConsigneeCity);

		Shipment.ConsigneeDocumentaryAddress.E2_State = "BY";
		AssertEquals("BY", bill.ABL_ConsigneeState);

		Shipment.ConsigneeDocumentaryAddress.E2_Postcode = "12345";
		AssertEquals("12345", bill.ABL_ConsigneePostcode);

		Shipment.ConsigneeDocumentaryAddress.E2_RN_NKCountryCode = "DE";
		AssertEquals("DE", bill.ABL_RN_NKConsigneeCountry);

		Shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = false;
		Shipment.ConsigneeDocumentaryAddress.E2_OA_Address = Address.PK;
		Factory.Save();

		AssertEquals(Address.PK, bill.ABL_OA_Consignee);
		AssertEquals("A Company Name", bill.ABL_ConsigneeName);
		AssertEquals("A Address 1", bill.ABL_ConsigneeStreet1);
		AssertEquals("A Address 2", bill.ABL_ConsigneeStreet2);
		AssertEquals("A City", bill.ABL_ConsigneeCity);
		AssertEquals("15", bill.ABL_ConsigneeState);
		AssertEquals("23456", bill.ABL_ConsigneePostcode);

		Shipment.ConsigneeDocumentaryAddress.OrganisationPK = ZGuid.Empty;
		Factory.Save();

		AssertEquals(ZGuid.Empty, bill.ABL_OA_Consignee);
		AssertEquals(ZString.Empty, bill.ABL_ConsigneeName);
		AssertEquals(ZString.Empty, bill.ABL_ConsigneeStreet1);
		AssertEquals(ZString.Empty, bill.ABL_ConsigneeStreet2);
		AssertEquals(ZString.Empty, bill.ABL_ConsigneeCity);
		AssertEquals(ZString.Empty, bill.ABL_ConsigneeState);
		AssertEquals(ZString.Empty, bill.ABL_ConsigneePostcode);
		AssertEquals(ZString.Empty, bill.ABL_RN_NKConsigneeCountry);
	}

	public void TestNotifyPartySynchroniser()
	{
		var header = Factory.New<AsycudaManifestHeader>();

		header.AMA_ParentId = Consol.PK;
		header.AMA_ParentTableCode = Consol.TablePrefix;

		Shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = Address.PK;

		header.Synchroniser.Synchronise(true);

		var bill = header.Bills[0];
		AssertEquals(Address.PK, bill.ABL_OA_NotifyParty);

		Shipment.NotifyPartyDocumentaryAddress.E2_AddressOverride = true;
		AssertEquals(ZGuid.Empty, bill.ABL_OA_NotifyParty);
		AssertEquals(ZGuid.Empty, bill.NotifyPartyOrgPK);

		Shipment.NotifyPartyDocumentaryAddress.E2_CompanyName = "Company Name";
		AssertEquals("Company Name", bill.ABL_NotifyPartyName);

		Shipment.NotifyPartyDocumentaryAddress.E2_Address1 = "Address 1";
		AssertEquals("Address 1", bill.ABL_NotifyPartyStreet1);

		Shipment.NotifyPartyDocumentaryAddress.E2_Address2 = "Address 2";
		AssertEquals("Address 2", bill.ABL_NotifyPartyStreet2);

		Shipment.NotifyPartyDocumentaryAddress.E2_City = "My City";
		AssertEquals("My City", bill.ABL_NotifyPartyCity);

		Shipment.NotifyPartyDocumentaryAddress.E2_State = "BY";
		AssertEquals("BY", bill.ABL_NotifyPartyState);

		Shipment.NotifyPartyDocumentaryAddress.E2_Postcode = "12345";
		AssertEquals("12345", bill.ABL_NotifyPartyPostcode);

		Shipment.NotifyPartyDocumentaryAddress.E2_RN_NKCountryCode = "DE";
		AssertEquals("DE", bill.ABL_RN_NKNotifyPartyCountry);

		Shipment.NotifyPartyDocumentaryAddress.E2_AddressOverride = false;
		Shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = Address.PK;
		Factory.Save();

		AssertEquals(Address.PK, bill.ABL_OA_NotifyParty);
		AssertEquals("A Company Name", bill.ABL_NotifyPartyName);
		AssertEquals("A Address 1", bill.ABL_NotifyPartyStreet1);
		AssertEquals("A Address 2", bill.ABL_NotifyPartyStreet2);
		AssertEquals("A City", bill.ABL_NotifyPartyCity);
		AssertEquals("15", bill.ABL_NotifyPartyState);
		AssertEquals("23456", bill.ABL_NotifyPartyPostcode);
		AssertEquals("LV", bill.ABL_RN_NKNotifyPartyCountry);

		Shipment.NotifyPartyDocumentaryAddress.OrganisationPK = ZGuid.Empty;
		Factory.Save();

		AssertEquals(ZGuid.Empty, bill.ABL_OA_NotifyParty);
		AssertEquals(ZString.Empty, bill.ABL_NotifyPartyName);
		AssertEquals(ZString.Empty, bill.ABL_NotifyPartyStreet1);
		AssertEquals(ZString.Empty, bill.ABL_NotifyPartyStreet2);
		AssertEquals(ZString.Empty, bill.ABL_NotifyPartyCity);
		AssertEquals(ZString.Empty, bill.ABL_NotifyPartyState);
		AssertEquals(ZString.Empty, bill.ABL_NotifyPartyPostcode);
		AssertEquals(ZString.Empty, bill.ABL_RN_NKNotifyPartyCountry);
	}

	protected override void AssertDocAddressesSynchronisedResult(IManifestBillForSynchroniser bill, ForwardingShipment shipment)
	{
		Assert("Tested in other cases in this class", true);
	}

	public void TestDestination()
	{
		var consol = Factory.NewWithValidTestData<ForwardingConsol>();
		var shipment = consol.Shipments.AddNew();
		var bill = GetManifestBill(consol);
		var synchroniser = GetManifestBillSynchroniser(bill, shipment) as AsycudaBillSynchroniser;
		AssertType<AsycudaBill>(synchroniser.Destination);
	}

	public void TestSyncABL_PrepaidCollect_HasJobHeaderWithCreditInformation()
	{
		var testCases = new[]
		{
			(null, string.Empty),
			(string.Empty, string.Empty),
			(OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck, EUICS2PaymentMethodList.Codes.C),
			(OrgConstants.CreditAgreedPaymentMethods.Code.CreditCard, EUICS2PaymentMethodList.Codes.B),
			(OrgConstants.CreditAgreedPaymentMethods.Code.BankTransfer, EUICS2PaymentMethodList.Codes.H),
			(OrgConstants.CreditAgreedPaymentMethods.Code.CashAndBankCheck, EUICS2PaymentMethodList.Codes.A),
			(OrgConstants.CreditAgreedPaymentMethods.Code.DebitCard, EUICS2PaymentMethodList.Codes.D),
			(OrgConstants.CreditAgreedPaymentMethods.Code.CollectionRequest, EUICS2PaymentMethodList.Codes.D),
			(OrgConstants.CreditAgreedPaymentMethods.Code.EPayment, EUICS2PaymentMethodList.Codes.H),
		};

		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		Shipment.CreateShipmentJobHeaderWithMutex();
		Shipment.ShipmentJobHeader.JH_OA_LocalChargesAddr_ZAddress.OrgPK = orgHeader.PK;

		var manifestBill = (AsycudaBill)GetManifestBill(Consol);
		var synchroniser = GetManifestBillSynchroniser(manifestBill, Shipment);
		synchroniser.SetEnabled(enabled: true, enableDetection: false);
		foreach (var (paymentMethod, expectedPrepaidCollect) in testCases)
		{
			orgHeader.CompanyData.OB_ARCreditAgreedPaymentMethod = paymentMethod;
			AssertEquals($"Has JobHeader with PaymentInformation: {expectedPrepaidCollect}", expectedPrepaidCollect, manifestBill.ABL_PrepaidCollect);
		}
		Shipment.ShipmentJobHeader.Dispose();
	}

	public void TestSyncABL_PrepaidCollect_NoJobHeader()
	{
		var manifestBill = (AsycudaBill)GetManifestBill(Consol);
		var synchroniser = GetManifestBillSynchroniser(manifestBill, Shipment);
		AssertNull("PreReq", Shipment.JobHeader);

		synchroniser.Synchronise(force: true);
		CombineAssertions(() =>
		{
			AssertNull("No JobHeader created", Shipment.JobHeader);
			AssertEquals("PrepaidCollect not populated", ZString.Empty, manifestBill.ABL_PrepaidCollect);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CompanyData.OB_ARCreditAgreedPaymentMethod = OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck;
			Shipment.CreateShipmentJobHeaderWithMutex();
			Shipment.ShipmentJobHeader.JH_OA_LocalChargesAddr_ZAddress.OrgPK = orgHeader.PK;
			AssertEquals("PrepaidCollect populated", EUICS2PaymentMethodList.Codes.C, manifestBill.ABL_PrepaidCollect);
		});
		Shipment.ShipmentJobHeader.Dispose();
	}

	protected override ZString GetPortOfLading(ForwardingShipment shipment) => shipment.JS_RL_NKOrigin;

	protected override ZString GetPlaceOfReceipt(ForwardingShipment shipment) => shipment.JS_RL_NKDestination;

	protected override ZString GetLastForeignPort(ForwardingShipment shipment) => shipment.JS_RL_NKOrigin;

	protected override BusinessObjectSynchroniser GetManifestBillSynchroniser(IManifestBillForSynchroniser bill, ForwardingShipment shipment) => new AsycudaBillSynchroniser((AsycudaBill)bill, shipment);

	protected override IManifestBillForSynchroniser GetManifestBill(ForwardingConsol consol)
	{
		var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
		header.AMA_ParentId = consol.PK;
		header.AMA_ParentTableCode = consol.TablePrefix;
		var bill = header.Bills.AddNew();
		return bill;
	}

	OrgAddress Address => address ??= GetNewAddress();

	OrgAddress address;

	OrgAddress GetNewAddress()
	{
		var header = Factory.New<OrgHeader>();
		header.OH_Code = "ORG";
		var addr = header.Addresses.AddNew();
		addr.OA_Code = "ADDR";
		addr.ValidationStatus = "INV";
		addr.CompanyName = "A Company Name";
		addr.Address1 = "A Address 1";
		addr.Address2 = "A Address 2";
		addr.City = "A City";
		addr.OA_RN_NKCountryCode = "LV";
		addr.Postcode = "23456";
		addr.State = "15";
		return addr;
	}
}
