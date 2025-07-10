using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(ClientFeatureRequestValueAndContribution))]
	internal class ClientFeatureRequestValueAndContributionTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLoad()
		{
			DummyBusinessObject parentBizO = Factory.New<DummyBusinessObject>();
			parentBizO.Z0_Description = "Test parent BizO";
			AssertNull("Should not load as there is no related ClientFeatureRequestValueAndContribution", ClientFeatureRequestValueAndContribution.Load(parentBizO));

			FeatureRequestValueAndContribution.T9_ParentID = parentBizO.PK;
			AssertEquals("Should load ClientFeatureRequestValueAndContribution", FeatureRequestValueAndContribution.PK, ClientFeatureRequestValueAndContribution.Load(parentBizO).PK);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestLoad_NullParent()
		{
			ClientFeatureRequestValueAndContribution.Load(null);
		}

		public void TestLoadOrCreateNew()
		{
			DummyBusinessObject parentBizO = Factory.New<DummyBusinessObject>();
			parentBizO.Z0_Description = "Test parent BizO";

			ClientFeatureRequestValueAndContribution result = ClientFeatureRequestValueAndContribution.LoadOrCreateNew(parentBizO);
			AssertNotNull("Should create new ClientFeatureRequestValueAndContribution", result);
			AssertEquals("Should set the ParentID", parentBizO.PK, result.T9_ParentID);
			AssertEquals("Should set the ParentTableCode", DummyBizoSchema.Constants.Prefix, result.T9_ParentTableCode);

			ClientFeatureRequestValueAndContribution result2 = ClientFeatureRequestValueAndContribution.LoadOrCreateNew(parentBizO);
			AssertEquals("Should load existing ClientFeatureRequestValueAndContribution", result, result2);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestLoadOrCreateNew_NullParent()
		{
			ClientFeatureRequestValueAndContribution.LoadOrCreateNew(null);
		}

		public void TestUniqueIndexFailureHandler()
		{
			var handlers = (IEnumerable<IUniqueIndexFailureHandler>)typeof(ClientFeatureRequestValueAndContribution).GetProperty("UniqueIndexFailureHandlers", BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(FeatureRequestValueAndContribution, null);
			var handler = handlers.Single();
			AssertEquals(typeof(ClientFeatureRequestValueAndContributionUniqueIndexFailureHandler), handler.GetType());
		}

		public void TestHasNonEmptyValueFields()
		{
			Assert("Pre-condition", !FeatureRequestValueAndContribution.HasNonEmptyValueFields);

			FeatureRequestValueAndContribution.T9_Contribution = 100;
			Assert(FeatureRequestValueAndContribution.HasNonEmptyValueFields);

			FeatureRequestValueAndContribution.T9_Contribution = ZDecimal.Zero;
			FeatureRequestValueAndContribution.T9_EBV = 200;
			Assert("Should check current (calculated) EBV state - not persist", !FeatureRequestValueAndContribution.HasNonEmptyValueFields);

			FeatureRequestValueAndContribution.T9_CostReduction = 300;
			Assert(FeatureRequestValueAndContribution.HasNonEmptyValueFields);
		}

		public void TestIsSavedByFactory()
		{
			Assert("Should not be saved if new one AND has no filled money fields", !FeatureRequestValueAndContribution.IsSavedByFactory);

			FeatureRequestValueAndContribution.T9_Contribution = 100;
			Assert("Should be saved if has Non empty money fields", FeatureRequestValueAndContribution.IsSavedByFactory);

			Factory.Save();
			FeatureRequestValueAndContribution.T9_Contribution = ZDecimal.Zero;
			Assert("Should be saved if already in DB even if all fields are empty", FeatureRequestValueAndContribution.IsSavedByFactory);
		}

		public void TestCurrencyCode()
		{
			Assert("Currency code should not be empty", !FeatureRequestValueAndContribution.CurrencyCode.IsEmpty);
			AssertEquals("CurrencyCode property should match default currency code constant", ClientFeatureRequestValueAndContribution.DefaultCurrencyCode, FeatureRequestValueAndContribution.CurrencyCode);
		}

		public void TestShouldValidateMandatoryEBV()
		{
			Assert("By default should be false", !FeatureRequestValueAndContribution.ShouldValidateMandatoryEBV);

			FeatureRequestValueAndContribution.SetShouldValidateMandatoryEBVDelegate(delegate
			{ return true; });
			Assert("ShouldValidateMandatoryEBV", FeatureRequestValueAndContribution.ShouldValidateMandatoryEBV);

			FeatureRequestValueAndContribution.SetShouldValidateMandatoryEBVDelegate(delegate
			{ return false; });
			Assert("ShouldValidateMandatoryEBV", !FeatureRequestValueAndContribution.ShouldValidateMandatoryEBV);

			FeatureRequestValueAndContribution.SetShouldValidateMandatoryEBVDelegate(null);
			Assert("By default should be false", !FeatureRequestValueAndContribution.ShouldValidateMandatoryEBV);
		}

		#region EBV

		public void TestCalculatedEBV()
		{
			Assert("Pre-condition", FeatureRequestValueAndContribution.CalculatedEBV.IsEmpty);

			FeatureRequestValueAndContribution.T9_EBV = 100;
			Assert("Should calculated current EBV state - sum all EBV parts", FeatureRequestValueAndContribution.CalculatedEBV.IsEmpty);

			FeatureRequestValueAndContribution.T9_CostReduction = 100;
			AssertEquals((ZDecimal)100, FeatureRequestValueAndContribution.CalculatedEBV);

			FeatureRequestValueAndContribution.T9_RiskReduction = 110;
			AssertEquals((ZDecimal)210, FeatureRequestValueAndContribution.CalculatedEBV);

			FeatureRequestValueAndContribution.T9_ReductionInErrorRates = 120;
			AssertEquals((ZDecimal)330, FeatureRequestValueAndContribution.CalculatedEBV);

			FeatureRequestValueAndContribution.T9_ProcessSimplification = 130;
			AssertEquals((ZDecimal)460, FeatureRequestValueAndContribution.CalculatedEBV);

			FeatureRequestValueAndContribution.T9_Satisfaction = 140;
			AssertEquals((ZDecimal)600, FeatureRequestValueAndContribution.CalculatedEBV);

			FeatureRequestValueAndContribution.T9_SalesImprovement = 150;
			AssertEquals((ZDecimal)750, FeatureRequestValueAndContribution.CalculatedEBV);

			FeatureRequestValueAndContribution.T9_PreventionOfLossOfCustomerOrBusiness = 160;
			AssertEquals((ZDecimal)910, FeatureRequestValueAndContribution.CalculatedEBV);

			FeatureRequestValueAndContribution.T9_CompetitiveAdvantage = 170;
			AssertEquals((ZDecimal)1080, FeatureRequestValueAndContribution.CalculatedEBV);

			FeatureRequestValueAndContribution.T9_AnyEffectThatLowersCostsOrGrowsRevenue = 180;
			AssertEquals((ZDecimal)1260, FeatureRequestValueAndContribution.CalculatedEBV);
		}

		public void TestEBVReadOnly()
		{
			Assert("EBV should be readonly", FeatureRequestValueAndContribution.T9_EBVInfo.ReadOnly);
		}

		public void TestPersistEBV()
		{
			Assert("Pre-condition", FeatureRequestValueAndContribution.T9_EBV.IsEmpty);

			FeatureRequestValueAndContribution.T9_CostReduction = 100;
			Assert("However EBV is calc. property, for speed-up it is stored in DB and set manually", FeatureRequestValueAndContribution.T9_EBV.IsEmpty);

			FeatureRequestValueAndContribution.PersistEBV();
			AssertEquals("EBV should be updated", (ZDecimal)100, FeatureRequestValueAndContribution.T9_EBV);

			FeatureRequestValueAndContribution.T9_CompetitiveAdvantage = 200;
			AssertEquals("EBV should not be updated", (ZDecimal)100, FeatureRequestValueAndContribution.T9_EBV);
			FeatureRequestValueAndContribution.PersistEBV();
			AssertEquals("EBV should be updated", (ZDecimal)300, FeatureRequestValueAndContribution.T9_EBV);
		}

		public void TestPersistEBVOnRunPreSaveValidation()
		{
			DummyBusinessObject parentBizO = Factory.New<DummyBusinessObject>();
			parentBizO.Z0_Description = "Test parent BizO";

			FeatureRequestValueAndContribution.T9_EBV = 100;
			FeatureRequestValueAndContribution.T9_CostReduction = 25;
			FeatureRequestValueAndContribution.T9_ProcessSimplification = 30;

			AssertEquals("Not yet updated", (ZDecimal)100, FeatureRequestValueAndContribution.T9_EBV);
			FeatureRequestValueAndContribution.RunPreSaveValidation();
			AssertEquals("Should be persisted now", (ZDecimal)55, FeatureRequestValueAndContribution.T9_EBV);
			Factory.Save();

			BusinessObjectFactory newFactory = NewFactory();
			ClientFeatureRequestValueAndContribution loadedFeatureRequestValueAndContribution =
				newFactory.Load<ClientFeatureRequestValueAndContribution>(FeatureRequestValueAndContribution.PK);
			AssertEquals("New value should be persisted", (ZDecimal)55, loadedFeatureRequestValueAndContribution.T9_EBV);
		}

		public void TestStoreRestoreCurrentEBVComponentValuesToFromMemory()
		{
			AssertEBVComponentValues(0, 0, 0, 0, 0, 0, 0, 0, 0);

			SetEBVComponentValues(1, 2, 3, 4, 5, 6, 7, 8, 9);
			FeatureRequestValueAndContribution.StoreCurrentEBVComponentValuesToMemory();

			SetEBVComponentValues(11, 12, 13, 14, 15, 16, 17, 18, 19);
			AssertEBVComponentValues(11, 12, 13, 14, 15, 16, 17, 18, 19);

			FeatureRequestValueAndContribution.RestoreEBVComponentValuesFromMemory();
			AssertEBVComponentValues(1, 2, 3, 4, 5, 6, 7, 8, 9);

			SetEBVComponentValues(21, 22, 23, 24, 25, 26, 27, 28, 29);
			AssertEBVComponentValues(21, 22, 23, 24, 25, 26, 27, 28, 29);

			FeatureRequestValueAndContribution.StoreCurrentEBVComponentValuesToMemory();

			SetEBVComponentValues(0, 0, 0, 0, 0, 0, 0, 0, 0);
			AssertEBVComponentValues(0, 0, 0, 0, 0, 0, 0, 0, 0);

			FeatureRequestValueAndContribution.RestoreEBVComponentValuesFromMemory();
			AssertEBVComponentValues(21, 22, 23, 24, 25, 26, 27, 28, 29);
		}

		#endregion

		public void TestOnSaving()
		{
			bool savingCalled = false;
			FeatureRequestValueAndContribution.Saving += delegate
				{
					savingCalled = true;
				};
			Assert(!FeatureRequestValueAndContribution.HasChanges);
			Factory.Save();
			Assert("HasChanges was false", !savingCalled);

			FeatureRequestValueAndContribution.T9_Contribution = 100;
			Assert(FeatureRequestValueAndContribution.HasChanges);
			Factory.Save();
			Assert(savingCalled);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			ClientFeatureRequestValueAndContribution result = (ClientFeatureRequestValueAndContribution)base.GetNewBusinessObjectForDeleteTest(factory);
			result.T9_Contribution = 100;
			return result;
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			FeatureRequestValueAndContribution = Factory.New<ClientFeatureRequestValueAndContribution>();
		}

		void SetEBVComponentValues(ZDecimal t9_CostReduction, ZDecimal t9_RiskReduction, ZDecimal t9_ReductionInErrorRates, ZDecimal t9_ProcessSimplification, ZDecimal t9_Satisfaction,
			ZDecimal t9_SalesImprovement, ZDecimal t9_PreventionOfLossOfCustomerOrBusiness, ZDecimal t9_CompetitiveAdvantage, ZDecimal t9_AnyEffectThatLowersCostsOrGrowsRevenue)
		{
			FeatureRequestValueAndContribution.T9_CostReduction = t9_CostReduction;
			FeatureRequestValueAndContribution.T9_RiskReduction = t9_RiskReduction;
			FeatureRequestValueAndContribution.T9_ReductionInErrorRates = t9_ReductionInErrorRates;
			FeatureRequestValueAndContribution.T9_ProcessSimplification = t9_ProcessSimplification;
			FeatureRequestValueAndContribution.T9_Satisfaction = t9_Satisfaction;
			FeatureRequestValueAndContribution.T9_SalesImprovement = t9_SalesImprovement;
			FeatureRequestValueAndContribution.T9_PreventionOfLossOfCustomerOrBusiness = t9_PreventionOfLossOfCustomerOrBusiness;
			FeatureRequestValueAndContribution.T9_CompetitiveAdvantage = t9_CompetitiveAdvantage;
			FeatureRequestValueAndContribution.T9_AnyEffectThatLowersCostsOrGrowsRevenue = t9_AnyEffectThatLowersCostsOrGrowsRevenue;
		}

		void AssertEBVComponentValues(ZDecimal t9_CostReduction, ZDecimal t9_RiskReduction, ZDecimal t9_ReductionInErrorRates, ZDecimal t9_ProcessSimplification, ZDecimal t9_Satisfaction,
			ZDecimal t9_SalesImprovement, ZDecimal t9_PreventionOfLossOfCustomerOrBusiness, ZDecimal t9_CompetitiveAdvantage, ZDecimal t9_AnyEffectThatLowersCostsOrGrowsRevenue)
		{
			AssertEquals("Incorrect value T9_CostReduction", t9_CostReduction, FeatureRequestValueAndContribution.T9_CostReduction);
			AssertEquals("Incorrect value T9_RiskReduction", t9_RiskReduction, FeatureRequestValueAndContribution.T9_RiskReduction);
			AssertEquals("Incorrect value T9_ReductionInErrorRates", t9_ReductionInErrorRates, FeatureRequestValueAndContribution.T9_ReductionInErrorRates);
			AssertEquals("Incorrect value T9_ProcessSimplification", t9_ProcessSimplification, FeatureRequestValueAndContribution.T9_ProcessSimplification);
			AssertEquals("Incorrect value T9_Satisfaction", t9_Satisfaction, FeatureRequestValueAndContribution.T9_Satisfaction);
			AssertEquals("Incorrect value T9_SalesImprovement", t9_SalesImprovement, FeatureRequestValueAndContribution.T9_SalesImprovement);
			AssertEquals("Incorrect value T9_PreventionOfLossOfCustomerOrBusiness", t9_PreventionOfLossOfCustomerOrBusiness, FeatureRequestValueAndContribution.T9_PreventionOfLossOfCustomerOrBusiness);
			AssertEquals("Incorrect value T9_CompetitiveAdvantage", t9_CompetitiveAdvantage, FeatureRequestValueAndContribution.T9_CompetitiveAdvantage);
			AssertEquals("Incorrect value T9_AnyEffectThatLowersCostsOrGrowsRevenue", t9_AnyEffectThatLowersCostsOrGrowsRevenue, FeatureRequestValueAndContribution.T9_AnyEffectThatLowersCostsOrGrowsRevenue);
		}

		#endregion

		ClientFeatureRequestValueAndContribution FeatureRequestValueAndContribution;
	}
}
