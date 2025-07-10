using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(ClientLicenceBilling))]
	internal class ClientLicenceBillingTest : EnterpriseBusinessObjectTestCase
	{
		#region Business Object Overrides

		public void TestSetDefaultValues()
		{
			ClientLicenceBilling billing = Factory.New<ClientLicenceBilling>();
			AssertEquals(billing.L4_ProcessingFee, "NON");
		}

		public void TestDelete()
		{
			ClientLicenceBilling billing = Factory.New<ClientLicenceBilling>();
			ClientLicenceBillingDiscount discount = billing.BillingDiscounts.AddNew();

			AssertEquals("Precondition", false, discount.IsDeleted);

			billing.Delete();
			AssertEquals("Discounts deleted", true, discount.IsDeleted);
		}

		#endregion

		public void TestCompany()
		{
			LicenceHeader lic = BillingTestHelper.CreateLicence(Factory, "ABC");
			ClientLicenceBilling billing = lic.Company.SelfBilling;
			AssertEquals(lic.Company.PK, billing.Company.PK);
		}

		public void TestBillingDiscountsCollectionReadOnly()
		{
			ClientLicenceBilling billing = Factory.New<ClientLicenceBilling>();
			EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = true;
			AssertEquals(false, billing.BillingDiscounts.ReadOnly);

			billing = Factory.New<ClientLicenceBilling>();
			EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = false;
			AssertEquals(true, billing.BillingDiscounts.ReadOnly);
		}

		public void TestPropertiesReadOnly()
		{
			LicenceHeader lic = BillingTestHelper.CreateLicence(Factory, "ABC");
			LicenceHeader lic2 = BillingTestHelper.CreateLicence(Factory, "XYZ");
			ClientLicenceBilling billing = lic.Company.SelfBilling;

			EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = true;
			foreach (ZPropertyInfo propertyInfo in billing.ZPropertyInfoHash)
			{
				bool expectReadonly = propertyInfo.Name == ClientLicenceBilling.Schema.PartnerEmail;
				AssertEquals(propertyInfo.Name, expectReadonly, propertyInfo.ReadOnly);
			}

			billing.L4_IsPartner = true;
			foreach (ZPropertyInfo propertyInfo in billing.ZPropertyInfoHash)
			{
				bool expectReadonly =
					propertyInfo.Name == ClientLicenceBilling.Schema.L4_InvoiceComment ||
					propertyInfo.Name == ClientLicenceBilling.Schema.L4_ProcessingFee;
				AssertEquals(propertyInfo.Name, expectReadonly, propertyInfo.ReadOnly);
			}
			billing.L4_IsPartner = false;

			EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = false;
			foreach (ZPropertyInfo propertyInfo in billing.ZPropertyInfoHash)
			{
				AssertEquals(propertyInfo.Name, true, propertyInfo.ReadOnly);
			}

			EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = true;
			AssertEquals("L4_InvoiceCommentInfo", false, billing.L4_InvoiceCommentInfo.ReadOnly);
			AssertEquals("L4_ProcessingFeeInfo", false, billing.L4_ProcessingFeeInfo.ReadOnly);
			AssertEquals("L4_IsPartnerInfo", false, billing.L4_IsPartnerInfo.ReadOnly);

			EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = false;
			foreach (ZPropertyInfo propertyInfo in billing.ZPropertyInfoHash)
			{
				AssertEquals(propertyInfo.Name, true, propertyInfo.ReadOnly);
			}
		}

		public void TestLastOdplMonthlyInvoice()
		{
			AssertEquals("Same usage code for all companies", Enterprise.Integration.RegistryStorageFlags.System, EDIDataRegistry.Instance.OdplUsageChargeCode.Storage);

			LicenceHeader lic = BillingTestHelper.CreateLicence(Factory, "ABC");
			LicenceHeader anotherLic = BillingTestHelper.CreateLicence(Factory, "XYZ");
			ClientLicencePriceHeader prices = BillingTestHelper.CreatePriceList(lic);
			ClientLicenceBilling billing = lic.Company.SelfBilling;
			GlbBranch anotherBranch = Factory.NewWithValidTestData<GlbBranch>();
			anotherBranch.Company.GC_RX_NKLocalCurrency = "USD";
			BillingTestHelper.SetInvoicing(lic, Env.CurrentBranch.PK);
			AccTaxRate rate2 = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", Enterprise.MasterFiles.Business.AccTaxRate.Types.Rated, 10);
			AccChargeCode usageChargeCode2 = BillingTestHelper.CreateChargeCode(Factory, rate2, EDIDataRegistry.Instance.OdplUsageChargeCode.Value);
			Factory.Save();

			using (anotherBranch.SetAsTemporaryContext())
			{
				AccTaxRate rate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", Enterprise.MasterFiles.Business.AccTaxRate.Types.Rated, 10);
				AccChargeCode usageChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, anotherBranch, rate, EDIDataRegistry.Instance.OdplUsageChargeCode.Value);
				AccChargeCode anotherChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, anotherBranch, rate, "NOTUSAGE");
			}

			Factory.Save();

			ARInvoice lastInvoice;

			using (anotherBranch.SetAsTemporaryContext())
			{
				lastInvoice = BillingTestHelper.CreateInvoice(lic.Factory, lic, EDIDataRegistry.Instance.OdplUsageChargeCode.Value, 1200m, 1, "", 0);
				lastInvoice.AH_PostDate = new ZDateTime(2010, 4, 1);

				var olderInvoice = BillingTestHelper.CreateInvoice(lic.Factory, lic, EDIDataRegistry.Instance.OdplUsageChargeCode.Value, 900m, 1, null, 0);
				olderInvoice.AH_PostDate = new ZDateTime(2010, 3, 1);

				var recentNonUsageInvoice = BillingTestHelper.CreateInvoice(lic.Factory, lic, "NOTUSAGE", 900m, 1, null, 0);
				recentNonUsageInvoice.AH_PostDate = new ZDateTime(2010, 6, 1);

				var recentUsageInvoiceForAnotherOrg = BillingTestHelper.CreateInvoice(anotherLic.Factory, anotherLic, "NOTUSAGE", 900m, 1, null, 0);
				recentUsageInvoiceForAnotherOrg.AH_PostDate = new ZDateTime(2010, 6, 1);

				var recentNonInvoice = Factory.NewWithValidTestData<ARCreditNote>();
				recentNonInvoice.AH_OH = lic.Company.LC_OH;
				ARCreditNoteLine line = (ARCreditNoteLine)recentNonInvoice.Lines.AddNew();
				line.AL_AC = BillingInvoicingHelper.GetChargeCodePK(anotherBranch, EDIDataRegistry.Instance.OdplUsageChargeCode.Value);
				line.AL_OSExTaxAmount = 1000m;
				recentNonInvoice.AH_PostDate = new ZDateTime(2010, 6, 1);
			}

			Factory.Save();

			AssertEquals(lastInvoice.PK, billing.LastOdplMonthlyInvoice.PK);

			ARInvoice newerInvoice = BillingTestHelper.CreateInvoice(lic.Factory, lic, EDIDataRegistry.Instance.OdplUsageChargeCode.Value, 1200m, 1, "", 0);
			newerInvoice.AH_PostDate = lastInvoice.AH_PostDate.AddDays(1);
			Factory.Save();
			AssertEquals(newerInvoice.PK, billing.LastOdplMonthlyInvoice.PK);
		}

		[TestDate(2010, 1, 1)]
		public void TestLogChanges()
		{
			AccTaxRate taxId = Factory.NewWithValidTestData<AccTaxRate>();
			taxId.AT_Code = "TTT";

			EDIOrgHeader org = Factory.New<EDIOrgHeader>();
			org.OH_Code = "DDDSYD";
			org.CreateAndLoadLicenceForOrg();
			var billing = org.LicCompany.SelfBilling;
			billing.L4_ProcessingFee = "NON";

			Factory.Save();

			ZQuery query = new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "InvoicingSetting");
			query.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + OrderByClause.Descending;
			StmALog[] logs = org.Logs.Find(query);
			AssertEquals("No log for new added invoicing setting", 0, logs.Length);

			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "DLX";
			EDIOrgHeader payingEntity = Factory.NewWithValidTestData<EDIOrgHeader>();
			payingEntity.OH_Code = "DDDNYC";

			billing.L4_ProcessingFee = "DDE";

			Factory.Save();

			logs = org.Logs.Find(query);
			AssertEquals("Should be one log for invoicing setting changes", 1, logs.Length);

			string expected = "InvoicingSetting | Pr.Fee:NON=>DDE";
			AssertEquals(expected, logs[0].SL_Reference);
			AssertEquals(Events.EditedARecordCode, logs[0].SL_SE_NKEvent);
		}

		public void TestCalculateProcessingFeeAmount()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			ClientLicenceBilling billing = organisation.LicCompany.SelfBilling;
			billing.L4_ProcessingFeePercent = 0.0m;
			AssertEquals("Default", 0m, billing.CalculateProcessingFeeAmount(100m));

			billing.L4_ProcessingFee = "DDE";
			billing.L4_ProcessingFeePercent = 3.0m;
			AssertEquals("Original DirectDebit", -3.0m, billing.CalculateProcessingFeeAmount(100m));
			AssertEquals(-3m, billing.SignedProcessingFeePercent);

			billing.L4_ProcessingFee = "MPF";
			billing.L4_ProcessingFeePercent = 5.0m;
			AssertEquals("Original ManualProcessingFee", 5.0m, billing.CalculateProcessingFeeAmount(100m));
			AssertEquals(5m, billing.SignedProcessingFeePercent);
			AssertEquals("Rounding to 2 decimals", 5.02m, billing.CalculateProcessingFeeAmount(100.3333m));

			var newRegistry = new CodeDescriptionBoolCollection();
			newRegistry.AddRange(EDIDataRegistry.Instance.InvoicingProcessingFeeLookup.Value);
			newRegistry.Add("AAA", (NoResString)"Discount", true);
			newRegistry.Add("BBB", (NoResString)"Fee", false);
			EDIDataRegistry.Instance.InvoicingProcessingFeeLookup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newRegistry);

			billing.L4_ProcessingFee = "AAA"; //New Value
			billing.L4_ProcessingFeePercent = 10.0m; //Discount
			AssertEquals("New Fee with new Percent", -10.0m, billing.CalculateProcessingFeeAmount(100m));
			AssertEquals(-10m, billing.SignedProcessingFeePercent);

			billing.L4_ProcessingFee = "BBB"; //New Value
			billing.L4_ProcessingFeePercent = 10.0m; //Fee
			AssertEquals("New Fee with new Percent", 10.0m, billing.CalculateProcessingFeeAmount(100m));
			AssertEquals(10m, billing.SignedProcessingFeePercent);
		}

		public void TestProcessingFeePercentLabel()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			ClientLicenceBilling billing = organisation.LicCompany.SelfBilling;

			billing.L4_ProcessingFeePercent = 5.0m;
			AssertEquals("5% Fee", "This is a 5.0% Fee", billing.L4_ProcessingFeePercentLabel);
			billing.L4_ProcessingFee = "DDE";
			billing.L4_ProcessingFeePercent = 5.0m;
			AssertEquals("5% Discount", "This is a 5.0% Discount", billing.L4_ProcessingFeePercentLabel);

			billing.L4_ProcessingFeePercent = 0.0m;
			AssertEquals("No Fee or Discount", "No Fee or Discount", billing.L4_ProcessingFeePercentLabel);
		}

		public void TestSynchronize()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			LicenceCompany company = organisation.LicCompany;
			Factory.Save();

			company.SelfBilling.L4_Comment = "6m";

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			LicenceCompany companyReloaded = factory2.Load<LicenceCompany>(company.PK);

			companyReloaded.SelfBilling.L4_Comment = "1m";
			AssertNotEquals("separate children before save", company.SelfBilling.PK, companyReloaded.SelfBilling.PK);

			factory2.Save();
			Factory.Save();

			AssertEquals("child synchronized", company.SelfBilling.PK, companyReloaded.SelfBilling.PK);
			AssertEquals("last save wins", "6m", companyReloaded.SelfBilling.L4_Comment);
			AssertEquals("last save wins", "6m", company.SelfBilling.L4_Comment);

			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			ClientLicenceBilling[] allChildren = factory3.Load<ClientLicenceBilling>(new ZQuery(ClientLicenceBillingSchema.L4_LC, company.PK));
			AssertEquals("only 1 child in DB", 1, allChildren.Length);
		}

		public void TestMiscProperties()
		{
			var organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var company = organisation.LicCompany;
			var billing = company.SelfBilling;
			Factory.Save();

			AssertEquals(0m, billing.L4_ProcessingFeePercent);
			AssertEquals("NON", billing.L4_ProcessingFee);

			AssertEquals(false, billing.IsProcessingFeeValid);
			AssertEquals(false, billing.IsProcessingFeeADiscount);
			AssertEquals("None", billing.ProcessingFeeDescription);

			billing.L4_ProcessingFee = "DDE";
			billing.L4_ProcessingFeePercent = 5.0m;
			AssertEquals(true, billing.IsProcessingFeeValid);
			AssertEquals(true, billing.IsProcessingFeeADiscount);
			AssertEquals("Direct Debit Discount", billing.ProcessingFeeDescription.ToString());
		}

		public void TestL4_InvoiceComment_Translatable()
		{
			var obj = Factory.New<ClientLicenceBilling>();
			obj.L4_InvoiceComment = "A";
			var resKey = obj.L4_InvoiceCommentInfo.CustomizableDataResourceStrings.GetMultilingualString(obj, "A").ResourceKey;
			AssertEquals("A", obj.L4_InvoiceCommentMultilingual);
			using (Res.TemporarilySwitchLanguage("CHS"))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "B"));
				AssertEquals("B", obj.L4_InvoiceCommentMultilingual);
			}
		}
	}
}
