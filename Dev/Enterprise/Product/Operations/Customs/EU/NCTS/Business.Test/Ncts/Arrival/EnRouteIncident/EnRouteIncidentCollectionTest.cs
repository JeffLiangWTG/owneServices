using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using NUnit.Framework;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(EnRouteIncidentCollection))]
	class EnRouteIncidentCollectionTest : ActiveBusinessObjectCollectionTestCase<EnRouteIncidentCollection>
	{
		public void TestCollectionPhase4IsEditable()
		{
			AssertCollectionEditabilityWithNC5TPFunction(Common.CusInBondApplicationCodeList.Codes.NCTS4, false, true);
		}

		public void TestCollectionPhase5IsEditableDuringTransitionPeriod()
		{
			AssertCollectionEditabilityWithNC5TPFunction(Common.CusInBondApplicationCodeList.Codes.NCTS5, true, true);
		}

		public void TestCollectionPhase5IsReadOnlyOutsideTransitionPeriod()
		{
			AssertCollectionEditabilityWithNC5TPFunction(Common.CusInBondApplicationCodeList.Codes.NCTS5, false, false);
		}

		public void TestCollectionPhase5SetDefaultsForNewElement()
		{
			var collection = GetCollectionToTest();
			var item1 = collection.AddNew();
			item1.Header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			var item2 = collection.AddNew();
			CombineAssertions(() =>
			{
				AssertNotEquals("BN_CustomsStatus Not Phase5", IncidentCustomsStatusList.Codes.ONA, item1.BN_CustomsStatus);
				AssertEquals("BN_CustomsStatus Phase5", IncidentCustomsStatusList.Codes.ONA, item2.BN_CustomsStatus);
			});
		}

		public void TestAnyCreatedByCustomsMessage()
		{
			var collection = GetCollectionToTest();
			var item1 = collection.AddNew();
			var item2 = collection.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("Any incident created by customs", false, collection.AnyCreatedByCustomsMessage);
				item2.BN_CustomsStatus = IncidentCustomsStatusList.Codes.CUS;
				AssertEquals("Any incident created by customs", true, collection.AnyCreatedByCustomsMessage);
			});
		}

		public void TestMaxCount_RuleTR0011()
		{
			CombineAssertions(() =>
			{
				using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
				{
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0011Active));

					var header = Factory.New<NctsHeader>();
					header.SetMovementType(NctsMovementType.Codes.Arrival);
					header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
					var incidentCollection = new EnRouteIncidentCollection(header);

					var maxCountValidator = ((ISupportMaxCountValidation)incidentCollection).MaxCountValidator;
					var notification = maxCountValidator.Notification;

					AssertEquals("MaxCount", 9, maxCountValidator.MaxCount);
					AssertEquals("Notification Type", NotificationType.MessageError, notification.Type);
					AssertEquals("Notification Message", "[TR0011] Only 9 Incidents are allowed.", notification.Message);
					AssertEquals("WarnAtHalfway", false, maxCountValidator.WarnAtHalfway);

					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleTR0011Active));

					incidentCollection = new EnRouteIncidentCollection(header);

					maxCountValidator = ((ISupportMaxCountValidation)incidentCollection).MaxCountValidator;
					AssertEquals(maxCountValidator.MaxCount, -1);

					ruleTestContext.AssertRuleChecked(nameof(ValidationRuleConfiguration.IsRuleTR0011Active));
				}
			});
		}

		protected override EnRouteIncidentCollection GetCollectionToTest()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			return new EnRouteIncidentCollection(header);
		}

		void AssertCollectionEditabilityWithNC5TPFunction(string applicationCode, bool nc5tpEnabled, bool collectionIsEditable)
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, nc5tpEnabled))
			{
				var header = Factory.New<NctsHeader>();
				header.SetMovementType(NctsMovementType.Codes.Arrival);
				header.BH_ApplicationCode = applicationCode;
				var collection = (IBindingList)new EnRouteIncidentCollection(header);
				CombineAssertions(() =>
				{
					AssertEquals("AllowNew", collectionIsEditable, collection.AllowNew);
					AssertEquals("AllowEdit", collectionIsEditable, collection.AllowEdit);
					AssertEquals("AllowRemove", collectionIsEditable, collection.AllowRemove);
				});
			}
		}
	}
}
