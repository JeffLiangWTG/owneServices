using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;
using DocumentNote = Enterprise.DocumentEngine.DocumentNote;
using SDFields = Enterprise.DocumentWrappers.Customs.Base.DocBaseJobDeclaration.SDFields;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(CommercialInvoiceWrapper))]
	sealed class CommercialInvoiceWrapperTest : Base.Testing.GenericWrapperTest
	{
		public void TestJobNumber()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			CommercialInvoiceWrapper invoice = new CommercialInvoiceWrapper(invoiceHeader, Factory);
			declaration.JE_DeclarationReference = "12345";
			AssertEquals("12345", invoice.JobNumber);

			invoice = new CommercialInvoiceWrapper(invoiceHeader, Factory);
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "98765";
			declaration.JE_JS = shipment.PK;
			AssertEquals("98765", invoice.JobNumber);
		}

		public void TestIncoTerm()
		{
			CustomsIncoTermOverrideCollection collection = DocumentsDataRegistry.Instance.CustomsIncoTermsOverride.Value;
			collection.AddNew("FDD", Core.Constants.IncoTerms.FreeAlongsideShip);
			DocumentsDataRegistry.Instance.CustomsIncoTermsOverride.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			CommercialInvoiceWrapper invoiceWrapper = new CommercialInvoiceWrapper(null, Factory);
			AssertEquals(ZString.Empty, invoiceWrapper.IncoTerm.Code);

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_IncoTerm = ZString.Empty;
			invoiceWrapper = new CommercialInvoiceWrapper(invoiceHeader, Factory);
			AssertEquals(ZString.Empty, invoiceWrapper.IncoTerm.Code);

			invoiceHeader.JZ_IncoTerm = "III";
			AssertEquals("III", invoiceWrapper.IncoTerm.Code);

			invoiceHeader.JZ_IncoTerm = "FDD";
			AssertEquals(Core.Constants.IncoTerms.FreeAlongsideShip, invoiceWrapper.IncoTerm.Code);
		}

		public void TestSupplierAndImporterDropBackToDeclarationIfTheSame()
		{
			OrgHeader org1 = GetNewTestOrg("ORG1");
			OrgHeader org2 = GetNewTestOrg("ORG2");
			OrgHeader org3 = GetNewTestOrg("ORG3");
			OrgHeader org4 = GetNewTestOrg("ORG4");

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_OH_Importer = org1.PK;
			declaration.JE_OH_Supplier = org2.PK;
			BaseJobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_OH_Buyer = ZGuid.Empty;
			invoiceHeader.JZ_OH_Supplier = ZGuid.Empty;

			CommercialInvoiceWrapper invoiceWrapper = new CommercialInvoiceWrapper(invoiceHeader, Factory);
			AssertEquals("invoiceWrapper.Importer.CompanyName", "ORG1", invoiceWrapper.Importer.CompanyName);
			AssertEquals("invoiceWrapper.Importer.TypeDescription", "Importer", invoiceWrapper.Importer.TypeDescription);
			AssertEquals("invoiceWrapper.Supplier.CompanyName", "ORG2", invoiceWrapper.Supplier.CompanyName);
			AssertEquals("invoiceWrapper.Supplier.TypeDescription", "Supplier", invoiceWrapper.Supplier.TypeDescription);

			invoiceHeader.JZ_OH_Buyer = org1.PK;
			invoiceHeader.JZ_OH_Supplier = org2.PK;

			invoiceWrapper = new CommercialInvoiceWrapper(invoiceHeader, Factory);
			AssertEquals("invoiceWrapper.Importer.CompanyName", "ORG1", invoiceWrapper.Importer.CompanyName);
			AssertEquals("invoiceWrapper.Importer.TypeDescription", "Importer", invoiceWrapper.Importer.TypeDescription);
			AssertEquals("invoiceWrapper.Supplier.CompanyName", "ORG2", invoiceWrapper.Supplier.CompanyName);
			AssertEquals("invoiceWrapper.Supplier.TypeDescription", "Supplier", invoiceWrapper.Supplier.TypeDescription);

			declaration.ImporterDocumentaryAddress.E2_AddressOverride = true;
			declaration.ImporterDocumentaryAddress.E2_CompanyName = "ORANGUTAN 1";

			declaration.SupplierDocumentaryAddress.E2_AddressOverride = true;
			declaration.SupplierDocumentaryAddress.E2_CompanyName = "ORANGUTAN 2";

			invoiceWrapper = new CommercialInvoiceWrapper(invoiceHeader, Factory);
			AssertEquals("invoiceWrapper.Importer.CompanyName", "ORG1", invoiceWrapper.Importer.CompanyName);
			AssertEquals("invoiceWrapper.Importer.TypeDescription", "Importer", invoiceWrapper.Importer.TypeDescription);
			AssertEquals("invoiceWrapper.Supplier.CompanyName", "ORG2", invoiceWrapper.Supplier.CompanyName);
			AssertEquals("invoiceWrapper.Supplier.TypeDescription", "Supplier", invoiceWrapper.Supplier.TypeDescription);

			invoiceHeader.JZ_OH_Buyer = org3.PK;
			invoiceHeader.JZ_OH_Supplier = org4.PK;

			invoiceWrapper = new CommercialInvoiceWrapper(invoiceHeader, Factory);
			AssertEquals("invoiceWrapper.Importer.CompanyName", "ORG3", invoiceWrapper.Importer.CompanyName);
			AssertEquals("invoiceWrapper.Importer.TypeDescription", "Importer", invoiceWrapper.Importer.TypeDescription);
			AssertEquals("invoiceWrapper.Supplier.CompanyName", "ORG4", invoiceWrapper.Supplier.CompanyName);
			AssertEquals("invoiceWrapper.Supplier.TypeDescription", "Supplier", invoiceWrapper.Supplier.TypeDescription);

			org3.MainAddress.OA_Address1 = "ORG3 MAIN ADDRESS";
			org3.MainAddress.OA_RN_NKCountryCode = "US";
			var org3SelectedAddress = org3.Addresses.AddNew();
			org3SelectedAddress.OA_Address1 = "ORG3 SELECTED ADDRESS";
			org3SelectedAddress.OA_RN_NKCountryCode = "US";
			invoiceHeader.JZ_OA_ConsigneeAddress = org3SelectedAddress.PK;

			org4.MainAddress.OA_Address1 = "ORG4 MAIN ADDRESS";
			var org4SelectedAddress = org4.Addresses.AddNew();
			org4SelectedAddress.OA_Address1 = "ORG4 SELECTED ADDRESS";
			org4SelectedAddress.OA_RN_NKCountryCode = "US";
			invoiceHeader.JZ_OA_SupplierAddress = org4SelectedAddress.PK;

			invoiceWrapper = new CommercialInvoiceWrapper(invoiceHeader, Factory);
			AssertEquals("invoiceWrapper.Importer", "ORG3 SELECTED ADDRESS\nUNITED STATES", invoiceWrapper.Importer.CompanyAddress);
			AssertEquals("invoiceWrapper.Supplier", "ORG4 SELECTED ADDRESS\nUNITED STATES", invoiceWrapper.Supplier.CompanyAddress);
		}

		public void TestSDFBasedFieldsOnDeclaration()
		{
			CommercialInvoiceWrapper invoiceWrapper = new CommercialInvoiceWrapper(null, Factory);
			AssertEquals(ZString.Empty, invoiceWrapper.AdditionalInformation);
			AssertEquals(ZString.Empty, invoiceWrapper.AdditionalPaymentTerms);
			AssertEquals(ZString.Empty, invoiceWrapper.ExportersBankAccountNo);
			AssertEquals(ZString.Empty, invoiceWrapper.ExportersBankName);
			AssertEquals(ZString.Empty, invoiceWrapper.ExportersBankSWIFTCode);
			AssertEquals(ZString.Empty, invoiceWrapper.InsurancePolicyNumber);
			AssertEquals(ZString.Empty, invoiceWrapper.InsuredValue);
			AssertEquals(ZString.Empty, invoiceWrapper.LetterOfCreditNumber);
			AssertEquals(ZString.Empty, invoiceWrapper.LetterOfCreditDate);

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceWrapper = new CommercialInvoiceWrapper(invoiceHeader, Factory);
			AssertEquals(ZString.Empty, invoiceWrapper.AdditionalInformation);
			AssertEquals(ZString.Empty, invoiceWrapper.AdditionalPaymentTerms);
			AssertEquals(ZString.Empty, invoiceWrapper.ExportersBankAccountNo);
			AssertEquals(ZString.Empty, invoiceWrapper.ExportersBankName);
			AssertEquals(ZString.Empty, invoiceWrapper.ExportersBankSWIFTCode);
			AssertEquals(ZString.Empty, invoiceWrapper.InsurancePolicyNumber);
			AssertEquals(ZString.Empty, invoiceWrapper.InsuredValue);
			AssertEquals(ZString.Empty, invoiceWrapper.LetterOfCreditNumber);
			AssertEquals(ZString.Empty, invoiceWrapper.LetterOfCreditDate);

			invoiceHeader.JZ_InvoiceAmount = 123.45m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "NZD";
			invoiceHeader.JZ_LetterOfCreditNumber = "111111";
			invoiceHeader.JZ_LetterOfCreditDate = ZDate.BrettsBirthday;
			invoiceHeader.JZ_ExporterBankName = "E.SUN BANK, ESB";
			invoiceHeader.JZ_ExporterBankAccountNumber = "0000806449";
			invoiceHeader.JZ_ExporterBankSWIFTCode = "CITIUS33";
			invoiceHeader.JZ_Remarks = "Test AdditionalInformation";
			AssertEquals("123.45 NZD", invoiceWrapper.InsuredValue);
			AssertEquals("111111", invoiceWrapper.LetterOfCreditNumber);
			AssertEquals("18-Sep-71", invoiceWrapper.LetterOfCreditDate);
			AssertEquals("E.SUN BANK, ESB", invoiceWrapper.ExportersBankName);
			AssertEquals("0000806449", invoiceWrapper.ExportersBankAccountNo);
			AssertEquals("CITIUS33", invoiceWrapper.ExportersBankSWIFTCode);
			AssertEquals("Test AdditionalInformation", invoiceWrapper.AdditionalInformation);

			DocumentNote note = DocumentNote.LoadNote(declaration);
			note.SetSystemDefinedFieldValue(SDFields.AdditionalPaymentTerms, "Test AdditionalPaymentTerms");
			note.SetSystemDefinedFieldValue(SDFields.InsurancePolicyNumber, "Test InsurancePolicyNumber");
			note.SetSystemDefinedFieldValue(SDFields.InsuredValue, "Test InsuredValue");
			note.SetSystemDefinedFieldValue(SDFields.LetterOfCreditNumber, "Test LetterOfCreditNumber");
			note.SetSystemDefinedFieldValue(SDFields.LetterOfCreditDate, "Test LetterOfCreditDate");

			invoiceWrapper = new CommercialInvoiceWrapper(invoiceHeader, Factory);
			AssertEquals("Test AdditionalPaymentTerms", invoiceWrapper.AdditionalPaymentTerms);
			AssertEquals("Test InsurancePolicyNumber", invoiceWrapper.InsurancePolicyNumber);
			AssertEquals("Test InsuredValue", invoiceWrapper.InsuredValue);
			AssertEquals("Test LetterOfCreditNumber", invoiceWrapper.LetterOfCreditNumber);
			AssertEquals("Test LetterOfCreditDate", invoiceWrapper.LetterOfCreditDate);
		}

		public void TestSDFBasedFieldsOnShipment()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			BaseJobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			CommercialInvoiceWrapper invoiceWrapper = new CommercialInvoiceWrapper(invoiceHeader, Factory);
			AssertEquals(ZString.Empty, invoiceWrapper.LetterOfCreditNumber);
			AssertEquals(ZString.Empty, invoiceWrapper.LetterOfCreditDate);
			AssertEquals(ZString.Empty, invoiceWrapper.ExportersBankName);
			AssertEquals(ZString.Empty, invoiceWrapper.ExportersBankAccountNo);
			AssertEquals(ZString.Empty, invoiceWrapper.ExportersBankSWIFTCode);

			invoiceHeader.JZ_InvoiceAmount = 123.45m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "NZD";
			invoiceHeader.JZ_LetterOfCreditNumber = "111111";
			invoiceHeader.JZ_LetterOfCreditDate = ZDate.BrettsBirthday;
			invoiceHeader.JZ_ExporterBankName = "E.SUN BANK, ESB";
			invoiceHeader.JZ_ExporterBankAccountNumber = "0000806449";
			invoiceHeader.JZ_ExporterBankSWIFTCode = "CITIUS33";
			invoiceHeader.JZ_Remarks = "Test AdditionalInformation";
			AssertEquals("111111", invoiceWrapper.LetterOfCreditNumber);
			AssertEquals("18-Sep-71", invoiceWrapper.LetterOfCreditDate);
			AssertEquals("E.SUN BANK, ESB", invoiceWrapper.ExportersBankName);
			AssertEquals("0000806449", invoiceWrapper.ExportersBankAccountNo);
			AssertEquals("CITIUS33", invoiceWrapper.ExportersBankSWIFTCode);
			AssertEquals("Test AdditionalInformation", invoiceWrapper.AdditionalInformation);

			DocumentNote note = DocumentNote.LoadNote(shipment);
			note.SetSystemDefinedFieldValue(SDFields.AdditionalPaymentTerms, "Test AdditionalPaymentTerms");
			note.SetSystemDefinedFieldValue(SDFields.InsurancePolicyNumber, "Test InsurancePolicyNumber");
			note.SetSystemDefinedFieldValue(SDFields.InsuredValue, "Test InsuredValue");
			note.SetSystemDefinedFieldValue(SDFields.LetterOfCreditNumber, "Test LetterOfCreditNumber");
			note.SetSystemDefinedFieldValue(SDFields.LetterOfCreditDate, "Test LetterOfCreditDate");

			invoiceWrapper = new CommercialInvoiceWrapper(invoiceHeader, Factory);
			AssertEquals("Test AdditionalPaymentTerms", invoiceWrapper.AdditionalPaymentTerms);
			AssertEquals("Test InsurancePolicyNumber", invoiceWrapper.InsurancePolicyNumber);
			AssertEquals("Test InsuredValue", invoiceWrapper.InsuredValue);
			AssertEquals("Test LetterOfCreditNumber", invoiceWrapper.LetterOfCreditNumber);
			AssertEquals("Test LetterOfCreditDate", invoiceWrapper.LetterOfCreditDate);
		}

		public void TestCountryOfOrigin()
		{
			CommercialInvoiceWrapper invoiceWrapper = new CommercialInvoiceWrapper(null, Factory);
			AssertEquals(ZString.Empty, invoiceWrapper.CountryOfOrigin.Code);

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceWrapper = new CommercialInvoiceWrapper(invoiceHeader, Factory);
			AssertEquals(ZString.Empty, invoiceWrapper.CountryOfOrigin.Code);

			declaration.JE_RL_NKOrigin = "HKHKG";
			invoiceWrapper = new CommercialInvoiceWrapper(invoiceHeader, Factory);
			AssertEquals("HK", invoiceWrapper.CountryOfOrigin.Code);

			invoiceHeader.JZ_RN_NKDefaultOrigin = "TH";
			invoiceWrapper = new CommercialInvoiceWrapper(invoiceHeader, Factory);
			AssertEquals("TH", invoiceWrapper.CountryOfOrigin.Code);
		}

		void PopulateRefDataImporter()
		{
			var ain = Factory.New<RefDocOrgCusCode>();
			ain.DOC_DocumentType = "HBL";
			ain.DOC_RN_NKCodeCountry = Constants.CountryCodes.Eritrea;
			ain.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.Bangladesh;
			ain.DOC_Notes = "Tax Id";
			ain.DOC_CodeType = "AIN";
			ain.DOC_ShortLabel = "AIN";
			ain.DOC_Priority = 1;

			Factory.Save();
		}

		public void TestImporterRequiredVATNumber()
		{
			OrgHeader org1 = GetNewTestOrg("ORG1");
			org1.OH_Code = "BDSPD" + "UYMLZ" + new Random(DateTime.Now.Millisecond).Next(100).ToString();

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			PopulateRefDataImporter();

			OrgCusCode taxCode1 = org1.CustomsCodes.AddNew();
			taxCode1.OK_CodeType = OrgCusCode.BangladeshCodeTypes.AIN;
			taxCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Eritrea;
			taxCode1.OK_CustomsRegNo = OrgCusCode.BangladeshCodeTypes.AIN + "CVTEST";

			declaration.JE_OH_Importer = org1.PK;
			declaration.JE_OH_Supplier = Guid.Empty;
			declaration.JE_RL_NKOrigin = "UYMLZ";
			declaration.JE_RL_NKFinalDestination = "BDSPD";

			CommercialInvoiceWrapper invoiceWrapper = new CommercialInvoiceWrapper(invoiceHeader, Factory);
			AssertEquals("AIN: AINCVTEST", invoiceWrapper.ImporterRequiredVATNumber);

			declaration.JE_OH_Importer = Guid.Empty;
			AssertEquals(string.Empty, invoiceWrapper.ImporterRequiredVATNumber);
			declaration.JE_RL_NKOrigin = "";
			AssertEquals(string.Empty, invoiceWrapper.ImporterRequiredVATNumber);
			declaration.JE_RL_NKFinalDestination = "";
			AssertEquals(string.Empty, invoiceWrapper.ImporterRequiredVATNumber);
		}

		void PopulateRefDataSupplier()
		{
			var ain = Factory.New<RefDocOrgCusCode>();
			ain.DOC_DocumentType = "HBL";
			ain.DOC_RN_NKCodeCountry = Constants.CountryCodes.Eritrea;
			ain.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.Uruguay;
			ain.DOC_Notes = "Tax Id";
			ain.DOC_CodeType = "RUT";
			ain.DOC_ShortLabel = "RUT";
			ain.DOC_Priority = 1;

			Factory.Save();
		}

		public void TestSupplierRequiredVATNumber()
		{
			OrgHeader org2 = GetNewTestOrg("ORG2");
			org2.OH_Code = "BDSPD" + "UYMLZ" + new Random(DateTime.Now.Millisecond).Next(100).ToString();

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			PopulateRefDataSupplier();

			OrgCusCode taxCode1 = org2.CustomsCodes.AddNew();
			taxCode1.OK_CodeType = UruguayOrgCusCodeInfo.OrgCusCodes.RUT;
			taxCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Eritrea;
			taxCode1.OK_CustomsRegNo = UruguayOrgCusCodeInfo.OrgCusCodes.RUT + "CVTEST";

			declaration.JE_OH_Importer = Guid.Empty;
			declaration.JE_OH_Supplier = org2.PK;
			declaration.JE_RL_NKOrigin = "UYMLZ";
			declaration.JE_RL_NKFinalDestination = "BDSPD";

			CommercialInvoiceWrapper invoiceWrapper = new CommercialInvoiceWrapper(invoiceHeader, Factory);
			AssertEquals("RUT: RUTCVTEST", invoiceWrapper.SupplierRequiredVATNumber);

			declaration.JE_OH_Supplier = Guid.Empty;
			AssertEquals(string.Empty, invoiceWrapper.SupplierRequiredVATNumber);
			declaration.JE_RL_NKOrigin = "";
			AssertEquals(string.Empty, invoiceWrapper.SupplierRequiredVATNumber);
			declaration.JE_RL_NKFinalDestination = "";
			AssertEquals(string.Empty, invoiceWrapper.SupplierRequiredVATNumber);
		}

		public void TestIncludedCharges()
		{
			CommercialInvoiceWrapper invoiceWrapper = new CommercialInvoiceWrapper(null, Factory);
			AssertEquals(0m, invoiceWrapper.IncludedOverseasFreight.Amount);
			AssertEquals(0m, invoiceWrapper.IncludedOverseasInsurance.Amount);
			AssertEquals(0m, invoiceWrapper.IncludedExWorksCharges.Amount);
			AssertEquals(0m, invoiceWrapper.IncludedInlandFreight.Amount);
			AssertEquals(0m, invoiceWrapper.IncludedPackingCharges.Amount);
			AssertEquals(0m, invoiceWrapper.IncludedLandingCharges.Amount);
			AssertEquals(0m, invoiceWrapper.IncludedDutiableOtherCharges.Amount);
			AssertEquals(0m, invoiceWrapper.IncludedNonDutiableOtherCharges.Amount);
			AssertEquals(0m, invoiceWrapper.IncludedCommission.Amount);
			AssertEquals(0m, invoiceWrapper.IncludedDiscount.Amount);
			AssertEquals(0m, invoiceWrapper.IncludedChargesTotal.Amount);

			ZGuid currencyPK = GlbCompany.CurrentCompany.LocalCurrency.PK;
			ZString currencyCode = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 10000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = currencyCode;
			invoiceHeader.JZ_IncoTerm = Constants.IncoTerms.DeliveredDutyPaid;
			invoiceHeader.Charges.RemoveAll();

			BaseJobComInvHeaderCharge osFreightCharge = invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 3m, currencyCode);
			BaseJobComInvHeaderCharge osInsuranceCharge = invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 5m, currencyCode);
			BaseJobComInvHeaderCharge exWorksCharge = invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 7m, currencyCode);
			BaseJobComInvHeaderCharge inlandFreightCharge = invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 11m, currencyCode);
			BaseJobComInvHeaderCharge packingCharge = invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 13m, currencyCode);
			BaseJobComInvHeaderCharge landingCharge = invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 17m, currencyCode);
			BaseJobComInvHeaderCharge dutiableOtherCharge = invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 19m, currencyCode);
			dutiableOtherCharge.J7_IsDutiable = true;
			BaseJobComInvHeaderCharge nonDutiableOtherCharge = invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 23m, currencyCode);
			nonDutiableOtherCharge.J7_IsDutiable = false;
			BaseJobComInvHeaderCharge comissionCharge = invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.Commission, 29m, currencyCode);
			BaseJobComInvHeaderCharge discountCharge = invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 31m, currencyCode);

			invoiceWrapper = new CommercialInvoiceWrapper(invoiceHeader, Factory);
			AssertEquals(3m, invoiceWrapper.IncludedOverseasFreight.Amount);
			AssertEquals(5m, invoiceWrapper.IncludedOverseasInsurance.Amount);
			AssertEquals(7m, invoiceWrapper.IncludedExWorksCharges.Amount);
			AssertEquals(11m, invoiceWrapper.IncludedInlandFreight.Amount);
			AssertEquals(13m, invoiceWrapper.IncludedPackingCharges.Amount);
			AssertEquals(17m, invoiceWrapper.IncludedLandingCharges.Amount);
			AssertEquals(19m, invoiceWrapper.IncludedDutiableOtherCharges.Amount);
			AssertEquals(23m, invoiceWrapper.IncludedNonDutiableOtherCharges.Amount);
			AssertEquals(29m, invoiceWrapper.IncludedCommission.Amount);
			AssertEquals(31m, invoiceWrapper.IncludedDiscount.Amount);
			AssertEquals(96m, invoiceWrapper.IncludedChargesTotal.Amount);
		}

		public void TestExcludedCharges()
		{
			CommercialInvoiceWrapper invoiceWrapper = new CommercialInvoiceWrapper(null, Factory);
			AssertEquals(0m, invoiceWrapper.ExcludedOverseasFreight.Amount);
			AssertEquals(0m, invoiceWrapper.ExcludedOverseasInsurance.Amount);
			AssertEquals(0m, invoiceWrapper.ExcludedExWorksCharges.Amount);
			AssertEquals(0m, invoiceWrapper.ExcludedInlandFreight.Amount);
			AssertEquals(0m, invoiceWrapper.ExcludedPackingCharges.Amount);
			AssertEquals(0m, invoiceWrapper.ExcludedLandingCharges.Amount);
			AssertEquals(0m, invoiceWrapper.ExcludedDutiableOtherCharges.Amount);
			AssertEquals(0m, invoiceWrapper.ExcludedNonDutiableOtherCharges.Amount);
			AssertEquals(0m, invoiceWrapper.ExcludedCommission.Amount);
			AssertEquals(0m, invoiceWrapper.ExcludedDiscount.Amount);
			AssertEquals(0m, invoiceWrapper.ExcludedChargesTotal.Amount);

			ZGuid currencyPK = GlbCompany.CurrentCompany.LocalCurrency.PK;
			ZString currencyCode = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 10000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = currencyCode;
			invoiceHeader.JZ_IncoTerm = Constants.IncoTerms.UnpackedAtFactory;
			invoiceHeader.Charges.RemoveAll();

			invoiceHeader.GroupCharges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 3m, currencyCode).J7_Calc_IsIncludedInInvoiceAmount = false;
			invoiceHeader.GroupCharges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 5m, currencyCode).J7_Calc_IsIncludedInInvoiceAmount = false;
			invoiceHeader.GroupCharges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 7m, currencyCode).J7_Calc_IsIncludedInInvoiceAmount = false;
			invoiceHeader.GroupCharges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 11m, currencyCode).J7_Calc_IsIncludedInInvoiceAmount = false;
			invoiceHeader.GroupCharges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 13m, currencyCode).J7_Calc_IsIncludedInInvoiceAmount = false;
			invoiceHeader.GroupCharges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 17m, currencyCode).J7_Calc_IsIncludedInInvoiceAmount = false;
			BaseJobComInvHeaderCharge dutiableOtherCharge = invoiceHeader.GroupCharges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 19m, currencyCode);
			dutiableOtherCharge.J7_IsDutiable = true;
			dutiableOtherCharge.J7_Calc_IsIncludedInInvoiceAmount = false;
			BaseJobComInvHeaderCharge nonDutiableOtherCharge = invoiceHeader.GroupCharges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 23m, currencyCode);
			nonDutiableOtherCharge.J7_IsDutiable = false;
			nonDutiableOtherCharge.J7_Calc_IsIncludedInInvoiceAmount = false;
			invoiceHeader.GroupCharges.AddNew(CustomsChargeTypeList.Codes.Commission, 29m, currencyCode).J7_Calc_IsIncludedInInvoiceAmount = false;
			invoiceHeader.GroupCharges.AddNew(CustomsChargeTypeList.Codes.Discount, 31m, currencyCode).J7_Calc_IsIncludedInInvoiceAmount = false;

			invoiceWrapper = new CommercialInvoiceWrapper(invoiceHeader, Factory);
			AssertEquals(3m, invoiceWrapper.ExcludedOverseasFreight.Amount);
			AssertEquals(5m, invoiceWrapper.ExcludedOverseasInsurance.Amount);
			AssertEquals(7m, invoiceWrapper.ExcludedExWorksCharges.Amount);
			AssertEquals(11m, invoiceWrapper.ExcludedInlandFreight.Amount);
			AssertEquals(13m, invoiceWrapper.ExcludedPackingCharges.Amount);
			AssertEquals(17m, invoiceWrapper.ExcludedLandingCharges.Amount);
			AssertEquals(19m, invoiceWrapper.ExcludedDutiableOtherCharges.Amount);
			AssertEquals(23m, invoiceWrapper.ExcludedNonDutiableOtherCharges.Amount);
			AssertEquals(29m, invoiceWrapper.ExcludedCommission.Amount);
			AssertEquals(31m, invoiceWrapper.ExcludedDiscount.Amount);
			AssertEquals(96m, invoiceWrapper.ExcludedChargesTotal.Amount);
		}

		public void TestStaticNewConstructorFromNull()
		{
			CommercialInvoiceWrapper[] nullWrapperCollection = CommercialInvoiceWrapper.New(null, Factory);
			AssertEquals("CommercialInvoiceWrapper.New(null, Factory)", null, nullWrapperCollection);
		}

		public void TestStaticNewConstructorFromShipment()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			BaseJobComInvoiceHeader invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceHeader invoiceHeader2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			CommercialInvoiceWrapper[] invoiceWrapperArray = CommercialInvoiceWrapper.New(shipment, Factory);
			AssertEquals(2, invoiceWrapperArray.Length);
			AssertEquals(invoiceHeader1, invoiceWrapperArray[0].WrappedObject);
			AssertEquals(invoiceHeader2, invoiceWrapperArray[1].WrappedObject);
		}

		public void TestStaticNewConstructorFromDeclaration()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceHeader invoiceHeader2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			CommercialInvoiceWrapper[] invoiceWrapperArray = CommercialInvoiceWrapper.New(declaration, Factory);
			AssertEquals(2, invoiceWrapperArray.Length);
			AssertEquals(invoiceHeader1, invoiceWrapperArray[0].WrappedObject);
			AssertEquals(invoiceHeader2, invoiceWrapperArray[1].WrappedObject);
		}

		public void TestStaticNewConstructorFromInvoiceHeader()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceHeader invoiceHeader2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceHeader invoiceHeader3 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			CommercialInvoiceWrapper[] invoiceWrapperArray1 = CommercialInvoiceWrapper.New(invoiceHeader2, Factory);
			AssertEquals(1, invoiceWrapperArray1.Length);
			AssertEquals(invoiceHeader2, invoiceWrapperArray1[0].WrappedObject);

			CommercialInvoiceWrapper[] invoiceWrapperArray2 = CommercialInvoiceWrapper.New(invoiceHeader3, Factory);
			AssertEquals(1, invoiceWrapperArray2.Length);
			AssertEquals(invoiceHeader3, invoiceWrapperArray2[0].WrappedObject);
		}

		public override void TestWrapperMappingsEmpty()
		{
			BaseJobComInvoiceHeader nullInvoiceHeader = Factory.GetNull<BaseJobComInvoiceHeader>();
			CommercialInvoiceWrapper wrapperEmpty = new CommercialInvoiceWrapper(null, Factory);
			AssertEquals("wrapperEmpty.ToString()", "", wrapperEmpty.ToString());
			AssertEquals(nullInvoiceHeader.JZ_IncoTerm, wrapperEmpty.IncoTerm.Code);
			AssertEquals(null, wrapperEmpty.FreightJob);
			AssertEquals(ZString.Empty, wrapperEmpty.InsuredValue.ToString());
			AssertEquals(ZString.Empty, wrapperEmpty.InvoiceAmount.ToString());
			AssertEquals(ZString.Empty, wrapperEmpty.Importer.CompanyName);
			AssertEquals(ZString.Empty, wrapperEmpty.Supplier.CompanyName);
			AssertEquals(ZString.Empty, wrapperEmpty.Volume.ToString());
			AssertEquals(ZString.Empty, wrapperEmpty.Weight.ToString());
			AssertEquals(ZDecimal.Zero, wrapperEmpty.ExchangeRate);
			AssertEquals(nullInvoiceHeader.JZ_InvoiceDate, wrapperEmpty.InvoiceDate);
			AssertEquals(ZString.Empty, wrapperEmpty.InvoiceNumber);
			AssertEquals(0, wrapperEmpty.InvoiceLines.Count);
			AssertEquals(0, wrapperEmpty.UnclassifiedInvoiceLines.Count);
		}

		public void TestWrapperMappingsFull()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_GoodsDescription = "SYDNEY AIRPORT";
			declaration.JE_TotalNoOfPacksPackType = "PKG";
			BaseJobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_OH_Buyer = OrgTestHelper.GetNewOrganisation("BUY", Factory).PK;
			invoiceHeader.JZ_OH_Supplier = OrgTestHelper.GetNewOrganisation("SUP", Factory).PK;
			invoiceHeader.JZ_InvoiceNumber = "NUMBER_ONE";
			invoiceHeader.JZ_InvoiceAmount = 1234.56m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "NZD";
			invoiceHeader.JZ_Volume = 4.53m;
			invoiceHeader.JZ_VolumeUQ = "M3";
			invoiceHeader.JZ_Weight = 340m;
			invoiceHeader.JZ_WeightUQ = "KG";
			invoiceHeader.JZ_InvoiceCurrExRate = 0.56m;
			invoiceHeader.JZ_InvoiceDate = new ZDateTime(2006, 8, 9);
			invoiceHeader.JZ_MarksAndNumbers = "JZ_MarksAndNumbers";
			invoiceHeader.JZ_NoOfPacks = 101;

			BaseJobComInvoiceLine invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = ZString.Empty;
			BaseJobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "1221.1442.43";

			CommercialInvoiceWrapper wrapperFull = new CommercialInvoiceWrapper(invoiceHeader, Factory);

			AssertEquals("FOB - Free On Board", wrapperFull.IncoTerm.ToString());
			AssertEquals("SYDNEY AIRPORT", wrapperFull.FreightJob.GoodsDescription);
			AssertEquals("1,234.56 NZD", wrapperFull.InsuredValue.ToString());
			AssertEquals("1,234.56 NZD", wrapperFull.InvoiceAmount.ToString());
			AssertEquals("BUY_NAME", wrapperFull.Importer.CompanyName);
			AssertEquals("SUP_NAME", wrapperFull.Supplier.CompanyName);
			AssertEquals("4.53 M3", wrapperFull.Volume.ToString());
			AssertEquals("340 KG", wrapperFull.Weight.ToString());
			AssertEquals(0.56m, wrapperFull.ExchangeRate);
			AssertEquals(new ZDateTime(2006, 8, 9), wrapperFull.InvoiceDate);
			AssertEquals("NUMBER_ONE", wrapperFull.InvoiceNumber);

			AssertEquals(2, wrapperFull.InvoiceLines.Count);
			AssertEquals(1, wrapperFull.UnclassifiedInvoiceLines.Count);

			AssertEquals("JZ_MarksAndNumbers", wrapperFull.MarksAndNumbers);
			AssertEquals("101 PKG", wrapperFull.NoOfPacks.ToString());
		}

		public void TestVolumeAndWeight()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_GoodsDescription = "SYDNEY AIRPORT";
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_Volume = 4.535874m;
			invoiceHeader.JZ_VolumeUQ = "M3";
			invoiceHeader.JZ_Weight = 340.235174m;
			invoiceHeader.JZ_WeightUQ = "KG";

			var wrapperFull = new CommercialInvoiceWrapper(invoiceHeader, Factory);
			AssertEquals("4.536 M3", wrapperFull.Volume.ToString());
			AssertEquals("340.235 KG", wrapperFull.Weight.ToString());
		}

		public void TestResponsibleParty()
		{
			var supplier = OrgTestHelper.GetNewOrganisation("SUP", Factory);
			supplier.OH_FullName = "ABC INC.";

			var invoiceHeader = Factory.New<BaseJobComInvoiceHeader>();
			invoiceHeader.JZ_InvoiceNumber = "12345";
			invoiceHeader.JZ_InvoiceDate = new ZDateTime(2025, 04, 07);
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			invoiceHeader.InvoiceHeaderRefs.AddNew(InvoiceHeaderRefsTypeList.Codes.RP, "JOHN SMITH");

			var wrapper = new CommercialInvoiceWrapper(invoiceHeader, Factory);
			AssertEquals("Date Of Issue", invoiceHeader.JZ_InvoiceDate, wrapper.DateOfIssue);
			AssertEquals("Signatory's Company", supplier.OH_FullName, wrapper.SignatoryCompany);
			AssertEquals("Name Of Signatory", invoiceHeader.InvoiceHeaderRefs.GetFirstJobComInvoiceHeaderRefs(InvoiceHeaderRefsTypeList.Codes.RP).J2_ReferenceNumber, wrapper.NameOfSignatory);

			invoiceHeader.InvoiceHeaderRefs.DeleteAll();
			AssertEquals("Date Of Issue", wrapper.Now, wrapper.DateOfIssue);
			AssertEquals("Signatory's Company", wrapper.CurrentBranch.BranchName, wrapper.SignatoryCompany);
			AssertEquals("Name Of Signatory", ZString.Empty, wrapper.NameOfSignatory);
		}

		#region US Certificate of Origin custom fields

		public void TestLocalChamberOfCommerceInfo()
		{
			DocumentsDataRegistry.Instance.LocalChamberOfCommerceInformation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "information");
			CommercialInvoiceWrapper wrapper = new CommercialInvoiceWrapper(null, Factory);

			AssertEquals("LocalChamberOfCommerceInfo", "information", wrapper.LocalChamberOfCommerceInfo);
		}

		public void TestNotaryPublicInfo()
		{
			DocumentsDataRegistry.Instance.NotaryPublicInformation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "information");
			CommercialInvoiceWrapper wrapper = new CommercialInvoiceWrapper(null, Factory);

			AssertEquals("NotaryPublicInfo", "information", wrapper.NotaryPublicInfo);
		}

		#endregion

		#region Implementation
		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
CommercialInvoice                       (Default Field: InvoiceNumber)
======================================================================
Name                                    Type
----------------------------------------------------------------------
IncoTerm                                CodeAndDescription
CountryOfOrigin                         Country
FreightJob                              Freight
ExcludedChargesTotal                    Money
ExcludedCommission                      Money
ExcludedDiscount                        Money
ExcludedDutiableOtherCharges            Money
ExcludedExWorksCharges                  Money
ExcludedInlandFreight                   Money
ExcludedLandingCharges                  Money
ExcludedNonDutiableOtherCharges         Money
ExcludedOverseasFreight                 Money
ExcludedOverseasInsurance               Money
ExcludedPackingCharges                  Money
IncludedChargesTotal                    Money
IncludedCommission                      Money
IncludedDiscount                        Money
IncludedDutiableOtherCharges            Money
IncludedExWorksCharges                  Money
IncludedInlandFreight                   Money
IncludedLandingCharges                  Money
IncludedNonDutiableOtherCharges         Money
IncludedOverseasFreight                 Money
IncludedOverseasInsurance               Money
IncludedPackingCharges                  Money
InvoiceAmount                           Money
Buyer                                   Organisation
Importer                                Organisation
Supplier                                Organisation
NoOfPacks                               ValueAndUnit
Volume                                  ValueAndUnit
Weight                                  ValueAndUnit
AdditionalInformation                   String
AdditionalPaymentTerms                  String
ApprovalNumber                          String
DateOfIssue                             DateTime
ExchangeRate                            Decimal
ExportersBankAccountNo                  String
ExportersBankName                       String
ExportersBankSWIFTCode                  String
ImporterRequiredVATNumber               String
InsurancePolicyNumber                   String
InsuredValue                            String
InvoiceDate                             DateTime
InvoiceNumber                           String
JobNumber                               String
LCDateNumber                            String
LCDateNumberOrInvoiceDate               String
LetterOfCreditDate                      String
LetterOfCreditNumber                    String
LocalChamberOfCommerceInfo              String
MarksAndNumbers                         String
NameOfSignatory                         String
NotaryPublicInfo                        String
SignatoryCompany                        String
SupplierRequiredVATNumber               String

InvoiceLines                            CommercialInvoiceLine Collection
UnclassifiedInvoiceLines                CommercialInvoiceLine Collection
";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Buyer : 
CountryOfOrigin : NZ - New Zealand
ExcludedChargesTotal : 96.00 NZD
ExcludedCommission : 29.00 NZD
ExcludedDiscount : 31.00 NZD
ExcludedDutiableOtherCharges : 19.00 NZD
ExcludedExWorksCharges : 7.00 NZD
ExcludedInlandFreight : 11.00 NZD
ExcludedLandingCharges : 17.00 NZD
ExcludedNonDutiableOtherCharges : 23.00 NZD
ExcludedOverseasFreight : 3.00 NZD
ExcludedOverseasInsurance : 5.00 NZD
ExcludedPackingCharges : 13.00 NZD
FreightJob : 
Importer : BUY_NAME\nBUY_ADDRESS1\nBUY_ADDRESS2\nBUY_CITY BUY_S BUY_PC
IncludedChargesTotal : 96.00 NZD
IncludedCommission : 29.00 NZD
IncludedDiscount : 31.00 NZD
IncludedDutiableOtherCharges : 19.00 NZD
IncludedExWorksCharges : 7.00 NZD
IncludedInlandFreight : 11.00 NZD
IncludedLandingCharges : 17.00 NZD
IncludedNonDutiableOtherCharges : 23.00 NZD
IncludedOverseasFreight : 3.00 NZD
IncludedOverseasInsurance : 5.00 NZD
IncludedPackingCharges : 13.00 NZD
IncoTerm : UAF
InvoiceAmount : 1,234.56 NZD
NoOfPacks : 100 PKG
Registry : (No Default Field Value Available on Registry)
Supplier : SUP_NAME\nSUP_ADDRESS1\nSUP_ADDRESS2\nSUP_CITY SUP_S SUP_PC
Volume : 4.53 M3
Weight : 340 KG
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			RefCurrency currencyCode = RefCurrency.LoadFromCurrencyCode(Factory, "NZD");

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TotalNoOfPacksPackType = "PKG";
			BaseJobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_RN_NKDefaultOrigin = "NZ";
			invoiceHeader.JZ_OH_Buyer = OrgTestHelper.GetNewOrganisation("BUY", Factory).PK;
			invoiceHeader.JZ_OH_Supplier = OrgTestHelper.GetNewOrganisation("SUP", Factory).PK;
			invoiceHeader.JZ_InvoiceNumber = "NUMBER_ONE";
			invoiceHeader.JZ_InvoiceAmount = 1234.56m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = currencyCode.RX_Code;
			invoiceHeader.JZ_Volume = 4.53m;
			invoiceHeader.JZ_VolumeUQ = "M3";
			invoiceHeader.JZ_Weight = 340m;
			invoiceHeader.JZ_WeightUQ = "KG";
			invoiceHeader.JZ_InvoiceCurrExRate = 0.56m;
			invoiceHeader.JZ_InvoiceDate = new ZDateTime(2006, 8, 9);
			invoiceHeader.JZ_IncoTerm = Constants.IncoTerms.UnpackedAtFactory;
			invoiceHeader.JZ_NoOfPacks = 100m;
			invoiceHeader.Charges.RemoveAll();

			invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 3m, currencyCode.RX_Code);
			invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 5m, currencyCode.RX_Code);
			invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 7m, currencyCode.RX_Code);
			invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 11m, currencyCode.RX_Code);
			invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 13m, currencyCode.RX_Code);
			invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 17m, currencyCode.RX_Code);
			BaseJobComInvHeaderCharge dutiableOtherCharge = invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 19m, currencyCode.RX_Code);
			dutiableOtherCharge.J7_IsDutiable = true;
			BaseJobComInvHeaderCharge nonDutiableOtherCharge = invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 23m, currencyCode.RX_Code);
			nonDutiableOtherCharge.J7_IsDutiable = false;
			invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.Commission, 29m, currencyCode.RX_Code);
			invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 31m, currencyCode.RX_Code);

			invoiceHeader.GroupCharges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 3m, currencyCode.RX_Code).J7_Calc_IsIncludedInInvoiceAmount = false;
			invoiceHeader.GroupCharges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 5m, currencyCode.RX_Code).J7_Calc_IsIncludedInInvoiceAmount = false;
			invoiceHeader.GroupCharges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 7m, currencyCode.RX_Code).J7_Calc_IsIncludedInInvoiceAmount = false;
			invoiceHeader.GroupCharges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 11m, currencyCode.RX_Code).J7_Calc_IsIncludedInInvoiceAmount = false;
			invoiceHeader.GroupCharges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 13m, currencyCode.RX_Code).J7_Calc_IsIncludedInInvoiceAmount = false;
			invoiceHeader.GroupCharges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 17m, currencyCode.RX_Code).J7_Calc_IsIncludedInInvoiceAmount = false;
			BaseJobComInvHeaderCharge dutiableOtherGroupCharge = invoiceHeader.GroupCharges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 19m, currencyCode.RX_Code);
			dutiableOtherGroupCharge.J7_IsDutiable = true;
			dutiableOtherGroupCharge.J7_Calc_IsIncludedInInvoiceAmount = false;
			BaseJobComInvHeaderCharge nonDutiableOtherGroupCharge = invoiceHeader.GroupCharges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 23m, currencyCode.RX_Code);
			nonDutiableOtherGroupCharge.J7_IsDutiable = false;
			nonDutiableOtherGroupCharge.J7_Calc_IsIncludedInInvoiceAmount = false;
			invoiceHeader.GroupCharges.AddNew(CustomsChargeTypeList.Codes.Commission, 29m, currencyCode.RX_Code).J7_Calc_IsIncludedInInvoiceAmount = false;
			invoiceHeader.GroupCharges.AddNew(CustomsChargeTypeList.Codes.Discount, 31m, currencyCode.RX_Code).J7_Calc_IsIncludedInInvoiceAmount = false;

			return new CommercialInvoiceWrapper(invoiceHeader, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new CommercialInvoiceWrapper(null, Factory);
		}

		OrgHeader GetNewTestOrg(string nameAndCode)
		{
			OrgHeader org3 = Factory.New<OrgHeader>();
			org3.OH_FullName = nameAndCode;
			org3.OH_Code = nameAndCode;
			return org3;
		}
		#endregion
	}
}
