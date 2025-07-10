using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business
{
	public class PenaltyExemptionSessionalData : CusSupportingInfo
	{
		public PenaltyExemptionSessionalData(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : AutoCusSupportingInfo.Schema
		{
			public const int ApplyDutyPenaltyReductionMaxLength = 1;
			public const int PenaltyExemptionCodeMaxLength = 1;
		}

		[MaxLength(Schema.PenaltyExemptionCodeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(PenaltyExemptionSessionalDataLookups.YNCodeList))]
		[ReadOnlyMember(nameof(IsPenaltyExemptionCode_ReadOnly))]
		[ResourceStringData("F9826BC7-3530-4D65-95EB-7245E66DBC57", Caption = "Penalty Exempt Req.")]
		public ZString PenaltyExemptionCode
		{
			get => CSI_Code;
			set
			{
				CheckMaximumLength(PenaltyExemptionCodeInfo, value);
				CSI_Code = value;
				PenaltyExemptionCodeInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidatePenaltyExemptionCode();
				}
			}
		}
		public ZPropertyInfo PenaltyExemptionCodeInfo => GetZPropertyInfo(nameof(PenaltyExemptionCode));

		[MaxLength(Schema.ApplyDutyPenaltyReductionMaxLength)]
		[List(nameof(Lookups) + "." + nameof(PenaltyExemptionSessionalDataLookups.YNCodeList))]
		[ReadOnlyMember(nameof(IsDutyPenalty_ReadOnly))]
		[ResourceStringData("E9A12267-8E52-4933-A3F4-80B5E6656F2E", Caption = "DTY Penalty Reduced Y/N")]
		public ZString ApplyDutyPenaltyReduction
		{
			get => CSI_Procedure;
			set
			{
				CheckMaximumLength(ApplyDutyPenaltyReductionInfo, value);
				CSI_Procedure = value;
				ApplyDutyPenaltyReductionInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateApplyDutyPenaltyReduction();
				}
			}
		}
		public ZPropertyInfo ApplyDutyPenaltyReductionInfo => GetZPropertyInfo(nameof(ApplyDutyPenaltyReduction));

		[ResourceStringData("23615F52-CE81-4359-AC60-B6CCCC3D1A76", Caption = "Penalty Exempt Seq.")]
		public ZInt PenaltyExemptionRequestVersionNo => CSI_LineNo;

		public ZPropertyInfo PenaltyExemptionRequestVersionNoInfo => GetZPropertyInfo(nameof(PenaltyExemptionRequestVersionNo));

		[ResourceStringData("FEF9D13E-9C38-4B58-A842-63A3CB201C28", Caption = "Penalty Exemption Amount")]
		public ZDecimal PenaltyExemptionAmount => CSI_Value;

		public ZPropertyInfo PenaltyExemptionAmountInfo => GetZPropertyInfo(nameof(PenaltyExemptionAmount));

		[ResourceStringData("7834670E-F9B4-4265-B9F3-6F8C196736AD", Caption = "5UA Acceptance Date ")]
		public ZDateTime AcceptanceDate5UA => CSI_DateOfIssue;

		public ZPropertyInfo AcceptanceDate5UAInfo => GetZPropertyInfo(nameof(AcceptanceDate5UA));

		[ResourceStringData("594AD818-7DBF-471B-AE57-BE6B24AF9F1B", Caption = "5UA Entry Status")]
		public ZString EntryStatus5UA => CSI_Status;

		public ZPropertyInfo EntryStatus5UAInfo => GetZPropertyInfo(nameof(EntryStatus5UA));

		public ZBool IsDutyPenalty_ReadOnly => amendmentSessionalData.IsDutyPenalty_ReadOnly;
		public ZBool IsPenaltyExemptionCode_ReadOnly => IsDutyPenalty_ReadOnly && amendmentSessionalData.IsTaxPenalty_ReadOnly;
		AmendmentSessionalData amendmentSessionalData => Parent.AmendmentSessionalDataCollection.First();

		public new CusEntryInstruction Parent => (CusEntryInstruction)base.Parent;
		public new PenaltyExemptionSessionalDataLookups Lookups => (PenaltyExemptionSessionalDataLookups)base.Lookups;
		public new PenaltyExemptionSessionalDataValidation Validation => (PenaltyExemptionSessionalDataValidation)base.Validation;
		protected override CusSupportingInfoLookups GetNewLookups() => new PenaltyExemptionSessionalDataLookups(this);
		protected override CusSupportingInfoValidation GetNewValidation() => new PenaltyExemptionSessionalDataValidation(this);

		public void ResetVersionAndIndicator()
		{
			CSI_LineNo = ZInt.Zero;
			CSI_Code = ZString.Empty;
		}
	}
}
