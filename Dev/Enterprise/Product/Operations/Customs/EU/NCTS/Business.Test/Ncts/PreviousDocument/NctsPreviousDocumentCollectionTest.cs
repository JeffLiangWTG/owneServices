using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using NUnit.Framework;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsPreviousDocumentCollection<NctsPreviousDocument>))]
	class NctsPreviousDocumentCollectionTest : CusSupportingInfoCollectionTest<NctsPreviousDocument>
	{
		public void TestShortSequenceNumberGenerator()
		{
			AssertType<ShortSequenceNumberGenerator>("Collection should have a ShortSequenceNumberGenerator", GetNewCollection().SequenceGenerator);
		}

		public void TestMaxCountValidation_TR0030()
		{
			CombineAssertions(() =>
			{
				using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
				{
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0030Active));

					var previousDocuments = GetNctsPreviousDocumentCollectionForMovementTypeP5(NctsMovementType.Codes.Departure);
					var maxCountValidator = ((ISupportMaxCountValidation)previousDocuments).MaxCountValidator;
					var notification = maxCountValidator.Notification;

					AssertEquals("MaxCount for Departure movement", 99, maxCountValidator.MaxCount);
					AssertEquals("Notification Type", NotificationType.MessageError, notification.Type);
					AssertEquals("Notification Message", "[TR0030] The maximum number of 99 Previous Documents has been exceeded.", notification.Message);
					AssertEquals("WarnAtHalfway", false, maxCountValidator.WarnAtHalfway);

					previousDocuments = GetNctsPreviousDocumentCollectionForMovementTypeP5(NctsMovementType.Codes.Arrival);
					maxCountValidator = ((ISupportMaxCountValidation)previousDocuments).MaxCountValidator;

					AssertEquals("MaxCount for Arrival movement", -1, maxCountValidator.MaxCount);

					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleTR0030Active));

					previousDocuments = GetNctsPreviousDocumentCollectionForMovementTypeP5(NctsMovementType.Codes.Departure);
					maxCountValidator = ((ISupportMaxCountValidation)previousDocuments).MaxCountValidator;

					AssertEquals("MaxCount when RuleTR0030 is disabled", maxCountValidator.MaxCount, -1);
				}
			});
		}

		public void TestMaxCountValidation_E1401()
		{
			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleE1401_1Active));
				CombineAssertions("When Rule enabled", () =>
				{
					var previousDocuments = GetNctsPreviousDocumentCollectionForDepartureypeP4();
					AssertEquals("For Phase 4 Departure, MaxCount", -1, previousDocuments.MaxCount);

					previousDocuments = GetNctsPreviousDocumentCollectionForMovementTypeP5(NctsMovementType.Codes.Arrival);
					AssertEquals("For Phase 5 Arrival, MaxCount", -1, previousDocuments.MaxCount);

					using (TemporarilySetTransitionPeriod(isActive: false))
					{
						previousDocuments = GetNctsPreviousDocumentCollectionForMovementTypeP5(NctsMovementType.Codes.Departure);
						AssertEquals("When Phase 5 Departure and Transition period OFF - TR0030 active, MaxCount", 99, previousDocuments.MaxCount);

						ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleTR0030Active));
						previousDocuments = GetNctsPreviousDocumentCollectionForMovementTypeP5(NctsMovementType.Codes.Departure);
						AssertEquals("When Phase 5 Departure and Transition period OFF - TR0030 inactive, MaxCount", -1, previousDocuments.MaxCount);
					}
				});

				CombineAssertions("When Rule enabled, Phase 5 Departure and Transition period ON", () =>
				{
					using (TemporarilySetTransitionPeriod(isActive: true))
					{
						var previousDocuments = GetNctsPreviousDocumentCollectionForMovementTypeP5(NctsMovementType.Codes.Departure);
						var maxCountValidator = ((ISupportMaxCountValidation)previousDocuments).MaxCountValidator;
						var notification = maxCountValidator.Notification;

						AssertEquals("MaxCount", 9, previousDocuments.MaxCount);
						AssertEquals("Notification Type", NotificationType.MessageError, notification.Type);
						AssertEquals("Notification Message", "[E1401-1] In transition period, which is now, a maximum of 9 lines can be entered.", notification.Message);
						AssertEquals("WarnAtHalfway", false, maxCountValidator.WarnAtHalfway);
					}
				});

				ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleE1401_1Active));
				using (TemporarilySetTransitionPeriod(isActive: true))
				{
					var previousDocuments = GetNctsPreviousDocumentCollectionForMovementTypeP5(NctsMovementType.Codes.Departure);
					AssertEquals("When Rule disabled, Phase 5 Departure and Transition period ON, MaxCount", -1, previousDocuments.MaxCount);
				}
			}
		}

		protected override BusinessObjectCollection GetCollectionToTest() => GetCusSupportingInfoCollection();

		protected override CusSupportingInfoCollection<NctsPreviousDocument> GetCusSupportingInfoCollection() => GetNewCollection();

		NctsPreviousDocumentCollection<NctsPreviousDocument> GetNewCollection()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var goodItem = header.Bills.AddNew().GoodsItems.AddNew();

			return new NctsPreviousDocumentCollection<NctsPreviousDocument>(goodItem);
		}

		INctsPreviousDocumentCollection<NctsPreviousDocument> GetNctsPreviousDocumentCollectionForMovementTypeP5(string movementType)
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(movementType);
			return movementType == NctsMovementType.Codes.Arrival
				? header.Bills.AddNew().ArrivalGoodsItems.AddNew().PreviousDocuments
				: header.Bills.AddNew().GoodsItems.AddNew().PreviousDocuments;
		}

		INctsPreviousDocumentCollection<NctsPreviousDocument> GetNctsPreviousDocumentCollectionForDepartureypeP4()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			return header.MovementHeader.GoodsItems.AddNew().PreviousDocuments;
		}

		IDisposable TemporarilySetTransitionPeriod(bool isActive)
			=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, isActive);
	}
}
