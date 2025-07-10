using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IE.Business
{
	public class RefundApplicationMessageSendingAction : CusEntryHeaderMessageSendingAction
	{
		public RefundApplicationMessageSendingAction(CusEntryHeader cusEntryHeader) : base(cusEntryHeader) { }

		public override ZBool ShouldSend
		{
			get => base.ShouldSend;
			set
			{
				base.ShouldSend = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateAll();
					DocumentSendingObjectCollection.Cast<RefundApplicationDocumentSendingObject>().ForEach(x => x.Validation.ValidateAll());
				}
				DocumentSendingObjectCollection.RefreshBinding();
			}
		}

		[ResourceStringData("3D8A4258-0FB1-4BD1-939C-F816217B23D0", ShortCaption = "MRN", Caption = "Movement Reference Number (MRN)")]
		[ReadOnly(true)]
		public ZString MovementReferenceNumber => EntryHeader.MovementReferenceNumber;

		public ZPropertyInfo MovementReferenceNumberInfo => GetZPropertyInfo(nameof(MovementReferenceNumber));

		[ResourceStringData("800999AF-6ADF-467D-A78E-D3EC64CB3A35", Caption = "Type")]
		[List(nameof(Lookups) + "." + nameof(RefundApplicationMessageSendingActionLookups.RefundTypeList))]
		public ZString RefundType
		{
			get { return fRefundType; }
			set
			{
				SetNonPersistentPropertyValue(RefundTypeInfo, ref fRefundType, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateRefundType();
				}
			}
		}
		ZString fRefundType;

		public ZPropertyInfo RefundTypeInfo => GetZPropertyInfo(nameof(RefundType));

		[ResourceStringData("DB5BE8B9-5B42-428B-BA2E-4978249429F4", Caption = "Office of Debt", FullDescription = "Customs office where the debt was notified.")]
		[List(nameof(Lookups) + "." + nameof(RefundApplicationMessageSendingActionLookups.CustomsOfficesList))]
		public ZString OfficeOfDebt
		{
			get { return fOfficeOfDebt; }
			set
			{
				SetNonPersistentPropertyValue(OfficeOfDebtInfo, ref fOfficeOfDebt, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateOfficeOfDebt();
				}
			}
		}
		ZString fOfficeOfDebt;

		public ZPropertyInfo OfficeOfDebtInfo => GetZPropertyInfo(nameof(OfficeOfDebt));

		[ResourceStringData("26E2CFB1-6AA3-4F54-9CFD-82E1147FD5C9", Caption = "Office of Responsibility", FullDescription = "Customs office responsible for the place where the goods are located.")]
		[List(nameof(Lookups) + "." + nameof(RefundApplicationMessageSendingActionLookups.CustomsOfficesList))]
		public ZString OfficeOfResponsibility
		{
			get { return fOfficeOfResponsibility; }
			set
			{
				SetNonPersistentPropertyValue(OfficeOfResponsibilityInfo, ref fOfficeOfResponsibility, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateOfficeOfResponsibility();
				}
			}
		}
		ZString fOfficeOfResponsibility;

		public ZPropertyInfo OfficeOfResponsibilityInfo => GetZPropertyInfo(nameof(OfficeOfResponsibility));

		[ResourceStringData("330F042C-14F8-4211-BC84-0B9F35594487", Caption = "Legal Basis")]
		[List(nameof(Lookups) + "." + nameof(RefundApplicationMessageSendingActionLookups.LegalBasisList))]
		public ZString LegalBasis
		{
			get { return fLegalBasis; }
			set
			{
				if (SetNonPersistentPropertyValue(LegalBasisInfo, ref fLegalBasis, value))
				{
					SetDefaultDescriptionOfGroundsValue();
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateLegalBasis();
				}
			}
		}

		void SetDefaultDescriptionOfGroundsValue()
		{
			if (!LegalBasis.IsEmpty && DescriptionOfGrounds.IsEmpty)
			{
				DescriptionOfGrounds = RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusCodeListTypes.Codes.LegalBasisCode, ZDateTime.Today).GetDescriptionFromCode(LegalBasis);
			}
		}

		ZString fLegalBasis;

		public ZPropertyInfo LegalBasisInfo => GetZPropertyInfo(nameof(LegalBasis));

		[ResourceStringData("DF0C97B5-B8AA-4892-A4F2-768EBC665039", Caption = "Description of Grounds")]
		[MaxLength(512)]
		public ZString DescriptionOfGrounds
		{
			get { return fDescriptionOfGrounds; }
			set
			{
				SetNonPersistentPropertyValue(DescriptionOfGroundsInfo, ref fDescriptionOfGrounds, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateDescriptionOfGrounds();
				}
			}
		}
		ZString fDescriptionOfGrounds;

		public ZPropertyInfo DescriptionOfGroundsInfo => GetZPropertyInfo(nameof(DescriptionOfGrounds));

		[ResourceStringData("D23FFD6F-7ED5-4BC3-9B36-CFE320039A2A", Caption = "Bank Details")]
		[MaxLength(512)]
		public ZString BankDetails
		{
			get { return fBankDetails; }
			set
			{
				SetNonPersistentPropertyValue(BankDetailsInfo, ref fBankDetails, value);
			}
		}
		ZString fBankDetails;

		public ZPropertyInfo BankDetailsInfo => GetZPropertyInfo(nameof(BankDetails));

		[ResourceStringData("7CF4AA25-F7C1-49E7-9557-CCC7E13E5210", Caption = "Amount")]
		[DecimalPrecision(16)]
		[DecimalPlaces(2)]
		public ZDecimal Amount
		{
			get { return fAmount; }
			set
			{
				SetNonPersistentPropertyValue(AmountInfo, ref fAmount, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateAmount();
				}
			}
		}
		ZDecimal fAmount;

		public ZPropertyInfo AmountInfo => GetZPropertyInfo(nameof(Amount));

		[ResourceStringData("8E1251E7-985C-4E2E-9AA5-34F9A761C22B", Caption = "Additional Information")]
		[MaxLength(512)]
		public ZString AdditionalInformation
		{
			get { return fAdditionalInformation; }
			set
			{
				SetNonPersistentPropertyValue(AdditionalInformationInfo, ref fAdditionalInformation, value);
			}
		}
		ZString fAdditionalInformation;

		public ZPropertyInfo AdditionalInformationInfo => GetZPropertyInfo(nameof(AdditionalInformation));

		RefundApplicationDocumentSendingObjectCollection documentSendingObjectCollection;
		public RefundApplicationDocumentSendingObjectCollection DocumentSendingObjectCollection
		{
			get
			{
				if (documentSendingObjectCollection == null)
				{
					documentSendingObjectCollection = new RefundApplicationDocumentSendingObjectCollection(EntryHeader, this);
					RegisterEditableChildObject(documentSendingObjectCollection);
				}
				return documentSendingObjectCollection;
			}
		}

		public new RefundApplicationMessageSendingActionValidation Validation => (RefundApplicationMessageSendingActionValidation)base.Validation;

		public new RefundApplicationMessageSendingActionLookups Lookups => (RefundApplicationMessageSendingActionLookups)base.Lookups;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			MessageType = AISOutgoingMessageTypeList.Codes.ApplicationForRemissionOfCustomsDebtF15;
			RefundType = AISRefundTypeList.Codes.REP;
		}

		protected override Type SenderType => typeof(RefundApplicationMessageSender);

		protected override CusEntryHeaderMessageSendingActionLookups GetNewLookups() => new RefundApplicationMessageSendingActionLookups(this);

		protected override CusEntryHeaderMessageSendingActionValidation GetNewValidation() => new RefundApplicationMessageSendingActionValidation(this);
	}
}
