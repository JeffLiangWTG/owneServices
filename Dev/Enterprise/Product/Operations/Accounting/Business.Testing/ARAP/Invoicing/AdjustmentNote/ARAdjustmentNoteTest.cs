using System;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(ARAdjustmentNote))]
	public class ARAdjustmentNoteTest : AdjustmentNoteTest
	{
		public void TestIEdocsParsingSupportProvider()
		{
			var bo = Factory.NewWithValidTestData<ARAdjustmentNote>();
			var eDocsParsingSupport = bo as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		protected override bool ShouldSupportCalculatingTaxAtHeaderLevel
		{
			get { return true; }
		}

		#region TestExchangeRateRecalculatedFromOtherTaxesAmounts

		public override void TestExchangeRateRecalculatedFromOSTaxAmountOtherTaxes_WhenAPARInvoicePostingExchangeRateOptionIsDEF()
		{
			Assert("Not Applicable", true);
		}

		public override void TestExchangeRateRecalculatedFromLocalTaxAmountOtherTaxes_WhenAPARInvoicePostingExchangeRateOptionIsDEF()
		{
			Assert("Not Applicable", true);
		}

		public override void TestExchangeRateRecalculatedFromOSTaxAmountOtherTaxes_WhenAPInvoiceUseJobExchangeRateFlagIsTicked()
		{
			Assert("Not Applicable", true);
		}

		public override void TestExchangeRateRecalculatedFromLocalTaxAmountOtherTaxes_WhenAPInvoiceUseJobExchangeRateFlagIsTicked()
		{
			Assert("Not Applicable", true);
		}

		public override void TestExchangeRateNOTRecalculatedFromOSTaxAmountOtherTaxes_WhenAPARInvoicePostingExchangeRateOptionIsTOD()
		{
			Assert("Not Applicable", true);
		}

		public override void TestExchangeRateNOTRecalculatedFromLocalTaxAmountOtherTaxes_WhenAPARInvoicePostingExchangeRateOptionIsTOD()
		{
			Assert("Not Applicable", true);
		}

		public override void TestExchangeRateNOTRecalculatedFromOSTaxAmountOtherTaxes_WhenAPARInvoicePostingExchangeRateOptionIsINV()
		{
			Assert("Not Applicable", true);
		}

		public override void TestExchangeRateNOTRecalculatedFromLocalTaxAmountOtherTaxes_WhenAPARInvoicePostingExchangeRateOptionIsINV()
		{
			Assert("Not Applicable", true);
		}

		public override void TestExchangeRateNOTRecalculatedFromOSTaxAmountOtherTaxes_WhenAPARInvoicePostingExchangeRateOptionIsPST()
		{
			Assert("Not Applicable", true);
		}

		public override void TestExchangeRateNOTRecalculatedFromLocalTaxAmountOtherTaxes_WhenAPARInvoicePostingExchangeRateOptionIsPST()
		{
			Assert("Not Applicable", true);
		}

		#endregion

		protected override Type GetExpectedBusinessObjectLineType()
		{
			return typeof(ARAdjustmentNoteLine);
		}

		public void TestAH_InvoiceTerm_ReadOnly()
		{
			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.NewReceivablesAdjustmentNoteTerm.IsAllowed = true;
			AssertEquals(false, InvoicingBase.AH_InvoiceTermInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Env.Security.NewReceivablesAdjustmentNoteTerm.IsAllowed = true;
			AssertEquals(true, InvoicingBase.AH_InvoiceTermInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.NewReceivablesAdjustmentNoteTerm.IsAllowed = false;
			AssertEquals(true, InvoicingBase.AH_InvoiceTermInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Env.Security.NewReceivablesAdjustmentNoteTerm.IsAllowed = false;
			AssertEquals(true, InvoicingBase.AH_InvoiceTermInfo.ReadOnly);
		}

		public void TestAH_InvoiceTermDays_ReadOnly()
		{
			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.NewReceivablesAdjustmentNoteTerm.IsAllowed = true;
			InvoicingBase.AH_InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			AssertEquals(true, InvoicingBase.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Env.Security.NewReceivablesAdjustmentNoteTerm.IsAllowed = true;
			InvoicingBase.AH_InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			AssertEquals(true, InvoicingBase.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.NewReceivablesAdjustmentNoteTerm.IsAllowed = false;
			InvoicingBase.AH_InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			AssertEquals(true, InvoicingBase.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Env.Security.NewReceivablesAdjustmentNoteTerm.IsAllowed = false;
			InvoicingBase.AH_InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			AssertEquals(true, InvoicingBase.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.NewReceivablesAdjustmentNoteTerm.IsAllowed = true;
			InvoicingBase.AH_InvoiceTerm = Constants.InvoiceTerms.PaymentInAdvance;
			AssertEquals(false, InvoicingBase.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Env.Security.NewReceivablesAdjustmentNoteTerm.IsAllowed = true;
			InvoicingBase.AH_InvoiceTerm = Constants.InvoiceTerms.PaymentInAdvance;
			AssertEquals(true, InvoicingBase.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.NewReceivablesAdjustmentNoteTerm.IsAllowed = false;
			InvoicingBase.AH_InvoiceTerm = Constants.InvoiceTerms.PaymentInAdvance;
			AssertEquals(true, InvoicingBase.AH_InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Env.Security.NewReceivablesAdjustmentNoteTerm.IsAllowed = false;
			InvoicingBase.AH_InvoiceTerm = Constants.InvoiceTerms.PaymentInAdvance;
			AssertEquals(true, InvoicingBase.AH_InvoiceTermDaysInfo.ReadOnly);
		}

		public void TestAH_InvoiceDate_ReadOnly()
		{
			Env.Security.NewReceivablesAdjustmentNoteInvoiceDate.IsAllowed = true;
			Assert("Should not be readonly as the security right is true.", !InvoicingBase.AH_InvoiceDateInfo.ReadOnly);

			Env.Security.NewReceivablesAdjustmentNoteInvoiceDate.IsAllowed = false;
			Assert("Should be readonly as the security right is false.", InvoicingBase.AH_InvoiceDateInfo.ReadOnly);

			using (AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingConstants.InvAndPstDateDefaultingRuleTypes.MonthEndSuspension.Code))
			{
				Env.Security.NewReceivablesAdjustmentNoteInvoiceDate.IsAllowed = true;
				Assert("Should be readonly as the registry is set to MTH and then it doesn't matter to the security right.", InvoicingBase.AH_InvoiceDateInfo.ReadOnly);

				Env.Security.NewReceivablesAdjustmentNoteInvoiceDate.IsAllowed = false;
				Assert("Should be readonly as the registry is set to MTH and then it doesn't matter to the security right.", InvoicingBase.AH_InvoiceDateInfo.ReadOnly);
			}
		}

		public void TestNumberFountainWhenTransactionTypeChanges()
		{
			ShareSequentialTransactionNumbers item = new ShareSequentialTransactionNumbers();
			item.Value = true;
			AccountingConfigurationRegistry.Instance.ShareSequentialInvoiceTransactionNumbers.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, item);
			AssertEquals("Should be AR Invoice number fountain", Env.NumberFountains.ARInvoiceNo.GetTodaysPeriodFountain(), ((ARAdjustmentNote)InvoicingBase).NumberFountainForTransactionNumber_ForTestOnly.numberFountainPooler.GetTodaysPeriodFountain());
			AssertNotEquals("Should be AR Invoice number fountain", Env.NumberFountains.ARAdjustmentNoteNo.GetTodaysPeriodFountain(), ((ARAdjustmentNote)InvoicingBase).NumberFountainForTransactionNumber_ForTestOnly.numberFountainPooler.GetTodaysPeriodFountain());
			item.Value = false;
			AccountingConfigurationRegistry.Instance.ShareSequentialInvoiceTransactionNumbers.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, item);
			AssertNotEquals("Should be AR AdjustmentNoteNo number fountain", Env.NumberFountains.ARInvoiceNo.GetTodaysPeriodFountain(), ((ARAdjustmentNote)InvoicingBase).NumberFountainForTransactionNumber_ForTestOnly.numberFountainPooler.GetTodaysPeriodFountain());
			AssertEquals("Should be AR AdjustmentNoteNo number fountain", Env.NumberFountains.ARAdjustmentNoteNo.GetTodaysPeriodFountain(), ((ARAdjustmentNote)InvoicingBase).NumberFountainForTransactionNumber_ForTestOnly.numberFountainPooler.GetTodaysPeriodFountain());
		}

		public void TestDocManagerCode()
		{
			AssertEquals("Wrong DocManagerCode. Any change to the IDocManagerSupport interface must also be changed in document scanning lookup", "RAN", ((IDocManagerSupport)Factory.New<ARAdjustmentNote>()).DocManagerInfo.DocManagerCode);
		}

		[TestDate(2018, 01, 01)]
		public void TestAuthorisationRequiredBranchDepartmentWhenParentIsJob_ReceivableAuthorizationSettingsRegsitry()
		{
			var originalFirstAmountLevelAllows = Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed;
			var originalSecondAmountLevelAllows = Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed;
			try
			{
				var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("10001002"), false);
				var adjustNote = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("002", 90.00M, 5M, new ZDateTime(2008, 05, 15), TestObjectCreator.ABIGAS.PK);
				adjustNote.AH_JH = job.PK;
				adjustNote.AH_LocalExTaxAmount = -955M;
				TestObjectCreator.SetBranchDepartmentAuthorizationLevelSettings(job.Branch.PK, job.Department.PK, 1000m, 2000m);
				AssertCorrectAuthorisationRequired(adjustNote, false, false);
				adjustNote.AH_LocalExTaxAmount = -1955M;
				AssertCorrectAuthorisationRequired(adjustNote, true, false);
				adjustNote.AH_LocalExTaxAmount = -2955M;
				AssertCorrectAuthorisationRequired(adjustNote, false, true);
			}
			finally
			{
				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = originalFirstAmountLevelAllows;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = originalSecondAmountLevelAllows;
			}
		}

		[TestDate(2018, 01, 01)]
		public void TestAuthorisationRequiredBranchDepartmentWhenMiscellaneousCreditNote_ReceivableAuthorizationSettingsRegsitry()
		{
			var originalFirstAmountLevelAllows = Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed;
			var originalSecondAmountLevelAllows = Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed;
			try
			{
				var adjustNote = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("002", 90.00M, 5M, new ZDateTime(2008, 05, 15), TestObjectCreator.ABIGAS.PK);
				TestObjectCreator.SetBranchDepartmentAuthorizationLevelSettings(adjustNote.Branch.PK, adjustNote.Department.PK, 1000m, 2000m);
				adjustNote.AH_LocalExTaxAmount = -955M;
				AssertCorrectAuthorisationRequired(adjustNote, false, false);
				adjustNote.AH_LocalExTaxAmount = -1955M;
				AssertCorrectAuthorisationRequired(adjustNote, true, false);
				adjustNote.AH_LocalExTaxAmount = -2955M;
				AssertCorrectAuthorisationRequired(adjustNote, false, true);
			}
			finally
			{
				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = originalFirstAmountLevelAllows;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = originalSecondAmountLevelAllows;
			}
		}

		[TestDate(2018, 01, 01)]
		public void TestAuthorisationRequiredBranchDepartment_ReceivableReversalAuthorizationSettingsRegsitry()
		{
			var originalFirstAmountLevelAllows = Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed;
			var originalSecondAmountLevelAllows = Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed;
			try
			{
				var adjustNote = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("002", 90.00M, 5M, new ZDateTime(2008, 05, 15), TestObjectCreator.ABIGAS.PK);
				adjustNote.IsCreatingCreditNoteForReversal = true;
				TestObjectCreator.SetBranchDepartmentAuthorizationLevelSettings(adjustNote.Branch.PK, adjustNote.Department.PK, 1000m, 2000m, true);
				adjustNote.AH_LocalExTaxAmount = 955M;
				AssertCorrectAuthorisationRequired(adjustNote, false, false);
				adjustNote.AH_LocalExTaxAmount = 1955M;
				AssertCorrectAuthorisationRequired(adjustNote, true, false);
				adjustNote.AH_LocalExTaxAmount = 2955M;
				AssertCorrectAuthorisationRequired(adjustNote, false, true);
			}
			finally
			{
				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = originalFirstAmountLevelAllows;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = originalSecondAmountLevelAllows;
			}
		}

		protected override ZString ExpectedTransactionTypeForIncomplete
		{
			get { throw new NotSupportedException(); }
		}

		protected override BooleanRegistryItem EnforcePostingAtFixedPlaceOfSupplyLevelForTransactionRuleRegistry => AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForReceivableTransactions;
	}
}
