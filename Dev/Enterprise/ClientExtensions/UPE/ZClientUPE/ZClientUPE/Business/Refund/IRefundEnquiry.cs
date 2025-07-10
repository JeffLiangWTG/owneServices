using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business
{
	public interface IRefundEnquiry
	{
		ClientRefund Refund { get; }
		SchemaGuidColumn OwnerColumn { get; }
		ZGuid PK { get; }
		IRefundEnquiry RelatedOwner { get; }
		ZBool IsRefundEnquiry { get; set; }
		ZBool IsRefundProcessed { get; set; }
		void AddRefundNote();
		void SendNotificationEmail();
		RefundManager RefundManager { get; }
	}
}
