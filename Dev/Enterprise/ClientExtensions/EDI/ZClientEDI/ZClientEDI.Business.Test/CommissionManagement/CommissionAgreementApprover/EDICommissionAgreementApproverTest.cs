using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.CommissionManagement.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.CommissionManagement.Business.Test
{
	class EDICommissionAgreementApproverTest : TestCaseWithFactory
	{
		#region Approve

		public void TestGetOdplInvoicesWithoutRevenueBreakdownFilter()
		{
			var orgXXX = Factory.NewWithValidTestData<OrgHeader>();
			var licenceCompanyXXX = Factory.NewWithValidTestData<LicenceCompany>();
			licenceCompanyXXX.LC_OH = orgXXX.PK;

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "TEST";
			chargeCode.AC_IsCommissionable = true;

			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();

			var orgYYY = Factory.NewWithValidTestData<OrgHeader>();
			var licenceCompanyYYY = Factory.NewWithValidTestData<LicenceCompany>();
			licenceCompanyYYY.LC_OH = orgYYY.PK;

			var agreementXXX = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreement(Factory, orgXXX);
			var agreementYYY = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreement(Factory, orgYYY);
			agreementYYY.CA0_EffectiveDate = new ZDate(2002, 2, 2);

			var billingInvoiceA = Factory.New<ARInvoice>();
			billingInvoiceA.AH_Desc = "billingInvoiceA";
			billingInvoiceA.AH_PostDate = new ZDateTime(2004, 4, 4);
			var lineA = (TransactionLine)billingInvoiceA.Lines.AddNew();
			lineA.AL_AC = chargeCode.PK;
			var usageA1 = Factory.New<ClientChargeableUsage>();
			usageA1.U1_AH_Invoice = billingInvoiceA.PK;
			usageA1.U1_LC = licenceCompanyXXX.PK;
			usageA1.U1_LD = licenceDatabase.PK;
			usageA1.U1_PeriodStart = new ZDateTime(2002, 1, 1);
			var usageA2 = Factory.New<ClientChargeableUsage>();
			usageA2.U1_AH_Invoice = billingInvoiceA.PK;
			usageA2.U1_LC = licenceCompanyYYY.PK;
			usageA2.U1_PeriodStart = new ZDateTime(2003, 1, 1);

			var billingInvoiceB = Factory.New<ARInvoice>();
			billingInvoiceB.AH_Desc = "billingInvoiceB";
			billingInvoiceB.AH_PostDate = new ZDateTime(2004, 4, 4);
			var usageB1 = Factory.New<ClientChargeableUsage>();
			usageB1.U1_AH_Invoice = billingInvoiceB.PK;
			usageB1.U1_LC = licenceCompanyXXX.PK;
			usageB1.U1_LD = licenceDatabase.PK;
			usageB1.U1_PeriodStart = new ZDateTime(2003, 1, 1);
			var usageB2 = Factory.New<ClientChargeableUsage>();
			usageB2.U1_AH_Invoice = billingInvoiceB.PK;
			usageB2.U1_LC = licenceCompanyXXX.PK;
			usageB2.U1_LD = licenceDatabase.PK;
			usageB2.U1_PeriodStart = new ZDateTime(2004, 1, 1);

			var billingInvoiceC = Factory.New<ARInvoice>();
			billingInvoiceC.AH_Desc = "billingInvoiceC";
			billingInvoiceC.AH_PostDate = new ZDateTime(2003, 3, 3);
			var lineC = (TransactionLine)billingInvoiceC.Lines.AddNew();
			lineC.AL_AC = chargeCode.PK;
			var usageC1 = Factory.New<ClientChargeableUsage>();
			usageC1.U1_AH_Invoice = billingInvoiceC.PK;
			usageC1.U1_LC = licenceCompanyXXX.PK;
			usageC1.U1_LD = licenceDatabase.PK;
			usageC1.U1_PeriodStart = new ZDateTime(2002, 1, 1);
			var usageC2 = Factory.New<ClientChargeableUsage>();
			usageC2.U1_AH_Invoice = billingInvoiceC.PK;
			usageC2.U1_LC = licenceCompanyYYY.PK;
			usageC2.U1_LD = licenceDatabase.PK;
			usageC2.U1_PeriodStart = new ZDateTime(2003, 1, 1);

			var stlInvoice = Factory.New<ARInvoice>();
			stlInvoice.AH_Desc = "stlInvoice";
			stlInvoice.AH_PostDate = new ZDateTime(2005, 1, 1);
			var stlUsage = Factory.New<EdiBilledUsage>();
			stlUsage.BU9_AH_Invoice = stlInvoice.PK;
			stlUsage.BU9_LC = licenceCompanyXXX.PK;
			stlUsage.BU9_LD = licenceDatabase.PK;
			stlUsage.BU9_UsageCode = "#HU";
			stlUsage.BU9_PeriodStart = new ZDate(2005, 1, 1);

			var billingInvoiceD = Factory.New<ARInvoice>();
			billingInvoiceD.AH_Desc = "billingInvoiceD";
			billingInvoiceD.AH_PostDate = new ZDateTime(2006, 1, 1);
			var usageD = Factory.New<ClientChargeableUsage>();
			usageD.U1_AH_Invoice = billingInvoiceD.PK;
			usageD.U1_LC = licenceCompanyXXX.PK;
			usageD.U1_LD = licenceDatabase.PK;
			usageD.U1_Code = "#HU";
			usageD.U1_PeriodStart = new ZDateTime(2006, 1, 1);

			Factory.Save();

			var context = new CreateCommissionContext() { FromDate = ZDateTime.MinSmallDateTimeValue };
			AssertContainsExactElementsInAnyOrder(
				BusinessObjectEqualityComparer<AccTransactionHeader>.PKOnlyComparer,
				x => x.AH_Desc,
				new AccTransactionHeader[] { billingInvoiceA, billingInvoiceB, billingInvoiceC, billingInvoiceD },
				Factory.Load<AccTransactionHeader>(EDICommissionAgreementApprover.GetOdplInvoicesWithoutRevenueBreakdownFilter(context)));

			context = new CreateCommissionContext() { FromDate = new ZDateTime(2004, 1, 1) };
			AssertContainsExactElementsInAnyOrder(
				BusinessObjectEqualityComparer<AccTransactionHeader>.PKOnlyComparer,
				x => x.AH_Desc,
				new AccTransactionHeader[] { billingInvoiceA, billingInvoiceB, billingInvoiceD },
				Factory.Load<AccTransactionHeader>(EDICommissionAgreementApprover.GetOdplInvoicesWithoutRevenueBreakdownFilter(context)));

			var agreementsBeingApproved = new HashSet<OrgCommissionAgreement>() { agreementYYY };
			context = new CreateCommissionContext() { FromDate = ZDateTime.MinSmallDateTimeValue, AgreementsBeingApproved = agreementsBeingApproved };
			AssertContainsExactElementsInAnyOrder(
				BusinessObjectEqualityComparer<AccTransactionHeader>.PKOnlyComparer,
				x => x.AH_Desc,
				new AccTransactionHeader[] { billingInvoiceA, billingInvoiceC },
				Factory.Load<AccTransactionHeader>(EDICommissionAgreementApprover.GetOdplInvoicesWithoutRevenueBreakdownFilter(context)));

			agreementYYY.CA0_EffectiveDate = new ZDate(2004, 1, 1);
			Factory.Save();

			context = new CreateCommissionContext() { FromDate = ZDateTime.MinSmallDateTimeValue, AgreementsBeingApproved = agreementsBeingApproved };
			var query = EDICommissionAgreementApprover.GetOdplInvoicesWithoutRevenueBreakdownFilter(context);
			query.ReLoadExistingRows = true;
			AssertContainsExactElementsInAnyOrder("Should not include billingInvoiceC as it's posted date is before agreement effective date",
				BusinessObjectEqualityComparer<AccTransactionHeader>.PKOnlyComparer,
				x => x.AH_Desc,
				new AccTransactionHeader[] { billingInvoiceA },
				Factory.Load<AccTransactionHeader>(query));

			var existingAgreementYYYCommissionHeaderForBillingInvoiceC = Factory.NewWithValidTestData<AccCommissionHeader>();
			existingAgreementYYYCommissionHeaderForBillingInvoiceC.CH0_AH_Source = billingInvoiceC.PK;
			existingAgreementYYYCommissionHeaderForBillingInvoiceC.CH0_CA0 = agreementYYY.PK;

			Factory.Save();

			context = new CreateCommissionContext() { OverwriteOldValues = true, FromDate = ZDateTime.MinSmallDateTimeValue, AgreementsBeingApproved = agreementsBeingApproved };
			AssertContainsExactElementsInAnyOrder(@"Since we are overwriting old commissions, should include billingInvoiceC again (even though it isn't in the effective date range) as it has existing commission.
This is needed in the scenarios where commission needs to be canceled due to agreement effective date changing",
				BusinessObjectEqualityComparer<AccTransactionHeader>.PKOnlyComparer,
				x => x.AH_Desc,
				new AccTransactionHeader[] { billingInvoiceA, billingInvoiceC },
				Factory.Load<AccTransactionHeader>(EDICommissionAgreementApprover.GetOdplInvoicesWithoutRevenueBreakdownFilter(context)));
		}

		public void TestGetInvoicesFilter()
		{
			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			var orgXXX = Factory.NewWithValidTestData<OrgHeader>();
			var orgYYY = Factory.NewWithValidTestData<OrgHeader>();

			var agreementXXX = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreement(Factory, orgXXX);
			var agreementYYY = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreement(Factory, orgYYY);
			agreementYYY.CA0_EffectiveDate = new ZDate(2002, 2, 2);

			var stlInvoiceA = Factory.New<ARInvoice>();
			stlInvoiceA.AH_Desc = "stlInvoiceA";
			stlInvoiceA.AH_OH = orgXXX.PK;
			stlInvoiceA.AH_PostDate = new ZDateTime(2004, 4, 4);
			var stlUsageA1 = Factory.NewWithValidTestData<EdiBilledUsage>();
			stlUsageA1.BU9_AH_Invoice = stlInvoiceA.PK;
			stlUsageA1.BU9_BillingModel = BillingConstants.PriceHeaderType.STL;
			stlUsageA1.BU9_LD = licenceDatabase.PK;
			var stlUsageA2 = Factory.NewWithValidTestData<EdiBilledUsage>();
			stlUsageA2.BU9_AH_Invoice = stlInvoiceA.PK;
			stlUsageA2.BU9_BillingModel = BillingConstants.PriceHeaderType.STL;
			stlUsageA2.BU9_LD = licenceDatabase.PK;

			var stlInvoiceB = Factory.New<ARInvoice>();
			stlInvoiceB.AH_Desc = "stlInvoiceB";
			stlInvoiceB.AH_OH = orgXXX.PK;
			stlInvoiceB.AH_PostDate = new ZDateTime(2004, 4, 4);
			var stlUsageB1 = Factory.NewWithValidTestData<EdiBilledUsage>();
			stlUsageB1.BU9_AH_Invoice = stlInvoiceB.PK;
			stlUsageB1.BU9_BillingModel = BillingConstants.PriceHeaderType.STL;
			stlUsageB1.BU9_LD = licenceDatabase.PK;

			var stlInvoiceC = Factory.New<ARInvoice>();
			stlInvoiceC.AH_Desc = "stlInvoiceC";
			stlInvoiceC.AH_OH = orgYYY.PK;
			stlInvoiceC.AH_PostDate = new ZDateTime(2003, 3, 3);
			var stlUsageC1 = Factory.NewWithValidTestData<EdiBilledUsage>();
			stlUsageC1.BU9_AH_Invoice = stlInvoiceC.PK;
			stlUsageC1.BU9_BillingModel = BillingConstants.PriceHeaderType.STL;
			stlUsageC1.BU9_LD = licenceDatabase.PK;
			var stlUsageC2 = Factory.NewWithValidTestData<EdiBilledUsage>();
			stlUsageC2.BU9_AH_Invoice = stlInvoiceC.PK;
			stlUsageC2.BU9_BillingModel = BillingConstants.PriceHeaderType.STL;
			stlUsageC2.BU9_LD = licenceDatabase.PK;

			var stlInvoiceD = Factory.New<ARInvoice>();
			stlInvoiceD.AH_Desc = "stlInvoiceD";
			stlInvoiceD.AH_OH = orgYYY.PK;
			stlInvoiceD.AH_PostDate = new ZDateTime(2006, 1, 1);
			var stlUsageD = Factory.NewWithValidTestData<EdiBilledUsage>();
			stlUsageD.BU9_AH_Invoice = stlInvoiceD.PK;
			stlUsageD.BU9_BillingModel = BillingConstants.PriceHeaderType.STL;
			stlUsageD.BU9_LD = licenceDatabase.PK;

			var odplInvoiceA = Factory.New<ARInvoice>();
			odplInvoiceA.AH_Desc = "odplInvoiceA";
			odplInvoiceA.AH_OH = orgXXX.PK;
			odplInvoiceA.AH_PostDate = new ZDateTime(2004, 4, 4);
			var odplUsageA1 = Factory.NewWithValidTestData<EdiBilledUsage>();
			odplUsageA1.BU9_AH_Invoice = odplInvoiceA.PK;
			odplUsageA1.BU9_BillingModel = BillingConstants.PriceHeaderType.ODM;
			odplUsageA1.BU9_LD = licenceDatabase.PK;
			var odplUsageA2 = Factory.NewWithValidTestData<EdiBilledUsage>();
			odplUsageA2.BU9_AH_Invoice = odplInvoiceA.PK;
			odplUsageA2.BU9_BillingModel = BillingConstants.PriceHeaderType.ODM;
			odplUsageA2.BU9_LD = licenceDatabase.PK;

			var odplInvoiceB = Factory.New<ARInvoice>();
			odplInvoiceB.AH_Desc = "odplInvoiceB";
			odplInvoiceB.AH_OH = orgXXX.PK;
			odplInvoiceB.AH_PostDate = new ZDateTime(2004, 4, 4);
			var odplUsageB1 = Factory.NewWithValidTestData<EdiBilledUsage>();
			odplUsageB1.BU9_AH_Invoice = odplInvoiceB.PK;
			odplUsageB1.BU9_BillingModel = BillingConstants.PriceHeaderType.ODM;
			odplUsageB1.BU9_LD = licenceDatabase.PK;

			var odplInvoiceC = Factory.New<ARInvoice>();
			odplInvoiceC.AH_Desc = "odmInvoiceC";
			odplInvoiceC.AH_OH = orgYYY.PK;
			odplInvoiceC.AH_PostDate = new ZDateTime(2003, 3, 3);
			var odplUsageC1 = Factory.NewWithValidTestData<EdiBilledUsage>();
			odplUsageC1.BU9_AH_Invoice = odplInvoiceC.PK;
			odplUsageC1.BU9_BillingModel = BillingConstants.PriceHeaderType.ODM;
			odplUsageC1.BU9_LD = licenceDatabase.PK;
			var odplUsageC2 = Factory.NewWithValidTestData<EdiBilledUsage>();
			odplUsageC2.BU9_AH_Invoice = odplInvoiceC.PK;
			odplUsageC2.BU9_BillingModel = BillingConstants.PriceHeaderType.ODM;
			odplUsageC2.BU9_LD = licenceDatabase.PK;

			var odplInvoiceD = Factory.New<ARInvoice>();
			odplInvoiceD.AH_Desc = "odplInvoiceD";
			odplInvoiceD.AH_OH = orgYYY.PK;
			odplInvoiceD.AH_PostDate = new ZDateTime(2006, 1, 1);
			var odplUsageD = Factory.NewWithValidTestData<EdiBilledUsage>();
			odplUsageD.BU9_AH_Invoice = odplInvoiceD.PK;
			odplUsageD.BU9_BillingModel = BillingConstants.PriceHeaderType.ODM;
			odplUsageD.BU9_LD = licenceDatabase.PK;

			var borderWiseOnlyInvoiceA = Factory.New<ARInvoice>();
			borderWiseOnlyInvoiceA.AH_Desc = "borderWiseOnlyInvoiceA";
			borderWiseOnlyInvoiceA.AH_OH = orgXXX.PK;
			borderWiseOnlyInvoiceA.AH_PostDate = new ZDateTime(2006, 1, 1);
			var borderWiseOnlyUsageA = Factory.NewWithValidTestData<EdiBilledUsage>();
			borderWiseOnlyUsageA.BU9_AH_Invoice = borderWiseOnlyInvoiceA.PK;
			borderWiseOnlyUsageA.BU9_BillingModel = BillingConstants.PriceHeaderType.BorderWise;
			borderWiseOnlyUsageA.BU9_LD = licenceDatabase.PK;

			var borderWiseOnlyInvoiceB = Factory.New<ARInvoice>();
			borderWiseOnlyInvoiceB.AH_Desc = "borderWiseOnlyInvoiceB";
			borderWiseOnlyInvoiceB.AH_OH = orgYYY.PK;
			borderWiseOnlyInvoiceB.AH_PostDate = new ZDateTime(2006, 1, 1);
			var borderWiseOnlyUsageB = Factory.NewWithValidTestData<EdiBilledUsage>();
			borderWiseOnlyUsageB.BU9_AH_Invoice = borderWiseOnlyInvoiceB.PK;
			borderWiseOnlyUsageB.BU9_BillingModel = BillingConstants.PriceHeaderType.BorderWise;
			borderWiseOnlyUsageB.BU9_LD = licenceDatabase.PK;

			var borderWiseUsageWithOdlpInvoice = Factory.NewWithValidTestData<EdiBilledUsage>();
			borderWiseUsageWithOdlpInvoice.BU9_AH_Invoice = odplInvoiceA.PK;
			borderWiseUsageWithOdlpInvoice.BU9_BillingModel = BillingConstants.PriceHeaderType.BorderWise;
			borderWiseUsageWithOdlpInvoice.BU9_LD = licenceDatabase.PK;

			var borderWiseUsageWithStlInvoice = Factory.NewWithValidTestData<EdiBilledUsage>();
			borderWiseUsageWithStlInvoice.BU9_AH_Invoice = stlInvoiceA.PK;
			borderWiseUsageWithStlInvoice.BU9_BillingModel = BillingConstants.PriceHeaderType.BorderWise;
			borderWiseUsageWithStlInvoice.BU9_LD = licenceDatabase.PK;

			Factory.Save();

			var context = new CreateCommissionContext() { FromDate = ZDateTime.MinSmallDateTimeValue };
			AssertContainsExactElementsInAnyOrder(
				BusinessObjectEqualityComparer<AccTransactionHeader>.PKOnlyComparer,
				x => x.AH_Desc,
				new AccTransactionHeader[] { stlInvoiceA, stlInvoiceB, stlInvoiceC, stlInvoiceD },
				Factory.Load<AccTransactionHeader>(EDICommissionAgreementApprover.GetInvoicesFilter(context, BillingConstants.PriceHeaderType.STL)));

			AssertContainsExactElementsInAnyOrder(
				BusinessObjectEqualityComparer<AccTransactionHeader>.PKOnlyComparer,
				x => x.AH_Desc,
				new AccTransactionHeader[] { odplInvoiceA, odplInvoiceB, odplInvoiceC, odplInvoiceD },
				Factory.Load<AccTransactionHeader>(EDICommissionAgreementApprover.GetInvoicesFilter(context, BillingConstants.PriceHeaderType.ODM)));

			AssertContainsExactElementsInAnyOrder(
				BusinessObjectEqualityComparer<AccTransactionHeader>.PKOnlyComparer,
				x => x.AH_Desc,
				new AccTransactionHeader[] { borderWiseOnlyInvoiceA, borderWiseOnlyInvoiceB },
				Factory.Load<AccTransactionHeader>(EDICommissionAgreementApprover.GetInvoicesFilter(context, BillingConstants.PriceHeaderType.BorderWise, new[] { BillingConstants.PriceHeaderType.ODM, BillingConstants.PriceHeaderType.STL })));

			context = new CreateCommissionContext() { FromDate = new ZDateTime(2004, 1, 1) };
			AssertContainsExactElementsInAnyOrder(
				BusinessObjectEqualityComparer<AccTransactionHeader>.PKOnlyComparer,
				x => x.AH_Desc,
				new AccTransactionHeader[] { stlInvoiceA, stlInvoiceB, stlInvoiceD },
				Factory.Load<AccTransactionHeader>(EDICommissionAgreementApprover.GetInvoicesFilter(context, BillingConstants.PriceHeaderType.STL)));

			AssertContainsExactElementsInAnyOrder(
				BusinessObjectEqualityComparer<AccTransactionHeader>.PKOnlyComparer,
				x => x.AH_Desc,
				new AccTransactionHeader[] { odplInvoiceA, odplInvoiceB, odplInvoiceD },
				Factory.Load<AccTransactionHeader>(EDICommissionAgreementApprover.GetInvoicesFilter(context, BillingConstants.PriceHeaderType.ODM)));

			AssertContainsExactElementsInAnyOrder(
				BusinessObjectEqualityComparer<AccTransactionHeader>.PKOnlyComparer,
				x => x.AH_Desc,
				new AccTransactionHeader[] { borderWiseOnlyInvoiceA, borderWiseOnlyInvoiceB },
				Factory.Load<AccTransactionHeader>(EDICommissionAgreementApprover.GetInvoicesFilter(context, BillingConstants.PriceHeaderType.BorderWise, new[] { BillingConstants.PriceHeaderType.ODM, BillingConstants.PriceHeaderType.STL })));

			var agreementsBeingApproved = new HashSet<OrgCommissionAgreement>() { agreementYYY };
			context = new CreateCommissionContext() { FromDate = ZDateTime.MinSmallDateTimeValue, AgreementsBeingApproved = agreementsBeingApproved };
			AssertContainsExactElementsInAnyOrder(
				BusinessObjectEqualityComparer<AccTransactionHeader>.PKOnlyComparer,
				x => x.AH_Desc,
				new AccTransactionHeader[] { stlInvoiceC, stlInvoiceD },
				Factory.Load<AccTransactionHeader>(EDICommissionAgreementApprover.GetInvoicesFilter(context, BillingConstants.PriceHeaderType.STL)));

			AssertContainsExactElementsInAnyOrder(
				BusinessObjectEqualityComparer<AccTransactionHeader>.PKOnlyComparer,
				x => x.AH_Desc,
				new AccTransactionHeader[] { odplInvoiceC, odplInvoiceD },
				Factory.Load<AccTransactionHeader>(EDICommissionAgreementApprover.GetInvoicesFilter(context, BillingConstants.PriceHeaderType.ODM)));

			AssertContainsExactElementsInAnyOrder(
				BusinessObjectEqualityComparer<AccTransactionHeader>.PKOnlyComparer,
				x => x.AH_Desc,
				new AccTransactionHeader[] { borderWiseOnlyInvoiceB },
				Factory.Load<AccTransactionHeader>(EDICommissionAgreementApprover.GetInvoicesFilter(context, BillingConstants.PriceHeaderType.BorderWise, new[] { BillingConstants.PriceHeaderType.ODM, BillingConstants.PriceHeaderType.STL })));

			agreementYYY.CA0_EffectiveDate = new ZDate(2004, 1, 1);
			Factory.Save();

			context = new CreateCommissionContext() { FromDate = ZDateTime.MinSmallDateTimeValue, AgreementsBeingApproved = agreementsBeingApproved };
			var query = EDICommissionAgreementApprover.GetInvoicesFilter(context, BillingConstants.PriceHeaderType.STL);
			query.ReLoadExistingRows = true;
			AssertContainsExactElementsInAnyOrder("Should not include stlInvoiceC as it's posted date is before agreement effective date",
				BusinessObjectEqualityComparer<AccTransactionHeader>.PKOnlyComparer,
				x => x.AH_Desc,
				new AccTransactionHeader[] { stlInvoiceD },
				Factory.Load<AccTransactionHeader>(query));

			query = EDICommissionAgreementApprover.GetInvoicesFilter(context, BillingConstants.PriceHeaderType.ODM);
			query.ReLoadExistingRows = true;
			AssertContainsExactElementsInAnyOrder("Should not include odplInvoiceC as it's posted date is before agreement effective date",
				BusinessObjectEqualityComparer<AccTransactionHeader>.PKOnlyComparer,
				x => x.AH_Desc,
				new AccTransactionHeader[] { odplInvoiceD },
				Factory.Load<AccTransactionHeader>(query));

			var existingAgreementYYYCommissionHeaderForBillingInvoiceC = Factory.NewWithValidTestData<AccCommissionHeader>();
			existingAgreementYYYCommissionHeaderForBillingInvoiceC.CH0_AH_Source = stlInvoiceC.PK;
			existingAgreementYYYCommissionHeaderForBillingInvoiceC.CH0_CA0 = agreementYYY.PK;

			Factory.Save();

			context = new CreateCommissionContext() { OverwriteOldValues = true, FromDate = ZDateTime.MinSmallDateTimeValue, AgreementsBeingApproved = agreementsBeingApproved };
			AssertContainsExactElementsInAnyOrder(@"Since we are overwriting old commissions, should include stlInvoiceC again (even though it isn't in the effective date range) as it has existing commission.
This is needed in the scenarios where commission needs to be canceled due to agreement effective date changing",
				BusinessObjectEqualityComparer<AccTransactionHeader>.PKOnlyComparer,
				x => x.AH_Desc,
				new AccTransactionHeader[] { stlInvoiceD, stlInvoiceC },
				Factory.Load<AccTransactionHeader>(EDICommissionAgreementApprover.GetInvoicesFilter(context, BillingConstants.PriceHeaderType.STL)));
		}

		public void TestGetNonJobRelatedInvoiceFilter_DoesNotIncludeBillingInvoices()
		{
			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement = opportunity.CommissionAgreementsForEdit.AddNew();
			agreement.FillWithValidTestData();
			agreement.CA0_OH_Customer = customer.PK;

			var invoiceWithoutUsageCharge = Factory.NewWithValidTestData<ARInvoice>();
			invoiceWithoutUsageCharge.AH_OH = customer.PK;

			var invoiceWithUsageCharge = Factory.NewWithValidTestData<ARInvoice>();
			invoiceWithUsageCharge.AH_OH = customer.PK;
			var usageCharge = Factory.NewWithValidTestData<ClientChargeableUsage>();
			usageCharge.U1_AH_Invoice = invoiceWithUsageCharge.PK;

			var invoiceWithoutUsageChargeButWithEdiBilledUsage = Factory.NewWithValidTestData<ARInvoice>();
			invoiceWithoutUsageChargeButWithEdiBilledUsage.AH_OH = customer.PK;
			var billedUsage = Factory.NewWithValidTestData<EdiBilledUsage>();
			billedUsage.BU9_AH_Invoice = invoiceWithoutUsageChargeButWithEdiBilledUsage.PK;

			Factory.Save();

			var approver = EDICommissionAgreementApprover.New(Factory);
			AssertContainsExactElementsInAnyOrder("Should exclude invoiceWithUsageCharge",
				BusinessObjectEqualityComparer<AccTransactionHeader>.PKOnlyComparer,
				new AccTransactionHeader[] { invoiceWithoutUsageCharge, },
				Factory.Load<AccTransactionHeader>(approver.GetNonJobRelatedTransactionFilterForTesting(new CreateCommissionContext())));
		}

		public void TestGetNonJobRelatedInvoiceFilter_NonAllAllAllAgreementsShouldBeAbleToBeMatched()
		{
			var chargeCodeWithCommissionDefaults = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCodeWithCommissionDefaults.AC_IsCommissionable = true;
			chargeCodeWithCommissionDefaults.AC_DefaultCommissionProduct = "ENT";
			chargeCodeWithCommissionDefaults.AC_DefaultCommissionService = "XXX";
			chargeCodeWithCommissionDefaults.AC_DefaultCommissionSubModule = "ALL";

			var opportunityClient = Factory.NewWithValidTestData<OrgHeader>();
			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_OH = opportunityClient.PK;
			var agreement = opportunity.CommissionAgreementsForEdit.AddNew();
			agreement.FillWithValidTestData();
			agreement.CA0_OH_Customer = customer.PK;
			OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(agreement, "ENT", "XXX", "ALL");

			var invoiceWithCustomerAsDebtor = Factory.NewWithValidTestData<ARInvoice>();
			invoiceWithCustomerAsDebtor.AH_OH = customer.PK;

			var invoiceWithOpportunityClientAsDebtor = Factory.NewWithValidTestData<ARInvoice>();
			invoiceWithOpportunityClientAsDebtor.AH_OH = opportunityClient.PK;

			Factory.Save();

			var context = new CreateCommissionContext() { AgreementsBeingApproved = new HashSet<OrgCommissionAgreement>() { agreement } };
			var approver = EDICommissionAgreementApprover.New(Factory);
			AssertContainsExactElementsInAnyOrder("Should include both invoices",
				BusinessObjectEqualityComparer<AccTransactionHeader>.PKOnlyComparer,
				new AccTransactionHeader[] { invoiceWithCustomerAsDebtor, invoiceWithOpportunityClientAsDebtor },
				Factory.Load<AccTransactionHeader>(approver.GetNonJobRelatedTransactionFilterForTesting(context)));
		}

		public void TestCreateCommissions_OdplAndStlShouldDeleteLegacyAmbigiousCommissions()
		{
			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var licenceCompany = Factory.NewWithValidTestData<LicenceCompany>();
			licenceCompany.LC_OH = org.PK;

			var agreement1 = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreement(Factory, org, "ENT", "STL", "USR");
			var agreement2 = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreement(Factory, org, "ENT", "STL", "SHP");

			var periodStart = new ZDate(2004, 3, 1);
			var postDate = periodStart.AddMonths(1).AddDays(3);
			var stlInvoice = Factory.NewWithValidTestData<ARInvoice>();
			stlInvoice.AH_OH = org.PK;
			stlInvoice.AH_PostDate = postDate;
			var stlUsrBilledUsage = Factory.NewWithValidTestData<EdiBilledUsage>();
			stlUsrBilledUsage.BU9_AH_Invoice = stlInvoice.PK;
			stlUsrBilledUsage.BU9_BillingModel = BillingConstants.PriceHeaderType.STL;
			stlUsrBilledUsage.BU9_LD = licenceDatabase.PK;
			stlUsrBilledUsage.BU9_UsageCode = "STL";
			stlUsrBilledUsage.BU9_UsageSubCode = "USR";
			stlUsrBilledUsage.BU9_PeriodStart = periodStart;

			var stlAmbigiousCommission = Factory.NewWithValidTestData<AccAmbiguousCommission>();
			stlAmbigiousCommission.AC0_AH_Source = stlInvoice.PK;
			stlAmbigiousCommission.AC0_CA0_SelectedAgreement = agreement1.PK;

			var odplInvoice = Factory.NewWithValidTestData<ARInvoice>();
			odplInvoice.AH_OH = org.PK;
			odplInvoice.AH_PostDate = postDate;
			var odplBilledUsage = Factory.NewWithValidTestData<EdiBilledUsage>();
			odplBilledUsage.BU9_AH_Invoice = odplInvoice.PK;
			odplBilledUsage.BU9_BillingModel = BillingConstants.PriceHeaderType.ODM;
			odplBilledUsage.BU9_LD = licenceDatabase.PK;
			odplBilledUsage.BU9_UsageCode = "SHP";
			odplBilledUsage.BU9_UsageSubCode = "#HR";
			odplBilledUsage.BU9_PeriodStart = periodStart;

			var odplAmbigiousCommission = Factory.NewWithValidTestData<AccAmbiguousCommission>();
			odplAmbigiousCommission.AC0_AH_Source = odplInvoice.PK;
			odplAmbigiousCommission.AC0_CA0_SelectedAgreement = agreement1.PK;

			var nonJobRelatedInvoice = Factory.NewWithValidTestData<ARInvoice>();
			nonJobRelatedInvoice.AH_OH = org.PK;
			nonJobRelatedInvoice.AH_PostDate = postDate;
			var nonJobRelatedAmbigiousCommission = Factory.NewWithValidTestData<AccAmbiguousCommission>();
			nonJobRelatedAmbigiousCommission.AC0_AH_Source = nonJobRelatedInvoice.PK;
			nonJobRelatedAmbigiousCommission.AC0_CA0_SelectedAgreement = agreement1.PK;

			Factory.Save();

			var approver = new EDICommissionAgreementApproverForTest(Factory);
			var createContext = new CreateCommissionContext()
			{
				AgreementsBeingApproved = new HashSet<OrgCommissionAgreement> { agreement1, agreement2 }
			};
			approver.CreateCommissionsCore_Exposed(createContext, null);

			CombineAssertions("Should have deleted ambiguous commission for stl and odpl invoices", () =>
			{
				AssertEquals("stlAmbigiousCommission", true, stlAmbigiousCommission.IsDeleted);
				AssertEquals("odplAmbigiousCommission", true, odplAmbigiousCommission.IsDeleted);
				AssertEquals("nonJobRelatedAmbigiousCommission", false, nonJobRelatedAmbigiousCommission.IsDeleted);
			});
		}

		public void TestCreateCommissions_StlBorderWiseInvociesAreProcessedOnlyOnce()
		{
			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "STF";

			var chargeCodeBorderWise = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCodeBorderWise.AC_IsCommissionable = true;

			var chargeCodeStl = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCodeStl.AC_IsCommissionable = true;

			var agreement = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreement(Factory, org, "ALL", "ALL", "ALL");
			agreement.CA0_CommissionBasis = CommissionBasisType.Codes.PRF;
			var recipient = agreement.Recipients.AddNew();
			recipient.CAR_GS_NKStaff = staff.GS_Code;
			recipient.CAR_CommissionType = CommissionTypes.Codes.PCT;
			recipient.CAR_Share = 1;
			var recipientRate = recipient.Rates.AddNew();
			recipientRate.CAT_CommissionPercentage = 50;

			var periodStart = new ZDate(2004, 3, 1);
			var postDate = periodStart.AddMonths(1).AddDays(3);

			//BorderWise only setup
			var borderWiseOnlyInvoice = Factory.New<ARInvoice>();
			borderWiseOnlyInvoice.AH_Desc = "borderWiseOnlyInvoice";
			borderWiseOnlyInvoice.AH_OH = org.PK;
			borderWiseOnlyInvoice.AH_PostDate = postDate;
			borderWiseOnlyInvoice.AH_InvoiceAmount = 158.50;
			borderWiseOnlyInvoice.AH_OutstandingAmount = 158.50;

			var borderWiseOnlyInvoiceLine = borderWiseOnlyInvoice.Lines.AddNew() as InvoicingLineBase;
			borderWiseOnlyInvoiceLine.AL_AC = chargeCodeBorderWise.PK;
			borderWiseOnlyInvoiceLine.AL_LineAmount = 158.50;
			borderWiseOnlyInvoiceLine.AL_OSAmount = 158.50;
			borderWiseOnlyInvoiceLine.AL_RX_NKTransactionCurrency = "AUD";

			var borderWiseOnlyUsage = Factory.NewWithValidTestData<EdiBilledUsage>();
			borderWiseOnlyUsage.BU9_AH_Invoice = borderWiseOnlyInvoice.PK;
			borderWiseOnlyUsage.BU9_BillingModel = BillingConstants.PriceHeaderType.BorderWise;
			borderWiseOnlyUsage.BU9_LD = licenceDatabase.PK;
			borderWiseOnlyUsage.BU9_UsageCode = "BOR";
			borderWiseOnlyUsage.BU9_UsageSubCode = "BOR";
			borderWiseOnlyUsage.BU9_PeriodStart = periodStart;
			borderWiseOnlyUsage.BU9_LocalAmountPreDiscount = 158.50;
			borderWiseOnlyUsage.BU9_LocalAmountPostDiscount = 158.50;
			borderWiseOnlyUsage.BU9_AC_AmountChargeCode = chargeCodeBorderWise.PK;

			//STL + BorderWise setup
			var stlInvoice = Factory.New<ARInvoice>();
			stlInvoice.AH_Desc = "stlInvoice";
			stlInvoice.AH_OH = org.PK;
			stlInvoice.AH_PostDate = postDate;
			stlInvoice.AH_InvoiceAmount = 124;
			stlInvoice.AH_OutstandingAmount = 124;

			var stlInvoiceLine = stlInvoice.Lines.AddNew() as InvoicingLineBase;
			stlInvoiceLine.AL_AC = chargeCodeStl.PK;
			stlInvoiceLine.AL_LineAmount = 124;
			stlInvoiceLine.AL_OSAmount = 124;
			stlInvoiceLine.AL_RX_NKTransactionCurrency = "AUD";

			var borderWiseInvoiceLine = stlInvoice.Lines.AddNew() as InvoicingLineBase;
			borderWiseInvoiceLine.AL_AC = chargeCodeBorderWise.PK;
			borderWiseInvoiceLine.AL_LineAmount = 24;
			borderWiseInvoiceLine.AL_OSAmount = 24;
			borderWiseInvoiceLine.AL_RX_NKTransactionCurrency = "AUD";

			var stlUsageForStlInvoice = Factory.NewWithValidTestData<EdiBilledUsage>();
			stlUsageForStlInvoice.BU9_AH_Invoice = stlInvoice.PK;
			stlUsageForStlInvoice.BU9_BillingModel = BillingConstants.PriceHeaderType.STL;
			stlUsageForStlInvoice.BU9_LD = licenceDatabase.PK;
			stlUsageForStlInvoice.BU9_UsageCode = "STL";
			stlUsageForStlInvoice.BU9_UsageSubCode = "SHP";
			stlUsageForStlInvoice.BU9_PeriodStart = periodStart;
			stlUsageForStlInvoice.BU9_LocalAmountPreDiscount = 124;
			stlUsageForStlInvoice.BU9_LocalAmountPostDiscount = 124;
			stlUsageForStlInvoice.BU9_AC_AmountChargeCode = chargeCodeStl.PK;

			var borderWiseUsageForStlInvoice = Factory.NewWithValidTestData<EdiBilledUsage>();
			borderWiseUsageForStlInvoice.BU9_AH_Invoice = stlInvoice.PK;
			borderWiseUsageForStlInvoice.BU9_BillingModel = BillingConstants.PriceHeaderType.BorderWise;
			borderWiseUsageForStlInvoice.BU9_LD = licenceDatabase.PK;
			borderWiseUsageForStlInvoice.BU9_UsageCode = "BOR";
			borderWiseUsageForStlInvoice.BU9_UsageSubCode = "AAA";
			borderWiseUsageForStlInvoice.BU9_PeriodStart = periodStart;
			borderWiseUsageForStlInvoice.BU9_LocalAmountPreDiscount = 24;
			borderWiseUsageForStlInvoice.BU9_LocalAmountPostDiscount = 24;
			borderWiseUsageForStlInvoice.BU9_AC_AmountChargeCode = chargeCodeBorderWise.PK;

			Factory.Save();

			var createContext = new CreateCommissionContext()
			{
				AgreementsBeingApproved = new HashSet<OrgCommissionAgreement> { agreement },
			};
			var progress = new List<(string Status, int PercentComplete)>();
			var approver = new EDICommissionAgreementApproverForTest(Factory);
			approver.CreateCommissionsCore_Exposed(createContext, (string status, int percentComplete) => { progress.Add((status, percentComplete)); });

			var progressMessage = new ZStringBuilder();
			progress.ForEach(p => progressMessage.AppendLine($"{p.Status}: {p.PercentComplete}"));

			var expectedProgress =
@"Creating STL Billing Commissions: 1 of 1: 0
Creating STL Billing Commissions: 1 of 1: 100
Creating BorderWise Billing Commissions: 1 of 1: 0
Creating BorderWise Billing Commissions: 1 of 1: 100
Creating Non Job Related Commissions: 0 of 0: 100
Creating Job Related Commissions: 0 of 0: 100
Saving..: 0
Saving..: 100";

			AssertMultilineASCIIEquals(expectedProgress, progressMessage.ToString());
		}

		public void TestCreateCommissions_ErrorHandling()
		{
			var invoice = Factory.New<ARInvoice>();
			invoice.AH_PostDate = new ZDateTime(2014, 1, 1);

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "TEST";
			chargeCode.AC_IsCommissionable = true;

			var line = (TransactionLine)invoice.Lines.AddNew();
			line.AL_AC = chargeCode.PK;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "DDDAAASYD";
			var licCompany1 = Factory.NewWithValidTestData<LicenceCompany>();
			licCompany1.LC_OH = org1.PK;

			var invoiceDelivery1 = licCompany1.InvoiceDeliveries.AddNew();
			invoiceDelivery1.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;
			invoiceDelivery1.L9_OH_InvoiceTo = org1.PK;

			var usage1 = Factory.New<ClientChargeableUsage>();
			usage1.U1_Code = BillingConstants.BillingSystem.DeniedPartyScreening;
			usage1.U1_SubCode = "DPS";
			usage1.U1_AH_Invoice = invoice.PK;
			usage1.U1_LC = licCompany1.PK;
			usage1.U1_PeriodStart = new ZDateTime(2014, 1, 1);

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "DDD123MEL";
			var licCompany2 = Factory.NewWithValidTestData<LicenceCompany>();
			licCompany2.LC_OH = org2.PK;

			var priceHeader = licCompany2.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = BillingConstants.BillingSystem.ODM;
			priceHeader.L6_ValidFrom = new ZDateTime(2013, 1, 1);
			priceHeader.L6_RX_NKCurrency = "";
			BillingTestHelper.AddPriceItem(priceHeader, "AFR", BillingConstants.FeeType.Transactional, "", 1.50m).L7_Description = "Pre Departure Sea Manifest Filing (AFR)";

			var invoiceDelivery2 = licCompany2.InvoiceDeliveries.AddNew();
			invoiceDelivery2.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;
			invoiceDelivery2.L9_OH_InvoiceTo = org1.PK;

			var usage2 = Factory.New<ClientChargeableUsage>();
			usage2.U1_Code = BillingConstants.BillingSystem.JapanAFR;
			usage2.U1_SubCode = "AFR";
			usage2.U1_AH_Invoice = invoice.PK;
			usage2.U1_LC = licCompany2.PK;
			usage2.U1_PeriodStart = new ZDateTime(2014, 1, 1);

			var opportunity1 = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, org1);
			opportunity1.P8_OpportunityID = "O00003001";
			var agreement1 = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreementForAllItems(opportunity1, org1);
			agreement1.CA0_Name = "#1";
			agreement1.CA0_EffectiveDate = new ZDate(2010, 1, 1);

			var opportunity2_1 = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, org2);
			opportunity2_1.P8_OpportunityID = "O00003002";
			var agreement2_1 = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreementForAllItems(opportunity2_1, org2);
			agreement2_1.CA0_Name = "Test 1";
			agreement2_1.CA0_EffectiveDate = new ZDate(2011, 1, 1);

			var opportunity2_2 = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, org2);
			opportunity2_2.P8_OpportunityID = "O00003003";
			var agreement2_2 = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreementForAllItems(opportunity2_2, org2);
			agreement2_2.CA0_Name = "Test 2";
			agreement2_2.CA0_EffectiveDate = new ZDate(2013, 1, 1);

			Factory.Save();

			//TODO: Hello, fellow developer! If this test is failing, uncomment the "DISABLE TRIGGER" command below and remove this TODO line
			//HACK: Temporarily allow UPDATE of AH_InvoiceAmount to test Revenue Breakdown checks. Refer to WI00559931, WI00482153.
			//TestConnection.ExecuteNonQuery("DISABLE TRIGGER TG_AccTransactionHeader_ProtectCriticalFieldsFromUpdating ON AccTransactionHeader");
			TestConnection.ExecuteNonQuery($"update dbo.AccTransactionHeader set AH_InvoiceAmount = 1, AH_SystemLastEditTimeUtc = GETUTCDATE(), AH_SystemLastEditUser = 'TST' where AH_PK = '{invoice.PK}';");
			invoice.Reload();

			Env.OutgoingMailManager.EmailsCreated.Clear();
			var newFactory = new BusinessObjectFactory();
			var staff = newFactory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "LDK";
			staff.GS_EmailAddress = "someone@test.com";
			var group = newFactory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(staff);
			newFactory.Save();
			EnvProxy.Instance.Registry.MailboxEmailAddress = "test@example.com";
			EDIDataRegistry.Instance.CommissionGeneratorNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			string expectedErrorMessage = @"The following Invoices were created prior to July 2016 and therefore have no associated revenue breakdown. As a result, these invoices cannot be processed by the Commission Agreement Generator and must be processed manually.

In order to process these Commission Agreements, please set the Backdate Commission Options 'From' date on the Commission Agreement Approval form to a Specified Date from 01-July-2016, and then reprocess the agreement.

Invoice(s): 00001000

Agreement(s): O00003001#1, O00003002Test 1, O00003003Test 2";

			AssertExceptionThrown("Should throw exception because no Revenue Breakdown",
				typeof(CommissionAgreementApprovalWizardException),
				expectedErrorMessage,
				() =>
				{
					using (var transactionManager = ((ITransactionParticipant)newFactory).BeginTransactionWithManager())
					{
						var approver = (EDICommissionAgreementApprover)CommissionAgreementApprover.New(newFactory);
						var context = new CreateCommissionContext() { FromDate = ZDateTime.MinSmallDateTimeValue };
						context.AgreementsBeingApproved = new HashSet<OrgCommissionAgreement>(new[] { agreement1, agreement2_1, agreement2_2 });
						approver.CreateCommissions(context, null);

						transactionManager.CommitTransaction();
					}
				});

			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Error generating commissions", email.Subject);
			AssertEquals(expectedErrorMessage, email.Body);
		}

		public void TestNonCommissionableITransactionsExcluded()
		{
			var orgXXX = Factory.NewWithValidTestData<OrgHeader>();

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "TEST";
			chargeCode.AC_IsCommissionable = false;

			var agreementXXX = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreement(Factory, orgXXX);
			agreementXXX.CA0_EffectiveDate = new ZDate(2002, 2, 2);

			var context = new CreateCommissionContext
			{
				FromDate = ZDateTime.MinSmallDateTimeValue,
				AgreementsBeingApproved = new HashSet<OrgCommissionAgreement> { agreementXXX }
			};

			var odplInvoiceA = Factory.New<ARInvoice>();
			odplInvoiceA.AH_Desc = "odplInvoiceA";
			odplInvoiceA.AH_OH = orgXXX.PK;
			odplInvoiceA.AH_PostDate = new ZDateTime(2004, 4, 4);

			var nonCommissionableLine = (TransactionLine)odplInvoiceA.Lines.AddNew();
			nonCommissionableLine.AL_AC = chargeCode.PK;

			var licenceCompany = Factory.NewWithValidTestData<LicenceCompany>();
			licenceCompany.LC_OH = orgXXX.PK;

			var clientChargeableUsage = Factory.NewWithValidTestData<ClientChargeableUsage>();
			clientChargeableUsage.U1_AH_Invoice = odplInvoiceA.PK;
			clientChargeableUsage.U1_Code = BillingConstants.BillingSystem.ODM;
			clientChargeableUsage.U1_LC = licenceCompany.PK;

			Factory.Save();

			var filter = EDICommissionAgreementApprover.GetOdplInvoicesWithoutRevenueBreakdownFilter(context);
			var invoices = Factory.Load<InvoicingBase>(filter).Select(x => x.AH_TransactionNum);

			AssertEquals("Non-commissionable invoice should be excluded.", 0, invoices.Count());
		}

		public void TestNonCommissionableITransactionsExcluded_InvoiceWithMixedLines()
		{
			var orgXXX = Factory.NewWithValidTestData<OrgHeader>();

			var nonCommissionableChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			nonCommissionableChargeCode.AC_Code = "UNCOM";
			nonCommissionableChargeCode.AC_IsCommissionable = false;

			var commissionableChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			commissionableChargeCode.AC_Code = "COMM";
			commissionableChargeCode.AC_IsCommissionable = true;

			var agreementXXX = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreement(Factory, orgXXX);
			agreementXXX.CA0_EffectiveDate = new ZDate(2002, 2, 2);

			var context = new CreateCommissionContext
			{
				FromDate = ZDateTime.MinSmallDateTimeValue,
				AgreementsBeingApproved = new HashSet<OrgCommissionAgreement> { agreementXXX }
			};

			var odplInvoiceA = Factory.New<ARInvoice>();
			odplInvoiceA.AH_Desc = "odplInvoiceA";
			odplInvoiceA.AH_OH = orgXXX.PK;
			odplInvoiceA.AH_PostDate = new ZDateTime(2004, 4, 4);

			var nonCommissionableLine1 = (TransactionLine)odplInvoiceA.Lines.AddNew();
			nonCommissionableLine1.AL_AC = nonCommissionableChargeCode.PK;

			var nonCommissionableLine2 = (TransactionLine)odplInvoiceA.Lines.AddNew();
			nonCommissionableLine2.AL_AC = nonCommissionableChargeCode.PK;

			var commissionableLine1 = (TransactionLine)odplInvoiceA.Lines.AddNew();
			commissionableLine1.AL_AC = commissionableChargeCode.PK;

			var commissionableLine2 = (TransactionLine)odplInvoiceA.Lines.AddNew();
			commissionableLine2.AL_AC = commissionableChargeCode.PK;

			var licenceCompany = Factory.NewWithValidTestData<LicenceCompany>();
			licenceCompany.LC_OH = orgXXX.PK;

			var clientChargeableUsage = Factory.NewWithValidTestData<ClientChargeableUsage>();
			clientChargeableUsage.U1_AH_Invoice = odplInvoiceA.PK;
			clientChargeableUsage.U1_Code = BillingConstants.BillingSystem.ODM;
			clientChargeableUsage.U1_LC = licenceCompany.PK;

			Factory.Save();

			var filter = EDICommissionAgreementApprover.GetOdplInvoicesWithoutRevenueBreakdownFilter(context);
			var invoices = Factory.Load<InvoicingBase>(filter).Select(x => x.AH_TransactionNum);

			AssertEquals("Invoice with mixed commissionable and non-commissionable lines should NOT be excluded.", 1, invoices.Count());
		}

		#endregion
	}

	class EDICommissionAgreementApproverForTest : EDICommissionAgreementApprover
	{
		public EDICommissionAgreementApproverForTest(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public void CreateCommissionsCore_Exposed(CreateCommissionContext context, Progress progress)
		{
			base.CreateCommissionsCore(context, progress);
		}
	}
}
