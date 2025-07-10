using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	internal class SystemUsageTest : TestCaseWithFactory
	{
		public void TestDocumentSupporter()
		{
			AssertEquals(typeof(SystemUsageDocumentSupporter), DummyUsage.DocumentSupporter.GetType());
		}

		public void TestMainProperties()
		{
			AssertEquals(Organisation.PK, DummyUsage.OrganisationPK);
			AssertEquals(new ZDateTime(2010, 10, 01), DummyUsage.PeriodStart);
			AssertEquals("DUM", DummyUsage.SystemCode);
		}

		public void TestEmptyOrganisationPK()
		{
			DummyUsage = new DummyUsage(Factory, User, new ZDateTime(2010, 10, 01));
			AssertEquals(Organisation.PK, DummyUsage.OrganisationPK);

			DummyUsage = new DummyUsage(Factory, new UsingParty(), new ZDateTime(2010, 10, 01));
			AssertEquals(SystemUsage.MiscOrganisationPK, DummyUsage.OrganisationPK);
		}

		public void TestChargeableUsagePKs()
		{
			AssertNotNull(DummyUsage.ChargeableUsagePKs);
		}

		public void TestOrganisation()
		{
			AssertEquals(Organisation, DummyUsage.Organisation);
		}

		public void TestInvoicedOrganisation()
		{
			AssertEquals(InvoicedOrganisation.PK, DummyUsage.InvoicedOrganisation.PK);
			AssertEquals(InvoicedOrganisation, DummyUsage.InvoicedOrganisation);

			EDIOrgHeader anotherOrganisation = BillingTestHelper.CreateOrganisation(Factory, "XXX");
			anotherOrganisation.LicCompany.InvoiceDeliveries.AddNew().L9_OH_InvoiceTo = ZGuid.Invalid;
			DummyUsage = new DummyUsage(Factory, new UsingParty(anotherOrganisation), new ZDateTime(2010, 10, 01));

			AssertEquals(anotherOrganisation.PK, DummyUsage.InvoicedOrganisation.PK);
			AssertEquals(anotherOrganisation, DummyUsage.InvoicedOrganisation);
		}

		public void TestLicCompany()
		{
			AssertEquals(Organisation.LicCompany, DummyUsage.LicCompany);

			DummyUsage anotherSystemUsage = new DummyUsage(Factory, new UsingParty(Factory.New<EDIOrgHeader>()), new ZDateTime(2010, 10, 01));
			AssertNull(anotherSystemUsage.LicCompany);
		}

		public void TestInvoicedLicCompany()
		{
			AssertEquals(InvoicedOrganisation.LicCompany, DummyUsage.InvoicedLicCompany);

			EDIOrgHeader anotherOrganisation = BillingTestHelper.CreateOrganisation(Factory, "XXX");
			anotherOrganisation.LicCompany.InvoiceDeliveries.AddNew().L9_OH_InvoiceTo = Factory.New<EDIOrgHeader>().PK;
			DummyUsage = new DummyUsage(Factory, new UsingParty(anotherOrganisation), new ZDateTime(2010, 10, 01));

			AssertNull(DummyUsage.InvoicedLicCompany);
		}

		public void TestBilling()
		{
			AssertEquals(Organisation.LicCompany.SelfBilling, DummyUsage.Billing);

			DummyUsage anotherSystemUsage = new DummyUsage(Factory, new UsingParty(Factory.New<EDIOrgHeader>()), new ZDateTime(2010, 10, 01));
			AssertNull(anotherSystemUsage.InvoiceDelivery);
		}

		public void TestPriceHeader()
		{
			ClientLicencePriceHeader oldPriceHeader = Organisation.LicCompany.PriceHeaders.AddNew();
			oldPriceHeader.L6_RX_NKCurrency = "AUD";
			oldPriceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);

			ClientLicencePriceHeader recentPriceHeader = Organisation.LicCompany.PriceHeaders.AddNew();
			recentPriceHeader.L6_RX_NKCurrency = "AUD";
			recentPriceHeader.L6_ValidFrom = new ZDateTime(2010, 10, 1);

			ClientLicencePriceHeader futurePriceHeader = Organisation.LicCompany.PriceHeaders.AddNew();
			futurePriceHeader.L6_RX_NKCurrency = "AUD";
			futurePriceHeader.L6_ValidFrom = new ZDateTime(2011, 1, 1);

			var oldParentPrices = InvoicedOrganisation.LicCompany.PriceHeaders.AddNew();
			oldParentPrices.L6_RX_NKCurrency = "AUD";
			oldParentPrices.L6_ValidFrom = new ZDateTime(2010, 1, 1);

			var recentParentPrices = InvoicedOrganisation.LicCompany.PriceHeaders.AddNew();
			recentParentPrices.L6_RX_NKCurrency = "AUD";
			recentParentPrices.L6_ValidFrom = new ZDateTime(2010, 10, 1);

			var futureParentPrices = InvoicedOrganisation.LicCompany.PriceHeaders.AddNew();
			futureParentPrices.L6_RX_NKCurrency = "AUD";
			futureParentPrices.L6_ValidFrom = new ZDateTime(2011, 1, 1);

			Factory.Save();

			AssertEquals(recentPriceHeader, DummyUsage.PriceHeader);

			DummyUsage = new DummyUsage(Factory, User, new ZDateTime(2010, 10, 01));
			Organisation.LicCompany.InvoiceDeliveries[0].L9_UseParentPrices = true;

			AssertEquals(recentParentPrices, DummyUsage.PriceHeader);
		}

		public void TestIsInvoiced()
		{
			AssertEquals(false, DummyUsage.IsInvoiced);

			DummyUsage = new DummyUsage(Factory, new UsingParty(InvoicedOrganisation), new ZDateTime(2010, 10, 01));
			AssertEquals(true, DummyUsage.IsInvoiced);
		}

		public void TestPeriodStartAsText()
		{
			DummyUsage = new DummyUsage(Factory, new UsingParty(InvoicedOrganisation), new ZDateTime(2010, 10, 01));
			AssertEquals("Oct 2010", DummyUsage.PeriodStartAsText);

			DummyUsage = new DummyUsage(Factory, new UsingParty(InvoicedOrganisation), new ZDateTime(2012, 12, 01));
			AssertEquals("Dec 2012", DummyUsage.PeriodStartAsText);
		}

		public void TestMiscOrganisationPK()
		{
			AssertEquals(OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation, SystemUsage.MiscOrganisationPK);
		}

		public void TestAmount()
		{
			DummyUsage dummyUsage = new DummyUsage(Factory, new UsingParty(InvoicedOrganisation), new ZDateTime(2010, 11, 01));
			AssertEquals("Default", 0m, dummyUsage.Amount);

			dummyUsage.Amount_Exposed = 10.033;
			AssertEquals("Amount rounded", 10.03m, dummyUsage.Amount);

			dummyUsage.Amount_Exposed = 10.3333;
			AssertEquals("Amount rounded", 10.33m, dummyUsage.Amount);

			dummyUsage.Amount_Exposed = 10.334;
			AssertEquals("Amount rounded", 10.33m, dummyUsage.Amount);

			dummyUsage.Amount_Exposed = 10.335;
			AssertEquals("Amount rounded", 10.34m, dummyUsage.Amount);

			dummyUsage.Amount_Exposed = 10.7788;
			AssertEquals("Amount rounded", 10.78m, dummyUsage.Amount);
		}

		public void TestAmountExemptProcessingFee()
		{
			DummyUsage dummyUsage = new DummyUsage(Factory, new UsingParty(InvoicedOrganisation), new ZDateTime(2010, 11, 01));
			AssertEquals("Default", 0m, dummyUsage.AmountExemptProcessingFee);

			dummyUsage.AmountExemptProcessingFee_Exposed = 10.033;
			AssertEquals("rounded", 10.03m, dummyUsage.AmountExemptProcessingFee);
		}

		public void TestCurrencyCode()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "XXX");

			PriceItemUsage usage = new PriceItemUsage(Factory, new UsingParty(organisation), EdiDateTest.MonthToday, "DUM", true);
			AssertEquals("No price header, no currency", "", usage.CurrencyCode);

			ClientLicencePriceHeader priceHeader = organisation.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RX_NKCurrency = "USD";
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			AssertEquals("USD", usage.CurrencyCode);

			priceHeader.L6_RX_NKCurrency = "NZD";
			AssertEquals("NZD", usage.CurrencyCode);
		}

		public void TestHasLicenceUnits()
		{
			var org = BillingTestHelper.CreateOrganisation(Factory, "AAA");

			var usage1 = new DummyUsage(Factory, new UsingParty(org), new ZDateTime(2015, 1, 1), 2m, "DU1");
			var usage2 = new DummyUsage(Factory, new UsingParty(org), new ZDateTime(2015, 1, 1), 7m, "DU2");

			AssertEquals(false, usage1.HasLicenceUnits);
			AssertEquals(false, usage2.HasLicenceUnits);
		}

		public void TestLicenceNineCode()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "ENT", "SYD", "PRD");
			var usage = new DummyUsage(Factory, new UsingParty(lic), new ZDateTime(2015, 2, 1));
			AssertEquals("ENT-SYD-PRD", usage.LicenceNineCode);
		}

		public void TestClientCompanyDescription()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "ENT", "SYD", "PRD");
			var usage = new DummyUsage(Factory, new UsingParty(lic, lic.Database.ClientCompanies[0]), new ZDateTime(2015, 2, 1));
			AssertEquals("SYD Co (ENT-SYD-PRD)", usage.ClientCompanyDescription);
		}

		#region Implementation

		DummyUsage DummyUsage;
		IUsingParty User;
		EDIOrgHeader Organisation;
		EDIOrgHeader InvoicedOrganisation;

		protected override void SetUp()
		{
			base.SetUp();

			InvoicedOrganisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			Organisation = BillingTestHelper.CreateDependentOrganisation(InvoicedOrganisation, "BBB");
			User = new UsingParty(Organisation, "BBB");
			DummyUsage = new DummyUsage(Factory, User, new ZDateTime(2010, 10, 01));
		}

		#endregion
	}

	#region DummyUsage class

	internal class DummyUsageWithLicenceUnits : DummyUsage
	{
		public DummyUsageWithLicenceUnits(BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart, ZDecimal amount, string systemCode, decimal licenceUnitsAmount)
			: base(factory, user, periodStart, amount, systemCode)
		{
			LicenceUnitsAmount = licenceUnitsAmount;
		}

		public DummyUsageWithLicenceUnits(BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart, ZDecimal amount, decimal licenceUnitsAmount)
			: this(factory, user, periodStart, amount, "DUM", licenceUnitsAmount)
		{
		}

		public override bool HasLicenceUnits
		{
			get { return true; }
		}

		public override ZDecimal LicenceUnitsAmount
		{
			get;
			protected set;
		}
	}

	internal class DummyUsage : SystemUsage
	{
		public DummyUsage(BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart, ZDecimal amount, string systemCode)
			: base(factory, user, periodStart)
		{
			Amount_Exposed = amount;
			CurrencyCode_Exposed = "AUD";
			this.systemCode = systemCode;
		}

		public DummyUsage(BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart, ZDecimal amount)
			: this(factory, user, periodStart, amount, "DUM")
		{
		}

		public DummyUsage(BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
			: this(factory, user, periodStart, 0m)
		{
		}

		public override ZString SystemCode
		{
			get { return systemCode; }
		}
		readonly string systemCode;

		public override void CalculateAmount()
		{
			TextNotifications.Append("CalculateAmount");
		}

		protected override ZDecimal AmountCore
		{
			get { return Amount_Exposed; }
		}
		public ZDecimal Amount_Exposed;

		protected override ZDecimal AmountExemptProcessingFeeCore
		{
			get { return AmountExemptProcessingFee_Exposed; }
		}
		public ZDecimal AmountExemptProcessingFee_Exposed;

		public override ZInt UnitCount
		{
			get { return UnitCount_Exposed; }
		}
		public ZInt UnitCount_Exposed;

		public override ZDecimal UnitPrice
		{
			get { return UnitPrice_Exposed; }
		}
		public ZDecimal UnitPrice_Exposed;

		public override ZString CurrencyCode
		{
			get { return CurrencyCode_Exposed; }
		}
		public ZString CurrencyCode_Exposed;

		public override SummarySection[] GetGeneralSummarySections()
		{
			SummarySection result = new SummarySection(Factory);
			SummaryLine summaryLine = result.Lines.AddNew();
			summaryLine.MainDescription = "Dummy";
			summaryLine.UnitPrice = UnitPrice.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
			summaryLine.UnitCount = UnitCount.ToString();
			summaryLine.Amount = Amount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);

			SummaryLine summaryHeader = result.Header;
			summaryHeader.MainDescription = "Dummy";
			summaryHeader.UnitPrice = "Price";
			summaryHeader.UnitCount = "Units";
			summaryHeader.Amount = "Total";
			summaryHeader.TotalAmount = Amount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);

			return new SummarySection[] { result };
		}

		#region Implementation

		public readonly ZStringBuilder TextNotifications = new ZStringBuilder();

		public bool MethodWasCalled(string methodName)
		{
			return TextNotifications.ToStringWithNewLineBetweenAppends().Contains(methodName);
		}

		#endregion
	}

	#endregion
}
