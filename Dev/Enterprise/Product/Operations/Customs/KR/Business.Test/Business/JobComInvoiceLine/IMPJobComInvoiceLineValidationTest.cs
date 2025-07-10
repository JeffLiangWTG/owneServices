using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class IMPJobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJI_CustomsSecondQty()
		{
			invoiceLine.Validation.ValidateJI_CustomsSecondQuantity();
			AssertNoMessageErrors(invoiceLine.JI_CustomsSecondQuantityInfo);

			invoiceLine.JI_CustomsSecondQuantity = 1110;
			AssertHasMessageErrorContaining(invoiceLine.JI_CustomsSecondQuantityInfo, "Second Customs Qty must be zero.");

			invoiceLine.JI_CustomsSecondUnitQty = "U";
			invoiceLine.Validation.ValidateJI_CustomsSecondQuantity();
			AssertNoMessageErrorContaining(invoiceLine.JI_CustomsSecondQuantityInfo, "Second Customs Qty must be zero.");

			invoiceLine.JI_CustomsSecondQuantity = 0;
			AssertHasMessageErrorContaining(invoiceLine.JI_CustomsSecondQuantityInfo, CannotBeZeroIfUQIsNotEmptyErrorMsg);

			invoiceLine.JI_CustomsSecondQuantity = -1;
			AssertHasMessageErrorContaining(invoiceLine.JI_CustomsSecondQuantityInfo, MandatoryValidation.ValueCannotBeNegative);
			AssertCustomsQuantity(invoiceLine, "JI_CustomsSecondQuantity", "JI_CustomsSecondUnitQty", "JI_CustomsSecondQuantityInfo");
		}

		public void TestCheckJI_BrandName()
		{
			invoiceLine.JI_BrandName = "";
			AssertHasMessageErrorContaining(invoiceLine.JI_BrandNameInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_BrandName = "CHRISTIAN DIOR";
			AssertNoMessageErrorContaining(invoiceLine.JI_BrandNameInfo, IMPJobComInvoiceLineValidation.BrandNameCheckError);

			invoiceLine.JI_BrandName = @"CHRIST
IANDIOR";
			AssertHasMessageErrorContaining(invoiceLine.JI_BrandNameInfo, IMPJobComInvoiceLineValidation.BrandNameCheckError);

			invoiceLine.JI_BrandName = "CHRISTIAN\tDIOR";
			AssertHasMessageErrorContaining(invoiceLine.JI_BrandNameInfo, IMPJobComInvoiceLineValidation.BrandNameCheckError);

			invoiceLine.JI_BrandName = "CHRISTIAN@DIOR";
			AssertHasMessageErrorContaining(invoiceLine.JI_BrandNameInfo, IMPJobComInvoiceLineValidation.BrandNameCheckError);

			invoiceLine.JI_BrandName = "CHRISTIANDIOR";
			AssertNoMessageErrorContaining(invoiceLine.JI_BrandNameInfo, IMPJobComInvoiceLineValidation.BrandNameCheckError);

			invoiceLine.JI_BrandName = "christiandior";
			AssertNoMessageErrorContaining(invoiceLine.JI_BrandNameInfo, IMPJobComInvoiceLineValidation.BrandNameCheckError);

			invoiceLine.JI_BrandName = "CHRISTIANDIORV1";
			AssertNoMessageErrorContaining(invoiceLine.JI_BrandNameInfo, IMPJobComInvoiceLineValidation.BrandNameCheckError);

			invoiceLine.JI_BrandName = "1234567890";
			AssertNoMessageErrorContaining(invoiceLine.JI_BrandNameInfo, IMPJobComInvoiceLineValidation.BrandNameCheckError);
		}

		public void TestCheckJI_Model()
		{
			invoiceLine.JI_Model = "";
			AssertHasMessageErrorContaining(invoiceLine.JI_ModelInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_Model = "Test_Model";
			AssertNoMessageErrorContaining(invoiceLine.JI_ModelInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckQuantityUnit()
		{
			declaration.ValidationMode = ValidationModes.ExtendReExport;
			invoiceLine.Validation.ValidateJI_InvoiceUQ();
			AssertHasMessageErrorContaining(invoiceLine.JI_InvoiceUQInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.ValidationMode = ValidationModes.None;
			invoiceLine.Validation.ValidateJI_InvoiceUQ();
			AssertNoMessageErrorContaining(invoiceLine.JI_InvoiceUQInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_InvoiceUQ = "-1";
			AssertHasMessageErrorContaining(invoiceLine.JI_InvoiceUQInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.JI_InvoiceUQ = InvoiceUnitQuantityCodeList.Codes.BO;
			AssertNoMessageErrors(invoiceLine.JI_InvoiceUQInfo);
		}

		public void TestCheckQuantity()
		{
			var validation = (IMPJobComInvoiceLineValidation)invoiceLine.Validation;

			declaration.ValidationMode = ValidationModes.ExtendReExport;
			validation.ValidateJI_InvoiceQuantity();
			AssertHasMessageErrorContaining(invoiceLine.JI_InvoiceQuantityInfo, MandatoryValidation.ValueCannotBeZero);

			Assert929Validation();

			declaration.ValidationMode = ValidationModes.None;
			invoiceLine.JI_InvoiceQuantity = 0m;
			AssertNoMessageErrorContaining(invoiceLine.JI_InvoiceQuantityInfo, MandatoryValidation.ValueCannotBeZero);

			Assert929Validation();

			void Assert929Validation()
			{
				invoiceLine.JI_LinePrice = 10m;
				validation.ValidateJI_InvoiceQuantity();
				AssertHasMessageErrorContaining(invoiceLine.JI_InvoiceQuantityInfo, "If the amount is greater than 0, the quantity must also be greater than 0.");

				invoiceLine.JI_InvoiceQuantity = -10m;
				AssertHasMessageErrorContaining(invoiceLine.JI_InvoiceQuantityInfo, MandatoryValidation.ValueCannotBeNegative);

				invoiceLine.JI_InvoiceQuantity = 10m;
				AssertHasMessageErrorContaining(invoiceLine.JI_InvoiceQuantityInfo, "You have entered an invoice quantity. Please enter its unit too.");
			}
		}

		public void TestCertificateOfOriginUQ()
		{
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var validation = (IMPJobComInvoiceLineValidation)invoiceLine.Validation;

			AssertNoMessageErrors(invoiceLine.CertificateOfOriginUQInfo);

			declaration.SetValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._DHR);
			validation.ValidateCertificateOfOriginUQ();
			AssertHasMessageErrorContaining(invoiceLine.CertificateOfOriginUQInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.CertificateOfOriginUQ = "12";
			AssertHasMessageErrorContaining(invoiceLine.CertificateOfOriginUQInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.CertificateOfOriginUQ = "EA";
			AssertNoMessageErrors(invoiceLine.CertificateOfOriginUQInfo);
		}

		public void TestCertificateOfOriginUsedQuantity()
		{
			var validation = (IMPJobComInvoiceLineValidation)invoiceLine.Validation;

			invoiceLine.JI_CustomsFifthQuantity = -1m;
			AssertNoMessageErrors(invoiceLine.JI_CustomsFifthQuantityInfo);

			declaration.SetValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._DHR);
			validation.ValidateJI_CustomsFifthQuantity();

			invoiceLine.JI_CustomsFifthQuantity = -1;
			AssertHasMessageErrorContaining(invoiceLine.JI_CustomsFifthQuantityInfo, "Please enter a 'C/O Used Quantity' greater than 0.");

			invoiceLine.JI_CustomsFifthQuantity = 0;
			AssertHasMessageErrorContaining(invoiceLine.JI_CustomsFifthQuantityInfo, "Please enter a 'C/O Used Quantity' greater than 0.");

			invoiceLine.JI_CustomsFifthQuantity = 1;
			AssertNoMessageErrors(invoiceLine.JI_CustomsFifthQuantityInfo);
		}

		public void TestCheckUnitPrice()
		{
			var validation = (IMPJobComInvoiceLineValidation)invoiceLine.Validation;

			declaration.ValidationMode = ValidationModes.ExtendReExport;
			validation.ValidateUnitPrice();
			AssertHasMessageErrorContaining(invoiceLine.UnitPriceInfo, MandatoryValidation.ValueCannotBeZero);

			invoiceLine.JI_LinePrice = 10m;
			validation.ValidateUnitPrice();
			AssertHasMessageErrorContaining(invoiceLine.UnitPriceInfo, "If the amount is greater than 0, the 'Unit Price' must also be greater than 0.");

			invoiceLine.UnitPrice = -10m;
			AssertHasMessageErrorContaining(invoiceLine.UnitPriceInfo, MandatoryValidation.ValueCannotBeNegative);

			declaration.ValidationMode = ValidationModes.None;

			invoiceLine.Validation.ValidateUnitPrice();
			AssertHasMessageErrorContaining(invoiceLine.UnitPriceInfo, MandatoryValidation.ValueCannotBeNegative);

			invoiceLine.JI_LinePrice = 10m;
			invoiceLine.UnitPrice = ZDecimal.Zero;
			AssertHasMessageErrorContaining(invoiceLine.UnitPriceInfo, "If the amount is greater than 0, the 'Unit Price' must also be greater than 0.");
			AssertNoMessageErrorContaining(invoiceLine.UnitPriceInfo, MandatoryValidation.ValueCannotBeZero);
		}

		public void TestCheckLinePrice()
		{
			var validation = (IMPJobComInvoiceLineValidation)invoiceLine.Validation;

			declaration.ValidationMode = ValidationModes.ExtendReExport;
			validation.ValidateJI_LinePrice();
			AssertHasMessageErrorContaining(invoiceLine.JI_LinePriceInfo, MandatoryValidation.ValueCannotBeZero);

			invoiceLine.JI_LinePrice = -1;
			AssertHasMessageErrorContaining(invoiceLine.JI_LinePriceInfo, MandatoryValidation.ValueCannotBeNegative);

			declaration.ValidationMode = ValidationModes.None;
			invoiceLine.JI_LinePrice = 0m;
			AssertNoMessageErrors(invoiceLine.JI_LinePriceInfo);

			invoiceLine.JI_LinePrice = -1;
			AssertHasMessageErrorContaining(invoiceLine.JI_LinePriceInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		public void TestCertificateOfOriginNo()
		{
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var validation = (IMPJobComInvoiceLineValidation)invoiceLine.Validation;

			invoiceLine.CertificateOfOriginNo = "ABC";
			AssertHasMessageErrorContaining(invoiceLine.CertificateOfOriginNoInfo, IMPJobComInvoiceLineValidation.ReferenceNumberLengthError);

			invoiceLine.CertificateOfOriginNo = "CERNO";
			AssertNoMessageErrors(invoiceLine.CertificateOfOriginNoInfo);

			invoiceLine.CertificateOfOriginNo = "";
			AssertNoMessageErrorContaining(invoiceLine.CertificateOfOriginNoInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.SetValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._DHR);
			validation.ValidateCertificateOfOriginNo();
			AssertHasMessageErrorContaining(invoiceLine.CertificateOfOriginNoInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCertificateOfOriginLineNo()
		{
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var validation = (IMPJobComInvoiceLineValidation)invoiceLine.Validation;

			invoiceLine.CertificateOfOriginLineNo = ZInt.Zero;
			AssertNoMessageErrorContaining(invoiceLine.CertificateOfOriginLineNoInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.SetValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._DHR);
			validation.ValidateCertificateOfOriginLineNo();
			AssertHasMessageErrorContaining(invoiceLine.CertificateOfOriginLineNoInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCertificateOfOriginCriteriaCode()
		{
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var validation = (IMPJobComInvoiceLineValidation)invoiceLine.Validation;

			invoiceLine.CertificateOfOriginCriteriaCode = "0";
			AssertHasMessageErrorContaining(invoiceLine.CertificateOfOriginCriteriaCodeInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.CertificateOfOriginCriteriaCode = CountryOfOriginDeterminationRuleCodeList.Codes.A;
			AssertHasMessageErrorContaining(invoiceLine.CertificateOfOriginCriteriaCodeInfo, MandatoryValidation.DoNotEntered);

			invoiceLine.CertificateOfOriginNo = "CERNO";
			validation.ValidateCertificateOfOriginCriteriaCode();
			AssertNoMessageErrors(invoiceLine.CertificateOfOriginCriteriaCodeInfo);
		}

		[TestDate(2023, 10, 12)]
		public void TestCertificateOfOriginIssueDate()
		{
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var validation = (IMPJobComInvoiceLineValidation)invoiceLine.Validation;

			invoiceLine.CertificateOfOriginIssueDate = ZDateTime.Today.AddDays(3);
			AssertHasMessageErrorContaining(invoiceLine.CertificateOfOriginIssueDateInfo, MandatoryValidation.DoNotEntered);

			invoiceLine.CertificateOfOriginNo = "CERNO";
			validation.ValidateCertificateOfOriginIssueDate();
			AssertHasMessageErrorContaining(invoiceLine.CertificateOfOriginIssueDateInfo, IMPJobComInvoiceLineValidation.DeclarationDateError);

			invoiceLine.CertificateOfOriginIssueDate = ZDateTime.Today.AddDays(-1);
			AssertNoMessageErrors(invoiceLine.CertificateOfOriginIssueDateInfo);

			invoiceLine.CertificateOfOriginIssueDate = ZDateTime.Today;
			AssertNoMessageErrors(invoiceLine.CertificateOfOriginIssueDateInfo);

			#region entry
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = "IMP";
			entry.EntryNumber = "1163921400096U";

			var entryNum = entry.CusEntryNumber;
			entryNum.CE_IssueDate = ZDateTime.Today.AddDays(-3);
			#endregion

			#region entryLine
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			entryLine.CL_AdValoremTariff = "8523491020";
			entryLine.CL_CustomsValue = 26317636m;
			invoiceLine.JI_CL = entryLine.PK;
			#endregion

			validation.ValidateCertificateOfOriginIssueDate();
			AssertHasMessageErrorContaining(invoiceLine.CertificateOfOriginIssueDateInfo, IMPJobComInvoiceLineValidation.DeclarationDateError);
		}

		public void TestCertificateOfOriginIssuingCountry()
		{
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var validation = (IMPJobComInvoiceLineValidation)invoiceLine.Validation;

			invoiceLine.CertificateOfOriginIssuingCountry = "XX";
			AssertHasMessageErrorContaining(invoiceLine.CertificateOfOriginIssuingCountryInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.CertificateOfOriginIssuingCountry = "KR";
			AssertHasMessageErrorContaining(invoiceLine.CertificateOfOriginIssuingCountryInfo, MandatoryValidation.DoNotEntered);

			invoiceLine.CertificateOfOriginNo = "CERNO";
			validation.ValidateCertificateOfOriginIssuingCountry();
			AssertNoMessageErrors(invoiceLine.CertificateOfOriginIssuingCountryInfo);
		}

		public void TestCertificateOfOriginAgencyName()
		{
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var validation = (IMPJobComInvoiceLineValidation)invoiceLine.Validation;

			invoiceLine.CertificateOfOriginAgencyName = "Agency Name";
			AssertHasMessageErrorContaining(invoiceLine.CertificateOfOriginAgencyNameInfo, MandatoryValidation.DoNotEntered);

			invoiceLine.CertificateOfOriginNo = "CERNO";
			validation.ValidateCertificateOfOriginAgencyName();
			AssertNoMessageErrors(invoiceLine.CertificateOfOriginAgencyNameInfo);
		}

		public void TestCertificateOfOriginAreaName()
		{
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var validation = (IMPJobComInvoiceLineValidation)invoiceLine.Validation;

			invoiceLine.CertificateOfOriginAreaName = "Area Name";
			AssertHasMessageErrorContaining(invoiceLine.CertificateOfOriginAreaNameInfo, MandatoryValidation.DoNotEntered);

			invoiceLine.CertificateOfOriginNo = "CERNO";
			validation.ValidateCertificateOfOriginAreaName();
			AssertNoMessageErrors(invoiceLine.CertificateOfOriginAreaNameInfo);
		}

		public void TestCertificateOfOriginPersonName()
		{
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var validation = (IMPJobComInvoiceLineValidation)invoiceLine.Validation;

			invoiceLine.CertificateOfOriginPersonName = "Person Name";
			AssertHasMessageErrorContaining(invoiceLine.CertificateOfOriginPersonNameInfo, MandatoryValidation.DoNotEntered);

			invoiceLine.CertificateOfOriginNo = "CERNO";
			validation.ValidateCertificateOfOriginPersonName();
			AssertNoMessageErrors(invoiceLine.CertificateOfOriginPersonNameInfo);
		}

		public void TestCertificateOfOriginStatus()
		{
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var validation = (IMPJobComInvoiceLineValidation)invoiceLine.Validation;

			invoiceLine.CertificateOfOriginStatus = CertificateOfOriginSplitCodeList.Codes.Y;
			AssertHasMessageErrorContaining(invoiceLine.CertificateOfOriginStatusInfo, MandatoryValidation.DoNotEntered);

			invoiceLine.CertificateOfOriginNo = "CERNO";
			validation.ValidateCertificateOfOriginStatus();
			AssertNoMessageErrors(invoiceLine.CertificateOfOriginStatusInfo);
		}

		public void TestCriteriaForDeterminingCountryOfOrigin()
		{
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			invoiceLine.CriteriaForDeterminingCountryOfOrigin = "";
			AssertHasMessageErrorContaining(invoiceLine.CriteriaForDeterminingCountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.CriteriaForDeterminingCountryOfOrigin = "X";
			AssertHasMessageErrorContaining(invoiceLine.CriteriaForDeterminingCountryOfOriginInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.CriteriaForDeterminingCountryOfOrigin = CountryOfOriginDeterminationRuleCodeList.Codes.A;
			AssertNoMessageErrors(invoiceLine.CriteriaForDeterminingCountryOfOriginInfo);
		}

		public void TestCheckJI_ParentLine()
		{
			invoiceLine.JI_ParentLine = 1;
			AssertHasMessageErrorContaining(invoiceLine.JI_ParentLineInfo, "Parent Line must be zero.");

			invoiceLine.JI_ParentLine = -1;
			AssertHasMessageErrorContaining(invoiceLine.JI_ParentLineInfo, MandatoryValidation.ValueCannotBeNegative);

			invoiceLine.JI_ParentLine = 0;
			AssertNoMessageErrors(invoiceLine.JI_ParentLineInfo);

			invoiceLine.JI_ProductTypeCode = "B";
			CombineAssertions("Mandatory if Declaration Procedure Type is '29' or '36' and JI_ProductTypeCode is 'B'.", () =>
			{
				declaration.JE_ProcedureType = ImportKindCodeList.Codes._29;
				invoiceLine.JI_ParentLine = 0;
				AssertHasMessageErrorContaining(invoiceLine.JI_ParentLineInfo, MandatoryValidation.ValueCannotBeZero);

				invoiceLine.JI_ParentLine = 1;
				AssertNoMessageErrors(invoiceLine.JI_ParentLineInfo);

				declaration.JE_ProcedureType = ImportKindCodeList.Codes._36;
				invoiceLine.JI_ParentLine = 0;
				AssertHasMessageErrorContaining(invoiceLine.JI_ParentLineInfo, MandatoryValidation.ValueCannotBeZero);

				invoiceLine.JI_ParentLine = 1;
				AssertNoMessageErrors(invoiceLine.JI_ParentLineInfo);

				declaration.JE_ProcedureType = ImportKindCodeList.Codes._11;
				invoiceLine.JI_ParentLine = 0;
				AssertNoMessageErrors(invoiceLine.JI_ParentLineInfo);

				invoiceLine.JI_ParentLine = 1;
				AssertHasMessageErrorContaining(invoiceLine.JI_ParentLineInfo, "Parent Line must be zero.");
			});
		}

		public void TestCheckJI_NetWeight()
		{
			invoiceLine.JI_NetWeight = 0;
			AssertNoMessageErrors(invoiceLine.JI_NetWeightInfo);

			invoiceLine.JI_NetWeight = -1;
			AssertHasMessageErrorContaining(invoiceLine.JI_NetWeightInfo, MandatoryValidation.ValueCannotBeNegative);

			invoiceLine.JI_NetWeight = 1;
			AssertNoMessageErrors(invoiceLine.JI_NetWeightInfo);
		}

		public void TestCheckJI_CustomsQuantity()
		{
			invoiceLine.JI_CustomsQuantity = 0;
			AssertNoMessageErrors(invoiceLine.JI_CustomsQuantityInfo);

			invoiceLine.JI_CustomsQuantity = -1;
			AssertHasMessageErrorContaining(invoiceLine.JI_CustomsQuantityInfo, MandatoryValidation.ValueCannotBeNegative);

			invoiceLine.JI_CustomsQuantity = 1;
			AssertHasMessageErrorContaining(invoiceLine.JI_CustomsQuantityInfo, "Customs Quantity must be zero.");

			invoiceLine.JI_CustomsUnitQty = "BAG";
			invoiceLine.JI_CustomsQuantity = 1;
			AssertNoMessageErrors(invoiceLine.JI_CustomsQuantityInfo);

			invoiceLine.JI_CustomsQuantity = -1;
			AssertHasMessageErrorContaining(invoiceLine.JI_CustomsQuantityInfo, MandatoryValidation.ValueCannotBeNegative);

			invoiceLine.JI_CustomsQuantity = 0;
			AssertHasMessageErrorContaining(invoiceLine.JI_CustomsQuantityInfo, CannotBeZeroIfUQIsNotEmptyErrorMsg);
		}

		public void TestJI_CustomsQuantity()
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			AssertCustomsQuantity(invoiceLine, "JI_CustomsQuantity", "JI_CustomsUnitQty", "JI_CustomsQuantityInfo");
		}

		public void TestJI_CustomsThirdQuantity()
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			AssertCustomsQuantity(invoiceLine, "JI_CustomsThirdQuantity", "JI_CustomsThirdUnitQty", "JI_CustomsThirdQuantityInfo");
		}

		public void TestJI_CustomsFourthQuantity()
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			AssertCustomsQuantity(invoiceLine, "JI_CustomsFourthQuantity", "JI_CustomsFourthUnitQty", "JI_CustomsFourthQuantityInfo");
		}

		void AssertCustomsQuantity(JobComInvoiceLine invoiceLine, string quantity, string unitQty, string info)
		{
			invoiceLine.SetPropertyValue(unitQty, ZString.Empty);
			invoiceLine.SetPropertyValue(quantity, (ZDecimal)0m);
			AssertNoMessageErrorContaining((ZPropertyInfo)invoiceLine.GetPropertyValue(info), CannotBeZeroIfUQIsNotEmptyErrorMsg);

			invoiceLine.SetPropertyValue(quantity, (ZDecimal)(-1m));
			AssertHasMessageErrorContaining((ZPropertyInfo)invoiceLine.GetPropertyValue(info), MandatoryValidation.ValueCannotBeNegative);

			invoiceLine.SetPropertyValue(unitQty, (ZString)DomesticTaxBaseQtyOrPriceCode.UQs.AlcoholContent);
			invoiceLine.SetPropertyValue(quantity, (ZDecimal)0m);
			AssertHasMessageErrorContaining((ZPropertyInfo)invoiceLine.GetPropertyValue(info), CannotBeZeroIfUQIsNotEmptyErrorMsg);
		}

		public void TestCheckJI_CountryOfOrigin()
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.Validation.ValidateJI_CountryOfOrigin();
			AssertHasMessageErrorContaining(invoiceLine.JI_CountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_CountryOfOrigin = "12";
			AssertHasMessageErrorContaining(invoiceLine.JI_CountryOfOriginInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Albania;
			declaration.JE_TradeIndicatorWithKP = "1";
			invoiceLine.Validation.ValidateJI_CountryOfOrigin();
			AssertHasMessageErrorContaining(invoiceLine.JI_CountryOfOriginInfo, "if Country Of Origin is not 'KR' or 'KP', then South North Trade Y/N must be empty.");

			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.KoreaSouth;
			AssertNoMessageErrors(invoiceLine.JI_CountryOfOriginInfo);

			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.KoreaNorth;
			AssertNoMessageErrors(invoiceLine.JI_CountryOfOriginInfo);
		}

		public void TestCheckJI_PrimaryPreference()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreatePreferenceForCountry("FEU1", "FEU1", Core.Constants.CountryCodes.KoreaSouth);
			helper.CreatePreferenceForCountry("FCN1", "FCN1", Core.Constants.CountryCodes.KoreaSouth);
			helper.CreateCusCodeType(Messaging.Constants.ZZ.NKCodeType.KRFTE, "FTA codes eligible for COO Certification Exporter system", Core.Constants.CountryCodes.KoreaSouth, 6);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Messaging.Constants.ZZ.NKCodeType.KRFTE, "FCN", "Korea-China FTA", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(2));
			Factory.Save();
			var codes = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.KRFTE, ZDateTime.Today);

			invoiceLine.Validation.ValidateJI_PrimaryPreference();
			AssertHasMessageErrorContaining(invoiceLine.JI_PrimaryPreferenceInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_PrimaryPreference = "X";
			AssertHasMessageErrorContaining(invoiceLine.JI_PrimaryPreferenceInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.JI_PrimaryPreference = "FEU1";
			AssertNoMessageErrors(invoiceLine.JI_PrimaryPreferenceInfo);

			invoiceLine.JI_CoveredByCOOExporter = true;
			invoiceLine.Validation.ValidateJI_PrimaryPreference();
			AssertHasMessageErrorContaining(invoiceLine.JI_PrimaryPreferenceInfo, "The entered FTA is not covered by the COO Certification Exporter system");

			invoiceLine.JI_PrimaryPreference = "FCN1";
			AssertNoMessageErrorContaining(invoiceLine.JI_PrimaryPreferenceInfo, "The entered FTA is not covered by the COO Certification Exporter system");
		}

		public void TestCheckJI_SecondaryPreference()
		{
			var tariffDRE1 = "A088000101";
			var tariffDRE2 = "A099000001";

			var helper = new UniversalReferenceTestDataHelper(Factory);

			var tariffType_DRE = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, ZZ.TariffTypes.DutyReductionExemption);

			var tariff_DRE1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType_DRE.PK, tariffDRE1, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "관세법 제99조제1호 해당물품");

			var tariff_DRE2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType_DRE.PK, tariffDRE2, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "관세법 제99조제1호 해당물품");
			helper.CreateNewOrGetExistingTariffAttribute(Constants.ZZ.TariffAttributes.ReImport, Constants.YesNo.Yes, tariff_DRE2);

			Factory.Save();

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			invoiceLine.JI_SecondaryPreference = "123456";
			AssertHasMessageErrorContaining(invoiceLine.JI_SecondaryPreferenceInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.JI_SecondaryPreference = tariffDRE2;
			AssertEquals(0, invoiceLine.PreviousExpDecLineCollection.Count);
			invoiceLine.Validation.ValidateJI_SecondaryPreference();
			AssertHasMessageErrorContaining(invoiceLine.JI_SecondaryPreferenceInfo, "The entered duty reduction/exemption code is for re-Import of previously exported goods. You should enter re-Imported goods information.");

			invoiceLine.JI_SecondaryPreference = tariffDRE1;
			AssertNoMessageErrors(invoiceLine.JI_SecondaryPreferenceInfo);

			invoiceLine.PreviousExpDecLineCollection.AddNew();
			invoiceLine.Validation.ValidateJI_SecondaryPreference();
			AssertHasMessageErrorContaining(invoiceLine.JI_SecondaryPreferenceInfo, "The entered duty reduction/exemption code is not relevant for re-Import of previously exported goods. You should not enter re-Imported goods information.");

			invoiceLine.JI_SecondaryPreference = tariffDRE2;
			AssertNoMessageErrors(invoiceLine.JI_SecondaryPreferenceInfo);

			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._11;
			invoiceLine.Validation.ValidateJI_SecondaryPreference();
			AssertNoMessageErrors(invoiceLine.JI_SecondaryPreferenceInfo);

			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._12;
			invoiceLine.Validation.ValidateJI_SecondaryPreference();
			AssertHasMessageErrorContaining(invoiceLine.JI_SecondaryPreferenceInfo, MandatoryValidation.DoNotEntered);

			invoiceLine.JI_SecondaryPreference = ZString.Empty;
			AssertNoMessageError(invoiceLine.JI_SecondaryPreferenceInfo, MandatoryValidation.DoNotEntered);

			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._11;
			declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._01;
			invoiceLine.JI_SecondaryPreference = tariffDRE2;
			AssertNoMessageErrors(invoiceLine.JI_SecondaryPreferenceInfo);

			declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._33;
			invoiceLine.Validation.ValidateJI_SecondaryPreference();
			AssertHasMessageErrorContaining(invoiceLine.JI_SecondaryPreferenceInfo, MandatoryValidation.DoNotEntered);

			invoiceLine.JI_SecondaryPreference = ZString.Empty;
			AssertNoMessageError(invoiceLine.JI_SecondaryPreferenceInfo, MandatoryValidation.DoNotEntered);
		}

		public void TestCheckJI_Tariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem);
			var condtionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.KoreaSouth, "CTRL", "OGA");
			var regulationNumber = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.CountryCodes.KoreaSouth, ZZ.RefCusConditionValueType.OGARegulationNumber, "OGA Regulation Number");
			var documentName = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.CountryCodes.KoreaSouth, ZZ.RefCusConditionValueType.OGADocumentName, "OGA Document Name");

			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "8429521021", new ZDateTime("2024-01-01"), new ZDateTime("2024-12-31"), "Not Exist OGA RefCusCondition");

			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "8429521022", new ZDateTime("2024-01-01"), new ZDateTime("2024-12-31"), "Exist 1 OGA RefCusCondition");
			TestRefConditionSetupHelper.AddRefCusCondition(tariff2, "13", "수입식품안전관리 특별법", condtionType, regulationNumber, documentName, false, true, new ZDateTime("2024-01-01"), new ZDateTime("2024-04-30"));
			TestRefConditionSetupHelper.AddRefCusCondition(tariff2, "15", "수출 OGA", condtionType, regulationNumber, documentName, true, false, new ZDateTime("2024-01-01"), new ZDateTime("2024-04-30"));

			var tariff3 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "8429521023", new ZDateTime("2024-01-01"), new ZDateTime("2024-12-31"), "Exist 2 OGA RefCusCondition");
			TestRefConditionSetupHelper.AddRefCusCondition(tariff3, "71", "야생생물 보호 및 관리에 관한 법률", condtionType, regulationNumber, documentName, false, true, new ZDateTime("2024-01-01"), new ZDateTime("2024-04-30"));
			TestRefConditionSetupHelper.AddRefCusCondition(tariff3, "89", "가축전염병 예방법", condtionType, regulationNumber, documentName, false, true, new ZDateTime("2024-01-01"), new ZDateTime("2024-04-30"));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(KRJobMessageTypeList.Codes.Import);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			Factory.Save();

			CombineAssertions("When declaration date is 2024-05-01", () =>
			{
				entryNum.CE_IssueDate = new ZDateTime("2024-05-01");
				invoiceLine.JI_Tariff = ZString.Empty;
				AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "Tariff may not be empty");

				invoiceLine.JI_Tariff = tariff1.ZZ1_TariffCode;
				AssertNoMessageErrors(invoiceLine.JI_TariffInfo);

				invoiceLine.JI_Tariff = tariff2.ZZ1_TariffCode;
				AssertNoMessageErrors(invoiceLine.JI_TariffInfo);

				invoiceLine.JI_Tariff = tariff3.ZZ1_TariffCode;
				AssertNoMessageErrors(invoiceLine.JI_TariffInfo);
			});

			CombineAssertions("When declaration date is 2024-04-30", () =>
			{
				entryNum.CE_IssueDate = new ZDateTime("2024-04-30");
				invoiceLine.JI_Tariff = ZString.Empty;
				AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "Tariff may not be empty");

				invoiceLine.JI_Tariff = tariff1.ZZ1_TariffCode;
				AssertNoMessageErrors(invoiceLine.JI_TariffInfo);

				invoiceLine.JI_Tariff = tariff2.ZZ1_TariffCode;
				AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "This tariff has OGA requirements for the regulation 13 but the details are not in Approval & Re-Import > Approval Document or Non-Approval Document.");
				invoiceLine.NonGADetailCollection.AddNew().CSI_Procedure = "13";
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertNoMessageErrorContaining(invoiceLine.JI_TariffInfo, "This tariff has OGA requirements for the regulation 13 but the details are not in Approval & Re-Import > Approval Document or Non-Approval Document.");
				invoiceLine.NonGADetailCollection.RemoveAndDeleteAll();

				invoiceLine.Validation.ValidateJI_Tariff();
				AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "This tariff has OGA requirements for the regulation 13 but the details are not in Approval & Re-Import > Approval Document or Non-Approval Document.");
				invoiceLine.GAApprovalDataCollection.AddNew().CSI_Procedure = "13";
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertNoMessageErrorContaining(invoiceLine.JI_TariffInfo, "This tariff has OGA requirements for the regulation 13 but the details are not in Approval & Re-Import > Approval Document or Non-Approval Document.");
				invoiceLine.GAApprovalDataCollection.RemoveAndDeleteAll();

				invoiceLine.JI_Tariff = tariff3.ZZ1_TariffCode;
				AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "This tariff has OGA requirements for the regulations 71, 89 but the details are not in Approval & Re-Import > Approval Document or Non-Approval Document.");

				invoiceLine.NonGADetailCollection.AddNew().CSI_Procedure = "71";
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "This tariff has OGA requirements for the regulation 89 but the details are not in Approval & Re-Import > Approval Document or Non-Approval Document.");

				invoiceLine.GAApprovalDataCollection.AddNew().CSI_Procedure = "89";
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertNoMessageErrorContaining(invoiceLine.JI_TariffInfo, "This tariff has OGA requirements for the regulations 71, 89 but the details are not in Approval & Re-Import > Approval Document or Non-Approval Document.");
			});
		}

		public void TestCheckJI_Description()
		{
			var messageError = "You must enter either 'Goods Description' or 'Ingredient'.";

			invoiceLine.Validation.ValidateJI_Description();
			AssertHasMessageErrorContaining(invoiceLine.JI_DescriptionInfo, messageError);
			AssertNoMessageErrorContaining(invoiceLine.JI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.ValidationMode = ValidationModes.ExtendReExport;
			invoiceLine.Validation.ValidateJI_Description();
			AssertHasMessageErrorContaining(invoiceLine.JI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(invoiceLine.JI_DescriptionInfo, messageError);

			invoiceLine.JI_Description = "test";
			AssertNoMessageErrors(invoiceLine.JI_DescriptionInfo);

			declaration.ValidationMode = ValidationModes.Import;

			invoiceLine.JI_Ingredient = "";
			invoiceLine.JI_Description = "Test_Description";
			AssertNoMessageErrorContaining(invoiceLine.JI_DescriptionInfo, messageError);

			invoiceLine.JI_Ingredient = "Test_Ingredient";
			invoiceLine.JI_Description = "";
			AssertNoMessageErrorContaining(invoiceLine.JI_DescriptionInfo, messageError);
		}

		public void TestCheckJI_ProductTypeCode()
		{
			CombineAssertions("Must be empty if Declaration Procedure Type is not '29' or '36'.", () =>
			{
				AssertEquals(ZString.Empty, declaration.JE_ProcedureType);

				invoiceLine.JI_ProductTypeCode = ZString.Empty;
				AssertNoMessageErrors(invoiceLine.JI_ProductTypeCodeInfo);

				invoiceLine.JI_ProductTypeCode = ProductOrMaterialCodeList.Codes.A;
				AssertHasMessageErrorContaining(invoiceLine.JI_ProductTypeCodeInfo, MandatoryValidation.DoNotEntered);
			});

			CombineAssertions("Mandatory if Declaration Procedure Type is '29' or '36'.", () =>
			{
				declaration.JE_ProcedureType = ImportKindCodeList.Codes._29;
				AssertJI_ProductTypeCode();

				declaration.JE_ProcedureType = ImportKindCodeList.Codes._36;
				AssertJI_ProductTypeCode();
			});

			void AssertJI_ProductTypeCode()
			{
				invoiceLine.JI_ProductTypeCode = ZString.Empty;
				AssertHasMessageErrorContaining(invoiceLine.JI_ProductTypeCodeInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine.JI_ProductTypeCode = "X";
				AssertHasMessageErrorContaining(invoiceLine.JI_ProductTypeCodeInfo, ListValidation.InvalidCodeMessageError);

				invoiceLine.JI_ProductTypeCode = ProductOrMaterialCodeList.Codes.A;
				AssertNoMessageErrors(invoiceLine.JI_ProductTypeCodeInfo);
			}
		}

		public void TestCheckJI_DrawbackUQ()
		{
			invoiceLine.JI_DrawbackQuantity = 0;
			invoiceLine.JI_DrawbackUQ = ZString.Empty;
			AssertNoMessageErrors(invoiceLine.JI_DrawbackUQInfo);

			invoiceLine.JI_DrawbackUQ = InvoiceUnitQuantityCodeList.Codes.BAG;
			AssertHasMessageErrorContaining(invoiceLine.JI_DrawbackUQInfo, MandatoryValidation.DoNotEntered);

			invoiceLine.JI_DrawbackUQ = "XX";
			AssertHasMessageErrorContaining(invoiceLine.JI_DrawbackUQInfo, MandatoryValidation.DoNotEntered);

			invoiceLine.JI_DrawbackQuantity = 1;
			invoiceLine.JI_DrawbackUQ = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.JI_DrawbackUQInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_DrawbackUQ = InvoiceUnitQuantityCodeList.Codes.BAG;
			AssertNoMessageErrors(invoiceLine.JI_DrawbackUQInfo);

			invoiceLine.JI_DrawbackUQ = "XX";
			AssertHasMessageErrorContaining(invoiceLine.JI_DrawbackUQInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJI_DrawbackQuantity()
		{
			invoiceLine.JI_DrawbackUQ = ZString.Empty;
			invoiceLine.JI_DrawbackQuantity = 0;
			AssertNoMessageErrors(invoiceLine.JI_DrawbackQuantityInfo);

			invoiceLine.JI_DrawbackQuantity = 1;
			AssertHasMessageErrorContaining(invoiceLine.JI_DrawbackQuantityInfo, "Drawback Quantity must be zero.");

			invoiceLine.JI_DrawbackQuantity = -1;
			AssertHasMessageErrorContaining(invoiceLine.JI_DrawbackQuantityInfo, "Drawback Quantity must be zero.");
			AssertHasMessageErrorContaining(invoiceLine.JI_DrawbackQuantityInfo, MandatoryValidation.ValueCannotBeNegative);

			invoiceLine.JI_DrawbackUQ = InvoiceUnitQuantityCodeList.Codes.BAG;
			invoiceLine.JI_DrawbackQuantity = 0;
			AssertHasMessageErrorContaining(invoiceLine.JI_DrawbackQuantityInfo, MandatoryValidation.ValueCannotBeZero);

			invoiceLine.JI_DrawbackQuantity = 1;
			AssertNoMessageErrors(invoiceLine.JI_DrawbackQuantityInfo);

			invoiceLine.JI_DrawbackQuantity = -1;
			AssertHasMessageErrorContaining(invoiceLine.JI_DrawbackQuantityInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		public void TestCheckJI_MightRequireInspection()
		{
			var declarationRef = declaration.DeclarationRefs.AddNew();
			declarationRef.J3_ReferenceType = AdditionalInformationStatementCodes_929._259;
			declarationRef.J3_ReferenceNumber = "";
			invoiceLine.JI_MightRequireInspection = ZString.Empty;
			AssertNoMessageErrors(invoiceLine.JI_MightRequireInspectionInfo);

			invoiceLine.JI_MightRequireInspection = MightRequireInspectionIndicatorCodeList.Codes.Y;
			AssertHasMessageErrorContaining(invoiceLine.JI_MightRequireInspectionInfo, MandatoryValidation.DoNotEntered);

			invoiceLine.JI_MightRequireInspection = MightRequireInspectionIndicatorCodeList.Codes.N;
			AssertHasMessageErrorContaining(invoiceLine.JI_MightRequireInspectionInfo, MandatoryValidation.DoNotEntered);

			invoiceLine.JI_MightRequireInspection = "X";
			AssertHasMessageErrorContaining(invoiceLine.JI_MightRequireInspectionInfo, MandatoryValidation.DoNotEntered);
			AssertHasMessageErrorContaining(invoiceLine.JI_MightRequireInspectionInfo, ListValidation.InvalidCodeMessageError);

			declarationRef.J3_ReferenceNumber = "B";
			AssertJI_MightRequireInspection();

			declarationRef.J3_ReferenceNumber = "C";
			AssertJI_MightRequireInspection();

			declarationRef.J3_ReferenceNumber = "D";
			AssertJI_MightRequireInspection();

			declarationRef.J3_ReferenceNumber = "E";
			AssertJI_MightRequireInspection();

			void AssertJI_MightRequireInspection()
			{
				invoiceLine.JI_MightRequireInspection = ZString.Empty;
				AssertHasMessageErrorContaining(invoiceLine.JI_MightRequireInspectionInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine.JI_MightRequireInspection = MightRequireInspectionIndicatorCodeList.Codes.Y;
				AssertNoMessageErrors(invoiceLine.JI_MightRequireInspectionInfo);

				invoiceLine.JI_MightRequireInspection = MightRequireInspectionIndicatorCodeList.Codes.N;
				AssertNoMessageErrors(invoiceLine.JI_MightRequireInspectionInfo);

				invoiceLine.JI_MightRequireInspection = "X";
				AssertHasMessageErrorContaining(invoiceLine.JI_MightRequireInspectionInfo, ListValidation.InvalidCodeMessageError);
			}
		}
		public void TestCheckJI_PostClearanceProcedureGA1()
		{
			SetZZData_OGAAA();
			invoiceLine.JI_PostClearanceProcedureGA1 = ZString.Empty;
			AssertNoMessageErrors(invoiceLine.JI_PostClearanceProcedureGA1Info);

			invoiceLine.JI_PostClearanceProcedureGA1 = "001";
			AssertNoMessageErrors(invoiceLine.JI_PostClearanceProcedureGA1Info);

			invoiceLine.JI_PostClearanceProcedureGA1 = "XXX";
			AssertHasMessageErrorContaining(invoiceLine.JI_PostClearanceProcedureGA1Info, ListValidation.InvalidCodeMessageError);
		}
		public void TestCheckJI_PostClearanceProcedureGA2()
		{
			SetZZData_OGAAA();

			AssertEquals(ZString.Empty, invoiceLine.JI_PostClearanceProcedureGA1);
			AssertEquals(ZString.Empty, invoiceLine.JI_PostClearanceProcedureGA2);
			invoiceLine.Validation.ValidateJI_PostClearanceProcedureGA2();
			AssertNoMessageErrors(invoiceLine.JI_PostClearanceProcedureGA2Info);

			invoiceLine.JI_PostClearanceProcedureGA2 = "002";
			AssertHasMessageErrorContaining(invoiceLine.JI_PostClearanceProcedureGA2Info, "You should enter a value here only if you have the first post clearance procedure government agency.");

			invoiceLine.JI_PostClearanceProcedureGA1 = "001";
			invoiceLine.Validation.ValidateJI_PostClearanceProcedureGA2();
			AssertNoMessageErrors(invoiceLine.JI_PostClearanceProcedureGA2Info);

			invoiceLine.JI_PostClearanceProcedureGA2 = "XXX";
			AssertHasMessageErrorContaining(invoiceLine.JI_PostClearanceProcedureGA2Info, ListValidation.InvalidCodeMessageError);
		}
		public void TestCheckJI_PostClearanceProcedureGA3()
		{
			SetZZData_OGAAA();

			AssertEquals(ZString.Empty, invoiceLine.JI_PostClearanceProcedureGA2);
			AssertEquals(ZString.Empty, invoiceLine.JI_PostClearanceProcedureGA3);
			invoiceLine.Validation.ValidateJI_PostClearanceProcedureGA3();
			AssertNoMessageErrors(invoiceLine.JI_PostClearanceProcedureGA3Info);

			invoiceLine.JI_PostClearanceProcedureGA3 = "003";
			AssertHasMessageErrorContaining(invoiceLine.JI_PostClearanceProcedureGA3Info, "You should enter a value here only if you have the second post clearance procedure government agency.");

			invoiceLine.JI_PostClearanceProcedureGA2 = "002";
			invoiceLine.Validation.ValidateJI_PostClearanceProcedureGA3();
			AssertNoMessageErrors(invoiceLine.JI_PostClearanceProcedureGA3Info);

			invoiceLine.JI_PostClearanceProcedureGA3 = "XXX";
			AssertHasMessageErrorContaining(invoiceLine.JI_PostClearanceProcedureGA3Info, ListValidation.InvalidCodeMessageError);
		}
		void SetZZData_OGAAA()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(ZZ.NKCodeType.OtherGovernmentAndAssociatedAgency, "Other Government and Associated Agency");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.OtherGovernmentAndAssociatedAgency, "001", "과학기술부", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.OtherGovernmentAndAssociatedAgency, "002", "국방부", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.OtherGovernmentAndAssociatedAgency, "003", "보건부", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();
		}

		public void TestCheckJI_BrandCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(ZZ.NKCodeType.BrandCode, "Brand Codes");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.BrandCode, "0005", "AIRWALK", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.BrandCode, "0045", "DUNHILL", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			invoiceLine.JI_BrandCode = "0005";
			AssertNoMessageErrors(invoiceLine.JI_BrandCodeInfo);

			invoiceLine.JI_BrandCode = "0045";
			AssertNoMessageErrors(invoiceLine.JI_BrandCodeInfo);

			invoiceLine.JI_BrandCode = "XXX";
			AssertHasMessageErrorContaining(invoiceLine.JI_BrandCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJI_DomesticTaxCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.TariffTypes.DomesticTaxRate);

			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "941210-A", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "약주");
			helper.CreateTariffAttribute(Constants.ZZ.TariffAttributes.TaxClassification1, ChargeTypeList.Codes.LiquorTax, tariff1);

			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "821100-B", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "휘발유와 이와 유사한 대체유류");
			helper.CreateTariffAttribute(Constants.ZZ.TariffAttributes.TaxClassification1, ChargeTypeList.Codes.TransportationTax, tariff2);

			var tariff3 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "803008-C", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "냄새 맡는 담배");
			helper.CreateTariffAttribute(Constants.ZZ.TariffAttributes.TaxClassification1, ChargeTypeList.Codes.SpecialConsumptionTax, tariff3);
			Factory.Save();

			invoiceLine.JI_ProductTypeCode = ProductOrMaterialCodeList.Codes.A;
			invoiceLine.JI_DomesticTaxCode = "941210-A";
			AssertHasMessageErrorContaining(invoiceLine.JI_DomesticTaxCodeInfo, "If 'Product Or Material Code' is 'A' then, Domestic Tax Classification can't be Liquor Tax.");

			invoiceLine.JI_ProductTypeCode = ProductOrMaterialCodeList.Codes.B;
			invoiceLine.Validation.ValidateJI_DomesticTaxCode();
			AssertNoMessageErrors(invoiceLine.JI_DomesticTaxCodeInfo);

			invoiceLine.JI_DomesticTaxCode = "821100-B";
			AssertHasMessageErrorContaining(invoiceLine.JI_DomesticTaxCodeInfo, "If 'Product Or Material Code' is 'B' then, Domestic Tax Classification can't be Special Consumption Tax or Transportation Tax.");

			invoiceLine.JI_ProductTypeCode = ProductOrMaterialCodeList.Codes.A;
			invoiceLine.Validation.ValidateJI_DomesticTaxCode();
			AssertNoMessageErrors(invoiceLine.JI_DomesticTaxCodeInfo);

			invoiceLine.JI_UseCode = "2";
			invoiceLine.JI_DomesticTaxCode = "803008-C";
			AssertNoMessageErrors(invoiceLine.JI_DomesticTaxCodeInfo);

			invoiceLine.JI_ProductTypeCode = ProductOrMaterialCodeList.Codes.B;
			invoiceLine.Validation.ValidateJI_DomesticTaxCode();
			AssertHasMessageErrorContaining(invoiceLine.JI_DomesticTaxCodeInfo, "If 'Product Or Material Code' is 'B' then, Domestic Tax Classification can't be Special Consumption Tax or Transportation Tax.");

			invoiceLine.JI_DomesticTaxCode = "XXXZZZ-D";
			AssertHasMessageErrorContaining(invoiceLine.JI_DomesticTaxCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJI_DomesticTaxExemptionCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, Messaging.Constants.ZZ.TariffTypes.DomesticTaxReductionExemption);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "E106211", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "조세특례제한법 제106조의2제1항제1호 물품");
			helper.CreateTariffAttribute(Messaging.Constants.ZZ.TariffAttributeNames.TaxCode, ChargeTypeList.Codes.SpecialConsumptionTax, tariff1);

			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "D310204", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "주세법 제31조 제2항 제4호 (의약품 원료)");
			helper.CreateTariffAttribute(Messaging.Constants.ZZ.TariffAttributeNames.TaxCode, ChargeTypeList.Codes.LiquorTax, tariff2);
			Factory.Save();

			declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._33;
			invoiceLine.Validation.ValidateJI_DomesticTaxExemptionCode();
			AssertNoMessageErrors(invoiceLine.JI_DomesticTaxExemptionCodeInfo);

			invoiceLine.JI_DomesticTaxExemptionCode = "E106211";
			AssertHasMessageErrorContaining(invoiceLine.JI_DomesticTaxExemptionCodeInfo, MandatoryValidation.DoNotEntered);

			declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._13;
			invoiceLine.JI_DomesticTaxExemptionCode = "D310204";
			AssertNoMessageErrors(invoiceLine.JI_DomesticTaxExemptionCodeInfo);

			invoiceLine.JI_DomesticTaxExemptionCode = "ABCDEF";
			AssertHasMessageErrorContaining(invoiceLine.JI_DomesticTaxExemptionCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJI_VATReductionCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, Messaging.Constants.ZZ.TariffTypes.DomesticTaxReductionExemption);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "D310201", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "주세법 제31조 제2항 제1호 (주한외국공관 수입)");
			helper.CreateTariffAttribute(Messaging.Constants.ZZ.TariffAttributeNames.TaxCode, ChargeTypeList.Codes.VAT, tariff1);

			invoiceLine.Validation.ValidateJI_VATReductionCode();
			AssertNoMessageErrors(invoiceLine.JI_VATReductionCodeInfo);

			invoiceLine.JI_VATReductionCode = "D310201";
			AssertNoMessageErrors(invoiceLine.JI_VATReductionCodeInfo);

			invoiceLine.JI_VATReductionCode = "ABCDEF";
			AssertHasMessageErrorContaining(invoiceLine.JI_VATReductionCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJI_SpecificUseProductType()
		{
			invoiceLine.Declaration.SetValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._5FN);
			invoiceLine.JI_SpecificUseProductType = SpecificUseProductTypeList.Codes._10;
			AssertNoMessageErrors(invoiceLine.JI_SpecificUseProductTypeInfo);

			invoiceLine.JI_SpecificUseProductType = SpecificUseProductTypeList.Codes._11;
			AssertNoMessageErrors(invoiceLine.JI_SpecificUseProductTypeInfo);

			invoiceLine.JI_SpecificUseProductType = SpecificUseProductTypeList.Codes._99;
			AssertNoMessageErrors(invoiceLine.JI_SpecificUseProductTypeInfo);

			invoiceLine.JI_SpecificUseProductType = "12";
			AssertHasMessageErrorContaining(invoiceLine.JI_SpecificUseProductTypeInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.JI_SpecificUseProductType = "";
			AssertNoMessageErrors(invoiceLine.JI_SpecificUseProductTypeInfo);

			invoiceLine.JI_PCProcedure = "Y";
			invoiceLine.Validation.ValidateJI_SpecificUseProductType();
			AssertHasMessageErrorContaining(invoiceLine.JI_SpecificUseProductTypeInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_SpecificUseProductType = SpecificUseProductTypeList.Codes._10;
			AssertNoMessageErrors(invoiceLine.JI_SpecificUseProductTypeInfo);
		}

		public void TestCheckJI_ScheduledReExportCustomsOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(ZZ.NKCodeType.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.CustomsOffice, "010", "서울세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			AssertNoMessageErrors(invoiceLine.JI_ScheduledReExportCustomsOfficeInfo);

			invoiceLine.JI_ScheduledReExportCustomsOffice = "010";
			AssertNoMessageErrors(invoiceLine.JI_ScheduledReExportCustomsOfficeInfo);

			invoiceLine.JI_ScheduledReExportCustomsOffice = "X";
			AssertHasMessageErrorContaining(invoiceLine.JI_ScheduledReExportCustomsOfficeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJI_RN_NJIeExportDestinationCountry()
		{
			AssertNoMessageErrors(invoiceLine.JI_RN_NKReExportDestinationCountryInfo);

			invoiceLine.JI_RN_NKReExportDestinationCountry = "AE";
			AssertNoMessageErrors(invoiceLine.JI_RN_NKReExportDestinationCountryInfo);

			invoiceLine.JI_RN_NKReExportDestinationCountry = "XX";
			AssertHasMessageErrorContaining(invoiceLine.JI_RN_NKReExportDestinationCountryInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJI_JurisdictionalCusOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Constants.ZZ.NKCodeType.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.CustomsOffice, "010", "서울세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			invoiceLine.JI_JurisdictionalCusOffice = "X";
			AssertHasMessageErrorContaining(invoiceLine.JI_JurisdictionalCusOfficeInfo, ListValidation.InvalidCodeMessageError);

			declaration.SetValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._5FN);

			invoiceLine.JI_PCProcedure = "N";
			invoiceLine.JI_JurisdictionalCusOffice = "";
			AssertNoMessageErrors(invoiceLine.JI_JurisdictionalCusOfficeInfo);

			invoiceLine.JI_PCProcedure = "Y";
			invoiceLine.Validation.ValidateJI_JurisdictionalCusOffice();
			AssertHasMessageErrorContaining(invoiceLine.JI_JurisdictionalCusOfficeInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_JurisdictionalCusOffice = "010";
			AssertNoMessageErrors(invoiceLine.JI_JurisdictionalCusOfficeInfo);
		}

		public void TestCheckJI_SpecificUseCodeDescription()
		{
			declaration.SetValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._5FN);
			invoiceLine.JI_PCProcedure = "N";
			invoiceLine.Validation.ValidateJI_SpecificUseCodeDescription();
			AssertNoMessageErrors(invoiceLine.JI_SpecificUseCodeDescriptionInfo);

			invoiceLine.JI_PCProcedure = "Y";
			invoiceLine.Validation.ValidateJI_SpecificUseCodeDescription();
			AssertHasMessageErrorContaining(invoiceLine.JI_SpecificUseCodeDescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_SpecificUseCodeDescription = "Description";
			AssertNoMessageErrors(invoiceLine.JI_SpecificUseCodeDescriptionInfo);
		}

		public void TestCheckJI_COOLabelLocation()
		{
			invoiceLine.JI_COOLabelLocation = "";
			AssertHasMessageErrorContaining(invoiceLine.JI_COOLabelLocationInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_COOLabelLocation = "X";
			AssertHasMessageErrorContaining(invoiceLine.JI_COOLabelLocationInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.JI_COOLabelLocation = CountryOfOriginLabelLocationCodeList.Codes.B;
			AssertNoMessageErrors(invoiceLine.JI_COOLabelLocationInfo);
		}

		public void TestCheckJI_COOLabelType()
		{
			invoiceLine.JI_COOLabelType = "";
			AssertNoMessageErrors(invoiceLine.JI_COOLabelTypeInfo);
			invoiceLine.JI_COOLabelLocation = CountryOfOriginLabelLocationCodeList.Codes.B;
			invoiceLine.Validation.ValidateJI_COOLabelType();
			AssertHasMessageErrorContaining(invoiceLine.JI_COOLabelTypeInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_COOLabelType = CountryOfOriginLabelTypeCodeList.Codes.A;
			AssertHasMessageErrorContaining(invoiceLine.JI_COOLabelTypeInfo, "If Country Of Origin Label Location is 'B', then Country Of Origin Label Type cannot be 'A', 'C' or 'E'");
			invoiceLine.JI_COOLabelType = CountryOfOriginLabelTypeCodeList.Codes.B;
			AssertNoMessageErrors(invoiceLine.JI_COOLabelTypeInfo);
			invoiceLine.JI_COOLabelLocation = CountryOfOriginLabelLocationCodeList.Codes.N;
			invoiceLine.JI_COOLabelType = "";
			AssertNoMessageErrors(invoiceLine.JI_COOLabelTypeInfo);
		}

		public void TestCheckJI_COOExemptionReason()
		{
			invoiceLine.JI_COOExemptionReason = "";
			AssertNoMessageErrors(invoiceLine.JI_COOExemptionReasonInfo);
			invoiceLine.JI_COOLabelLocation = CountryOfOriginLabelLocationCodeList.Codes.E;
			invoiceLine.Validation.ValidateJI_COOExemptionReason();
			AssertHasMessageErrorContaining(invoiceLine.JI_COOExemptionReasonInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_COOExemptionReason = "16";
			AssertHasMessageErrorContaining(invoiceLine.JI_COOExemptionReasonInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.JI_COOExemptionReason = CountryOfOriginExemptionReasonCodeList.Codes._12;
			AssertNoMessageErrors(invoiceLine.JI_COOExemptionReasonInfo);
		}

		public void TestCheckJI_CourierCargoSelectivityIndicator()
		{
			var emptyOrgHeader = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK1", "READYKOREA1");

			var orgHeader = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK2", "READYKOREA2");
			var orgHeaderCodes = new IDNumberAndType[]
			{
					new IDNumberAndType() { Type = Constants.IdentificationType.CourierCompanyID, Number = "AD0001" },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(orgHeader, orgHeaderCodes);

			declaration.JE_OH_Forwarder = emptyOrgHeader.PK;
			invoiceLine.JI_CourierCargoSelectivityIndicator = CourierCargoSelectivityIndicatorCodeList.Codes.Y;
			AssertHasMessageErrorContaining(invoiceLine.JI_CourierCargoSelectivityIndicatorInfo, MandatoryValidation.DoNotEntered);

			declaration.JE_OH_Forwarder = orgHeader.PK;
			invoiceLine.JI_CourierCargoSelectivityIndicator = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.JI_CourierCargoSelectivityIndicatorInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_CourierCargoSelectivityIndicator = "A";
			AssertHasMessageErrorContaining(invoiceLine.JI_CourierCargoSelectivityIndicatorInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.JI_CourierCargoSelectivityIndicator = CourierCargoSelectivityIndicatorCodeList.Codes.N;
			AssertNoMessageErrors(invoiceLine.JI_CourierCargoSelectivityIndicatorInfo);
		}

		public void TestCheckJI_AdditionalDutyRate()
		{
			invoiceLine.JI_AdditionalDutyRate = 0;
			AssertNoMessageErrors(invoiceLine.JI_AdditionalDutyRateInfo);

			invoiceLine.JI_AdditionalDutyType = AdditionalDutyTypeCodeList.Codes.DumpingDuty;
			invoiceLine.Validation.ValidateJI_AdditionalDutyRate();
			AssertHasMessageErrorContaining(invoiceLine.JI_AdditionalDutyRateInfo, "Please enter an 'Additional Duty Rate' greater than 0.");

			invoiceLine.JI_AdditionalDutyRate = 1;
			AssertNoMessageErrors(invoiceLine.JI_AdditionalDutyRateInfo);
		}

		public void TestCheckJI_AdditionalDutyType()
		{
			invoiceLine.JI_AdditionalDutyType = "X";
			AssertHasMessageErrorContaining(invoiceLine.JI_AdditionalDutyTypeInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.JI_AdditionalDutyType = "I";
			AssertNoMessageErrors(invoiceLine.JI_AdditionalDutyTypeInfo);
		}

		public void TestCheckJI_InstallmentCode()
		{
			var tariffDRE = "A099000001";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType_DRE = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, ZZ.TariffTypes.DutyReductionExemption);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType_DRE.PK, tariffDRE, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "관세법 제99조제1호 해당물품");
			Factory.Save();

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._11;
			invoiceLine.JI_InstallmentCode = tariffDRE;
			AssertNoMessageErrors(invoiceLine.JI_InstallmentCodeInfo);

			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._12;
			invoiceLine.Validation.ValidateJI_InstallmentCode();
			AssertHasMessageErrorContaining(invoiceLine.JI_InstallmentCodeInfo, MandatoryValidation.DoNotEntered);

			invoiceLine.JI_InstallmentCode = ZString.Empty;
			AssertNoMessageErrors(invoiceLine.JI_InstallmentCodeInfo);

			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._11;
			declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._01;
			invoiceLine.JI_InstallmentCode = tariffDRE;
			AssertNoMessageErrors(invoiceLine.JI_InstallmentCodeInfo);

			declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._33;
			invoiceLine.Validation.ValidateJI_InstallmentCode();
			AssertHasMessageErrorContaining(invoiceLine.JI_InstallmentCodeInfo, MandatoryValidation.DoNotEntered);

			invoiceLine.JI_InstallmentCode = ZString.Empty;
			AssertNoMessageErrors(invoiceLine.JI_InstallmentCodeInfo);
		}

		public void TestCheckJI_Ingredient()
		{
			invoiceLine.JI_Description = "";
			invoiceLine.JI_Ingredient = "";
			AssertHasMessageErrorContaining(invoiceLine.JI_IngredientInfo, "You must enter either 'Goods Description' or 'Ingredient'.");

			invoiceLine.JI_Description = "Test_Description";
			invoiceLine.JI_Ingredient = "";
			AssertNoMessageErrorContaining(invoiceLine.JI_IngredientInfo, "You must enter either 'Goods Description' or 'Ingredient'.");

			invoiceLine.JI_Description = "";
			invoiceLine.JI_Ingredient = "Test_Ingredient";
			AssertNoMessageErrorContaining(invoiceLine.JI_IngredientInfo, "You must enter either 'Goods Description' or 'Ingredient'.");
		}

		public void TestCheckJI_DutyRateSelection()
		{
			var dataGrouping = Core.Constants.CountryCodes.KoreaSouth;
			var referenceDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = referenceDataHelper.CreateTariffType(dataGrouping, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();

			var emptyTariff = referenceDataHelper.CreateTariff(dataGrouping, tariffType.PK, "1206101000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "우육");

			var tariff = referenceDataHelper.CreateTariff(dataGrouping, tariffType.PK, "3706101000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "기타");
			referenceDataHelper.CreateTariffUOM(tariff, "CU1", "KG");
			var preferenceC2 = referenceDataHelper.CreatePreferenceForCountry("C2", "WTO협정세율(선택2)", Core.Constants.CountryCodes.KoreaSouth);
			var rateType = referenceDataHelper.CreateCusRateType(dataGrouping, Universal.Constants.RateTypes.Duty);
			var rateCodeDTA = referenceDataHelper.CreateCusRateCode(Factory, Constants.ZZ.RateCodes.DutyAdValorem, rateType.PK);
			var rateAdValorem = referenceDataHelper.CreateRate(tariff, rateCodeDTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.065", dataGrouping: dataGrouping, preferencePk: preferenceC2.PK);
			var rateCodeDTS = referenceDataHelper.CreateCusRateCode(Factory, Constants.ZZ.RateCodes.DutySpecific, rateType.PK);
			var rateSpecific = referenceDataHelper.CreateRate(tariff, rateCodeDTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "[KG] * 1560", dataGrouping: dataGrouping, preferencePk: preferenceC2.PK);
			var tradeGroup = referenceDataHelper.CreateTradeGroup("AU", "AU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			referenceDataHelper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Australia);
			referenceDataHelper.CreateCusApplicability(rateAdValorem, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			referenceDataHelper.CreateCusApplicability(rateSpecific, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			referenceDataHelper.CreateCusApplicability(rateAdValorem, null, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date, additionalCode: ZZ.ApplicabilityAdditionalCodes.Max);
			referenceDataHelper.CreateCusApplicability(rateAdValorem, null, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date, additionalCode: ZZ.ApplicabilityAdditionalCodes.Min);
			referenceDataHelper.CreateCusApplicability(rateSpecific, null, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date, additionalCode: ZZ.ApplicabilityAdditionalCodes.Max);
			referenceDataHelper.CreateCusApplicability(rateSpecific, null, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date, additionalCode: ZZ.ApplicabilityAdditionalCodes.Min);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.JI_Tariff = emptyTariff.ZZ1_TariffCode;
			invoiceLine.Validation.ValidateJI_DutyRateSelection();
			AssertNoMessageErrorContaining(invoiceLine.JI_DutyRateSelectionInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
			invoiceLine.JI_PrimaryPreference = "C2";
			invoiceLine.JI_CountryOfOrigin = "AU";
			invoiceLine.Validation.ValidateJI_DutyRateSelection();
			AssertHasMessageErrorContaining(invoiceLine.JI_DutyRateSelectionInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_DutyRateSelection = "X";
			AssertHasMessageErrorContaining(invoiceLine.JI_DutyRateSelectionInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.JI_DutyRateSelection = ZZ.ApplicabilityAdditionalCodes.Max;
			AssertNoMessageErrors(invoiceLine.JI_DutyRateSelectionInfo);
		}

		public void TestCheckJI_PCProcedure()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, ZZ.TariffTypes.DutyReductionExemption);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "A093000004", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "관세법 제93조제4호 해당물품");
			helper.CreateTariffAttribute(Customs.KR.Messaging.Constants.ZZ.TariffAttributes.IsDutyExempt, YesNo.Yes, tariff1);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "A1070001", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "관세법 제107조 제1항 해당물품");
			Factory.Save();

			var validation = invoiceLine.Validation;

			validation.ValidateJI_PCProcedure();
			AssertNoMessageErrors(invoiceLine.JI_PCProcedureInfo);

			invoiceLine.JI_PCProcedure = "X";
			AssertHasMessageErrorContaining(invoiceLine.JI_PCProcedureInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.JI_SecondaryPreference = "A093000004";
			invoiceLine.JI_PCProcedure = ZString.Empty;
			AssertNoMessageErrors(invoiceLine.JI_PCProcedureInfo);

			declaration.SetValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._5FN);
			validation.ValidateJI_PCProcedure();
			AssertHasMessageErrorContaining(invoiceLine.JI_PCProcedureInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_PCProcedure = YesNoList.Codes.No;
			AssertNoMessageErrors(invoiceLine.JI_PCProcedureInfo);

			invoiceLine.JI_PCProcedure = YesNoList.Codes.Yes;
			AssertHasMessageErrorContaining(invoiceLine.JI_PCProcedureInfo, "If Post Clearance YN is Y, you must enter the Importer's type of business. Please press F3 in the Importer Organization field and set the Type of Business on the Organization form > Details > Config > Korea.");
			AssertHasMessageErrorContaining(invoiceLine.JI_PCProcedureInfo, "If Post Clearance YN is Y, you must enter the Goods Location's address. Please press F3 in the Goods Location field and enter the address on the Organization form > Addresses.");
			AssertHasMessageErrorContaining(invoiceLine.JI_PCProcedureInfo, "If Post Clearance YN is Y, you must enter the Goods Location's postcode. Please press F3 in the Goods Location field and enter the postcode on the Organization form > Addresses.");
			AssertHasMessageErrorContaining(invoiceLine.JI_PCProcedureInfo, "If Post Clearance YN is Y, you must enter the Goods Location's phone number. Please press F3 in the Goods Location field and enter the phone number on the Organization form > Addresses.");

			var importer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "2", "상호1");
			TestOrgDataSetUpHelper.AddOrgAddress(importer.MainAddress, "수입자주소", "", "");
			var organizationWrapper = OrgHeaderWrapper.New(importer);
			organizationWrapper.ZO_TypeOfBusiness = "업태";

			var consignee = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "3", "상호2");
			TestOrgDataSetUpHelper.AddOrgAddress(consignee.MainAddress, "goods location address 1", "", "12345");
			consignee.MainAddress.OA_Phone = "01012345678";

			Factory.Save();

			declaration.JE_OA_ImporterAddress = importer.MainAddress.PK;
			invoiceLine.JI_OA_ConsigneeAddress = consignee.MainAddress.PK;
			validation.ValidateJI_PCProcedure();
			AssertNoMessageErrors(invoiceLine.JI_PCProcedureInfo);
		}

		public void TestCheckJI_RN_NKReExportDestinationCountry()
		{
			invoiceLine.JI_RN_NKReExportDestinationCountry = "X";
			AssertHasMessageErrorContaining(invoiceLine.JI_RN_NKReExportDestinationCountryInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.JI_RN_NKReExportDestinationCountry = "KR";
			AssertNoMessageErrors(invoiceLine.JI_RN_NKReExportDestinationCountryInfo);
		}

		public void TestCheckJI_CoveredByCOOExporter()
		{
			invoiceLine.JI_CoveredByCOOExporter = true;
			AssertNoMessageErrorContaining(invoiceLine.JI_CoveredByCOOExporterInfo, "The entered supplier does not have a registration number of type 10. Please press F3 and add a number of type '10' in Config > Registration Numbers/Codes on the Organization form");

			var supplier = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK2", "READYKOREA2");
			invoice.JZ_OH_Supplier = supplier.PK;
			invoiceLine.JI_PrimaryPreference = "FEU1";
			invoiceLine.JI_CoveredByCOOExporter = false;
			AssertNoMessageErrorContaining(invoiceLine.JI_CoveredByCOOExporterInfo, "The entered supplier does not have a registration number of type 10. Please press F3 and add a number of type '10' in Config > Registration Numbers/Codes on the Organization form");

			invoiceLine.JI_CoveredByCOOExporter = true;
			AssertHasMessageErrorContaining(invoiceLine.JI_CoveredByCOOExporterInfo, "The entered supplier does not have a registration number of type 10. Please press F3 and add a number of type '10' in Config > Registration Numbers/Codes on the Organization form");

			var orgHeaderCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = Constants.IdentificationType.CertificateOfOriginExporterNumber, Number = "P641150121378" },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(supplier, orgHeaderCodes);
			invoiceLine.Validation.ValidateJI_CoveredByCOOExporter();
			AssertNoMessageErrorContaining(invoiceLine.JI_CoveredByCOOExporterInfo, "The entered supplier does not have a registration number of type 10. Please press F3 and add a number of type '10' in Config > Registration Numbers/Codes on the Organization form");
		}

			public void TestCheckDutyReductionSeqNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var validation = invoiceLine.Validation as IMPJobComInvoiceLineValidation;

			invoiceLine.DutyReductionGroupNumber = "A";
			validation.ValidateDutyReductionSeqNumber();
			AssertNoMessageErrorContaining(invoiceLine.DutyReductionSeqNumberInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.SetValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._5FN);

			invoiceLine.DutyReductionGroupNumber = "";
			validation.ValidateDutyReductionSeqNumber();
			AssertNoMessageErrorContaining(invoiceLine.DutyReductionSeqNumberInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.DutyReductionGroupNumber = "A";
			validation.ValidateDutyReductionSeqNumber();
			AssertHasMessageErrorContaining(invoiceLine.DutyReductionSeqNumberInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.DutyReductionSeqNumber = "001";
			validation.ValidateDutyReductionSeqNumber();
			AssertNoMessageErrorContaining(invoiceLine.DutyReductionSeqNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckDutyReductionItemNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var validation = invoiceLine.Validation as IMPJobComInvoiceLineValidation;

			invoiceLine.DutyReductionGroupNumber = "A";
			validation.ValidateDutyReductionItemNumber();
			AssertNoMessageErrorContaining(invoiceLine.DutyReductionItemNumberInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.SetValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._5FN);

			invoiceLine.DutyReductionGroupNumber = "";
			validation.ValidateDutyReductionItemNumber();
			AssertNoMessageErrorContaining(invoiceLine.DutyReductionItemNumberInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.DutyReductionGroupNumber = "A";
			validation.ValidateDutyReductionItemNumber();
			AssertHasMessageErrorContaining(invoiceLine.DutyReductionItemNumberInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.DutyReductionItemNumber = "02";
			validation.ValidateDutyReductionItemNumber();
			AssertNoMessageErrorContaining(invoiceLine.DutyReductionItemNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();

			Factory.Save();
		}
		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;

		const string CannotBeZeroIfUQIsNotEmptyErrorMsg = "Customs Quantity must be entered if UQ is not empty";
	}
}
