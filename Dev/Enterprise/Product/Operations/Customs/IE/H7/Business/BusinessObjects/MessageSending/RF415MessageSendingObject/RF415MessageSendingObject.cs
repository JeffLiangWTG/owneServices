using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.H7.Business
{
	public class RF415MessageSendingObject : AutoRF415MessageSendingObject, IH7MessageSendingObject
	{
		public RF415MessageSendingObject(AsycudaBill bill)
			: base(bill.Factory)
		{
			Bill = Argument.NotNull(bill, nameof(bill));

			SetDefaultData();
		}

		void SetDefaultData()
		{
			ShouldSend = true;
			RefundType = RF415RefundTypes.Codes.Repayment;
		}

		public AsycudaBill Bill { get; }

		EU.H7.Business.AsycudaBill IH7MessageSendingObject.Bill => Bill;

		public ZString Action => AISOutgoingMessageTypeList.Codes.ApplicationForRemissionOfCustomsDebtF15;

		public override ZBool ShouldSend
		{
			get => base.ShouldSend;
			set
			{
				base.ShouldSend = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateAll();
				}

				DocumentSendingObjectCollection.RefreshBinding();
			}
		}

		public override ZString MovementReferenceNumber => Bill.MovementReferenceNumber;

		public override ZString BillNumber => Bill.ABL_BillNumber;

		[List(nameof(Lookups) + "." + nameof(RF415MessageSendingObjectLookups.RefundTypeList))]
		public override ZString RefundType
		{
			get => base.RefundType;
			set => base.RefundType = value;
		}

		[List(nameof(Lookups) + "." + nameof(RF415MessageSendingObjectLookups.CustomsOfficesList))]
		public override ZString OfficeOfDebt
		{
			get => base.OfficeOfDebt;
			set => base.OfficeOfDebt = value;
		}

		[List(nameof(Lookups) + "." + nameof(RF415MessageSendingObjectLookups.CustomsOfficesList))]
		public override ZString OfficeOfResponsibility
		{
			get => base.OfficeOfResponsibility;
			set => base.OfficeOfResponsibility = value;
		}

		[List(nameof(Lookups) + "." + nameof(RF415MessageSendingObjectLookups.LegalBasisList))]
		public override ZString LegalBasis
		{
			get => base.LegalBasis;
			set
			{
				base.LegalBasis = value;

				if (DescriptionOfGrounds.IsEmpty)
				{
					DescriptionOfGrounds = Lookups.LegalBasisList.GetDescriptionFromCode(value);
				}
			}
		}

		public RF415DocumentSendingObjectCollection DocumentSendingObjectCollection
		{
			get
			{
				if (documentSendingObjectCollection == null)
				{
					documentSendingObjectCollection = new RF415DocumentSendingObjectCollection(this);
					RegisterEditableChildObject(documentSendingObjectCollection);
				}
				return documentSendingObjectCollection;
			}
		}
		RF415DocumentSendingObjectCollection documentSendingObjectCollection;

		public RF415MessageSendingObjectLookups Lookups => fLookups ??= new RF415MessageSendingObjectLookups(this);
		RF415MessageSendingObjectLookups fLookups;

		public MessageSender CreateSender() => new IEMessageSender(this);

		public ZString DataGrouping => Bill.DataGrouping;
	}
}
