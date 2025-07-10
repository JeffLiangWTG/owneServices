using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ComplianceReport.HMRC
{
	public class MTDSubmissionDataRow : NonPersistentBusinessObject
	{
		public MTDSubmissionDataRow(BusinessObjectFactory factory, int rowNumber, string rowText, MTDSubmissionDataColumns submissionData) : base(factory)
		{
			RowNo = rowNumber;
			RowText = rowText;
			SubmissionData = submissionData;
			switch (RowNo)
			{
				case 1:
					CW1Amount = SubmissionData.ComputedByCW1.Box1_VATDue;
					ErrorsAmount = SubmissionData.UnsubmitedPreviousValues.Box1_VATDue;
					AdjustmentsAmount = SubmissionData.Adjustments.Box1_VATDue;
					TotalAmount = SubmissionData.ValuesToSubmitToHMRC.Box1_VATDue;
					break;
				case 2:
					CW1Amount = SubmissionData.ComputedByCW1.Box2_VATDueReverseChg;
					ErrorsAmount = SubmissionData.UnsubmitedPreviousValues.Box2_VATDueReverseChg;
					AdjustmentsAmount = SubmissionData.Adjustments.Box2_VATDueReverseChg;
					TotalAmount = SubmissionData.ValuesToSubmitToHMRC.Box2_VATDueReverseChg;
					break;
				case 4:
					CW1Amount = SubmissionData.ComputedByCW1.Box4_VATReclaimed;
					ErrorsAmount = SubmissionData.UnsubmitedPreviousValues.Box4_VATReclaimed;
					AdjustmentsAmount = SubmissionData.Adjustments.Box4_VATReclaimed;
					TotalAmount = SubmissionData.ValuesToSubmitToHMRC.Box4_VATReclaimed;
					break;
				case 6:
					CW1Amount = SubmissionData.ComputedByCW1.Box6_TotalSalesExVAT;
					ErrorsAmount = SubmissionData.UnsubmitedPreviousValues.Box6_TotalSalesExVAT;
					AdjustmentsAmount = SubmissionData.Adjustments.Box6_TotalSalesExVAT;
					TotalAmount = SubmissionData.ValuesToSubmitToHMRC.Box6_TotalSalesExVAT;
					break;
				case 7:
					CW1Amount = SubmissionData.ComputedByCW1.Box7_TotalPurchaseExVAT;
					ErrorsAmount = SubmissionData.UnsubmitedPreviousValues.Box7_TotalPurchaseExVAT;
					AdjustmentsAmount = SubmissionData.Adjustments.Box7_TotalPurchaseExVAT;
					TotalAmount = SubmissionData.ValuesToSubmitToHMRC.Box7_TotalPurchaseExVAT;
					break;
				case 8:
					CW1Amount = SubmissionData.ComputedByCW1.Box8_GoodsSalesECMembersExVAT;
					ErrorsAmount = SubmissionData.UnsubmitedPreviousValues.Box8_GoodsSalesECMembersExVAT;
					AdjustmentsAmount = SubmissionData.Adjustments.Box8_GoodsSalesECMembersExVAT;
					TotalAmount = SubmissionData.ValuesToSubmitToHMRC.Box8_GoodsSalesECMembersExVAT;
					break;
				case 9:
					CW1Amount = SubmissionData.ComputedByCW1.Box9_GoodsPurchaseECMembersExVAT;
					ErrorsAmount = SubmissionData.UnsubmitedPreviousValues.Box9_GoodsPurchaseECMembersExVAT;
					AdjustmentsAmount = SubmissionData.Adjustments.Box9_GoodsPurchaseECMembersExVAT;
					TotalAmount = SubmissionData.ValuesToSubmitToHMRC.Box9_GoodsPurchaseECMembersExVAT;
					break;
			}
		}

		public ZString RowText { get; }

		int RowNo { get; }

		MTDSubmissionDataColumns SubmissionData { get; }

		public AdjustmentReasonHolder ReasonHolder => reasonHolder ?? (reasonHolder = GetReasonHolder());
		AdjustmentReasonHolder reasonHolder;

		AdjustmentReasonHolder GetReasonHolder()
		{
			var result = new AdjustmentReasonHolder(() => AdjustmentsAmount);
			this.RegisterEditableChildObject(result);
			result.ReadOnly = ReadOnly;

			using (result.GetValidationSuspender())
			{
				switch (RowNo)
				{
					case 1:
						result.CopyValuesFrom(SubmissionData.ReasonHolder1);
						break;
					case 2:
						result.CopyValuesFrom(SubmissionData.ReasonHolder2);
						break;
					case 4:
						result.CopyValuesFrom(SubmissionData.ReasonHolder4);
						break;
					case 6:
						result.CopyValuesFrom(SubmissionData.ReasonHolder6);
						break;
					case 7:
						result.CopyValuesFrom(SubmissionData.ReasonHolder7);
						break;
					case 8:
						result.CopyValuesFrom(SubmissionData.ReasonHolder8);
						break;
					case 9:
						result.CopyValuesFrom(SubmissionData.ReasonHolder9);
						break;
				}
			}
			return result;
		}

		int LocalDecimals => 2;

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal CW1Amount { get; }

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal ErrorsAmount { get; }

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal AdjustmentsAmount
		{
			get
			{
				return adjustmentsAmount;
			}
			set
			{
				SetNonPersistentPropertyValue(AdjustmentsAmountInfo, ref adjustmentsAmount, value);
				AdjustmentsAmountInfo.RefreshBinding();
				TotalAmount = CW1Amount + ErrorsAmount + adjustmentsAmount;
				TotalAmountInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo AdjustmentsAmountInfo
		{
			get { return GetZPropertyInfo(nameof(AdjustmentsAmount)); }
		}
		ZDecimal adjustmentsAmount;

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal TotalAmount
		{
			get { return totalAmount; }
			set { SetNonPersistentPropertyValue(TotalAmountInfo, ref totalAmount, value); }
		}
		public ZPropertyInfo TotalAmountInfo
		{
			get { return GetZPropertyInfo(nameof(TotalAmount)); }
		}
		ZDecimal totalAmount;

		public void AdjustmentAproved()
		{
			switch (RowNo)
			{
				case 1:
					SubmissionData.Adjustments.Box1_VATDue = AdjustmentsAmount;
					SubmissionData.ReasonHolder1.CopyValuesFrom(ReasonHolder);
					break;
				case 2:
					SubmissionData.Adjustments.Box2_VATDueReverseChg = AdjustmentsAmount;
					SubmissionData.ReasonHolder2.CopyValuesFrom(ReasonHolder);
					break;
				case 4:
					SubmissionData.Adjustments.Box4_VATReclaimed = AdjustmentsAmount;
					SubmissionData.ReasonHolder4.CopyValuesFrom(ReasonHolder);
					break;
				case 6:
					SubmissionData.Adjustments.Box6_TotalSalesExVAT = AdjustmentsAmount;
					SubmissionData.ReasonHolder6.CopyValuesFrom(ReasonHolder);
					break;
				case 7:
					SubmissionData.Adjustments.Box7_TotalPurchaseExVAT = AdjustmentsAmount;
					SubmissionData.ReasonHolder7.CopyValuesFrom(ReasonHolder);
					break;
				case 8:
					SubmissionData.Adjustments.Box8_GoodsSalesECMembersExVAT = AdjustmentsAmount;
					SubmissionData.ReasonHolder8.CopyValuesFrom(ReasonHolder);
					break;
				case 9:
					SubmissionData.Adjustments.Box9_GoodsPurchaseECMembersExVAT = AdjustmentsAmount;
					SubmissionData.ReasonHolder9.CopyValuesFrom(ReasonHolder);
					break;
			}
		}

		public override bool ReadOnly
		{
			get { return SubmissionData.ReadOnly ? SubmissionData.ReadOnly : base.ReadOnly; }
			set { base.ReadOnly = value; }
		}
	}
}