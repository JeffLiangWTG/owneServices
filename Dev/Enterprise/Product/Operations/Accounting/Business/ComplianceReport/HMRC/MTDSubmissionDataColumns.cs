using System;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ComplianceReport.HMRC
{
	public partial class MTDSubmissionDataColumns : NonPersistentBusinessObject
	{
		public MTDSubmissionDataColumns(BusinessObjectFactory factory, AccComplianceReport report) : base(factory)
		{
			Argument.NotNull(report, nameof(report));
			ComplianceReport = report;
			Status = AccTaxReturn.Status.Saved;

			ReasonHolder1 = new AdjustmentReasonHolder(() => Adjustments.Box1_VATDue) { HasChanges = false };
			ReasonHolder2 = new AdjustmentReasonHolder(() => Adjustments.Box2_VATDueReverseChg) { HasChanges = false };
			ReasonHolder4 = new AdjustmentReasonHolder(() => Adjustments.Box4_VATReclaimed) { HasChanges = false };
			ReasonHolder6 = new AdjustmentReasonHolder(() => Adjustments.Box6_TotalSalesExVAT) { HasChanges = false };
			ReasonHolder7 = new AdjustmentReasonHolder(() => Adjustments.Box7_TotalPurchaseExVAT) { HasChanges = false };
			ReasonHolder8 = new AdjustmentReasonHolder(() => Adjustments.Box8_GoodsSalesECMembersExVAT) { HasChanges = false };
			ReasonHolder9 = new AdjustmentReasonHolder(() => Adjustments.Box9_GoodsPurchaseECMembersExVAT) { HasChanges = false };
		}

		public void PopulateDataFromReport()
		{
			if (!IsSubmitted)
			{
				var helper = new MTDSubmissionDataHelper(ComplianceReport);
				helper.SetValues(ComputedByCW1, UnsubmitedPreviousValues);

				if (!IsGroupMemberSubmission)
				{
					helper.SetGroupMemberTotalValues(GroupMemberTotal);
				}

				UpdateValuesToSubmitToHMRC();
				TooOldWarning = helper.TooOldWarning;

				if (IsGroupMemberSubmission)
				{
					this.HasChanges = true;
				}
			}
		}

		public AccComplianceReport ComplianceReport { get; }

		#region Columns

		public MTDSubmissionData ComputedByCW1
		{
			get
			{
				if (computedByCW1 == null)
				{
					computedByCW1 = new MTDSubmissionData(Factory);
				}

				return computedByCW1;
			}
		}
		MTDSubmissionData computedByCW1;

		public MTDSubmissionData UnsubmitedPreviousValues
		{
			get
			{
				if (unsubmitedPreviousValues == null)
				{
					unsubmitedPreviousValues = new MTDSubmissionData(Factory);
				}

				return unsubmitedPreviousValues;
			}
		}
		MTDSubmissionData unsubmitedPreviousValues;

		public MTDSubmissionData Adjustments
		{
			get
			{
				if (adjustments == null)
				{
					adjustments = new MTDSubmissionData(Factory);
					RegisterEditableChildObject(adjustments);

					adjustments.Box1_VATDueInfo.ValueChanged += AdjustmentsEvent;
					adjustments.Box2_VATDueReverseChgInfo.ValueChanged += AdjustmentsEvent;
					adjustments.Box4_VATReclaimedInfo.ValueChanged += AdjustmentsEvent;
					adjustments.Box6_TotalSalesExVATInfo.ValueChanged += AdjustmentsEvent;
					adjustments.Box7_TotalPurchaseExVATInfo.ValueChanged += AdjustmentsEvent;
					adjustments.Box8_GoodsSalesECMembersExVATInfo.ValueChanged += AdjustmentsEvent;
					adjustments.Box9_GoodsPurchaseECMembersExVATInfo.ValueChanged += AdjustmentsEvent;
				}
				return adjustments;
			}
		}
		MTDSubmissionData adjustments;

		void AdjustmentsEvent(object sender, EventArgs e)
		{
			UpdateValuesToSubmitToHMRC();
		}

		public MTDSubmissionData GroupMemberTotal
		{
			get
			{
				if (groupMemberTotal == null)
				{
					groupMemberTotal = new MTDSubmissionData(Factory);
				}

				return groupMemberTotal;
			}
		}
		MTDSubmissionData groupMemberTotal;

		public MTDSubmissionData ValuesToSubmitToHMRC
		{
			get
			{
				if (valuesToSubmitToHMRC == null)
				{
					valuesToSubmitToHMRC = new MTDSubmissionData(Factory);
				}

				return valuesToSubmitToHMRC;
			}
		}
		MTDSubmissionData valuesToSubmitToHMRC;

		#endregion

#if DEBUG
		public
#endif
		void UpdateValuesToSubmitToHMRC()
		{
			if (IsOverThreshold)
			{
				ValuesToSubmitToHMRC.Box1_VATDue = ComputedByCW1.Box1_VATDue + Adjustments.Box1_VATDue + GroupMemberTotal.Box1_VATDue;
				ValuesToSubmitToHMRC.Box2_VATDueReverseChg = ComputedByCW1.Box2_VATDueReverseChg + Adjustments.Box2_VATDueReverseChg + GroupMemberTotal.Box2_VATDueReverseChg;
				ValuesToSubmitToHMRC.Box4_VATReclaimed = ComputedByCW1.Box4_VATReclaimed + Adjustments.Box4_VATReclaimed + GroupMemberTotal.Box4_VATReclaimed;
				ValuesToSubmitToHMRC.Box6_TotalSalesExVAT = ComputedByCW1.Box6_TotalSalesExVAT + Adjustments.Box6_TotalSalesExVAT + GroupMemberTotal.Box6_TotalSalesExVAT;
				ValuesToSubmitToHMRC.Box7_TotalPurchaseExVAT = ComputedByCW1.Box7_TotalPurchaseExVAT + Adjustments.Box7_TotalPurchaseExVAT + GroupMemberTotal.Box7_TotalPurchaseExVAT;
				ValuesToSubmitToHMRC.Box8_GoodsSalesECMembersExVAT = ComputedByCW1.Box8_GoodsSalesECMembersExVAT + Adjustments.Box8_GoodsSalesECMembersExVAT + GroupMemberTotal.Box8_GoodsSalesECMembersExVAT;
				ValuesToSubmitToHMRC.Box9_GoodsPurchaseECMembersExVAT = ComputedByCW1.Box9_GoodsPurchaseECMembersExVAT + Adjustments.Box9_GoodsPurchaseECMembersExVAT + GroupMemberTotal.Box9_GoodsPurchaseECMembersExVAT;
			}
			else
			{
				ValuesToSubmitToHMRC.Box1_VATDue = ComputedByCW1.Box1_VATDue + UnsubmitedPreviousValues.Box1_VATDue + Adjustments.Box1_VATDue + GroupMemberTotal.Box1_VATDue;
				ValuesToSubmitToHMRC.Box2_VATDueReverseChg = ComputedByCW1.Box2_VATDueReverseChg + UnsubmitedPreviousValues.Box2_VATDueReverseChg + Adjustments.Box2_VATDueReverseChg + GroupMemberTotal.Box2_VATDueReverseChg;
				ValuesToSubmitToHMRC.Box4_VATReclaimed = ComputedByCW1.Box4_VATReclaimed + UnsubmitedPreviousValues.Box4_VATReclaimed + Adjustments.Box4_VATReclaimed + GroupMemberTotal.Box4_VATReclaimed;
				ValuesToSubmitToHMRC.Box6_TotalSalesExVAT = ComputedByCW1.Box6_TotalSalesExVAT + UnsubmitedPreviousValues.Box6_TotalSalesExVAT + Adjustments.Box6_TotalSalesExVAT + GroupMemberTotal.Box6_TotalSalesExVAT;
				ValuesToSubmitToHMRC.Box7_TotalPurchaseExVAT = ComputedByCW1.Box7_TotalPurchaseExVAT + UnsubmitedPreviousValues.Box7_TotalPurchaseExVAT + Adjustments.Box7_TotalPurchaseExVAT + GroupMemberTotal.Box7_TotalPurchaseExVAT;
				ValuesToSubmitToHMRC.Box8_GoodsSalesECMembersExVAT = ComputedByCW1.Box8_GoodsSalesECMembersExVAT + UnsubmitedPreviousValues.Box8_GoodsSalesECMembersExVAT + Adjustments.Box8_GoodsSalesECMembersExVAT + GroupMemberTotal.Box8_GoodsSalesECMembersExVAT;
				ValuesToSubmitToHMRC.Box9_GoodsPurchaseECMembersExVAT = ComputedByCW1.Box9_GoodsPurchaseECMembersExVAT + UnsubmitedPreviousValues.Box9_GoodsPurchaseECMembersExVAT + Adjustments.Box9_GoodsPurchaseECMembersExVAT + GroupMemberTotal.Box9_GoodsPurchaseECMembersExVAT;
			}
			ValuesToSubmitToHMRC.UpdateBox5_NetVATNotification();
		}

		bool IsOverThreshold
		{
			get
			{
				var netValueOfErrors = Math.Abs(UnsubmitedPreviousValues.Box5_NetVAT);

				if (netValueOfErrors < AccountingConfigurationRegistry.Instance.NetValueOfVATErrorsThresholdLowerLimit.Value)
				{
					return false;
				}
				else if (netValueOfErrors < AccountingConfigurationRegistry.Instance.NetValueOfVATErrorsThresholdUpperLimit.Value)
				{
					var box6Total = Math.Abs(ComputedByCW1.Box6_TotalSalesExVAT + UnsubmitedPreviousValues.Box6_TotalSalesExVAT + Adjustments.Box6_TotalSalesExVAT);
					if (netValueOfErrors < (box6Total / 100m * AccountingConfigurationRegistry.Instance.PercentageOfNetOutputs.Value))
					{
						return false;
					}
				}

				return true;
			}
		}

		public bool OverThresholdWarning => IsOverThreshold;

		public ZString CompanyName => ComplianceReport.Company.CompanyName;

		public ZString CompanyAddress1 => ComplianceReport.Company.Address1;

		public ZString CompanyAddress2 => ComplianceReport.Company.Address2;

		public ZString CompanyCountry => ComplianceReport.Company.Country.Description;

		public ZString CompanyCity => ComplianceReport.Company.City;

		public ZString CompanyPostCode => ComplianceReport.Company.Postcode;

		public ZString CompanyState => ComplianceReport.Company.State;

		public ZString GSTRegNo => MTDClient.GetVATRegistrationNumber(ComplianceReport.Company, ComplianceReport.ReportCountryCode);

		public ZString DeclarationHeaderText => Res.GetString("53be05e0-eb60-4eb3-bae2-eef97d310962", "Declaration to HMRC by {0} for {1} (VRN - {2}):", NameOfLoggedInUserOrLoginID, CompanyName, GSTRegNo);

		public ZDate PeriodStartDate => ComplianceReport.ACR_DateFrom;

		public ZDate PeriodEndDate => ComplianceReport.ACR_DateTo;

		[ReadOnly(true)]
		public ZDate ReturnDueDate { get; set; }

		public ZString TimeStamp => ZDateTime.Now.ToString();

		public ZString NameOfLoggedInUserOrLoginID => GlbStaff.CurrentUser.GS_FullName;

		public ZString ReceiptReferenceReceivedFromHMRC { get; set; }

		public ZString PeriodKey { get; set; }

		public ZString Status { get; set; }

		public static string DidNotIncludeChangesInCurrentVATReturnMessage => Res.GetString("c3c15715-4eb7-4277-8f3c-8a1f2f3ba670",
					@"You cannot submit VAT errors beyond the thresholds set along with the current VAT return. 

1.Net value of VAT errors less than £10000 for the period.
2.Net value of VAT errors larger than(or equal to) £10000, and less than £50000, AND not larger than 1% of ""box 6""(net outputs).

These values are not included in this VAT return.");

		public AdjustmentReasonHolder ReasonHolder1 { get; }

		public AdjustmentReasonHolder ReasonHolder2 { get; }

		public AdjustmentReasonHolder ReasonHolder4 { get; }

		public AdjustmentReasonHolder ReasonHolder6 { get; }

		public AdjustmentReasonHolder ReasonHolder7 { get; }

		public AdjustmentReasonHolder ReasonHolder8 { get; }

		public AdjustmentReasonHolder ReasonHolder9 { get; }

		public bool TooOldWarning { get; private set; }

		public ZBool Declaration { get; set; }

		public override bool ReadOnly
		{
			get { return base.ReadOnly || IsSubmitted || !(ReturnDueDate.IsValid || IsGroupMemberSubmission); }
			set { base.ReadOnly = value; }
		}

		FunctionalitySuspender SaveOnlyAdjustmentData =>
			saveOnlyAdjustmentData ?? (saveOnlyAdjustmentData = new FunctionalitySuspender());

		FunctionalitySuspender saveOnlyAdjustmentData;

		#region Implementations

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			var saveOnlyAdjustments = !SaveOnlyAdjustmentData.IsSuspended && !IsGroupMemberSubmission;
			CreateOrUpdateTaxReturn(saveOnlyAdjustments);

			base.OnFactorySavingBeforeTransactionCore();
		}

		#endregion
	}
}
