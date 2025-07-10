using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ES.Manifest.H7.Business.BusinessObjects.Interfaces;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class H7MessageSendingObject : MessageSendingObject, IH7CommonMessageSendingObject
	{
		public H7MessageSendingObject(AsycudaBill bill)
			: base(bill)
		{
			this.bill = bill;
		}

		protected override void SetDefaultData()
		{
			base.SetDefaultData();
			ShouldSend = !HasH7MRN;
		}

		protected override ZString DefaultAction => !HasH7MRN ? DeclarationMessageTypeList.Codes.H7Declaration : ZString.Empty;

		protected override CodeDescriptionPairList GetActionList()
		{
			var result = new CodeDescriptionPairList();

			if (!HasH7MRN)
			{
				result.AddPair(DeclarationMessageTypeList.Codes.H7Declaration, DeclarationMessageTypeList.Descriptions.H7Declaration);
			}
			else
			{
				result.AddPair(DeclarationMessageTypeList.Codes.H7Cancellation, DeclarationMessageTypeList.Descriptions.H7Cancellation);
				result.AddPair(DeclarationMessageTypeList.Codes.H7Query, DeclarationMessageTypeList.Descriptions.H7Query);
				result.AddPair(DeclarationMessageTypeList.Codes.H7ReExport, DeclarationMessageTypeList.Descriptions.H7ReExport);
			}

			return result;
		}

		[List(nameof(ActionList))]
		public override ZString Action
		{
			get => base.Action;
			set {
				var oldValue = base.Action;
				base.Action = value;

				if (oldValue != value)
				{
					OperationCode = value == DeclarationMessageTypeList.Codes.H7ReExport ? Business.OperationCodeList.Codes.InvalidateH7WithExsEtd : ZString.Empty;
				}
			}
		}

		public override ZString ActionForSendingCustomsDeclaration => DeclarationMessageTypeList.Codes.H7Declaration;

		bool HasH7MRN => !Bill.H7MovementReferenceNumber.IsEmpty;

		protected override bool OperationCode_ReadOnly => (Action != DeclarationMessageTypeList.Codes.H7ReExport);

		protected override CodeDescriptionPairList GetOperationCodeList() => Factory.GetCachedValue<OperationCodeList>();

		[ResourceStringData("NPBO:Enterprise.Customs.ES.Manifest.H7.Business.G3MessageSendingObject|G3LRN", Caption = "G3 Local Reference Number", ShortCaption = "LRN (G3)", FullDescription = "A system-generated local reference number to uniquely identify each single G3 declaration.")]
		public ZString G3LocalReferenceNumber => Bill.G3LocalReferenceNumber;

		public ZPropertyInfo G3LocalReferenceNumberInfo => GetZPropertyInfo(nameof(G3LocalReferenceNumber));

		[ResourceStringData("NPBO:Enterprise.Customs.ES.Manifest.H7.Business.G3MessageSendingObject|G3MRN", Caption = "G3 Movement Reference Number", ShortCaption = "MRN (G3)", FullDescription = "A unique identifier issued by the relevant Customs authority that enables the Customs authority to identify and process your shipment in the customs system.")]
		public ZString G3MovementReferenceNumber => Bill.G3MovementReferenceNumber;

		public ZPropertyInfo G3MovementReferenceNumberInfo => GetZPropertyInfo(nameof(G3MovementReferenceNumber));

		[ResourceStringData("NPBO:Enterprise.Customs.ES.Manifest.H7.Business.G3MessageSendingObject|H7MRN", Caption = "H7 Movement Reference Number", ShortCaption = "MRN (H7)", FullDescription = "A unique identifier issued by the relevant Customs authority that enables the Customs authority to identify and process your shipment in the customs system.")]
		public ZString H7MovementReferenceNumber => Bill.H7MovementReferenceNumber;

		public ZPropertyInfo H7MovementReferenceNumberInfo => GetZPropertyInfo(nameof(H7MovementReferenceNumber));

		public new MessageSendingObjectValidation Validation => (H7MessageSendingObjectValidation)base.Validation;

		public new AsycudaBill Bill => (AsycudaBill)base.Bill;

		AsycudaBill IH7CommonMessageSendingObject.Bill => bill;
		readonly AsycudaBill bill;

		protected override MessageSendingObjectValidation GetNewValidation() => new H7MessageSendingObjectValidation(this);
	}
}
