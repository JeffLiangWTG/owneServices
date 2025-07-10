using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(ReleaseShipmentDetailsWrapper))]
sealed class ReleaseShipmentDetailsWrapperTest : NonPersistentBusinessObjectTestCase
{
	protected override BusinessObject GetNewBusinessObject() => new ReleaseShipmentDetailsWrapper(cusEntryHeader, declaration);

	public void TestConstructor() => AssertExceptionThrown<ArgumentNullException>(() => new ReleaseShipmentDetailsWrapper(null, null));

	public void TestID() => AssertEquals("SHIPMENTID", wrapper.ID);

	public void TestShipperSupplier() => AssertType<AddressInformationDocumentWrapper>(wrapper.ShipperSupplier);

	public void TestConsignee() => AssertType<AddressInformationDocumentWrapper>(wrapper.Consignee);

	public void TestPackages()
	{
		var pack1 = invoiceLine1.PackagesPivot.AddNew();
		pack1.CHC_NumberOfPacks = 9;
		var pack2 = invoiceLine1.PackagesPivot.AddNew();
		pack2.CHC_NumberOfPacks = 11;

		AssertEquals(20, wrapper.Packages);
	}

	public void TestWeight() => AssertEquals(217.153m, wrapper.Weight);

	public void TestOrigin() => AssertEquals((ZString)"CHSHA", wrapper.Origin);

	public void TestDestination() => AssertEquals((ZString)"NLRTM", wrapper.Destination);

	public void TestMaster() => AssertEquals((ZString)"MASTER123", wrapper.Master);

	public void TestHouse() => AssertEquals((ZString)"HOUSE123", wrapper.House);

	public void TestContainers() => AssertEquals("MSCU1234566,APLU6543218", wrapper.Containers);

	public void TestMrn() => AssertEquals((ZString)"MRN1234567890", wrapper.Mrn);

	public void TestGoodsLocation()
	{
		CombineAssertions(() =>
		{
			AssertType<ReleaseDeclarationGoodsLocationWrapper>(wrapper.GoodsLocation);
			AssertEquals("TypeCode (Qualifier) when goodslocation from declaration", "Z", wrapper.GoodsLocation.TypeCode);

			var goodsLocation = entryInstruction.GoodsLocation;
			goodsLocation.CGL_Qualifier = "U";

			AssertType<ReleaseDeclarationGoodsLocationWrapper>(wrapper.GoodsLocation);
			AssertEquals("TypeCode (Qualifier) when goodsLocation from entryInstruction", "U", wrapper.GoodsLocation.TypeCode);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		var shipment = Factory.New<ForwardingShipment>();
		shipment.JS_BookingReference = "SHIPMENTID";

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_JS = shipment.PK;
		declaration.JE_TotalWeight = 217.153m;
		declaration.JE_RL_NKOrigin = "CHSHA";
		declaration.JE_RL_NKFinalDestination = "NLRTM";
		declaration.JE_HouseBill = "HOUSE123";
		declaration.JE_MasterBill = "MASTER123";

		var goodsLocation = declaration.GoodsLocation;
		goodsLocation.CGL_Qualifier = "Z";
		goodsLocation.CGL_Type = "A";
		goodsLocation.Address.Address1 = "Street 5";
		goodsLocation.Address.Postcode = "1234AB";
		goodsLocation.Address.E2_RN_NKCountryCode = "NL";

		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Style = "H1";

		declaration.Bills.Add(GetBill("MB", "MASTER001"));
		declaration.Bills.Add(GetBill("HB", "HOUSE001"));
		declaration.Bills.Add(GetBill("HB", "HOUSE002"));

		declaration.JE_OA_ShipperAddress = GetShipper().MainAddress.PK;
		var importer = GetImporter();
		declaration.JE_OH_Consignee = importer.PK;
		declaration.JE_OA_ConsigneeAddress = importer.MainAddress.PK;

		declaration.CusContainers.Add(GetContainer("MSCU1234566"));
		declaration.CusContainers.Add(GetContainer("APLU6543218"));

		var invoice1 = declaration.Invoices.AddNew();
		invoice1.JZ_InvoiceNumber = "INV001";
		invoiceLine1 = invoice1.InvoiceLines.AddNew();
		invoiceLine1.JI_Description = "Smartwatch";

		Factory.Save();
		var shutterUpperer = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
		declaration.DoMerge(shutterUpperer);
		Factory.Save();

		cusEntryHeader = declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault();
		cusEntryHeader.MovementReferenceNumberSetter("MRN1234567890", new ZDateTime(2021, 11, 18, 15, 00, 00));
		cusEntryHeader.CH_EntryReleaseDate = new ZDateTime(2021, 11, 18, 15, 00, 00);

		wrapper = new ReleaseShipmentDetailsWrapper(cusEntryHeader, cusEntryHeader.Declaration);
	}
	ReleaseShipmentDetailsWrapper wrapper;
	CusEntryHeader cusEntryHeader;
	CusEntryInstruction entryInstruction;
	JobDeclaration declaration;
	JobComInvoiceLine invoiceLine1;

	OrgHeader GetShipper()
	{
		var orgHeaderShipper = Factory.New<OrgHeader>();
		orgHeaderShipper.OH_FullName = "Rudy the Shipper";
		orgHeaderShipper.OH_Code = "RtS";

		var addressShipper = orgHeaderShipper.Addresses.AddNew();
		addressShipper.OA_Code = "EXP";
		addressShipper.Address1 = "Shipstreet 12";
		addressShipper.OA_City = "Brielle";
		addressShipper.OA_PostCode = "3047AL";
		addressShipper.OA_RL_NKRelatedPortCode = "NLRTM";
		addressShipper.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;

		return orgHeaderShipper;
	}

	OrgHeader GetImporter()
	{
		var orgHeaderImporter = Factory.New<OrgHeader>();
		orgHeaderImporter.OH_FullName = "Indy the Importer";
		orgHeaderImporter.OH_Code = "ItI";

		var addressImporter = orgHeaderImporter.Addresses.AddNew();
		addressImporter.OA_Code = "EXP";
		addressImporter.Address1 = "Impstreet 24";
		addressImporter.OA_City = "Utrecht";
		addressImporter.OA_PostCode = "3355CC";
		addressImporter.OA_RL_NKRelatedPortCode = "NLRTM";
		addressImporter.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;

		return orgHeaderImporter;
	}

	CusContainer GetContainer(ZString containerNumber)
	{
		var container = Factory.New<CusContainer>();
		container.CO_ContainerNumber = containerNumber;
		return container;
	}

	EU.Business.Declaration.Bill GetBill(ZString type, ZString billNumber)
	{
		var bill = Factory.New<EU.Business.Declaration.Bill>();
		bill.CU_BillType = type;
		bill.CU_BillNum = billNumber;
		return bill;
	}
}
