using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business.Testing;

static class DocumentWrapperTestHelper
{
	public static CusEntryHeader GetEntryHeaderForTest(BusinessObjectFactory factory)
	{
		CusEntryHeader cusEntryHeader;
		CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, true, false, "Customs duties on industrial products");
		CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, EU.Business.UniversalReferenceConstants.RefCusRateCodes.DefunctUseA00InsteadCustomsDutyOnAgriculturalProducts, true, false, "Customs duties on agricultural products");
		CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Vat, EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat, true, false, "BTW");
		factory.Save();

		var declaration = factory.New<JobDeclaration>();

		var orgHeaderSupplier = factory.New<OrgHeader>();
		orgHeaderSupplier.OH_FullName = "Sunday the Supplier";
		orgHeaderSupplier.OH_Code = "StS";

		var addressSupplier = orgHeaderSupplier.Addresses.AddNew();
		addressSupplier.OA_Code = "EXP";
		addressSupplier.Address1 = "Supstreet 12";
		addressSupplier.OA_City = "Den Haag";
		addressSupplier.OA_PostCode = "2244BB";
		addressSupplier.OA_RL_NKRelatedPortCode = "NLRTM";
		addressSupplier.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;

		declaration.JE_OH_Supplier = orgHeaderSupplier.PK;

		var orgHeaderControllingAgent = factory.New<OrgHeader>();
		orgHeaderControllingAgent.OH_FullName = "Chris the Controlling Agent";
		orgHeaderControllingAgent.OH_Code = "CtCA";

		var addressControllingAgent = orgHeaderControllingAgent.Addresses.AddNew();
		addressControllingAgent.OA_Code = "EXP";
		addressControllingAgent.Address1 = "CAstreet 12";
		addressControllingAgent.OA_City = "Rotterdam";
		addressControllingAgent.OA_PostCode = "1079CK";
		addressControllingAgent.OA_RL_NKRelatedPortCode = "NLRTM";
		addressControllingAgent.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;
		addressControllingAgent.AddAddressType(OrgAddressType.CustomsAddressOfRecord);

		var cusCodeControllingAgent = orgHeaderControllingAgent.CustomsCodes.AddNew();
		cusCodeControllingAgent.OK_RN_NKCodeCountry = "NL";
		cusCodeControllingAgent.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
		cusCodeControllingAgent.OK_CustomsRegNo = "123456789";

		declaration.JE_OH_ControllingAgent = orgHeaderControllingAgent.PK;

		var orgHeaderDeclarant = factory.New<OrgHeader>();
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

		declaration.JE_OA_DeclarantAddress = addressDeclarant.PK;

		var orgHeaderImporter = factory.New<OrgHeader>();
		orgHeaderImporter.OH_FullName = "Indy the Importer";
		orgHeaderImporter.OH_Code = "ItI";

		var addressImporter = orgHeaderImporter.Addresses.AddNew();
		addressImporter.OA_Code = "EXP";
		addressImporter.Address1 = "Impstreet 24";
		addressImporter.OA_City = "Utrecht";
		addressImporter.OA_PostCode = "3355CC";
		addressImporter.OA_RL_NKRelatedPortCode = "NLRTM";
		addressImporter.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;

		declaration.JE_OH_Importer = orgHeaderImporter.PK;

		declaration.JE_MessageType = "IMP";
		declaration.JE_EntryStyle = "IM";
		declaration.JE_TransportMode = "AIR";
		declaration.JE_RS_NKServiceLevel = "STD";
		declaration.JE_ApplicationCode = "BLT";

		declaration.JE_RL_NKPortOfLoading = "CNSHA";
		declaration.JE_RL_NKPortOfFirstArrival = "NLRTM";
		declaration.JE_RL_NKPortOfArrival = "NLRTM";

		declaration.JE_RL_NKOrigin = "CNSHA";
		declaration.JE_GoodsOrigin = "CN";
		declaration.JE_RL_NKFinalDestination = "NLRTM";
		declaration.JE_GoodsDestination = "NL";
		declaration.JE_UCR = "2-B00169514";

		declaration.GoodsLocation.CGL_Qualifier = Customs.Business.CusGoodsLocationQualifierList.Codes.PostcodeAddress;
		declaration.GoodsLocation.CGL_Type = Customs.Business.CusGoodsLocationTypeList.Codes.DesignatedLocation;
		declaration.GoodsLocation.CGL_AdditionalIdentifier = "25";
		declaration.GoodsLocation.Address.CompanyName = "Janssen BV";
		declaration.GoodsLocation.Address.Address1 = "Locationstreet 5";
		declaration.GoodsLocation.Address.Address2 = " Department of Goods";
		declaration.GoodsLocation.Address.City = "Amersfoort";
		declaration.GoodsLocation.Address.Postcode = "1062XD";
		declaration.GoodsLocation.Address.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;
		declaration.GoodsLocation.Address.E2_GovRegNum = "NL194563729B01";

		var invoice1 = declaration.Invoices.AddNew();
		invoice1.JZ_InvoiceNumber = "INV001";
		invoice1.JZ_InvoiceDate = new ZDateTime(2021, 11, 18);
		invoice1.JZ_OH_Supplier = orgHeaderSupplier.PK;
		invoice1.JZ_ValuationDateOverride = new ZDateTime(2021, 11, 18, 12, 00, 00);
		invoice1.JZ_InvoiceAmount = 1200.00;
		invoice1.JZ_RX_NKInvoice_Currency = "EUR";
		invoice1.JZ_InvoiceCurrExRate = 1;

		var invoice2 = declaration.Invoices.AddNew();
		invoice2.JZ_InvoiceNumber = "INV002";
		invoice2.JZ_InvoiceDate = new ZDateTime(2021, 11, 18);
		invoice2.JZ_OH_Supplier = orgHeaderSupplier.PK;
		invoice2.JZ_ValuationDateOverride = new ZDateTime(2021, 11, 18, 13, 00, 00);
		invoice2.JZ_InvoiceAmount = 17000.00;
		invoice2.JZ_RX_NKInvoice_Currency = "EUR";
		invoice2.JZ_InvoiceCurrExRate = 1;

		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		invoiceLine1.JI_Tariff = "00";
		invoiceLine1.JI_Description = "Smartwatch";
		invoiceLine1.JI_InvoiceQuantity = 200;
		invoiceLine1.JI_InvoiceUQ = "UNT";
		invoiceLine1.JI_CountryOfOrigin = "NL";
		invoiceLine1.JI_CustomsQuantity = 200;
		invoiceLine1.ZG_TransNature = "6";
		invoiceLine1.ZG_CountryOfDestination = "NL";
		invoiceLine1.JI_LinePrice = 2;

		var invoiceLine2 = invoice1.InvoiceLines.AddNew();
		invoiceLine2.JI_Tariff = "00";
		invoiceLine2.JI_Description = "Mobile phone";
		invoiceLine2.JI_InvoiceQuantity = 400;
		invoiceLine2.JI_InvoiceUQ = "UNT";
		invoiceLine2.JI_CountryOfOrigin = "NL";
		invoiceLine2.JI_CustomsQuantity = 400;
		invoiceLine2.ZG_TransNature = "6";
		invoiceLine2.ZG_CountryOfDestination = "NL";
		invoiceLine2.JI_LinePrice = 2;

		var invoiceLine3 = invoice2.InvoiceLines.AddNew();
		invoiceLine3.JI_Tariff = "00";
		invoiceLine3.JI_Description = "Lego Star Wars";
		invoiceLine3.JI_InvoiceQuantity = 850;
		invoiceLine3.JI_InvoiceUQ = "UNT";
		invoiceLine3.JI_CountryOfOrigin = "NL";
		invoiceLine3.JI_CustomsQuantity = 850;
		invoiceLine3.ZG_TransNature = "6";
		invoiceLine3.ZG_CountryOfDestination = "NL";
		invoiceLine3.JI_LinePrice = 20;

		factory.Save();
		var shutterUpperer = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
		declaration.DoMerge(shutterUpperer);
		factory.Save();

		cusEntryHeader = declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault();
		cusEntryHeader.MovementReferenceNumberSetter("MRN1234567890", new ZDateTime(2021, 11, 18, 15, 00, 00));
		cusEntryHeader.CH_EntryReleaseDate = new ZDateTime(2021, 11, 18, 15, 00, 00);

		var fee1 = cusEntryHeader.MergedLines.ElementAt(0).Fees.AddNew();
		fee1.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;
		fee1.CF_BaseValue = 1200m;
		fee1.CF_MethodOfCalculation = "%";
		fee1.CF_MethodOfPayment = "X";
		fee1.CF_Rate = 8m;
		fee1.CF_ChargeAmount = 96m;
		fee1.CF_RateOverrideReasonCode = "ADD";

		var fee2 = cusEntryHeader.MergedLines.ElementAt(0).Fees.AddNew();
		fee2.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
		fee2.CF_BaseValue = 1200m;
		fee2.CF_MethodOfCalculation = "%";
		fee2.CF_Rate = 21m;
		fee2.CF_ChargeAmount = 252m;
		fee2.CF_RateOverrideReasonCode = "ADD";
		fee1.CF_MethodOfPayment = "Z";

		var fee3 = cusEntryHeader.MergedLines.ElementAt(0).Fees.AddNew();
		fee3.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;
		fee3.CF_BaseValue = 1200m;
		fee3.CF_MethodOfCalculation = "CEN";
		fee3.CF_Rate = 2m;
		fee3.CF_ChargeAmount = 24m;
		fee3.CF_RateOverrideReasonCode = "ADD";
		fee1.CF_MethodOfPayment = "Z";

		var fee4 = cusEntryHeader.MergedLines.ElementAt(1).Fees.AddNew();
		fee4.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;
		fee4.CF_BaseValue = 800m;
		fee4.CF_MethodOfCalculation = "%";
		fee1.CF_MethodOfPayment = "X";
		fee4.CF_Rate = 6m;
		fee4.CF_ChargeAmount = 48m;
		fee4.CF_RateOverrideReasonCode = "ADD";

		var fee5 = cusEntryHeader.MergedLines.ElementAt(1).Fees.AddNew();
		fee5.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
		fee5.CF_BaseValue = 800m;
		fee5.CF_MethodOfCalculation = "%";
		fee1.CF_MethodOfPayment = "Z";
		fee5.CF_Rate = 21m;
		fee5.CF_ChargeAmount = 16800m;
		fee5.CF_RateOverrideReasonCode = "ADD";

		var fee6 = cusEntryHeader.MergedLines.ElementAt(2).Fees.AddNew();
		fee6.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;
		fee6.CF_BaseValue = 17000m;
		fee6.CF_MethodOfCalculation = "%";
		fee1.CF_MethodOfPayment = "X";
		fee6.CF_Rate = 6m;
		fee6.CF_ChargeAmount = 1020m;
		fee6.CF_RateOverrideReasonCode = "ADD";

		var fee7 = cusEntryHeader.MergedLines.ElementAt(2).Fees.AddNew();
		fee7.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
		fee7.CF_BaseValue = 17000m;
		fee7.CF_MethodOfCalculation = "%";
		fee1.CF_MethodOfPayment = "Z";
		fee7.CF_Rate = 21m;
		fee7.CF_ChargeAmount = 3570m;
		fee7.CF_RateOverrideReasonCode = "ADD";

		factory.Save();
		return cusEntryHeader;
	}
}
