using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(DocJobComInvoiceHeader))]
	sealed class DocJobComInvoiceHeaderTest : DocBaseJobComInvoiceHeaderAbstractTest<JobComInvoiceHeader, DocJobComInvoiceHeader>
	{
		#region Overrides

		public override void TestIncoTermDescription()
		{
			InvoiceHeaderInternal.JZ_IncoTerm = "EXW";
			Assert("Inco term should be empty", InvoiceHeaderWrapperInternal.IncoTermDescription != ZString.Empty);

			InvoiceHeaderInternal.JZ_IncoTerm = "CIF";
			Assert("Inco term should not be empty", InvoiceHeaderWrapperInternal.IncoTermDescription != ZString.Empty);
		}

		public override void TestConversionFactorIsWrapped()
		{
			InvoiceHeaderInternal.JZ_InvoiceAmount = 1000m;
			InvoiceHeaderInternal.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals(1m, InvoiceHeaderWrapperInternal.ConversionFactor);
		}

		public override void TestCIFCurrency()
		{
			AssertEquals("CIFCurrency", "CAD", InvoiceHeaderWrapperInternal.CIFCurrency.Code);
		}

		public override void TestFOBCurrency()
		{
			AssertEquals("FOBCurrency", "CAD", InvoiceHeaderWrapperInternal.FOBCurrency.Code);
		}

		public override void TestInvoiceCurr()
		{
			AssertEquals("Invoice_Currency", "CAD", InvoiceHeaderWrapperInternal.InvoiceCurr.Code);
			var newCurrency = (Factory.LoadTop1<RefCurrency>(new ZQuery())).RX_Code;
			InvoiceHeaderInternal.JZ_RX_NKInvoice_Currency = newCurrency;
			AssertEquals("Invoice_Currency", newCurrency, InvoiceHeaderWrapperInternal.InvoiceCurr.Code);
		}

		#endregion

		public void TestAccountingTotalAmounts()
		{
			var declaration = InvoiceHeaderInternal.JobDeclaration;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			//var invoice = declaration.Invoices.AddNew();
			InvoiceHeaderInternal.JZ_InvoiceNumber = "INVNO1";
			declaration.InvoiceLines.AddNew();
			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.TransactionNumber.SequentialNumber = "1";
			AssertEquals("12345000000012", declaration.DeclarationNumber);

			var testChgCode = Factory.NewWithValidTestData<AccChargeCode>();
			testChgCode.AC_ChargeType = "DSB";
			testChgCode.AC_Code = "TST";
			testChgCode.AC_GC = GlbCompany.CurrentCompany.PK;

			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(declaration.Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, testChgCode.PK.ToGuid());

			var testChgCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			testChgCode2.AC_ChargeType = "DSB";
			testChgCode2.AC_Code = "TST2";
			testChgCode2.AC_GC = GlbCompany.CurrentCompany.PK;

			var testChgCode3 = Factory.NewWithValidTestData<AccChargeCode>();
			testChgCode3.AC_ChargeType = "TST";
			testChgCode3.AC_Code = "TST3";
			testChgCode3.AC_GC = GlbCompany.CurrentCompany.PK;

			var chargeTypeSettings = new EntryChargeTypeSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
			EntryChargeTypeSetting chargeTypeSetting = chargeTypeSettings.AddNew();
			chargeTypeSetting.ChargeType = Enterprise.Customs.CA.Registry.EntryChargeTypeList.Codes.TotalGSTAmount;
			chargeTypeSetting.AC_ChargeCode = testChgCode2.PK;
			RatingDataRegistry.Instance.EntryChargeTypesAndCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, chargeTypeSettings);

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentID = declaration.PK;
			jobHeader.JH_ParentTableCode = "JE";
			jobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
			jobHeader.JH_GC = GlbCompany.CurrentCompany.PK;

			var charge1 = Factory.NewWithValidTestData<JobCharge>();
			charge1.JR_JH = jobHeader.PK;
			charge1.JR_AC = testChgCode.PK;
			charge1.JR_LocalSellAmt = 100m;
			charge1.JR_OSSellAmt = 100m;
			charge1.JR_Desc = "Customs Disbursement Charges-DUTY - 12345000000012 - INVNO1";

			var charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge2.JR_JH = jobHeader.PK;
			charge2.JR_AC = testChgCode2.PK;
			charge2.JR_LocalSellAmt = 218.75m;
			charge2.JR_OSSellAmt = 218.75m;
			charge2.JR_Desc = "Customs Disbursement Charges-GST - 12345000000012 - INVNO1";

			var charge3 = Factory.NewWithValidTestData<JobCharge>();
			charge3.JR_JH = jobHeader.PK;
			charge3.JR_AC = testChgCode3.PK;
			charge3.JR_LocalSellAmt = 73.13m;
			charge3.JR_OSSellAmt = 73.13m;
			charge3.JR_Desc = "Agency - 12345000000012 - INVNO1";

			var accTransHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			accTransHeader.AH_JH = jobHeader.PK;
			accTransHeader.AH_Ledger = "AR";
			accTransHeader.AH_TransactionCategory = "DBT";
			accTransHeader.AH_TransactionType = TransactionTypes.Invoice;
			accTransHeader.AH_InvoiceAmount = 461.88m;
			accTransHeader.AH_OutstandingAmount = 461.88m;
			accTransHeader.AH_GC = GlbCompany.CurrentCompany.PK;
			accTransHeader.AH_GB = GlbBranch.CurrentBranch.PK;

			var accTransLine1 = Factory.NewWithValidTestData<AccTransactionLines>();
			accTransLine1.AL_AH = accTransHeader.PK;
			accTransLine1.AL_LineAmount = 100m;
			accTransLine1.AL_OSAmount = 100m;
			accTransLine1.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			accTransLine1.AL_AC = testChgCode.PK;
			accTransLine1.AL_JH = jobHeader.PK;
			accTransLine1.AL_RevRecognitionType = "CUS";
			accTransLine1.AL_Desc = "Customs Disbursement Charges-DUTY - 12345000000012 - INVNO1";
			charge1.JR_AL_ARLine = accTransLine1.PK;

			var accTransLine2 = Factory.NewWithValidTestData<AccTransactionLines>();
			accTransLine2.AL_AH = accTransHeader.PK;
			accTransLine2.AL_LineAmount = 218.75m;
			accTransLine2.AL_OSAmount = 218.75m;
			accTransLine2.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			accTransLine2.AL_AC = testChgCode2.PK;
			accTransLine2.AL_JH = jobHeader.PK;
			accTransLine2.AL_RevRecognitionType = "CUS";
			accTransLine2.AL_Desc = "Customs Disbursement Charges-GST - 12345000000012 - INVNO1";
			charge2.JR_AL_ARLine = accTransLine2.PK;

			var accTransLine3 = Factory.NewWithValidTestData<AccTransactionLines>();
			accTransLine3.AL_AH = accTransHeader.PK;
			accTransLine3.AL_LineAmount = 73.13m;
			accTransLine3.AL_OSAmount = 73.13m;
			accTransLine3.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			accTransLine3.AL_AC = testChgCode3.PK;
			accTransLine3.AL_JH = jobHeader.PK;
			accTransLine3.AL_RevRecognitionType = "CUS";
			accTransLine3.AL_Desc = "Agency - 12345000000012 - INVNO1";
			charge3.JR_AL_ARLine = accTransLine3.PK;

			var charge4 = Factory.NewWithValidTestData<JobCharge>();
			charge4.JR_JH = jobHeader.PK;
			charge4.JR_AC = testChgCode.PK;
			charge4.JR_LocalSellAmt = 10m;
			charge4.JR_OSSellAmt = 10m;
			charge4.JR_Desc = "Customs Disbursement Charges-DUTY - 12345000000012 - INVNO2";

			var charge5 = Factory.NewWithValidTestData<JobCharge>();
			charge5.JR_JH = jobHeader.PK;
			charge5.JR_AC = testChgCode2.PK;
			charge5.JR_LocalSellAmt = 20m;
			charge5.JR_OSSellAmt = 20m;
			charge5.JR_Desc = "Customs Disbursement Charges-GST - 12345000000012 - INVNO2";

			var charge6 = Factory.NewWithValidTestData<JobCharge>();
			charge6.JR_JH = jobHeader.PK;
			charge6.JR_AC = testChgCode3.PK;
			charge6.JR_LocalSellAmt = 40m;
			charge6.JR_OSSellAmt = 40m;
			charge6.JR_Desc = "Agency - 12345000000012 - INVNO2";

			var accTransLine4 = Factory.NewWithValidTestData<AccTransactionLines>();
			accTransLine4.AL_AH = accTransHeader.PK;
			accTransLine4.AL_LineAmount = 10m;
			accTransLine4.AL_OSAmount = 10m;
			accTransLine4.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			accTransLine4.AL_AC = testChgCode.PK;
			accTransLine4.AL_JH = jobHeader.PK;
			accTransLine4.AL_RevRecognitionType = "CUS";
			accTransLine4.AL_Desc = "Customs Disbursement Charges-DUTY - 12345000000012 - INVNO2";
			charge4.JR_AL_ARLine = accTransLine4.PK;

			var accTransLine5 = Factory.NewWithValidTestData<AccTransactionLines>();
			accTransLine5.AL_AH = accTransHeader.PK;
			accTransLine5.AL_LineAmount = 20m;
			accTransLine5.AL_OSAmount = 20m;
			accTransLine5.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			accTransLine5.AL_AC = testChgCode2.PK;
			accTransLine5.AL_JH = jobHeader.PK;
			accTransLine5.AL_RevRecognitionType = "CUS";
			accTransLine5.AL_Desc = "Customs Disbursement Charges-GST - 12345000000012 - INVNO2";
			charge5.JR_AL_ARLine = accTransLine5.PK;

			var accTransLine6 = Factory.NewWithValidTestData<AccTransactionLines>();
			accTransLine6.AL_AH = accTransHeader.PK;
			accTransLine6.AL_LineAmount = 40m;
			accTransLine6.AL_OSAmount = 40m;
			accTransLine6.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			accTransLine6.AL_AC = testChgCode3.PK;
			accTransLine6.AL_JH = jobHeader.PK;
			accTransLine6.AL_RevRecognitionType = "CUS";
			accTransLine6.AL_Desc = "Agency - 12345000000012 - INVNO2";
			charge6.JR_AL_ARLine = accTransLine6.PK;
			Factory.Save();

			AssertEquals("Total Billed Amount", 318.75m, InvoiceHeaderWrapperInternal.LVSTotalBilledAmount);
			AssertEquals("Total Invoiced Amount", 391.88m, InvoiceHeaderWrapperInternal.LVSTotalInvoicedAmount);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			AssertEquals("Total Billed Amount", 348.75m, InvoiceHeaderWrapperInternal.LVSTotalBilledAmount);
			AssertEquals("Total Invoiced Amount", 461.88m, InvoiceHeaderWrapperInternal.LVSTotalInvoicedAmount);
		}

		public void TestNumberOfPacksFormatted()
		{
			InvoiceHeaderInternal.JobDeclaration.JE_TotalNoOfPacksPackType = "PKG";
			InvoiceHeaderInternal.JZ_NoOfPacks = 0m;
			AssertEquals("Zero packs", ZString.Empty, InvoiceHeaderWrapperInternal.NumberOfPacksFormatted);
			InvoiceHeaderInternal.JZ_NoOfPacks = 1234.567m;
			AssertEquals("Packs", "1,234.567 PKG", InvoiceHeaderWrapperInternal.NumberOfPacksFormatted);
		}

		public void TestInvoiceCount()
		{
			AssertEquals("InvoiceCount should be always 1 to calculate count of invoices for groups", 1, InvoiceHeaderWrapperInternal.InvoiceCount);
		}

		public void TestCommercialInvoiceOriginatorAddress()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.MainAddress.OA_Address1 = "Test Adress 1";
			InvoiceHeaderInternal.JobDeclaration.CommercialInvoiceOriginator.OrganisationPK = org1.PK;
			AssertEquals(InvoiceHeaderWrapperInternal.Declaration.CommercialInvoiceOriginatorAddress, InvoiceHeaderWrapperInternal.CommercialInvoiceOriginatorAddress);
			AssertEquals("Get address from declaration when invoice header has no originator", new AddressFormatter(Factory, org1, GlbCompany.CurrentCompany, false).PostalAddress(), InvoiceHeaderWrapperInternal.CommercialInvoiceOriginatorAddress);

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.MainAddress.OA_Address1 = "Test Adress 2";
			InvoiceHeaderInternal.CommercialInvoiceOriginator.OrganisationPK = org2.PK;
			AssertEquals("Get address from invoice header when invoice header has an originator", new AddressFormatter(Factory, org2, GlbCompany.CurrentCompany, false).PostalAddress(), InvoiceHeaderWrapperInternal.CommercialInvoiceOriginatorAddress);
		}

		public void TestEffectiveValuationDate()
		{
			var effectiveValuationDate = new ZDateTime(2004, 04, 05);
			InvoiceHeaderInternal.JZ_ValuationDateOverride = effectiveValuationDate;
			AssertEquals("EffectiveValuationDate", effectiveValuationDate, InvoiceHeaderWrapperInternal.EffectiveValuationDate);
		}

		public void TestOrganisations()
		{
			var helper = new DeclarationTestHelper(Factory, true);
			var canada = Factory.Load<RefCountry>(Core.Constants.CountryGuids.Canada);
			var supplier1 = helper.CreateOrganisation("Supplier 1", "CATOR");
			supplier1.MainAddress.Address1 = "Main Address1";
			var supplier2 = helper.CreateOrganisation("Supplier 2", "CATOR");
			var supplier2Address = supplier2.Addresses.AddNew();
			supplier2Address.Address1 = "Address 1";
			var importer = helper.CreateOrganisation("IMPORTER NAME", "CATOR");
			var buyer = helper.CreateOrganisation("PURCHASER NAME", "CATOR");
			var buyerAddress = buyer.Addresses.AddNew();
			buyerAddress.Address1 = "PURCHASER ADDRESS 1";
			var consignee = helper.CreateOrganisation("CONSIGNEE NAME", "CABLO");
			var consigneeAddress = consignee.Addresses.AddNew();
			consigneeAddress.Address1 = "CONSIGNEE ADDRESS 1";
			var exporter = helper.CreateOrganisation("EXPORTER NAME", "NZAKL");
			var exporterAddress = exporter.Addresses.AddNew();
			exporterAddress.Address1 = "EXPORTER ADDRESS 1";
			var manufacturer = helper.CreateOrganisation("MANUFACTURER NAME", "CATOR");
			var manufactureAddress = manufacturer.Addresses.AddNew();
			manufactureAddress.Address1 = "MANUFACTURER ADDRESS 1";
			var importerOfRecord = helper.CreateOrganisation("IMPORTER OF RECORD", "CATOR");
			importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberImporterCommercial, "1234", canada);
			consignee.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "3021", canada);
			buyer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "4321", canada);
			importerOfRecord.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberImporterCommercial, "4567", canada);

			InvoiceHeaderInternal.JobDeclaration.JE_OH_Supplier = supplier1.PK;
			InvoiceHeaderInternal.JobDeclaration.JE_OH_Importer = importer.PK;
			InvoiceHeaderInternal.ExporterDocumentaryAddress.OrganisationPK = exporter.PK;
			InvoiceHeaderInternal.JZ_OH_Supplier = ZGuid.Empty;

			AssertEquals("Vendor from declaration", supplier1.OH_FullName, InvoiceHeaderWrapperInternal.Vendor.Name);
			AssertEquals("Vendor from declaration", "SUPPLIER 1\nMAIN ADDRESS1\nCITY ON", InvoiceHeaderWrapperInternal.Vendor.SelectedAddress.PostalAddressExcludeCountryIfSame);
			AssertNull("Supplier from declaration", InvoiceHeaderWrapperInternal.Supplier);
			AssertEquals("Exporter", exporter.OH_FullName, InvoiceHeaderWrapperInternal.Exporter.Name);
			AssertEquals("Purchaser from declaration", importer.OH_FullName, InvoiceHeaderWrapperInternal.Buyer.Name);
			AssertEquals("ImporterBusinessNumber", "1234", InvoiceHeaderWrapperInternal.ImporterBusinessNumber);
			AssertEquals("Consignee from declaration", importer.OH_FullName, InvoiceHeaderWrapperInternal.Consignee.Name);
			AssertEquals("ConsigneeBusinessNumber", "1234", InvoiceHeaderWrapperInternal.ConsigneeBusinessNumber);

			InvoiceHeaderInternal.JobDeclaration.ImporterOfRecordAddress.OrganisationPK = importerOfRecord.PK;
			AssertEquals("Consignee from declaration", importerOfRecord.OH_FullName, InvoiceHeaderWrapperInternal.Consignee.Name);
			AssertEquals("ConsigneeBusinessNumber", "4567", InvoiceHeaderWrapperInternal.ConsigneeBusinessNumber);

			InvoiceHeaderInternal.JZ_OH_Supplier = supplier2.PK;
			InvoiceHeaderInternal.SupplierDocumentaryAddress.E2_OA_Address = supplier2Address.PK;
			InvoiceHeaderInternal.JZ_OH_Buyer = buyer.PK;
			InvoiceHeaderInternal.FinalConsigneeAddress.E2_OA_Address = consigneeAddress.PK;
			InvoiceHeaderInternal.BuyerDocumentaryAddress.E2_OA_Address = buyerAddress.PK;
			InvoiceHeaderInternal.ExporterDocumentaryAddress.E2_OA_Address = exporterAddress.PK;
			InvoiceHeaderInternal.JZ_OA_ManufacturerAddress = manufactureAddress.PK;

			AssertEquals("Vendor from invoice header", supplier2.OH_FullName, InvoiceHeaderWrapperInternal.Vendor.Name);
			AssertEquals("Vendor from declaration", "SUPPLIER 2\nADDRESS 1", InvoiceHeaderWrapperInternal.Vendor.SelectedAddress.PostalAddressExcludeCountryIfSame);
			AssertEquals("Supplier from invoice header", supplier2.OH_FullName, InvoiceHeaderWrapperInternal.Supplier.Name);
			AssertEquals("Supplier from declaration", "SUPPLIER 2\nADDRESS 1", InvoiceHeaderWrapperInternal.Supplier.SelectedAddress.PostalAddressExcludeCountryIfSame);
			AssertEquals("Purchaser", buyer.OH_FullName, InvoiceHeaderWrapperInternal.Buyer.Name);
			AssertEquals("ImporterBusinessNumber", "4321", InvoiceHeaderWrapperInternal.ImporterBusinessNumber);
			AssertEquals("Consignee", consignee.OH_FullName, InvoiceHeaderWrapperInternal.Consignee.Name);
			AssertEquals("Consignee Address", "CONSIGNEE NAME\nCONSIGNEE ADDRESS 1\nCANADA", InvoiceHeaderWrapperInternal.Consignee.SelectedAddress.PostalAddress);
			AssertEquals("ConsigneeBusinessNumber", "3021", InvoiceHeaderWrapperInternal.ConsigneeBusinessNumber);
			AssertEquals("Purchaser Address", "PURCHASER NAME\nPURCHASER ADDRESS 1\nCANADA", InvoiceHeaderWrapperInternal.Buyer.SelectedAddress.PostalAddress);
			AssertEquals("Exporter Address", "EXPORTER NAME\nEXPORTER ADDRESS 1\nCANADA", InvoiceHeaderWrapperInternal.Exporter.SelectedAddress.PostalAddress);
		}

		public void TestMeasurement()
		{
			InvoiceHeaderInternal.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceHeaderInternal.JZ_Weight = 2000;
			InvoiceHeaderInternal.JZ_WeightUQ = Core.Constants.Weight.Grams;
			InvoiceHeaderInternal.JZ_NetWeight = 3000;
			InvoiceHeaderInternal.JZ_NetWeightUQ = Core.Constants.Weight.Grams;

			AssertEquals("GrossWeightInKilograms", 2m, InvoiceHeaderWrapperInternal.GrossWeightInKilograms);
			AssertEquals("NetWeightInKilograms", 3m, InvoiceHeaderWrapperInternal.NetWeightInKilograms);

			InvoiceHeaderInternal.CA_TimeLimit = 2;
			InvoiceHeaderInternal.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Month;
			AssertEquals("TimeLimit", InvoiceHeaderInternal.CA_TimeLimit, InvoiceHeaderWrapperInternal.TimeLimit);
			AssertEquals("TimeLimitUnit", InvoiceHeaderInternal.CA_TimeLimitCode, InvoiceHeaderWrapperInternal.TimeLimitUnit);
		}

		public void TestAmountsAndCharges()
		{
			var usd = Enterprise.MasterFiles.Business.RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates);
			usd.SetCustomsRate(new ZDateTime(2000, 1, 1), ZDateTime.MaxSmallDateTimeValue, 0.719424m);

			var helper = new DeclarationTestHelper(Factory, true);
			InvoiceHeaderInternal.JZ_RX_NKInvoice_Currency = helper.CAD.RX_Code;

			InvoiceHeaderInternal.Charges.AddNew(CAChargeTypeList.Codes.OverseasFreight, 100m);
			InvoiceHeaderInternal.Charges.AddNew(CAChargeTypeList.Codes.OverseasInsurance, 50m);
			InvoiceHeaderInternal.Charges.AddNew(CAChargeTypeList.Codes.Construction, 200m);
			InvoiceHeaderInternal.Charges.AddNew(CAChargeTypeList.Codes.PackingCost, 300.60m);

			var charge = InvoiceHeaderInternal.GroupCharges.AddNew(CAChargeTypeList.Codes.OverseasFreight, 200m);
			charge.J7_Calc_IsIncludedInInvoiceAmount = false;
			charge.J7_RX_NKCurrency = helper.CAD.RX_Code;
			charge = InvoiceHeaderInternal.GroupCharges.AddNew(CAChargeTypeList.Codes.OverseasInsurance, 100.6789m);
			charge.J7_Calc_IsIncludedInInvoiceAmount = false;
			charge.J7_RX_NKCurrency = helper.CAD.RX_Code;
			charge = InvoiceHeaderInternal.GroupCharges.AddNew(CAChargeTypeList.Codes.PackingCost, 300m);
			charge.J7_Calc_IsIncludedInInvoiceAmount = false;
			charge.J7_RX_NKCurrency = helper.USD.RX_Code;
			charge = InvoiceHeaderInternal.GroupCharges.AddNew(CAChargeTypeList.Codes.Commission, 150m);
			charge.J7_Calc_IsIncludedInInvoiceAmount = false;
			charge.J7_RX_NKCurrency = helper.CAD.RX_Code;

			AssertEquals("IncludedTransportChargesAndInsuranceInCAD", 150m, InvoiceHeaderWrapperInternal.IncludedTransportChargesAndInsuranceInCAD);
			AssertEquals("IncludedConstructionCostsInCAD", 200m, InvoiceHeaderWrapperInternal.IncludedConstructionCostsInCAD);
			AssertEquals("IncludedExportPackingCostsInCAD", 300.60m, InvoiceHeaderWrapperInternal.IncludedExportPackingCostsInCAD);
			AssertEquals("ExcludedTransportChargesAndInsuranceInCAD", 300.68m, InvoiceHeaderWrapperInternal.ExcludedTransportChargesAndInsuranceInCAD);
			AssertEquals("ExcludedCommissionInCAD", 150m, InvoiceHeaderWrapperInternal.ExcludedCommissionInCAD);
			AssertEquals("ExcludedExportPackingCostsInCAD", 215.83m, InvoiceHeaderWrapperInternal.ExcludedExportPackingCostsInCAD);
			AssertEquals("IsAdjustmentsToPricePaidOrPayableApplicable", true, InvoiceHeaderWrapperInternal.IsAdjustmentsToPricePaidOrPayableApplicable);
		}

		public void TestCountriesAndPlaces()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var officeCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0804", "Nanaimo", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "BC");
			Factory.Save();

			InvoiceHeaderInternal.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.UnitedStates;
			InvoiceHeaderInternal.JZ_RW_NKOriginState = USStatesList.Codes.NewYork;
			AssertEquals("CountryAndStateOfOrigin is US", "UNY", InvoiceHeaderWrapperInternal.CountryAndStateOfOrigin);
			InvoiceHeaderInternal.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.Zimbabwe;
			AssertEquals("CountryAndStateOfOrigin", InvoiceHeaderInternal.JZ_RN_NKDefaultOrigin, InvoiceHeaderWrapperInternal.CountryAndStateOfOrigin);
			var line = (JobComInvoiceLine)InvoiceHeaderInternal.InvoiceLines.AddNew();
			line.JI_CountryOfOrigin = Core.Constants.CountryCodes.Zimbabwe;
			AssertEquals("CountryAndStateOfOrigin is blank", "See Below", InvoiceHeaderWrapperInternal.CountryAndStateOfOrigin);

			InvoiceHeaderInternal.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedStates;
			InvoiceHeaderInternal.CA_USStateOfExport = USStatesList.Codes.NewYork;
			AssertEquals("CountryAndStateOfExport is US", "UNY", InvoiceHeaderWrapperInternal.CountryAndStateOfExport);
			InvoiceHeaderInternal.CA_RN_NKExport = Core.Constants.CountryCodes.Zimbabwe;
			AssertEquals("CountryAndStateOfExport", InvoiceHeaderInternal.CA_RN_NKExport, InvoiceHeaderWrapperInternal.CountryAndStateOfExport);
			line.CA_RN_NKExport = Core.Constants.CountryCodes.Zimbabwe;
			AssertEquals("CountryAndStateOfExport is blank", "See Below", InvoiceHeaderWrapperInternal.CountryAndStateOfExport);

			InvoiceHeaderInternal.CA_RN_NKTranshipment = "SG";
			InvoiceHeaderInternal.CA_RL_NKLastPort = "AUBNE";
			AssertEquals("PlaceOfDirectShipment", InvoiceHeaderInternal.LastPort.RL_PortName, InvoiceHeaderWrapperInternal.PlaceOfDirectShipment.PortName);
			AssertEquals("TranshipmentCountry", InvoiceHeaderInternal.CA_RN_NKTranshipment, InvoiceHeaderWrapperInternal.TranshipmentCountry.Code);

			InvoiceHeaderInternal.JobDeclaration.JE_CustomsOffice = "804";
			AssertEquals("PortOfClearance", "0804", InvoiceHeaderWrapperInternal.PortOfClearance);
			AssertEquals("ProvinceOfClearance", "BC", InvoiceHeaderWrapperInternal.ProvinceOfClearance);
			AssertEquals("PortOfClearanceCodeDescription", "0804 - Nanaimo", InvoiceHeaderWrapperInternal.PortOfClearanceCodeDescription);
			InvoiceHeaderInternal.CA_PortOfClearance = "123";
			AssertEquals("PortOfClearance", "0123", InvoiceHeaderWrapperInternal.PortOfClearance);
			AssertEquals("PortOfClearanceCodeDescription", "0123", InvoiceHeaderWrapperInternal.PortOfClearanceCodeDescription);
		}

		public void TestReferencesAndOtherTextFields()
		{
			InvoiceHeaderInternal.CA_OtherReference = "OTHER REFERENCE";
			InvoiceHeaderInternal.CA_DepartmentRuling = "DEPARTMENTAL RULINGS";
			InvoiceHeaderInternal.CA_ConditionsOfSale = "CONDITIONS OF SALE";
			InvoiceHeaderInternal.CA_TermsOfPayment = "TERMS OF PAYMENT";
			InvoiceHeaderInternal.CA_ValueForDutyCode = ValueForDutyCodes.Codes.UnrelatedFirmsPaidPayableWithoutAdjustments;

			AssertEquals("OtherReference", InvoiceHeaderInternal.CA_OtherReference, InvoiceHeaderWrapperInternal.OtherReference);
			AssertEquals("DepartmentRuling", InvoiceHeaderInternal.CA_DepartmentRuling, InvoiceHeaderWrapperInternal.DepartmentRuling);
			AssertEquals("ConditionsOfSale", InvoiceHeaderInternal.CA_ConditionsOfSale, InvoiceHeaderWrapperInternal.ConditionsOfSale);
			AssertEquals("TermsOfPayment", InvoiceHeaderInternal.CA_TermsOfPayment, InvoiceHeaderWrapperInternal.TermsOfPayment);
			AssertEquals("ValueForDutyCode", InvoiceHeaderInternal.CA_ValueForDutyCode, InvoiceHeaderWrapperInternal.ValueForDutyCode);

			InvoiceHeaderInternal.CA_TreatmentCode = TariffTreatmentCodes.Codes.UnitedStates;
			AssertEquals("TreatmentCode", InvoiceHeaderInternal.CA_TreatmentCode, InvoiceHeaderWrapperInternal.TreatmentCode);
			var line = (JobComInvoiceLine)InvoiceHeaderInternal.InvoiceLines.AddNew();
			line.CA_TreatmentCode = TariffTreatmentCodes.Codes.Mexico;
			AssertEquals("TreatmentCode", "See Below", InvoiceHeaderWrapperInternal.TreatmentCode);
		}

		public void TestIndicators()
		{
			AssertEquals("IsAdjustmentsToPricePaidOrPayableApplicable", false, InvoiceHeaderWrapperInternal.IsAdjustmentsToPricePaidOrPayableApplicable);

			InvoiceHeaderInternal.CA_ServicesInd = true;
			InvoiceHeaderInternal.CA_RoyaltyInd = true;

			AssertEquals("ServicesInd", InvoiceHeaderInternal.CA_ServicesInd, InvoiceHeaderWrapperInternal.ServicesInd);
			AssertEquals("RoyaltyInd", InvoiceHeaderInternal.CA_RoyaltyInd, InvoiceHeaderWrapperInternal.RoyaltyInd);
			AssertEquals("IsAdjustmentsToPricePaidOrPayableApplicable", true, InvoiceHeaderWrapperInternal.IsAdjustmentsToPricePaidOrPayableApplicable);
		}

		#region Implementation

		protected override string TestingCountry
		{
			get { return Core.Constants.CountryCodes.Canada; }
		}

		protected override DocJobComInvoiceHeader CreateInvoiceHeaderWrapper(JobComInvoiceHeader invoiceHeaderInternal)
		{
			return DocJobComInvoiceHeader.New(invoiceHeaderInternal, Factory);
		}

		#endregion
	}
}
