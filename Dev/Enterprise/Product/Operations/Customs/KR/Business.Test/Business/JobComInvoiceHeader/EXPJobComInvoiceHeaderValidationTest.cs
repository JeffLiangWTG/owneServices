using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class EXPJobComInvoiceHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestIndustrialParkCodeAndDRWApplicantType()
		{
			CombineAssertions("Check IndustrialParkCode", () =>
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateNewOrGetExistingCusCodeType(ZZ.NKCodeType.IndustrialParkCode, "Industrial Park Code");
				helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
				helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.IndustrialParkCode, "101", "한국수출", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
				Factory.Save();
				var org = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "KRTEST1", "CompanyName");
				org.MainAddress.Postcode = "12345";
				var org2 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "KRTEST2", "CompanyName2");
				var cusCodes2 = new IDNumberAndType[]
				{
					new IDNumberAndType() { Type = IdentificationType.IndustrialParkCode, Number = "001", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth }
				};
				TestOrgDataSetUpHelper.AddCustomsCode(org2.MainAddress, cusCodes2);
				org2.MainAddress.Postcode = "12345";

				var org3 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "KRTEST3", "CompanyName3");
				var cusCodes3 = new IDNumberAndType[]
				{
					new IDNumberAndType() { Type = IdentificationType.IndustrialParkCode, Number = "101", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth }
				};
				TestOrgDataSetUpHelper.AddCustomsCode(org3.MainAddress, cusCodes3);
				org3.MainAddress.Postcode = "12345";

				var org4 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "KRTEST4", "CompanyName4");
				TestOrgDataSetUpHelper.AddOrgAddress(org4.MainAddress, "서울특별시 서초구 서초대로 64길 55 (서초동,준원빌딩3층)", "", "06636");
				var cusCodes4 = new IDNumberAndType[]
				{
					new IDNumberAndType() { Type = Constants.IdentificationType.UnipassIDForOrganization, Number = "레디코리1234000", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth }
				};
				TestOrgDataSetUpHelper.AddCustomsCode(org4, cusCodes4);
				var cusCodesAddress4 = new IDNumberAndType[]
				{
					new IDNumberAndType() { Type = IdentificationType.IndustrialParkCode, Number = "101", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth },
				};
				TestOrgDataSetUpHelper.AddCustomsCode(org4.MainAddress, cusCodesAddress4);
				Factory.Save();

				var validation = (EXPJobComInvoiceHeaderValidation)invoice.Validation;

				declaration.JE_ProcedureType = "E";
				invoice.JZ_OA_ManufacturerAddress = org.MainAddress.PK;
				validation.ValidateJZ_OA_ManufacturerAddress();
				validation.ValidateManufacturerIPCCode();
				validation.ValidateManufacturerUnipassID();
				AssertNoWarningContaining(invoice.ManufacturerUnipassIDInfo, "Please check if this Manufacturer has a Unipass ID assigned. As there is no Unipass ID, '제조미상9999000' will be sent in a declaration.");
				AssertNoWarningContaining(invoice.ManufacturerIPCCodeInfo, "Please check if this Manufacturer has a Industrial Park Code assigned. As there is no Industrial Park Code, '999' will be sent in a declaration.");
				AssertNoMessageErrorContaining(invoice.ManufacturerIPCCodeInfo, ListValidation.InvalidCodeMessageError);

				declaration.JE_ProcedureType = "H";
				validation.ValidateJZ_OA_ManufacturerAddress();
				validation.ValidateManufacturerIPCCode();
				validation.ValidateManufacturerUnipassID();
				AssertHasWarningContaining(invoice.ManufacturerUnipassIDInfo, "Please check if this Manufacturer has a Unipass ID assigned. As there is no Unipass ID, '제조미상9999000' will be sent in a declaration.");
				AssertHasWarningContaining(invoice.ManufacturerIPCCodeInfo, "Please check if this Manufacturer has a Industrial Park Code assigned. As there is no Industrial Park Code, '999' will be sent in a declaration.");
				AssertNoMessageErrorContaining(invoice.ManufacturerIPCCodeInfo, ListValidation.InvalidCodeMessageError);

				invoice.JZ_OA_ManufacturerAddress = org2.MainAddress.PK;
				validation.ValidateJZ_OA_ManufacturerAddress();
				validation.ValidateManufacturerIPCCode();
				validation.ValidateManufacturerUnipassID();
				AssertHasWarningContaining(invoice.ManufacturerUnipassIDInfo, "Please check if this Manufacturer has a Unipass ID assigned. As there is no Unipass ID, '제조미상9999000' will be sent in a declaration.");
				AssertNoWarningContaining(invoice.ManufacturerIPCCodeInfo, "Please check if this Manufacturer has a Industrial Park Code assigned. As there is no Industrial Park Code, '999' will be sent in a declaration.");
				AssertHasMessageErrorContaining(invoice.ManufacturerIPCCodeInfo, ListValidation.InvalidCodeMessageError);

				invoice.JZ_OA_ManufacturerAddress = org3.MainAddress.PK;
				validation.ValidateJZ_OA_ManufacturerAddress();
				validation.ValidateManufacturerIPCCode();
				validation.ValidateManufacturerUnipassID();
				AssertHasWarningContaining(invoice.ManufacturerUnipassIDInfo, "Please check if this Manufacturer has a Unipass ID assigned. As there is no Unipass ID, '제조미상9999000' will be sent in a declaration.");
				AssertNoWarningContaining(invoice.ManufacturerIPCCodeInfo, "Please check if this Manufacturer has a Industrial Park Code assigned. As there is no Industrial Park Code, '999' will be sent in a declaration.");
				AssertNoMessageErrorContaining(invoice.ManufacturerIPCCodeInfo, ListValidation.InvalidCodeMessageError);

				invoice.JZ_DRWApplicantType = DrawbackApplicantTypeList.Codes.Supplier;
				validation.ValidateJZ_OA_ManufacturerAddress();
				validation.ValidateManufacturerIPCCode();
				validation.ValidateManufacturerUnipassID();
				AssertNoMessageErrorContaining(invoice.JZ_OA_ManufacturerAddressInfo, "You have indicated that a drawback applicant type is '2 - Manufacturer', but this manufacturer does not have a UNIPASS ID entered.");
				AssertHasWarningContaining(invoice.ManufacturerUnipassIDInfo, "Please check if this Manufacturer has a Unipass ID assigned. As there is no Unipass ID, '제조미상9999000' will be sent in a declaration.");
				AssertNoNotifications(invoice.ManufacturerIPCCodeInfo);

				invoice.JZ_DRWApplicantType = DrawbackApplicantTypeList.Codes.Manufacturer;
				invoice.JZ_OA_ManufacturerAddress = ZGuid.Empty;
				AssertHasMessageErrorContaining(invoice.JZ_OA_ManufacturerAddressInfo, "If 'Drawback Applicant Type' is '2', the Manufacturer's address must be entered.");
				invoice.JZ_OA_ManufacturerAddress = org3.MainAddress.PK;
				validation.ValidateManufacturerIPCCode();
				validation.ValidateManufacturerUnipassID();
				AssertHasMessageErrorContaining(invoice.JZ_OA_ManufacturerAddressInfo, "You have indicated that a drawback applicant type is '2 - Manufacturer', but this manufacturer does not have a UNIPASS ID entered.");
				AssertHasWarningContaining(invoice.ManufacturerUnipassIDInfo, "Please check if this Manufacturer has a Unipass ID assigned. As there is no Unipass ID, '제조미상9999000' will be sent in a declaration.");
				AssertNoNotifications(invoice.ManufacturerIPCCodeInfo);

				invoice.JZ_OA_ManufacturerAddress = org4.MainAddress.PK;
				validation.ValidateJZ_OA_ManufacturerAddress();
				validation.ValidateManufacturerIPCCode();
				validation.ValidateManufacturerUnipassID();
				AssertNoNotifications(invoice.JZ_OA_ManufacturerAddressInfo);
				AssertNoNotifications(invoice.ManufacturerUnipassIDInfo);
				AssertNoNotifications(invoice.ManufacturerIPCCodeInfo);
			});
		}

		public void TestInvoicePaymentTerm()
		{
			CombineAssertions("Check InvoicePaymentTerm", () =>
			{
				declaration.JE_ExportGoodsType = TransactionTypeCodeList.Codes._11;
				invoice.JZ_PaymentTerms = ZString.Empty;
				AssertHasMessageErrorContaining(invoice.JZ_PaymentTermsInfo, MandatoryValidation.YouHaveNotEntered);

				invoice.JZ_PaymentTerms = "12";
				AssertHasMessageErrorContaining(invoice.JZ_PaymentTermsInfo, ListValidation.InvalidCodeMessageError);

				invoice.JZ_PaymentTerms = InvoicePaymentTermCodeList.Codes.GN;
				AssertHasMessageErrorContaining(invoice.JZ_PaymentTermsInfo, "If Transaction Type Code is '11' then, Invoice Payment Term must not be 'GN'.");

				invoice.JZ_PaymentTerms = InvoicePaymentTermCodeList.Codes.CD;
				AssertNoMessageErrors(invoice.JZ_PaymentTermsInfo);
			});
		}

		public void TestJZ_LetterOfCreditNumber()
		{
			CombineAssertions("Check JZ_LetterOfCreditNumber", () =>
			{
				invoice.JZ_PaymentTerms = "LS";
				invoice.JZ_LetterOfCreditNumber = "";
				AssertHasMessageErrorContaining(invoice.JZ_LetterOfCreditNumberInfo, MandatoryValidation.YouHaveNotEntered);

				invoice.JZ_LetterOfCreditNumber = "AAA";
				AssertNoMessageErrorContaining(invoice.JZ_LetterOfCreditNumberInfo, MandatoryValidation.YouHaveNotEntered);

				invoice.JZ_PaymentTerms = "";
				invoice.JZ_LetterOfCreditNumber = "";
				AssertNoMessageErrorContaining(invoice.JZ_LetterOfCreditNumberInfo, MandatoryValidation.YouHaveNotEntered);

				invoice.JZ_PaymentTerms = "LU";
				invoice.JZ_LetterOfCreditNumber = "";
				AssertHasMessageErrorContaining(invoice.JZ_LetterOfCreditNumberInfo, MandatoryValidation.YouHaveNotEntered);

				invoice.JZ_LetterOfCreditNumber = "AAA";
				AssertNoMessageErrorContaining(invoice.JZ_LetterOfCreditNumberInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestJZ_OA_ManufacturerAddress()
		{
			var manufacturer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK", "manufacturer Company Name");
			var manufacturerAddress = manufacturer.MainAddress;
			manufacturerAddress.Postcode = "123456";

			var manufacturerEmpty = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK", "");
			var manufacturerEmptyAddress = manufacturerEmpty.MainAddress;
			manufacturerEmptyAddress.Postcode = "";

			invoice.ManufacturerOrgPK = ZGuid.Empty;
			invoice.JZ_OA_ManufacturerAddress = ZGuid.Empty;
			AssertNoErrorContaining(invoice.JZ_OA_ManufacturerAddressInfo, "Please select an address for the entered organization.");

			invoice.ManufacturerOrgPK = manufacturer.PK;
			invoice.JZ_OA_ManufacturerAddress = ZGuid.Empty;
			AssertHasErrorContaining(invoice.JZ_OA_ManufacturerAddressInfo, "Please select an address for the entered organization.");

			invoice.JZ_OA_ManufacturerAddress = manufacturerAddress.PK;
			AssertNoErrorContaining(invoice.JZ_OA_ManufacturerAddressInfo, "Please select an address for the entered organization.");
			AssertHasMessageErrorContaining(invoice.JZ_OA_ManufacturerAddressInfo, "The maximum number of digits in a zip code is 5 digits.");
			AssertNoMessageErrorContaining(invoice.JZ_OA_ManufacturerAddressInfo, "The name of this company is missing. Press F3 here and enter the company name.");
			AssertNoMessageErrorContaining(invoice.JZ_OA_ManufacturerAddressInfo, "The post code of this company is missing. Press F3 here and enter the post code.");

			manufacturerAddress.Postcode = "12345";
			invoice.Validation.ValidateJZ_OA_ManufacturerAddress();
			AssertNoMessageErrorContaining(invoice.JZ_OA_ManufacturerAddressInfo, "The maximum number of digits in a zip code is 5 digits.");

			invoice.JZ_OA_ManufacturerAddress = manufacturerEmptyAddress.PK;
			AssertHasMessageErrorContaining(invoice.JZ_OA_ManufacturerAddressInfo, "The name of this company is missing. Press F3 here and enter the company name.");
			AssertHasMessageErrorContaining(invoice.JZ_OA_ManufacturerAddressInfo, "The post code of this company is missing. Press F3 here and enter the post code.");
		}

		public void TestJZ_OH_Buyer()
		{
			CombineAssertions("Check JZ_OH_Buyer", () =>
			{
				var buyerEmpty = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK1", "");
				var buyer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK2", "");
				var buyerCodes = new IDNumberAndType[]
				{
					new IDNumberAndType() { Type = IdentificationType.ForeignCompanyID, Number = "1234567890123" },
				};
				TestOrgDataSetUpHelper.AddCustomsCode(buyer, buyerCodes);

				var validation = (EXPJobComInvoiceHeaderValidation)invoice.Validation;
				declaration.JE_ProcedureType = "E";
				invoice.JZ_OH_Buyer = ZGuid.Empty;
				AssertHasMessageErrorContaining(invoice.JZ_OH_BuyerInfo, MandatoryValidation.YouHaveNotEntered);

				invoice.JZ_OH_Buyer = buyerEmpty.PK;
				validation.ValidateBuyerID();
				AssertNoWarningContaining(invoice.BuyerIDInfo, "Please check if this Buyer has a Buyer ID assigned. As there is no Buyer ID, 'ZZZZZZZZ9999A' will be sent in a declaration.");

				declaration.JE_ProcedureType = "B";
				invoice.JZ_OH_Buyer = buyerEmpty.PK;
				validation.ValidateBuyerID();
				AssertHasWarningContaining(invoice.BuyerIDInfo, "Please check if this Buyer has a Buyer ID assigned. As there is no Buyer ID, 'ZZZZZZZZ9999A' will be sent in a declaration.");

				invoice.JZ_OH_Buyer = buyer.PK;
				validation.ValidateBuyerID();
				AssertNoMessageErrorContaining(invoice.JZ_OH_BuyerInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoWarningContaining(invoice.BuyerIDInfo, "Please check if this Buyer has a Buyer ID assigned. As there is no Buyer ID, 'ZZZZZZZZ9999A' will be sent in a declaration.");
			});
		}

		public void TestJZ_NoOfPacks()
		{
			CombineAssertions("Check JZ_NoOfPacks", () =>
			{
				invoice.JZ_NoOfPacks = -1;
				AssertHasMessageErrorContaining(invoice.JZ_NoOfPacksInfo, "Please enter a 'Packages' greater than or equal to 0.");

				invoice.JZ_NoOfPacks = 0;
				AssertHasMessageErrorContaining(invoice.JZ_NoOfPacksInfo, "If 'Pack Type' is not 'bulk', then 'Total Packages' needs to be bigger than 0.");

				invoice.JZ_NoOfPacks = 1;
				AssertNoMessageErrors(invoice.JZ_NoOfPacksInfo);
			});
		}

		public void TestJZ_Weight()
		{
			CombineAssertions("Check JZ_Weight", () =>
			{
				invoice.JZ_Weight = -1;
				AssertHasMessageErrorContaining(invoice.JZ_WeightInfo, "Please enter an 'Inv. Gross Weight' greater than 0.");

				invoice.JZ_Weight = 0;
				AssertHasMessageErrorContaining(invoice.JZ_WeightInfo, "Please enter an 'Inv. Gross Weight' greater than 0.");

				invoice.InvoiceLines.AddNew().JI_NetWeight = 10;
				invoice.InvoiceLines.AddNew().JI_NetWeight = 20;
				invoice.InvoiceLines.AddNew().JI_NetWeight = 30;
				invoice.JZ_Weight = 1;
				AssertHasMessageErrorContaining(invoice.JZ_WeightInfo, "Please enter the gross weight of invoice greater than or equal to the total net weight of its invoice lines.");

				invoice.JZ_Weight = 60;
				AssertNoMessageErrors(invoice.JZ_WeightInfo);

				invoice.JZ_Weight = 100;
				AssertNoMessageErrors(invoice.JZ_WeightInfo);
			});
		}

		public void TestJZ_WeightUQ()
		{
			CombineAssertions("Check JZ_WeightUQ", () =>
			{
				invoice.JZ_WeightUQ = "";
				AssertHasMessageErrorContaining(invoice.JZ_WeightUQInfo, MandatoryValidation.YouHaveNotEntered);

				invoice.JZ_WeightUQ = "12";
				AssertHasMessageErrorContaining(invoice.JZ_WeightUQInfo, ListValidation.InvalidCodeMessageError);

				invoice.JZ_WeightUQ = "KG";
				AssertNoMessageErrors(invoice.JZ_WeightUQInfo);
			});
		}

		public void TestTotalInvoiceAmount()
		{
			CombineAssertions("Check TotalInvoiceAmount", () =>
			{
				invoice.JZ_InvoiceAmount = -1;
				AssertHasMessageErrorContaining(invoice.JZ_InvoiceAmountInfo, MandatoryValidation.ValueCannotBeNegative);

				invoice.JZ_InvoiceAmount = ZDecimal.Zero;
				AssertHasMessageErrorContaining(invoice.JZ_InvoiceAmountInfo, MandatoryValidation.ValueCannotBeZero);

				invoice.JZ_InvoiceAmount = 1;
				AssertNoMessageErrors(invoice.JZ_InvoiceAmountInfo);
			});
		}

		public void TestGrossWeight()
		{
			CombineAssertions("Check GrossWeight", () =>
			{
				invoice.JZ_Weight = ZDecimal.Zero;
				invoice.JZ_WeightUQ = ZString.Empty;
				AssertHasMessageErrorContaining(invoice.JZ_WeightInfo, "Please enter an 'Inv. Gross Weight' greater than 0.");
				AssertHasMessageErrorContaining(invoice.JZ_WeightUQInfo, MandatoryValidation.YouHaveNotEntered);

				invoice.JZ_Weight = 1;
				invoice.JZ_WeightUQ = "ZZ";
				AssertNoMessageErrors(invoice.JZ_WeightInfo);
				AssertHasMessageErrorContaining(invoice.JZ_WeightUQInfo, ListValidation.InvalidCodeMessageError);

				invoice.JZ_WeightUQ = Core.Constants.Weight.Kilograms;
				AssertNoMessageErrors(invoice.JZ_WeightUQInfo);
			});
		}

		public void TestJE_ExporterTypeAndManufacturer()
		{
			var manufacturer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK1", "");
			var manufacturerAddress1 = manufacturer.MainAddress;

			var exporter = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK2", "");
			var exporterAddress = exporter.MainAddress;
			var manufacturerAddress2 = exporter.Addresses.AddNew();

			var invoice1 = invoice;
			var invoice2 = declaration.Invoices.AddNew();

			declaration.JE_OA_SellerAddress = exporterAddress.PK;
			declaration.JE_ExporterType = ExporterTypeCodeList.Codes.A;
			invoice1.JZ_OA_ManufacturerAddress = manufacturerAddress1.PK;
			AssertHasMessageErrorContaining(invoice1.JZ_OA_ManufacturerAddressInfo, "You have indicated the exporter type as 'A - Manufacturer Export'. However the exporter and the manufacturer are set to the different organizations.");
			invoice2.JZ_OA_ManufacturerAddress = manufacturerAddress2.PK;
			AssertNoMessageErrorContaining(invoice2.JZ_OA_ManufacturerAddressInfo, "You have indicated the exporter type as 'A - Manufacturer Export'. However the exporter and the manufacturer are set to the different organizations.");

			declaration.JE_ExporterType = ExporterTypeCodeList.Codes.C;
			invoice1.JZ_OA_ManufacturerAddress = manufacturerAddress2.PK;
			AssertHasMessageErrorContaining(invoice1.JZ_OA_ManufacturerAddressInfo, "You have indicated the exporter type as 'C'. However the exporter and the manufacturer are set to the same organization.");
			invoice2.JZ_OA_ManufacturerAddress = manufacturerAddress1.PK;
			AssertNoMessageErrorContaining(invoice2.JZ_OA_ManufacturerAddressInfo, "You have indicated the exporter type as 'C'. However the exporter and the manufacturer are set to the same organization.");
		}

		public void TestJE_ExporterTypeIsD()
		{
			declaration.JE_ExporterType = ExporterTypeCodeList.Codes.D;
			var org1_1 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK1", "");
			var org1_1Codes = new IDNumberAndType[]
			{
					new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "가나다라1234001" },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(org1_1, org1_1Codes);
			var org1_2 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK2", "");
			var org1_2Codes = new IDNumberAndType[]
			{
					new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "가나다라1234002" },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(org1_2, org1_2Codes);

			var org2 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK3", "");
			var org2Codes = new IDNumberAndType[]
			{
					new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "가나다**1234003" },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(org2, org2Codes);

			var org3 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK5", "");
			var org3Codes = new IDNumberAndType[]
			{
					new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "가나****1234004" },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(org3, org3Codes);

			declaration.JE_OA_SellerAddress = org1_1.MainAddress.PK;
			invoice.JZ_OA_ManufacturerAddress = org1_1.MainAddress.PK;
			AssertHasMessageErrorContaining("Error, The exporter's UnipassID and The manufacturer's UnipassID are '가나다라1234001'.", invoice.JZ_OA_ManufacturerAddressInfo, "You have indicated the exporter type as 'D'. In that case, the UNIPASS ID of the exporter and manufacturer are expected to have the same sequence number except for the last 3 digits.");
			invoice.JZ_OA_ManufacturerAddress = org2.MainAddress.PK;
			AssertHasMessageErrorContaining("Error, The truncated exporter's UnipassID is '가나다라1234', but The truncated manufacturer's UnipassID is '가나다**1234'.", invoice.JZ_OA_ManufacturerAddressInfo, "You have indicated the exporter type as 'D'. In that case, the UNIPASS ID of the exporter and manufacturer are expected to have the same sequence number except for the last 3 digits.");
			invoice.JZ_OA_ManufacturerAddress = org3.MainAddress.PK;
			AssertHasMessageErrorContaining("Error, The truncated exporter's UnipassID is '가나다라1234', but The truncated manufacturer's UnipassID is '가나****1234'.", invoice.JZ_OA_ManufacturerAddressInfo, "You have indicated the exporter type as 'D'. In that case, the UNIPASS ID of the exporter and manufacturer are expected to have the same sequence number except for the last 3 digits.");

			invoice.JZ_OA_ManufacturerAddress = org1_2.MainAddress.PK;
			AssertNoMessageErrorContaining("Success, The exporter's UnipassID and The manufacturer's UnipassID are different. The truncated code is the same as '가나다라1234'.", invoice.JZ_OA_ManufacturerAddressInfo, "You have indicated the exporter type as 'D'. In that case, the UNIPASS ID of the exporter and manufacturer are expected to have the same sequence number except for the last 3 digits.");
		}

		public void TestJE_SimpleDRWAppAndManufacturer()
		{
			var wrongManufacturer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK1", "");
			var wrongManufacturerCodes = new IDNumberAndType[]
			{
					new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "" },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(wrongManufacturer, wrongManufacturerCodes);

			var manufacturer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK2", "");
			var manufacturerCodes = new IDNumberAndType[]
			{
					new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "123456789012345" },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(manufacturer, manufacturerCodes);

			var invoice1 = invoice;
			var invoice2 = declaration.Invoices.AddNew();

			declaration.JE_SimpleDRWApp = ApplicationForSimpleDrawbackCodeList.Codes.AD;

			invoice1.JZ_OA_ManufacturerAddress = wrongManufacturer.MainAddress.PK;
			AssertHasMessageErrorContaining(invoice1.JZ_OA_ManufacturerAddressInfo, "A simple drawback is indicated as AD. In this case, this manufacturer should have a UNIPASS ID issued.");

			invoice2.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			AssertNoMessageErrorContaining(invoice2.JZ_OA_ManufacturerAddressInfo, "A simple drawback is indicated as AD. In this case, this manufacturer should have a UNIPASS ID issued.");
		}

		public void TestCheckCertificateOfOriginIssueStatus()
		{
			invoice.Validation.ValidateAll();
			AssertNoMessageErrors(invoice.CertificateOfOriginIssueStatusInfo);

			invoice.CertificateOfOriginIssueStatus = "B";
			AssertHasMessageErrorContaining(invoice.CertificateOfOriginIssueStatusInfo, ListValidation.InvalidCodeMessageError);

			invoice.CertificateOfOriginIssueStatus = CertificateOfOriginIssuedCodeList.Codes.Y;
			AssertNoMessageErrorContaining(invoice.CertificateOfOriginIssueStatusInfo, ListValidation.InvalidCodeMessageError);

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.Validation.ValidateAll();
			AssertNoMessageErrors(invoiceLine.CertificateOfOriginIssueStatusInfo);

			invoiceLine.CertificateOfOriginIssueStatus = "B";
			AssertHasMessageErrorContaining(invoiceLine.CertificateOfOriginIssueStatusInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.CertificateOfOriginIssueStatus = CertificateOfOriginIssuedCodeList.Codes.Y;
			AssertNoMessageErrorContaining(invoiceLine.CertificateOfOriginIssueStatusInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestTotalPackages()
		{
			declaration.JE_TotalNoOfPacksPackType = "BA";
			invoice.Validation.ValidateJZ_NoOfPacks();
			AssertHasMessageErrorContaining(invoice.JZ_NoOfPacksInfo, "If 'Pack Type' is not 'bulk', then 'Total Packages' needs to be bigger than 0.");

			invoice.JZ_NoOfPacks = 10m;
			AssertNoMessageErrors(invoice.JZ_NoOfPacksInfo);

			declaration.JE_TotalNoOfPacksPackType = "VG";
			invoice.JZ_NoOfPacks = 0m;
			AssertNoMessageErrors(invoice.JZ_NoOfPacksInfo);
		}

		public void TestJZ_DRWApplicantType()
		{
			var manufacturer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK2", "");
			invoice.JZ_OH_Manufacturer = manufacturer.PK;
			invoice.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			invoice.JZ_DRWApplicantType = ZString.Empty;
			AssertNoMessageErrors(invoice.JZ_DRWApplicantTypeInfo);

			declaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.M;
			AssertNoMessageErrors(invoice.JZ_DRWApplicantTypeInfo);

			invoice.JZ_DRWApplicantType = DrawbackApplicantTypeList.Codes.Supplier;
			AssertHasMessageErrorContaining(invoice.JZ_DRWApplicantTypeInfo, "If Declaration Type is 'M', then Drawback Applicant Type must be empty.");

			declaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.H;
			invoice.JZ_DRWApplicantType = DrawbackApplicantTypeList.Codes.Supplier;
			AssertNoMessageErrors(invoice.JZ_DRWApplicantTypeInfo);

			invoice.JZ_DRWApplicantType = "X";
			AssertHasMessageErrorContaining(invoice.JZ_DRWApplicantTypeInfo, ListValidation.InvalidCodeMessageError);

			invoice.JZ_DRWApplicantType = DrawbackApplicantTypeList.Codes.Manufacturer;
			AssertHasMessageErrorContaining(invoice.JZ_DRWApplicantTypeInfo, "If Manufacturer does not have a UNIPASS ID, Drawback Applicant Type must not be '2'.");
			invoice.JZ_OA_ManufacturerAddress = ZGuid.Empty;
			invoice.Validation.ValidateJZ_DRWApplicantType();
			AssertNoMessageErrorContaining(invoice.JZ_DRWApplicantTypeInfo, "If Manufacturer does not have a UNIPASS ID, Drawback Applicant Type must not be '2'.");

			var manufacturerCodes = new IDNumberAndType[]
			{
					new IDNumberAndType() { Type = IdentificationType.UnipassIDForOrganization, Number = "123456789012543" },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(manufacturer, manufacturerCodes);
			invoice.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			invoice.Validation.ValidateJZ_DRWApplicantType();
			AssertNoMessageErrorContaining(invoice.JZ_DRWApplicantTypeInfo, "If Manufacturer does not have a UNIPASS ID, Drawback Applicant Type must not be '2'.");
		}

		public void TestJZ_ImportCargoManagementNumber()
		{
			invoice.JobDeclaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.B;
			invoice.Validation.ValidateJZ_ImportCargoManagementNumber();
			AssertNoMessageErrorContaining(invoice.JZ_ImportCargoManagementNumberInfo, MandatoryValidation.DoNotEntered);

			invoice.JZ_ImportCargoManagementNumber = "012345678901234";
			AssertHasMessageErrorContaining(invoice.JZ_ImportCargoManagementNumberInfo, MandatoryValidation.DoNotEntered);

			invoice.JobDeclaration.JE_ProcedureType = DeclarationProcedureTypeList.Codes.M;
			invoice.Validation.ValidateJZ_ImportCargoManagementNumber();
			AssertNoMessageErrorContaining(invoice.JZ_ImportCargoManagementNumberInfo, MandatoryValidation.YouHaveNotEntered);

			invoice.JZ_ImportCargoManagementNumber = "";
			AssertHasMessageErrorContaining(invoice.JZ_ImportCargoManagementNumberInfo, MandatoryValidation.YouHaveNotEntered);

			invoice.JZ_ImportCargoManagementNumber = "123";
			AssertHasMessageErrorContaining(invoice.JZ_ImportCargoManagementNumberInfo, "The length of the value should be 2, 15 or 19.");

			invoice.JZ_ImportCargoManagementNumber = "12";
			AssertHasMessageErrorContaining(invoice.JZ_ImportCargoManagementNumberInfo, "Only 'NO' is accepted when the length of the value is 2.");

			invoice.JZ_ImportCargoManagementNumber = "NO";
			AssertNoMessageErrors(invoice.JZ_ImportCargoManagementNumberInfo);

			invoice.JZ_ImportCargoManagementNumber = "01234567890123A";
			AssertHasMessageErrorContaining(invoice.JZ_ImportCargoManagementNumberInfo, "The last 4 digits should be numeric.");

			invoice.JZ_ImportCargoManagementNumber = "012345678901234";
			AssertNoMessageErrors(invoice.JZ_ImportCargoManagementNumberInfo);

			invoice.JZ_ImportCargoManagementNumber = "012345678901234567B";
			AssertHasMessageErrorContaining(invoice.JZ_ImportCargoManagementNumberInfo, "The last 8 digits should be numeric.");

			invoice.JZ_ImportCargoManagementNumber = "0123456789012345678";
			AssertNoMessageErrors(invoice.JZ_ImportCargoManagementNumberInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			invoice = declaration.Invoices.AddNew();
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "86420");

			Factory.Save();
		}
		JobComInvoiceHeader invoice;
		JobDeclaration declaration;
	}
}
