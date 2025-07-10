using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.H7.Business
{
	public class MessageSendingObject : AutoMessageSendingObject, IH7MessageSendingObject
	{
		public MessageSendingObject(AsycudaBill bill)
			: base(bill.Factory)
		{
			Bill = Argument.NotNull(bill, nameof(bill));

			SetDefaultData();
		}

		public AsycudaBill Bill { get; }

		protected override ZString HumanReadableNameCore => Res.GetString("3a5484ab-1266-43c1-b069-8c90c19621d1", "Message");

		protected virtual void SetDefaultData()
		{
			ShouldSend = true;
			Action = DefaultAction;
		}

		[List(nameof(ActionList))]
		public override ZString Action
		{
			get => base.Action;
			set => base.Action = value;
		}

		[List(nameof(OperationCodeList))]
		public override ZString OperationCode { get => base.OperationCode; set => base.OperationCode = value; }

		public CodeDescriptionPairList ActionList => GetActionList();

		public CodeDescriptionPairList AmendmentReasonCodeList => GetAmendmentReasonCodeList();

		public CodeDescriptionPairList QueryTypeList => GetQueryTypeList();

		public CodeDescriptionPairList OperationCodeList => GetOperationCodeList();

		[BusinessObjectTestExclude]
		[List(nameof(SubStyleList))]
		public override ZString SubStyle => Bill.ABL_ShipmentType;

		[List(nameof(AmendmentReasonCodeList))]
		public override ZString AmendmentReasonCode { get => base.AmendmentReasonCode; set => base.AmendmentReasonCode = value; }

		[List(nameof(QueryTypeList))]
		public override ZString QueryType { get => base.QueryType; set => base.QueryType = value; }

		internal protected virtual ZString DefaultAction => ZString.Empty;

		protected override bool SubStyle_ReadOnly => true;

		protected override bool AmendmentInvalidationReason_ReadOnly => false;

		public CodeDescriptionPairList SubStyleList => GetSubStyleList();

		public override ZString BillNumber => Bill.ABL_BillNumber;

		public override ZString LocalReferenceNumber => Bill.LocalReferenceNumber;

		public override ZString MessageStatus => Bill.ABL_MessageStatus;

		public override ZString EntryStatus => Bill.ABL_CargoStatus;

		public override ZString MRN => Bill.MovementReferenceNumber;

		public override ZString CustomsStatus => Bill.ABL_BillStatus;

		public override ZString ReferenceNumber => Bill.ABL_UCRNumber;

		#region Implement

		// Since Bill is not yet initialized in SetDefaultValues,
		// we mark it as sealed to enforce the use of SetDefaultData instead.
		protected sealed override void SetDefaultValues() => base.SetDefaultValues();

		protected virtual CodeDescriptionPairList GetActionList() => new CodeDescriptionPairList();

		protected virtual CodeDescriptionPairList GetSubStyleList() => new CodeDescriptionPairList();

		protected virtual CodeDescriptionPairList GetAmendmentReasonCodeList() => new CodeDescriptionPairList();

		protected virtual CodeDescriptionPairList GetQueryTypeList() => new CodeDescriptionPairList();

		public virtual MessageSender CreateSender() => null;

		public virtual ZString ActionForSendingCustomsDeclaration => ZString.Empty;

		protected virtual CodeDescriptionPairList GetOperationCodeList() => new CodeDescriptionPairList();

		public virtual bool IsAmendmentAction => false;

		public virtual bool IsCancellationAction => false;

		#endregion
	}
}
