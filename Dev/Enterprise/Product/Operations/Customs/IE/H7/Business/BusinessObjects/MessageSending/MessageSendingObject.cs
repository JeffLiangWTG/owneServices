using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.H7.Business
{
	public class MessageSendingObject : EU.H7.Business.MessageSendingObject, IH7MessageSendingObject
	{
		public MessageSendingObject(EU.H7.Business.AsycudaBill bill)
			: base(bill)
		{
		}

		protected override EU.H7.Business.MessageSendingObjectValidation GetNewValidation() => new MessageSendingObjectValidation(this);

		protected override void SetDefaultData()
		{
			subStyleCached = Bill.ABL_ShipmentType;
			base.SetDefaultData();
		}

		protected override ZString DefaultAction => MessageStatus.IsEmpty ? AISOutgoingMessageTypeList.Codes.CustomsDeclaration : ZString.Empty;

		public override ZString Action
		{
			get
			{
				return base.Action;
			}
			set
			{
				base.Action = value;
				if (base.Action != ZString.Empty)
				{
					UpdateSubStyleIfNeeded();
					UpdateAmendmentInvalidationReasonIfNeeded();
				}
			}
		}

		public override MessageSender CreateSender()
		{
			return new IEMessageSender(this);
		}

		void UpdateSubStyleIfNeeded()
		{
			if (Action == AISOutgoingMessageTypeList.Codes.CustomsDeclaration || Action == AISOutgoingMessageTypeList.Codes.AmendmentRequest)
			{
				SubStyle = subStyleCached.IsEmpty ? SubStyleCodeList.Codes.NormalDeclaration : subStyleCached;
			}
			else
			{
				SubStyle = ZString.Empty;
			}
		}

		ZString subStyleCached;

		void UpdateAmendmentInvalidationReasonIfNeeded()
		{
			if (Action == AISOutgoingMessageTypeList.Codes.InvalidationRequest || Action == AISOutgoingMessageTypeList.Codes.AmendmentRequest)
			{
				isAmendmentInvalidationReasonEditable = true;
			}
			else
			{
				isAmendmentInvalidationReasonEditable = false;
			}
			AmendmentInvalidationReason = ZString.Empty;
		}

		[ResourceStringData("Enterprise.Customs.IE.H7.Business.MessageSendingObject|SubStyle", Caption = "Additional Declaration Type", ShortCaption = "Add. Decl. Type", FullDescription = "[11 02 001 000] Additional Declaration Type")]
		public override ZString SubStyle
		{
			get
			{
				return Bill.ABL_ShipmentType;
			}
			set
			{
				if (value != Bill.ABL_ShipmentType)
				{
					CheckMaximumLength(SubStyleInfo, value);
					Bill.ABL_ShipmentType = value;
					Validation.ValidateSubStyle();
					SubStyleInfo.RefreshBinding();
				}
			}
		}

		public override ZString ActionForSendingCustomsDeclaration => AISOutgoingMessageTypeList.Codes.CustomsDeclaration;

		protected override CodeDescriptionPairList GetActionList() => Factory.GetCachedValue("IE.H7.MessageSendingObject.AISOutgoingMessageTypeList", () =>
		{
			var result = new AISOutgoingMessageTypeList();
			result.RemoveCode(AISOutgoingMessageTypeList.Codes.D3ElectronicTransportDocument);
			result.RemoveCode(AISOutgoingMessageTypeList.Codes.EntryIntoTheDeclarantRecords);
			result.RemoveCode(AISOutgoingMessageTypeList.Codes.ProvideSupportingDocuments);
			result.RemoveCode(AISOutgoingMessageTypeList.Codes.DocumentsReceived);
			result.RemoveCode(AISOutgoingMessageTypeList.Codes.ApplicationForRemissionOfCustomsDebtR15);
			result.RemoveCode(AISOutgoingMessageTypeList.Codes.ApplicationForRemissionOfCustomsDebtF15);
			return result;
		});

		protected override CodeDescriptionPairList GetSubStyleList() => Factory.GetCachedValue<SubStyleCodeList>();

		protected override bool AmendmentInvalidationReason_ReadOnly => !isAmendmentInvalidationReasonEditable;

		bool isAmendmentInvalidationReasonEditable;
	}
}
