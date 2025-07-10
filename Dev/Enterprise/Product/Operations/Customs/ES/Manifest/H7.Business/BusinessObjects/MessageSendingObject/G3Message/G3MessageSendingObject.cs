using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ES.Manifest.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class G3MessageSendingObject : EU.H7.Business.MessageSendingObject
	{
		public G3MessageSendingObject(AsycudaBill bill, bool isRevoke = false) : base(bill)
		{
			Action = isRevoke ? G3MessageTypes.Codes.G3Revoke : HasG3MRN ? string.Empty : G3MessageTypes.Codes.G3Declaration;
			ShouldSend = isRevoke || !HasG3MRN;
			this.isRevoke = isRevoke;
		}

		readonly bool isRevoke;

		protected override void SetDefaultData()
		{
			base.SetDefaultData();
			RevokeReason = DefaultRevokeReason;
		}

		protected override CodeDescriptionPairList GetActionList()
		{
			var result = new CodeDescriptionPairList();
			if (!isRevoke && HasG3MRN)
			{
				result.AddPair(G3MessageTypes.Codes.G3Declaration, G3MessageTypes.Descriptions.G3Declaration);
			}

			return result;
		}

		protected override bool Action_ReadOnly => isRevoke || !HasG3MRN;

		bool HasG3MRN => !G3MovementReferenceNumber.IsEmpty;

		[List(nameof(RevokeReasonList))]
		public override ZString RevokeReason { get => base.RevokeReason; set => base.RevokeReason = value; }

		public ZString DefaultRevokeReason => ESH7G3RevokeReasonList.Codes.G3001;

		public CodeDescriptionPairList RevokeReasonList => Factory.GetCachedValue<ESH7G3RevokeReasonList>();

		[ResourceStringData("NPBO:Enterprise.Customs.ES.Manifest.H7.Business.G3MessageSendingObject|G3LRN", Caption = "G3 Local Reference Number", ShortCaption = "LRN (G3)", FullDescription = "A system-generated local reference number to uniquely identify each single G3 declaration.")]
		public ZString G3LocalReferenceNumber => Bill.G3LocalReferenceNumber;

		public ZPropertyInfo G3LocalReferenceNumberInfo => GetZPropertyInfo(nameof(G3LocalReferenceNumber));

		[ResourceStringData("NPBO:Enterprise.Customs.ES.Manifest.H7.Business.G3MessageSendingObject|G3MRN", Caption = "G3 Movement Reference Number", ShortCaption = "MRN (G3)", FullDescription = "A unique identifier issued by the relevant Customs authority that enables the Customs authority to identify and process your shipment in the customs system.")]
		public ZString G3MovementReferenceNumber => Bill.G3MovementReferenceNumber;

		public ZPropertyInfo G3MovementReferenceNumberInfo => GetZPropertyInfo(nameof(G3MovementReferenceNumber));

		[ResourceStringData("NPBO:Enterprise.Customs.ES.Manifest.H7.Business.G3MessageSendingObject|H7MRN", Caption = "H7 Movement Reference Number", ShortCaption = "MRN (H7)", FullDescription = "A unique identifier issued by the relevant Customs authority that enables the Customs authority to identify and process your shipment in the customs system.")]
		public ZString H7MovementReferenceNumber => Bill.H7MovementReferenceNumber;

		public ZPropertyInfo H7MovementReferenceNumberInfo => GetZPropertyInfo(nameof(H7MovementReferenceNumber));

		public new AsycudaBill Bill => (AsycudaBill)base.Bill;

		public new EU.H7.Business.MessageSendingObjectValidation Validation => (G3MessageSendingObjectValidation)base.Validation;

		protected override EU.H7.Business.MessageSendingObjectValidation GetNewValidation() => new G3MessageSendingObjectValidation(this);
	}
}
