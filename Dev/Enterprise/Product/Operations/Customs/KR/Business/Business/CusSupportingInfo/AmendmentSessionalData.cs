using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class AmendmentSessionalData : CusSupportingInfo
	{
		public AmendmentSessionalData(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : AutoCusSupportingInfo.Schema
		{
			public const int DutyTaxPenaltyCauseCodeMaxLength = 2;
		}

		[ResourceStringData("CEF64EE9-54CB-4B58-8779-F0CE79458BB0", Caption = "Amendment Type")]
		public ZString AmendmentType => CSI_Code;

		[ResourceStringData("8D7C3557-61E2-441E-80CB-91A34520C377", Caption = "5FE CW1 Version No.")]
		public ZInt AmendmentCW1VersionNo => CSI_LineNo;

		CusEntryHeader Entry => Parent?.EntryHeader;
		public EDIMessage Message5FE => Entry?.Messages.GetLastMessageWithMatchingVersionNumber(ElectronicDocumentTypeList.Codes._5FE, AmendmentCW1VersionNo.ToString());

		[ResourceStringData("21A5BB01-1749-45B8-A1A6-5C67F3AF33C2", Caption = "5FE Customs Version No.")]
		public ZShort AmendmentCustomsVersionNo
		{
			get
			{
				var result = ZShort.Zero;
				if (Message5FE != null)
				{
					result = Entry.GetCustoms5FEVersionNumber(Message5FE);
				}
				else if (Entry != null)
				{
					result = Entry.GetCustoms5FEVersionNumber(Entry.CH_VersionID + 1, ZDateTime.Today);
				}
				return result;
			}
		}

		[ResourceStringData("2A7F015E-998B-4806-B1E9-2558002BC3E1", Caption = "5FE Message No.")]
		public ZString MessageNum5FE => Message5FE?.EM_MessageNum ?? ZString.Empty;

		[ResourceStringData("D4E2B924-7D09-494F-9B52-AEF1DC158A43", Caption = "5FE Entry Status")]
		public ZString AmendmentEntryStatus => CSI_Status;

		[ResourceStringData("81B179A5-3307-454F-A137-D5C094CBE15C", Caption = "5FE Accept Date")]
		public ZDateTime AcceptanceDate5FE => CSI_DateOfIssue;

		[ResourceStringData("3D6DABE0-92B5-461B-A3C3-8EC1EFCFEE62", Caption = "5WN Version No.")]
		public ZInt VersionNumber5WN => CSI_ItemNumber;

		public ZBool IsDutyPenalty_ReadOnly => !IsPenaltyRelevant(DutyDifference);
		public ZBool IsTaxPenalty_ReadOnly => !IsPenaltyRelevant(TaxDifference);

		ZBool IsPenaltyRelevant(ZDecimal differenceOfDutyOrTax)
		{
			var result = false;
			if (Entry != null)
			{
				var amendmentCanBeSent = !CustomsMessageStatusTypeList.IsAmendmentMessageSentOrAccepted(MessageStatus);
				result = amendmentCanBeSent && differenceOfDutyOrTax > 0 && Entry.IsValidDueDate && Entry.Statement929.B2_DueDate.IsInThePastDatePartOnly;
			}
			return result;
		}

		public ZString MessageStatus
		{
			get
			{
				var result = ZString.Empty;
				if (Entry != null)
				{
					if (AmendmentCW1VersionNo == Entry.CH_VersionID + 1)
					{
						result = Entry.CH_Status;
					}
					else
					{
						result = Message5FE?.MessageStatus ?? ZString.Empty;
					}
				}
				return result;
			}
		}

		[ResourceStringData("BEC1689E-7509-4A54-AE4D-19103315BE35", Caption = "Duty Difference")]
		public ZDecimal DutyDifference => Entry?.Charges.Where(x => x.C1_RateOverrideReasonCode.IsEmpty && x.C1_ChargeType == ChargeTypeList.Codes.Duty)?.Sum(x => x.C1_ChargeAmount) ?? ZDecimal.Zero;

		[MaxLength(Schema.DutyTaxPenaltyCauseCodeMaxLength)]
		[ReadOnlyMember(nameof(IsDutyPenalty_ReadOnly))]
		[List(nameof(Lookups) + "." + nameof(AmendmentSessionalDataLookups.DutyPenaltyCauseList))]
		[ResourceStringData("8779DE28-062D-4DB0-82E1-11F99177F253", Caption = "DTY Penalty Type")]
		public ZString DutyPenaltyCause
		{
			get => CSI_SubType;
			set
			{
				CheckMaximumLength(DutyPenaltyCauseInfo, value);
				CSI_SubType = value;
				DutyPenaltyCauseInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateDutyPenaltyCause();
				}
			}
		}
		public ZPropertyInfo DutyPenaltyCauseInfo => GetZPropertyInfo(nameof(DutyPenaltyCause));

		[ResourceStringData("537A769B-55D3-40EA-873E-B32AC92D40EF", Caption = "Domestic Tax Difference")]
		public ZDecimal TaxDifference => Entry?.Charges.Where(x => x.C1_RateOverrideReasonCode.IsEmpty && ChargeTypeList.GetDomesticTaxTypes().Contains(x.C1_ChargeType))?.Sum(x => x.C1_ChargeAmount) ?? ZDecimal.Zero;

		[MaxLength(Schema.DutyTaxPenaltyCauseCodeMaxLength)]
		[ReadOnlyMember(nameof(IsTaxPenalty_ReadOnly))]
		[List(nameof(Lookups) + "." + nameof(AmendmentSessionalDataLookups.TaxPenaltyCauseList))]
		[ResourceStringData("9EA0F168-A1AB-452A-9A14-88A8830A1BF5", Caption = "Domestic Tax Penalty Type")]
		public ZString TaxPenaltyCause
		{
			get => CSI_Procedure;
			set
			{
				CheckMaximumLength(TaxPenaltyCauseInfo, value);
				CSI_Procedure = value;
				TaxPenaltyCauseInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateTaxPenaltyCause();
				}
			}
		}
		public ZPropertyInfo TaxPenaltyCauseInfo => GetZPropertyInfo(nameof(TaxPenaltyCause));

		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set
			{
				var oldValue = base.CSI_Code;
				base.CSI_Code = value;
				if (oldValue != value)
				{
					Reset5UASessionalData();
					if (DutyTaxCorrectionCodeList.Is5UARelevant(value))
					{
						PenaltyExemptionSessionalDataCollection.AddNew();
					}
				}
			}
		}

		public ZString RefundDeclarationNumber => RefundSessionalData?.CSI_ReferenceNumber ?? ZString.Empty;

		void Reset5UASessionalData()
		{
			PenaltyExemptionSessionalDataCollection.RemoveAndDeleteAll();
			penaltyExemptionSessionalData = null;
		}

		public ZDecimal PenaltyAmountFrom5FK
		{
			get
			{
				var result = ZDecimal.Zero;

				if (Message5FE != null && Entry != null)
				{
					result = Entry.GetPenaltyExemptionAmount(Message5FE);
				}

				return result;
			}
		} 

		public PenaltyExemptionSessionalData ValidPenaltyExemptionSessionalData
		{
			get
			{
				var sessionalData = PenaltyExemptionSessionalData?.CSI_Code ?? DutyPenaltyExemptionCodeList.Codes.X;
				if (validPenaltyExemptionSessionalData == null && (sessionalData == DutyPenaltyExemptionCodeList.Codes.Y || sessionalData == DutyPenaltyExemptionCodeList.Codes.N))
				{
					validPenaltyExemptionSessionalData = PenaltyExemptionSessionalData;
				}
				return validPenaltyExemptionSessionalData;
			}
		}
		PenaltyExemptionSessionalData validPenaltyExemptionSessionalData;

		public PenaltyExemptionSessionalData PenaltyExemptionSessionalData
		{
			get
			{
				if (penaltyExemptionSessionalData?.IsDeleted ?? true)
				{
					penaltyExemptionSessionalData = PenaltyExemptionSessionalDataCollection.FirstOrDefault();
				}
				return penaltyExemptionSessionalData;
			}
		}
		PenaltyExemptionSessionalData penaltyExemptionSessionalData;

		[ChildEditable(true)]
		PenaltyExemptionSessionalDataCollection PenaltyExemptionSessionalDataCollection
		{
			get
			{
				if (penaltyExemptionSessionalDataCollection == null)
				{
					penaltyExemptionSessionalDataCollection = new PenaltyExemptionSessionalDataCollection(Parent, PK);
					penaltyExemptionSessionalDataCollection.Load();
					RegisterEditableChildObject(penaltyExemptionSessionalDataCollection);
				}
				return penaltyExemptionSessionalDataCollection;
			}
		}
		PenaltyExemptionSessionalDataCollection penaltyExemptionSessionalDataCollection;

		public RefundSessionalData RefundSessionalData
		{
			get
			{
				if (refundSessionalData?.IsDeleted ?? true)
				{
					refundSessionalData = RefundSessionalDataCollection.FirstOrDefault();
				}
				return refundSessionalData;
			}
		}
		RefundSessionalData refundSessionalData;

		[ChildEditable(true)]
		public RefundSessionalDataCollection RefundSessionalDataCollection
		{
			get
			{
				if (refundSessionalDataCollection == null)
				{
					refundSessionalDataCollection = new RefundSessionalDataCollection(Parent, PK);
					refundSessionalDataCollection.Load();
					RegisterEditableChildObject(refundSessionalDataCollection);
				}
				return refundSessionalDataCollection;
			}
		}
		RefundSessionalDataCollection refundSessionalDataCollection;

		public new CusEntryInstruction Parent => (CusEntryInstruction)base.Parent;
		public new AmendmentSessionalDataLookups Lookups => (AmendmentSessionalDataLookups)base.Lookups;
		public new AmendmentSessionalDataValidation Validation => (AmendmentSessionalDataValidation)base.Validation;
		protected override CusSupportingInfoLookups GetNewLookups() => new AmendmentSessionalDataLookups(this);
		protected override CusSupportingInfoValidation GetNewValidation() => new AmendmentSessionalDataValidation(this);
	}
}
