using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Accounting.Business.ARAP.PaymentApproval.PaymentApprovalWithAuthorisation;
using static Enterprise.Core.Constants;
using AuthorisationCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;
using RangeCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing
{
	public abstract class PaymentApprovalWithAuthorisationTest : PaymentApprovalBaseTest
	{
		#region Draft Payment Approval

		public void TestHasNonDraftTransactions()
		{
			AssertEquals("Percondition", 0, TestPaymentApproval.MatchingBaseObject.BalancingAPJournals.Count);
			AssertEquals("Percondition", 0, TestPaymentApproval.MatchingBaseObject.BalancingARJournals.Count);
			AssertNull("Percondition", TestPaymentApproval.MatchingBaseObject.BankFeeCurrent);
			AssertEquals(false, TestPaymentApproval.HasNonDraftTransactions_ForTestOnly);

			TestPaymentApproval.MatchingBaseObject.BalancingAPJournals.AddNew();
			AssertEquals("Percondition", 1, TestPaymentApproval.MatchingBaseObject.BalancingAPJournals.Count);
			AssertEquals("Percondition", 0, TestPaymentApproval.MatchingBaseObject.BalancingARJournals.Count);
			AssertNull("Percondition", TestPaymentApproval.MatchingBaseObject.BankFeeCurrent);
			AssertEquals(true, TestPaymentApproval.HasNonDraftTransactions_ForTestOnly);

			TestPaymentApproval.MatchingBaseObject.BalancingAPJournals.DeleteAll();
			TestPaymentApproval.MatchingBaseObject.BalancingARJournals.AddNew();
			AssertEquals("Percondition", 0, TestPaymentApproval.MatchingBaseObject.BalancingAPJournals.Count);
			AssertEquals("Percondition", 1, TestPaymentApproval.MatchingBaseObject.BalancingARJournals.Count);
			AssertNull("Percondition", TestPaymentApproval.MatchingBaseObject.BankFeeCurrent);
			AssertEquals(true, TestPaymentApproval.HasNonDraftTransactions_ForTestOnly);

			TestPaymentApproval.MatchingBaseObject.BalancingAPJournals.DeleteAll();
			TestPaymentApproval.MatchingBaseObject.BalancingARJournals.DeleteAll();
			TestPaymentApproval.MatchingBaseObject.AddMiscellaneousTransaction(Factory.NewWithValidTestData<ARJournal>());
			AssertNotNull(TestPaymentApproval.MatchingBaseObject.BankFeeCurrent);
			AssertEquals("Percondition", 0, TestPaymentApproval.MatchingBaseObject.BalancingAPJournals.Count);
			AssertEquals("Percondition", 0, TestPaymentApproval.MatchingBaseObject.BalancingARJournals.Count);
			AssertNotNull("Percondition", TestPaymentApproval.MatchingBaseObject.BankFeeCurrent);
			AssertEquals(true, TestPaymentApproval.HasNonDraftTransactions_ForTestOnly);
		}

		public void TestCheckCanSaveAsDraft()
		{
			TestPaymentApproval.MatchingBaseObject.AddMiscellaneousTransaction(Factory.NewWithValidTestData<ARJournal>());
			AssertNotNull(TestPaymentApproval.MatchingBaseObject.BankFeeCurrent);
			AssertEquals("Percondition", false, TestPaymentApproval.IsInDatabase);
			AssertEquals("Percondition", true, TestPaymentApproval.HasNonDraftTransactions_ForTestOnly);
			AssertEquals("Matching containing Bank Fee, AR Journal or AP Journal cannot be saved in Draft mode. To save as Draft, please remove these transactions.", TestPaymentApproval.CheckCanSaveAsDraft());

			TestPaymentApproval.MatchingBaseObject.ClearCachedMiscTransactions_ForTestOnly();
			Factory.Save();
			AssertEquals("Percondition", true, TestPaymentApproval.IsInDatabase);
			AssertEquals("Percondition", false, TestPaymentApproval.IsDraft);
			AssertEquals("Percondition", false, TestPaymentApproval.HasNonDraftTransactions_ForTestOnly);
			AssertEquals($"{TestPaymentApproval.GetDescription()} can not Save as Draft since status is Fully Approved", TestPaymentApproval.CheckCanSaveAsDraft());

			TestPaymentApproval.AV_Status = PaymentApprovalStatus.Draft;
			Factory.Save();
			AssertEquals("Percondition", true, TestPaymentApproval.IsInDatabase);
			AssertEquals("Percondition", true, TestPaymentApproval.IsDraft);
			AssertEquals("Percondition", false, TestPaymentApproval.HasNonDraftTransactions_ForTestOnly);
			AssertNullOrEmpty(TestPaymentApproval.CheckCanSaveAsDraft());
		}

		public void TestIsSavingAsDraft()
		{
			Factory.SetContext(BusinessContext.SavingPaymentApprovalAsDraft);
			TestPaymentApproval.AV_Status = PaymentApprovalStatus.Draft;

			AssertEquals("Percondition", true, TestPaymentApproval.IsDraft);
			AssertEquals("Percondition", false, TestPaymentApproval.IsInDatabase);
			AssertEquals("Percondition", false, TestPaymentApproval.HasNonDraftTransactions_ForTestOnly);
			AssertEquals("Percondition", true, TestPaymentApproval.HasContext(BusinessContext.SavingPaymentApprovalAsDraft));
			AssertEquals("Valid to save draft", true, TestPaymentApproval.IsValidToSaveAsDraft);

			TestPaymentApproval.MatchingBaseObject.AddMiscellaneousTransaction(Factory.NewWithValidTestData<ARJournal>());
			AssertNotNull(TestPaymentApproval.MatchingBaseObject.BankFeeCurrent);

			AssertEquals("Percondition", true, TestPaymentApproval.IsDraft);
			AssertEquals("Percondition", false, TestPaymentApproval.IsInDatabase);
			AssertEquals("Percondition", true, TestPaymentApproval.HasNonDraftTransactions_ForTestOnly);
			AssertEquals("Percondition", true, TestPaymentApproval.HasContext(BusinessContext.SavingPaymentApprovalAsDraft));
			AssertEquals("Invalid because has non-drat-able transactions", false, TestPaymentApproval.IsValidToSaveAsDraft);

			TestPaymentApproval.MatchingBaseObject.ClearCachedMiscTransactions_ForTestOnly();
			TestPaymentApproval.AV_Status = PaymentApprovalStatus.FullyApproved;

			AssertEquals("Percondition", false, TestPaymentApproval.IsDraft);
			AssertEquals("Percondition", false, TestPaymentApproval.IsInDatabase);
			AssertEquals("Percondition", false, TestPaymentApproval.HasNonDraftTransactions_ForTestOnly);
			AssertEquals("Percondition", true, TestPaymentApproval.HasContext(BusinessContext.SavingPaymentApprovalAsDraft));
			AssertEquals("Valid because not in DB though status is not draft", true, TestPaymentApproval.IsValidToSaveAsDraft);

			Factory.RemoveContext(BusinessContext.SavingPaymentApprovalAsDraft);
			TestPaymentApproval.AV_Status = PaymentApprovalStatus.Draft;

			AssertEquals("Percondition", true, TestPaymentApproval.IsDraft);
			AssertEquals("Percondition", false, TestPaymentApproval.IsInDatabase);
			AssertEquals("Percondition", false, TestPaymentApproval.HasNonDraftTransactions_ForTestOnly);
			AssertEquals("Percondition", false, TestPaymentApproval.HasContext(BusinessContext.SavingPaymentApprovalAsDraft));
			AssertEquals("Invalid because does not have relative context", false, TestPaymentApproval.IsValidToSaveAsDraft);

			Factory.SetContext(BusinessContext.SavingPaymentApprovalAsDraft);
			TestPaymentApproval.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			Factory.Save();

			AssertEquals("Percondition", false, TestPaymentApproval.IsDraft);
			AssertEquals("Percondition", true, TestPaymentApproval.IsInDatabase);
			AssertEquals("Percondition", false, TestPaymentApproval.HasNonDraftTransactions_ForTestOnly);
			AssertEquals("Percondition", true, TestPaymentApproval.HasContext(BusinessContext.SavingPaymentApprovalAsDraft));
			AssertEquals("Invalid because already in DB and status is not draft", false, TestPaymentApproval.IsValidToSaveAsDraft);
		}

		public void TestUpdateDraftStatus()
		{
			TestPaymentApproval.AV_Status = PaymentApprovalStatus.FullyApproved;
			AssertEquals("Percondition", false, TestPaymentApproval.IsInDatabase);
			AssertEquals("Percondition", false, TestPaymentApproval.HasNonDraftTransactions_ForTestOnly);
			AssertEquals("Percondition", false, TestPaymentApproval.IsSavingPaymentApprovalAsDraft);
			TestPaymentApproval.UpdateDraftStatus();
			AssertEquals("Status not updated to Draft", true, TestPaymentApproval.IsFullyApproved);

			using (TestPaymentApproval.Factory.SetTempContext(BusinessContext.SavingPaymentApprovalAsDraft))
			{
				AssertEquals("Percondition", true, TestPaymentApproval.IsSavingPaymentApprovalAsDraft);
				TestPaymentApproval.UpdateDraftStatus();
				AssertEquals("Status updated to Draft", true, TestPaymentApproval.IsDraft);

				Factory.Save();
				AssertEquals("Percondition", true, TestPaymentApproval.IsInDatabase);
				AssertEquals("Percondition", true, TestPaymentApproval.IsDraft);
				AssertEquals("Status not updated as ", true, TestPaymentApproval.IsSavingPaymentApprovalAsDraft);
				TestPaymentApproval.UpdateDraftStatus();
				AssertEquals("Status not updated to FullyApproved", false, TestPaymentApproval.IsFullyApproved);
			}

			AssertEquals("Percondition", true, TestPaymentApproval.IsInDatabase);
			AssertEquals("Percondition", true, TestPaymentApproval.IsDraft);
			AssertEquals("Status not updated as ", false, TestPaymentApproval.IsSavingPaymentApprovalAsDraft);
			TestPaymentApproval.UpdateDraftStatus();
			AssertEquals("Status updated to FullyApproved", true, TestPaymentApproval.IsFullyApproved);

			PaymentAuthorisationSettingsCollection collection = new PaymentAuthorisationSettingsCollection();
			PaymentAuthorisationSettings newUpToSetting = collection.AddNew();
			newUpToSetting.Amount = TestPaymentApproval.AV_Amount - 1m;
			newUpToSetting.AuthorisationRequirement = AuthorisationCodes.SecondApprovalRequiredOnly;
			newUpToSetting.Range = RangeCodes.UpTo;
			PaymentAuthorisationSettings newSetting = collection.AddNew();
			newSetting.Amount = TestPaymentApproval.AV_Amount - 1m;
			newSetting.AuthorisationRequirement = AuthorisationCodes.AllThreeApprovalRequired;
			newSetting.Range = RangeCodes.Above;
			using (AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				TestPaymentApproval.AV_Status = PaymentApprovalStatus.Draft;
				TestPaymentApproval.UpdateDraftStatus();
				AssertEquals("Status updated to AwaitingApproval", true, TestPaymentApproval.IsAwaitingApproval);
			}
		}

		public void TestUpdateStatus()
		{
			TestPaymentApproval.AV_Status = PaymentApprovalStatus.Draft;
			TestPaymentApproval.AV_GS_NKApproval1st = GlbStaff.CurrentUser.GS_Code;
			AssertEquals("Status not updated", PaymentApprovalStatus.Draft, TestPaymentApproval.AV_Status);

			TestPaymentApproval.AV_Status = PaymentApprovalStatus.Posted;
			TestPaymentApproval.AV_GS_NKApproval1st = GlbStaff.CurrentUser.GS_Code;
			AssertEquals("Status not updated", PaymentApprovalStatus.Posted, TestPaymentApproval.AV_Status);

			TestPaymentApproval.AV_Status = PaymentApprovalStatus.Rejected;
			TestPaymentApproval.AV_GS_NKApproval1st = GlbStaff.CurrentUser.GS_Code;
			AssertEquals("Status not updated", PaymentApprovalStatus.Rejected, TestPaymentApproval.AV_Status);

			TestPaymentApproval.AV_Status = PaymentApprovalStatus.Cancelled;
			TestPaymentApproval.AV_GS_NKApproval1st = GlbStaff.CurrentUser.GS_Code;
			AssertEquals("Status updated", PaymentApprovalStatus.FullyApproved, TestPaymentApproval.AV_Status);
		}

		public void TestResetAuthorisationToUnapproved()
		{
			TestPaymentApproval.AV_Status = PaymentApprovalStatus.Draft;
			TestPaymentApproval.AV_PaymentDate = ZDateTime.Now;
			AssertEquals("Status not updated", PaymentApprovalStatus.Draft, TestPaymentApproval.AV_Status);

			TestPaymentApproval.AV_Status = PaymentApprovalStatus.Cancelled;
			TestPaymentApproval.AV_PaymentDate = ZDateTime.Now.AddDays(1);
			AssertEquals("Status updated", PaymentApprovalStatus.FullyApproved, TestPaymentApproval.AV_Status);
		}

		public void TestResetAuthorisationToUnapproved_Draft()
		{
			PrepareResetAuthorisationToUnapprovedTestData();
			TestPaymentApproval.AV_Status = PaymentApprovalStatus.Draft;

			TestPaymentApproval.AV_PayExRate = 0.5m;
			AssertEquals("Precondition", 3000m, TestPaymentApproval.AV_Calc_LocalAmount);
			AssertEquals("Value Change excceds 10% tolerance. Original Status is Draft. Do not update approval status.", PaymentApprovalStatus.Draft, TestPaymentApproval.AV_Status);
		}

		public void TestResetAuthorisationToUnapproved_WithinApprovalLevelAndTolerance()
		{
			PrepareResetAuthorisationToUnapprovedTestData();

			TestPaymentApproval.AV_PayExRate = 0.95m;
			AssertEquals("Precondition", 1578.95m, TestPaymentApproval.AV_Calc_LocalAmount);
			AssertEquals("Value Change within 10% tolerance, within UpTo2000 authorization. Do not update approval status.", PaymentApprovalStatus.FullyApproved, TestPaymentApproval.AV_Status);
		}

		public void TestResetAuthorisationToUnapproved_WithinApprovalLevelAndExceedTolerance()
		{
			PrepareResetAuthorisationToUnapprovedTestData();

			TestPaymentApproval.AV_PayExRate = 0.8m;
			AssertEquals("Precondition", 1875m, TestPaymentApproval.AV_Calc_LocalAmount);
			AssertEquals("Value Change exceeds 10% tolerance, within UpTo2000 authorization. Reset approval status.", PaymentApprovalStatus.AwaitingApproval, TestPaymentApproval.AV_Status);
		}

		public void TestResetAuthorisationToUnapproved_ExceedApprovalLevelAndWithinTolerance()
		{
			PrepareResetAuthorisationToUnapprovedTestData(1950);

			TestPaymentApproval.AV_PayExRate = 0.95m;
			AssertEquals("Precondition", 2052.63m, TestPaymentApproval.AV_Calc_LocalAmount);
			AssertEquals("Value Change within 10% tolerance, exceeds UpTo2000 authorization. Do not update approval status, because the operator is the authorizer.", PaymentApprovalStatus.FullyApproved, TestPaymentApproval.AV_Status);
		}

		public void TestResetAuthorisationToUnapproved_ExceedApprovalLevelAndExceedTolerance()
		{
			PrepareResetAuthorisationToUnapprovedTestData();

			TestPaymentApproval.AV_PayExRate = 0.6m;
			AssertEquals("Precondition", 2500m, TestPaymentApproval.AV_Calc_LocalAmount);
			AssertEquals("Value Change exceeds 10% tolerance, exceeds UpTo2000 authorization. Do not update approval status, because the operator is the authorizer.", PaymentApprovalStatus.FullyApproved, TestPaymentApproval.AV_Status);
		}

		public void TestResetAuthorisationToUnapproved_Draft_NonAuthorizer()
		{
			PrepareResetAuthorisationToUnapprovedTestData();
			TestPaymentApproval.AV_Status = PaymentApprovalStatus.Draft;

			var testUser = TestObjectCreator.CreateGlbStaff();
			Factory.Save();

			using (Env.SetTemporaryUserContext(testUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				TestPaymentApproval.AV_PayExRate = 0.5m;
				AssertEquals("Precondition", 3000m, TestPaymentApproval.AV_Calc_LocalAmount);
				AssertEquals("Value Change excceds 10% tolerance. Original Status is Draft. Do not update approval status.", PaymentApprovalStatus.Draft, TestPaymentApproval.AV_Status);
			}
		}

		public void TestResetAuthorisationToUnapproved_WithinApprovalLevelAndTolerance_NonAuthorizer()
		{
			PrepareResetAuthorisationToUnapprovedTestData();

			var testUser = TestObjectCreator.CreateGlbStaff();
			Factory.Save();

			using (Env.SetTemporaryUserContext(testUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				TestPaymentApproval.AV_PayExRate = 0.95m;
				AssertEquals("Precondition", 1578.95m, TestPaymentApproval.AV_Calc_LocalAmount);
				AssertEquals("Value Change within 10% tolerance, within UpTo2000 authorization. Do not update approval status.", PaymentApprovalStatus.FullyApproved, TestPaymentApproval.AV_Status);
			}
		}

		public void TestResetAuthorisationToUnapproved_WithinApprovalLevelAndExceedTolerance_NonAuthorizer()
		{
			PrepareResetAuthorisationToUnapprovedTestData();

			var testUser = TestObjectCreator.CreateGlbStaff();
			Factory.Save();

			using (Env.SetTemporaryUserContext(testUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				TestPaymentApproval.AV_PayExRate = 0.8m;
				AssertEquals("Precondition", 1875m, TestPaymentApproval.AV_Calc_LocalAmount);
				AssertEquals("Value Change exceeds 10% tolerance, within UpTo2000 authorization. Reset approval status.", PaymentApprovalStatus.AwaitingApproval, TestPaymentApproval.AV_Status);
			}
		}

		public void TestResetAuthorisationToUnapproved_ExceedApprovalLevelAndWithinTolerance_NonAuthorizer()
		{
			PrepareResetAuthorisationToUnapprovedTestData(1950);

			var testUser = TestObjectCreator.CreateGlbStaff();
			Factory.Save();

			using (Env.SetTemporaryUserContext(testUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				TestPaymentApproval.AV_PayExRate = 0.95m;
				AssertEquals("Precondition", 2052.63m, TestPaymentApproval.AV_Calc_LocalAmount);
				AssertEquals("Value Change within 10% tolerance, exceeds UpTo2000 authorization. Reset approval status, because the operator is NOT the original authorizer.", PaymentApprovalStatus.AwaitingApproval, TestPaymentApproval.AV_Status);
			}
		}

		public void TestResetAuthorisationToUnapproved_ExceedApprovalLevelAndExceedTolerance_NonAuthorizer()
		{
			PrepareResetAuthorisationToUnapprovedTestData();

			var testUser = TestObjectCreator.CreateGlbStaff();
			Factory.Save();

			using (Env.SetTemporaryUserContext(testUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				TestPaymentApproval.AV_PayExRate = 0.6m;
				AssertEquals("Precondition", 2500m, TestPaymentApproval.AV_Calc_LocalAmount);
				AssertEquals("Value Change exceeds 10% tolerance, exceeds UpTo2000 authorization. Reset approval status, because the operator is NOT the original authorizer.", PaymentApprovalStatus.AwaitingApproval, TestPaymentApproval.AV_Status);
			}
		}

		void PrepareResetAuthorisationToUnapprovedTestData(decimal paymentAmount = 1500m)
		{
			SetUpRegistryForTest();

			var config = new ExchangeRateToleranceConfiguration();
			config.ExchangeRateToleranceCollection.RemoveAndDeleteAll();
			var newExchnageRateTolerance = new ExchangeRateTolerance { Currency = CurrencyCodes.Australia, ExchangeRateTolerancePercentage = 10M };
			config.ExchangeRateToleranceCollection.Add(newExchnageRateTolerance);
			AccountingConfigurationRegistry.Instance.ExchangeRateTolerance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, config);

			TestPaymentApproval.AV_Amount = paymentAmount;
			TestPaymentApproval.AV_PayExRate = 1m;
			ResetApprovalUsers(true);
			Factory.Save();

			AssertEquals("Precondition", PaymentApprovalStatus.FullyApproved, TestPaymentApproval.GetApprovalStatus());
		}

		#endregion

		#region Test Required Authorisation Changed Event

		public void TestRequiredAuthorisationChanged()
		{
			try
			{
				SetUpRegistryForTest();
				TestPaymentApproval.AV_Amount = 100M;
				TestPaymentApproval.RequiredAuthorisationChanged += new RequiredAuthorisationChangedHandler(TestPaymentApproval_RequiredAuthorisationChanged);

				AssertAuthorisationRequiredDisplayForAmount(1, UpTo1000);
				AssertAuthorisationRequiredDisplayForAmount(500, UpTo1000);
				AssertAuthorisationRequiredDisplayForAmount(1000, UpTo1000);

				AssertAuthorisationRequiredDisplayForAmount(1001, UpTo2000);
				AssertAuthorisationRequiredDisplayForAmount(1500, UpTo2000);
				AssertAuthorisationRequiredDisplayForAmount(2000, UpTo2000);

				AssertAuthorisationRequiredDisplayForAmount(2001, UpTo3000);
				AssertAuthorisationRequiredDisplayForAmount(2500, UpTo3000);
				AssertAuthorisationRequiredDisplayForAmount(3000, UpTo3000);

				AssertAuthorisationRequiredDisplayForAmount(3001, UpTo4000);
				AssertAuthorisationRequiredDisplayForAmount(3500, UpTo4000);
				AssertAuthorisationRequiredDisplayForAmount(4000, UpTo4000);

				AssertAuthorisationRequiredDisplayForAmount(4001, UpTo5000);
				AssertAuthorisationRequiredDisplayForAmount(4500, UpTo5000);
				AssertAuthorisationRequiredDisplayForAmount(5000, UpTo5000);

				AssertAuthorisationRequiredDisplayForAmount(5001, UpTo6000);
				AssertAuthorisationRequiredDisplayForAmount(5500, UpTo6000);
				AssertAuthorisationRequiredDisplayForAmount(6000, UpTo6000);

				AssertAuthorisationRequiredDisplayForAmount(6001, UpTo7000);
				AssertAuthorisationRequiredDisplayForAmount(6500, UpTo7000);
				AssertAuthorisationRequiredDisplayForAmount(7000, UpTo7000);

				AssertAuthorisationRequiredDisplayForAmount(7001, Over7000);
				AssertAuthorisationRequiredDisplayForAmount(7500, Over7000);
				AssertAuthorisationRequiredDisplayForAmount(8000, Over7000);

				Expected = TestPaymentApproval.PostsOnSave ? 0 : 25;

				AssertEquals("RequiredAuthorisationChangedEvent should have been called " + Expected + " times ", Expected, NumberOfTimesEventWasCalledDuringTest);
			}
			finally
			{
				ResetRegistryForTest();
			}
		}

		void TestPaymentApproval_RequiredAuthorisationChanged(object sender, string message)
		{
			NumberOfTimesEventWasCalledDuringTest++;

			AssertEquals("Display for Required Authorisation", ExpectedEventMessageForTest,
				TestPaymentApproval.AV_Calc_DescriptionOfAuthorisationRequiredCore_ForTestOnly);
		}

		void AssertAuthorisationRequiredDisplayForAmount(ZDecimal amount, PaymentAuthorisationSettings setting)
		{
			ExpectedEventMessageForTest = TestPaymentApproval.GetDescriptionForDisplay_ForTestOnly(setting);
			TestPaymentApproval.AV_Amount = amount;
		}

		#endregion

		#region Test First Approval Status Changed Event

		public void TestFirstApprovalStatusChangedEvent()
		{
			TestPaymentApproval.FirstApprovalStatusChanged += new FirstApprovalStatusChangedHandler(TestPaymentApproval_FirstApprovalStatusChanged);

			TestPaymentApproval.AV_GS_NKApproval1st = GlbStaff.CurrentUser.GS_Code;
			TestPaymentApproval.AV_GS_NKApproval1st = ZString.Empty;
			TestPaymentApproval.AV_GS_NKApproval1st = GlbStaff.CurrentUser.GS_Code;

			int expected = TestPaymentApproval.PostsOnSave ? 0 : 3;
			AssertEquals("FirstApprovalStatusChangedEvent should have been called " + expected + " times", expected, NumberOfTimesEventWasCalledDuringTest);
		}

		void TestPaymentApproval_FirstApprovalStatusChanged(object sender, string message)
		{
			NumberOfTimesEventWasCalledDuringTest++;
		}

		#endregion

		#region Test Second Approval Status Changed Event

		public void TestSecondApprovalStatusChanged()
		{
			TestPaymentApproval.SecondApprovalStatusChanged += new SecondApprovalStatusChangedHandler(TestPaymentApproval_SecondApprovalStatusChanged);

			TestPaymentApproval.AV_GS_NKApproval2nd = GlbStaff.CurrentUser.GS_Code;
			TestPaymentApproval.AV_GS_NKApproval2nd = ZString.Empty;
			TestPaymentApproval.AV_GS_NKApproval2nd = GlbStaff.CurrentUser.GS_Code;

			int expected = TestPaymentApproval.PostsOnSave ? 0 : 3;
			AssertEquals("SecondApprovalStatusChangedEvent should have been called " + expected + " times", expected, NumberOfTimesEventWasCalledDuringTest);
		}

		void TestPaymentApproval_SecondApprovalStatusChanged(object sender, string message)
		{
			NumberOfTimesEventWasCalledDuringTest++;
		}

		#endregion

		#region Test Third Approval Status Changed Event

		public void TestThirdApprovalStatusChanged()
		{
			TestPaymentApproval.ThirdApprovalStatusChanged += new ThirdApprovalStatusChangedHandler(TestPaymentApproval_ThirdApprovalStatusChanged);

			TestPaymentApproval.AV_GS_NKApproval3rd = GlbStaff.CurrentUser.GS_Code;
			TestPaymentApproval.AV_GS_NKApproval3rd = ZString.Empty;
			TestPaymentApproval.AV_GS_NKApproval3rd = GlbStaff.CurrentUser.GS_Code;

			int expected = TestPaymentApproval.PostsOnSave ? 0 : 3;
			AssertEquals("ThirdApprovalStatusChangedEvent should have been called " + expected + " times", expected, NumberOfTimesEventWasCalledDuringTest);
		}

		void TestPaymentApproval_ThirdApprovalStatusChanged(object sender, string message)
		{
			NumberOfTimesEventWasCalledDuringTest++;
		}

		#endregion

		#region Test First Approval Logs

		public void TestCreateFirstApprovalLogs()
		{
			string developerLogin = GlbStaff.CurrentUser.GS_LoginName;
			string developerCode = GlbStaff.CurrentUser.GS_Code;
			GlbStaff developer = GlbStaff.CurrentUser;

			string testUserLogin = "UserForTest";
			string testUserCode = "TU";
			GlbStaff testUser = Factory.NewWithValidTestData<GlbStaff>();
			testUser.GS_Code = testUserCode;
			testUser.GS_LoginName = testUserLogin;

			Guid branchPK = GlbBranch.CurrentBranch.PK.ToGuid();
			Guid departmentPK = GlbDepartment.CurrentDepartment.PK.ToGuid();
			IDisposable userContextChange = null;

			try
			{
				TestPaymentApproval.AV_GS_NKApproval1st = GlbStaff.CurrentUser.GS_Code;
				Factory.Save();
				ZQuery query = QueryToLoadEvents(AutoEvents.Authorised, TestPaymentApproval);
				StmALog[] logs = Factory.Load<StmALog>(query);
				AssertEquals("Logs Length", 1, logs.Length);
				AssertNotNull("Action Authorised Log", logs[0]);
				AssertLogIsCreatedCorrectly(logs[0], TestPaymentApproval, AutoEvents.Authorised, PaymentApprovalWithAuthorisation.FirstAuthorisationLogText, developer);

				userContextChange = Env.SetTemporaryUserContext(testUserLogin, branchPK, departmentPK);

				TestPaymentApproval.AV_GS_NKApproval1st = ZString.Empty;
				Factory.Save();
				query = QueryToLoadEvents(AutoEvents.AuthorisationWithdrawn, TestPaymentApproval);
				logs = Factory.Load<StmALog>(query);
				AssertEquals("Logs Length", 1, logs.Length);
				AssertNotNull("Authorisation Withdrawn Log", logs[0]);
				AssertLogIsCreatedCorrectly(logs[0], TestPaymentApproval, AutoEvents.AuthorisationWithdrawn, PaymentApprovalWithAuthorisation.FirstAuthorisationWithdrawnLogText, testUser);

				TestPaymentApproval.AV_GS_NKApproval1st = GlbStaff.CurrentUser.GS_Code;
				Factory.Save();
				query = QueryToLoadEvents(AutoEvents.Authorised, TestPaymentApproval);
				logs = Factory.Load<StmALog>(query);
				AssertEquals("Logs Length", 2, logs.Length);
				AssertNotNull("Log", logs[0]);
				AssertNotNull("Log", logs[1]);
				int indexOfNewApprovedLog = (logs[0].SL_GS_NKUser == testUserCode) ? 0 : 1;
				AssertLogIsCreatedCorrectly(logs[indexOfNewApprovedLog], TestPaymentApproval, AutoEvents.Authorised, PaymentApprovalWithAuthorisation.FirstAuthorisationLogText, testUser);
			}
			finally
			{
				if (userContextChange != null)
				{
					userContextChange.Dispose();
				}
			}
		}

		#endregion

		#region Test Second Approval Logs

		public void TestCreateSecondApprovalLogs()
		{
			string developerLogin = GlbStaff.CurrentUser.GS_LoginName;
			string developerCode = GlbStaff.CurrentUser.GS_Code;
			GlbStaff developer = GlbStaff.CurrentUser;

			string testUserLogin = "UserForTest";
			string testUserCode = "TU";
			GlbStaff testUser = Factory.NewWithValidTestData<GlbStaff>();
			testUser.GS_Code = testUserCode;
			testUser.GS_LoginName = testUserLogin;

			Guid branchPK = GlbBranch.CurrentBranch.PK.ToGuid();
			Guid departmentPK = GlbDepartment.CurrentDepartment.PK.ToGuid();
			IDisposable userContextChange = null;

			try
			{
				TestPaymentApproval.AV_GS_NKApproval2nd = GlbStaff.CurrentUser.GS_Code;
				Factory.Save();
				ZQuery query = QueryToLoadEvents(AutoEvents.Authorised, TestPaymentApproval);
				StmALog[] logs = Factory.Load<StmALog>(query);
				AssertEquals("Logs Length", 1, logs.Length);
				AssertNotNull("Action Authorised Log", logs[0]);
				AssertLogIsCreatedCorrectly(logs[0], TestPaymentApproval, AutoEvents.Authorised, PaymentApprovalWithAuthorisation.SecondAuthorisationLogText, developer);

				userContextChange = Env.SetTemporaryUserContext(testUserLogin, branchPK, departmentPK);

				TestPaymentApproval.AV_GS_NKApproval2nd = ZString.Empty;
				Factory.Save();
				query = QueryToLoadEvents(AutoEvents.AuthorisationWithdrawn, TestPaymentApproval);
				logs = Factory.Load<StmALog>(query);
				AssertEquals("Logs Length", 1, logs.Length);
				AssertNotNull("Authorisation Withdrawn Log", logs[0]);
				AssertLogIsCreatedCorrectly(logs[0], TestPaymentApproval, AutoEvents.AuthorisationWithdrawn, PaymentApprovalWithAuthorisation.SecondAuthorisationWithdrawnLogText, testUser);

				TestPaymentApproval.AV_GS_NKApproval2nd = GlbStaff.CurrentUser.GS_Code;
				Factory.Save();
				query = QueryToLoadEvents(AutoEvents.Authorised, TestPaymentApproval);
				logs = Factory.Load<StmALog>(query);
				AssertEquals("Logs Length", 2, logs.Length);
				AssertNotNull("Log", logs[0]);
				AssertNotNull("Log", logs[1]);
				int indexOfNewApprovedLog = (logs[0].SL_GS_NKUser == testUserCode) ? 0 : 1;
				AssertLogIsCreatedCorrectly(logs[indexOfNewApprovedLog], TestPaymentApproval, AutoEvents.Authorised, PaymentApprovalWithAuthorisation.SecondAuthorisationLogText, testUser);
			}
			finally
			{
				if (userContextChange != null)
				{
					userContextChange.Dispose();
				}
			}
		}

		#endregion

		#region Test Third Approval Logs

		public void TestCreateThirdApprovalLogs()
		{
			string developerLogin = GlbStaff.CurrentUser.GS_LoginName;
			string developerCode = GlbStaff.CurrentUser.GS_Code;
			GlbStaff developer = GlbStaff.CurrentUser;

			string testUserLogin = "UserForTest";
			string testUserCode = "TU";
			GlbStaff testUser = Factory.NewWithValidTestData<GlbStaff>();
			testUser.GS_Code = testUserCode;
			testUser.GS_LoginName = testUserLogin;

			Guid branchPK = GlbBranch.CurrentBranch.PK.ToGuid();
			Guid departmentPK = GlbDepartment.CurrentDepartment.PK.ToGuid();
			IDisposable userContextChange = null;

			try
			{
				TestPaymentApproval.AV_GS_NKApproval3rd = GlbStaff.CurrentUser.GS_Code;
				Factory.Save();
				ZQuery query = QueryToLoadEvents(AutoEvents.Authorised, TestPaymentApproval);
				StmALog[] logs = Factory.Load<StmALog>(query);
				AssertEquals("Logs Length", 1, logs.Length);
				AssertNotNull("Action Authorised Log", logs[0]);
				AssertLogIsCreatedCorrectly(logs[0], TestPaymentApproval, AutoEvents.Authorised, PaymentApprovalWithAuthorisation.ThirdAuthorisationLogText, developer);

				userContextChange = Env.SetTemporaryUserContext(testUserLogin, branchPK, departmentPK);

				TestPaymentApproval.AV_GS_NKApproval3rd = ZString.Empty;
				Factory.Save();
				query = QueryToLoadEvents(AutoEvents.AuthorisationWithdrawn, TestPaymentApproval);
				logs = Factory.Load<StmALog>(query);
				AssertEquals("Logs Length", 1, logs.Length);
				AssertNotNull("Authorisation Withdrawn Log", logs[0]);
				AssertLogIsCreatedCorrectly(logs[0], TestPaymentApproval, AutoEvents.AuthorisationWithdrawn, PaymentApprovalWithAuthorisation.ThirdAuthorisationWithdrawnLogText, testUser);

				TestPaymentApproval.AV_GS_NKApproval3rd = GlbStaff.CurrentUser.GS_Code;
				Factory.Save();
				query = QueryToLoadEvents(AutoEvents.Authorised, TestPaymentApproval);
				logs = Factory.Load<StmALog>(query);
				AssertEquals("Logs Length", 2, logs.Length);
				AssertNotNull("Log", logs[0]);

				AssertNotNull("Log", logs[1]);
				int indexOfNewApprovedLog = (logs[0].SL_GS_NKUser == testUserCode) ? 0 : 1;
				AssertLogIsCreatedCorrectly(logs[indexOfNewApprovedLog], TestPaymentApproval, AutoEvents.Authorised, PaymentApprovalWithAuthorisation.ThirdAuthorisationLogText, testUser);
			}
			finally
			{
				if (userContextChange != null)
				{
					userContextChange.Dispose();
				}
			}
		}

		#endregion

		#region Test Create Payment Posted Log

		public override void TestCreatePaymentPostedLog()
		{
			TestPaymentApproval.AV_Status = PaymentApprovalStatus.Posted;
			Factory.Save();

			StmALog[] logs = Factory.Load<StmALog>(QueryToLoadEvents(AutoEvents.TransactionPosted, TestPaymentApproval));
			AssertEquals("Logs Length", 1, logs.Length);
			AssertNotNull("Log", logs[0]);
			AssertLogIsCreatedCorrectly(logs[0], TestPaymentApproval, AutoEvents.TransactionPosted, TestPaymentApproval.UserPostedFullyApprovedTransactionLogText_ForTestOnly);
		}

		#endregion

		#region Test Create Payment Cancel Log

		public void TestCancelPaymentLog()
		{
			TestPaymentApproval.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			Factory.Save();

			var buffer = new NotificationBuffer();
			TestPaymentApproval.TryCancelPayment(buffer);
			Assert("Successful cancel should not have any errors", !buffer.HasErrors);
			AssertEquals(PaymentApprovalStatus.Cancelled, TestPaymentApproval.AV_Status);
			Factory.Save();

			var query = QueryToLoadEvents(AutoEvents.EditedARecord, TestPaymentApproval);
			query.AddToFilter(StmALogSchema.SL_Reference, "Payment Cancelled");
			var logs = Factory.Load<StmALog>(query);
			AssertEquals("Logs Length", 1, logs.Length);
			AssertNotNull("Log", logs[0]);
			AssertLogIsCreatedCorrectly(logs[0], TestPaymentApproval, AutoEvents.EditedARecord, PaymentCancelledLogText);
		}

		#endregion

		#region Test Edit Log

		public void TestCreateEditLog()
		{
			Factory.Save();
			StmALog[] editLogs = TestPaymentApproval.Logs.Find(QueryToLoadEvents(AutoEvents.EditedARecord, TestPaymentApproval));
			AssertEquals("No Edit Logs", 0, editLogs.Length);

			TestPaymentApproval.AV_AB = TestObjectCreator.AUDBankAccount2.PK;
			Assert("HasChanges", TestPaymentApproval.HasChanges);
			Factory.Save();

			editLogs = TestPaymentApproval.Logs.Find(QueryToLoadEvents(AutoEvents.EditedARecord, TestPaymentApproval));
			Assert("Should be Edit Logs", editLogs.Length > 0);
			StmALog editLog = editLogs[0];
			AssertNotNull("Edit Log", editLog);
			AssertLogIsCreatedCorrectly(editLog, TestPaymentApproval, AutoEvents.EditedARecord, ZString.Empty);
		}

		#endregion

		protected override bool IsCurrencyReadOnly { get { return false; } }

		public void TestApproveFirstApproval()
		{
			SetUpRegistryForTest();

			GlbStaff newStaff = Factory.New<GlbStaff>();
			newStaff.GS_Code = "XYZ";

			PaymentApprovalWithAuthorisation approval1 = GetNewPaymentApproval();
			PaymentApprovalWithAuthorisation approval2 = GetNewPaymentApproval();
			approval1.AV_Amount = 8000m;
			approval2.AV_Amount = 8000m;
			approval1.AV_GS_NKApproval1st = newStaff.GS_Code;

			FirstApprovalCheckPoint.IsAllowed = false;
			approval1.ApproveFirstApproval();
			approval2.ApproveFirstApproval();

			AssertEquals("approval1.FirstApproval", newStaff.GS_Code, approval1.AV_GS_NKApproval1st);
			AssertEquals("approval2.FirstApproval", ZString.Empty, approval2.AV_GS_NKApproval1st);

			FirstApprovalCheckPoint.IsAllowed = true;
			approval1.ApproveFirstApproval();
			approval2.ApproveFirstApproval();

			AssertEquals("approval1.FirstApproval", newStaff.GS_Code, approval1.AV_GS_NKApproval1st);
			AssertEquals("approval2.FirstApproval", GlbStaff.CurrentUser.GS_Code, approval2.AV_GS_NKApproval1st);
		}

		public void TestApproveSecondApproval()
		{
			SetUpRegistryForTest();

			GlbStaff newStaff = Factory.New<GlbStaff>();
			newStaff.GS_Code = "XYZ";
			PaymentApprovalWithAuthorisation approval1 = GetNewPaymentApproval();
			PaymentApprovalWithAuthorisation approval2 = GetNewPaymentApproval();
			approval1.AV_Amount = 8000m;
			approval2.AV_Amount = 8000m;
			approval1.AV_GS_NKApproval2nd = newStaff.GS_Code;

			SecondApprovalCheckPoint.IsAllowed = false;
			approval1.ApproveSecondApproval();
			approval2.ApproveSecondApproval();

			AssertEquals("approval1.SecondApproval", newStaff.GS_Code, approval1.AV_GS_NKApproval2nd);
			AssertEquals("approval2.SecondApproval", ZString.Empty, approval2.AV_GS_NKApproval2nd);

			SecondApprovalCheckPoint.IsAllowed = true;
			approval1.ApproveSecondApproval();
			approval2.ApproveSecondApproval();

			AssertEquals("approval1.SecondApproval", newStaff.GS_Code, approval1.AV_GS_NKApproval2nd);
			AssertEquals("approval2.SecondApproval", GlbStaff.CurrentUser.GS_Code, approval2.AV_GS_NKApproval2nd);
		}

		public void TestApproveThirdApproval()
		{
			SetUpRegistryForTest();

			GlbStaff newStaff = Factory.New<GlbStaff>();
			newStaff.GS_Code = "XYZ";

			PaymentApprovalWithAuthorisation approval1 = GetNewPaymentApproval();
			PaymentApprovalWithAuthorisation approval2 = GetNewPaymentApproval();
			approval1.AV_Amount = 8000m;
			approval2.AV_Amount = 8000m;
			approval1.AV_GS_NKApproval3rd = newStaff.GS_Code;

			ThirdApprovalCheckPoint.IsAllowed = false;
			approval1.ApproveThirdApproval();
			approval2.ApproveThirdApproval();

			AssertEquals("approval1.ThirdApproval", newStaff.GS_Code, approval1.AV_GS_NKApproval3rd);
			AssertEquals("approval2.ThirdApproval", ZString.Empty, approval2.AV_GS_NKApproval3rd);

			ThirdApprovalCheckPoint.IsAllowed = true;
			approval1.ApproveThirdApproval();
			approval2.ApproveThirdApproval();

			AssertEquals("approval1.ThirdApproval", newStaff.GS_Code, approval1.AV_GS_NKApproval3rd);
			AssertEquals("approval2.ThirdApproval", GlbStaff.CurrentUser.GS_Code, approval2.AV_GS_NKApproval3rd);
		}

		#region Authorise

		public void TestTryAuthorisePayment_AWA() => AssertAuthoriseSuccessfully(PaymentApprovalStatus.AwaitingApproval);
		void AssertAuthoriseSuccessfully(string status)
		{
			var newStaff = Factory.New<GlbStaff>();
			newStaff.GS_Code = "XYZ";
			TestPaymentApproval.AV_Status = status;

			var buffer = new NotificationBuffer();
			TestPaymentApproval.TryAuthorisePayment(buffer);
			Assert("Successful Authorise should not have any errors", !buffer.HasErrors);
			AssertEquals(PaymentApprovalStatus.FullyApproved, TestPaymentApproval.AV_Status);
		}

		public void TestTryAuthorisePayment_APP() => AssertAuthoriseUnsuccessfully(PaymentApprovalStatus.FullyApproved, "This Payment is already Fully Approved");
		public void TestTryAuthorisePayment_REJ() => AssertAuthoriseUnsuccessfully(PaymentApprovalStatus.Rejected, "This Payment is already Rejected");
		public void TestTryAuthorisePayment_PST() => AssertAuthoriseUnsuccessfully(PaymentApprovalStatus.Posted, "This Payment is already Posted");
		public void TestTryAuthorisePayment_CAN() => AssertAuthoriseUnsuccessfully(PaymentApprovalStatus.Cancelled, "This Payment is already Canceled");

		void AssertAuthoriseUnsuccessfully(ZString status, string expectedErrorMessage)
		{
			var newStaff = Factory.New<GlbStaff>();
			newStaff.GS_Code = "XYZ";
			SetUpAsFullyAuthorisedForTests(TestPaymentApproval, newStaff);
			TestPaymentApproval.AV_Status = status;

			var buffer = new NotificationBuffer();
			TestPaymentApproval.TryAuthorisePayment(buffer);
			Assert("Unsuccessful Authorise should add an error", buffer.HasErrors);
			AssertEquals(expectedErrorMessage, buffer.AsString.Trim());
			AssertEquals("Should be no status change if unsuccessful", status, TestPaymentApproval.AV_Status);
		}

		#endregion

		#region Unauthorise

		public void TestTryUnauthorisePayment_AWA() => AssertUnauthoriseSuccessfully(PaymentApprovalStatus.AwaitingApproval);
		public void TestTryUnauthorisePayment_APP_AuthRequired() => AssertUnauthoriseSuccessfully(PaymentApprovalStatus.FullyApproved);
		void AssertUnauthoriseSuccessfully(string status)
		{
			var newStaff = Factory.New<GlbStaff>();
			newStaff.GS_Code = "XYZ";
			if (status == PaymentApprovalStatus.FullyApproved)
			{
				SetUpAsFullyAuthorisedForTests(TestPaymentApproval, newStaff);

				var valuesForTest = new PaymentAuthorisationSettingsCollection();
				MakeNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 1000, AuthorisationCodes.NoApprovalRequired);
				MakeNewAuthorisationSetting(valuesForTest, RangeCodes.Above, 1000, AuthorisationCodes.AllThreeApprovalRequired);

				AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			}
			TestPaymentApproval.AV_Status = status;

			var buffer = new NotificationBuffer();
			TestPaymentApproval.TryUnauthorisePayment(buffer);
			Assert("Successful Unauthorise should not have any errors", !buffer.HasErrors);
			AssertEquals(PaymentApprovalStatus.AwaitingApproval, TestPaymentApproval.AV_Status);
		}

		protected void MakeNewAuthorisationSetting(PaymentAuthorisationSettingsCollection collection,
			ZString range, ZInt amount, ZString requirement)
		{
			PaymentAuthorisationSettings newSetting = collection.AddNew();
			newSetting.Amount = (ZDecimal)amount;
			newSetting.AuthorisationRequirement = requirement;
			newSetting.Range = range;
		}

		public void TestTryUnauthorisePayment_APP_NoAuthRequired() => AssertUnauthoriseUnsuccessfully(PaymentApprovalStatus.FullyApproved, "This Payment requires no authorization level");
		public void TestTryUnauthorisePayment_REJ() => AssertUnauthoriseUnsuccessfully(PaymentApprovalStatus.Rejected, "This Payment is already Rejected");
		public void TestTryUnauthorisePayment_PST() => AssertUnauthoriseUnsuccessfully(PaymentApprovalStatus.Posted, "This Payment is already Posted");
		public void TestTryUnauthorisePayment_CAN() => AssertUnauthoriseUnsuccessfully(PaymentApprovalStatus.Cancelled, "This Payment is already Canceled");

		void AssertUnauthoriseUnsuccessfully(ZString status, string expectedErrorMessage)
		{
			var newStaff = Factory.New<GlbStaff>();
			newStaff.GS_Code = "XYZ";
			SetUpAsFullyAuthorisedForTests(TestPaymentApproval, newStaff);
			TestPaymentApproval.AV_Amount = 1M;
			TestPaymentApproval.AV_Status = status;

			var buffer = new NotificationBuffer();
			TestPaymentApproval.TryUnauthorisePayment(buffer);
			Assert("Unsuccessful Unauthorise should add an error", buffer.HasErrors);
			AssertEquals(expectedErrorMessage, buffer.AsString.Trim());
			AssertEquals("Should be no status change if unsuccessful", status, TestPaymentApproval.AV_Status);
		}

		#endregion

		#region Reject

		public void TestTryRejectPayment_AWA() => AssertRejectsSuccessfully(PaymentApprovalStatus.AwaitingApproval);
		public void TestTryRejectPayment_APP() => AssertRejectsSuccessfully(PaymentApprovalStatus.FullyApproved);

		void AssertRejectsSuccessfully(string status)
		{
			var newStaff = Factory.New<GlbStaff>();
			newStaff.GS_Code = "XYZ";
			if (status == PaymentApprovalStatus.FullyApproved)
			{
				SetUpAsFullyAuthorisedForTests(TestPaymentApproval, newStaff);
			}
			TestPaymentApproval.AV_Status = status;

			var buffer = new NotificationBuffer();
			TestPaymentApproval.TryRejectPayment(buffer);
			Assert("Successful rejection should not have any errors", !buffer.HasErrors);
			AssertEquals(PaymentApprovalStatus.Rejected, TestPaymentApproval.AV_Status);

			AssertEquals(GlbStaff.CurrentUser.GS_Code, TestPaymentApproval.AV_GS_NKApproval1st);
			AssertEquals("Rejected", TestPaymentApproval.Level1AuthorisationStatus);
			AssertEquals(ZString.Empty, TestPaymentApproval.AV_GS_NKApproval2nd);
			AssertEquals("Not Required", TestPaymentApproval.Level2AuthorisationStatus);
			AssertEquals(ZString.Empty, TestPaymentApproval.AV_GS_NKApproval3rd);
			AssertEquals("Not Required", TestPaymentApproval.Level3AuthorisationStatus);
		}

		public void TestTryRejectPayment_PST() => AssertRejectsUnsuccessfully(PaymentApprovalStatus.Posted, "This Payment is already Posted");
		public void TestTryRejectPayment_REJ() => AssertRejectsUnsuccessfully(PaymentApprovalStatus.Rejected, "This Payment is already Rejected");
		public void TestTryRejectPayment_CAN() => AssertRejectsUnsuccessfully(PaymentApprovalStatus.Cancelled, "This Payment is already Canceled");

		void AssertRejectsUnsuccessfully(ZString status, string expectedErrorMessage)
		{
			var newStaff = Factory.New<GlbStaff>();
			newStaff.GS_Code = "XYZ";
			SetUpAsFullyAuthorisedForTests(TestPaymentApproval, newStaff);
			TestPaymentApproval.AV_Status = status;

			var buffer = new NotificationBuffer();
			TestPaymentApproval.TryRejectPayment(buffer);
			Assert("Unsuccessful rejection should add an error", buffer.HasErrors);
			AssertEquals(expectedErrorMessage, buffer.AsString.Trim());
			AssertEquals("Should be no status change if unsuccessful", status, TestPaymentApproval.AV_Status);

			AssertEquals("Should be no auth user change if unsuccessful", newStaff.GS_Code, TestPaymentApproval.AV_GS_NKApproval1st);
			AssertEquals("Should be no auth user change if unsuccessful", newStaff.GS_Code, TestPaymentApproval.AV_GS_NKApproval2nd);
			AssertEquals("Should be no auth user change if unsuccessful", newStaff.GS_Code, TestPaymentApproval.AV_GS_NKApproval3rd);
		}

		public void TestRejectReasonRemovedWhenRequired()
		{
			var newStaff = Factory.New<GlbStaff>();
			newStaff.GS_Code = "XYZ";
			foreach (var status in new[] { PaymentApprovalStatus.AwaitingApproval, PaymentApprovalStatus.FullyApproved, PaymentApprovalStatus.Posted, PaymentApprovalStatus.Rejected })
			{
				SetUpAsFullyAuthorisedForTests(TestPaymentApproval, newStaff);
				TestPaymentApproval.AV_RejectionReasonCode = "ABC";
				TestPaymentApproval.AV_RejectionReasonDetails = "String";

				TestPaymentApproval.AV_Status = status;
				AssertNotNullOrEmpty(TestPaymentApproval.AV_RejectionReasonCode);
				AssertNotNullOrEmpty(TestPaymentApproval.AV_RejectionReasonDetails);

				TestPaymentApproval.AV_Status = PaymentApprovalStatus.Rejected;
				TestPaymentApproval.AV_Status = status;
				AssertReasonResetIfRejected(status);

				TestPaymentApproval.AV_RejectionReasonCode = "ABC";
				TestPaymentApproval.AV_RejectionReasonDetails = "String";
				Factory.Save();
				AssertReasonResetIfRejected(status);
			}
		}

		void AssertReasonResetIfRejected(ZString status)
		{
			if (status == PaymentApprovalStatus.Rejected)
			{
				AssertNotNullOrEmpty(TestPaymentApproval.AV_RejectionReasonCode);
				AssertNotNullOrEmpty(TestPaymentApproval.AV_RejectionReasonDetails);
			}
			else
			{
				AssertNullOrEmpty(TestPaymentApproval.AV_RejectionReasonCode);
				AssertNullOrEmpty(TestPaymentApproval.AV_RejectionReasonDetails);
			}
		}

		public void TestRejectPaymentSecurity()
		{
			FirstApprovalCheckPoint.IsAllowed = false;
			SecondApprovalCheckPoint.IsAllowed = false;
			ThirdApprovalCheckPoint.IsAllowed = false;

			SetUpRegistryForTest();
			var authUser = TestObjectCreator.CreateStaff("NEW");
			Factory.Save();

			var expectedSecurityMessage = "You do not have sufficient rights to reject this payment. Required authorization level: Level 1 and Level 2 and Level 3.";

			var approval = newReadyForReject();
			var buffer = new NotificationBuffer();
			approval.TryRejectPayment(buffer);
			Assert("Any user can reject an approval with no authorisation", !buffer.HasErrors);

			approval = newReadyForReject();
			approval.AV_GS_NKApproval1st = authUser.GS_Code;

			approval.TryRejectPayment(buffer);
			Assert("Only a user with level 1 authority can reject a level 1 approval", buffer.HasErrors);
			AssertEquals("Only a user with level 1 authority can reject a level 1 approval", expectedSecurityMessage, buffer.AsString.Trim());
			buffer.Clear();

			FirstApprovalCheckPoint.IsAllowed = true;
			approval.TryRejectPayment(buffer);
			Assert("A user with level 1 authority can reject a level 1 approval", !buffer.HasErrors);

			approval = newReadyForReject();
			approval.AV_GS_NKApproval1st = authUser.GS_Code;
			approval.AV_GS_NKApproval2nd = authUser.GS_Code;

			approval.TryRejectPayment(buffer);
			Assert("Only a user with level 2 authority can reject a level 2 approval", buffer.HasErrors);
			AssertEquals("Only a user with level 2 authority can reject a level 2 approval", expectedSecurityMessage, buffer.AsString.Trim());
			buffer.Clear();

			SecondApprovalCheckPoint.IsAllowed = true;
			approval.TryRejectPayment(buffer);
			Assert("A user with level 2 authority can reject a level 2 approval", !buffer.HasErrors);

			approval = newReadyForReject();
			approval.AV_GS_NKApproval1st = authUser.GS_Code;
			approval.AV_GS_NKApproval2nd = authUser.GS_Code;
			approval.AV_GS_NKApproval3rd = authUser.GS_Code;

			approval.TryRejectPayment(buffer);
			Assert("Only a user with level 3 authority can reject a level 3 approval", buffer.HasErrors);
			AssertEquals("Only a user with level 3 authority can reject a level 3 approval", expectedSecurityMessage, buffer.AsString.Trim());
			buffer.Clear();

			ThirdApprovalCheckPoint.IsAllowed = true;
			approval.TryRejectPayment(buffer);
			Assert("A user with level 3 authority can reject a level 3 approval", !buffer.HasErrors);

			PaymentApprovalWithAuthorisation newReadyForReject()
			{
				var newApproval = GetNewPaymentApproval();
				newApproval.AV_Amount = 7001M;
				newApproval.AV_RejectionReasonCode = newApproval.RejectionReasonCodesList.GetAllCodes().First();
				return newApproval;
			}
		}

		public void TestRejectDetailsFilledAutomatically()
		{
			var reasonCodes = new CodeDescriptionPairList();
			reasonCodes.AddPairIfNotExist("XYZ", "Some Description");
			AccountingMasterFilesRegistry.Instance.PaymentRejectionReasonCodesList.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, reasonCodes);

			var approval = GetNewPaymentApproval();
			approval.AV_RejectionReasonCode = "AAA";
			approval.AV_RejectionReasonDetails = "bbb";

			approval.AV_RejectionReasonCode = "AAA";
			AssertEquals("Details is only defaulted when there is a change in code value", "bbb", approval.AV_RejectionReasonDetails);

			approval.AV_RejectionReasonCode = "CCC";
			AssertEquals("Details is defaulted to empty when there is no description associated with the reason code", ZString.Empty, approval.AV_RejectionReasonDetails);

			approval.AV_RejectionReasonCode = "XYZ";
			AssertEquals("Details is defaulted to the description associated with the reason code", "Some Description", approval.AV_RejectionReasonDetails);
		}

		#endregion

		#region Cancel

		public void TestTryCancelPayment_AWA() => AssertCancelSuccessfully(PaymentApprovalStatus.AwaitingApproval);
		public void TestTryCancelPayment_APP() => AssertCancelSuccessfully(PaymentApprovalStatus.FullyApproved);
		public void TestTryCancelPayment_REJ() => AssertCancelSuccessfully(PaymentApprovalStatus.Rejected);

		void AssertCancelSuccessfully(string status)
		{
			var newStaff = Factory.New<GlbStaff>();
			newStaff.GS_Code = "XYZ";
			if (status == PaymentApprovalStatus.FullyApproved)
			{
				SetUpAsFullyAuthorisedForTests(TestPaymentApproval, newStaff);
			}
			TestPaymentApproval.AV_Status = status;
			Assert("Precondition", !TestPaymentApproval.ReadOnly);

			var buffer = new NotificationBuffer();
			TestPaymentApproval.TryCancelPayment(buffer);
			Assert("Successful cancel should not have any errors", !buffer.HasErrors);
			AssertEquals(PaymentApprovalStatus.Cancelled, TestPaymentApproval.AV_Status);
			Assert(TestPaymentApproval.ReadOnly);
		}

		public void TestTryCancelPayment_PST() => AssertCancelUnsuccessfully(PaymentApprovalStatus.Posted, "This Payment is already Posted");
		public void TestTryCancelPayment_CAN() => AssertCancelUnsuccessfully(PaymentApprovalStatus.Cancelled, "This Payment is already Canceled");

		void AssertCancelUnsuccessfully(ZString status, string expectedErrorMessage)
		{
			var newStaff = Factory.New<GlbStaff>();
			newStaff.GS_Code = "XYZ";
			SetUpAsFullyAuthorisedForTests(TestPaymentApproval, newStaff);
			TestPaymentApproval.AV_Status = status;

			var buffer = new NotificationBuffer();
			TestPaymentApproval.TryCancelPayment(buffer);
			Assert("Unsuccessful cancel should add an error", buffer.HasErrors);
			AssertEquals(expectedErrorMessage, buffer.AsString.Trim());
			AssertEquals("Should be no status change if unsuccessful", status, TestPaymentApproval.AV_Status);
		}

		#endregion

		#region Cancel E-Payment

		public void TestCancelEPaymentSecurity()
		{
			var expectedCancelEPaymentCode = CancelEPaymentCheckPoint.DisplayTextPathToSecurityRight;
			var cancelEPaymentCode = TestPaymentApproval.CancelEPaymentSecurityCheckPoint_ForTestOnly.DisplayTextPathToSecurityRight;

			AssertEquals("Cancel E-Payment", expectedCancelEPaymentCode, cancelEPaymentCode);
		}

		public void TestIsCancelEPaymentPossible()
		{
			var deal = TestObjectCreator.CreateValidEPaymentDealForStatus(EPaymentStatusCodes.Deal.Queued);
			Factory.Save();

			var paymentApprovalWithDeal = Factory.Load<PaymentApprovalWithAuthorisation>(deal.Quote.PaymentApproval.PK);
			AssertNotNull(paymentApprovalWithDeal.CurrentDeal);
			AssertEquals(deal.PK, paymentApprovalWithDeal.CurrentDeal.PK);

			AssertEquals(EPaymentStatusCodes.Deal.Queued, deal.AED_Status);
			Assert(!paymentApprovalWithDeal.IsCancelEPaymentPossible);

			deal.AED_Status = EPaymentStatusCodes.Deal.Pending;
			Assert(!paymentApprovalWithDeal.IsCancelEPaymentPossible);

			deal.AED_Status = EPaymentStatusCodes.Deal.ReadyToSend;
			Assert(!paymentApprovalWithDeal.IsCancelEPaymentPossible);

			deal.AED_Status = EPaymentStatusCodes.Deal.Requested;
			Assert(!paymentApprovalWithDeal.IsCancelEPaymentPossible);

			deal.AED_Status = EPaymentStatusCodes.Deal.Accepted;
			Assert(paymentApprovalWithDeal.IsCancelEPaymentPossible);

			deal.AED_Status = EPaymentStatusCodes.Deal.InProgress;
			Assert(paymentApprovalWithDeal.IsCancelEPaymentPossible);

			deal.AED_Status = EPaymentStatusCodes.Deal.Paid;
			Assert(!paymentApprovalWithDeal.IsCancelEPaymentPossible);

			deal.AED_Status = EPaymentStatusCodes.Deal.SubmissionFailed;
			Assert(!paymentApprovalWithDeal.IsCancelEPaymentPossible);

			deal.AED_Status = EPaymentStatusCodes.Deal.Cancelled;
			Assert(!paymentApprovalWithDeal.IsCancelEPaymentPossible);

			deal.AED_Status = EPaymentStatusCodes.Deal.Declined;
			Assert(!paymentApprovalWithDeal.IsCancelEPaymentPossible);

			deal.AED_Status = EPaymentStatusCodes.Deal.Failed;
			Assert(!paymentApprovalWithDeal.IsCancelEPaymentPossible);

			var paymentApprovalWithoutDeal = GetNewPaymentApproval();
			AssertNull(paymentApprovalWithoutDeal.CurrentDeal);
			Assert(!paymentApprovalWithoutDeal.IsCancelEPaymentPossible);
		}

		#endregion

		public void TestChangingDetailsResetsAuthorisation_Rejected()
		{
			Action<PaymentApprovalWithAuthorisation> additionalApprovalSetup = (PaymentApprovalWithAuthorisation approval) =>
			{
				FirstApprovalCheckPoint.IsAllowed = true;
				SecondApprovalCheckPoint.IsAllowed = true;
				ThirdApprovalCheckPoint.IsAllowed = true;

				var buffer = new NotificationBuffer();
				approval.TryRejectPayment(buffer);
				Assert(!buffer.HasErrors);
			};

			Action<PaymentApprovalWithAuthorisation> additionalAssertion = (PaymentApprovalWithAuthorisation approval) =>
			{
				AssertNull(approval.Approval1st);
				AssertNull(approval.Approval2nd);
				AssertNull(approval.Approval3rd);
			};

			AssertChangingDetailsResetsAuthorisation(PaymentApprovalStatus.AwaitingApproval, additionalApprovalSetup, additionalAssertion);
		}

		public void TestChangingDetailsResetsAuthorisation_ApprovedByCurrentUser_ApprovedByOtherUser()
		{
			Action<PaymentApprovalWithAuthorisation> additionalAssertion = (PaymentApprovalWithAuthorisation approval) =>
			{
				AssertNull(approval.Approval1st);
				AssertNull(approval.Approval2nd);
				AssertNull(approval.Approval3rd);
			};

			AssertChangingDetailsResetsAuthorisation(PaymentApprovalStatus.AwaitingApproval, (PaymentApprovalWithAuthorisation approval) => { }, additionalAssertion);
		}

		public void TestChangingDetailsResetsAuthorisation_ApprovedByCurrentUser_ApprovedByUser()
		{
			Action<PaymentApprovalWithAuthorisation> additionalApprovalSetup = (PaymentApprovalWithAuthorisation approval) =>
			{
				approval.AV_GS_NKApproval1st = GlbStaff.CurrentUser.GS_Code;
				approval.AV_GS_NKApproval2nd = GlbStaff.CurrentUser.GS_Code;
				approval.AV_GS_NKApproval3rd = GlbStaff.CurrentUser.GS_Code;

				FirstApprovalCheckPoint.IsAllowed = true;
				SecondApprovalCheckPoint.IsAllowed = true;
				ThirdApprovalCheckPoint.IsAllowed = true;
			};

			Action<PaymentApprovalWithAuthorisation> additionalAssertion = (PaymentApprovalWithAuthorisation approval) =>
			{
				AssertEquals(GlbStaff.CurrentUser.GS_Code, approval.AV_GS_NKApproval1st);
				AssertEquals(GlbStaff.CurrentUser.GS_Code, approval.AV_GS_NKApproval2nd);
				AssertEquals(GlbStaff.CurrentUser.GS_Code, approval.AV_GS_NKApproval3rd);
			};

			AssertChangingDetailsResetsAuthorisation(PaymentApprovalStatus.FullyApproved, additionalApprovalSetup, additionalAssertion);
		}

		void AssertChangingDetailsResetsAuthorisation(string excpectedStatus, Action<PaymentApprovalWithAuthorisation> additionalApprovalSetup, Action<PaymentApprovalWithAuthorisation> additionalAssertion)
		{
			Invoice invoice = Factory.New<APInvoice>();
			invoice.AH_OSExTaxAmount = 100M;

			if (TestPaymentApproval.PostsOnSave)
			{
				Assert("This functionality is not used by Posts OnSave", true);
			}
			else
			{
				SetUpRegistryForTest();
				GlbStaff newUser = Factory.New<GlbStaff>();

				// PaymentDate
				SetUpAsFullyAuthorisedForTests(TestPaymentApproval, newUser);
				additionalApprovalSetup(TestPaymentApproval);
				TestPaymentApproval.AV_PaymentDate = ZDateTime.Now;
				AssertEquals("Approval Status", excpectedStatus, TestPaymentApproval.AV_Status);
				additionalAssertion(TestPaymentApproval);

				// PostDate
				SetUpAsFullyAuthorisedForTests(TestPaymentApproval, newUser);
				additionalApprovalSetup(TestPaymentApproval);
				TestPaymentApproval.AV_PostDate = ZDateTime.Now;
				AssertEquals("Approval Status", excpectedStatus, TestPaymentApproval.AV_Status);
				additionalAssertion(TestPaymentApproval);

				// OrgHeader
				SetUpAsFullyAuthorisedForTests(TestPaymentApproval, newUser);
				additionalApprovalSetup(TestPaymentApproval);
				TestPaymentApproval.AV_OH = TestOrgHeader2.PK;
				AssertEquals("Approval Status", excpectedStatus, TestPaymentApproval.AV_Status);
				additionalAssertion(TestPaymentApproval);

				// PaymentType
				SetUpAsFullyAuthorisedForTests(TestPaymentApproval, newUser);
				additionalApprovalSetup(TestPaymentApproval);
				TestPaymentApproval.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
				AssertEquals("Approval Status", excpectedStatus, TestPaymentApproval.AV_Status);
				additionalAssertion(TestPaymentApproval);

				// BankAccount
				SetUpAsFullyAuthorisedForTests(TestPaymentApproval, newUser);
				additionalApprovalSetup(TestPaymentApproval);
				TestPaymentApproval.AV_AB = TestObjectCreator.USDBankAccount.PK;
				AssertEquals("Approval Status", excpectedStatus, TestPaymentApproval.AV_Status);
				additionalAssertion(TestPaymentApproval);
				TestPaymentApproval.AV_AK = TestObjectCreator.AUDChequeBook.PK;

				// Cheque Book
				SetUpAsFullyAuthorisedForTests(TestPaymentApproval, newUser);
				additionalApprovalSetup(TestPaymentApproval);
				TestPaymentApproval.AV_AK = TestObjectCreator.USDChequeBook.PK;
				AssertEquals("Approval Status", excpectedStatus, TestPaymentApproval.AV_Status);
				additionalAssertion(TestPaymentApproval);
				TestPaymentApproval.AV_ChequeOrReference = TestObjectCreator.GetRandomString(6);

				// Changing Cheque / Reference should NOT reset authorisation
				SetUpAsFullyAuthorisedForTests(TestPaymentApproval, newUser);
				additionalApprovalSetup(TestPaymentApproval);
				var originalStatus = TestPaymentApproval.AV_Status;
				TestPaymentApproval.AV_ChequeOrReference = TestObjectCreator.GetRandomString(6);
				AssertEquals("Approval Status", originalStatus, TestPaymentApproval.AV_Status);

				// Exchange Rate
				SetUpAsFullyAuthorisedForTests(TestPaymentApproval, newUser);
				additionalApprovalSetup(TestPaymentApproval);
				TestPaymentApproval.AV_PayExRate = 0.74M;
				AssertEquals("Approval Status", excpectedStatus, TestPaymentApproval.AV_Status);
				additionalAssertion(TestPaymentApproval);

				// Amount
				SetUpAsFullyAuthorisedForTests(TestPaymentApproval, newUser);
				additionalApprovalSetup(TestPaymentApproval);
				TestPaymentApproval.AV_Amount = 7005M;
				AssertEquals("Approval Status", excpectedStatus, TestPaymentApproval.AV_Status);
				additionalAssertion(TestPaymentApproval);

				// Matched Transactions
				SetUpAsFullyAuthorisedForTests(TestPaymentApproval, newUser);
				additionalApprovalSetup(TestPaymentApproval);
				TestPaymentApproval.PaymentMatchingBaseObject.UnmatchedTransactions.Add(invoice);
				TestPaymentApproval.PaymentMatchingBaseObject.MoveFromUnmatchToMatch(new BusinessObject[] { invoice });
				AssertEquals("Approval Status", excpectedStatus, TestPaymentApproval.AV_Status);
				additionalAssertion(TestPaymentApproval);
			}
		}

		public void TestRemovalOfPaymentApprovalItems()
		{
			if (TestPaymentApproval.PostsOnSave)
			{
				Assert("Does not Apply to PostOnSave", true);
			}
			else
			{
				ZDecimal invoice1OSAmount = 600m;
				ZDecimal invoice1ExchangeRate = 1m;
				ZDecimal invoice1LocalAmount = 600m;
				RefCurrency invoice1Currency = TestObjectCreator.AUD;

				ZDecimal invoice2OSAmount = 1000m;
				ZDecimal invoice2ExchangeRate = 1m;
				ZDecimal invoice2LocalAmount = 1000m;
				RefCurrency invoice2Currency = TestObjectCreator.AUD;

				ZDecimal approvalOSAmount = 800m;
				ZDecimal approvalExchangeRate = 1m;
				ZDecimal approvalLocalAmount = 800m;
				RefCurrency approvalCurrency = TestObjectCreator.AUD;

				BusinessObjectFactory factoryToSave = new BusinessObjectFactory();
				TestObjectCreator otherCreator = new TestObjectCreator(factoryToSave);

				APInvoice invoice1 = factoryToSave.New<APInvoice>();
				invoice1.AH_OH = TestOrgHeader.PK;
				invoice1.AH_TransactionNum = "00001100";
				invoice1.AH_RX_NKTransactionCurrency = invoice1Currency.RX_Code;
				invoice1.AH_ExchangeRate = invoice1ExchangeRate;
				otherCreator.CreateInvoiceLine(invoice1, invoice1Currency, invoice1ExchangeRate, invoice1OSAmount, 0m, 0m);
				AssertEquals("Precondition: OS Invoice Amount", invoice1OSAmount, invoice1.AH_OSExTaxAmount);
				AssertEquals("Precondition: Invoice Exchange Rate", invoice1ExchangeRate, invoice1.AH_ExchangeRate);
				AssertEquals("Precondition: Local Invoice Amount", invoice1LocalAmount, invoice1.AH_LocalExTaxAmount);

				APInvoice invoice2 = factoryToSave.New<APInvoice>();
				invoice2.AH_OH = TestOrgHeader.PK;
				invoice2.AH_TransactionNum = "00001101";
				invoice2.AH_RX_NKTransactionCurrency = invoice2Currency.RX_Code;
				invoice2.AH_ExchangeRate = invoice2ExchangeRate;
				otherCreator.CreateInvoiceLine(invoice2, invoice2Currency, invoice2ExchangeRate, invoice2OSAmount, 0m, 0m);
				AssertEquals("Precondition: OS Invoice Amount", invoice2OSAmount, invoice2.AH_OSExTaxAmount);
				AssertEquals("Precondition: Invoice Exchange Rate", invoice2ExchangeRate, invoice2.AH_ExchangeRate);
				AssertEquals("Precondition: Local Invoice Amount", invoice2LocalAmount, invoice2.AH_LocalExTaxAmount);
				factoryToSave.Save();

				TestPaymentApproval.AV_OH = ZGuid.Empty;
				TestPaymentApproval.AV_OH = TestOrgHeader.PK;
				TestPaymentApproval.AV_AB = TestObjectCreator.AUDBankAccount.PK;
				TestPaymentApproval.AV_AK = TestObjectCreator.AUDChequeBook.PK;
				TestPaymentApproval.AV_ChequeOrReference = TestObjectCreator.AUDChequeBook.AK_CurrentNo.ToString();
				TestPaymentApproval.AV_Amount = approvalOSAmount;

				AssertEquals("Precondition: Payment Approval OS Amount", approvalOSAmount, TestPaymentApproval.AV_Amount);
				AssertEquals("Precondition: Payment Approval Exchange Rate", approvalExchangeRate, TestPaymentApproval.AV_PayExRate);
				AssertEquals("Precondition: Payment Approval OS Amount", approvalLocalAmount, TestPaymentApproval.AV_Calc_LocalAmount);

				TestPaymentApproval.PaymentMatchingBaseObject.MoveAllFromUnmatchToMatch();
				invoice1 = Factory.Load<APInvoice>(invoice1.PK);
				invoice2 = Factory.Load<APInvoice>(invoice2.PK);
				((IMatching)invoice1).OSPartialPaymentAmount = -300;
				((IMatching)invoice2).OSPartialPaymentAmount = -500;

				AssertEquals("Matched Transactions Count", 3, TestPaymentApproval.PaymentMatchingBaseObject.MatchedTransactions.Count);
				AssertEquals("Payment Matching Session Balances To Zero", true, TestPaymentApproval.PaymentMatchingBaseObject.SessionBalancesToZero);

				TestPaymentApproval.PaymentMatchingBaseObject.MatchAndClearTransactions(); // Called by Gui
				Factory.Save();

				PaymentApprovalItemCollection approvalItems = new PaymentApprovalItemCollection(TestPaymentApproval);
				approvalItems.Load();
				AssertEquals("ApprovalItems Count", 2, approvalItems.Count);

				BusinessObjectFactory newFactory = new BusinessObjectFactory();

				PaymentApprovalBase reloadedApproval = newFactory.Load<PaymentApprovalBase>(TestPaymentApproval.PK);
				TransactionHeader reloadedInvoice1 = newFactory.Load<TransactionHeader>(invoice1.PK);
				TransactionHeader reloadedInvoice2 = newFactory.Load<TransactionHeader>(invoice2.PK);

				AssertEquals("Matched Transactions Count", 3, reloadedApproval.PaymentMatchingBaseObject.MatchedTransactions.Count);
				reloadedApproval.PaymentMatchingBaseObject.MoveFromMatchToUnmatch(new BusinessObject[] { reloadedInvoice1 });
				((IMatching)reloadedInvoice1).OSPartialPaymentAmount = 0m;
				((IMatching)reloadedInvoice2).OSPartialPaymentAmount = -800m;

				AssertEquals("Matched Transactions Count", 2, reloadedApproval.PaymentMatchingBaseObject.MatchedTransactions.Count);
				AssertEquals("Payment Matching Session Balance", 0m, reloadedApproval.PaymentMatchingBaseObject.Balance);
				AssertEquals("Payment Matching Session Balances To Zero", true, reloadedApproval.PaymentMatchingBaseObject.SessionBalancesToZero);
				reloadedApproval.PaymentMatchingBaseObject.MatchAndClearTransactions(); // Called by Gui
				newFactory.Save();

				approvalItems = new PaymentApprovalItemCollection(TestPaymentApproval);
				approvalItems.Load();
				AssertEquals("ApprovalItems Count", 1, approvalItems.Count);
				AssertEquals("Approval Item Header", reloadedInvoice2.PK, approvalItems[0].A2_AH);
				AssertEquals("Approval Item Pay This Run", -800m, approvalItems[0].A2_PaymentThisRun);
			}
		}

		public void TestCancelPaymentApprovalRemovesLinksToTransactions()
		{
			APInvoice invoice1 = Factory.New<APInvoice>();
			invoice1.AH_OH = TestOrgHeader.PK;
			invoice1.AH_TransactionNum = "00001001";
			invoice1.AH_OSExTaxAmount = 150M;

			APInvoice invoice2 = Factory.New<APInvoice>();
			invoice2.AH_OH = TestOrgHeader.PK;
			invoice2.AH_TransactionNum = "00001002";
			invoice2.AH_OSExTaxAmount = 150M;

			APInvoice invoice3 = Factory.New<APInvoice>();
			invoice3.AH_OH = TestOrgHeader.PK;
			invoice3.AH_TransactionNum = "00001003";
			invoice3.AH_OSExTaxAmount = 150M;

			TestPaymentApproval.AV_Amount = 300M;

			AssertEquals("Payment Approval OS Partial Payment Amount", 300M, ((IMatching)TestPaymentApproval).OSPartialPaymentAmount);

			InvoicingBaseCollection invoices = new InvoicingBaseCollection(Factory);
			invoices.AddRange(new BusinessObject[] { invoice1, invoice2, invoice3 });
			AssertEquals(3, invoices.Count);

			TestPaymentApproval.PaymentMatchingBaseObject.MoveFromUnmatchToMatch(invoices.ToArray());

			((IMatching)invoice1).OSPartialPaymentAmount = -100M;
			AssertEquals("Invoice 1 OS Partial Payment Amount", -100M, ((IMatching)invoice1).OSPartialPaymentAmount);
			((IMatching)invoice2).OSPartialPaymentAmount = -100M;
			AssertEquals("Invoice 2 OS Partial Payment Amount", -100M, ((IMatching)invoice2).OSPartialPaymentAmount);
			((IMatching)invoice3).OSPartialPaymentAmount = -100M;
			AssertEquals("Invoice 3 OS Partial Payment Amount", -100M, ((IMatching)invoice3).OSPartialPaymentAmount);

			AssertEquals("Session Balance", 0m, TestPaymentApproval.PaymentMatchingBaseObject.Balance);
			AssertEquals("SessionBalancesToZero", true, TestPaymentApproval.PaymentMatchingBaseObject.SessionBalancesToZero);
			AssertEquals("Should be matchable", true, TestPaymentApproval.PaymentMatchingBaseObject.MatchAndClearTransactions());

			PaymentApprovalItemCollection approvalItems = new PaymentApprovalItemCollection(TestPaymentApproval);
			approvalItems.Load();
			AssertEquals("Expect approval Items Count", 3, approvalItems.Count);

			AssertEquals("PaymentApprovalBase Item was created for one of the invoices", true, invoices.Contains(approvalItems[0].A2_AH));
			AssertEquals("PaymentApprovalBase This Run", -100M, approvalItems[0].A2_PaymentThisRun);

			AssertEquals("PaymentApprovalBase Item was created for one of the invoices", true, invoices.Contains(approvalItems[1].A2_AH));
			AssertEquals("PaymentApprovalBase This Run", -100M, approvalItems[1].A2_PaymentThisRun);

			AssertEquals("PaymentApprovalBase Item was created for one of the invoices", true, invoices.Contains(approvalItems[2].A2_AH));
			AssertEquals("PaymentApprovalBase This Run", -100M, approvalItems[2].A2_PaymentThisRun);

			Factory.Save();

			var buffer = new NotificationBuffer();
			TestPaymentApproval.TryCancelPayment(buffer);
			Factory.Save();
			Assert("should not have any errors", !buffer.HasErrors);
			AssertEquals(PaymentApprovalStatus.Cancelled, TestPaymentApproval.AV_Status);

			approvalItems = new PaymentApprovalItemCollection(TestPaymentApproval);
			approvalItems.Load();
			AssertEquals("Expect no approval Items once cancelled", 0, approvalItems.Count);
		}

		public void TestGetApprovalStatus()
		{
			SetUpRegistryForTest();

			TestPaymentApproval.AV_PayExRate = 1m;

			TestPaymentApproval.AV_Amount = 500m;
			ResetApprovalUsers();
			AssertCorrectAuthorisationRequired(TestPaymentApproval.AV_Amount, false, false, false, UpTo1000);
			AssertEquals("No approval needed", PaymentApprovalStatus.FullyApproved, TestPaymentApproval.GetApprovalStatus());

			TestPaymentApproval.AV_Amount = 1500m;
			ResetApprovalUsers();
			AssertCorrectAuthorisationRequired(TestPaymentApproval.AV_Amount, true, false, false, UpTo2000);
			AssertEquals("Need level 1 approval but not granted", PaymentApprovalStatus.AwaitingApproval, TestPaymentApproval.GetApprovalStatus());

			TestPaymentApproval.AV_GS_NKApproval1st = GlbStaff.CurrentUser.GS_Code;
			AssertEquals("Need level 1 approval and granted", PaymentApprovalStatus.FullyApproved, TestPaymentApproval.GetApprovalStatus());

			TestPaymentApproval.AV_Amount = 2500m;
			ResetApprovalUsers();
			AssertCorrectAuthorisationRequired(TestPaymentApproval.AV_Amount, false, true, false, UpTo3000);
			AssertEquals("Need level 2 approval but not granted", PaymentApprovalStatus.AwaitingApproval, TestPaymentApproval.GetApprovalStatus());

			TestPaymentApproval.AV_GS_NKApproval2nd = GlbStaff.CurrentUser.GS_Code;
			AssertEquals("Need level 2 approval and granted", PaymentApprovalStatus.FullyApproved, TestPaymentApproval.GetApprovalStatus());

			TestPaymentApproval.AV_Amount = 3500m;
			ResetApprovalUsers();
			AssertCorrectAuthorisationRequired(TestPaymentApproval.AV_Amount, false, false, true, UpTo4000);
			AssertEquals("Need level 3 approval but not granted", PaymentApprovalStatus.AwaitingApproval, TestPaymentApproval.GetApprovalStatus());

			TestPaymentApproval.AV_GS_NKApproval3rd = GlbStaff.CurrentUser.GS_Code;
			AssertEquals("Need level 3 approval and granted", PaymentApprovalStatus.FullyApproved, TestPaymentApproval.GetApprovalStatus());

			TestPaymentApproval.AV_Amount = 6500m;
			ResetApprovalUsers();
			AssertCorrectAuthorisationRequired(TestPaymentApproval.AV_Amount, false, true, true, UpTo7000);
			AssertEquals("Need level 2 and 3 approvals but not granted", PaymentApprovalStatus.AwaitingApproval, TestPaymentApproval.GetApprovalStatus());

			TestPaymentApproval.AV_GS_NKApproval2nd = GlbStaff.CurrentUser.GS_Code;
			TestPaymentApproval.AV_GS_NKApproval3rd = GlbStaff.CurrentUser.GS_Code;
			AssertEquals("Need level 2 and 3 approvals and granted", PaymentApprovalStatus.FullyApproved, TestPaymentApproval.GetApprovalStatus());
		}

		public void TestResetAuthorisationToUnapproved_ExchangeRateTolerance()
		{
			SetUpRegistryForTest();

			var emptyUser = string.Empty;
			var nonEmptyUser = GlbStaff.CurrentUser.GS_Code;

			var config = new ExchangeRateToleranceConfiguration();
			config.ExchangeRateToleranceCollection.RemoveAndDeleteAll();
			var newExchnageRateTolerance = new ExchangeRateTolerance
			{
				Currency = CurrencyCodes.Australia,
				ExchangeRateTolerancePercentage = 10M
			};
			config.ExchangeRateToleranceCollection.Add(newExchnageRateTolerance);

			TestPaymentApproval.AV_Amount = 7000m;
			TestPaymentApproval.AV_PayExRate = 1m;
			ResetApprovalUsers(true);
			Factory.Save();
			TestPaymentApproval.AV_Amount = 7500m;
			AssertEquals(7500m, TestPaymentApproval.AV_Calc_LocalAmount);
			AssertEquals(true, TestPaymentApproval.AV_AmountInfo.HasChanges);
			AssertCorrectAuthorisationRequired(TestPaymentApproval.AV_Amount, true, true, true, Over7000);
			AssertApprovalUsers("ShouldCheckExRateTolerance : false, IsExRateToleranceEnabled : false, IsAuthorisationRequiredLevelUnchanged : true, no ex-rate change", nonEmptyUser, nonEmptyUser, nonEmptyUser);

			TestPaymentApproval.AV_PayExRate = 0.95m;
			AssertEquals(7894.74m, TestPaymentApproval.AV_Calc_LocalAmount);
			AssertApprovalUsers("ShouldCheckExRateTolerance : false, IsExRateToleranceEnabled : false, IsAuthorisationRequiredLevelUnchanged : true, ex-rate change is with in 10%", nonEmptyUser, nonEmptyUser, nonEmptyUser);

			TestPaymentApproval.AV_PayExRate = 0.85m;
			AssertEquals(8823.53m, TestPaymentApproval.AV_Calc_LocalAmount);
			AssertApprovalUsers("ShouldCheckExRateTolerance : false, IsExRateToleranceEnabled : false, IsAuthorisationRequiredLevelUnchanged : true, ex-rate change exceeds 10%", nonEmptyUser, nonEmptyUser, nonEmptyUser);

			TestPaymentApproval.AV_PayExRate = 2m;
			AssertEquals(3750m, TestPaymentApproval.AV_Calc_LocalAmount);
			AssertCorrectAuthorisationRequired(TestPaymentApproval.AV_Amount, false, false, true, UpTo4000);
			AssertApprovalUsers("ShouldCheckExRateTolerance : false, IsExRateToleranceEnabled : false, IsAuthorisationRequiredLevelUnchanged : false, regardless of ex-rate change", emptyUser, emptyUser, nonEmptyUser);

			AccountingConfigurationRegistry.Instance.ExchangeRateTolerance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, config);

			TestPaymentApproval.AV_Amount = 7000m;
			TestPaymentApproval.AV_PayExRate = 1m;
			ResetApprovalUsers(true);
			Factory.Save();
			TestPaymentApproval.AV_Amount = 7500m;
			AssertCorrectAuthorisationRequired(TestPaymentApproval.AV_Amount, true, true, true, Over7000);
			AssertEquals(7500m, TestPaymentApproval.AV_Calc_LocalAmount);
			AssertEquals("ShouldCheckExRateTolerance will be false when have OS amount changes", true, TestPaymentApproval.AV_AmountInfo.HasChanges);
			AssertApprovalUsers("ShouldCheckExRateTolerance : false, IsExRateToleranceEnabled : true, IsAuthorisationRequiredLevelUnchanged : true, no Local Amount change", nonEmptyUser, nonEmptyUser, nonEmptyUser);

			TestPaymentApproval.AV_PayExRate = 0.95m;
			AssertEquals(7894.74m, TestPaymentApproval.AV_Calc_LocalAmount);
			AssertApprovalUsers("ShouldCheckExRateTolerance : false, IsExRateToleranceEnabled : true, IsAuthorisationRequiredLevelUnchanged : true, Value Changed is with in 10%", nonEmptyUser, nonEmptyUser, nonEmptyUser);

			TestPaymentApproval.AV_PayExRate = 0.85m;
			AssertEquals(8823.53m, TestPaymentApproval.AV_Calc_LocalAmount);
			AssertApprovalUsers("ShouldCheckExRateTolerance : false, IsExRateToleranceEnabled : true, IsAuthorisationRequiredLevelUnchanged : true, Value Changed exceeds 10%", nonEmptyUser, nonEmptyUser, nonEmptyUser);

			TestPaymentApproval.AV_PayExRate = 2m;
			AssertEquals("Authorisation Required Level changed", 3750m, TestPaymentApproval.AV_Calc_LocalAmount);
			AssertCorrectAuthorisationRequired(TestPaymentApproval.AV_Amount, false, false, true, UpTo4000);
			AssertApprovalUsers("ShouldCheckExRateTolerance : false, IsExRateToleranceEnabled : true, IsAuthorisationRequiredLevelUnchanged : false, regardless of ex-rate change", emptyUser, emptyUser, nonEmptyUser);

			TestPaymentApproval.AV_Amount = 7500m;
			TestPaymentApproval.AV_PayExRate = 1m;
			ResetApprovalUsers(true);
			Factory.Save();
			AssertCorrectAuthorisationRequired(TestPaymentApproval.AV_Amount, true, true, true, Over7000);
			AssertEquals(7500m, TestPaymentApproval.AV_Calc_LocalAmount);
			AssertEquals(false, TestPaymentApproval.AV_AmountInfo.HasChanges);
			AssertApprovalUsers("ShouldCheckExRateTolerance : true, IsExRateToleranceEnabled : true, IsAuthorisationRequiredLevelUnchanged : true, no Local Amount change", nonEmptyUser, nonEmptyUser, nonEmptyUser);

			TestPaymentApproval.AV_PayExRate = 0.95m;
			AssertEquals(7894.74m, TestPaymentApproval.AV_Calc_LocalAmount);
			AssertApprovalUsers("ShouldCheckExRateTolerance : true, IsExRateToleranceEnabled : true, IsAuthorisationRequiredLevelUnchanged : true, Value Changed is with in 10%", nonEmptyUser, nonEmptyUser, nonEmptyUser);

			TestPaymentApproval.AV_PayExRate = 0.85m;
			AssertEquals(8823.53m, TestPaymentApproval.AV_Calc_LocalAmount);
			AssertApprovalUsers("ShouldCheckExRateTolerance : true, IsExRateToleranceEnabled : true, IsAuthorisationRequiredLevelUnchanged : true, Value Changed exceeds 10%", emptyUser, emptyUser, emptyUser);

			TestPaymentApproval.AV_Amount = 7500m;
			TestPaymentApproval.AV_PayExRate = 1m;
			ResetApprovalUsers(true);
			Factory.Save();
			AssertCorrectAuthorisationRequired(TestPaymentApproval.AV_Amount, true, true, true, Over7000);
			AssertEquals(7500m, TestPaymentApproval.AV_Calc_LocalAmount);
			AssertEquals(false, TestPaymentApproval.AV_AmountInfo.HasChanges);
			TestPaymentApproval.AV_PayExRate = 2m;
			AssertEquals("Authorisation Required Level changed", 3750m, TestPaymentApproval.AV_Calc_LocalAmount);
			AssertCorrectAuthorisationRequired(TestPaymentApproval.AV_Amount, false, false, true, UpTo4000);
			AssertApprovalUsers("ShouldCheckExRateTolerance : true, IsExRateToleranceEnabled : true, IsAuthorisationRequiredLevelUnchanged : false, no Local Amount change", emptyUser, emptyUser, nonEmptyUser);
		}

		public void TestAuthorisationSettingsWhenRegistryIsNotOverridden()
		{
			AssertEquals("Description of Authorization Required", "No Authorization Required", TestPaymentApproval.AV_Calc_DescriptionOfAuthorisationRequired);
			AssertEquals("Status should default to Fully Approved", PaymentApprovalStatus.FullyApproved, TestPaymentApproval.AV_Status);

			AssertEquals("No Authorisation Required", true, TestPaymentApproval.NoAuthorisationRequired);
			AssertEquals("Level 1 Authorisation Required", false, TestPaymentApproval.Level1AuthorisationRequired);
			AssertEquals("Level 2 Authorisation Required", false, TestPaymentApproval.Level2AuthorisationRequired);
			AssertEquals("Level 3 Authorisation Required", false, TestPaymentApproval.Level3AuthorisationRequired);

			AssertEquals("Level 1 Authorisation Required", "Not Required", TestPaymentApproval.Level1AuthorisationStatus);
			AssertEquals("Level 2 Authorisation Required", "Not Required", TestPaymentApproval.Level2AuthorisationStatus);
			AssertEquals("Level 3 Authorisation Required", "Not Required", TestPaymentApproval.Level3AuthorisationStatus);
		}

		public void TestAuthorisationSettings()
		{
			try
			{
				SetUpRegistryForTest();

				AssertCorrectAuthorisationRequired(1, false, false, false, UpTo1000);
				AssertCorrectAuthorisationRequired(500, false, false, false, UpTo1000);
				AssertCorrectAuthorisationRequired(1000, false, false, false, UpTo1000);

				AssertCorrectAuthorisationRequired(1001, true, false, false, UpTo2000);
				AssertCorrectAuthorisationRequired(1500, true, false, false, UpTo2000);
				AssertCorrectAuthorisationRequired(2000, true, false, false, UpTo2000);

				AssertCorrectAuthorisationRequired(2001, false, true, false, UpTo3000);
				AssertCorrectAuthorisationRequired(2500, false, true, false, UpTo3000);
				AssertCorrectAuthorisationRequired(3000, false, true, false, UpTo3000);

				AssertCorrectAuthorisationRequired(3001, false, false, true, UpTo4000);
				AssertCorrectAuthorisationRequired(3500, false, false, true, UpTo4000);
				AssertCorrectAuthorisationRequired(4000, false, false, true, UpTo4000);

				AssertCorrectAuthorisationRequired(4001, true, true, false, UpTo5000);
				AssertCorrectAuthorisationRequired(4500, true, true, false, UpTo5000);
				AssertCorrectAuthorisationRequired(5000, true, true, false, UpTo5000);

				AssertCorrectAuthorisationRequired(5001, true, false, true, UpTo6000);
				AssertCorrectAuthorisationRequired(5500, true, false, true, UpTo6000);
				AssertCorrectAuthorisationRequired(6000, true, false, true, UpTo6000);

				AssertCorrectAuthorisationRequired(6001, false, true, true, UpTo7000);
				AssertCorrectAuthorisationRequired(6500, false, true, true, UpTo7000);
				AssertCorrectAuthorisationRequired(7000, false, true, true, UpTo7000);

				AssertCorrectAuthorisationRequired(7001, true, true, true, Over7000);
				AssertCorrectAuthorisationRequired(7500, true, true, true, Over7000);
				AssertCorrectAuthorisationRequired(8000, true, true, true, Over7000);
			}
			finally
			{
				ResetRegistryForTest();
			}
		}

		public void TestAuthorisationSecurity()
		{
			var firstExpectedApprovalCode = FirstApprovalCheckPoint.DisplayTextPathToSecurityRight;
			var secondExpectedApprovalCode = SecondApprovalCheckPoint.DisplayTextPathToSecurityRight;
			var thirdExpectedApprovalCode = ThirdApprovalCheckPoint.DisplayTextPathToSecurityRight;
			var cancelExpectedApprovalCode = CancelApprovalCheckPoint.DisplayTextPathToSecurityRight;

			var firstApprovalCode = TestPaymentApproval.FirstApprovalCheckpoint_ForTestOnly.DisplayTextPathToSecurityRight;
			var secondApprovalCode = TestPaymentApproval.SecondApprovalCheckpoint_ForTestOnly.DisplayTextPathToSecurityRight;
			var thirdApprovalCode = TestPaymentApproval.ThirdApprovalCheckpoint_ForTestOnly.DisplayTextPathToSecurityRight;
			var cancelApprovalCode = TestPaymentApproval.CancelApprovalCheckpoint_ForTestOnly.DisplayTextPathToSecurityRight;

			AssertEquals("First CheckPoint", firstExpectedApprovalCode, firstApprovalCode);
			AssertEquals("Second CheckPoint", secondExpectedApprovalCode, secondApprovalCode);
			AssertEquals("Third CheckPoint", thirdExpectedApprovalCode, thirdApprovalCode);
			AssertEquals("Cancel CheckPoint", cancelExpectedApprovalCode, cancelApprovalCode);
		}

		public void TestCreatorNotificationEmailSentWhenActionMade_Authorise()
		{
			AssertCreatorNotificationEmailSentWhenActionMade("Approved", (approval, errors) => approval.TryAuthorisePayment(errors), false);
		}

		public void TestCreatorNotificationEmailSentWhenActionMade_AuthoriseUser1()
		{
			AssertCreatorNotificationEmailSentWhenActionMade("Approved", (approval, errors) => approval.ApproveFirstApproval());
		}

		public void TestCreatorNotificationEmailSentWhenActionMade_AuthoriseUser2()
		{
			AssertCreatorNotificationEmailSentWhenActionMade("Approved", (approval, errors) => approval.ApproveSecondApproval());
		}

		public void TestCreatorNotificationEmailSentWhenActionMade_AuthoriseUser3()
		{
			AssertCreatorNotificationEmailSentWhenActionMade("Approved", (approval, errors) => approval.ApproveThirdApproval());
		}

		public void TestCreatorNotificationEmailSentWhenActionMade_Reject()
		{
			AssertCreatorNotificationEmailSentWhenActionMade("Rejected", (approval, errors) => approval.TryRejectPayment(errors));
		}

		public void TestCreatorNotificationEmailSentWhenActionMade_Cancel()
		{
			AssertCreatorNotificationEmailSentWhenActionMade("Cancelled", (approval, errors) => approval.TryCancelPayment(errors));
		}

		public void AssertCreatorNotificationEmailSentWhenActionMade(string actionText, Action<PaymentApprovalWithAuthorisation, NotificationBuffer> action, bool setAuthSettings = true)
		{
			if (setAuthSettings)
			{
				var settings = new PaymentAuthorisationSettingsCollection();
				MakeNewAuthorisationSetting(settings, RangeCodes.Above, 0, AuthorisationCodes.AllThreeApprovalRequired);
				AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, settings);
			}
			var newStaff = Factory.NewWithValidTestData<GlbStaff>();
			newStaff.GS_EmailAddress = "waiting@outsi.de";
			Factory.Save();

			PaymentApprovalWithAuthorisation approval;
			using (Env.SetTemporaryUserContext(newStaff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				approval = GetNewPaymentApproval();
				approval.AV_Amount = 1000m;
				approval.AV_OH = TestObjectCreator.TestOrganisation.PK;
				approval.AV_Status = PaymentApprovalStatus.AwaitingApproval;
				Factory.Save();
			}
			AssertEquals("Precondition: Approval Status", PaymentApprovalStatus.AwaitingApproval, approval.AV_Status);

			FirstApprovalCheckPoint.IsAllowed = true;
			SecondApprovalCheckPoint.IsAllowed = true;
			ThirdApprovalCheckPoint.IsAllowed = true;
			var errors = new NotificationBuffer();
			action(approval, errors);
			Assert(!errors.HasErrors);
			Factory.Save();

			var email = Env.OutgoingMailManager.EmailsCreated.Single();
			AssertContains(actionText, email.Subject);
		}

		public void TestCreatorNotificationEmailSentWhenActionMade_Unauthorise()
		{
			AssertCreatorNotificationEmailSentWhenActionMade_Unauthorise((approval, errors) => approval.TryUnauthorisePayment(errors));
		}

		public void TestCreatorNotificationEmailSentWhenActionMade_UnauthoriseUser1()
		{
			AssertCreatorNotificationEmailSentWhenActionMade_Unauthorise((approval, errors) => approval.UnApproveFirstApproval());
		}

		public void TestCreatorNotificationEmailSentWhenActionMade_UnauthoriseUser2()
		{
			AssertCreatorNotificationEmailSentWhenActionMade_Unauthorise((approval, errors) => approval.UnApproveSecondApproval());
		}

		public void TestCreatorNotificationEmailSentWhenActionMade_UnauthoriseUser3()
		{
			AssertCreatorNotificationEmailSentWhenActionMade_Unauthorise((approval, errors) => approval.UnApproveThirdApproval());
		}

		void AssertCreatorNotificationEmailSentWhenActionMade_Unauthorise(Action<PaymentApprovalWithAuthorisation, NotificationBuffer> action)
		{
			var settings = new PaymentAuthorisationSettingsCollection();
			MakeNewAuthorisationSetting(settings, RangeCodes.Above, 0, AuthorisationCodes.AllThreeApprovalRequired);
			AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, settings);

			var newStaff = Factory.NewWithValidTestData<GlbStaff>();
			newStaff.GS_EmailAddress = "waiting@outsi.de";
			Factory.Save();

			FirstApprovalCheckPoint.IsAllowed = true;
			SecondApprovalCheckPoint.IsAllowed = true;
			ThirdApprovalCheckPoint.IsAllowed = true;

			PaymentApprovalWithAuthorisation approval;
			using (Env.SetTemporaryUserContext(newStaff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				approval = GetNewPaymentApproval();
				approval.AV_Amount = 1000m;
				approval.AV_OH = TestObjectCreator.TestOrganisation.PK;
				approval.AV_Status = PaymentApprovalStatus.AwaitingApproval;
				Factory.Save();
			}
			approval.ApproveFirstApproval();
			approval.ApproveSecondApproval();
			approval.ApproveThirdApproval();
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertEquals("Precondition: Approval Status", PaymentApprovalStatus.FullyApproved, approval.AV_Status);
			AssertNotNull("Precondition: Needs a valid approving user to Unauthorise successfully", approval.Approval1st);

			var errors = new NotificationBuffer();
			action(approval, errors);
			Assert(!errors.HasErrors);
			Factory.Save();

			var email = Env.OutgoingMailManager.EmailsCreated.Single();
			AssertContains("Unauthorized", email.Subject);
		}

		public void TestCreatorNotificationEmailNotSentWhenNoOverallChanges()
		{
			var settings = new PaymentAuthorisationSettingsCollection();
			MakeNewAuthorisationSetting(settings, RangeCodes.Above, 0, AuthorisationCodes.AllThreeApprovalRequired);
			AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, settings);

			var newStaff = Factory.NewWithValidTestData<GlbStaff>();
			newStaff.GS_EmailAddress = "waiting@outsi.de";
			Factory.Save();

			FirstApprovalCheckPoint.IsAllowed = true;
			SecondApprovalCheckPoint.IsAllowed = true;
			ThirdApprovalCheckPoint.IsAllowed = true;

			PaymentApprovalWithAuthorisation approval;
			using (Env.SetTemporaryUserContext(newStaff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				approval = GetNewPaymentApproval();
				approval.AV_Amount = 1000m;
				approval.AV_OH = TestObjectCreator.TestOrganisation.PK;
				approval.AV_Status = PaymentApprovalStatus.AwaitingApproval;
				Factory.Save();
			}

			Env.OutgoingMailManager.EmailsCreated.Clear();

			var errors = new NotificationBuffer();
			approval.TryRejectPayment(errors);
			approval.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			approval.TryAuthorisePayment(errors);
			approval.TryUnauthorisePayment(errors);
			Assert(!errors.HasErrors);
			Assert(!approval.AV_StatusInfo.HasChanges);
			Assert(!approval.AV_GS_NKApproval1stInfo.HasChanges);
			Assert(!approval.AV_GS_NKApproval2ndInfo.HasChanges);
			Assert(!approval.AV_GS_NKApproval3rdInfo.HasChanges);
			Factory.Save();

			AssertEquals("If no approval changes are made, no email is sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestCreatorNotificationEmailNotSentIfUserSameAsCreator()
		{
			var newStaff = Factory.NewWithValidTestData<GlbStaff>();
			newStaff.GS_EmailAddress = "waiting@outsi.de";
			Factory.Save();

			using (Env.SetTemporaryUserContext(newStaff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var approval = GetNewPaymentApproval();
				approval.AV_Amount = 1000m;
				approval.AV_OH = TestObjectCreator.TestOrganisation.PK;
				approval.AV_Status = PaymentApprovalStatus.AwaitingApproval;
				Factory.Save();
				AssertEquals("Precondition: Approval Status", PaymentApprovalStatus.AwaitingApproval, approval.AV_Status);

				FirstApprovalCheckPoint.IsAllowed = true;
				SecondApprovalCheckPoint.IsAllowed = true;
				ThirdApprovalCheckPoint.IsAllowed = true;
				var errors = new NotificationBuffer();
				approval.TryAuthorisePayment(errors);
				Assert(!errors.HasErrors);
				Factory.Save();
				AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		[TestDate(2020, 05, 14, 10, 16, 33)]
		public void TestCreatorNotificationEmailContent()
		{
			var newStaff = Factory.NewWithValidTestData<GlbStaff>();
			newStaff.GS_EmailAddress = "waiting@outsi.de";
			Factory.Save();

			PaymentApprovalWithAuthorisation approval;

			using (Env.SetTemporaryUserContext(newStaff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				approval = GetNewPaymentApproval();
				approval.AV_Amount = 1000m;
				approval.AV_OH = TestObjectCreator.TestOrganisation.PK;
				approval.AV_Status = PaymentApprovalStatus.AwaitingApproval;
				Factory.Save();
			}
			AssertEquals("Precondition: Approval Status", PaymentApprovalStatus.AwaitingApproval, approval.AV_Status);

			FirstApprovalCheckPoint.IsAllowed = true;
			SecondApprovalCheckPoint.IsAllowed = true;
			ThirdApprovalCheckPoint.IsAllowed = true;
			var errors = new NotificationBuffer();
			approval.TryAuthorisePayment(errors);
			Assert(!errors.HasErrors);
			Factory.Save();

			var email = Env.OutgoingMailManager.EmailsCreated.Single();

			AssertEquals("waiting@outsi.de", email.Recipients.Cast<RecipientDef>().Single().Email);

			var (expectedSubject, expectedBody) = GetExpectedCreatorNotification(approval.PK);
			AssertEquals(expectedSubject, email.Subject);

			string removeHash(string s)
			{
				var startIndex = s.IndexOf("&Hash=") + 6;
				var endIndex = s.IndexOf(@""">ZOrg - 14-May");

				return s.Substring(0, startIndex) + s.Substring(endIndex);
			}

			string extractContent(string s)
			{
				string start = @"<td class=""content"">";
				var startIndex = s.IndexOf(start) + start.Length;

				var endIndex = s.IndexOf("</td>", startIndex);

				var content = s.Substring(startIndex, endIndex - startIndex);
				return content;
			}

			AssertEquals(removeHash(expectedBody).Trim(), removeHash(extractContent(email.Body)).Trim());
		}

		protected abstract (string, string) GetExpectedCreatorNotification(ZGuid aprovalPK);

		protected override void AssertTransactionAlreadyPaidHasExpectedError(PaymentApprovalBase approval, string exceptionMessage)
		{
			var expectedError = $@"Transaction already paid

PaymentApprovalBaseMatchDetails:
PaymentApprovalMatchingBase.Match()
Current company's currency Code: AUD, Decimals: 2
Ledger: {approval.Ledger}, Transaction type: UNA, Payment Amount: 305, Outstanding Amount: 305, OS Payment Amount: 305, OS Outstanding Amount: 305, Currency: AUD, Exchange Rate Amount: 1";
			AssertContains(expectedError, exceptionMessage);

			expectedError = $@"MatchingBase.Match()
Current company's currency Code: AUD, Decimals: 2
Ledger: {approval.Ledger}, Transaction type: PAY, Payment Amount: 305, Outstanding Amount: 305, OS Payment Amount: 305, OS Outstanding Amount: 305, Currency: AUD, Exchange Rate Amount: 1
Ledger: {approval.Ledger}, Transaction type: EXX, Payment Amount: -5, Outstanding Amount: -5, OS Payment Amount: -5, OS Outstanding Amount: -5, Currency: AUD, Exchange Rate Amount: 1";
			AssertContains(expectedError, exceptionMessage);

			expectedError = @"Ledger: AP, Transaction type: CRD, Payment Amount: -200, Outstanding Amount: -200, OS Payment Amount: -200, OS Outstanding Amount: -200, Currency: AUD, Exchange Rate Amount: 1";
			AssertContains(expectedError, exceptionMessage);

			expectedError = @"Ledger: AP, Transaction type: CRD, Payment Amount: -100, Outstanding Amount: 0, OS Payment Amount: -100, OS Outstanding Amount: 0, Currency: AUD, Exchange Rate Amount: 1";
			AssertContains(expectedError, exceptionMessage);

			expectedError = "MatchedTransactions contains transaction from primary organization: true";
			AssertContains(expectedError, exceptionMessage);

			expectedError = "Match group info: There is no data collected.";
			AssertContains(expectedError, exceptionMessage);
		}

		protected override void SetUpForTestAddressesOnPayments()
		{
			TestPaymentApproval.FullyApprove();
			TestPaymentApproval.IsAllowedToPost = true;
		}

		protected abstract SecurityCheckpoint FirstApprovalCheckPoint
		{
			get;
		}

		protected abstract SecurityCheckpoint SecondApprovalCheckPoint
		{
			get;
		}

		protected abstract SecurityCheckpoint ThirdApprovalCheckPoint
		{
			get;
		}

		protected abstract SecurityCheckpoint CancelApprovalCheckPoint
		{
			get;
		}

		protected abstract SecurityCheckpoint CancelEPaymentCheckPoint
		{
			get;
		}

		public override void TestCreateNewPaymentCore_AutoAllocation()
		{
			Assert("Payment approval with authorisation doesn't have this method", true);
		}

		public override void TestSetAV_AK()
		{
			Assert("Payment approval with authorisation doesn't post on save", true);
		}

		public void TestDescriptonOfAuthorisationRequiredIsTranslatable()
		{
			try
			{
				SetUpRegistryForTest();

				TestPaymentApproval.AV_Amount = 1;
				AssertEquals("None (Up to 1000)", TestPaymentApproval.AV_Calc_DescriptionOfAuthorisationRequired);

				using (var mockRes = Res.UseMockData())
				{
					mockRes.SetResourceGetter(delegate(string key)
					{ return key == "B9270455-ED4D-490D-8A39-F96C9C89BB0D" ? new ResourceStringData(key, "{0} ({1} {2})") : new ResourceStringData(key, "Good"); });
					AssertEquals("Good (Good 1000)", TestPaymentApproval.AV_Calc_DescriptionOfAuthorisationRequired);
				}
			}
			finally
			{
				ResetRegistryForTest();
			}
		}

		public void TestCanSendAuthorisationEmail()
		{
			var loggedInUser = Factory.NewWithValidTestData<GlbStaff>();
			var creatingUserWithEmail = Factory.NewWithValidTestData<GlbStaff>();
			var creatingUserWithoutEmail = Factory.NewWithValidTestData<GlbStaff>();

			creatingUserWithEmail.GS_Code = "TST";
			creatingUserWithEmail.GS_EmailAddress = "test@test.com";
			creatingUserWithoutEmail.GS_Code = "MLG";
			loggedInUser.GS_Code = "LOG";
			Factory.Save();

			using (Env.SetTemporaryUserContext(creatingUserWithEmail.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var approval = GetNewPaymentApproval();
				var staffCode = approval.Logs.CreatedByUserInitials;

				using (Env.SetTemporaryUserContext(loggedInUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var result = approval.CanSendNotificationEmail();
					AssertEquals("TST", staffCode);
					AssertEquals(true, result);
				}
			}

			using (Env.SetTemporaryUserContext(creatingUserWithoutEmail.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var approval = GetNewPaymentApproval();
				var staffCode = approval.Logs.CreatedByUserInitials;

				using (Env.SetTemporaryUserContext(loggedInUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var result = approval.CanSendNotificationEmail();
					AssertEquals("MLG", staffCode);
					AssertEquals(false, result);
				}
			}

			using (Env.SetTemporaryUserContext(creatingUserWithEmail.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var approval = GetNewPaymentApproval();
				var staffCode = approval.Logs.CreatedByUserInitials;

				var result = approval.CanSendNotificationEmail();
				AssertEquals("TST", staffCode);
				AssertEquals(false, result);
			}
		}

		#region Test Submit For Approval

		public void TestTrySubmitForApproval_ShowError()
		{
			var buffer = new NotificationBuffer();

			TestPaymentApproval.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			TestPaymentApproval.TrySubmitForApproval(buffer);
			AssertEquals(true, buffer.HasErrors);
			AssertContains("This Payment is not in Draft status", buffer.AsString);

			buffer.Clear();
			TestPaymentApproval.AV_Status = PaymentApprovalStatus.Draft;
			TestPaymentApproval.AV_AB = ZGuid.NewZGuid();
			TestPaymentApproval.TrySubmitForApproval(buffer);
			AssertContains("Error - AV_AB: Enter a valid Bank Account.", buffer.AsString);
			AssertContains("Error - Balance: The balance must equal 0", buffer.AsString);
		}

		public void TestTrySubmitForApproval_UpdateStatusToAPP()
		{
			AssertTrySubmitForApproval((x) =>
			{
				AssertEquals(true, x.IsFullyApproved);
			});
		}

		public void TestTrySubmitForApproval_UpdateStatusToAWA()
		{
			PaymentAuthorisationSettingsCollection collection = new PaymentAuthorisationSettingsCollection();
			PaymentAuthorisationSettings newSetting = collection.AddNew();
			newSetting.Amount = 0m;
			newSetting.AuthorisationRequirement = AuthorisationCodes.AllThreeApprovalRequired;
			newSetting.Range = RangeCodes.Above;
			using (AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				AssertTrySubmitForApproval((x) =>
				{
					AssertEquals(true, x.IsAwaitingApproval);
				});
			}
		}

		void AssertTrySubmitForApproval(Action<PaymentApprovalWithAuthorisation> assertAction)
		{
			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_OH = TestOrgHeader.PK;
			invoice.AH_TransactionNum = "00001001";
			invoice.AH_OSExTaxAmount = 150M;

			InvoicingBaseCollection invoices = new InvoicingBaseCollection(Factory);
			invoices.AddRange(new BusinessObject[] { invoice });
			AssertEquals(1, invoices.Count);

			TestPaymentApproval.AV_Status = PaymentApprovalStatus.Draft;
			TestPaymentApproval.AV_Amount = 150M;
			TestPaymentApproval.PaymentMatchingBaseObject.MoveFromUnmatchToMatch(invoices.ToArray());
			AssertEquals(0m, TestPaymentApproval.PaymentMatchingBaseObject.Balance);

			var buffer = new NotificationBuffer();
			TestPaymentApproval.TrySubmitForApproval(buffer);
			AssertEquals(false, buffer.HasErrors);
			assertAction(TestPaymentApproval);
		}

		#endregion

		public void TestReadOnlyCanBeSetToTrueWhenStatusIsNotCancelled()
		{
			AssertEquals(false, TestPaymentApproval.ReadOnly);
			AssertEquals(false , TestPaymentApproval.IsCancelled);

			TestPaymentApproval.SetReadOnlyIncludingChildren(true);

			AssertEquals(true, TestPaymentApproval.ReadOnly);
			AssertEquals(false, TestPaymentApproval.IsCancelled);
		}

		#region Implementation

		void ResetApprovalUsers(bool shouldFillUsers = false)
		{
			TestPaymentApproval.AV_GS_NKApproval1st = 
			TestPaymentApproval.AV_GS_NKApproval2nd = 
			TestPaymentApproval.AV_GS_NKApproval3rd = GetUserToFill();

			string GetUserToFill() => shouldFillUsers ? GlbStaff.CurrentUser.GS_Code : null;
		}

		void AssertApprovalUsers(string message, string approval1st, string approval2nd, string approval3rd)
		{
			AssertEquals(message + "(approval1st)", approval1st, TestPaymentApproval.AV_GS_NKApproval1st);
			AssertEquals(message + "(approval2nd)", approval2nd, TestPaymentApproval.AV_GS_NKApproval2nd);
			AssertEquals(message + "(approval3rd)", approval3rd, TestPaymentApproval.AV_GS_NKApproval3rd);
		}

		void AssertCorrectAuthorisationRequired(ZDecimal amount, bool firstRequired, bool secondRequired, bool thirdRequired, PaymentAuthorisationSettings setting)
		{
			TestPaymentApproval.AV_Amount = amount;
			ZString description = "Local Amount = $" + TestPaymentApproval.AV_Calc_LocalAmount.ToString();

			AssertEquals(description + " (First Authorisation Required)", firstRequired, TestPaymentApproval.Level1AuthorisationRequired);
			AssertEquals(description + " (Second Authorisation Required)", secondRequired, TestPaymentApproval.Level2AuthorisationRequired);
			AssertEquals(description + " (Third Authorisation Required)", thirdRequired, TestPaymentApproval.Level3AuthorisationRequired);

			AssertEquals(description + " (Authorisation Requirement)", setting.AuthorisationRequirement,
				TestPaymentApproval.AuthorisationRequired.AuthorisationRequirement);
		}

		PaymentApprovalWithAuthorisation TestPaymentApproval
		{
			get { return fTestPaymentApproval as PaymentApprovalWithAuthorisation; }
		}

		PaymentApprovalWithAuthorisation GetNewPaymentApproval()
		{
			return GetNewBusinessObject() as PaymentApprovalWithAuthorisation;
		}

		protected override bool GetIsCreatedForPostingValue(PaymentApprovalBase paymentApproval)
		{
			return paymentApproval.IsAllowedToPost;
		}

		IDisposable instanceDetailsDisposable;

		protected override void SetUp()
		{
			base.SetUp();
			instanceDetailsDisposable = InstanceDetails.SetUpCurrentForTest();
		}

		protected override void TearDown()
		{
			instanceDetailsDisposable?.Dispose();
			base.TearDown();
		}

		#endregion

	}
}
