using System.ComponentModel;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using NUnit.Framework;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsBillCollection<NctsBill>))]
	sealed class NctsBillCollectionTest : ActiveBusinessObjectCollectionTestCase<INctsBillCollection<NctsBill>>
	{
		#region Bills

		public void TestMaxCountValidation_Phase5DuringTransitionPhase()
		{
			TestMaxCountValidation(1, true);
		}

		public void TestMaxCountValidation_Phase5DuringTransitionPhase_NotIncreasedWithStateDIF()
		{
			TestMaxCountValidation(1, true, NctsUnloadedStateList.Codes.DIF);
		}

		public void TestMaxCountValidation_Phase5DuringTransitionPhase_NotIncreasedWithStateDEC()
		{
			TestMaxCountValidation(1, true, NctsUnloadedStateList.Codes.DEC);
		}

		public void TestMaxCountValidation_Phase5DuringTransitionPhase_IncreasedWithStateMIS()
		{
			TestMaxCountValidation(2, true, NctsUnloadedStateList.Codes.MIS);
		}

		public void TestMaxCountValidation_Phase5AfterTransitionPhase()
		{
			TestMaxCountValidation(99, false);
		}

		public void TestMaxCountValidation_RuleE1406Inactive()
		{
			using (ValidationRuleConfigurationTestHelper.TemporarilyInactivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleE1406Active)))
			{
				var header = CreateNewHeader(NctsMovementType.Codes.Arrival);
				var collection = header.Bills;
				var bill = collection.AddNew();
				AssertNull(collection.MaxCountValidator.Notification);
			}
		}

		public void TestAllowNew_Arrival()
		{
			var header = CreateNewHeader(NctsMovementType.Codes.Arrival);
			var arrivalMovementHeader = header.ArrivalMovementHeader;
			var collection = new NctsBillCollection<NctsBill>(header);
			CombineAssertions(() =>
			{
				arrivalMovementHeader.BM_NoChangesToReport = false;
				AssertEquals("ArrivalMovementHeader.UnloadingDifferenceDataReadOnly false", expected: true, ((IBindingList)collection).AllowNew);

				arrivalMovementHeader.BM_NoChangesToReport = true;
				collection.RefreshBinding();
				AssertEquals("ArrivalMovementHeader.UnloadingDifferenceDataReadOnly true", expected: false, ((IBindingList)collection).AllowNew);
			});
		}

		public void TestAllowNew_Departure()
		{
			var collection = GetCollectionToTest();
			Assert(collection.AllowNew);
		}

		public void TestMaxCountValidationWhenDeclarationIsDeparture()
		{
			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleE1406Active)))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: true))
			{
				var header = Factory.New<NctsHeader>();
				header.SetMovementType(NctsMovementType.Codes.Departure);

				IBindingList bills = header.Bills;
				var bill = bills.AddNew() as NctsBill;
				AssertNotNull("Bill", bill);
				AssertNotNull("Bill.MovementDetail", bill.MovementDetail);

				var maxCountValidator = header.Bills.MaxCountValidator;
				var notification = maxCountValidator.Notification;

				CombineAssertions(() =>
				{
					AssertEquals("MaxCount", 1, maxCountValidator.MaxCount);
					AssertEquals("Notification Type", NotificationType.MessageError, notification.Type);
					AssertEquals("Notification Message", $"You may enter a maximum of 1 House Consignments.", notification.Message);
				});
			}
		}

		void TestMaxCountValidation(int expectedMaxCount, bool isNcts5TransitionPhase, string addBillWithState = "")
		{
			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleE1406Active)))
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, isNcts5TransitionPhase))
				{
					var header = CreateNewHeader(NctsMovementType.Codes.Arrival);
					var collection = header.Bills;
					if (!addBillWithState.IsNullOrEmpty())
					{
						var bill = collection.AddNew();
						bill.MovementDetail.B9_UnloadedState = addBillWithState;
					}

					var maxCountValidator = collection.MaxCountValidator;
					var notification = maxCountValidator.Notification;
					CombineAssertions(addBillWithState, () =>
					{
						AssertEquals("MaxCount", expectedMaxCount, maxCountValidator.MaxCount);
						AssertEquals("Notification Type", NotificationType.MessageError, notification.Type);
						AssertEquals("Notification Message", $"You may enter a maximum of {expectedMaxCount} House Consignments.", notification.Message);
						AssertEquals("WarnAtHalfway", false, maxCountValidator.WarnAtHalfway);
					});
				}
			}
		}
		#endregion

		#region Sequence Number

		public void TestISequenceNumberHeader()
		{
			var collection = GetCollectionToTest();
			AssertSame(collection, collection.Lines);
		}

		#endregion

		#region Implementation

		protected override INctsBillCollection<NctsBill> GetCollectionToTest()
		{
			var header = CreateNewHeader(NctsMovementType.Codes.Departure);
			return header.Bills;
		}

		NctsHeaderPhase5ForTest CreateNewHeader(string movementType)
		{
			var header = Factory.New<NctsHeaderPhase5ForTest>();
			header.SetMovementType(movementType);
			return header;
		}

		#endregion
	}
}
