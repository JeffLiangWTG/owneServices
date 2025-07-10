using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class ReleaseDocumentWrapperTest : TestCaseWithFactory
{
	public void TestConstructor() => AssertExceptionThrown<ArgumentNullException>(() => new ReleaseDocumentWrapper(null));

	public void TestDeclarant() => AssertType<AddressInformationDocumentWrapper>(wrapper.Declarant);

	public void TestExporter() => AssertType<AddressInformationDocumentWrapper>(wrapper.Exporter);

	public void TestReleaseDate() => AssertEquals(new ZDateTime(2021, 11, 18, 15, 00, 00), wrapper.ReleaseDate);

	public void TestConsolID() => AssertEquals("CONSOLID", wrapper.ConsolID);

	public void TestShipmentDetails() => AssertType<ReleaseShipmentDetailsWrapper>(wrapper.ShipmentDetails);

	public void TestItemLines() => AssertType<ReleaseItemLineDetailsWrapper>(wrapper.ItemLineDetails.First());

	protected override void SetUp()
	{
		base.SetUp();

		var shipment = Factory.New<ForwardingShipment>();
		shipment.JS_BookingReference = "SHIPMENTID";
		var consol = shipment.Consols.AddNew();
		consol.JK_BookingReference = "CONSOLID";
		consol.JK_RL_NKDischargePort = "NLRTM";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_JS = shipment.PK;

		declaration.JE_OA_DeclarantAddress = GetDeclarant().MainAddress.PK;

		declaration.JE_OH_Exporter = GetExporter().PK;

		declaration.JE_TotalWeight = 50.91;
		declaration.JE_TotalWeightUnit = "KG";

		var invoice1 = declaration.Invoices.AddNew();
		invoice1.JZ_InvoiceNumber = "INV001";
		invoice1.JZ_InvoiceDate = new ZDateTime(2021, 11, 18);
		invoice1.JZ_InvoiceAmount = 1200.00;
		invoice1.JZ_RX_NKInvoice_Currency = "EUR";
		invoice1.JZ_InvoiceCurrExRate = 1;

		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		invoiceLine1.JI_Weight = 50.91;
		invoiceLine1.JI_WeightUQ = "KG";

		Factory.Save();
		var shutterUpperer = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
		declaration.DoMerge(shutterUpperer);
		Factory.Save();

		var cusEntryHeader = declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault();
		cusEntryHeader.MovementReferenceNumberSetter("MRN1234567890", new ZDateTime(2021, 11, 18, 15, 00, 00));
		cusEntryHeader.CH_EntryReleaseDate = new ZDateTime(2021, 11, 18, 15, 00, 00);

		wrapper = new ReleaseDocumentWrapper(cusEntryHeader);
	}
	ReleaseDocumentWrapper wrapper;

	OrgHeader GetDeclarant()
	{
		var orgHeaderDeclarant = Factory.New<OrgHeader>();
		orgHeaderDeclarant.OH_FullName = "Delta the Declarant";
		orgHeaderDeclarant.OH_Code = "DtD";

		var addressDeclarant = orgHeaderDeclarant.Addresses.AddNew();
		addressDeclarant.OA_Code = "EXP";
		addressDeclarant.Address1 = "Decstreet 12";
		addressDeclarant.OA_City = "Brussel";
		addressDeclarant.OA_PostCode = "2010AB";
		addressDeclarant.OA_RL_NKRelatedPortCode = "NLRTM";
		addressDeclarant.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;
		addressDeclarant.AddAddressType(OrgAddressType.CustomsAddressOfRecord);

		var cusCodeDeclarant = orgHeaderDeclarant.CustomsCodes.AddNew();
		cusCodeDeclarant.OK_RN_NKCodeCountry = "NL";
		cusCodeDeclarant.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
		cusCodeDeclarant.OK_CustomsRegNo = "987654321";
		return orgHeaderDeclarant;
	}

	OrgHeader GetExporter()
	{
		var orgHeaderExporter = Factory.New<OrgHeader>();
		orgHeaderExporter.OH_FullName = "Eddy the Exporter";
		orgHeaderExporter.OH_Code = "EtE";

		var addressExporter = orgHeaderExporter.Addresses.AddNew();
		addressExporter.OA_Code = "EXP";
		addressExporter.Address1 = "Expstreet 36";
		addressExporter.OA_City = "Arnhem";
		addressExporter.OA_PostCode = "3811EX";
		addressExporter.OA_RL_NKRelatedPortCode = "NLRTM";
		addressExporter.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;

		return orgHeaderExporter;
	}
}
