using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business
{
	public class DepositRefundApplicationMessageSendingAction : CusEntryHeaderMessageSendingAction
	{
		public DepositRefundApplicationMessageSendingAction(CusEntryHeader entryHeader) : base(entryHeader)
		{
			using (GetValidationSuspender())
			using (SuspendSettingHasChanges())
			{
				SetMessageSendingDefaultValues();
			}
		}

		public override ZBool ShouldSend
		{
			get => base.ShouldSend;
			set
			{
				base.ShouldSend = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateAll();
				}
			}
		}

		[ResourceStringData("B1C9217B-A201-4EA1-AE35-CEC5FE7C66A1", Caption = "Import MRN")]
		public ZString MovementReferenceNumber => EntryHeader.MovementReferenceNumber;

		public ZPropertyInfo MovementReferenceNumberInfo => GetZPropertyInfo(nameof(MovementReferenceNumber));

		[ResourceStringData("1161F662-BDF3-42BC-A341-2FFC9DDF242D", Caption = "Export MRN")]
		[List(nameof(Lookups) + "." + nameof(DepositRefundApplicationMessageSendingActionLookups.ExportMovementReferenceNumberList))]
		public ZString ExportMovementReferenceNumber
		{
			get { return fExportMovementReferenceNumber; }
			set
			{
				SetNonPersistentPropertyValue(ExportMovementReferenceNumberInfo, ref fExportMovementReferenceNumber, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateExportMovementReferenceNumber();
				}
			}
		}
		ZString fExportMovementReferenceNumber;

		public ZPropertyInfo ExportMovementReferenceNumberInfo => GetZPropertyInfo(nameof(ExportMovementReferenceNumber));

		[ResourceStringData("DA00E3F4-FC7F-4039-B53F-2B88D9D680DD", Caption = "Export Date")]
		public ZDateTime ExportDate
		{
			get { return fExportDate; }
			set
			{
				SetNonPersistentPropertyValue(ExportDateInfo, ref fExportDate, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateExportDate();
				}
			}
		}
		ZDateTime fExportDate;

		public ZPropertyInfo ExportDateInfo => GetZPropertyInfo(nameof(ExportDate));

		[ResourceStringData("CDEAD1A0-7A52-4502-AA1C-3582504F561F", Caption = "Customs Duty")]
		[DecimalPrecision(16)]
		[DecimalPlaces(2)]
		public ZDecimal CustomsDuty
		{
			get { return fCustomsDuty; }
			set
			{
				SetNonPersistentPropertyValue(CustomsDutyInfo, ref fCustomsDuty, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateCustomsDuty();
				}
			}
		}
		ZDecimal fCustomsDuty;

		public ZPropertyInfo CustomsDutyInfo => GetZPropertyInfo(nameof(CustomsDuty));

		[ResourceStringData("FD69E5C5-32BE-48CA-AF5B-3457AA8D4A3C", Caption = "VAT")]
		[DecimalPrecision(16)]
		[DecimalPlaces(2)]
		public ZDecimal Vat
		{
			get { return fVat; }
			set
			{
				SetNonPersistentPropertyValue(VatInfo, ref fVat, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateVat();
				}
			}
		}
		ZDecimal fVat;

		public ZPropertyInfo VatInfo => GetZPropertyInfo(nameof(Vat));

		[ResourceStringData("F2326101-CACD-48FA-B6A9-9D5EF155BECD", Caption = "Other Duties")]
		[DecimalPrecision(16)]
		[DecimalPlaces(2)]
		public ZDecimal OtherDuties
		{
			get { return fOtherDuties; }
			set
			{
				SetNonPersistentPropertyValue(OtherDutiesInfo, ref fOtherDuties, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateOtherDuties();
				}
			}
		}
		ZDecimal fOtherDuties;

		public ZPropertyInfo OtherDutiesInfo => GetZPropertyInfo(nameof(OtherDuties));

		[ResourceStringData("804C76F2-C20C-4019-B02D-A248918D04EF", Caption = "Imported Goods Discharged")]
		public ZBool ImportedGoodsDischarged
		{
			get { return fImportedGoodsDischarged; }
			set
			{
				SetNonPersistentPropertyValue(ImportedGoodsDischargedInfo, ref fImportedGoodsDischarged, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateImportedGoodsDischarged();
				}
			}
		}
		ZBool fImportedGoodsDischarged;

		public ZPropertyInfo ImportedGoodsDischargedInfo => GetZPropertyInfo(nameof(ImportedGoodsDischarged));

		[ResourceStringData("2A52DD5F-E546-4C96-9D1F-1C6752DE9BA2", Caption = "Outstanding Balance", FullDescription = "Outstanding balance on imported goods.")]
		[DecimalPrecision(16)]
		[DecimalPlaces(2)]
		public ZDecimal OutstandingBalance
		{
			get { return fOutstandingBalance; }
			set
			{
				SetNonPersistentPropertyValue(OutstandingBalanceInfo, ref fOutstandingBalance, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateOutstandingBalance();
				}
			}
		}
		ZDecimal fOutstandingBalance;

		public ZPropertyInfo OutstandingBalanceInfo => GetZPropertyInfo(nameof(OutstandingBalance));

		[ResourceStringData("DC958803-A5BF-4534-8616-48CBFCB46BAE", Caption = "Amount of Deposit Refund")]
		[DecimalPrecision(16)]
		[DecimalPlaces(2)]
		public ZDecimal AmountOfDepositRefund
		{
			get { return fAmountOfDepositRefund; }
			set
			{
				SetNonPersistentPropertyValue(AmountOfDepositRefundInfo, ref fAmountOfDepositRefund, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateAmountOfDepositRefund();
				}
			}
		}
		ZDecimal fAmountOfDepositRefund;

		public ZPropertyInfo AmountOfDepositRefundInfo => GetZPropertyInfo(nameof(AmountOfDepositRefund));

		[ResourceStringData("D504C743-9E35-43A3-92CE-554C52414F45", Caption = "Payer EORI", FullDescription = "Payer EORI for Refund.")]
		[MaxLength(17)]
		public ZString PayerEori
		{
			get { return fPayerEori; }
			set
			{
				SetNonPersistentPropertyValue(PayerEoriInfo, ref fPayerEori, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidatePayerEori();
				}
			}
		}
		ZString fPayerEori;
		public ZPropertyInfo PayerEoriInfo => GetZPropertyInfo(nameof(PayerEori));

		[ResourceStringData("05D53124-0983-4090-B3EC-81EC82687A98", Caption = "Period for Discharge")]
		[MaxLength(2)]
		public ZInt PeriodForDischarge
		{
			get { return fPeriodForDischarge; }
			set
			{
				SetNonPersistentPropertyValue(PeriodForDischargeInfo, ref fPeriodForDischarge, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidatePeriodForDischarge();
				}
			}
		}
		ZInt fPeriodForDischarge;

		public ZPropertyInfo PeriodForDischargeInfo => GetZPropertyInfo(nameof(PeriodForDischarge));

		[ResourceStringData("A87CD2CD-0027-4A13-B6E3-928F64F02A34", Caption = "Rate of Yield")]
		[MaxLength(512)]
		public ZString RateOfYield
		{
			get { return fRateOfYield; }
			set
			{
				SetNonPersistentPropertyValue(RateOfYieldInfo, ref fRateOfYield, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateRateOfYield();
				}
			}
		}
		ZString fRateOfYield;

		public ZPropertyInfo RateOfYieldInfo => GetZPropertyInfo(nameof(RateOfYield));

		public new DepositRefundApplicationMessageSendingActionLookups Lookups => (DepositRefundApplicationMessageSendingActionLookups)base.Lookups;

		public new DepositRefundApplicationMessageSendingActionValidation Validation => (DepositRefundApplicationMessageSendingActionValidation)base.Validation;

		protected override Type SenderType => typeof(DepositRefundApplicationMessageSender);

		protected override CusEntryHeaderMessageSendingActionLookups GetNewLookups() => new DepositRefundApplicationMessageSendingActionLookups(this);

		protected override CusEntryHeaderMessageSendingActionValidation GetNewValidation() => new DepositRefundApplicationMessageSendingActionValidation(this);

		void SetMessageSendingDefaultValues()
		{
			MessageType = AISOutgoingMessageTypeList.Codes.ApplicationForRemissionOfCustomsDebtR15;
			SetDefaultMovementRefenceNumber();
			SetDefaultExportDate();
			SetDefaultPayerEori();
			SetDefaultPeriodForDischarge();
			SetDefaultRateOfYield();
		}

		void SetDefaultMovementRefenceNumber()
		{
			if (EntryHeader.EntryInstruction is CusEntryInstruction entryInstruction)
			{
				ExportMovementReferenceNumber = GetDefaultMovementReferenceNumber();
			}

			ZString GetDefaultMovementReferenceNumber() => entryInstruction.PreviousDocuments.Where(pd => pd.CSI_Code == Constants.PreviousDocumentTypeCodes.MRN).Select(pd => pd.CSI_ReferenceNumber).FirstOrDefault();
		}

		void SetDefaultExportDate()
		{
			if (EntryHeader.Declaration is JobDeclaration declaration)
			{
				ExportDate = declaration.JE_ExportDate;
			}
		}

		void SetDefaultPayerEori()
		{
			if (EntryHeader.Declaration is JobDeclaration declaration && declaration.DutyPayer != null)
			{
				PayerEori = declaration.DutyPayer.GetEoriNumber(declaration.GetDefaultDataGroupingCode());
			}
		}

		void SetDefaultPeriodForDischarge()
		{
			if (EntryHeader.EntryInstruction is CusEntryInstruction instruction)
			{
				PeriodForDischarge = instruction.ZG_PeriodForDischarge;
			}
		}

		void SetDefaultRateOfYield()
		{
			if (EntryHeader.EntryInstruction is CusEntryInstruction instruction)
			{
				RateOfYield = instruction.PeriodForDischargeDetails;
			}
		}
	}
}
