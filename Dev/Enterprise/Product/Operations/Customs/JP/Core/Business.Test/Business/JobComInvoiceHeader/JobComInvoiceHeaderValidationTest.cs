using System;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Grammar;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.JP;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(JobComInvoiceHeaderValidation))]
	sealed class JobComInvoiceHeaderValidationTest : Customs.Business.Testing.JobComInvoiceHeaderValidationTest
	{
		protected override Type GetTypeForTest() => typeof(JobComInvoiceHeaderValidation);

		public void TestCheckJZ_ValuationDateOverride()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var info = invoiceHeader.JZ_ValuationDateOverrideInfo;

			var expectedErrorMessage1 = "must be the same as the Declaration Valuation Date";
			var expectedErrorMessage2 = "Invoice Header Valuation Date 2023-05-05 must be the same as the Declaration Valuation Date 2023-05-06.";

			declaration.JE_ValuationDate = ZDate.Empty;
			Assert(invoiceHeader.JZ_ValuationDateOverride.IsEmpty);
			invoiceHeader.Validation.ValidateJZ_ValuationDateOverride();
			AssertNoMessageErrorContaining(info, expectedErrorMessage1);

			declaration.JE_ValuationDate = ZDate.Today;
			invoiceHeader.Validation.ValidateJZ_ValuationDateOverride();
			AssertNoMessageErrorContaining(info, expectedErrorMessage1);

			declaration.JE_ValuationDate = ZDate.Empty;
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2023, 5, 5);
			invoiceHeader.Validation.ValidateJZ_ValuationDateOverride();
			AssertNoMessageErrorContaining(info, expectedErrorMessage1);

			declaration.JE_ValuationDate = new ZDate(2023, 5, 6);
			invoiceHeader.Validation.ValidateJZ_ValuationDateOverride();
			AssertHasMessageErrorContaining(info, expectedErrorMessage2);
			AssertHasMessageErrorContaining(info, expectedErrorMessage1);
		}

		public void TestCheckJZ_ValuationCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.ComprehensiveValuations.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = ZGuid.Empty;

			var targetInfo = invoiceHeader.JZ_ValuationCodeInfo;
			var expectedErrorMessage = "The entered Valuation Type cannot be used when there are at least one Comprehensive Valuation Number.";

			invoiceHeader.JZ_ValuationCode = string.Empty;
			AssertNoMessageError(targetInfo, expectedErrorMessage);

			invoiceHeader.JZ_ValuationCode = ValuationTypeCodeList.Codes.Five;
			AssertNoMessageError(targetInfo, expectedErrorMessage);

			invoiceHeader.JZ_ValuationCode = ValuationTypeCodeList.Codes.Z;
			AssertNoMessageError(targetInfo, expectedErrorMessage);

			invoiceHeader.JZ_ValuationCode = ValuationTypeCodeList.Codes.Six;
			AssertHasMessageError(targetInfo, expectedErrorMessage);

			invoiceHeader.JZ_ValuationCode = ValuationTypeCodeList.Codes.Seven;
			AssertHasMessageError(targetInfo, expectedErrorMessage);

			invoiceHeader.JZ_ValuationCode = ValuationTypeCodeList.Codes.Zero;
			AssertHasMessageError(targetInfo, expectedErrorMessage);

			expectedErrorMessage = "Valuation Type cannot be entered when Shipment Type is IMP and Declaration Type contains Y, H, or N.";
			declaration.JE_MessageType = JPJobMessageTypeList.Codes.Import;
			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.H;
			invoiceHeader.JZ_ValuationCode = ValuationTypeCodeList.Codes.Five;
			AssertNoMessageError(targetInfo, expectedErrorMessage);

			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.Y;
			invoiceHeader.Validation.ValidateJZ_ValuationCode();
			AssertNoMessageError(targetInfo, expectedErrorMessage);

			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.N;
			invoiceHeader.Validation.ValidateJZ_ValuationCode();
			AssertNoMessageError(targetInfo, expectedErrorMessage);

			invoiceLine.JI_CEI = instruction.PK;
			AssertValuationCode();

			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.H;
			AssertValuationCode();

			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.Y;
			AssertValuationCode();

			void AssertValuationCode()
			{
				invoiceHeader.JZ_ValuationCode = string.Empty;
				AssertNoMessageError(targetInfo, expectedErrorMessage);
				invoiceHeader.JZ_ValuationCode = ValuationTypeCodeList.Codes.Five;
				AssertHasMessageError(targetInfo, expectedErrorMessage);
			}
		}

		public void TestCheckJZ_InvoiceAmount()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();

			invoiceHeader.JZ_InvoiceAmount = 100.1;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "JPY";
			AssertHasMessageError(invoiceHeader.JZ_InvoiceAmountInfo, "Invoice Total must be a whole number when Invoice Currency is JPY.");

			invoiceHeader.JZ_InvoiceAmount = 100;
			AssertNoMessageError(invoiceHeader.JZ_InvoiceAmountInfo, "Invoice Total must be a whole number when Invoice Currency is JPY.");
			AssertNoError(invoiceHeader.JZ_InvoiceAmountInfo, EnterValueGreaterThanOrEqualToZeroMessage(invoiceHeader.JZ_InvoiceAmountInfo.HumanReadableName));

			invoiceHeader.JZ_InvoiceAmount = -100;
			AssertHasError(invoiceHeader.JZ_InvoiceAmountInfo, EnterValueGreaterThanOrEqualToZeroMessage(invoiceHeader.JZ_InvoiceAmountInfo.HumanReadableName));
		}

		public void TestCheckJZ_Weight()
		{
			ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(invoiceHeader.JZ_WeightInfo, invoiceHeader.JZ_WeightUQInfo);

			invoiceHeader.JZ_Weight = -100;
			AssertHasError(invoiceHeader.JZ_WeightInfo, EnterValueGreaterThanOrEqualToZeroMessage(invoiceHeader.JZ_WeightInfo.HumanReadableName));

			invoiceHeader.JZ_Weight = 100;
			AssertNoError(invoiceHeader.JZ_WeightInfo, EnterValueGreaterThanOrEqualToZeroMessage(invoiceHeader.JZ_WeightInfo.HumanReadableName));
		}

		public void TestCheckJZ_WeightUQ()
		{
			ValidationTestHelper.AssertErrorIfInvalidCode(invoiceHeader.JZ_WeightUQInfo, "XX", "KG");
			ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(invoiceHeader.JZ_WeightUQInfo, invoiceHeader.JZ_WeightInfo);
		}

		public void TestCheckJZ_NetWeight()
		{
			ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(invoiceHeader.JZ_NetWeightInfo, invoiceHeader.JZ_NetWeightUQInfo);

			invoiceHeader.JZ_NetWeight = -100;
			AssertHasError(invoiceHeader.JZ_NetWeightInfo, EnterValueGreaterThanOrEqualToZeroMessage(invoiceHeader.JZ_NetWeightInfo.HumanReadableName));

			invoiceHeader.JZ_NetWeight = 100;
			AssertNoError(invoiceHeader.JZ_NetWeightInfo, EnterValueGreaterThanOrEqualToZeroMessage(invoiceHeader.JZ_NetWeightInfo.HumanReadableName));
		}

		public void TestCheckJZ_NetWeightUQ()
		{
			ValidationTestHelper.AssertErrorIfInvalidCode(invoiceHeader.JZ_NetWeightUQInfo, "XX", "KG");
			ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(invoiceHeader.JZ_NetWeightUQInfo, invoiceHeader.JZ_NetWeightInfo);
		}

		public void TestCheckJZ_InvoiceType_Import()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var header = jobDeclaration.Invoices.AddNew();
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			header.InvoiceLines.AddNew();
			var subTypeRequireInvoiceTypeLists = new string[]
			{
				JPImportDeclarationTypeList.Codes.C,
				JPImportDeclarationTypeList.Codes.F,
				JPImportDeclarationTypeList.Codes.J,
				JPImportDeclarationTypeList.Codes.P,
				JPImportDeclarationTypeList.Codes.S,
				JPImportDeclarationTypeList.Codes.M,
				JPImportDeclarationTypeList.Codes.A,
				JPImportDeclarationTypeList.Codes.G,
				JPImportDeclarationTypeList.Codes.K,
				JPImportDeclarationTypeList.Codes.D,
				JPImportDeclarationTypeList.Codes.U,
				JPImportDeclarationTypeList.Codes.L,
				JPImportDeclarationTypeList.Codes.B,
				JPImportDeclarationTypeList.Codes.E,
				JPImportDeclarationTypeList.Codes.R
			};

			CombineAssertions(() =>
			{
				foreach (var subType in subTypeRequireInvoiceTypeLists)
				{
					AssertInvoiceType(subType, header);
				}
			});

			jobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			subTypeRequireInvoiceTypeLists = new string[]
			{
				JPImportDeclarationTypeList.Codes.C,
				JPImportDeclarationTypeList.Codes.F,
				JPImportDeclarationTypeList.Codes.Y,
				JPImportDeclarationTypeList.Codes.J,
				JPImportDeclarationTypeList.Codes.P,
				JPImportDeclarationTypeList.Codes.S,
				JPImportDeclarationTypeList.Codes.M,
				JPImportDeclarationTypeList.Codes.A,
				JPImportDeclarationTypeList.Codes.G,
				JPImportDeclarationTypeList.Codes.K,
				JPImportDeclarationTypeList.Codes.D,
				JPImportDeclarationTypeList.Codes.U,
				JPImportDeclarationTypeList.Codes.L,
				JPImportDeclarationTypeList.Codes.B,
				JPImportDeclarationTypeList.Codes.E,
				JPImportDeclarationTypeList.Codes.R
			};

			CombineAssertions(() =>
			{
				foreach (var subType in subTypeRequireInvoiceTypeLists)
				{
					AssertInvoiceType(subType, header);
				}
			});
		}

		void AssertInvoiceType(string declarationType, JobComInvoiceHeader header)
		{
			var instruction = header.JobDeclaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = declarationType;
			var invoiceTypeInfo = header.JZ_InvoiceTypeInfo;
			var invoiceLine = header.InvoiceLines[0];
			invoiceLine.JI_CEI = ZGuid.Empty;
			header.JZ_InvoiceType = ZString.Empty;
			AssertNoMessageError(string.Format("When CEI_Style is {0}, invoice type is required.", declarationType), invoiceTypeInfo, expectedNeedInvoiceTypeErrorMessage);

			invoiceLine.JI_CEI = instruction.PK;
			header.Validation.ValidateJZ_InvoiceType();
			AssertHasMessageError(string.Format("When CEI_Style is {0}, invoice type is required.", declarationType), invoiceTypeInfo, expectedNeedInvoiceTypeErrorMessage);

			header.JZ_InvoiceType = RepresentativeInvoiceTypes.Codes.A;
			AssertNoMessageError(string.Format("When CEI_Style is {0}, invoice type is required.", declarationType), invoiceTypeInfo, expectedNeedInvoiceTypeErrorMessage);
		}

		public void TestJZ_ComprehensiveInsuranceNumberValidation()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var header = jobDeclaration.Invoices.AddNew();
			header.JZ_InsuranceType = "";
			header.JZ_ComprehensiveInsuranceNumber = "";
			AssertNoMessageError(header.JZ_ComprehensiveInsuranceNumberInfo, "The Blanket Insurance Number is required if the value of the Insurance Type is 'B'.");

			header.JZ_InsuranceType = "B";
			header.Validation.ValidateJZ_ComprehensiveInsuranceNumber();
			AssertHasMessageError(header.JZ_ComprehensiveInsuranceNumberInfo, "The Blanket Insurance Number is required if the value of the Insurance Type is 'B'.");

			header.JZ_ComprehensiveInsuranceNumber = "1234ABCD";
			AssertNoMessageError(header.JZ_ComprehensiveInsuranceNumberInfo, "The Blanket Insurance Number is required if the value of the Insurance Type is 'B'.");
		}

		public void TestJZ_ElectronicInvoiceReceiptNumberValidation()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var header = jobDeclaration.Invoices.AddNew();
			header.Validation.ValidateJZ_ElectronicInvoiceReceiptNumber();
			AssertNoErrors(header.JZ_ElectronicInvoiceReceiptNumberInfo);

			header.JZ_ElectronicInvoiceReceiptNumber = "ABC1234567";
			AssertNoErrors(header.JZ_ElectronicInvoiceReceiptNumberInfo);

			header.JZ_ElectronicInvoiceReceiptNumber = "ABC1234<>?";
			AssertHasMessageError(header.JZ_ElectronicInvoiceReceiptNumberInfo, "Only Numbers and Upper Case Letters will be accepted.");

			header.JZ_InvoiceType = RepresentativeInvoiceTypes.Codes.C;
			header.JZ_ElectronicInvoiceReceiptNumber = string.Empty;
			AssertHasMessageError(header.JZ_ElectronicInvoiceReceiptNumberInfo, "Electronic Invoice Receipt Number cannot be empty when Invoice Type is C or D.");

			header.JZ_InvoiceType = RepresentativeInvoiceTypes.Codes.D;
			header.Validation.ValidateJZ_ElectronicInvoiceReceiptNumber();
			AssertHasMessageError(header.JZ_ElectronicInvoiceReceiptNumberInfo, "Electronic Invoice Receipt Number cannot be empty when Invoice Type is C or D.");

			header.JZ_InvoiceType = RepresentativeInvoiceTypes.Codes.C;
			header.JZ_ElectronicInvoiceReceiptNumber = "ABC1234567";
			AssertNoErrors(header.JZ_ElectronicInvoiceReceiptNumberInfo);

			header.JZ_InvoiceType = RepresentativeInvoiceTypes.Codes.D;
			header.JZ_ElectronicInvoiceReceiptNumber = "ABC1234567";
			AssertNoErrors(header.JZ_ElectronicInvoiceReceiptNumberInfo);

			header.JZ_InvoiceType = RepresentativeInvoiceTypes.Codes.B;
			header.JZ_ElectronicInvoiceReceiptNumber = string.Empty;
			AssertNoErrors(header.JZ_ElectronicInvoiceReceiptNumberInfo);

			header.JZ_InvoiceType = RepresentativeInvoiceTypes.Codes.B;
			header.JZ_ElectronicInvoiceReceiptNumber = "123";
			AssertHasMessageError(header.JZ_ElectronicInvoiceReceiptNumberInfo, "Electronic Invoice Receipt Number must be empty when Invoice Type is not C nor D.");
		}

		public void TestJZ_InsuranceTypeValidation()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var header = jobDeclaration.Invoices.AddNew();
			header.JZ_InsuranceType = "";
			header.Validation.ValidateJZ_InsuranceType();
			AssertNoErrors("This field is not mandatory", header.JZ_InsuranceTypeInfo);
			AssertNoMessageErrors("This field is not mandatory", header.JZ_InsuranceTypeInfo);

			header.JZ_InsuranceType = "B";
			AssertNoMessageError(header.JZ_InsuranceTypeInfo, "The code you have selected is not in the list.");

			header.JZ_InsuranceType = "Z";
			AssertHasMessageError(header.JZ_InsuranceTypeInfo, "The code you have selected is not in the list.");

			var expectedEnteredErrorMessage = "Insurance Type must be empty when Incoterm is C&I or CIF.";
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			header.JZ_IncoTerm = "CIF";
			header.JZ_InsuranceType = "B";
			AssertNoMessageError(header.JZ_InsuranceTypeInfo, expectedEnteredErrorMessage);

			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			header.Validation.ValidateJZ_InsuranceType();
			AssertHasMessageError(header.JZ_InsuranceTypeInfo, expectedEnteredErrorMessage);

			header.JZ_IncoTerm = "FOB";
			header.Validation.ValidateJZ_InsuranceType();
			AssertNoMessageError(header.JZ_InsuranceTypeInfo, expectedEnteredErrorMessage);

			header.JZ_IncoTerm = "C&I";
			header.JZ_InsuranceType = string.Empty;
			AssertNoMessageError(header.JZ_InsuranceTypeInfo, expectedEnteredErrorMessage);
		}

		public void TestJZ_InvoiceAmountTypeValidation()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var header = jobDeclaration.Invoices.AddNew();
			header.JZ_InvoiceAmountType = "";
			header.Validation.ValidateJZ_InvoiceType();
			AssertNoMessageErrors("This is not a mandatory field", header.JZ_InvoiceAmountTypeInfo);

			header.JZ_InvoiceAmountType = "B";
			AssertNoMessageErrors("Valid code from list", header.JZ_InvoiceAmountTypeInfo);

			header.JZ_InvoiceAmountType = "Z";
			AssertHasMessageErrorContaining("JPZ_InvoicePriceClassificationInfo", header.JZ_InvoiceAmountTypeInfo, ListValidation.InvalidCodeMessageError.ToString());
		}

		public void TestCheckJZ_FreightType()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var header = jobDeclaration.Invoices.AddNew();
			header.JZ_FreightType = "";
			header.Validation.ValidateJZ_FreightType();
			AssertNoErrors("This field is not mandatory", header.JZ_FreightTypeInfo);
			AssertNoMessageErrors("This field is not mandatory", header.JZ_FreightTypeInfo);

			header.JZ_FreightType = "B";
			AssertNoMessageError(header.JZ_FreightTypeInfo, "The code you have selected is not in the list.");

			header.JZ_FreightType = "Z";
			AssertHasMessageError(header.JZ_FreightTypeInfo, "The code you have selected is not in the list.");

			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var charge1 = header.Charges.AddNew();
			var charge2 = header.Charges.AddNew();
			charge1.J7_ChargeType = "COM";
			charge2.J7_ChargeType = "DED";
			header.JZ_FreightType = FreightRatesTypes.Codes.A;
			AssertHasMessageError(header.JZ_FreightTypeInfo, "Freight Type A can only be used when the invoice has at least one Invoice Charge that is OFT - International Freight.");

			var groupCharge = jobDeclaration.TopGroupInvoice.Charges.AddNew();
			groupCharge.J7_ChargeType = "OFT";
			header.Validation.ValidateJZ_FreightType();
			AssertNoMessageError(header.JZ_FreightTypeInfo, "Freight Type A can only be used when the invoice has at least one Invoice Charge that is OFT - International Freight.");
			groupCharge.J7_ChargeType = "COM";

			charge2.J7_ChargeType = "OFT";
			header.Validation.ValidateJZ_FreightType();
			AssertNoMessageError(header.JZ_FreightTypeInfo, "Freight Type A can only be used when the invoice has at least one Invoice Charge that is OFT - International Freight.");

			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			charge2.J7_ChargeType = "DED";
			header.Validation.ValidateJZ_FreightType();
			AssertNoMessageError(header.JZ_FreightTypeInfo, "Freight Type A can only be used when the invoice has at least one Invoice Charge that is OFT - International Freight.");

			var instruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = JPExportDeclarationTypeList.Codes.T;
			header.JZ_FreightType = FreightRatesTypes.Codes.C;
			AssertNoMessageError(header.JZ_FreightTypeInfo, "Freight Type cannot be C when Declaration Type is Y.");

			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.Y;
			header.Validation.ValidateJZ_FreightType();
			AssertNoMessageError(header.JZ_FreightTypeInfo, "Freight Type cannot be C when Declaration Type is Y.");

			var invoiceLine = header.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			header.Validation.ValidateJZ_FreightType();
			AssertHasMessageError(header.JZ_FreightTypeInfo, "Freight Type cannot be C when Declaration Type is Y.");

			header.JZ_FreightType = FreightRatesTypes.Codes.H;
			AssertNoMessageError(header.JZ_FreightTypeInfo, "Freight Type cannot be C when Declaration Type is Y.");
		}

		public void TestCheckJZ_AdvanceRulingOnValuation1()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = declaration.Invoices.AddNew();

			header.JZ_AdvanceRulingOnValuation1 = "123456";
			AssertHasMessageError(header.JZ_AdvanceRulingOnValuation1Info, $"{header.JZ_AdvanceRulingOnValuation1Info.HumanReadableName} must be exactly 7 characters long.");
			header.JZ_AdvanceRulingOnValuation1 = "1234567";
			AssertNoMessageError(header.JZ_AdvanceRulingOnValuation1Info, $"{header.JZ_AdvanceRulingOnValuation1Info.HumanReadableName} must be exactly 7 characters long.");
		}

		public void TestCheckJZ_AdvanceRulingOnValuation2()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = declaration.Invoices.AddNew();

			header.JZ_AdvanceRulingOnValuation2 = "123456";
			AssertHasMessageError(header.JZ_AdvanceRulingOnValuation2Info, $"{header.JZ_AdvanceRulingOnValuation2Info.HumanReadableName} must be exactly 7 characters long.");
			header.JZ_AdvanceRulingOnValuation2 = "1234567";
			AssertNoMessageError(header.JZ_AdvanceRulingOnValuation2Info, $"{header.JZ_AdvanceRulingOnValuation2Info.HumanReadableName} must be exactly 7 characters long.");
		}

		readonly string expectedNeedInvoiceTypeErrorMessage = "Invoice Type is required.";

		string EnterValueGreaterThanOrEqualToZeroMessage(string propertyDescriptor) => $"Please enter {Grammar.Instance.IndefiniteArticlePrefix(propertyDescriptor)}'{propertyDescriptor}' greater than or equal to 0.";
	}
}
