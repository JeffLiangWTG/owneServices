using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business.Testing
{
	class RefundEnquiryDummy : DummyBusinessObject, IRefundEnquiry
	{
		public RefundEnquiryDummy(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override void Delete()
		{
			base.Delete();
			RefundManager.DeleteRefund();
		}

		#region IRefundEnquiry Members
		public ClientRefund Refund
		{
			get
			{
				return RefundManager.Refund;
			}
		}

		public SchemaGuidColumn OwnerColumn
		{
			get
			{
				return ownerColumn;
			}

			set
			{
				ownerColumn = value;
			}
		}

		SchemaGuidColumn ownerColumn;
		public IRefundEnquiry RelatedOwner
		{
			get
			{
				return relatedOwner;
			}

			set
			{
				relatedOwner = value;
			}
		}

		IRefundEnquiry relatedOwner;
		public ZBool IsRefundEnquiry { get; set; }

		public ZBool IsRefundProcessed { get; set; }

		public void AddRefundNote()
		{
		}

		public void SendNotificationEmail()
		{
		}

		public RefundManager RefundManager
		{
			get
			{
				return refundManager ?? (refundManager = new RefundManagerDummy<RefundEnquiryDummy>(this));
			}
		}

		RefundManager refundManager;
		#endregion
	}
}
