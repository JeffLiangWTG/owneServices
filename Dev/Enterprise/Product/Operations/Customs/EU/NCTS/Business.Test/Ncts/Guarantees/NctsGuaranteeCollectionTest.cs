using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsGuaranteeCollection<NctsGuarantee>))]
	public class NctsGuaranteeCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSetDefaultsForNewChild_NctsHeader()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.BH_HeaderType = NctsMovementType.Codes.Departure;
			var guarantee = header.Guarantees.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("PW_ParentID", header.PK, guarantee.PW_ParentID);
				AssertEquals("PW_ParentTableCode", header.TableCode, guarantee.PW_ParentTableCode);
				AssertEquals("liability", ZDecimal.Zero, guarantee.PW_BondAmount);
			});
		}

		public void TestSetDefaultsForNewChild_DepartureMovementHeader()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.BH_HeaderType = NctsMovementType.Codes.Departure;
			var movementHeader = header.MovementHeader;
			var guarantee = movementHeader.Guarantees.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("PW_ParentID", movementHeader.PK, guarantee.PW_ParentID);
				AssertEquals("PW_ParentTableCode", movementHeader.TablePrefix, guarantee.PW_ParentTableCode);
				AssertEquals("liability", ZDecimal.Zero, guarantee.PW_BondAmount);
			});
		}

		public void TestSetDefaultsForNewChild_ArrivalMovementHeader()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.BH_HeaderType = NctsMovementType.Codes.Arrival;
			var movementHeader = header.ArrivalMovementHeader;
			var guarantee = movementHeader.GuaranteesForArrival.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("PW_ParentID", movementHeader.PK, guarantee.PW_ParentID);
				AssertEquals("PW_ParentTableCode", movementHeader.TablePrefix, guarantee.PW_ParentTableCode);
				AssertEquals("liability", ZDecimal.Zero, guarantee.PW_BondAmount);
			});
		}

		public void TestMaxCountValidation_TR0023()
		{
			CombineAssertions(() =>
			{
				using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
				{
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0023Active));

					var guarantees = GetPhase5GuaranteeCollection();
					var maxCountValidator = ((ISupportMaxCountValidation)guarantees).MaxCountValidator;
					var notification = maxCountValidator.Notification;

					AssertEquals("MaxCount", 9, maxCountValidator.MaxCount);
					AssertEquals("Notification Type", NotificationType.MessageError, notification.Type);
					AssertEquals("Notification Message", "[TR0023] The maximum number of 9 Guarantees has been exceeded.", notification.Message);
					AssertEquals("WarnAtHalfway", false, maxCountValidator.WarnAtHalfway);

					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleTR0023Active));
					guarantees = GetPhase5GuaranteeCollection();
					maxCountValidator = ((ISupportMaxCountValidation)guarantees).MaxCountValidator;
					AssertEquals("MaxCount when RuleTR0023 is disabled", maxCountValidator.MaxCount, -1);
				}
			});
		}

		public void TestMaxCountValidation_TR0023_Arrival()
		{
			CombineAssertions(() =>
			{
				using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
				{
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0023Active));
					
					var guarantees = GetPhase5ArrivalGuaranteeCollection();
					var maxCountValidator = ((ISupportMaxCountValidation)guarantees).MaxCountValidator;
					var notification = maxCountValidator.Notification;

					AssertEquals("MaxCount", 1, maxCountValidator.MaxCount);
					AssertEquals("Notification Type", NotificationType.MessageError, notification.Type);
					AssertEquals("Notification Message", "[TR0023] The maximum number of 1 Guarantees has been exceeded.", notification.Message);
					AssertEquals("WarnAtHalfway", false, maxCountValidator.WarnAtHalfway);

					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleTR0023Active));
					guarantees = GetPhase5ArrivalGuaranteeCollection();
					maxCountValidator = ((ISupportMaxCountValidation)guarantees).MaxCountValidator;
					AssertEquals("MaxCount when RuleTR0023 is disabled", maxCountValidator.MaxCount, -1);
				}
			});
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			return new NctsGuaranteeCollection<NctsGuarantee>(header.MovementHeader);
		}

		INctsGuaranteeCollection<NctsGuarantee> GetPhase5GuaranteeCollection()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			return header.MovementHeader.Guarantees;
		}

		INctsGuaranteeCollection<NctsGuarantee> GetPhase5ArrivalGuaranteeCollection()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			return header.ArrivalMovementHeader.GuaranteesForArrival;
		}
	}
}
