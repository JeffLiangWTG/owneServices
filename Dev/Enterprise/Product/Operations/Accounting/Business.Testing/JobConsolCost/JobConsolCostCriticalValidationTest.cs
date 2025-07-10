using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ConsolCosting.Testing
{
	public class JobConsolCostCriticalValidationTest : CriticalValidationTest<JobConsolCost>
	{
		protected override List<TestCaseDefinitionWithDelegate_Obsolete> GetTestCases()
		{
			List<TestCaseDefinitionWithDelegate_Obsolete> result = new List<TestCaseDefinitionWithDelegate_Obsolete>();
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("OSCostAmount Equal", factory => OSCostAmountEqual(factory)));
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("LocalCostAmount Equal", factory => LocalCostAmountEqual(factory)));
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("OSGSTAmount Equal", factory => OSGSTAmountEqual(factory)));
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("ParentId and ParentTableCode Value Set", factory => ParentIdAndTableCodeValueSet(factory)));
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("OSCostAmount Not Equal", factory => OSCostAmountNotEqual(factory), true, CriticalValidationErrorType.OverseasCostAmountNotEqualToSumOfApportionmentsOverseasCostAmount_9, "Overseas cost amount is not equal to the sum of the apportionment's overseas cost amount", "Job Consol Cost:\r\n\tPK = ", "JR_OSCostAmt Stack Trace = "));
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("LocalCostAmount Not Equal", factory => LocalCostAmountNotEqual(factory), true, CriticalValidationErrorType.LocalCostAmountNotEqualToSumOfApportionmentsLocalCostAmount_6, "Local cost amount is not equal to the sum of the apportionment's local cost amount", "Job Consol Cost:\r\n\tPK = "));
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("OSGSTAmount Not Equal", factory => OSGSTAmountNotEqual(factory), true, CriticalValidationErrorType.OverseasCostTaxAmountNotEqualToSumOfApportionmentsOverseasCostTaxAmount_11, "Overseas cost tax amount is not equal to the sum of the apportionment's Overseas cost tax amount", "Job Consol Cost:\r\n\tPK = "));
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("Parent Id is not set", factory => ParentIdNotSet(factory), true, CriticalValidationErrorType.CostNotLinkedToJobDueToEmptyParentID_2, "Cost is not linked to a job properly due to Parent ID is empty", "Job Consol Cost:\r\n\tPK = "));
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("Parent Table Code is not set", factory => ParentIdSetButTableCodeNotSet(factory), true, CriticalValidationErrorType.CostNotLinkedToJobDueToEmptyParentTableCode_1, "Cost is not linked to a job properly due to Parent Table Code is empty", "Job Consol Cost:\r\n\tPK = "));
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("Amounts not equal, but cost after posting reversing.", factory => CostAmountsNotEqualAndPostingWasReversed(factory)));
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("CheckChargeAndConsolCostInvoiceDetailsAreEqual, all details are equal", CreatePostedChargeWithConsolCostAndAllCorrectInvoiceDetails));
			string expectedMessage1 =
@"Job Consol Cost:
	PK = a12986b4-036c-4cb2-a0e7-0c96438a46dd
	Type = JobConsolCost";
			string mismatchedInfo = "Invoice Number:    111    |    444444";
			string expectedMessage2 =
@"	IsInDb = False
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = False
	IsDeleting = False

Business Contexts = None

	Charge Code = N51N0S01T7, Invoice # = 444444, Invoice Date = 05-Jan-10 00:00:00, Currency = , OS Cost Amount = 0, GST is Overridden = Yes, OS GST Amount = 0, Exchange Rate = 0, Local Cost Amount = 0, Creditor = XVBQP68SIYXQ, PPDCLT = , Apportionment Method = CHG, Supplier Cost Reference = ABC, Payment Date = 10-Jan-10 00:00:00, Payment Type = , Cheque # = , Bank Account = 00000000-0000-0000-0000-000000000000, Cheque Book = 00000000-0000-0000-0000-000000000000, Tax Rate = 41312608-7ae1-4155-af94-0af99cdfdc97, Tax Date = , AR Invoice = 00000000-0000-0000-0000-000000000000, AP Invoice = 9670ef3f-c8ad-496d-a278-c40bfdfbba1e, Is For Collect Invoice = N, Consol = 40b5446d-6066-4ed1-b9c1-8ca588a2a43d, Is Final = No, Tax Code = TAX1, Supply Type = , Tax Branch = .
Parent collections:";
			string expectedMessage3 = @"
Header: PK = ";
			string expectedMessage4 = @"
Charge with incorrect data:
Charge: PK = f8a8e869-ce2a-4049-a9df-d0b7a7e79f4e";
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("CheckChargeAndConsolCostInvoiceDetailsAreEqual, Invoice Number is not equal", delegate(BusinessObjectFactory factory)
			{
				JobConsolCost consolCost = CreatePostedChargeWithConsolCostAndAllCorrectInvoiceDetails(factory);
				consolCost.E6_InvoiceNum = "444444";
				return consolCost;
			}

			, true, CriticalValidationErrorType.ApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetails_8, "Apportion Split Charge invoice details are not equal to parent consol cost invoice details", mismatchedInfo, expectedMessage1, expectedMessage2, expectedMessage3, expectedMessage4));
			expectedMessage2 =
@"	IsInDb = False
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = False
	IsDeleting = False

Business Contexts = None

	Charge Code = N51N0S01T7, Invoice # = 444444, Invoice Date = 05-Jan-10 00:00:00, Currency = , OS Cost Amount = 0, GST is Overridden = Yes, OS GST Amount = 0, Exchange Rate = 0, Local Cost Amount = 0, Creditor = XVBQP68SIYXQ, PPDCLT = , Apportionment Method = CHG, Supplier Cost Reference = ABC, Payment Date = 10-Jan-10 00:00:00, Payment Type = , Cheque # = , Bank Account = 00000000-0000-0000-0000-000000000000, Cheque Book = 00000000-0000-0000-0000-000000000000, Tax Rate = 41312608-7ae1-4155-af94-0af99cdfdc97, Tax Date = , AR Invoice = 00000000-0000-0000-0000-000000000000, AP Invoice = 00000000-0000-0000-0000-000000000000, Is For Collect Invoice = N, Consol = 40b5446d-6066-4ed1-b9c1-8ca588a2a43d, Is Final = No, Tax Code = TAX1, Supply Type = , Tax Branch = .
Parent collections:

Charge with incorrect data:
Charge: PK = f8a8e869-ce2a-4049-a9df-d0b7a7e79f4e";
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("CheckChargeAndConsolCostInvoiceDetailsAreEqual, Invoice Number is not equal, No Link From Consol To Invoice", delegate(BusinessObjectFactory factory)
			{
				JobConsolCost consolCost = CreateChargeWithConsolCostAndAllCorrectInvoiceDetailsNotPosted(factory);
				consolCost.E6_InvoiceNum = "444444";
				return consolCost;
			}

			, true, CriticalValidationErrorType.ApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetails_8, "Apportion Split Charge invoice details are not equal to parent consol cost invoice details", expectedMessage1, expectedMessage2));
			mismatchedInfo = "Invoice Date:    05-Jan-2010 12:00:00.0000    |    18-Sep-1971 12:00:00.0000";
			expectedMessage2 =
@"	IsInDb = False
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = False
	IsDeleting = False

Business Contexts = None

	Charge Code = N51N0S01T7, Invoice # = 111, Invoice Date = 18-Sep-71 00:00:00, Currency = , OS Cost Amount = 0, GST is Overridden = Yes, OS GST Amount = 0, Exchange Rate = 0, Local Cost Amount = 0, Creditor = XVBQP68SIYXQ, PPDCLT = , Apportionment Method = CHG, Supplier Cost Reference = ABC, Payment Date = 18-Sep-71 00:00:00, Payment Type = , Cheque # = , Bank Account = 00000000-0000-0000-0000-000000000000, Cheque Book = 00000000-0000-0000-0000-000000000000, Tax Rate = 41312608-7ae1-4155-af94-0af99cdfdc97, Tax Date = , AR Invoice = 00000000-0000-0000-0000-000000000000, AP Invoice = 9670ef3f-c8ad-496d-a278-c40bfdfbba1e, Is For Collect Invoice = N, Consol = 40b5446d-6066-4ed1-b9c1-8ca588a2a43d, Is Final = No, Tax Code = TAX1, Supply Type = , Tax Branch = .
Parent collections:";
			expectedMessage3 = @"
Header: PK = ";
			expectedMessage4 = @"
Charge with incorrect data:
Charge: PK = f8a8e869-ce2a-4049-a9df-d0b7a7e79f4e";
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("CheckChargeAndConsolCostInvoiceDetailsAreEqual, Invoice Date is not equal", delegate(BusinessObjectFactory factory)
			{
				JobConsolCost consolCost = CreatePostedChargeWithConsolCostAndAllCorrectInvoiceDetails(factory);
				consolCost.E6_InvoiceDate = ZDateTime.BrettsBirthday;
				return consolCost;
			}

			, true, CriticalValidationErrorType.ApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetails_8, "Apportion Split Charge invoice details are not equal to parent consol cost invoice details", mismatchedInfo, expectedMessage1, expectedMessage2, expectedMessage3, expectedMessage4));
			mismatchedInfo = "Payment Date:    10-Jan-2010 12:00:00.0000    |    18-Sep-1971 12:00:00.0000";
			expectedMessage2 =
@"	IsInDb = False
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = False
	IsDeleting = False

Business Contexts = None

	Charge Code = N51N0S01T7, Invoice # = 111, Invoice Date = 05-Jan-10 00:00:00, Currency = , OS Cost Amount = 0, GST is Overridden = Yes, OS GST Amount = 0, Exchange Rate = 0, Local Cost Amount = 0, Creditor = XVBQP68SIYXQ, PPDCLT = , Apportionment Method = CHG, Supplier Cost Reference = ABC, Payment Date = 18-Sep-71 00:00:00, Payment Type = , Cheque # = , Bank Account = 00000000-0000-0000-0000-000000000000, Cheque Book = 00000000-0000-0000-0000-000000000000, Tax Rate = 41312608-7ae1-4155-af94-0af99cdfdc97, Tax Date = , AR Invoice = 00000000-0000-0000-0000-000000000000, AP Invoice = 9670ef3f-c8ad-496d-a278-c40bfdfbba1e, Is For Collect Invoice = N, Consol = 40b5446d-6066-4ed1-b9c1-8ca588a2a43d, Is Final = No, Tax Code = TAX1, Supply Type = , Tax Branch = .
Parent collections:";
			expectedMessage3 = @"
Header: PK = ";
			expectedMessage4 = @"
Charge with incorrect data:
Charge: PK = f8a8e869-ce2a-4049-a9df-d0b7a7e79f4e";
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("CheckChargeAndConsolCostInvoiceDetailsAreEqual, Payment Date is not equal", delegate(BusinessObjectFactory factory)
			{
				JobConsolCost consolCost = CreatePostedChargeWithConsolCostAndAllCorrectInvoiceDetails(factory);
				consolCost.E6_PaymentDate = ZDateTime.BrettsBirthday;
				return consolCost;
			}

			, true, CriticalValidationErrorType.ApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetails_8, "Apportion Split Charge invoice details are not equal to parent consol cost invoice details", mismatchedInfo, expectedMessage1, expectedMessage2, expectedMessage3, expectedMessage4));
			mismatchedInfo = "Creditor:    XVBQP68SIYXQ    |    TTORG";
			expectedMessage2 =
@"	IsInDb = False
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = False
	IsDeleting = False

Business Contexts = None

	Charge Code = N51N0S01T7, Invoice # = 111, Invoice Date = 05-Jan-10 00:00:00, Currency = , OS Cost Amount = 0, GST is Overridden = Yes, OS GST Amount = 0, Exchange Rate = 0, Local Cost Amount = 0, Creditor = TTORG, PPDCLT = , Apportionment Method = CHG, Supplier Cost Reference = ABC, Payment Date = 10-Jan-10 00:00:00, Payment Type = , Cheque # = , Bank Account = 00000000-0000-0000-0000-000000000000, Cheque Book = 00000000-0000-0000-0000-000000000000, Tax Rate = 00000000-0000-0000-0000-000000000000, Tax Date = , AR Invoice = 00000000-0000-0000-0000-000000000000, AP Invoice = 9670ef3f-c8ad-496d-a278-c40bfdfbba1e, Is For Collect Invoice = N, Consol = 40b5446d-6066-4ed1-b9c1-8ca588a2a43d, Is Final = No, Tax Code = , Supply Type = , Tax Branch = .
Parent collections:";
			expectedMessage3 = @"
Header: PK = ";
			expectedMessage4 = @"
Charge with incorrect data:
Charge: PK = f8a8e869-ce2a-4049-a9df-d0b7a7e79f4e";
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("CheckChargeAndConsolCostInvoiceDetailsAreEqual, Creditor is not equal", delegate(BusinessObjectFactory factory)
			{
				JobConsolCost consolCost = CreatePostedChargeWithConsolCostAndAllCorrectInvoiceDetails(factory);
				consolCost.E6_OH_Creditor = factory.NewWithValidTestData<OrgHeader>().PK;
				consolCost.Creditor.OH_Code = "TTORG";
				return consolCost;
			}

			, true, CriticalValidationErrorType.ApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetails_8, "Apportion Split Charge invoice details are not equal to parent consol cost invoice details", mismatchedInfo, expectedMessage1, expectedMessage2, expectedMessage3, expectedMessage4));
			mismatchedInfo = "Supplier Cost Reference:    ABC    |    XYZ";
			expectedMessage2 =
@"	IsInDb = False
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = False
	IsDeleting = False

Business Contexts = None

	Charge Code = N51N0S01T7, Invoice # = 111, Invoice Date = 05-Jan-10 00:00:00, Currency = , OS Cost Amount = 0, GST is Overridden = Yes, OS GST Amount = 0, Exchange Rate = 0, Local Cost Amount = 0, Creditor = XVBQP68SIYXQ, PPDCLT = , Apportionment Method = CHG, Supplier Cost Reference = XYZ, Payment Date = 10-Jan-10 00:00:00, Payment Type = , Cheque # = , Bank Account = 00000000-0000-0000-0000-000000000000, Cheque Book = 00000000-0000-0000-0000-000000000000, Tax Rate = 41312608-7ae1-4155-af94-0af99cdfdc97, Tax Date = , AR Invoice = 00000000-0000-0000-0000-000000000000, AP Invoice = 9670ef3f-c8ad-496d-a278-c40bfdfbba1e, Is For Collect Invoice = N, Consol = 40b5446d-6066-4ed1-b9c1-8ca588a2a43d, Is Final = No, Tax Code = TAX1, Supply Type = , Tax Branch = .
Parent collections:";
			expectedMessage3 = @"
Header: PK = ";
			expectedMessage4 = @"
Charge with incorrect data:
Charge: PK = f8a8e869-ce2a-4049-a9df-d0b7a7e79f4e";
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("CheckChargeAndConsolCostInvoiceDetailsAreEqual, Supplier Cost Reference is not equal", delegate(BusinessObjectFactory factory)
			{
				JobConsolCost consolCost = CreatePostedChargeWithConsolCostAndAllCorrectInvoiceDetails(factory);
				consolCost.E6_CostReference = "XYZ";
				return consolCost;
			}

			, true, CriticalValidationErrorType.ApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetails_8, "Apportion Split Charge invoice details are not equal to parent consol cost invoice details", mismatchedInfo, expectedMessage1, expectedMessage2, expectedMessage3, expectedMessage4));

			mismatchedInfo = "Supply Type:        |    LOX";
			expectedMessage2 =
@"	IsInDb = False
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = False
	IsDeleting = False

Business Contexts = None

	Charge Code = N51N0S01T7, Invoice # = 111, Invoice Date = 05-Jan-10 00:00:00, Currency = , OS Cost Amount = 0, GST is Overridden = Yes, OS GST Amount = 0, Exchange Rate = 0, Local Cost Amount = 0, Creditor = XVBQP68SIYXQ, PPDCLT = , Apportionment Method = CHG, Supplier Cost Reference = ABC, Payment Date = 10-Jan-10 00:00:00, Payment Type = , Cheque # = , Bank Account = 00000000-0000-0000-0000-000000000000, Cheque Book = 00000000-0000-0000-0000-000000000000, Tax Rate = 00000000-0000-0000-0000-000000000000, Tax Date = , AR Invoice = 00000000-0000-0000-0000-000000000000, AP Invoice = 9670ef3f-c8ad-496d-a278-c40bfdfbba1e, Is For Collect Invoice = N, Consol = 40b5446d-6066-4ed1-b9c1-8ca588a2a43d, Is Final = No, Tax Code = , Supply Type = LOX, Tax Branch = .
Parent collections:";
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("CheckChargeAndConsolCostInvoiceDetailsAreEqual, Supply Type is not equal", delegate(BusinessObjectFactory factory)
			{
				JobConsolCost consolCost = CreatePostedChargeWithConsolCostAndAllCorrectInvoiceDetails(factory);
				consolCost.E6_SupplyType = "LOX";
				return consolCost;
			}

			, true, CriticalValidationErrorType.ApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetails_8, "Apportion Split Charge invoice details are not equal to parent consol cost invoice details", mismatchedInfo, expectedMessage1, expectedMessage2, expectedMessage3, expectedMessage4));

			mismatchedInfo = "Tax Branch:    <empty>    |    TB1";
			expectedMessage2 =
@"	IsInDb = False
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = False
	IsDeleting = False

Business Contexts = None

	Charge Code = N51N0S01T7, Invoice # = 111, Invoice Date = 05-Jan-10 00:00:00, Currency = , OS Cost Amount = 0, GST is Overridden = Yes, OS GST Amount = 0, Exchange Rate = 0, Local Cost Amount = 0, Creditor = XVBQP68SIYXQ, PPDCLT = , Apportionment Method = CHG, Supplier Cost Reference = ABC, Payment Date = 10-Jan-10 00:00:00, Payment Type = , Cheque # = , Bank Account = 00000000-0000-0000-0000-000000000000, Cheque Book = 00000000-0000-0000-0000-000000000000, Tax Rate = 00000000-0000-0000-0000-000000000000, Tax Date = , AR Invoice = 00000000-0000-0000-0000-000000000000, AP Invoice = 9670ef3f-c8ad-496d-a278-c40bfdfbba1e, Is For Collect Invoice = N, Consol = 40b5446d-6066-4ed1-b9c1-8ca588a2a43d, Is Final = No, Tax Code = , Supply Type = , Tax Branch = TB1.
Parent collections:";
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("CheckChargeAndConsolCostInvoiceDetailsAreEqual, Tax Branch is not equal", delegate(BusinessObjectFactory factory)
			{
				JobConsolCost consolCost = CreatePostedChargeWithConsolCostAndAllCorrectInvoiceDetails(factory);
				consolCost.E6_GB_CostTaxBranch = factory.NewWithValidTestData<GlbBranch>().PK;
				consolCost.CostTaxBranch.GB_Code = "TB1";
				consolCost.E6_AT_TaxRate = ZGuid.Empty;
				return consolCost;
			}

			, true, CriticalValidationErrorType.ApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetails_8, "Apportion Split Charge invoice details are not equal to parent consol cost invoice details", mismatchedInfo, expectedMessage1, expectedMessage2, expectedMessage3, expectedMessage4));

			expectedMessage2 =
@"	IsInDb = False
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = False
	IsDeleting = False

Business Contexts = None

	Charge Code = N51N0S01T7, Invoice # = 111, Invoice Date = 05-Jan-10 00:00:00, Currency = , OS Cost Amount = 0, GST is Overridden = Yes, OS GST Amount = 0, Exchange Rate = 0, Local Cost Amount = 0, Creditor = TTORG, PPDCLT = , Apportionment Method = CHG, Supplier Cost Reference = ABC, Payment Date = 10-Jan-10 00:00:00, Payment Type = , Cheque # = , Bank Account = 00000000-0000-0000-0000-000000000000, Cheque Book = 00000000-0000-0000-0000-000000000000, Tax Rate = 00000000-0000-0000-0000-000000000000, Tax Date = , AR Invoice = 00000000-0000-0000-0000-000000000000, AP Invoice = 9670ef3f-c8ad-496d-a278-c40bfdfbba1e, Is For Collect Invoice = N, Consol = 40b5446d-6066-4ed1-b9c1-8ca588a2a43d, Is Final = No, Tax Code = , Supply Type = , Tax Branch = .
Parent collections:";
			expectedMessage3 = @"
Header: PK = ";
			expectedMessage4 = @"
Charge with incorrect data:
Charge: PK = f8a8e869-ce2a-4049-a9df-d0b7a7e79f4e";
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("CheckUnpostedChargeAndConsolCostInvoiceDetailsAreEqual, Creditor is not equal", delegate(BusinessObjectFactory factory)
			{
				JobConsolCost consolCost = CreatePostedChargeWithConsolCostAndAllCorrectInvoiceDetails(factory);
				consolCost.E6_OH_Creditor = factory.NewWithValidTestData<OrgHeader>().PK;
				consolCost.Creditor.OH_Code = "TTORG";
				return consolCost;
			}

			, true, CriticalValidationErrorType.ApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetails_8, "Apportion Split Charge invoice details are not equal to parent consol cost invoice details", expectedMessage1, expectedMessage2, expectedMessage3, expectedMessage4));
			mismatchedInfo = "GST Rate:    TAX1    |    TAX2";
			expectedMessage2 =
@"	IsInDb = False
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = False
	IsDeleting = False

Business Contexts = None

	Charge Code = N51N0S01T7, Invoice # = 111, Invoice Date = 05-Jan-10 00:00:00, Currency = , OS Cost Amount = 0, GST is Overridden = Yes, OS GST Amount = 0, Exchange Rate = 0, Local Cost Amount = 0, Creditor = XVBQP68SIYXQ, PPDCLT = , Apportionment Method = CHG, Supplier Cost Reference = ABC, Payment Date = 10-Jan-10 00:00:00, Payment Type = , Cheque # = , Bank Account = 00000000-0000-0000-0000-000000000000, Cheque Book = 00000000-0000-0000-0000-000000000000, Tax Rate = 6a23b2be-f344-4bd9-b5b8-55d0a12516a0, Tax Date = , AR Invoice = 00000000-0000-0000-0000-000000000000, AP Invoice = 9670ef3f-c8ad-496d-a278-c40bfdfbba1e, Is For Collect Invoice = N, Consol = 40b5446d-6066-4ed1-b9c1-8ca588a2a43d, Is Final = No, Tax Code = TAX2, Supply Type = , Tax Branch = .
Parent collections:";
			expectedMessage3 = @"
Header: PK = ";
			expectedMessage4 = @"
Charge with incorrect data:
Charge: PK = f8a8e869-ce2a-4049-a9df-d0b7a7e79f4e";
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("CheckUnpostedChargeAndConsolCostInvoiceDetailsAreEqual, Tax code is not equal", delegate(BusinessObjectFactory factory)
			{
				JobConsolCost consolCost = CreatePostedChargeWithConsolCostAndAllCorrectInvoiceDetails(factory);
				consolCost.E6_AT_TaxRate = factory.New(typeof(AccTaxRate), new Guid("6a23b2be-f344-4bd9-b5b8-55d0a12516a0")).PK;
				consolCost.TaxRate.AT_Code = "TAX2";
				return consolCost;
			}

			, true, CriticalValidationErrorType.ApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetails_8, "Apportion Split Charge invoice details are not equal to parent consol cost invoice details", mismatchedInfo, expectedMessage1, expectedMessage2, expectedMessage3, expectedMessage4));
			mismatchedInfo = "Tax Class:    MSG1    |    MSG2";
			expectedMessage2 =
@"	IsInDb = False
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = False
	IsDeleting = False

Business Contexts = None

	Charge Code = N51N0S01T7, Invoice # = 111, Invoice Date = 05-Jan-10 00:00:00, Currency = , OS Cost Amount = 0, GST is Overridden = Yes, OS GST Amount = 0, Exchange Rate = 0, Local Cost Amount = 0, Creditor = XVBQP68SIYXQ, PPDCLT = , Apportionment Method = CHG, Supplier Cost Reference = ABC, Payment Date = 10-Jan-10 00:00:00, Payment Type = , Cheque # = , Bank Account = 00000000-0000-0000-0000-000000000000, Cheque Book = 00000000-0000-0000-0000-000000000000, Tax Rate = 41312608-7ae1-4155-af94-0af99cdfdc97, Tax Date = , AR Invoice = 00000000-0000-0000-0000-000000000000, AP Invoice = 9670ef3f-c8ad-496d-a278-c40bfdfbba1e, Is For Collect Invoice = N, Consol = 40b5446d-6066-4ed1-b9c1-8ca588a2a43d, Is Final = No, Tax Code = TAX1, Supply Type = , Tax Branch = .
Parent collections:";
			expectedMessage3 = @"
Header: PK = ";
			expectedMessage4 = @"
Charge with incorrect data:
Charge: PK = f8a8e869-ce2a-4049-a9df-d0b7a7e79f4e";
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("CheckUnpostedChargeAndConsolCostInvoiceDetailsAreEqual, Tax class is not equal", delegate(BusinessObjectFactory factory)
			{
				JobConsolCost consolCost = CreatePostedChargeWithConsolCostAndAllCorrectInvoiceDetails(factory);
				consolCost.E6_A9_VATClass = factory.NewWithValidTestData<AccInvMsg>().PK;
				consolCost.VATClass.A9_Code = "MSG2";
				return consolCost;
			}

			, true, CriticalValidationErrorType.ApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetails_8, "Apportion Split Charge invoice details are not equal to parent consol cost invoice details", mismatchedInfo, expectedMessage1, expectedMessage2, expectedMessage3, expectedMessage4));

			expectedMessage2 = @"You have entered 4 decimal places for the local cost amount. The AUD currency only allows entering amounts up to 2 decimal places.
   at System.Environment.GetStackTrace";
			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("The decimal places of local cost amount in consol cost exceeds the maximum", delegate(BusinessObjectFactory factory)
			{
				JobConsolCost consolCost = CreatePostedChargeWithConsolCostAndAllCorrectInvoiceDetails(factory);
				consolCost.E6_LocalCostAmount = 1.0001m;
				return consolCost;
			}

			, true, CriticalValidationErrorType.LocalCostAmountNotEqualToSumOfApportionmentsLocalCostAmount_6, "Local cost amount is not equal to the sum of the apportionment's local cost amount", expectedMessage1, expectedMessage2));

			return result;
		}

		public void TestPostedConsolCostShouldHaveCostPostedApportionmentCharges()
		{
			JobConsolCost consolCost = CreateChargeWithConsolCostAndAllCorrectInvoiceDetailsNotPosted(Factory);
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckPostedConsolCostShouldHaveCostPostedApportionmentCharges: Not posted");
			AssertOnSavingCheck(consolCost, testCase);
			consolCost.E6_AH_APInvoice = new ZGuid("9670ef3f-c8ad-496d-a278-c40bfdfbba1e");
			var expectedMessage = string.Format(CultureInfo.InvariantCulture,
@"Job Consol Cost:
	PK = a12986b4-036c-4cb2-a0e7-0c96438a46dd
	Type = JobConsolCost
	Types around row = JobConsolCost
	Factory Instance = {0}
	IsDeleted = False
	IsInDb = False
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = False
	IsDeleting = False

Business Contexts = None

	Charge Code = N51N0S01T7, Invoice # = 111, Invoice Date = 05-Jan-10 00:00:00, Currency = , OS Cost Amount = 0, GST is Overridden = Yes, OS GST Amount = 0, Exchange Rate = 0, Local Cost Amount = 0, Creditor = XVBQP68SIYXQ, PPDCLT = , Apportionment Method = CHG, Supplier Cost Reference = ABC, Payment Date = 10-Jan-10 00:00:00, Payment Type = , Cheque # = , Bank Account = 00000000-0000-0000-0000-000000000000, Cheque Book = 00000000-0000-0000-0000-000000000000, Tax Rate = 41312608-7ae1-4155-af94-0af99cdfdc97, Tax Date = , AR Invoice = 00000000-0000-0000-0000-000000000000, AP Invoice = 9670ef3f-c8ad-496d-a278-c40bfdfbba1e, Is For Collect Invoice = N, Consol = 40b5446d-6066-4ed1-b9c1-8ca588a2a43d, Is Final = No, Tax Code = TAX1, Supply Type = , Tax Branch = .
Parent collections:
<Posted transaction is not found>

Charge with incorrect data:
Charge: PK = f8a8e869-ce2a-4049-a9df-d0b7a7e79f4e", consolCost.Factory._Instance);
			testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckPostedConsolCostShouldHaveCostPostedApportionmentCharges: Consol Cost posted", true, CriticalValidationErrorType.PostedConsolCostWithCostUnpostedApportionmentCharge_4, "Non Cost posted Apportion Split Charge is linked to posted Consol Cost", expectedMessage);
			AssertOnSavingCheck(consolCost, testCase);
			testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckPostedConsolCostShouldHaveCostPostedApportionmentCharges: Consol Cost posted with AutoJRJ");
			var revJournal = Factory.NewWithValidTestData<JobRevenueJournal>();
			var revOnAP = revJournal.JournalLines.AddNew();
			Assert("Is Revenue line", AccTransactionLines.RevenueLineTypes.Contains(revOnAP.AL_LineType));
			var charge = Factory.LoadTop1<Charge>(new ZQuery(ZArchitecture.Schema.JobChargeSchema.JR_E6, consolCost.PK));
			AssertNotNull("Has Apportionment  Charges", charge);
			charge.JR_AL_APLine = revOnAP.PK;
			Assert("Is JRJ", charge.IsCostPostedWithJobRevenueJournal);
			consolCost.E6_AH_APInvoice = revOnAP.PK;
			AssertOnSavingCheck(consolCost, testCase);
		}

		public void TestPostedConsolCostShouldHaveCostPostedApportionmentChargesWithReloadedObjectsInfo()
		{
			var consolCost = CreateChargeWithConsolCostAndAllCorrectInvoiceDetailsNotPosted(Factory);
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckPostedConsolCostShouldHaveCostPostedApportionmentCharges: Not posted");
			AssertOnSavingCheck(consolCost, testCase);
			Factory.Save();
			consolCost.E6_AH_APInvoice = new ZGuid("9670ef3f-c8ad-496d-a278-c40bfdfbba1e");
			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var consolCostInNewFactory = newFactory.Load<JobConsolCost>(consolCost.PK);
			consolCostInNewFactory.E6_InvoiceNum = "NewE6_InvoiceNum001";
			newFactory.Save();
			var expectedMessage1 = string.Format(CultureInfo.InvariantCulture,
@"Job Consol Cost:
	PK = a12986b4-036c-4cb2-a0e7-0c96438a46dd
	Type = JobConsolCost
	Types around row = JobConsolCost
	Factory Instance = {0}
	IsDeleted = False
	IsInDb = True
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = False
	IsDeleting = False

Business Contexts = None

	Charge Code = N51N0S01T7, Invoice # = 111, Invoice Date = 05-Jan-10 00:00:00, Currency = , OS Cost Amount = 0, GST is Overridden = Yes, OS GST Amount = 0, Exchange Rate = 0, Local Cost Amount = 0, Creditor = XVBQP68SIYXQ, PPDCLT = , Apportionment Method = CHG, Supplier Cost Reference = ABC, Payment Date = 10-Jan-10 00:00:00, Payment Type = , Cheque # = , Bank Account = 00000000-0000-0000-0000-000000000000, Cheque Book = 00000000-0000-0000-0000-000000000000, Tax Rate = 41312608-7ae1-4155-af94-0af99cdfdc97, Tax Date = , AR Invoice = 00000000-0000-0000-0000-000000000000, AP Invoice = 9670ef3f-c8ad-496d-a278-c40bfdfbba1e, Is For Collect Invoice = N, Consol = 40b5446d-6066-4ed1-b9c1-8ca588a2a43d, Is Final = No, Tax Code = TAX1, Supply Type = , Tax Branch = .
	Fields with changes: E6_AH_APInvoice (00000000-0000-0000-0000-000000000000, 9670ef3f-c8ad-496d-a278-c40bfdfbba1e), E6_RatingBehaviour (NEW, STP).
Parent collections:
", Factory._Instance);

			var expectedChargeMessage1 = @"Charge with incorrect data:
Charge: PK = f8a8e869-ce2a-4049-a9df-d0b7a7e79f4e";

			var expectedConsolCostMessage = @"JobConsolCost Reloaded:
Job Consol Cost:
	PK = a12986b4-036c-4cb2-a0e7-0c96438a46dd
	Type = JobConsolCost";

			var expectedMessage2 =
@"	IsInDb = True
	IsSavedByFactory = False
	HasChanges = False
	HasErrors = False
	IsDeleting = False

Business Contexts = None

	Charge Code = N51N0S01T7, Invoice # = NewE6_InvoiceNum001, Invoice Date = 05-Jan-10 00:00:00, Currency = , OS Cost Amount = 0.0000, GST is Overridden = Yes, OS GST Amount = 0.0000, Exchange Rate = 0.000000000, Local Cost Amount = 0.0000, Creditor = XVBQP68SIYXQ, PPDCLT = , Apportionment Method = CHG, Supplier Cost Reference = ABC, Payment Date = 10-Jan-10 00:00:00, Payment Type = , Cheque # = , Bank Account = 00000000-0000-0000-0000-000000000000, Cheque Book = 00000000-0000-0000-0000-000000000000, Tax Rate = 41312608-7ae1-4155-af94-0af99cdfdc97, Tax Date = , AR Invoice = 00000000-0000-0000-0000-000000000000, AP Invoice = 00000000-0000-0000-0000-000000000000, Is For Collect Invoice = N, Consol = 40b5446d-6066-4ed1-b9c1-8ca588a2a43d, Is Final = No, Tax Code = TAX1, Supply Type = , Tax Branch = .
Parent collections:";

			var expectedChargeMessage2 = @"Apportionment Charges (1):
Charge: PK = f8a8e869-ce2a-4049-a9df-d0b7a7e79f4e";

			var expectedChargeMessage3 = @"Problem Charge Reloaded:
Charge: PK = f8a8e869-ce2a-4049-a9df-d0b7a7e79f4e";
			testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckPostedConsolCostShouldHaveCostPostedApportionmentCharges: Consol Cost posted", true, CriticalValidationErrorType.PostedConsolCostWithCostUnpostedApportionmentCharge_4, "Non Cost posted Apportion Split Charge is linked to posted Consol Cost", expectedMessage1, expectedChargeMessage1, expectedConsolCostMessage, expectedMessage2, expectedChargeMessage2, expectedChargeMessage3);
			AssertOnSavingCheck(consolCost, testCase);
		}

		public void TestPostedConsolCostShouldHaveApportionmentChargesCostPostedToTHeSameInvoice()
		{
			JobConsolCost consolCost = CreatePostedChargeWithConsolCostAndAllCorrectInvoiceDetails(Factory);
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("UnpostedConsolCostShouldNotHaveCostPostedApportionmentCharges: All posted");
			AssertOnSavingCheck(consolCost, testCase);
			consolCost.E6_AH_APInvoice = new ZGuid("6054c5d6-1960-434a-9478-4b279598d053");
			var expectedMessage = string.Format(CultureInfo.InvariantCulture,
@"Job Consol Cost:
	PK = a12986b4-036c-4cb2-a0e7-0c96438a46dd
	Type = JobConsolCost
	Types around row = JobConsolCost
	Factory Instance = {0}
	IsDeleted = False
	IsInDb = False
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = False
	IsDeleting = False

Business Contexts = None

	Charge Code = N51N0S01T7, Invoice # = 111, Invoice Date = 05-Jan-10 00:00:00, Currency = , OS Cost Amount = 0, GST is Overridden = Yes, OS GST Amount = 0, Exchange Rate = 0, Local Cost Amount = 0, Creditor = XVBQP68SIYXQ, PPDCLT = , Apportionment Method = CHG, Supplier Cost Reference = ABC, Payment Date = 10-Jan-10 00:00:00, Payment Type = , Cheque # = , Bank Account = 00000000-0000-0000-0000-000000000000, Cheque Book = 00000000-0000-0000-0000-000000000000, Tax Rate = 41312608-7ae1-4155-af94-0af99cdfdc97, Tax Date = , AR Invoice = 00000000-0000-0000-0000-000000000000, AP Invoice = 6054c5d6-1960-434a-9478-4b279598d053, Is For Collect Invoice = N, Consol = 40b5446d-6066-4ed1-b9c1-8ca588a2a43d, Is Final = No, Tax Code = TAX1, Supply Type = , Tax Branch = .
Parent collections:
<Posted transaction is not found>

Charge with incorrect data:
Charge: PK = f8a8e869-ce2a-4049-a9df-d0b7a7e79f4e", consolCost.Factory._Instance);
			testCase = new TestCaseDefinition_ForSeparateTestsMethods("CheckPostedConsolCostShouldHaveCostPostedApportionmentCharges: Consol Cost posted", true, CriticalValidationErrorType.PostedConsolCostWithApportionmentChargePostedToDifferentInvoice_4, "Consol Cost and linked Apportion Split Charge are posted to different AP Invoices", expectedMessage);
			AssertOnSavingCheck(consolCost, testCase);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1052:Do Not Cast Factory Method", Justification = "Baseline")]
		public void TestPostedConsolCostShouldHaveApportionmentChargesCostPostedToTheSameInvoiceForJRJ()
		{
			var consolCost = CreateChargeWithConsolCostAndAllCorrectInvoiceDetailsNotPosted(Factory);
			consolCost.E6_AH_APInvoice = new ZGuid("6054c5d6-1960-434a-9478-4b279598d053");
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("UnpostedConsolCostShouldNotHaveCostPostedApportionmentCharges: All posted");
			var revJournal = Factory.NewWithValidTestData<JobRevenueJournal>();
			var revOnAP = revJournal.JournalLines.AddNew();

			Assert("Is Revenue line", AccTransactionLines.RevenueLineTypes.Contains(revOnAP.AL_LineType));

			var charge = Factory.LoadTop1<Charge>(new ZQuery(ZArchitecture.Schema.JobChargeSchema.JR_E6, consolCost.PK));

			AssertNotNull("Has Apportionment  Charges", charge);
			charge.JR_AL_APLine = revOnAP.PK;

			var charge2 = (BaseCharge)Factory.New(typeof(BaseCharge), Guid.NewGuid());
			charge2.FillWithValidTestData();

			var job = Factory.NewJobWithPrimaryKeyForTesting<Job>(Guid.NewGuid());
			job.FillWithValidTestData();
			charge2.JR_JH = job.PK;
			charge2.JR_E6 = consolCost.PK;
			charge2.JR_APInvoiceNum = "111";
			charge2.JR_APInvoiceDate = new ZDateTime(2010, 01, 05);
			charge2.JR_OH_CostAccount = charge.JR_OH_CostAccount;
			charge2.JR_PaymentDate = new ZDateTime(2010, 01, 10);
			charge2.JR_CostReference = "ABC";
			charge2.JR_AT_CostGSTRate =	charge.JR_AT_CostGSTRate;
			charge2.JR_A9_CostVATClass = charge.JR_A9_CostVATClass;

			var invoiceLine = (APInvoiceLine)Factory.New(typeof(APInvoiceLine), Guid.NewGuid());
			invoiceLine.AL_LineType = TransactionLineTypes.Cost;
			invoiceLine.AL_AH = Guid.NewGuid();
			charge2.JR_AL_APLine = invoiceLine.PK;

			AssertOnSavingCheck(consolCost, testCase);
		}

		public void TestUnpostedConsolCostShouldNotHaveCostPostedApportionmentCharges()
		{
			JobConsolCost consolCost = CreatePostedChargeWithConsolCostAndAllCorrectInvoiceDetails(Factory);
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("UnpostedConsolCostShouldNotHaveCostPostedApportionmentCharges: All posted");
			AssertOnSavingCheck(consolCost, testCase);
			consolCost.E6_AH_APInvoice = ZGuid.Empty;
			consolCost.E6_IsTaxAmountOverridden = true;
			var expectedMessage = string.Format(CultureInfo.InvariantCulture,
@"Job Consol Cost:
	PK = a12986b4-036c-4cb2-a0e7-0c96438a46dd
	Type = JobConsolCost
	Types around row = JobConsolCost
	Factory Instance = {0}
	IsDeleted = False
	IsInDb = False
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = False
	IsDeleting = False

Business Contexts = None

	Charge Code = N51N0S01T7, Invoice # = 111, Invoice Date = 05-Jan-10 00:00:00, Currency = , OS Cost Amount = 0, GST is Overridden = Yes, OS GST Amount = 0, Exchange Rate = 0, Local Cost Amount = 0, Creditor = XVBQP68SIYXQ, PPDCLT = , Apportionment Method = CHG, Supplier Cost Reference = ABC, Payment Date = 10-Jan-10 00:00:00, Payment Type = , Cheque # = , Bank Account = 00000000-0000-0000-0000-000000000000, Cheque Book = 00000000-0000-0000-0000-000000000000, Tax Rate = 41312608-7ae1-4155-af94-0af99cdfdc97, Tax Date = , AR Invoice = 00000000-0000-0000-0000-000000000000, AP Invoice = 00000000-0000-0000-0000-000000000000, Is For Collect Invoice = N, Consol = 40b5446d-6066-4ed1-b9c1-8ca588a2a43d, Is Final = No, Tax Code = TAX1, Supply Type = , Tax Branch = .
Parent collections:

Charge with incorrect data:
Charge: PK = f8a8e869-ce2a-4049-a9df-d0b7a7e79f4e", consolCost.Factory._Instance);
			testCase = new TestCaseDefinition_ForSeparateTestsMethods("UnpostedConsolCostShouldNotHaveCostPostedApportionmentCharges: Consol Cost not posted", true, CriticalValidationErrorType.UnpostedConsolCostWithCostPostedApportionmentCharge_3, "Cost posted Apportion Split Charge is linked to unposted Consol Cost", expectedMessage);
			AssertOnSavingCheck(consolCost, testCase);
		}

		public void TestLocalCostAmountNotEqualToSumOfApportionmentsLocalCostAmountReportsExtraInfo()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.PushToGreatestChargeExchangeRateDifferences_BeforeMethodCall);
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.PushToGreatestChargeExchangeRateDifferences_AfterMethodCall);
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.LocalCostAmountChangedThatCausedUnApportionedAmount);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ActualWeight = 351.73m;
			shipment1.JS_ActualChargeable = 43m;
			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_ActualWeight = 786.23m;
			shipment2.JS_ActualChargeable = 5m;

			var apps = new ApportionmentListing(Factory, consol);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT, TestObjectCreator.AALSHI, apps);

			try
			{
				var charge1 = consolCost.ApportionmentCharges[0];
				consolCost.E6_OSCostAmount = 1840m;

				charge1.JR_LocalCostAmt += 4;

				var expectedMessage1 =
@"
PushToGreatestChargeExchangeRateDifferences_BeforeMethodCall:
E6_OSCostAmount: 1840, E6_LocalCostAmount: 1840, currency: AUD, local currency: AUD, exchange rate: 1
Amounts on charges (JR_IsUsedForApportionment, JR_OSCostAmt, JR_LocalCostAmt):
Before push: (Y, 1648.33, 1648.33), (Y, 191.67, 191.67)


PushToGreatestChargeExchangeRateDifferences_AfterMethodCall:
After  push: (Y, 1648.33, 1648.33), (Y, 191.67, 191.67)
   at Enterprise";

				var expectedMessage2 = $@"LocalCostAmountChangedThatCausedUnApportionedAmount:
Local Cost Amount is changed from 1648.33 to 1652.33 of charge with PK: {charge1.PK}.";

				var expectedMessage3 = $@"Charge: PK = {charge1.PK}";

				var expectedMessage4 = $@"Charge: PK = {consolCost.ApportionmentCharges[1].PK}";

				var expectedMessage5 = "Stacktrace -->";

				var testCase = new TestCaseDefinition_ForSeparateTestsMethods(CriticalValidationMessageTemplate.LocalCostAmountNotEqualToSumOfApportionmentsLocalCostAmountErrorMessage,
					true,
					CriticalValidationErrorType.LocalCostAmountNotEqualToSumOfApportionmentsLocalCostAmount_6,
					"Local cost amount is not equal to the sum of the apportionment's local cost amount.",
					expectedMessage1,
					expectedMessage2,
					expectedMessage3,
					expectedMessage4,
					expectedMessage5);
				AssertOnSavingCheck(consolCost, testCase);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestOverseasCostTaxAmountNotEqualToSumOfApportionmentsOverseasCostTaxAmountInfo()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.PushUnApportionedGSTAmountBasedOnRepresentation_BeforeMethodCall);
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.PushUnApportionedGSTAmountBasedOnRepresentation_AfterMethodCall);

			var consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ActualWeight = 351.73m;
			shipment1.JS_ActualChargeable = 43m;
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_ActualWeight = 786.23m;
			shipment2.JS_ActualChargeable = 5m;

			var apps = new ApportionmentListing(Factory, consol);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT, TestObjectCreator.AALSHI, apps);

			try
			{
				var charge1 = consolCost.ApportionmentCharges[0];
				consolCost.E6_OSCostAmount = 1840m;
				consolCost.E6_IsTaxAmountOverridden = true;
				consolCost.E6_OSGSTAmount_Calc = 184m;

				charge1.JR_OSCostGSTAmt += 4;

				var expectedMessage = @"
PushUnApportionedGSTAmountBasedOnRepresentation_BeforeMethodCall:
E6_OSCostAmount: 1840, E6_OSGSTAmountOverride: 184
Amounts on charges (JR_OSCostAmt, JR_OSCostGSTAmtOverride):
Before push: (1648.33, 164.83), (191.67, 19.17)


PushUnApportionedGSTAmountBasedOnRepresentation_AfterMethodCall:
After  push: (1648.33, 164.83), (191.67, 19.17)
   at Enterprise";

				var testCase = new TestCaseDefinition_ForSeparateTestsMethods(CriticalValidationMessageTemplate.OverseasCostTaxAmountNotEqualToSumOfApportionmentsOverseasCostTaxAmountErrorMessage,
					true,
					CriticalValidationErrorType.OverseasCostTaxAmountNotEqualToSumOfApportionmentsOverseasCostTaxAmount_11,
					"Overseas cost tax amount is not equal to the sum of the apportionment's Overseas cost tax amount.",
					expectedMessage);
				AssertOnSavingCheck(consolCost, testCase);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestExtendMsgForGetConsolInfo()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "test000001");
			var shipment = consol.Shipments.AddNew();
			var job = TestObjectCreator.CreateJobHeader();
			job.JH_ParentID = shipment.PK;
			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("AP100001", TestObjectCreator.AUD, 1.0m, 250m, 25m, 0m, 250m, 25m, 0m, TestObjectCreator.Creditor1);
			invoice.AH_JH = job.PK;
			var cost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			cost.E6_ApportionmentMethod = "CHG";
			cost.E6_OSCostAmount = 10;
			cost.E6_InvoiceDate = new ZDateTime(2015, 10, 18);
			cost.E6_PaymentDate = new ZDateTime(2015, 10, 18);
			invoice.ImportAllApportionmentsFromCosting();
			cost.ApportionmentCharges[0].JR_OSCostAmt = 5;
			string expectedMessage = string.Format(CultureInfo.InvariantCulture,
@"Job Consol Cost:
	PK = {2}
	Type = JobConsolCost
	Types around row = JobConsolCost
	Factory Instance = {0}
	IsDeleted = False
	IsInDb = False
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = True
	IsDeleting = False

Business Contexts = BizObj Level : (EnableDirectSettingConsolCostParent)

	Charge Code = , Invoice # = AP100001, Invoice Date = 18-Oct-15 00:00:00, Currency = AUD, OS Cost Amount = 10, GST is Overridden = Yes, OS GST Amount = 0, Exchange Rate = 1, Local Cost Amount = 10, Creditor = ZCreditor1, PPDCLT = ALL, Apportionment Method = CHG, Supplier Cost Reference = , Payment Date = 18-Oct-15 00:00:00, Payment Type = , Cheque # = , Bank Account = 00000000-0000-0000-0000-000000000000, Cheque Book = 00000000-0000-0000-0000-000000000000, Tax Rate = 00000000-0000-0000-0000-000000000000, Tax Date = , AR Invoice = 00000000-0000-0000-0000-000000000000, AP Invoice = 00000000-0000-0000-0000-000000000000, Is For Collect Invoice = N, Consol = {4}, Is Final = Yes, Tax Code = , Supply Type = , Tax Branch = .
Parent collections:
BusinessObjectCollection Info:
	Collection Type = Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceConsolCostCollection
	Element Type = Enterprise.Accounting.Business.ConsolCosting.JobConsolCost
	Factory Instance = {0}
	Hash Code = {1}
	Contains bizo = True
	Has Changes = True
	Number of elements = 1
	Is List Changed Suspended = False
	Has Changes From Delete = False
	Masters Are Deleted = False
	Masters Are In Database  = True
Apportionment Charges (1):
Charge: PK = {3}", Factory._Instance, invoice.ConsolCosting.ConsolCosts.GetHashCode(), cost.PK, cost.ApportionmentCharges[0].PK, consol.PK);

			var expectedMessage2 = "JR_OSCostAmt Stack Trace = ";
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods(CriticalValidationMessageTemplate.OverseasCostAmountNotEqualToSumOfApportionmentsOverseasCostAmountErrorMessage, true, CriticalValidationErrorType.OverseasCostAmountNotEqualToSumOfApportionmentsOverseasCostAmount_9, "Overseas cost amount is not equal to the sum of the apportionment's overseas cost amount.", expectedMessage, expectedMessage2);
			AssertOnSavingCheck(cost, testCase);
		}

		public void TestOverseasCostAmountNotEqualToSumOfApportionmentsOverseasCostAmountErrorMessageInfoForXmlImport()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment = consol.Shipments.AddNew();
			var job = TestObjectCreator.CreateJobHeader();
			job.JH_ParentID = shipment.PK;

			Factory.Save();

			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("AP100001", TestObjectCreator.AUD, 1.0m, 10m, 0m, 0m, 10m, 0m, 0m, TestObjectCreator.Creditor1);
			invoice.AH_JH = job.PK;

			var cost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			cost.E6_OSCostAmount = 10;
			cost.E6_InvoiceDate = new ZDateTime(2015, 10, 18);
			cost.E6_PaymentDate = new ZDateTime(2015, 10, 18);
			cost.E6_AH_APInvoice = invoice.PK;
			invoice.ImportAllApportionmentsFromCosting();

			CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(cost.PK, CriticalValidationInfoCollectorServiceKeyType.JobConsolCostOSCostAmountChanged);
			CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(cost.ApportionmentCharges[0].PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeOSCostAmountChanged);
			CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(invoice.Lines[0].PK, CriticalValidationInfoCollectorServiceKeyType.TransactionLineAmountChangeWhenImportInvoice);

			Factory.SetContext(Enterprise.Integration.Accounting.BusinessContext.AllowReopenJobWhenImporting);

			cost.E6_OSCostAmount++;
			cost.ApportionmentCharges[0].JR_OSCostAmt++;
			invoice.Lines[0].AL_LineAmount++;
			cost.ApportionmentCharges[0].JR_AL_APLine = invoice.Lines[0].PK;

			string expectedMessageMainError = @"Overseas cost amount is not equal to the sum of the apportionment's overseas cost amount.

11 != 12";

			string expectedMessageConsolCostError = @"JobConsolCostOSCostAmountChanged:
E6_OSCostAmount (Old: 10, New: 11), Unapportioned: 0

   at System.Environment.GetStackTrace";

			string expectedMessageConsolCostInfo = FormattableString.Invariant($@"Job Consol Cost:
	PK = {cost.PK}
	Type = JobConsolCost
	Types around row = JobConsolCost
	Factory Instance = {Factory._Instance}
	IsDeleted = False
	IsInDb = False
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = True
	IsDeleting = False");

			string expectedMessageConsolCostCollectionInfo = FormattableString.Invariant($@"Parent collections:
BusinessObjectCollection Info:
	Collection Type = Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceConsolCostCollection
	Element Type = Enterprise.Accounting.Business.ConsolCosting.JobConsolCost
	Factory Instance = {Factory._Instance}
	Hash Code = {invoice.ConsolCosting.ConsolCosts.GetHashCode()}
	Contains bizo = True
	Has Changes = True
	Number of elements = 1
	Is List Changed Suspended = False
	Has Changes From Delete = False
	Masters Are Deleted = False
	Masters Are In Database  = True
Apportionment Charges (1):
Charge: PK = {cost.ApportionmentCharges[0].PK}");

			var expectedMessageJobChargeError = @"JR_OSCostAmt Stack Trace = 
JobChargeOSCostAmountChanged:
JR_OSCostAmt: 12, JR_OSCostAmt old value: 11
   at System.Environment.GetStackTrace";

			var expectedMessageLineError = @"Relative AP Line construction stack trace = 
TransactionLineAmountChangeWhenImportInvoice:
AL_LineAmount = -9, AL_LineAmount Old Value = -10
   at Enterprise.MasterFiles.Business.AccTransactionLines";

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods(CriticalValidationMessageTemplate.OverseasCostAmountNotEqualToSumOfApportionmentsOverseasCostAmountErrorMessage, true,
				CriticalValidationErrorType.OverseasCostAmountNotEqualToSumOfApportionmentsOverseasCostAmount_9,
				"Overseas cost amount is not equal to the sum of the apportionment's overseas cost amount.",
				expectedMessageMainError, expectedMessageConsolCostError, expectedMessageConsolCostInfo, expectedMessageConsolCostCollectionInfo,
				expectedMessageJobChargeError, expectedMessageLineError);
			AssertOnSavingCheck(cost, testCase);
		}

		public void TestOverseasCostAmountNotEqualToSumOfApportionmentsOverseasCostAmountErrorMessage_DeleteApportionedCharge()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C000001");
			var shipment = TestObjectCreator.CreateShipment("S000001", consol);
			TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT);
			consolCost.E6_ApportionmentMethod = AllocationMethod.Shipment;
			consolCost.E6_OSCostAmount = 10m;
			CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(consolCost.PK, CriticalValidationInfoCollectorServiceKeyType.DeleteNewApportionedChargeWhenSaveJobConsolCost);

			using (DeleteApportionmentChargesWhenSaveJobConsolCostMonitor.AddTempService(Factory))
			{
				var monitor = Factory.ServiceContainer.GetService<DeleteApportionmentChargesWhenSaveJobConsolCostMonitor>();
				monitor.CollectApportionmentChargesInfo(consolCost);
				consolCost.ApportionmentCharges[0].Delete();
			}

			string expectedMessage1 = @"Charge Delete:
Charge: PK =";
			string expectedMessage2 = "at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)";

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				CriticalValidationMessageTemplate.OverseasCostAmountNotEqualToSumOfApportionmentsOverseasCostAmountErrorMessage,
				true,
				CriticalValidationErrorType.OverseasCostAmountNotEqualToSumOfApportionmentsOverseasCostAmount_9,
				"Overseas cost amount is not equal to the sum of the apportionment's overseas cost amount.",
				expectedMessage1, expectedMessage2);

			AssertOnSavingCheck(consolCost, testCase);
		}

		public void TestOverseasCostAmountNotEqualToSumOfApportionmentsOverseasCostAmountErrorMessage_ApportionSplitChargeCostAmountIsSetWithZero()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C000001");
			var shipment = TestObjectCreator.CreateShipment("S000001", consol);
			TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT);

			CriticalValidationInfoCollectorService.GetService(Factory).GetInfoSafe(consolCost.PK, CriticalValidationInfoCollectorServiceKeyType.ApportionSplitChargeCostAmountIsSetWithZero);

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				CriticalValidationMessageTemplate.OverseasCostAmountNotEqualToSumOfApportionmentsOverseasCostAmountErrorMessage,
				false,
				CriticalValidationErrorType.OverseasCostAmountNotEqualToSumOfApportionmentsOverseasCostAmount_9,
				"Overseas cost amount is not equal to the sum of the apportionment's overseas cost amount.",
				"ApportionSplitChargeCostAmountIsSetWithZero: ");

			AssertOnSavingCheck(consolCost, testCase);
		}

		public void TestOverseasCostAmountNotEqualToSumOfApportionmentsOverseasCostAmountErrorMessage_ApportionSplitChargeCostAmountIsSetWithZero_CollectedInfoIsAlwaysReported()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C000001");
			var shipment = TestObjectCreator.CreateShipment("S000001", consol);
			TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT);

			Factory.ServiceContainer.RemoveService<CriticalValidationInfoCollectorService>();

			var infoCollectorService = CriticalValidationInfoCollectorService.GetService(Factory);
			AssertNull($"Precondition: {nameof(infoCollectorService)}", infoCollectorService);

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				CriticalValidationMessageTemplate.OverseasCostAmountNotEqualToSumOfApportionmentsOverseasCostAmountErrorMessage,
				false,
				CriticalValidationErrorType.OverseasCostAmountNotEqualToSumOfApportionmentsOverseasCostAmount_9,
				"Overseas cost amount is not equal to the sum of the apportionment's overseas cost amount.",
				"ApportionSplitChargeCostAmountIsSetWithZero: There was no attempt to collect any data.");

			AssertOnSavingCheck(consolCost, testCase);
		}

		public void TestUnpostedConsolCostCouldHaveApportionmentChargesPostedViaJobRevenueJournal()
		{
			JobConsolCost consolCost = CreateChargeWithConsolCostAndAllCorrectInvoiceDetailsNotPosted(Factory);
			// Create an Job Revenue Journal
			var revJournal = Factory.NewWithValidTestData<JobRevenueJournal>();
			var revOnAP = revJournal.JournalLines.AddNew();
			Assert("Is Revenue line", AccTransactionLines.RevenueLineTypes.Contains(revOnAP.AL_LineType));
			var charge = Factory.LoadTop1<Charge>(new ZQuery(Enterprise.ZArchitecture.Schema.JobChargeSchema.JR_E6, consolCost.PK));
			AssertNotNull("Has Apportionment  Charges", charge);
			charge.JR_AL_APLine = revOnAP.PK;
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("UnpostedConsolCostShouldNotHaveCostPostedApportionmentCharges: Cost is posted via Job Revenue Journal");
			AssertOnSavingCheck(consolCost, testCase);
		}

		[TestDate(2018, 11, 14)]
		public void TestStackTraceInfoWhenApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetails()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfoSafe(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.ApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetails);
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfoSafe(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.ChargeSetsCreditorDifferentToConsolCost);
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfoSafe(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.ApportionSplitChargeCostAmountIsSetWithZero);

			var testObjectCreator = new TestObjectCreator(Factory);

			var consol = testObjectCreator.CreateConsol("AUSYD", "NZAKL", "test001");
			var shipment = testObjectCreator.CreateShipment("S0001", consol);
			_ = testObjectCreator.CreateJob(shipment);
			var cost = testObjectCreator.CreateConsolCost(consol, testObjectCreator.CC11, 100);
			Factory.Save();

			cost.E6_OH_Creditor = testObjectCreator.Creditor1.PK;
			cost.ApportionmentCharges[0].JR_OH_CostAccount = testObjectCreator.Creditor2.PK;

			cost.E6_InvoiceNum = "E6INV001";
			cost.ApportionmentCharges[0].JR_APInvoiceNum = "JRINV001";

			cost.E6_AT_TaxRate = testObjectCreator.KDV18.PK;
			cost.ApportionmentCharges[0].JR_AT_CostGSTRate = testObjectCreator.KDV18W5.PK;

			cost.E6_InvoiceDate = ZDateTime.Today;
			cost.ApportionmentCharges[0].JR_APInvoiceDate = ZDateTime.Today.AddDays(1);

			cost.E6_PaymentDate = ZDateTime.Today;
			cost.ApportionmentCharges[0].JR_PaymentDate = ZDateTime.Today.AddDays(1);

			var oldTaxClass = cost.ApportionmentCharges[0].JR_A9_CostVATClass;
			cost.ApportionmentCharges[0].JR_A9_CostVATClass = testObjectCreator.TaxMsg1.PK;

			cost.ApportionmentCharges[0].JR_CostReference = "JRCSTREF001";

			cost.ApportionmentCharges[0].JR_CostSupplyType = "ZZZ";

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("ApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetails",
				true,
				CriticalValidationErrorType.ApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetails_8,
				"Apportion Split Charge invoice details are not equal to parent consol cost invoice details",
				"Charge's AP Invoice Number is changed from E6INV001 to JRINV001. ConsolCost's AP Invoice Number is E6INV001.",
				"StackTrace -->",
				$"Charge's Tax Rate is changed from {testObjectCreator.KDV18.PK} to {testObjectCreator.KDV18W5.PK}. ConsolCost's Tax Rate is {testObjectCreator.KDV18.PK}.",
				"StackTrace -->",
				$"Charge's Invoice Date is changed from 14-Nov-18 00:00:00 to 15-Nov-18 00:00:00. ConsolCost's Invoice Date is 14-Nov-18 00:00:00.",
				"StackTrace -->",
				$"Charge's Payment Date is changed from 14-Nov-18 00:00:00 to 15-Nov-18 00:00:00. ConsolCost's Payment Date is 14-Nov-18 00:00:00.",
				"StackTrace -->",
				$"Charge's Tax Class is changed from {oldTaxClass} to {testObjectCreator.TaxMsg1.PK}. ConsolCost's Tax Class is {ZGuid.Empty}.",
				"StackTrace -->",
				$"Charge's Cost Reference is changed from '' to JRCSTREF001. ConsolCost's Cost Reference is ''.",
				"StackTrace -->",
				$"Charge's Supply Type is changed from '' to ZZZ. ConsolCost's Supply Type is ''.",
				"StackTrace -->",
				$"Charge account '{testObjectCreator.Creditor2.PK}' Consol Cost account '{testObjectCreator.Creditor1.PK}'.",
				"Call stack",
				"ApportionSplitChargeCostAmountIsSetWithZero: ");

			AssertOnSavingCheck(cost, testCase);
		}

		public void TestApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetails_ApportionChargeIsNotSaved()
		{
			var consolCost = CreateChargeWithConsolCostAndAllCorrectInvoiceDetailsNotPosted(Factory);
			var charge = Factory.LoadTop1<ApportionSplitCharge>(new ZQuery(ZArchitecture.Schema.JobChargeSchema.JR_E6, consolCost.PK));

			consolCost.E6_CostReference = "Reference123";
			charge.JR_CostReference = "Reference123";
			charge.JR_IsUsedForApportionment = true;
			ZDataUtils.SetShouldRowBeSaved(((IBusinessObjectInternals)charge).Row, false);

			string expectedMessageConsolCostInfo = FormattableString.Invariant($@"Job Consol Cost:
	PK = {consolCost.PK}");
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("ApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetails - Apportion charge is not saved", true, CriticalValidationErrorType.ApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetails_8,
				"Apportion Split Charge invoice details are not equal to parent consol cost invoice details",
				expectedMessageConsolCostInfo,
				"Charge is not saved:");

			AssertOnSavingCheck(consolCost, testCase);
		}

		public void TestApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetails_BadDataWithoutChanges()
		{
			var consolCost = CreateChargeWithConsolCostAndAllCorrectInvoiceDetailsNotPosted(Factory);
			Factory.Save();

			var charge = Factory.LoadTop1<Charge>(new ZQuery(ZArchitecture.Schema.JobChargeSchema.JR_E6, consolCost.PK));
			SuspendCriticalValidationAttribute.IsActive = true;
			charge.JR_CostReference = "ABC123";
			consolCost.E6_CostReference = "ABC";
			Factory.Save();
			SuspendCriticalValidationAttribute.IsActive = false;

			charge.JR_APInvoiceNum = "222";
			consolCost.E6_InvoiceNum = "222";

			var userErrorMessage = "Apportion Split Charge invoice details are not equal to parent consol cost invoice details.\r\nPlease go to related consol form, click 'Job Invoicing > Synchronize Cost Invoice Details' menu item and try again.";
			var misMatchedInfo = "Supplier Cost Reference:    ABC123    |    ABC";

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("ApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetails - Dismatched info(Cost Reference) don't have changes",
				true,
				CriticalValidationErrorType.ApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetailsWithoutChanges,
				userErrorMessage,
				misMatchedInfo);

			AssertOnSavingCheck(consolCost, testCase);
		}

		public void TestPostedConsolCostWithNonOverriddenTaxAmount()
		{
			var cost = CreateChargeWithConsolCostAndAllCorrectInvoiceDetails(Factory, true);
			cost.E6_IsTaxAmountOverridden = false;

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods(CriticalValidationMessageTemplate.GetPostedConsolCostWithNonOverriddenTaxAmountErrorMessage, true, CriticalValidationErrorType.PostedConsolCostWithNonOverriddenTaxAmount_2, "Posted Consol cost must have overridden GST Amount.", "Job Consol Cost:");
			AssertOnSavingCheck(cost, testCase);
		}

		public void TestMismatchedOverriddenTaxAmountFlag()
		{
			var cost = CreateChargeWithConsolCostAndAllCorrectInvoiceDetails(Factory, true);
			cost.ApportionmentCharges.ReloadFromLocalCache();
			cost.ApportionmentCharges[0].JR_IsCostTaxAmountOverridden = false;

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("ApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetails - JR_IsCostTaxAmountOverridden doesn't match with E6_IsTaxAmountOverridden", true, CriticalValidationErrorType.ApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetails_8, "Apportion Split Charge invoice details are not equal to parent consol cost invoice details", "Job Consol Cost:");
			AssertOnSavingCheck(cost, testCase);
		}

		JobConsolCost ParentIdAndTableCodeValueSet(BusinessObjectFactory factory)
		{
			JobConsolCost testCost = factory.NewWithValidTestData<JobConsolCost>();
			var consol = factory.NewWithValidTestData<ForwardingConsol>();
			using (testCost.ReportSettingParentSuspender.GetSuspender())
			{
				testCost.E6_ParentID = consol.PK;
				testCost.E6_ParentTableCode = consol.TablePrefix;
			}

			Assert("E6_ParentID is not empty", !testCost.E6_ParentID.IsEmpty);
			Assert("E6_ParentTableCode is not empty", !testCost.E6_ParentTableCode.IsEmpty);
			return testCost;
		}

		JobConsolCost ParentIdNotSet(BusinessObjectFactory factory)
		{
			JobConsolCost testCost = factory.NewWithValidTestData<JobConsolCost>();
			var consol = factory.NewWithValidTestData<ForwardingConsol>();
			using (testCost.ReportSettingParentSuspender.GetSuspender())
			{
				testCost.E6_ParentID = ZGuid.Empty;
				testCost.E6_ParentTableCode = consol.TablePrefix;
			}

			Assert("E6_ParentID is empty", testCost.E6_ParentID.IsEmpty);
			Assert("E6_ParentTableCode is not empty", !testCost.E6_ParentTableCode.IsEmpty);
			return testCost;
		}

		JobConsolCost ParentIdSetButTableCodeNotSet(BusinessObjectFactory factory)
		{
			JobConsolCost testCost = factory.NewWithValidTestData<JobConsolCost>();
			var consol = factory.NewWithValidTestData<ForwardingConsol>();
			using (testCost.ReportSettingParentSuspender.GetSuspender())
			{
				testCost.E6_ParentID = consol.PK;
				testCost.E6_ParentTableCode = ZString.Empty;
			}

			Assert("E6_ParentID is not empty", !testCost.E6_ParentID.IsEmpty);
			Assert("E6_ParentTableCode is empty", testCost.E6_ParentTableCode.IsEmpty);
			return testCost;
		}

		JobConsolCost OSCostAmountEqual(BusinessObjectFactory factory)
		{
			JobConsolCost testCost = factory.NewWithValidTestData<JobConsolCost>();
			ApportionSplitCharge charge1 = testCost.ApportionmentCharges.AddNew();
			charge1.FillWithValidTestData();
			ApportionSplitCharge charge2 = testCost.ApportionmentCharges.AddNew();
			charge2.FillWithValidTestData();
			charge1.JR_OSCostAmt = 500;
			charge2.JR_OSCostAmt = -500;
			AssertEquals("OS Cost Amount should equal to charges Cost Amount sum.", testCost.E6_OSCostAmount, testCost.ApportionmentCharges.JR_OSCostAmtSum);
			return testCost;
		}

		JobConsolCost OSCostAmountNotEqual(BusinessObjectFactory factory)
		{
			JobConsolCost testCost = OSCostAmountEqual(factory);
			testCost.ApportionmentCharges[0].JR_OSCostAmt = 100;
			AssertNotEquals("OS Cost Amount should not equal to charges Cost Amount sum.", testCost.E6_OSCostAmount, testCost.ApportionmentCharges.JR_OSCostAmtSum);
			return testCost;
		}

		JobConsolCost LocalCostAmountEqual(BusinessObjectFactory factory)
		{
			JobConsolCost testCost = factory.NewWithValidTestData<JobConsolCost>();
			ApportionSplitCharge charge1 = testCost.ApportionmentCharges.AddNew();
			charge1.FillWithValidTestData();
			ApportionSplitCharge charge2 = testCost.ApportionmentCharges.AddNew();
			charge2.FillWithValidTestData();
			charge1.JR_LocalCostAmt = 500;
			charge2.JR_LocalCostAmt = -500;
			AssertEquals("Local Cost Amount should equal to charges Local Cost Amount sum.", testCost.E6_LocalCostAmount, testCost.ApportionmentCharges.JR_LocalCostAmtSum);
			return testCost;
		}

		JobConsolCost LocalCostAmountNotEqual(BusinessObjectFactory factory)
		{
			JobConsolCost testCost = LocalCostAmountEqual(factory);
			testCost.ApportionmentCharges[0].JR_LocalCostAmt = 100;
			AssertNotEquals("Local Cost Amount should not equal to charges Local Cost Amount sum.", testCost.E6_LocalCostAmount, testCost.ApportionmentCharges.JR_LocalCostAmtSum);
			return testCost;
		}

		JobConsolCost OSGSTAmountEqual(BusinessObjectFactory factory)
		{
			JobConsolCost testCost = factory.NewWithValidTestData<JobConsolCost>();
			ApportionSplitCharge charge1 = testCost.ApportionmentCharges.AddNew();
			charge1.FillWithValidTestData();
			ApportionSplitCharge charge2 = testCost.ApportionmentCharges.AddNew();
			charge2.FillWithValidTestData();
			testCost.E6_IsTaxAmountOverridden = true;
			charge1.JR_OSCostGSTAmt_Calc = 500;
			charge2.JR_OSCostGSTAmt_Calc = -500;
			AssertEquals("OS GST Amount should equal to charges GST Amount sum.", testCost.E6_OSGSTAmount_Calc, testCost.ApportionmentCharges.JR_OSCostGSTAmtSum);
			return testCost;
		}

		JobConsolCost OSGSTAmountNotEqual(BusinessObjectFactory factory)
		{
			JobConsolCost testCost = OSGSTAmountEqual(factory);
			testCost.E6_IsTaxAmountOverridden = true;
			testCost.ApportionmentCharges[0].JR_OSCostGSTAmt_Calc = 100;
			AssertNotEquals("OS GST Amount should not equal to charges GST Amount sum.", testCost.E6_OSGSTAmount_Calc, testCost.ApportionmentCharges.JR_OSCostGSTAmtSum);
			return testCost;
		}

		JobConsolCost CostAmountsNotEqualAndPostingWasReversed(BusinessObjectFactory factory)
		{
			JobConsolCost testCost = factory.NewWithValidTestData<JobConsolCost>();
			testCost.E6_IsTaxAmountOverridden = true;
			ApportionSplitCharge charge1 = testCost.ApportionmentCharges.AddNew();
			charge1.FillWithValidTestData();
			ApportionSplitCharge charge2 = testCost.ApportionmentCharges.AddNew();
			charge2.FillWithValidTestData();
			charge1.JR_OSCostAmt = 500m;
			charge2.JR_OSCostAmt = -500m;
			charge1.JR_OSCostGSTAmt_Calc = 50m;
			charge2.JR_OSCostGSTAmt_Calc = -50m;
			charge1.JR_LocalCostAmt = 500m;
			charge2.JR_LocalCostAmt = -500m;
			AssertEquals("Precondition: Local Cost Amount should equal to charges Local Cost Amount sum.", testCost.E6_LocalCostAmount, testCost.ApportionmentCharges.JR_LocalCostAmtSum);
			AssertEquals("Precondition: OS GST Amount should equal to charges GST Amount sum.", testCost.E6_OSGSTAmount_Calc, testCost.ApportionmentCharges.JR_OSCostGSTAmtSum);
			AssertEquals("Precondition: OS Cost Amount should equal to charges Cost Amount sum.", testCost.E6_OSCostAmount, testCost.ApportionmentCharges.JR_OSCostAmtSum);
			var invoice = factory.NewWithValidTestData<APInvoice>();
			testCost.E6_AH_APInvoice = invoice.PK;
			var invoiceLine1 = (APInvoiceLine)invoice.Lines.AddNew();
			invoiceLine1.AL_LineAmount = -500m;
			invoiceLine1.AL_GSTVAT = -50m;
			invoiceLine1.AL_OSAmount = -550m;
			charge1.JR_AL_APLine = invoiceLine1.PK;
			var invoiceLine2 = (APInvoiceLine)invoice.Lines.AddNew();
			invoiceLine2.AL_LineAmount = 500m;
			invoiceLine2.AL_GSTVAT = 50m;
			invoiceLine2.AL_OSAmount = 550m;
			charge2.JR_AL_APLine = invoiceLine2.PK;
			factory.Save();

			testCost.E6_AH_APInvoice = ZGuid.Empty;
			testCost.E6_IsTaxAmountOverridden = true;
			testCost.ApportionmentCharges[0].JR_OSCostAmt = 100;
			testCost.ApportionmentCharges[0].JR_OSCostGSTAmt_Calc = 100;
			testCost.ApportionmentCharges[0].JR_LocalCostAmt = 100;

			AssertNotEquals("Precondition: OS GST Amount should not equal to charges GST Amount sum.", testCost.E6_OSGSTAmount_Calc, testCost.ApportionmentCharges.JR_OSCostGSTAmtSum);
			AssertNotEquals("Precondition: Consol cost should be in DB as posted.", ZGuid.Empty, testCost.E6_AH_APInvoiceInfo.OriginalValue);
			AssertEquals("Precondition: Consol cost should not be posted.", ZGuid.Empty, testCost.E6_AH_APInvoiceInfo.Value);
			return testCost;
		}

		public void TestCVTriggeredWhenSetJROSCostAmtDoesNotUpdateIsUsedForApportionment()
		{
			TestObjectCreator.SetUpCostVarianceApprovalRegistry(Core.Constants.CostVarianceComparisonOption.JobAndChargeCode);

			var cost = SetupConsolCostForAutoTickFinalFlag(150M);
			cost.E6_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			cost.E6_ExchangeRate = 0.6m;

			AssertEquals("Should have 2 charge lines", 2, cost.ApportionmentCharges.Count);
			var charge = cost.ApportionmentCharges[0];
			var charge2 = cost.ApportionmentCharges[1];

			cost.E6_OSCostAmount = 5M;

			charge.SetShipmentInfo(null);
			var oldAmt = charge.JR_OSCostAmt;
			charge.JR_OSCostAmt = 0m;
			cost.UpdateShipmentInfosOnCharges();

			charge.JR_OSCostAmt = oldAmt;

			cost.PushLocalAmountExchangeRateDifferencesToGreatestCharge();

			AssertOnSavingCheck(cost, new TestCaseDefinition_ForSeparateTestsMethods("Should not fail any critical validation."));
		}

		public void TestCostTaxAmountEqualToSumOfApportionmentsCostTaxAmountForCreditorChanged()
		{
			var consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			var shipment1 = consol.Shipments.AddNew();

			var apps = new ApportionmentListing(Factory, consol);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT, TestObjectCreator.AALSHI, apps);

			try
			{
				var charge = consolCost.ApportionmentCharges[0];
				consolCost.E6_OSCostAmount = 1840m;

				AssertEquals(0m, consolCost.E6_OSGSTAmount_Calc);

				consolCost.E6_IsTaxAmountOverridden = true;
				consolCost.E6_OSGSTAmount_Calc = 184m;
				AssertEquals(184m, charge.JR_OSCostGSTAmt);

				var oldOH_Creditor = consolCost.E6_OH_Creditor;
				var oldAT_TaxRate = consolCost.E6_AT_TaxRate;

				var newOrg = TestObjectCreator.CreateOrgHeader("TEST123", true, false, true, true, false, false);

				consolCost.E6_OH_Creditor = newOrg.PK;

				AssertNotEquals(oldOH_Creditor, consolCost.E6_OH_Creditor);
				AssertEquals(oldAT_TaxRate, consolCost.E6_AT_TaxRate);
				AssertEquals(184m, charge.JR_OSCostGSTAmt);
				AssertEquals(184m, consolCost.E6_OSGSTAmount_Calc);
				AssertEquals(charge.JR_OSCostGSTAmt, consolCost.E6_OSGSTAmount_Calc);

				var testCase = new TestCaseDefinition_ForSeparateTestsMethods("When consol cost creditor has changed and tax id not changed, consol cost tax amount should equal to sum of apportionments cost tax amount.");
				AssertOnSavingCheck(consolCost, testCase);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		JobConsolCost SetupConsolCostForAutoTickFinalFlag(decimal amount)
		{
			GlbDepartment.GetCurrentDepartment(Factory).GE_Misc = false;
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C001";
			var shipment1 = consol.Shipments.AddNew();
			var job1 = TestObjectCreator.CreateJob(shipment1);
			var shipment2 = consol.Shipments.AddNew();
			var job2 = TestObjectCreator.CreateJob(shipment2);

			Factory.Save();

			var invoice = Factory.NewWithValidTestData<APInvoice>();

			var cost = invoice.ConsolCosting.ConsolCosts.AddNew();
			cost.E6_ParentID = consol.PK;
			cost.E6_ParentTableCode = consol.TablePrefix;
			cost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			cost.E6_OSCostAmount = amount;
			Assert(!cost.HasErrors);

			return cost;
		}

		JobConsolCost CreateChargeWithConsolCostAndAllCorrectInvoiceDetailsNotPosted(BusinessObjectFactory factory)
		{
			return CreateChargeWithConsolCostAndAllCorrectInvoiceDetails(factory, false);
		}

		JobConsolCost CreatePostedChargeWithConsolCostAndAllCorrectInvoiceDetails(BusinessObjectFactory factory)
		{
			return CreateChargeWithConsolCostAndAllCorrectInvoiceDetails(factory, true);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1052:Do Not Cast Factory Method", Justification = "Baseline")]
		JobConsolCost CreateChargeWithConsolCostAndAllCorrectInvoiceDetails(BusinessObjectFactory factory, bool consolLinkedToInvoice)
		{
			BaseCharge charge = (BaseCharge)factory.New(typeof(BaseCharge), new Guid("f8a8e869-ce2a-4049-a9df-d0b7a7e79f4e"));
			charge.FillWithValidTestData();
			var job = factory.NewJobWithPrimaryKeyForTesting<Job>(new Guid("7dd1291f-ed46-4c34-8714-5d35e717cda5"));
			job.FillWithValidTestData();
			charge.JR_JH = job.PK;
			JobConsolCost consolCost = (JobConsolCost)factory.New(typeof(JobConsolCost), new Guid("a12986b4-036c-4cb2-a0e7-0c96438a46dd"));
			consolCost.FillWithValidTestData();
			consolCost.E6_IsTaxAmountOverridden = true;
			ForwardingConsol consol = (ForwardingConsol)factory.New(typeof(ForwardingConsol), new Guid("40b5446d-6066-4ed1-b9c1-8ca588a2a43d"));
			consol.FillWithValidTestData();
			using (consolCost.ReportSettingParentSuspender.GetSuspender())
			{
				consolCost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, consol.TablePrefix);
			}

			if (consolLinkedToInvoice)
			{
				var invoice = (APInvoice)factory.New(typeof(APInvoice), new Guid("9670ef3f-c8ad-496d-a278-c40bfdfbba1e"));
				invoice.AH_Ledger = LedgerTypes.AccountsPayable;
				invoice.AH_TransactionType = TransactionTypes.Invoice;
				consolCost.E6_AH_APInvoice = invoice.PK;
			}

			var taxRate = (AccTaxRate)factory.New(typeof(AccTaxRate), new Guid("41312608-7ae1-4155-af94-0af99cdfdc97"));
			taxRate.AT_Code = "TAX1";
			// AccTaxRate is not valid with empty AT_Type.
			taxRate.AT_Type = AccTaxRate.Types.Rated;
			var taxClass = factory.New<AccInvMsg>();
			taxClass.A9_Code = "MSG1";
			charge.JR_E6 = consolCost.PK;
			charge.JR_APInvoiceNum = "111";
			charge.JR_APInvoiceDate = new ZDateTime(2010, 01, 05);
			charge.JR_OH_CostAccount = factory.NewWithValidTestData<OrgHeader>().PK;
			charge.JR_PaymentDate = new ZDateTime(2010, 01, 10);
			charge.JR_CostReference = "ABC";
			charge.JR_AT_CostGSTRate = taxRate.PK;
			charge.JR_A9_CostVATClass = taxClass.PK;
			if (consolLinkedToInvoice)
			{
				var invoiceLine = (APInvoiceLine)factory.New(typeof(APInvoiceLine), new Guid("bf0c6e57-d1b6-40cd-866a-6b9f4ba584ae"));
				invoiceLine.AL_LineType = TransactionLineTypes.Cost;
				invoiceLine.AL_AH = consolCost.E6_AH_APInvoice;
				charge.JR_AL_APLine = invoiceLine.PK;
			}

			consolCost.E6_InvoiceNum = charge.JR_APInvoiceNum;
			consolCost.E6_InvoiceDate = charge.JR_APInvoiceDate;
			consolCost.E6_OH_Creditor = charge.JR_OH_CostAccount;
			consolCost.E6_PaymentDate = charge.JR_PaymentDate;
			consolCost.E6_CostReference = charge.JR_CostReference;
			consolCost.E6_AT_TaxRate = charge.JR_AT_CostGSTRate;
			consolCost.E6_A9_VATClass = charge.JR_A9_CostVATClass;
			consolCost.E6_IsTaxAmountOverridden = true;
			return consolCost;
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
