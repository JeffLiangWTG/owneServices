using System;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Fee.Test
{
	[TestedType(typeof(FeeSystemUsage))]
	internal class FeeSystemUsageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSystemCode()
		{
			FeeSystemUsage feeUsage = CreateFeeUsage();
			AssertEquals(BillingConstants.BillingSystem.Fee, feeUsage.SystemCode);
		}

		public void TestFees()
		{
			ClientLicenceFee unmatchedFee = BillingTestHelper.CreateLicenceFee(Organisation.LicCompany, "ZZZ", 10m);
			unmatchedFee.L8_StartDate = BillingDate.AddMonths(3);

			FeeSystemUsage feeUsage = CreateFeeUsage(Factory, Organisation, BillingDate, false);
			AssertEquals("Contains only matched fees", 3, feeUsage.Fees.Count());
			AssertEquals(true, feeUsage.Fees.Any(x => x.L8_Type == "AAA"));
			AssertEquals(true, feeUsage.Fees.Any(x => x.L8_Type == "BBB"));
			AssertEquals(true, feeUsage.Fees.Any(x => x.L8_Type == "CCC"));

			Organisation.LicCompany.Fees.DeleteAll();
			feeUsage = CreateFeeUsage(Factory, Organisation, BillingDate, false);
			AssertEquals("Empty when no matched fees", 0, feeUsage.Fees.Count());
		}

		public void TestAmount()
		{
			EDIOrgHeader organisationWithoutFees = BillingTestHelper.CreateOrganisation(Factory, "ABC");
			FeeSystemUsage feeUsage = CreateFeeUsage(Factory, organisationWithoutFees, BillingDate, false);
			AssertEquals("Default when no fees", 0m, feeUsage.Amount);

			feeUsage = CreateFeeUsage(Factory, Organisation, BillingDate, false);
			AssertEquals("Amount is the sum of amounts from matched by date fees", 10.24m + 20.48m + 40.96m, feeUsage.Amount);

			feeUsage = CreateFeeUsage(Factory, Organisation, BillingDate, true);
			AssertEquals("Remit to amount is the sum of amounts from matched by date fees", -(10.24m + 20.48m + 40.96m), feeUsage.Amount);
		}

		public void TestAmountExemptProcessingFee()
		{
			var feeTypes = new CodeDescriptionBoolCollection();
			feeTypes.AddRange(EDIDataRegistry.Instance.LicenceFeeTypes.Value);
			var exemptFee = feeTypes.AddNew();
			exemptFee.Code = "CCC";
			exemptFee.Bool = true;
			EDIDataRegistry.Instance.LicenceFeeTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, feeTypes);
			EDIOrgHeader org = BillingTestHelper.CreateOrganisation(Factory, "ABC");
			FeeSystemUsage feeUsage = CreateFeeUsage(Factory, org, BillingDate, false);
			AssertEquals("Default when no fees", 0m, feeUsage.AmountExemptProcessingFee);

			feeUsage = CreateFeeUsage(Factory, Organisation, BillingDate, false);
			AssertEquals("Amount is the sum of amounts from matched by date fees", 10.24m + 20.48m + 40.96m, feeUsage.Amount);
			AssertEquals("AmountExemptProcessingFee is CCC fee type only", 40.96m, feeUsage.AmountExemptProcessingFee);
		}

		public void TestCurrencyCode()
		{
			FeeSystemUsage feeUsage = CreateFeeUsage(Factory, Organisation, BillingDate, false);
			Organisation.LicCompany.Fees[0].L8_RX_NKCurrency = "";
			AssertEquals("Not specified", "", feeUsage.CurrencyCode);

			Organisation.LicCompany.Fees[0].L8_RX_NKCurrency = "AUD";
			AssertEquals("AUD", feeUsage.CurrencyCode);

			Organisation.LicCompany.Fees[0].L8_RX_NKCurrency = "USD";
			AssertEquals("USD", feeUsage.CurrencyCode);
		}

		public void TestGetGeneralSummarySections()
		{
			FeeSystemUsage feeUsage = CreateFeeUsage(Factory, Organisation, BillingDate, false);

			SummarySection[] summarySections = feeUsage.GetGeneralSummarySections();
			AssertEquals("Summary sections", 1, summarySections.Length);

			SummaryLine summaryHeader = summarySections[0].Header;
			AssertEquals("Product Fees", summaryHeader.MainDescription);
			AssertEquals("", summaryHeader.AdditionalDescription);
			AssertEquals("", summaryHeader.UnitCount);
			AssertEquals("", summaryHeader.UnitPrice);
			AssertEquals("Amount", summaryHeader.Amount);
			AssertEquals(feeUsage.Amount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture), summaryHeader.TotalAmount);

			AssertEquals("Summary line for each matched fee", 3, summarySections[0].Lines.Count);
			AssertSummaryLine(summarySections[0].Lines[0], "AAA fee description", 10.24m);
			AssertSummaryLine(summarySections[0].Lines[1], "BBB fee description", 20.48m);
			AssertSummaryLine(summarySections[0].Lines[2], "CCC fee description", 40.96m);

			feeUsage = CreateFeeUsage(Factory, Organisation, BillingDate, true);
			summarySections = feeUsage.GetGeneralSummarySections();
			summaryHeader = summarySections[0].Header;
			AssertEquals("Product Fees (Remitable)", summaryHeader.MainDescription);
			AssertSummaryLine(summarySections[0].Lines[0], "Remit: AAA fee description", -10.24m);
			AssertSummaryLine(summarySections[0].Lines[1], "Remit: BBB fee description", -20.48m);
			AssertSummaryLine(summarySections[0].Lines[2], "Remit: CCC fee description", -40.96m);
		}

		void AssertSummaryLine(SummaryLine summaryLine, ZString description, ZDecimal amount)
		{
			AssertEquals("Description", description, summaryLine.MainDescription);
			AssertEquals("Amount", amount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture), summaryLine.Amount);
			AssertEquals("", summaryLine.AdditionalDescription);
			AssertEquals("", summaryLine.UnitCount);
			AssertEquals("", summaryLine.UnitPrice);
		}

		internal static FeeSystemUsage CreateFeeUsage(BusinessObjectFactory factory, EDIOrgHeader org, ZDateTime periodStart, bool isRemitTo)
		{
			var fees = org.LicCompany.Fees.GetMatched(periodStart).Where(s => s.L8_SystemCode == BillingConstants.BillingSystem.ODM);
			return new FeeSystemUsage(factory, new UsingParty(org), periodStart, fees.ToArray(), isRemitTo);
		}

		#region Implementation

		readonly ZDateTime BillingDate = new ZDateTime(2010, 12, 1);
		EDIOrgHeader Organisation;

		protected override void SetUp()
		{
			base.SetUp();

			Organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			BillingTestHelper.CreateLicenceFee(Organisation.LicCompany, "AAA", 10.24m);
			BillingTestHelper.CreateLicenceFee(Organisation.LicCompany, "BBB", 20.48m);
			BillingTestHelper.CreateLicenceFee(Organisation.LicCompany, "CCC", 40.96m);

			BillingTestHelper.CreateLicenceFee(Organisation.LicCompany, "XXX", "XXX fee -- should be unmatched", 100m, "XXXCODE", BillingDate.AddMonths(1), ZDateTime.Empty);
		}

		FeeSystemUsage CreateFeeUsage()
		{
			return GetNewBusinessObject() as FeeSystemUsage;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new FeeSystemUsage(Factory, new UsingParty(), EdiDateTest.MonthToday, Array.Empty<ClientLicenceFee>());
		}

		#endregion
	}
}
