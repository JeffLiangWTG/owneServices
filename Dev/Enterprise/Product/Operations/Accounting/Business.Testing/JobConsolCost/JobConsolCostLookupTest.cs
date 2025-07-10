using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using PrepaidCollectCodes = Enterprise.Accounting.Integration.PrepaidCollectFreightForwardingList.Codes;

namespace Enterprise.Accounting.Business.ConsolCosting.Testing
{
	internal class JobConsolCostLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRatingBehaviourList_UnpostedNonSpotCharge()
		{
			AssertRatingBehaviour(false, false, "REA", new[] { "REA", "NEW", "STP" });
			AssertRatingBehaviour(false, true, "REA", new[] { "REA", "NEW", "STP" });
		}

		public void TestRatingBehaviourList_UnpostedSpotCharge_SpotRegistryOn()
		{
			AssertRatingBehaviour(false, true, RatingBehaviours.Spot, new[] { "", "REA", "NEW", "SPT", "SBA", "SBF", "SAA", "SAF" });
		}

		public void TestRatingBehaviourList_PostedNonSpotCharge()
		{
			AssertRatingBehaviour(true, false, "REA", new[] { "NEW", "STP" });
			AssertRatingBehaviour(true, true, "REA", new[] { "NEW", "STP" });
		}

		public void TestRatingBehaviourList_PostedSpotCharge_SpotRegistryOn()
		{
			AssertRatingBehaviour(true, true, RatingBehaviours.Spot, new[] { "NEW", "STP" });
		}

		public void TestRatingBehaviourList_ReversedNonSpotCharge()
		{
			AssertRatingBehaviourValueAfterReverse(false, "REA", new[] { "NEW", "REA", "STP" });
			AssertRatingBehaviourValueAfterReverse(true, "REA", new[] { "NEW", "REA", "STP" });
		}

		public void TestRatingBehaviourList_ReversedSpotCharge_SpotRegistryOn()
		{
			AssertRatingBehaviourValueAfterReverse(true, RatingBehaviours.Spot, new[] { "NEW", "REA", "STP" });
		}

		void AssertRatingBehaviour(bool isPosted, bool isSpotRegistryOn, string currentRatingBehaviour, IEnumerable<string> expectedList)
		{
			using (RatingDataRegistry.Instance.EnableSpotRatingBehaviourFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isSpotRegistryOn))
			{
				var jobConsolCost = Factory.NewWithValidTestData<JobConsolCost>();
				jobConsolCost.E6_RatingBehaviour = currentRatingBehaviour;
				if (isPosted)
				{
					var consolAPInvoice = Factory.New<APInvoice>();
					var random = new Random();
					consolAPInvoice.AH_TransactionNum = random.Next().ToString();
					jobConsolCost.E6_AH_APInvoice = consolAPInvoice.PK; // posting
				}
				else
				{
					jobConsolCost.E6_AH_APInvoice = Guid.Empty;
				}
				Factory.Save();

				AssertContainsExactElementsInAnyOrder(expectedList, jobConsolCost.Lookups.RatingBehaviourList.GetAllCodes());
			}
		}

		void AssertRatingBehaviourValueAfterReverse(bool isSpotRegistryOn, string currentRatingBehaviour, IEnumerable<string> expectedList)
		{
			using (RatingDataRegistry.Instance.EnableSpotRatingBehaviourFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isSpotRegistryOn))
			{
				var jobConsolCost = Factory.NewWithValidTestData<JobConsolCost>();
				jobConsolCost.E6_RatingBehaviour = currentRatingBehaviour;
				var consolAPInvoice = Factory.New<APInvoice>();
				var random = new Random();
				consolAPInvoice.AH_TransactionNum = random.Next().ToString();
				jobConsolCost.E6_AH_APInvoice = consolAPInvoice.PK; // posting
				Factory.Save();
				AssertEquals(true, jobConsolCost.IsPosted);
				AssertEquals(RatingBehaviours.StopFromAutorating, jobConsolCost.E6_RatingBehaviour);
				jobConsolCost.E6_AH_APInvoice = Guid.Empty; // unpost
				AssertEquals(false, jobConsolCost.IsPosted);
				AssertEquals(RatingBehaviours.ReAutorateCharge, jobConsolCost.E6_RatingBehaviour);
				Factory.Save();

				var lookup = new JobConsolCostLookups(jobConsolCost);
				var ratingBehaviours = lookup.RatingBehaviourList;
				expectedList.ForEach(e =>
				{
					Assert(ratingBehaviours.ContainsCode(e));
				});
			}
		}

		public void TestSupplyTypes()
		{
			var jobConsolCost = Factory.NewWithValidTestData<JobConsolCost>();
			AssertEquals("Default value", "LOC, LOX, LOA, INT, INX, INA, DSB", jobConsolCost.Lookups.SupplyTypes.CodesAsString);

			var values = new CodeDescriptionBoolDisallowNewCollection(AccountingMasterFilesConstants.SupplyTypeClassificationList);
			values.Cast<CodeDescriptionBool>().ForEach(x => x.Bool = true);
			values[0].Bool = false;
			using (AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesList.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, values))
			{
				AssertEquals("LOC was removed from list", "LOX, LOA, INT, INX, INA, DSB", jobConsolCost.Lookups.SupplyTypes.CodesAsString);
			}

			values.Cast<CodeDescriptionBool>().ForEach(x => x.Bool = false);
			using (AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesList.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, values))
			{
				AssertNullOrEmpty("All removed", jobConsolCost.Lookups.SupplyTypes.CodesAsString);
			}
		}

		public void TestTaxMessagesIsUsingTheCorrectCountryCodeFilter()
		{
			var creator = new TestObjectCreator(Factory);
			var taxMsg = creator.CreateTaxMsg("SG1", "SG1 Description", "English Msg 1", "Local Msg 1");
			var query = new ZQuery(GlbCompanySchema.GC_RN_NKCountryCode, SQLComparisonOperator.NotEqual, new string[] { GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Constants.CountryCodes.France });
			var otherCompany = Factory.LoadTop1<GlbCompany>(query);
			taxMsg.A9_RN_NKCountryCode = otherCompany.GC_RN_NKCountryCode;
			var jobConsolCost = Factory.NewWithValidTestData<JobConsolCost>();
			jobConsolCost.E6_GC = otherCompany.PK;
			Factory.Save();

			var lookup = new JobConsolCostLookups(jobConsolCost);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France))
			{
				Assert("Parent.Company is US, Current Company is France, taxMsg is US -> it should contains the message", lookup.VATClasses.Contains(taxMsg));
				taxMsg.A9_RN_NKCountryCode = Constants.CountryCodes.France;
				Factory.Save();
				Assert("Parent.Company is US, Current Company is France, taxMsg is France -> it should not contains the message", !lookup.VATClasses.Contains(taxMsg));
				jobConsolCost.E6_GC = ZGuid.Empty;
				Assert("Parent.Company is null, Current Company is France, taxMsg is France -> it should contains the message", lookup.VATClasses.Contains(taxMsg));
			}
		}

		public void TestPlacesOfSupply()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				var cost = Factory.NewWithValidTestData<JobConsolCost>();
				AssertEquals(typeof(ReadOnlyCodeDescriptionPairList), cost.Lookups.PlacesOfSupply.GetType());

				var placesCodes = cost.Lookups.PlacesOfSupply.GetAllCodes();
				var states = new RefCountryStatesDependentCollection(RefCountry.LoadFromCountryCode(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode), Factory);
				states.Load();
				var statesCodes = states.Cast<RefCountryStates>().Select(x => x.RW_Code).ToList();
				statesCodes.Add(PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry);
				statesCodes.Add(PlaceOfSupplyListProvider.Codes.OtherTerritories);
				AssertContainsExactElementsInAnyOrder(statesCodes, placesCodes);

				var types = cost.Lookups.PlaceOfSupplyTypes;
				AssertEquals("Expect 2 elements", 2, types.Count);
				AssertCollectionContains("State", PlaceOfSupplyTypes.State, types);
				AssertCollectionContains("PredefinedRule", PlaceOfSupplyTypes.PredefinedRule, types);
			}
		}

		public void TestChargeCodesLookupMustNotBeNull()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Shipments.AddNew();
			ApportionmentListing apportionmentListing = new ApportionmentListing(Factory, consol);
			JobConsolCostCollection costs = apportionmentListing.CostsCollection;
			JobConsolCost cost = costs.TryAddNew();
			cost.E6_AC_ChargeCode = creator.FRT.PK;
			cost.E6_OH_Creditor = creator.AALSHI.PK;
			Factory.Save();

			ApportionmentListing apportionmentListingReloaded = null;
			try
			{
				BusinessObjectFactory factory2 = new BusinessObjectFactory();
				ForwardingConsol consol2 = factory2.Load<ForwardingConsol>(consol.PK);
				apportionmentListingReloaded = new ApportionmentListing(factory2, consol2);
				JobConsolCost costReladed = apportionmentListingReloaded.CostsCollection[0];
				AssertNotNull("ChargeCodes Lookup should not be null", costReladed.Lookups.ChargeCodes);
			}
			finally
			{
				apportionmentListingReloaded.ReleaseMutexes();
			}
		}

		public void TestChargeCodeCollectionFilters()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();

			GlbDepartment yYYDept = Factory.NewWithValidTestData<GlbDepartment>();
			yYYDept.GE_Code = "YYY";

			GlbDepartment zZZDept = Factory.NewWithValidTestData<GlbDepartment>();
			zZZDept.GE_Code = "ZZZ";

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);

			try
			{
				JobConsolCostCollection costs = apps.CostsCollection;
				JobConsolCost testCost = costs.TryAddNew();
				testCost.ApportionmentCharges[0].JR_GE = yYYDept.PK;
				testCost.ApportionmentCharges[1].JR_GE = zZZDept.PK;

				FilterBusinessObjectDefault @default = testCost.Lookups.ChargeCodes.FilterBusinessObjectDefaults["Dept Filter" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"];

				AssertEquals("YYY, ZZZ, ALL", (ZString)@default.Value);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestChargeCodeCollectionFiltersOnChargeType()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();
			consol.Shipments.AddNew();

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);

			try
			{
				JobConsolCostCollection costs = apps.CostsCollection;
				JobConsolCost testCost = costs.TryAddNew();

				AccChargeCode chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
				AccChargeCode chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
				AccChargeCode chargeCode3 = Factory.NewWithValidTestData<AccChargeCode>();
				AccChargeCode chargeCode4 = Factory.NewWithValidTestData<AccChargeCode>();

				chargeCode1.AC_ChargeType = "MRG";
				chargeCode2.AC_ChargeType = "DSB";
				chargeCode3.AC_ChargeType = "MJA";
				chargeCode4.AC_ChargeType = "ABC";

				var collection = testCost.Lookups.ChargeCodes;
				collection.Load();

				AssertEquals("Collection should contain MRG Type Charge Code", true, collection.Contains(chargeCode1));
				AssertEquals("Collection should contain DSB Type Charge Code", true, collection.Contains(chargeCode2));
				AssertEquals("Collection should contain MJA Type Charge Code", true, collection.Contains(chargeCode3));
				AssertEquals("Collection should not contain ABC Type Charge Code", false, collection.Contains(chargeCode4));
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestChequeBooksCollectionOfValidType()
		{
			JobConsolCost testCost = Factory.NewWithValidTestData<JobConsolCost>();
			AssertEquals("ChequeBooksColleciton should be of valid type", typeof(ActiveChequeBookCollection), testCost.Lookups.ChequeBooks.GetType());
		}

		public void TestBankAccounts()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			GlbCompany otherCompany = Factory.NewWithValidTestData<GlbCompany>();

			AccBankAccount currentCompanyBankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			currentCompanyBankAccount.AB_GC = GlbCompany.CurrentCompany.PK;
			currentCompanyBankAccount.AB_RX_NKAccountCurrency = creator.AUD.RX_Code;
			AccBankAccount otherCompanyBankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			otherCompanyBankAccount.AB_GC = otherCompany.PK;
			currentCompanyBankAccount.AB_RX_NKAccountCurrency = creator.AUD.RX_Code;
			AccBankAccount otherCurrencyBankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			otherCurrencyBankAccount.AB_GC = GlbCompany.CurrentCompany.PK;
			otherCurrencyBankAccount.AB_RX_NKAccountCurrency = creator.USD.RX_Code;

			Factory.Save();

			JobConsolCost testCost = Factory.NewWithValidTestData<JobConsolCost>();
			testCost.E6_RX_NKCurrency = creator.AUD.RX_Code;
			AccBankAccountCollection bankAccounts = testCost.Lookups.BankAccounts;
			bankAccounts.Load();
			AssertEquals("Should contain 2 entries", 2, bankAccounts.Count);
			AssertEquals("Should contain current company bank account", true, bankAccounts.Contains(currentCompanyBankAccount));
			AssertEquals("Should not contain other company bank account", false, bankAccounts.Contains(otherCompanyBankAccount));
			AssertEquals("Should contain other currency bank account", true, bankAccounts.Contains(otherCurrencyBankAccount));
		}

		public void TestTaxRates()
		{
			AccTaxRate activeTaxRate = Factory.NewWithValidTestData<AccTaxRate>();
			activeTaxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			AccTaxRate otherCountryActiveTaxRate = Factory.NewWithValidTestData<AccTaxRate>();
			otherCountryActiveTaxRate.AT_RN_NKCountry = "GB";
			AccTaxRate inactiveTaxRate = Factory.NewWithValidTestData<AccTaxRate>();
			inactiveTaxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			inactiveTaxRate.AT_IsActive = false;

			JobConsolCost testCost = Factory.NewWithValidTestData<JobConsolCost>();
			var collection = testCost.Lookups.TaxRates;
			collection.Load();

			AssertEquals("Should contain active tax rate", true, collection.Contains(activeTaxRate));
			AssertEquals("Should not contain other company tax rate", false, collection.Contains(otherCountryActiveTaxRate));
			AssertEquals("Should not contain inactive tax rate", false, collection.Contains(inactiveTaxRate));
		}

		public void TestTaxRatesCollection()
		{
			var rate = Factory.NewWithValidTestData<AccTaxRate>();
			rate.AT_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			rate.AT_Type = AccTaxRate.Types.NotReportable;
			rate.AT_TaxSystemCode = "Other";

			var rate2 = Factory.NewWithValidTestData<AccTaxRate>();
			rate2.AT_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			rate2.AT_Code = "BBCXYZ";
			rate2.AT_Type = AccTaxRate.Types.NotReportable;
			rate2.AT_TaxSystemCode = "Other1";

			Factory.Save();

			JobConsolCost testCost = Factory.NewWithValidTestData<JobConsolCost>();
			var collection = testCost.Lookups.TaxRates;
			collection.Load();
			Assert("Collection should present only the VAT Tax System", collection.Cast<AccTaxRate>().All(item => item.AT_TaxSystemCode.IsEmpty));
		}

		public void TestPrepaidCollectList()
		{
			var consolCost = Factory.NewWithValidTestData<JobConsolCost>();
			var prepaidCollectList = consolCost.Lookups.PrepaidCollectList;
			AssertContainsExactElementsInAnyOrder(new[] {
				PrepaidCollectCodes.All,
				PrepaidCollectCodes.CTS,
				PrepaidCollectCodes.CCX,
				PrepaidCollectCodes.PPD,
				PrepaidCollectCodes.LOG,
				PrepaidCollectCodes.FOG,
				PrepaidCollectCodes.LDT,
				PrepaidCollectCodes.FDT,
			}, prepaidCollectList.ToArray().Select(x => x.Code));
		}

		public void TestApportionmentMethodList_WithNoExclusions()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var listing = new ApportionmentListing(Factory, consol);

			try
			{
				var consolCost = listing.CostsCollection.TryAddNew();
				var allocationMethodList = consolCost.Lookups.ApportionmentMethodList;

				AssertEquals("A forwarding consol is expected to allow all apportionment allocation methods", 11, allocationMethodList.Count);
				AssertEquals(true, allocationMethodList.ContainsCode(AllocationMethod.ChargeableUnits));
				AssertEquals(true, allocationMethodList.ContainsCode(AllocationMethod.Manual));
				AssertEquals(true, allocationMethodList.ContainsCode(AllocationMethod.Shipment));
				AssertEquals(true, allocationMethodList.ContainsCode(AllocationMethod.Revenue));
				AssertEquals(true, allocationMethodList.ContainsCode(AllocationMethod.GrossWeight));
				AssertEquals(true, allocationMethodList.ContainsCode(AllocationMethod.ContainerCount));
				AssertEquals(true, allocationMethodList.ContainsCode(AllocationMethod.OuterPackTotal));
				AssertEquals(true, allocationMethodList.ContainsCode(AllocationMethod.TwentyFootEquivalentUnit));
				AssertEquals(true, allocationMethodList.ContainsCode(AllocationMethod.CapacityPerContainer));
				AssertEquals(true, allocationMethodList.ContainsCode(AllocationMethod.FreeSpaceContribution));
				AssertEquals(true, allocationMethodList.ContainsCode(AllocationMethod.GrossVolume));
			}
			finally
			{
				listing.ReleaseMutexes();
			}
		}

		public void TestApportionmentMethodList_WithExclusionList()
		{
			var workSheet = Factory.NewWithValidTestData<CommonWorkSheet>();
			var listing = new ApportionmentListing(Factory, workSheet);

			try
			{
				var workSheetCost = listing.CostsCollection.TryAddNew();
				var apportionmentMethods = workSheetCost.Lookups.ApportionmentMethodList;
				AssertEquals("Some apportionment methods should be excluded for CommonWorkSheet", 8, apportionmentMethods.Count);
				AssertEquals("Expecting no Capacity per Container charge", false, apportionmentMethods.ContainsCode(AllocationMethod.CapacityPerContainer));
				AssertEquals("Expecting no Revenue charge", false, apportionmentMethods.ContainsCode(AllocationMethod.Revenue));
			}
			finally
			{
				listing.ReleaseMutexes();
			}
		}

		public void TestWithholdingTaxLookup()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var company1 = testObjectCreator.CreateNewCompany("ABC");
			var branch1 = testObjectCreator.CreateNewBranch(company1, "AB1");

			Factory.Save();

			var wht1 = testObjectCreator.CreateOrLoadWithholdingTax("WHT1", "WHT 1.0", 1M);
			AccWithholding wht3 = null;
			AccWithholding wht5 = null;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch1.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				wht3 = testObjectCreator.CreateOrLoadWithholdingTax("WHT3", "WHT 3.0", 3M);
				wht5 = testObjectCreator.CreateOrLoadWithholdingTax("WHT5", "WHT 5.0", 5M);
			}

			Factory.Save();

			var consol = testObjectCreator.CreateConsol();
			var listing = new ApportionmentListing(Factory, consol);
			var consolCost = listing.CostsCollection.TryAddNew();

			var collection = consolCost.Lookups.WithholdingTaxes;
			collection.Load();
			AssertCollectionContains(wht1, collection);
			AssertCollectionNotContains(wht3, collection);
			AssertCollectionNotContains(wht5, collection);

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch1.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				collection = consolCost.Lookups.WithholdingTaxes;
				collection.Load();
				AssertCollectionContains(wht3, collection);
				AssertCollectionContains(wht5, collection);

				AssertCollectionNotContains(wht1, collection);
			}
		}
	}
}
