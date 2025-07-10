using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.CommissionManagement.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.CommissionManagement.Business;
using Enterprise.CommissionManagement.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	class BillingCommissionCreatorTest : CommissionCreatorTestCase
	{
		public void TestCreateCommissions()
		{
			var rate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", Enterprise.MasterFiles.Business.AccTaxRate.Types.Rated, 10);
			var eHubChargeCode = BillingTestHelper.CreateChargeCode(Factory, rate, "EHUB");

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RX_NKLocalCurrency = "AUD";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var agreement = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreementForAllItems(Factory, org);
			var recipient = OrgCommissionAgreementTestHelper.AddPercentageRecipient(agreement, "ADL", 10);

			Factory.Save();

			var licenceDatabase = Factory.New<LicenceDatabase>();
			var clientCompany1 = Factory.New<ClientCompany>();
			clientCompany1.LCC_LD = licenceDatabase.PK;
			var clientCompany2 = Factory.New<ClientCompany>();
			clientCompany2.LCC_LD = licenceDatabase.PK;

			var invoice = Factory.New<ARInvoice>();
			invoice.AH_GC = company.PK;
			invoice.AH_OH = org.PK;
			invoice.AH_PostDate = new ZDateTime(2010, 10, 1);
			invoice.AH_RX_NKTransactionCurrency = "USD";
			invoice.AH_ExchangeRate = 2.0m;
			var priceItemA = Factory.New<ClientLicencePriceItem>();
			priceItemA.L7_Code = "AAA";
			priceItemA.L7_ChargeCode = "STLMTHUSE";
			priceItemA.L7_DiscountChargeCode = "DISCSTL";

			var priceItemB = Factory.New<ClientLicencePriceItem>();
			priceItemB.L7_Code = "BBB";
			priceItemB.L7_ChargeCode = "WISECLOUD";
			priceItemB.L7_DiscountChargeCode = "DISCWSC";

			var nonChargedPriceItem = Factory.New<ClientLicencePriceItem>();
			nonChargedPriceItem.L7_Code = "";
			nonChargedPriceItem.L7_ChargeCode = "STLMTHUSE";
			nonChargedPriceItem.L7_DiscountChargeCode = "DISCSTL";

			var stlmthuseChargeCode = GetOrCreateChargeCode("STLMTHUSE");
			var discstlChargeCode = GetOrCreateChargeCode("DISCSTL");
			var wisecloudChargeCode = GetOrCreateChargeCode("WISECLOUD");
			var discwscChargeCode = GetOrCreateChargeCode("DISCWSC");
			{
				var usage1Aa = Factory.New<EdiBilledUsage>();
				usage1Aa.BU9_AH_Invoice = invoice.PK;
				usage1Aa.BU9_LD = licenceDatabase.PK;
				usage1Aa.BU9_LCC = clientCompany1.PK;
				usage1Aa.BU9_L7 = priceItemA.PK;
				usage1Aa.BU9_TransactionAmountPostDiscount = 55;
				usage1Aa.BU9_TransactionAmountPreDiscount = 75;
				usage1Aa.BU9_TransactionProcessingAmount = 5;
				usage1Aa.BU9_LocalAmountPostDiscount = 110;
				usage1Aa.BU9_LocalAmountPreDiscount = 150;
				usage1Aa.BU9_LocalProcessingAmount = 10;
				usage1Aa.BU9_UsageCode = "STL";
				usage1Aa.BU9_UsageSubCode = "SHP";
				usage1Aa.BU9_PriceCode = priceItemA.L7_Code;
				usage1Aa.BU9_AC_AmountChargeCode = stlmthuseChargeCode.PK;
				usage1Aa.BU9_AC_DiscountChargeCode = discstlChargeCode.PK;
			}
			{
				var usage1Ab = Factory.New<EdiBilledUsage>();
				usage1Ab.BU9_AH_Invoice = invoice.PK;
				usage1Ab.BU9_LD = licenceDatabase.PK;
				usage1Ab.BU9_LCC = clientCompany1.PK;
				usage1Ab.BU9_L7 = priceItemA.PK;
				usage1Ab.BU9_TransactionAmountPostDiscount = 105;
				usage1Ab.BU9_TransactionAmountPreDiscount = 125;
				usage1Ab.BU9_TransactionProcessingAmount = 5;
				usage1Ab.BU9_LocalAmountPostDiscount = 210;
				usage1Ab.BU9_LocalAmountPreDiscount = 250;
				usage1Ab.BU9_LocalProcessingAmount = 10;
				usage1Ab.BU9_UsageCode = "STL";
				usage1Ab.BU9_UsageSubCode = "SHP";
				usage1Ab.BU9_PriceCode = priceItemA.L7_Code;
				usage1Ab.BU9_AC_AmountChargeCode = stlmthuseChargeCode.PK;
				usage1Ab.BU9_AC_DiscountChargeCode = discstlChargeCode.PK;
			}
			{
				var usage1B = Factory.New<EdiBilledUsage>();
				usage1B.BU9_AH_Invoice = invoice.PK;
				usage1B.BU9_LD = licenceDatabase.PK;
				usage1B.BU9_LCC = clientCompany1.PK;
				usage1B.BU9_L7 = priceItemB.PK;
				usage1B.BU9_TransactionAmountPostDiscount = 105;
				usage1B.BU9_TransactionAmountPreDiscount = 125;
				usage1B.BU9_TransactionProcessingAmount = 5;
				usage1B.BU9_LocalAmountPostDiscount = 210;
				usage1B.BU9_LocalAmountPreDiscount = 250;
				usage1B.BU9_LocalProcessingAmount = 10;
				usage1B.BU9_UsageCode = "STL";
				usage1B.BU9_UsageSubCode = "SHP";
				usage1B.BU9_PriceCode = priceItemB.L7_Code;
				usage1B.BU9_AC_AmountChargeCode = wisecloudChargeCode.PK;
				usage1B.BU9_AC_DiscountChargeCode = discwscChargeCode.PK;
			}
			{
				var usage2A = Factory.New<EdiBilledUsage>();
				usage2A.BU9_AH_Invoice = invoice.PK;
				usage2A.BU9_LD = licenceDatabase.PK;
				usage2A.BU9_LCC = clientCompany2.PK;
				usage2A.BU9_L7 = priceItemA.PK;
				usage2A.BU9_TransactionAmountPostDiscount = 250;
				usage2A.BU9_TransactionAmountPreDiscount = 275;
				usage2A.BU9_LocalAmountPostDiscount = 500;
				usage2A.BU9_LocalAmountPreDiscount = 550;
				usage2A.BU9_UsageCode = "STL";
				usage2A.BU9_UsageSubCode = "SHP";
				usage2A.BU9_PriceCode = priceItemA.L7_Code;
				usage2A.BU9_AC_AmountChargeCode = stlmthuseChargeCode.PK;
				usage2A.BU9_AC_DiscountChargeCode = discstlChargeCode.PK;
			}
			{
				var usage0B = Factory.New<EdiBilledUsage>();
				usage0B.BU9_AH_Invoice = invoice.PK;
				usage0B.BU9_LD = licenceDatabase.PK;
				usage0B.BU9_LCC = ZGuid.Empty;
				usage0B.BU9_L7 = priceItemB.PK;
				usage0B.BU9_TransactionAmountPostDiscount = 500;
				usage0B.BU9_TransactionAmountPreDiscount = 550;
				usage0B.BU9_LocalAmountPostDiscount = 1000;
				usage0B.BU9_LocalAmountPreDiscount = 1100;
				usage0B.BU9_UsageCode = "";
				usage0B.BU9_UsageSubCode = "";
				usage0B.BU9_PriceCode = priceItemB.L7_Code;
				usage0B.BU9_AC_AmountChargeCode = wisecloudChargeCode.PK;
				usage0B.BU9_AC_DiscountChargeCode = discwscChargeCode.PK;
			}
			{
				var feeUsage = Factory.New<EdiBilledUsage>();
				feeUsage.BU9_AH_Invoice = invoice.PK;
				feeUsage.BU9_LD = ZGuid.Empty;
				feeUsage.BU9_LCC = ZGuid.Empty;
				feeUsage.BU9_L7 = ZGuid.Empty;
				feeUsage.BU9_TransactionAmountPostDiscount = 100;
				feeUsage.BU9_TransactionAmountPreDiscount = 100;
				feeUsage.BU9_LocalAmountPostDiscount = 200;
				feeUsage.BU9_LocalAmountPreDiscount = 200;
				feeUsage.BU9_UsageCode = BillingConstants.BillingSystem.Fee;
				feeUsage.BU9_UsageSubCode = "EHUB";
				feeUsage.BU9_AC_AmountChargeCode = eHubChargeCode.PK;
			}
			{
				var usageNonCharged = Factory.New<EdiBilledUsage>();
				usageNonCharged.BU9_AH_Invoice = invoice.PK;
				usageNonCharged.BU9_LD = licenceDatabase.PK;
				usageNonCharged.BU9_LCC = clientCompany1.PK;
				usageNonCharged.BU9_L7 = nonChargedPriceItem.PK;
				usageNonCharged.BU9_TransactionAmountPostDiscount = 500;
				usageNonCharged.BU9_TransactionAmountPreDiscount = 550;
				usageNonCharged.BU9_LocalAmountPostDiscount = 1000;
				usageNonCharged.BU9_LocalAmountPreDiscount = 1100;
				usageNonCharged.BU9_UsageCode = "";
				usageNonCharged.BU9_UsageSubCode = "";
				usageNonCharged.BU9_AC_AmountChargeCode = stlmthuseChargeCode.PK;
				usageNonCharged.BU9_AC_DiscountChargeCode = discstlChargeCode.PK;
			}

			var usageGroupsCalculator = new StlBilledUsageCommissionGroupsCalculator();
			var commissionCreator = new BillingCommissionCreator(invoice, usageGroupsCalculator);
			commissionCreator.CreateCommissions();

			var commissionHeaders = Factory.Load<EdiCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, invoice.PK));
			{
				var commission1A = commissionHeaders.Single(x => x.AdditionalInfo != null && x.AdditionalInfo.ECH_LCC == clientCompany1.PK && x.CH0_SubModule == "AAA");
				CombineAssertions("commission1A Properties", () =>
				{
					AssertEquals("CH0_GC", invoice.AH_GC, commission1A.CH0_GC);
					AssertEquals("CH0_GroupingSourceTableCode", invoice.TablePrefix, commission1A.CH0_GroupingSourceTableCode);
					AssertEquals("CH0_GroupingSourceID", invoice.PK, commission1A.CH0_GroupingSourceID);
					AssertEquals("CH0_CA0", agreement.PK, commission1A.CH0_CA0);
					AssertEquals("CH0_OH_Debtor", org.PK, commission1A.CH0_OH_Debtor);
					AssertEquals("CH0_OH_Customer", org.PK, commission1A.CH0_OH_Customer);
					AssertEquals("CH0_Product", ProductTypes.Codes.Enterprise, commission1A.CH0_Product);
					AssertEquals("CH0_Service", BillingConstants.BillingSystem.STL, commission1A.CH0_Service);
					AssertEquals("CH0_SubModule", "AAA", commission1A.CH0_SubModule);

					AssertEquals("CH0_CommissionDate", new ZDate(2010, 10, 1), commission1A.CH0_CommissionDate);
					AssertEquals("CH0_SnapshotDateTime", new ZDateTime(2010, 10, 1), commission1A.CH0_SnapshotDateTime);
					AssertEquals("CH0_SnapshotEventCode", AccCommissionHeaderSnapshotEventList.Codes.Posted, commission1A.CH0_SnapshotEventCode);

					AssertEquals("AdditionalInfo.ECH_LD", licenceDatabase.PK, commission1A.AdditionalInfo.ECH_LD);
				});

				var stlmthuseAmountLineGroup = commission1A.LineGroups.First(x => x.CLG_AC == stlmthuseChargeCode.PK);
				CombineAssertions("stlmthuseAmountLineGroup Properties", () =>
				{
					AssertEquals("CLG_RX_NKTransactionCurrency", "USD", stlmthuseAmountLineGroup.CLG_RX_NKTransactionCurrency);
					AssertEquals("CLG_TransactionAmount", 125m + 75m, stlmthuseAmountLineGroup.CLG_TransactionAmount);
					AssertEquals("CLG_RX_NKCommissionCurrency", "AUD", stlmthuseAmountLineGroup.CLG_RX_NKCommissionCurrency);
					AssertEquals("CLG_TotalCommissionableAmount", 250m + 150m, stlmthuseAmountLineGroup.CLG_TotalCommissionableAmount);
					AssertEquals(1, stlmthuseAmountLineGroup.Lines.Count);
				});

				var discstlDiscountLineGroup = commission1A.LineGroups.First(x => x.CLG_AC == discstlChargeCode.PK);
				CombineAssertions("discstlDiscountLineGroup Properties", () =>
				{
					AssertEquals("CLG_RX_NKTransactionCurrency", "USD", discstlDiscountLineGroup.CLG_RX_NKTransactionCurrency);
					AssertEquals("CLG_TransactionAmount", -25m - 25m, discstlDiscountLineGroup.CLG_TransactionAmount);
					AssertEquals("CLG_RX_NKCommissionCurrency", "AUD", discstlDiscountLineGroup.CLG_RX_NKCommissionCurrency);
					AssertEquals("CLG_TotalCommissionableAmount", -50m - 50m, discstlDiscountLineGroup.CLG_TotalCommissionableAmount);
					AssertEquals(1, discstlDiscountLineGroup.Lines.Count);
				});

				AssertEquals(2, commission1A.LineGroups.Count);
			}

			{
				var commission1B = commissionHeaders.Single(x => x.AdditionalInfo != null && x.AdditionalInfo.ECH_LCC == clientCompany1.PK && x.CH0_SubModule == "BBB");
				CombineAssertions("commission1B Properties", () =>
				{
					AssertEquals("CH0_GC", invoice.AH_GC, commission1B.CH0_GC);
					AssertEquals("CH0_GroupingSourceTableCode", invoice.TablePrefix, commission1B.CH0_GroupingSourceTableCode);
					AssertEquals("CH0_GroupingSourceID", invoice.PK, commission1B.CH0_GroupingSourceID);
					AssertEquals("CH0_CA0", agreement.PK, commission1B.CH0_CA0);
					AssertEquals("CH0_OH_Debtor", org.PK, commission1B.CH0_OH_Debtor);
					AssertEquals("CH0_OH_Customer", org.PK, commission1B.CH0_OH_Customer);
					AssertEquals("CH0_Product", ProductTypes.Codes.Enterprise, commission1B.CH0_Product);
					AssertEquals("CH0_Service", BillingConstants.BillingSystem.STL, commission1B.CH0_Service);
					AssertEquals("CH0_SubModule", "BBB", commission1B.CH0_SubModule);

					AssertEquals("CH0_CommissionDate", new ZDate(2010, 10, 1), commission1B.CH0_CommissionDate);
					AssertEquals("CH0_SnapshotDateTime", new ZDateTime(2010, 10, 1), commission1B.CH0_SnapshotDateTime);
					AssertEquals("CH0_SnapshotEventCode", AccCommissionHeaderSnapshotEventList.Codes.Posted, commission1B.CH0_SnapshotEventCode);

					AssertEquals("AdditionalInfo.ECH_LD", licenceDatabase.PK, commission1B.AdditionalInfo.ECH_LD);
				});

				var wisecloudAmountLineGroup = commission1B.LineGroups.First(x => x.CLG_AC == wisecloudChargeCode.PK);
				CombineAssertions("wisecloudAmountLineGroup Properties", () =>
				{
					AssertEquals("CLG_RX_NKTransactionCurrency", "USD", wisecloudAmountLineGroup.CLG_RX_NKTransactionCurrency);
					AssertEquals("CLG_TransactionAmount", 125m, wisecloudAmountLineGroup.CLG_TransactionAmount);
					AssertEquals("CLG_RX_NKCommissionCurrency", "AUD", wisecloudAmountLineGroup.CLG_RX_NKCommissionCurrency);
					AssertEquals("CLG_TotalCommissionableAmount", 250m, wisecloudAmountLineGroup.CLG_TotalCommissionableAmount);
					AssertEquals(1, wisecloudAmountLineGroup.Lines.Count);
				});

				var discwscDiscountLineGroup = commission1B.LineGroups.First(x => x.CLG_AC == discwscChargeCode.PK);
				CombineAssertions("discwscDiscountLineGroup Properties", () =>
				{
					AssertEquals("CLG_RX_NKTransactionCurrency", "USD", discwscDiscountLineGroup.CLG_RX_NKTransactionCurrency);
					AssertEquals("CLG_TransactionAmount", -25m, discwscDiscountLineGroup.CLG_TransactionAmount);
					AssertEquals("CLG_RX_NKCommissionCurrency", "AUD", discwscDiscountLineGroup.CLG_RX_NKCommissionCurrency);
					AssertEquals("CLG_TotalCommissionableAmount", -50m, discwscDiscountLineGroup.CLG_TotalCommissionableAmount);
					AssertEquals(1, discwscDiscountLineGroup.Lines.Count);
				});

				AssertEquals(2, commission1B.LineGroups.Count);
			}

			{
				var commission2 = commissionHeaders.Single(x => x.AdditionalInfo != null && x.AdditionalInfo.ECH_LCC == clientCompany2.PK);
				CombineAssertions("commission2 Properties", () =>
				{
					AssertEquals("CH0_GC", invoice.AH_GC, commission2.CH0_GC);
					AssertEquals("CH0_GroupingSourceTableCode", invoice.TablePrefix, commission2.CH0_GroupingSourceTableCode);
					AssertEquals("CH0_GroupingSourceID", invoice.PK, commission2.CH0_GroupingSourceID);
					AssertEquals("CH0_CA0", agreement.PK, commission2.CH0_CA0);
					AssertEquals("CH0_OH_Debtor", org.PK, commission2.CH0_OH_Debtor);
					AssertEquals("CH0_OH_Customer", org.PK, commission2.CH0_OH_Customer);
					AssertEquals("CH0_Product", ProductTypes.Codes.Enterprise, commission2.CH0_Product);
					AssertEquals("CH0_Service", BillingConstants.BillingSystem.STL, commission2.CH0_Service);
					AssertEquals("CH0_SubModule", "AAA", commission2.CH0_SubModule);

					AssertEquals("CH0_CommissionDate", new ZDate(2010, 10, 1), commission2.CH0_CommissionDate);
					AssertEquals("CH0_SnapshotDateTime", new ZDateTime(2010, 10, 1), commission2.CH0_SnapshotDateTime);
					AssertEquals("CH0_SnapshotEventCode", AccCommissionHeaderSnapshotEventList.Codes.Posted, commission2.CH0_SnapshotEventCode);

					AssertEquals("AdditionalInfo.ECH_LD", licenceDatabase.PK, commission2.AdditionalInfo.ECH_LD);
				});

				var stlmthuseAmountLineGroup = commission2.LineGroups.First(x => x.CLG_AC == stlmthuseChargeCode.PK);
				CombineAssertions("stlmthuseAmountLineGroup Properties", () =>
				{
					AssertEquals("CLG_RX_NKTransactionCurrency", "USD", stlmthuseAmountLineGroup.CLG_RX_NKTransactionCurrency);
					AssertEquals("CLG_TransactionAmount", 275m, stlmthuseAmountLineGroup.CLG_TransactionAmount);
					AssertEquals("CLG_RX_NKCommissionCurrency", "AUD", stlmthuseAmountLineGroup.CLG_RX_NKCommissionCurrency);
					AssertEquals("CLG_TotalCommissionableAmount", 550m, stlmthuseAmountLineGroup.CLG_TotalCommissionableAmount.Round(0));
					AssertEquals(1, stlmthuseAmountLineGroup.Lines.Count);
				});

				var discstlDiscountLineGroup = commission2.LineGroups.First(x => x.CLG_AC == discstlChargeCode.PK);
				CombineAssertions("discstlDiscountLineGroup Properties", () =>
				{
					AssertEquals("CLG_RX_NKTransactionCurrency", "USD", discstlDiscountLineGroup.CLG_RX_NKTransactionCurrency);
					AssertEquals("CLG_TransactionAmount", -25m, discstlDiscountLineGroup.CLG_TransactionAmount);
					AssertEquals("CLG_RX_NKCommissionCurrency", "AUD", discstlDiscountLineGroup.CLG_RX_NKCommissionCurrency);
					AssertEquals("CLG_TotalCommissionableAmount", -50m, discstlDiscountLineGroup.CLG_TotalCommissionableAmount.Round(0));
					AssertEquals(1, discstlDiscountLineGroup.Lines.Count);
				});

				AssertEquals(2, commission2.LineGroups.Count);
			}

			{
				var commission0 = commissionHeaders.Single(x => x.AdditionalInfo != null && x.AdditionalInfo.ECH_LD == licenceDatabase.PK && x.AdditionalInfo.ECH_LCC.IsEmpty);
				CombineAssertions("commission0 Properties", () =>
				{
					AssertEquals("CH0_GC", invoice.AH_GC, commission0.CH0_GC);
					AssertEquals("CH0_GroupingSourceTableCode", invoice.TablePrefix, commission0.CH0_GroupingSourceTableCode);
					AssertEquals("CH0_GroupingSourceID", invoice.PK, commission0.CH0_GroupingSourceID);
					AssertEquals("CH0_CA0", agreement.PK, commission0.CH0_CA0);
					AssertEquals("CH0_OH_Debtor", org.PK, commission0.CH0_OH_Debtor);
					AssertEquals("CH0_OH_Customer", org.PK, commission0.CH0_OH_Customer);
					AssertEquals("CH0_Product", ProductTypes.Codes.Enterprise, commission0.CH0_Product);
					AssertEquals("CH0_Service", BillingConstants.BillingSystem.STL, commission0.CH0_Service);
					AssertEquals("CH0_SubModule", "BBB", commission0.CH0_SubModule);

					AssertEquals("CH0_CommissionDate", new ZDate(2010, 10, 1), commission0.CH0_CommissionDate);
					AssertEquals("CH0_SnapshotDateTime", new ZDateTime(2010, 10, 1), commission0.CH0_SnapshotDateTime);
					AssertEquals("CH0_SnapshotEventCode", AccCommissionHeaderSnapshotEventList.Codes.Posted, commission0.CH0_SnapshotEventCode);
				});

				var wisecloudAmountLineGroup = commission0.LineGroups.First(x => x.CLG_AC == wisecloudChargeCode.PK);
				CombineAssertions("wisecloudAmountLineGroup Properties", () =>
				{
					AssertEquals("CLG_RX_NKTransactionCurrency", "USD", wisecloudAmountLineGroup.CLG_RX_NKTransactionCurrency);
					AssertEquals("CLG_TransactionAmount", 550m, wisecloudAmountLineGroup.CLG_TransactionAmount);
					AssertEquals("CLG_RX_NKCommissionCurrency", "AUD", wisecloudAmountLineGroup.CLG_RX_NKCommissionCurrency);
					AssertEquals("CLG_TotalCommissionableAmount", 1100m, wisecloudAmountLineGroup.CLG_TotalCommissionableAmount.Round(0));
					AssertEquals(1, wisecloudAmountLineGroup.Lines.Count);
				});

				var discwscDiscountLineGroup = commission0.LineGroups.First(x => x.CLG_AC == discwscChargeCode.PK);
				CombineAssertions("discwscDiscountLineGroup Properties", () =>
				{
					AssertEquals("CLG_RX_NKTransactionCurrency", "USD", discwscDiscountLineGroup.CLG_RX_NKTransactionCurrency);
					AssertEquals("CLG_TransactionAmount", -50m, discwscDiscountLineGroup.CLG_TransactionAmount);
					AssertEquals("CLG_RX_NKCommissionCurrency", "AUD", discwscDiscountLineGroup.CLG_RX_NKCommissionCurrency);
					AssertEquals("CLG_TotalCommissionableAmount", -100m, discwscDiscountLineGroup.CLG_TotalCommissionableAmount.Round(0));
					AssertEquals(1, discwscDiscountLineGroup.Lines.Count);
				});

				AssertEquals(2, commission0.LineGroups.Count);
			}

			{
				var commissionFee = commissionHeaders.Single(x => x.AdditionalInfo == null);
				CombineAssertions("commissionFee Properties", () =>
				{
					AssertEquals("CH0_GC", invoice.AH_GC, commissionFee.CH0_GC);
					AssertEquals("CH0_GroupingSourceTableCode", invoice.TablePrefix, commissionFee.CH0_GroupingSourceTableCode);
					AssertEquals("CH0_GroupingSourceID", invoice.PK, commissionFee.CH0_GroupingSourceID);
					AssertEquals("CH0_CA0", agreement.PK, commissionFee.CH0_CA0);
					AssertEquals("CH0_OH_Debtor", org.PK, commissionFee.CH0_OH_Debtor);
					AssertEquals("CH0_OH_Customer", org.PK, commissionFee.CH0_OH_Customer);
					AssertEquals("CH0_Product", ProductTypes.Codes.Enterprise, commissionFee.CH0_Product);
					AssertEquals("CH0_Service", BillingConstants.BillingSystem.Fee, commissionFee.CH0_Service);
					AssertEquals("CH0_SubModule", "EHUB", commissionFee.CH0_SubModule);

					AssertEquals("CH0_CommissionDate", new ZDate(2010, 10, 1), commissionFee.CH0_CommissionDate);
					AssertEquals("CH0_SnapshotDateTime", new ZDateTime(2010, 10, 1), commissionFee.CH0_SnapshotDateTime);
					AssertEquals("CH0_SnapshotEventCode", AccCommissionHeaderSnapshotEventList.Codes.Posted, commissionFee.CH0_SnapshotEventCode);
				});

				var amountLineGroup = commissionFee.LineGroups.Single();
				CombineAssertions("amountLineGroup Properties", () =>
				{
					AssertEquals("CLG_RX_NKTransactionCurrency", "USD", amountLineGroup.CLG_RX_NKTransactionCurrency);
					AssertEquals("CLG_TransactionAmount", 100m, amountLineGroup.CLG_TransactionAmount);
					AssertEquals("CLG_RX_NKCommissionCurrency", "AUD", amountLineGroup.CLG_RX_NKCommissionCurrency);
					AssertEquals("CLG_TotalCommissionableAmount", 200m, amountLineGroup.CLG_TotalCommissionableAmount.Round(0));
					AssertEquals(1, amountLineGroup.Lines.Count);
				});

				AssertEquals(1, commissionFee.LineGroups.Count);
			}

			AssertEquals(6, commissionHeaders.Length);
		}

		public void TestStlCommissionWithUniversalUsage()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RX_NKLocalCurrency = "AUD";

			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();

			var org = Factory.NewWithValidTestData<OrgHeader>();

			var agreement1 = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreementForAllItems(Factory, org);
			OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(agreement1, "ENT", "STL", "ALL");
			var recipient1 = OrgCommissionAgreementTestHelper.AddPercentageRecipient(agreement1, "TS1", 10);

			var agreement2 = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreementForAllItems(Factory, org);
			OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(agreement2, "ENT", "CTR", "ALL");
			var recipient2 = OrgCommissionAgreementTestHelper.AddPercentageRecipient(agreement2, "TS2", 10);

			var stlmthuseChargeCode = GetOrCreateChargeCode("STLMTHUSE");

			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise.LE_OH = org.PK;
			var database = Factory.NewWithValidTestData<LicenceDatabase>();
			database.LD_LE = enterprise.PK;
			var clientCompany = Factory.NewWithValidTestData<ClientCompany>();
			clientCompany.LCC_LD = database.PK;

			var invoice = Factory.New<ARInvoice>();
			invoice.AH_GC = company.PK;
			invoice.AH_GB = branch.PK;
			invoice.AH_OH = org.PK;
			invoice.AH_PostDate = new ZDateTime(2018, 11, 1);
			invoice.AH_RX_NKTransactionCurrency = "USD";
			invoice.AH_ExchangeRate = 2.0m;

			var priceList = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			priceList.L6_SystemCode = "CTR";
			var priceItem = priceList.Items.AddNew();
			priceItem.L7_Code = "CTR";
			priceItem.L7_ChargeCode = "STLMTHUSE";

			var usage1 = Factory.New<EdiBilledUsage>();
			usage1.BU9_PeriodStart = new ZDate(2018, 10, 10);
			usage1.BU9_AH_Invoice = invoice.PK;
			usage1.BU9_LD = database.PK;
			usage1.BU9_LCC = clientCompany.PK;
			usage1.BU9_L7 = priceItem.PK;
			usage1.BU9_TransactionAmountPostDiscount = 55;
			usage1.BU9_TransactionAmountPreDiscount = 75;
			usage1.BU9_TransactionProcessingAmount = 5;
			usage1.BU9_LocalAmountPostDiscount = 110;
			usage1.BU9_LocalAmountPreDiscount = 150;
			usage1.BU9_LocalProcessingAmount = 10;
			usage1.BU9_UsageCode = "CTR";
			usage1.BU9_UsageSubCode = "CTR";
			usage1.BU9_BillingModel = "STL";
			usage1.BU9_PriceCode = priceItem.L7_Code;
			usage1.BU9_AC_AmountChargeCode = stlmthuseChargeCode.PK;

			Factory.Save();

			var usageGroupsCalculator = new StlBilledUsageCommissionGroupsCalculator();
			var commissionCreator = new BillingCommissionCreator(invoice, usageGroupsCalculator);
			commissionCreator.CreateCommissions();

			var commissionHeaders = Factory.Load<EdiCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, invoice.PK));

			AssertEquals(1, commissionHeaders.Length);

			var commission1 = commissionHeaders[0];
			AssertEquals("CH0_Product", ProductTypes.Codes.Enterprise, commission1.CH0_Product);
			AssertEquals("CH0_Service", BillingConstants.BillingSystem.GlobalContainerTracking, commission1.CH0_Service);
			AssertEquals("CH0_SubModule", "CTR", commission1.CH0_SubModule);
			AssertEquals("CH0_CA0", agreement2.PK, commission1.CH0_CA0);

			var lineGroup1 = commission1.LineGroups.Single(x => x.CLG_AC == stlmthuseChargeCode.PK);
			CombineAssertions("lineGroup1 Properties", () =>
			{
				AssertEquals("CLG_RX_NKTransactionCurrency", "USD", lineGroup1.CLG_RX_NKTransactionCurrency);
				AssertEquals("CLG_TransactionAmount", 75m, lineGroup1.CLG_TransactionAmount);
				AssertEquals("CLG_RX_NKCommissionCurrency", "AUD", lineGroup1.CLG_RX_NKCommissionCurrency);
				AssertEquals("CLG_TotalCommissionableAmount", 150m, lineGroup1.CLG_TotalCommissionableAmount);
				AssertEquals(1, lineGroup1.Lines.Count);
			});
		}

		AccChargeCode GetOrCreateChargeCode(ZString code)
		{
			var chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, code));
			if (chargeCode == null)
			{
				chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				chargeCode.AC_Code = code;
			}

			return chargeCode;
		}
	}
}