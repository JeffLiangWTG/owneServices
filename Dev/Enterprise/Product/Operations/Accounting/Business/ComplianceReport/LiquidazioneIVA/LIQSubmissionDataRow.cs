using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport.HMRC;

using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ComplianceReport.LiquidazioneIVA
{
	public enum RowType
	{
		TotalVatBaseReceivables = 1,
		TotalVatReceivables = 2,
		TotalVatBasePayables = 3,
		TotalVatPayablesRecoverable = 4,
		TotalVatPayablesNotRecoverable = 5,
		BalancePreviousPeriod = 7
	}

	public class LIQSubmissionDataRow : NonPersistentBusinessObject
	{
		public LIQSubmissionDataRow(BusinessObjectFactory factory, RowType rowNumber, string rowText, LIQSubmissionDataColumns submissionData) : base(factory)
		{
			RowNo = rowNumber;
			RowText = rowText;
			SubmissionData = submissionData;
			switch (RowNo)
			{
				case RowType.TotalVatBaseReceivables:
					CW1Amount = SubmissionData.ComputedByCW1.Box1_TotalVatBaseReceivables;
					AdjustmentsAmount = SubmissionData.Adjustments.Box1_TotalVatBaseReceivables;
					TotalAmount = SubmissionData.ValuesToSubmit.Box1_TotalVatBaseReceivables;
					break;
				case RowType.TotalVatReceivables:
					CW1Amount = SubmissionData.ComputedByCW1.Box2_TotalVatReceivables;
					AdjustmentsAmount = SubmissionData.Adjustments.Box2_TotalVatReceivables;
					TotalAmount = SubmissionData.ValuesToSubmit.Box2_TotalVatReceivables;
					break;
				case RowType.TotalVatBasePayables:
					CW1Amount = SubmissionData.ComputedByCW1.Box3_TotalVatBasePayables;
					AdjustmentsAmount = SubmissionData.Adjustments.Box3_TotalVatBasePayables;
					TotalAmount = SubmissionData.ValuesToSubmit.Box3_TotalVatBasePayables;
					break;
				case RowType.TotalVatPayablesRecoverable:
					CW1Amount = SubmissionData.ComputedByCW1.Box4_TotalVatPayablesRecoverable;
					AdjustmentsAmount = SubmissionData.Adjustments.Box4_TotalVatPayablesRecoverable;
					TotalAmount = SubmissionData.ValuesToSubmit.Box4_TotalVatPayablesRecoverable;
					break;
				case RowType.TotalVatPayablesNotRecoverable:
					CW1Amount = SubmissionData.ComputedByCW1.Box5_TotalVatPayablesNotRecoverable;
					AdjustmentsAmount = SubmissionData.Adjustments.Box5_TotalVatPayablesNotRecoverable;
					TotalAmount = SubmissionData.ValuesToSubmit.Box5_TotalVatPayablesNotRecoverable;
					break;
				case RowType.BalancePreviousPeriod:
					CW1Amount = SubmissionData.ComputedByCW1.Box7_BalancePreviousPeriod;
					AdjustmentsAmount = SubmissionData.Adjustments.Box7_BalancePreviousPeriod;
					TotalAmount = SubmissionData.ValuesToSubmit.Box7_BalancePreviousPeriod;
					break;
			}
		}

		public ZString RowText { get; }

		RowType RowNo { get; }

		LIQSubmissionDataColumns SubmissionData { get; }

		public AdjustmentReasonHolder ReasonHolder => reasonHolder ?? (reasonHolder = GetReasonHolder());
		AdjustmentReasonHolder reasonHolder;

		AdjustmentReasonHolder GetReasonHolder()
		{
			var result = new AdjustmentReasonHolder(() => AdjustmentsAmount);
			RegisterEditableChildObject(result);
			result.ReadOnly = ReadOnly;

			using (result.GetValidationSuspender())
			{
				switch (RowNo)
				{
					case RowType.TotalVatBaseReceivables:
						result.CopyValuesFrom(SubmissionData.ReasonHolder1);
						break;
					case RowType.TotalVatReceivables:
						result.CopyValuesFrom(SubmissionData.ReasonHolder2);
						break;
					case RowType.TotalVatBasePayables:
						result.CopyValuesFrom(SubmissionData.ReasonHolder3);
						break;
					case RowType.TotalVatPayablesRecoverable:
						result.CopyValuesFrom(SubmissionData.ReasonHolder4);
						break;
					case RowType.TotalVatPayablesNotRecoverable:
						result.CopyValuesFrom(SubmissionData.ReasonHolder5);
						break;
					case RowType.BalancePreviousPeriod:
						result.CopyValuesFrom(SubmissionData.ReasonHolder7);
						break;
				}
			}
			return result;
		}

		int LocalDecimals => GlbCompany.CurrentCompany.GetLocalDecimals();

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal CW1Amount { get; }

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
				TotalAmount = CW1Amount + adjustmentsAmount;
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
				case RowType.TotalVatBaseReceivables:
					SubmissionData.Adjustments.Box1_TotalVatBaseReceivables = AdjustmentsAmount;
					SubmissionData.ReasonHolder1.CopyValuesFrom(ReasonHolder);
					break;
				case RowType.TotalVatReceivables:
					SubmissionData.Adjustments.Box2_TotalVatReceivables = AdjustmentsAmount;
					SubmissionData.ReasonHolder2.CopyValuesFrom(ReasonHolder);
					break;
				case RowType.TotalVatBasePayables:
					SubmissionData.Adjustments.Box3_TotalVatBasePayables = AdjustmentsAmount;
					SubmissionData.ReasonHolder3.CopyValuesFrom(ReasonHolder);
					break;
				case RowType.TotalVatPayablesRecoverable:
					SubmissionData.Adjustments.Box4_TotalVatPayablesRecoverable = AdjustmentsAmount;
					SubmissionData.ReasonHolder4.CopyValuesFrom(ReasonHolder);
					break;
				case RowType.TotalVatPayablesNotRecoverable:
					SubmissionData.Adjustments.Box5_TotalVatPayablesNotRecoverable = AdjustmentsAmount;
					SubmissionData.ReasonHolder5.CopyValuesFrom(ReasonHolder);
					break;
				case RowType.BalancePreviousPeriod:
					SubmissionData.Adjustments.Box7_BalancePreviousPeriod = AdjustmentsAmount;
					SubmissionData.ReasonHolder7.CopyValuesFrom(ReasonHolder);
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
