using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using AuthorizationCodes = Enterprise.Customs.Business.CusAuthorizationHeaderTypeList.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(CusAuthorizationUsageCollection<CusAuthorizationUsage, NctsHeader>))]
	sealed class CusAuthorizationUsageCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestGetEnumerator()
		{
			AssertNotNull(authorizationUsageCollection.GetEnumerator());
		}

		public void TestSetDefaultsForNewChild()
		{
			var authorizationUsage = authorizationUsageCollection.AddNew();
			AssertEquals(CusInBondHeaderSchema.Constants.Prefix, authorizationUsage.AGC_ParentTableCode);
		}

		public void TestCheckRuleNR0002() => CombineAssertions(() =>
		{
			const string messageError = "[NR0002] Authorization Type needs to be unique.";
			using (var testContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				testContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleNR0002Active));

				var header = Factory.New<NctsHeader>();
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				header.SetMovementType(NctsMovementType.Codes.Departure);
				var departureMoveHeader = header.MovementHeader;
				var usage1 = departureMoveHeader.CusAuthorizationUsages.AddNew();
				var usage2 = departureMoveHeader.CusAuthorizationUsages.AddNew();

				usage1.AGC_Code = AuthorizationCodes.TransitReducedDataset;
				usage2.AGC_Code = AuthorizationCodes.SpecialSeals;

				AssertNoMessageError("Unique codes", usage2.AGC_CodeInfo, messageError);

				usage1.AGC_Code = AuthorizationCodes.AuthorizedConsigneeTransit;
				usage2.AGC_Code = AuthorizationCodes.AuthorizedConsigneeTransit;

				AssertHasMessageError("Duplicate codes", usage2.AGC_CodeInfo, messageError);

				testContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleNR0002Active));
				usage1 = departureMoveHeader.CusAuthorizationUsages.AddNew();
				usage2 = departureMoveHeader.CusAuthorizationUsages.AddNew();

				usage1.AGC_Code = AuthorizationCodes.AuthorizedConsigneeTransit;
				usage2.AGC_Code = AuthorizationCodes.AuthorizedConsigneeTransit;

				AssertNoMessageError("Unique codes", usage2.AGC_CodeInfo, messageError);
			}
		});

		public void TestMaxCountValidation_RuleTR0004Active()
		{
			var messageError = "[TR0004] The maximum number of 9 Authorizations has exceeded.";
			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleTR0004Active)))
			{
				var header = Factory.New<NctsHeader>();
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				header.SetMovementType(NctsMovementType.Codes.Departure);
				var collection = header.MovementHeader.CusAuthorizationUsages;
				var maxCountValidator = ((ISupportMaxCountValidation)collection).MaxCountValidator;
				var notification = maxCountValidator.Notification;

				CombineAssertions(() =>
				{
					AssertEquals("Rule active: MaxCount", 9, maxCountValidator.MaxCount);
					AssertEquals("Rule active: Notification Type", NotificationType.MessageError, notification.Type);
					AssertEquals("Rule active: Notification Message", messageError, notification.Message);
					AssertEquals("Rule active: WarnAtHalfway", false, maxCountValidator.WarnAtHalfway);
				});
			}
		}

		public void TestMaxCountValidation_RuleTR0004NotActive()
		{
			var error = "Only 9 authorizations are allowed.";
			using (ValidationRuleConfigurationTestHelper.TemporarilyInactivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleTR0004Active)))
			{
				var header = Factory.New<NctsHeader>();
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				header.SetMovementType(NctsMovementType.Codes.Departure);
				var collection = header.MovementHeader.CusAuthorizationUsages;
				var maxCountValidator = ((ISupportMaxCountValidation)collection).MaxCountValidator;
				var notification = maxCountValidator.Notification;

				CombineAssertions(() =>
				{
					AssertEquals("Rule active: MaxCount", 9, maxCountValidator.MaxCount);
					AssertEquals("Rule active: Notification Type", NotificationType.Error, notification.Type);
					AssertEquals("Rule active: Notification Message", error, notification.Message);
					AssertEquals("Rule active: WarnAtHalfway", false, maxCountValidator.WarnAtHalfway);
				});
			}
		}

		protected override Type GetExpectedCollectionType() => typeof(CusAuthorizationUsageCollection<CusAuthorizationUsage, NctsHeader>);

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var testNctsHeader = Factory.New<NctsHeader>();
			return (BusinessObjectCollection)testNctsHeader.CusAuthorizationUsages;
		}

		protected override void SetUp()
		{
			base.SetUp();
			authorizationUsageCollection = (CusAuthorizationUsageCollection<CusAuthorizationUsage, NctsHeader>)(ICusAuthorizationUsageCollection<CusAuthorizationUsage, NctsHeader>)GetCollectionToTest();
		}
		CusAuthorizationUsageCollection<CusAuthorizationUsage, NctsHeader> authorizationUsageCollection;
	}
}
