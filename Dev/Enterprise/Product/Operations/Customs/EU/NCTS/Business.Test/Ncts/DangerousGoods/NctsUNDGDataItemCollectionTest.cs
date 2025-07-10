using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsUNDGDataItemCollection))]
	sealed class NctsUNDGDataItemCollectionTest : ActiveBusinessObjectCollectionTestCase<NctsUNDGDataItemCollection>
	{
		protected override NctsUNDGDataItemCollection GetCollectionToTest()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			return header.Bills.AddNew().GoodsItems.AddNew().UNDGs;
		}

		public void TestMaxCountValidation_TR0027()
		{
			CombineAssertions(() =>
			{
				using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
				{
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0027Active));

					var guarantees = GetCollectionToTest();
					var maxCountValidator = ((ISupportMaxCountValidation)guarantees).MaxCountValidator;
					var notification = maxCountValidator.Notification;

					AssertEquals("MaxCount for Departure movement", 99, maxCountValidator.MaxCount);
					AssertEquals("Notification Type", NotificationType.MessageError, notification.Type);
					AssertEquals("Notification Message", "[TR0027] The maximum number of 99 Dangerous Goods Lines has been exceeded.", notification.Message);
					AssertEquals("WarnAtHalfway", false, maxCountValidator.WarnAtHalfway);

					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleTR0027Active));

					guarantees = GetCollectionToTest();
					maxCountValidator = ((ISupportMaxCountValidation)guarantees).MaxCountValidator;

					AssertEquals("MaxCount when RuleTR0027 is disabled", maxCountValidator.MaxCount, -1);
				}
			});
		}

		public void TestMaxCountValidation_E1406_InTransitionPeriod()
		{
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
				{
					using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
					{
						ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleE1406Active));
						ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleTR0027Active));

						var undgs = GetCollectionToTest();
						var maxCountValidator = ((ISupportMaxCountValidation)undgs).MaxCountValidator;
						var notification = maxCountValidator.Notification;

						AssertEquals("MaxCount for Departure movement", 1, maxCountValidator.MaxCount);
						AssertEquals("Notification Type", NotificationType.MessageError, notification.Type);
						AssertEquals("Notification Message", "[E1406] In transition period, which is now, a maximum of 1 Dangerous Goods Code can be entered", notification.Message);
						AssertEquals("WarnAtHalfway", false, maxCountValidator.WarnAtHalfway);

						ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleE1406Active));

						undgs = GetCollectionToTest();
						maxCountValidator = ((ISupportMaxCountValidation)undgs).MaxCountValidator;

						AssertEquals("MaxCount when RuleE1406 is disabled", maxCountValidator.MaxCount, -1);
					}
				}
			});
		}

		public void TestMaxCountValidation_E1406_NotInTransitionPeriod()
		{
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today.AddDays(-1), true))
				{
					using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
					{
						ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleE1406Active));
						ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleTR0027Active));

						var undgs = GetCollectionToTest();
						var maxCountValidator = ((ISupportMaxCountValidation)undgs).MaxCountValidator;
						var notification = maxCountValidator.Notification;

						AssertEquals("MaxCount when RuleE1406 is enabled, not in transition period", maxCountValidator.MaxCount, -1);

						ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleE1406Active));

						undgs = GetCollectionToTest();
						maxCountValidator = ((ISupportMaxCountValidation)undgs).MaxCountValidator;

						AssertEquals("MaxCount when RuleE1406 is disabled, not in transition period", maxCountValidator.MaxCount, -1);
					}
				}
			});
		}

		public void TestUniqueUNNumbers()
		{
			var substance0010 = UNDGSubstanceLoader.LoadSubstances(Factory, "0010", "", "IMO").First();
			var substance0012a = UNDGSubstanceLoader.LoadSubstances(Factory, "0012", "a", "IMO").First();

			var undgs = GetCollectionToTest();
			undgs.AddNew();
			undgs.AddNew().DI_DG = substance0010.PK;
			undgs.AddNew().DI_DG = substance0012a.PK;
			undgs.AddNew().DI_DG = substance0012a.PK;
			AssertContainsExactElementsInAnyOrder("UniqueUNNumbers", new[] { "0010", "0012" }, undgs.UniqueUNNumbers.ToArray());
		}
	}
}
