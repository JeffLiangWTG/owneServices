using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.ODPL;
using Enterprise.Client.EDI.Billing.ODPL.Test;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(DocOrganisationBill))]
	internal class DocOrganisationBillTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGeneralSummary()
		{
			foreach (SystemBillForTesting systemBill in OrganisationBill.SystemBills)
			{
				AssertEquals("Precondition", false, systemBill.MethodWasCalled("GetGeneralSummarySections"));
			}

			DocOrganisationBill docOrganisationBill = DocOrganisationBill.New(OrganisationBill, Factory, OrganisationBill.OrganisationPK);
			foreach (SystemBillForTesting systemBill in OrganisationBill.SystemBills)
			{
				AssertEquals(true, systemBill.MethodWasCalled("GetGeneralSummarySections" + Organisation1.PK.ToString()));
			}
			AssertEquals(3, docOrganisationBill.GeneralSummaryLines.Count);

			AssertGeneralSummaryLine(docOrganisationBill.GeneralSummaryLines[0], Organisation1.PK.ToString(), "AUD");
			AssertGeneralSummaryLine(docOrganisationBill.GeneralSummaryLines[1], Organisation2.PK.ToString(), "NZD");
			AssertGeneralSummaryLine(docOrganisationBill.GeneralSummaryLines[2], Organisation3.PK.ToString(), "USD");

			docOrganisationBill = DocOrganisationBill.New(OrganisationBill, Factory, Organisation2.PK);
			foreach (SystemBillForTesting systemBill in OrganisationBill.SystemBills)
			{
				AssertEquals(true, systemBill.MethodWasCalled("GetGeneralSummarySections" + Organisation2.PK.ToString()));
			}

			//pricelist Without Licence Units
			foreach (SystemBillWithoutLicenceUnitsForTesting systemBill in OrganisationBillWithoutLicenceUnits.SystemBills)
			{
				AssertEquals("Precondition", false, systemBill.MethodWasCalled("GetGeneralSummarySections"));
			}
			DocOrganisationBill docOrganisationBillWithoutLicenceUnits = DocOrganisationBill.New(OrganisationBillWithoutLicenceUnits, Factory, OrganisationBillWithoutLicenceUnits.OrganisationPK);
			foreach (SystemBillWithoutLicenceUnitsForTesting systemBill in OrganisationBillWithoutLicenceUnits.SystemBills)
			{
				AssertEquals(true, systemBill.MethodWasCalled("GetGeneralSummarySections" + OrganisationWithoutLicenceUnits1.PK.ToString()));
			}
			AssertEquals(0, docOrganisationBillWithoutLicenceUnits.GeneralSummaryLines.Count);
			AssertEquals(1, docOrganisationBillWithoutLicenceUnits.GeneralSummaryLinesWithoutLicenceUnits.Count);
			AssertGeneralSummaryLine(docOrganisationBillWithoutLicenceUnits.GeneralSummaryLinesWithoutLicenceUnits[0], OrganisationWithoutLicenceUnits1.PK.ToString(), "AUD", false);
		}

		void AssertGeneralSummaryLine(SummaryLine generalSummaryLine, ZString mainDescription, ZString currencyCode, bool newPricelist = true)
		{
			AssertEquals(mainDescription, generalSummaryLine.MainDescription);
			if (newPricelist)
			{
				AssertEquals(ZString.Format("Total ({0}):", currencyCode).Trim(), generalSummaryLine.Header.TotalDescription.Trim());
				AssertEquals(ZString.Format("Total Licence Units:", currencyCode).Trim(), generalSummaryLine.Header.TotalLicenceUnitsDescription.Trim());
			}
			else
			{
				AssertEquals(ZString.Format("Total ({0})", currencyCode).Trim(), generalSummaryLine.Header.TotalDescription.Trim());
			}
		}

		public void TestDiscountSummary()
		{
			foreach (SystemBillForTesting systemBill in OrganisationBill.SystemBills)
			{
				AssertEquals("Precondition", false, systemBill.MethodWasCalled("GetDiscountSummarySections"));
			}

			DocOrganisationBill docOrganisationBill = DocOrganisationBill.New(OrganisationBill, Factory, OrganisationBill.OrganisationPK);
			foreach (SystemBillForTesting systemBill in OrganisationBill.SystemBills)
			{
				AssertEquals("Discount sections for the main organisation", true, systemBill.MethodWasCalled("GetDiscountSummarySections"));
				AssertEquals("Surcharge sections for the main organisation", true, systemBill.MethodWasCalled("GetSurchargeSummarySections"));
			}

			AssertEquals(9, docOrganisationBill.DiscountSummaryLines.Count);
			AssertEquals("Usage Calculation", docOrganisationBill.DiscountSummaryLines[0].Header.MainDescription);

			foreach (SystemBillForTesting systemBill in OrganisationBill.SystemBills)
			{
				systemBill.ClearTextNotifications();
			}

			docOrganisationBill = DocOrganisationBill.New(OrganisationBill, Factory, Organisation2.PK);
			foreach (SystemBillForTesting systemBill in OrganisationBill.SystemBills)
			{
				AssertEquals("No discount for the secondary organisations", false, systemBill.MethodWasCalled("GetDiscountSummarySections"));
				AssertEquals("No surcharge for the secondary organisations", false, systemBill.MethodWasCalled("GetSurchargeSummarySections"));
			}
		}

		public void TestGroupSummary()
		{
			DocOrganisationBill docOrganisationBill = DocOrganisationBill.New(OrganisationBill, Factory, OrganisationBill.OrganisationPK);
			IEnumerable<SummaryLine> summaryLines = docOrganisationBill.GroupSummaryLines.Cast<SummaryLine>();
			AssertEquals("3 group summary, one for each system bill", 3, summaryLines.Select(x => x.Header).Distinct().Count());

			AssertEquals("Contains group summary from Organisation1", true, summaryLines.Any(x => x.Header.MainDescription == Organisation1.PK.ToString()));
			AssertEquals("Contains group summary from Organisation2", true, summaryLines.Any(x => x.Header.MainDescription == Organisation2.PK.ToString()));
			AssertEquals("Contains group summary from Organisation3", true, summaryLines.Any(x => x.Header.MainDescription == Organisation3.PK.ToString()));

			AssertEquals("All lines included in summary", 6, docOrganisationBill.GroupSummaryLines.Count);

			//pricelist Without Licence Units
			DocOrganisationBill docOrganisationBillWithoutLicenceUnits = DocOrganisationBill.New(OrganisationBillWithoutLicenceUnits, Factory, OrganisationBillWithoutLicenceUnits.OrganisationPK);
			IEnumerable<SummaryLine> summaryLinesWithoutLicenceUnits = docOrganisationBillWithoutLicenceUnits.GroupSummaryLinesWithoutLicenceUnits.Cast<SummaryLine>();
			IEnumerable<SummaryLine> summaryLinesWithLicenceUnits = docOrganisationBillWithoutLicenceUnits.GroupSummaryLines.Cast<SummaryLine>();
			AssertEquals("0 group summary without licence units, one for each system bill", 1, summaryLinesWithoutLicenceUnits.Select(x => x.Header).Distinct().Count());
			AssertEquals("1 group summary, one for each system bill", 0, summaryLinesWithLicenceUnits.Select(x => x.Header).Distinct().Count());
			AssertEquals("Contains group summary without licence units from OrganisationNew1", true, summaryLinesWithoutLicenceUnits.Any(x => x.Header.MainDescription == OrganisationWithoutLicenceUnits1.PK.ToString()));
			AssertEquals("NOT Contains group summary with licence units from OrganisationNew1", false, summaryLinesWithLicenceUnits.Any(x => x.Header.MainDescription == OrganisationWithoutLicenceUnits1.PK.ToString()));
			AssertEquals("lines included in summary without licence units", 2, docOrganisationBillWithoutLicenceUnits.GroupSummaryLinesWithoutLicenceUnits.Count);
			AssertEquals("NO lines included in summary with licence units", 0, docOrganisationBillWithoutLicenceUnits.GroupSummaryLines.Count);
		}

		public void TestPaymentSummary()
		{
			var fees = EDIDataRegistry.Instance.InvoicingProcessingFeeLookup.Value;
			fees.Add("ZZZ", (NoResString)"My ZZZ Discount");
			EDIDataRegistry.Instance.InvoicingProcessingFeeLookup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fees);

			Organisation1.LicCompany.SelfBilling.L4_ProcessingFee = "ZZZ";
			Organisation1.LicCompany.SelfBilling.L4_ProcessingFeePercent = 10;
			OrganisationBill.CalculateAll(0);
			AssertNotEquals(0m, OrganisationBill.ProcessingFeeAmount);
			AssertEquals("ZZZ", OrganisationBill.ProcessingFeeCode);
			AssertEquals("My ZZZ Discount", OrganisationBill.ProcessingFeeDescription);
			DocOrganisationBill docOrganisationBill = DocOrganisationBill.New(OrganisationBill, Factory, OrganisationBill.OrganisationPK);
			AssertEquals("Invoice currency", "Total (AUD)", docOrganisationBill.PaymentSummaryLines[0].Header.TotalDescription);
			AssertEquals("Total licence units MUST be blank on payment summary", "", docOrganisationBill.PaymentSummaryLines[0].Header.TotalLicenceUnitsDescription);
			AssertEquals("Total licence units MUST be blank on payment summary", "", docOrganisationBill.PaymentSummaryLines[0].Header.TotalLicenceUnitsDescription);

			var feeLine = docOrganisationBill.PaymentSummaryLines[docOrganisationBill.PaymentSummaryLines.Count - 1];
			AssertEquals("Processing fee description", "My ZZZ Discount", feeLine.MainDescription);
		}

		public void TestPrepaymentSummary()
		{
			OrganisationBill.CalculateAll(0);
			OrganisationBill.PrepayNext.CurrentInvoiceTotalAmount = 100;
			OrganisationBill.PrepayNext.CurrentPrepaymentBalance = 20;
			OrganisationBill.PrepayNext.PrepaymentBalanceRequired = 110;
			OrganisationBill.PrepayNext.FuturePrepaymentBalanceRequired = 120;
			var docOrganisationBill = DocOrganisationBill.New(OrganisationBill, Factory, OrganisationBill.OrganisationPK);
			AssertEquals(1, docOrganisationBill.PrepaymentSummaryLines.Count);
			var line = docOrganisationBill.PrepaymentSummaryLines[0];
			AssertEquals("20.00,110.00,,Revised Prepayment Balance - Effective 1st June 2018,200.00,120.00,100.00,AUD,,,,,,", line.UnsortedCode);

			OrganisationBill.PrepayNext.CurrentInvoiceTotalAmount = 120;
			OrganisationBill.PrepayNext.CurrentPrepaymentBalance = 50;
			OrganisationBill.PrepayNext.PrepaymentBalanceRequired = 0;
			OrganisationBill.PrepayNext.FuturePrepaymentBalanceRequired = 0;
			docOrganisationBill = DocOrganisationBill.New(OrganisationBill, Factory, OrganisationBill.OrganisationPK);
			AssertEquals("Hide the summary section if the predetermined balance is zero. Since that means they are not eligible for prepayment.", 0, docOrganisationBill.PrepaymentSummaryLines.Count);
		}

		public void TestIsEmpty()
		{
			DocOrganisationBill docOrganisationBill = DocOrganisationBill.New(OrganisationBill, Factory, OrganisationBill.OrganisationPK);
			AssertEquals(false, docOrganisationBill.IsEmpty);

			docOrganisationBill.GeneralSummaryLines.RemoveAndDeleteAll();
			AssertEquals(false, docOrganisationBill.IsEmpty);

			docOrganisationBill.DiscountSummaryLines.RemoveAndDeleteAll();
			AssertEquals(false, docOrganisationBill.IsEmpty);

			docOrganisationBill.GroupSummaryLines.RemoveAndDeleteAll();
			AssertEquals(false, docOrganisationBill.IsEmpty);

			docOrganisationBill.DepositSummaryLines.RemoveAndDeleteAll();
			AssertEquals(false, docOrganisationBill.IsEmpty);

			docOrganisationBill.PaymentSummaryLines.RemoveAndDeleteAll();
			AssertEquals(true, docOrganisationBill.IsEmpty);

			// Organisation's pricelist without licence units
			DocOrganisationBill docOrganisationBillWithoutLicenceUnits = DocOrganisationBill.New(OrganisationBillWithoutLicenceUnits, Factory, OrganisationBillWithoutLicenceUnits.OrganisationPK);
			docOrganisationBillWithoutLicenceUnits.DiscountSummaryLines.RemoveAndDeleteAll();
			docOrganisationBillWithoutLicenceUnits.DepositSummaryLines.RemoveAndDeleteAll();
			docOrganisationBillWithoutLicenceUnits.PaymentSummaryLines.RemoveAndDeleteAll();

			AssertEquals(false, docOrganisationBillWithoutLicenceUnits.IsEmpty);

			docOrganisationBillWithoutLicenceUnits.GeneralSummaryLinesWithoutLicenceUnits.RemoveAndDeleteAll();
			AssertEquals(false, docOrganisationBillWithoutLicenceUnits.IsEmpty);

			docOrganisationBillWithoutLicenceUnits.GroupSummaryLinesWithoutLicenceUnits.RemoveAndDeleteAll();
			AssertEquals(true, docOrganisationBillWithoutLicenceUnits.IsEmpty);
		}

		public void TestMixedSystemUsage()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "XYZ", "SYD", "XYZ");
			var organisation = lic.Company.Header;
			var user = new UsingParty(lic);
			ClientLicencePriceHeader pricelist = organisation.LicCompany.PriceHeaders.AddNew();
			pricelist.L6_RX_NKCurrency = "AUD";
			pricelist.L6_ValidFrom = new ZDateTime(2000, 1, 1);
			ClientLicencePriceItem onDemandPriceItem = BillingTestHelper.AddPriceItem(pricelist, BillingConstants.CoreModuleCode, BillingConstants.FeeType.Module, "", 10m, 1m);
			AssertEquals("Precondition", true, onDemandPriceItem.HasLicenceUnits);
			onDemandPriceItem.L7_FeeType = BillingConstants.FeeType.Included;
			ClientLicencePriceItem transactionalPriceItem = BillingTestHelper.AddPriceItem(pricelist, "EBA", BillingConstants.FeeType.Module, "", 10m);
			AssertEquals("Precondition", false, transactionalPriceItem.HasLicenceUnits);
			transactionalPriceItem.L7_FeeType = BillingConstants.FeeType.Transactional;

			// Odpl usage
			OrganisationBill organisationBill = new OrganisationBill(Factory, Env.CurrentBranch.PK, organisation.PK, "AUD", ZDateTime.Now);
			OdplSystemBill odplSystemBill = new OdplSystemBill(Factory);
			OdplUsage odplUsage = new OdplUsage(Factory, user, new ZDateTime(2011, 1, 1));
			OdplUsageTest.AddModuleUsage(odplUsage, "COR", 100, 1);
			odplSystemBill.PopulateFromSystemUsages(new SystemUsage[] { odplUsage });
			organisationBill.AddSystemBill(odplSystemBill);

			// Transactional usage
			TransactionalSystemBill transactionalSystemBill = new TransactionalSystemBill("", Factory);
			PriceItemUsage transactionalUsage = new PriceItemUsage(Factory, user, new ZDateTime(2011, 1, 1), "EBA", true);
			transactionalUsage.TransactionCount = 3;
			transactionalSystemBill.PopulateFromSystemUsages(new SystemUsage[] { transactionalUsage });
			organisationBill.AddSystemBill(transactionalSystemBill);

			DocOrganisationBill docOrganisationBill = DocOrganisationBill.New(organisationBill, Factory, organisationBill.OrganisationPK);
			AssertEquals(1, docOrganisationBill.GeneralSummaryLines.Count);
			AssertEquals(1, docOrganisationBill.GeneralSummaryLinesWithoutLicenceUnits.Count);

			AssertEquals(docOrganisationBill.GeneralSummaryLines[0].MainDescription, "COR Module");
			AssertEquals(ZString.Format("Total ({0}):", "AUD").Trim(), docOrganisationBill.GeneralSummaryLines[0].Header.TotalDescription.Trim());
			AssertEquals(ZString.Format("Total Licence Units:", "AUD").Trim(), docOrganisationBill.GeneralSummaryLines[0].Header.TotalLicenceUnitsDescription.Trim());

			AssertEquals(docOrganisationBill.GeneralSummaryLinesWithoutLicenceUnits[0].MainDescription.Trim(), "Usage");
			AssertEquals(ZString.Format("Total ({0})", "AUD").Trim(), docOrganisationBill.GeneralSummaryLinesWithoutLicenceUnits[0].Header.TotalDescription.Trim());

			AssertEquals(1, docOrganisationBill.GroupSummaryLines.Count);
			AssertEquals(1, docOrganisationBill.GroupSummaryLinesWithoutLicenceUnits.Count);

			AssertEquals("Total", docOrganisationBill.GroupSummaryLines[0].Header.TotalDescription.Trim());
			AssertEquals(ZString.Format("Total ({0})", "AUD").Trim(), docOrganisationBill.GroupSummaryLinesWithoutLicenceUnits[0].Header.TotalDescription.Trim());
		}

		#region Implementation

		EDIOrgHeader Organisation1;
		EDIOrgHeader Organisation2;
		EDIOrgHeader Organisation3;
		EDIOrgHeader OrganisationWithoutLicenceUnits1;

		SystemBillForTesting SystemBill1;
		SystemBillForTesting SystemBill2;
		SystemBillForTesting SystemBill3;
		SystemBillWithoutLicenceUnitsForTesting SystemBillWithoutLicenceUnits1;

		OrganisationBill OrganisationBill;
		OrganisationBill OrganisationBillWithoutLicenceUnits;

		protected override void SetUp()
		{
			base.SetUp();

			Organisation1 = BillingTestHelper.CreateOrganisation(Factory, "AAA");

			Organisation2 = BillingTestHelper.CreateOrganisation(Factory, "BBB");
			Organisation3 = BillingTestHelper.CreateOrganisation(Factory, "CCC");

			var periodStart = new ZDateTime(2010, 11, 01);
			SystemBill1 = new SystemBillForTesting(Factory, Organisation1, "AUD", false, 1m);
			SystemBill2 = new SystemBillForTesting(Factory, Organisation2, "NZD", true, 1m);
			SystemBill3 = new SystemBillForTesting(Factory, Organisation3, "USD", false, 1m);

			OrganisationBill = new OrganisationBill(Factory, Env.CurrentBranch.PK, SystemBill1.OrganisationPK, "AUD", ZDateTime.Now);
			OrganisationBill.AddSystemBill(SystemBill1);
			OrganisationBill.AddSystemBill(SystemBill2);
			OrganisationBill.AddSystemBill(SystemBill3);

			ClientLicencePriceHeader priceHeader1 = Organisation1.LicCompany.PriceHeaders.AddNew();
			priceHeader1.L6_ValidFrom = new ZDateTime(2000, 1, 1);
			ClientLicencePriceHeader priceHeader2 = Organisation2.LicCompany.PriceHeaders.AddNew();
			priceHeader2.L6_ValidFrom = new ZDateTime(2000, 1, 1);
			ClientLicencePriceHeader priceHeader3 = Organisation3.LicCompany.PriceHeaders.AddNew();
			priceHeader3.L6_ValidFrom = new ZDateTime(2000, 1, 1);

			// Setup new company with new pricelist
			OrganisationWithoutLicenceUnits1 = BillingTestHelper.CreateOrganisation(Factory, "DDD");
			SystemBillWithoutLicenceUnits1 = new SystemBillWithoutLicenceUnitsForTesting(Factory, OrganisationWithoutLicenceUnits1, "AUD");
			ClientLicencePriceHeader priceHeaderNew1 = OrganisationWithoutLicenceUnits1.LicCompany.PriceHeaders.AddNew();
			priceHeaderNew1.L6_ValidFrom = new ZDateTime(2000, 1, 1);
			OrganisationBillWithoutLicenceUnits = new OrganisationBill(Factory, Env.CurrentBranch.PK, SystemBillWithoutLicenceUnits1.OrganisationPK, "AUD", ZDateTime.Now);
			OrganisationBillWithoutLicenceUnits.AddSystemBill(SystemBillWithoutLicenceUnits1);

			Factory.Save();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			OrganisationBill organisationBill = new OrganisationBill(Factory, Env.CurrentBranch.PK, Organisation1.PK, "", ZDateTime.Now);
			return DocOrganisationBill.New(organisationBill, Factory, organisationBill.OrganisationPK);
		}

		class SystemBillForTesting : SystemBillWithoutLicenceUnitsForTesting
		{
			public SystemBillForTesting(BusinessObjectFactory factory, EDIOrgHeader organisation, ZString currencyCode, bool negativeAmount, decimal licenceUnitsAmount)
				: base(factory, organisation)
			{
				var dummyUsage = new DummyUsageWithLicenceUnits(Factory, new UsingParty(organisation), PeriodStart, negativeAmount ? -100m : 100m, licenceUnitsAmount);
				dummyUsage.CurrencyCode_Exposed = currencyCode;
				SystemUsages.Add(dummyUsage);
				CalculateGroupAmounts();
			}
		}

		class SystemBillWithoutLicenceUnitsForTesting : SystemBill
		{
			public SystemBillWithoutLicenceUnitsForTesting(BusinessObjectFactory factory, EDIOrgHeader organisation)
				: base(factory)
			{
				TextNotifications = new ZStringBuilder();
				OrganisationPK = organisation.PK;

				PeriodStart = new ZDateTime(2010, 11, 01);
			}

			public SystemBillWithoutLicenceUnitsForTesting(BusinessObjectFactory factory, EDIOrgHeader organisation, ZString currencyCode, bool negativeAmount = false)
				: this(factory, organisation)
			{
				DummyUsage dummyUsage = new DummyUsage(Factory, new UsingParty(organisation), PeriodStart);
				dummyUsage.Amount_Exposed = negativeAmount ? -100m : 100m;
				dummyUsage.CurrencyCode_Exposed = currencyCode;
				SystemUsages.Add(dummyUsage);
				CalculateGroupAmounts();
			}

			public override SummarySection[] GetGeneralSummarySections(ZGuid organisationPK)
			{
				TextNotifications.Append("GetGeneralSummarySections" + organisationPK.ToString());

				SummarySection result = new SummarySection(Factory);
				SummaryLine summaryLine = result.Lines.AddNew();
				summaryLine.MainDescription = OrganisationPK.ToString();

				return new SummarySection[] { result };
			}

			public override SummarySection[] GetGroupSummarySections()
			{
				SummarySection result = new SummarySection(Factory);
				result.Header.MainDescription = OrganisationPK.ToString();
				result.Lines.AddNew().MainDescription = "Line one";
				result.Lines.AddNew().MainDescription = "Line two";

				return new SummarySection[] { result };
			}

			public override SummarySection[] GetDiscountSummarySections()
			{
				TextNotifications.Append("GetDiscountSummarySections");

				SummarySection result = new SummarySection(Factory);
				result.Header.MainDescription = "Head main description";

				SummaryLine summaryLine1 = result.Lines.AddNew();
				summaryLine1.MainDescription = Organisation.OH_Code + " discount1";

				SummaryLine summaryLine2 = result.Lines.AddNew();
				summaryLine2.MainDescription = Organisation.OH_Code + " discount2";

				return new SummarySection[] { result };
			}

			public override SummarySection[] GetSurchargeSummarySections()
			{
				TextNotifications.Append("GetSurchargeSummarySections");

				SummarySection result = new SummarySection(Factory);
				result.Header.MainDescription = "Head main description";

				SummaryLine summaryLine = result.Lines.AddNew();
				summaryLine.MainDescription = Organisation.OH_Code + " surcharge";

				return new SummarySection[] { result };
			}

			#region Implementation

			ZStringBuilder TextNotifications;

			public bool MethodWasCalled(string methodName)
			{
				return TextNotifications.ToStringWithNewLineBetweenAppends().Contains(methodName);
			}

			public void ClearTextNotifications()
			{
				TextNotifications = new ZStringBuilder();
			}

			#endregion
		}

		#endregion
	}
}
