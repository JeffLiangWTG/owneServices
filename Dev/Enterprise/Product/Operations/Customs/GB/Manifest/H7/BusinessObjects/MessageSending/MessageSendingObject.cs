using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.H7.Business
{
	public class MessageSendingObject : EU.H7.Business.MessageSendingObject, IH7MessageSendingObject
	{
		public MessageSendingObject(EU.H7.Business.AsycudaBill bill)
			: base(bill)
		{
		}

		protected override ZString DefaultAction => H7EDIMessageTypeList.Codes.NewDeclaration;

		protected override CodeDescriptionPairList GetActionList() => Factory.GetCachedValue<H7EDIMessageTypeList>();

		protected override CodeDescriptionPairList GetSubStyleList() => Factory.GetCachedValue<EntrySubStyleListImport>();

		protected override CodeDescriptionPairList GetAmendmentReasonCodeList() => Factory.GetCachedValue("GB.H7.MessageSendingObject.GBH7AmendmentReasonCodeList", () =>
		{
			var result = base.GetAmendmentReasonCodeList();
			result.AddPair(AmendmentCancellationReasonCode.Codes.C_NotRequired, AmendmentCancellationReasonCode.Descriptions.C_NotRequired);
			result.AddPair(AmendmentCancellationReasonCode.Codes.C_Duplicate, AmendmentCancellationReasonCode.Descriptions.C_Duplicate);
			result.AddPair(AmendmentCancellationReasonCode.Codes.C_Other, AmendmentCancellationReasonCode.Descriptions.C_Other);
			result.AddPair(AmendmentCancellationReasonCode.Codes.C_GoodsPresentationNotice, AmendmentCancellationReasonCode.Descriptions.C_GoodsPresentationNotice);
			return result;
		});

		protected override CodeDescriptionPairList GetQueryTypeList() => Factory.GetCachedValue<H7QueryTypeList>();

		public override ZString Action
		{
			get => base.Action;
			set
			{
				if (base.Action != value)
				{
					base.Action = value;
					UpdateAmendmentReasonCodeAndInvalidationReasonIfNeeded(value);
					UpdateQueryByReferenceTypeIfNeeded(value);
				}
			}
		}

		void UpdateAmendmentReasonCodeAndInvalidationReasonIfNeeded(string messageType)
		{
			if (messageType == H7EDIMessageTypeList.Codes.ArrivalNotification && AmendmentReasonCode != AmendmentCancellationReasonCode.Codes.C_GoodsPresentationNotice)
			{
				AmendmentReasonCode = AmendmentCancellationReasonCode.Codes.C_GoodsPresentationNotice;
				AmendmentInvalidationReason = string.Empty;
			}
			else if (messageType == H7EDIMessageTypeList.Codes.NewDeclaration && !AmendmentReasonCode.IsEmpty)
			{
				AmendmentReasonCode = string.Empty;
				AmendmentInvalidationReason = string.Empty;
			}
		}

		void UpdateQueryByReferenceTypeIfNeeded(string messageType)
		{
			if (messageType == H7EDIMessageTypeList.Codes.QueryDeclaration && QueryType != H7QueryTypeList.Codes.MRNSummary)
			{
				QueryType = H7QueryTypeList.Codes.MRNSummary;
			}
			else if (messageType != H7EDIMessageTypeList.Codes.QueryDeclaration && !QueryType.IsEmpty)
			{
				QueryType = string.Empty;
			}
		}

		public bool IsAmendmentReasonCodeApplicableToCancellation => !AmendmentReasonCode.IsEmpty && AmendmentReasonCode != AmendmentCancellationReasonCode.Codes.C_GoodsPresentationNotice;

		protected override bool AmendmentReasonCode_ReadOnly => isNewDeclaration;

		protected override bool AmendmentInvalidationReason_ReadOnly => isNewDeclaration;

		protected override bool QueryType_ReadOnly => Action != H7EDIMessageTypeList.Codes.QueryDeclaration;

		bool isNewDeclaration => Action == H7EDIMessageTypeList.Codes.NewDeclaration;

		public bool IsQueryTypeApplicable => ((QueryType == H7QueryTypeList.Codes.MRNSummary || QueryType == H7QueryTypeList.Codes.MRNSnapshot) && !Bill.MovementReferenceNumber.IsEmpty)
			|| (QueryType == H7QueryTypeList.Codes.DUCR && Bill.PreviousDocuments.Any(d => d.CSI_Code == PreviousDocumentCodeListCDS.Codes.DeclarationUniqueConsignmentReferenceDucr))
			|| (QueryType == H7QueryTypeList.Codes.UCR && !Bill.ABL_UCRNumber.IsEmpty);

		[ResourceStringData("NPBO:Enterprise.Customs.GB.H7.Business.MessageSendingObject|AmendmentInvalidationReason", Caption = "Amendment Reason")]
		public override ZString AmendmentInvalidationReason { get => base.AmendmentInvalidationReason; set => base.AmendmentInvalidationReason = value; }

		[BusinessObjectTestExclude]
		[ResourceStringData("e4340e17-8da7-4136-893f-f0c9187f03b3", FullDescription = "Code identifying both the type of declaration and whether or not the goods have arrived at the goods location.", Caption = "Additional Declaration Type", MediumCaption = "Add. Decl. Type", ShortCaption = "Add. Decl. Type")]
		public override ZString SubStyle => Bill.ABL_ShipmentType;

		protected override bool SubStyle_ReadOnly => true;

		protected override bool EntryType_ReadOnly => true;

		#region Validation

		public new MessageSendingObjectValidation Validation => (MessageSendingObjectValidation)base.Validation;

		protected override EU.H7.Business.MessageSendingObjectValidation GetNewValidation()
		{
			return new MessageSendingObjectValidation(this);
		}

		#endregion
	}
}
