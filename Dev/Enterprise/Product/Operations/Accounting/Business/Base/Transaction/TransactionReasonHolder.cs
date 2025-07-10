using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public enum TransactionReasonCategory
	{
		ReverseReason,
		AmendmentReason,
		CreditNoteReversalReason
	}

	public class TransactionReasonHolder : NonPersistentBusinessObject, IObsoleteValidation
	{
		public TransactionReasonHolder()
			: base(new BusinessObjectFactory())
		{
		}

		public TransactionReasonHolder(string existingReasonCode)
			: base(new BusinessObjectFactory())
		{
			fCode = existingReasonCode;
			fDescription = TransactionReasonCodes_List.GetDescriptionFromCode(fCode);
			fReason = fDescription;
		}

		public TransactionReasonHolder(ITransaction businessEntity, TransactionReasonCategory category = TransactionReasonCategory.ReverseReason)
			: base(new BusinessObjectFactory())
		{
			Category = category;
			if (businessEntity != null)
			{
				BusinessEntity = businessEntity;
				switch (Category)
				{
					case TransactionReasonCategory.ReverseReason:
						if (businessEntity is IReversing reversing)
						{
							fCode = reversing.ReversingCode;
							fReason = reversing.ReversingReason;
						}
						break;
					case TransactionReasonCategory.AmendmentReason:
						if (businessEntity is IAmending amending)
						{
							fCode = amending.AmendingReasonCode;
							fReason = amending.AmendingReason;
						}
						break;
				}
				fDescription = TransactionReasonCodes_List.GetDescriptionFromCode(fCode);
			}
		}

		public ITransaction BusinessEntity { get; private set; }

		public readonly TransactionReasonCategory Category = TransactionReasonCategory.ReverseReason;

		[MaxLength(3)]
		[List("TransactionReasonCodes_List")]
		public ZString Code
		{
			get { return fCode; }
			set
			{
				SetNonPersistentPropertyValue(CodeInfo, ref fCode, value);
				var readDescription = TransactionReasonCodes_List.GetDescriptionFromCode(Code);
				Description = readDescription != null ? ((ZString)readDescription).Left(DescriptionInfo.MaxLength) : null;
				Reason = Description;
			}
		}
		ZString fCode;

		public ZPropertyInfo CodeInfo
		{
			get { return GetZPropertyInfo(nameof(Code)); }
		}

		[MaxLength(50)]
		public ZString Description
		{
			get { return fDescription; }
			set { SetNonPersistentPropertyValue(DescriptionInfo, ref fDescription, value); }
		}
		ZString fDescription;

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(Description)); }
		}

		[MaxLength(200)]
		public ZString Reason
		{
			get { return fReason; }
			set { SetNonPersistentPropertyValue(ReasonInfo, ref fReason, value); }
		}
		ZString fReason;

		public ZPropertyInfo ReasonInfo
		{
			get { return GetZPropertyInfo(nameof(Reason)); }
		}

		[MaxLength(60)]
		public ZString SupportingDocumentNumber
		{
			get { return fSupportingDocumentNumber; }
			set { SetNonPersistentPropertyValue(SupportingDocumentNumberInfo, ref fSupportingDocumentNumber, value); }
		}
		ZString fSupportingDocumentNumber;

		public ZPropertyInfo SupportingDocumentNumberInfo
		{
			get { return GetZPropertyInfo(nameof(SupportingDocumentNumber)); }
		}

		public ZBool IsAmendInFull
		{
			get { return fIsAmendInFull; }
			set
			{
				SetNonPersistentPropertyValue(IsAmendInFullInfo, ref fIsAmendInFull, value);
			}
		}
		ZBool fIsAmendInFull;

		public ZPropertyInfo IsAmendInFullInfo
		{
			get { return GetZPropertyInfo(nameof(IsAmendInFull)); }
		}

		ReadOnlyCodeDescriptionPairList fTransactionReasonCodes_List;
		public ReadOnlyCodeDescriptionPairList TransactionReasonCodes_List
		{
#if DEBUG
			set
			{
				fTransactionReasonCodes_List = value;
			}
#endif
			get
			{
				if (fTransactionReasonCodes_List == null)
				{
					switch (Category)
					{
						case TransactionReasonCategory.ReverseReason:
							fTransactionReasonCodes_List = AccountingMasterFilesRegistry.Instance.ReversalReasonCodesList.Value;
							break;
						case TransactionReasonCategory.AmendmentReason:
							fTransactionReasonCodes_List = AccountingMasterFilesRegistry.Instance.AmendmentReasonCodesList.Value;
							break;
						case TransactionReasonCategory.CreditNoteReversalReason:
							fTransactionReasonCodes_List = AccountingMasterFilesRegistry.Instance.CreditNoteReasonCodesList.Value;
							break;
					}
				}
				return fTransactionReasonCodes_List;
			}
		}
	}
}
