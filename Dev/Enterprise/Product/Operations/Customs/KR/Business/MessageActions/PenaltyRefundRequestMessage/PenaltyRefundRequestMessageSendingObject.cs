using System.ComponentModel;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.KR.Business
{
	public class PenaltyRefundRequestMessageSendingObject : JobDeclarationMessageSendingObject
	{
		public PenaltyRefundRequestMessageSendingObject(CusEntryHeader entry, ZString customsDisbursementBillNumber, CusEntryNumber refundEntryNumber) : base(entry, ElectronicDocumentTypeList.Codes._5UL)
		{
			this.CustomsDisbursementBillNumber = customsDisbursementBillNumber;
			this.RefundEntryNumber = refundEntryNumber;
			SetDefaultRefundAmount();
		}
		public readonly ZString CustomsDisbursementBillNumber;
		public readonly CusEntryNumber RefundEntryNumber;

		[ResourceStringData("5F635FB8-FDE1-455F-A82C-75EFA98960CE", Caption = "Customs Disbursement Bill #")]
		public ZString FormattedCustomsDisbursementBillNumber => MessageFunctions.GetFormattedNumber(CustomsDisbursementBillNumber, new int[] { 0, 4, 7, 9, 11, 12, 18 });

		#region PenaltyToRefund
		public ZDecimal DutyPenaltyToRefund
		{
			get => dutyPenaltyToRefund;
			set => SetNonPersistentPropertyValue(DutyPenaltyToRefundInfo, ref dutyPenaltyToRefund, value);
		}
		ZDecimal dutyPenaltyToRefund;
		public ZPropertyInfo DutyPenaltyToRefundInfo => this.GetZPropertyInfo(nameof(DutyPenaltyToRefund));

		public ZDecimal LQTPenaltyToRefund
		{
			get => lqtPenaltyToRefund;
			set => SetNonPersistentPropertyValue(LQTPenaltyToRefundInfo, ref lqtPenaltyToRefund, value);
		}
		ZDecimal lqtPenaltyToRefund;
		public ZPropertyInfo LQTPenaltyToRefundInfo => this.GetZPropertyInfo(nameof(LQTPenaltyToRefund));

		public ZDecimal SCTPenaltyToRefund
		{
			get => sctPenaltyToRefund;
			set => SetNonPersistentPropertyValue(SCTPenaltyToRefundInfo, ref sctPenaltyToRefund, value);
		}
		ZDecimal sctPenaltyToRefund;
		public ZPropertyInfo SCTPenaltyToRefundInfo => this.GetZPropertyInfo(nameof(SCTPenaltyToRefund));

		public ZDecimal TRTPenaltyToRefund
		{
			get => trtPenaltyToRefund;
			set => SetNonPersistentPropertyValue(TRTPenaltyToRefundInfo, ref trtPenaltyToRefund, value);
		}
		ZDecimal trtPenaltyToRefund;
		public ZPropertyInfo TRTPenaltyToRefundInfo => this.GetZPropertyInfo(nameof(TRTPenaltyToRefund));

		public ZDecimal EDTPenaltyToRefund
		{
			get => edtPenaltyToRefund;
			set => SetNonPersistentPropertyValue(EDTPenaltyToRefundInfo, ref edtPenaltyToRefund, value);
		}
		ZDecimal edtPenaltyToRefund;
		public ZPropertyInfo EDTPenaltyToRefundInfo => this.GetZPropertyInfo(nameof(EDTPenaltyToRefund));

		public ZDecimal AGTPenaltyToRefund
		{
			get => agtPenaltyToRefund;
			set => SetNonPersistentPropertyValue(AGTPenaltyToRefundInfo, ref agtPenaltyToRefund, value);
		}
		ZDecimal agtPenaltyToRefund;
		public ZPropertyInfo AGTPenaltyToRefundInfo => this.GetZPropertyInfo(nameof(AGTPenaltyToRefund));

		public ZDecimal VATPenaltyToRefund
		{
			get => vatPenaltyToRefund;
			set => SetNonPersistentPropertyValue(VATPenaltyToRefundInfo, ref vatPenaltyToRefund, value);
		}
		ZDecimal vatPenaltyToRefund;
		public ZPropertyInfo VATPenaltyToRefundInfo => this.GetZPropertyInfo(nameof(VATPenaltyToRefund));
		#endregion

		#region RefundAmount
		public ZDecimal DutyRefundAmount
		{
			get => dutyRefundAmount;
			set => SetNonPersistentPropertyValue(DutyRefundAmountInfo, ref dutyRefundAmount, value);
		}
		ZDecimal dutyRefundAmount;
		public ZPropertyInfo DutyRefundAmountInfo => this.GetZPropertyInfo(nameof(DutyRefundAmount));

		public ZDecimal LQTRefundAmount
		{
			get => lqtRefundAmount;
			set => SetNonPersistentPropertyValue(LQTRefundAmountInfo, ref lqtRefundAmount, value);
		}
		ZDecimal lqtRefundAmount;
		public ZPropertyInfo LQTRefundAmountInfo => this.GetZPropertyInfo(nameof(LQTRefundAmount));

		public ZDecimal SCTRefundAmount
		{
			get => sctRefundAmount;
			set => SetNonPersistentPropertyValue(SCTRefundAmountInfo, ref sctRefundAmount, value);
		}
		ZDecimal sctRefundAmount;
		public ZPropertyInfo SCTRefundAmountInfo => this.GetZPropertyInfo(nameof(SCTRefundAmount));

		public ZDecimal TRTRefundAmount
		{
			get => trtRefundAmount;
			set => SetNonPersistentPropertyValue(TRTRefundAmountInfo, ref trtRefundAmount, value);
		}
		ZDecimal trtRefundAmount;
		public ZPropertyInfo TRTRefundAmountInfo => this.GetZPropertyInfo(nameof(TRTRefundAmount));

		public ZDecimal EDTRefundAmount
		{
			get => edtRefundAmount;
			set => SetNonPersistentPropertyValue(EDTRefundAmountInfo, ref edtRefundAmount, value);
		}
		ZDecimal edtRefundAmount;
		public ZPropertyInfo EDTRefundAmountInfo => this.GetZPropertyInfo(nameof(EDTRefundAmount));

		public ZDecimal AGTRefundAmount
		{
			get => agtRefundAmount;
			set => SetNonPersistentPropertyValue(AGTRefundAmountInfo, ref agtRefundAmount, value);
		}
		ZDecimal agtRefundAmount;
		public ZPropertyInfo AGTRefundAmountInfo => this.GetZPropertyInfo(nameof(AGTRefundAmount));

		public ZDecimal VATRefundAmount
		{
			get => vatRefundAmount;
			set => SetNonPersistentPropertyValue(VATRefundAmountInfo, ref vatRefundAmount, value);
		}
		ZDecimal vatRefundAmount;
		public ZPropertyInfo VATRefundAmountInfo => this.GetZPropertyInfo(nameof(VATRefundAmount));

		[ReadOnly(true)]
		public ZDecimal ValueForVATRefundAmount { get; set; }

		[ReadOnly(true)]
		public ZDecimal VATExemptionValueRefundAmount { get; set; }

		public ZDecimal PenaltyForLateDeclarationRefundAmount
		{
			get => penaltyForLateDeclarationRefundAmount;
			set => SetNonPersistentPropertyValue(PenaltyForLateDeclarationRefundAmountInfo, ref penaltyForLateDeclarationRefundAmount, value);
		}
		ZDecimal penaltyForLateDeclarationRefundAmount;
		public ZPropertyInfo PenaltyForLateDeclarationRefundAmountInfo => this.GetZPropertyInfo(nameof(PenaltyForLateDeclarationRefundAmount));

		public ZDecimal PenaltyForMissedDeclarationRefundAmount
		{
			get => penaltyForMissedDeclarationRefundAmount;
			set => SetNonPersistentPropertyValue(PenaltyForMissedDeclarationRefundAmountInfo, ref penaltyForMissedDeclarationRefundAmount, value);
		}
		ZDecimal penaltyForMissedDeclarationRefundAmount;
		public ZPropertyInfo PenaltyForMissedDeclarationRefundAmountInfo => this.GetZPropertyInfo(nameof(PenaltyForMissedDeclarationRefundAmount));

		public ZDecimal LatePaymentRefundAmount
		{
			get => latePaymentRefundAmount;
			set => SetNonPersistentPropertyValue(LatePaymentRefundAmountInfo, ref latePaymentRefundAmount, value);
		}
		ZDecimal latePaymentRefundAmount;
		public ZPropertyInfo LatePaymentRefundAmountInfo => this.GetZPropertyInfo(nameof(LatePaymentRefundAmount));

		public ZDecimal NonDutyTaxRefundAmount
		{
			get => nonDutyTaxRefundAmount;
			set => SetNonPersistentPropertyValue(NonDutyTaxRefundAmountInfo, ref nonDutyTaxRefundAmount, value);
		}
		ZDecimal nonDutyTaxRefundAmount;
		public ZPropertyInfo NonDutyTaxRefundAmountInfo => this.GetZPropertyInfo(nameof(NonDutyTaxRefundAmount));

		[ResourceStringData("38C07EC9-65AE-4707-AD54-A8A4B66E4707", ShortCaption = "5WN Version No.", Caption = "5WN Version Number")]
		public ZShort Amendment5WNNumber
		{
			get => amendment5WNNumber;
			set => SetNonPersistentPropertyValue(Amendment5WNNumberInfo, ref amendment5WNNumber, value);
		}
		ZShort amendment5WNNumber;
		public ZPropertyInfo Amendment5WNNumberInfo => this.GetZPropertyInfo(nameof(Amendment5WNNumber));
		#endregion

		void SetDefaultRefundAmount()
		{
			var orderedEntryLines = Header.MergedLines.Cast<CusEntryLine>().OrderBy(x => x.CL_LineNumber);
			foreach (CusEntryLine entryLine in orderedEntryLines)
			{
				ValueForVATRefundAmount += entryLine.CL_ValueForVAT;
				VATExemptionValueRefundAmount += entryLine.CL_ValueExemptForVAT;
			}

			var chargeSums = Header.Charges
							.Where(x => x.C1_RateOverrideReasonCode.IsEmpty)
							.GroupBy(x => x.C1_ChargeType)
							.ToDictionary(g => g.Key, g => g.Sum(x => x.C1_ChargeAmount));

			dutyRefundAmount = chargeSums.GetValueOrDefault(ChargeTypeList.Codes.Duty);
			lqtRefundAmount = chargeSums.GetValueOrDefault(ChargeTypeList.Codes.LiquorTax);
			sctRefundAmount = chargeSums.GetValueOrDefault(ChargeTypeList.Codes.SpecialConsumptionTax);
			trtRefundAmount = chargeSums.GetValueOrDefault(ChargeTypeList.Codes.TransportationTax);
			edtRefundAmount = chargeSums.GetValueOrDefault(ChargeTypeList.Codes.EducationTax);
			agtRefundAmount = chargeSums.GetValueOrDefault(ChargeTypeList.Codes.AgricultureTax);
			vatRefundAmount = chargeSums.GetValueOrDefault(ChargeTypeList.Codes.VAT);
			penaltyForLateDeclarationRefundAmount = chargeSums.GetValueOrDefault(ChargeTypeList.Codes.PenaltyForLateDeclaration);
			penaltyForMissedDeclarationRefundAmount = chargeSums.GetValueOrDefault(ChargeTypeList.Codes.PenaltyForMissedDeclaration);
		}

		CusStatementLine StatementLine
		{
			get
			{
				if (statementLine == null)
				{
					statementLine = Header.PaidStatementLines.FirstOrDefault(x => x.IndividualCustomsDisbursementBillNo == CustomsDisbursementBillNumber);
				}
				return statementLine;
			}
		}
		CusStatementLine statementLine;

		CusStatementHeader StatementHeader
		{
			get
			{
				if (statementHeader == null && StatementLine != null)
				{
					statementHeader = StatementLine.StatementHeader;
				}
				return statementHeader;
			}
		}
		CusStatementHeader statementHeader;

		public ZString BillStatus
		{
			get
			{
				var result = ZString.Empty;
				if (StatementHeader != null)
				{
					result = StatementHeader.B2_Status + " (" + StatementHeader.B2_StatusName + ")";
				}
				return result;
			}
		}
		public ZString PaymentStatus
		{
			get
			{
				var result = ZString.Empty;
				if (StatementHeader != null)
				{
					result = StatementHeader.B2_PaymentStatus + " (" + StatementHeader.B2_PaymentStatusName + ")";
				}
				return result;
			}
		}

		public AmendmentSessionalData AmendmentSessionalData => Header.EntryInstruction?.AmendmentSessionalDataCollection.Cast<AmendmentSessionalData>().FirstOrDefault(x => x.VersionNumber5WN == Amendment5WNNumber);
		[ResourceStringData("CE1F73AC-CA64-4ECF-B96F-FAF88386529F", Caption = "Submission Date")]
		public ZDateTime SubmissionDate => AmendmentSessionalData?.CSI_DateOfIssue ?? ZDateTime.Empty;
		public ZString AmendmentVersionNoCW1 => AmendmentSessionalData?.CSI_LineNo.ToString(Constants.IdNumbersFormatConstants.D2) ?? ZString.Empty;
		[ResourceStringData("BD3A48B6-6B0C-45B9-B991-1ECB75750BD8", ShortCaption = "Amend Version No.", Caption = "Amend Version Number")]
		public ZShort AmendmentVersionNoCustoms
		{
			get
			{
				var result = ZShort.Zero;
				if (!AmendmentVersionNoCW1.IsEmpty && !SubmissionDate.IsEmpty)
				{
					result = Header.GetCustoms5FEVersionNumber(ZShort.ParseSafe(AmendmentVersionNoCW1, ZShort.Zero), (ZDate)SubmissionDate);
				}
				return result;
			}
		}

		[ResourceStringData("26139695-DDA4-42DD-A71C-C690383D4A95", Caption = "Payment Amount")]
		public ZDecimal PaymentAmount => StatementLine?.B3_CustomsFeesTotal ?? ZDecimal.Zero;

		[ResourceStringData("9CC22940-312F-4D27-B024-32AC7851D754", Caption = "Refund Type")]
		public ZString RefundType => RefundTypeList.Codes.A;

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(PenaltyRefundRequestMessageSendingObjectLookups.RefundCauseCodeList))]
		[ResourceStringData("DC07EC5F-F05B-432E-AA11-DDC2C7CA79FD", Caption = "Refund Cause")]
		public ZString RefundCause
		{
			get => refundCause;
			set
			{
				SetNonPersistentPropertyValue(RefundCauseInfo, ref refundCause, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateRefundCause();
				}
			}
		}
		ZString refundCause;
		public ZPropertyInfo RefundCauseInfo => this.GetZPropertyInfo(nameof(RefundCause));

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(PenaltyRefundRequestMessageSendingObjectLookups.RefundReasonCodeList))]
		[ResourceStringData("250EF7A1-CC9C-4A76-9969-B0F02399BF67", Caption = "Refund Reason")]
		public ZString RefundReason
		{
			get => refundReason;
			set
			{
				SetNonPersistentPropertyValue(RefundReasonInfo, ref refundReason, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateRefundReason();
				}
			}
		}
		ZString refundReason;
		public ZPropertyInfo RefundReasonInfo => this.GetZPropertyInfo(nameof(RefundReason));

		[MaxLength(3)]
		[List(nameof(Lookups) + "." + nameof(PenaltyRefundRequestMessageSendingObjectLookups.TaxOfficeList))]
		[ResourceStringData("DA8CB83B-4B6B-46AE-A743-A7047EB715EF", Caption = "Tax Office")]
		public ZString TaxOffice
		{
			get => Header.Declaration.JE_TaxOffice;
			set
			{
				CheckMaximumLength(TaxOfficeInfo, value);
				Header.Declaration.JE_TaxOffice = value;
				TaxOfficeInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateTaxOffice();
				}
			}
		}
		public ZPropertyInfo TaxOfficeInfo => this.GetZPropertyInfo(nameof(TaxOffice));

		public PenaltyRefundRequestMessageSendingObjectLookups Lookups => new PenaltyRefundRequestMessageSendingObjectLookups(this);

		public new PenaltyRefundRequestMessageSendingObjectValidation Validation => (PenaltyRefundRequestMessageSendingObjectValidation)base.Validation;
		protected override Customs.Business.JobDeclarationMessageSendingObjectValidation GetNewValidation() => new PenaltyRefundRequestMessageSendingObjectValidation(this);
	}
}
