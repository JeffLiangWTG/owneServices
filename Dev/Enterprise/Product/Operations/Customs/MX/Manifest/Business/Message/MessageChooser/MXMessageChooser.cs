using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.MX.Manifest.Business
{
	public sealed class MXMessageChooser : MessageChooser
	{
		public MXMessageChooser(ASYCUDA.Business.AsycudaManifestHeader header, IEnumerable<ISelectionItem> items, ZString messageSubType)
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
					return MXMessageConstants.Amend;
				}
				else if (MessageSubType == MessageSubTypeCodes.Codes.Cancellation)
				{
					return MXMessageConstants.Cancel;
				}
				else
				{
					return MXMessageConstants.Send;
				}
			}
		}

		public bool IsChangeOrCancellation => MessageSubType == MessageSubTypeCodes.Codes.Change || MessageSubType == MessageSubTypeCodes.Codes.Cancellation;

		public bool IsSeaMode => Header.AMA_TransportMode == Core.Constants.TransportModes.Sea;

		[List(nameof(Lookups) + "." + nameof(MXMessageChooserLookups.ReasonList))]
		[MaxLength(2)]
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

		#region Override

		public new MXMessageChooserLookups Lookups => (MXMessageChooserLookups)base.Lookups;

		public new MXMessageChooserValidation Validation => (MXMessageChooserValidation)base.Validation;

		protected override MessageChooserLookups GetNewLookups() => new MXMessageChooserLookups(this);

		protected override MessageChooserValidation GetNewValidation() => new MXMessageChooserValidation(this);

		#endregion
	}
}
