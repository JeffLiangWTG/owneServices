using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public sealed class CLMessageChooser : MessageChooser
	{
		public CLMessageChooser(ASYCUDA.Business.AsycudaManifestHeader header, IEnumerable<ISelectionItem> items, ZString messageSubType)
			: base(header, items, true)
		{
			MessageSubType = messageSubType;
		}
		public ZString MessageSubType { get; }

		public ZString ActionType
		{
			get
			{
				if (MessageSubType == MessageSubTypeCodes.Codes.Change)
				{
					return CLMessageConstants.Amend;
				}
				else if (MessageSubType == MessageSubTypeCodes.Codes.Cancellation)
				{
					return CLMessageConstants.Cancel;
				}
				else
				{
					return CLMessageConstants.Send;
				}
			}
		}

		public bool IsCancellation => MessageSubType == MessageSubTypeCodes.Codes.Cancellation;

		public bool IsAmendment => MessageSubType == MessageSubTypeCodes.Codes.Change;

		[MaxLength(255)]
		public ZString Reason
		{
			get { return reason; }
			set
			{
				if (reason != value)
				{
					CheckMaximumLength(ReasonInfo, value);
					reason = value;

					Validation.ValidateReason();
					ReasonInfo.RefreshBinding();
				}
			}
		}
		ZString reason;

		public ZPropertyInfo ReasonInfo { get { return GetZPropertyInfo(nameof(Reason)); } }

		[List(nameof(Lookups) + "." + nameof(CLMessageChooserLookups.AmendReasonList))]
		[MaxLength(5)]
		public ZString AmendReason
		{
			get { return amendReason; }
			set
			{
				if (amendReason != value)
				{
					CheckMaximumLength(AmendReasonInfo, value);
					amendReason = value;

					Validation.ValidateAmendReason();
					AmendReasonInfo.RefreshBinding();
				}
			}
		}
		ZString amendReason;

		public ZPropertyInfo AmendReasonInfo { get { return GetZPropertyInfo(nameof(AmendReason)); } }

		[List(nameof(Lookups) + "." + nameof(CLMessageChooserLookups.AmendTypeList))]
		[MaxLength(1)]
		public ZString AmendType
		{
			get { return amendType; }
			set
			{
				if (amendType != value)
				{
					CheckMaximumLength(AmendTypeInfo, value);
					amendType = value;

					Validation.ValidateAmendType();
					AmendTypeInfo.RefreshBinding();
				}
			}
		}
		ZString amendType;

		public ZPropertyInfo AmendTypeInfo { get { return GetZPropertyInfo(nameof(AmendType)); } }

		#region Override

		public new CLMessageChooserValidation Validation => (CLMessageChooserValidation)base.Validation;

		protected override MessageChooserValidation GetNewValidation() => new CLMessageChooserValidation(this);

		public new CLMessageChooserLookups Lookups => (CLMessageChooserLookups)base.Lookups;

		protected override MessageChooserLookups GetNewLookups() => new CLMessageChooserLookups(this);

		#endregion
	}
}
