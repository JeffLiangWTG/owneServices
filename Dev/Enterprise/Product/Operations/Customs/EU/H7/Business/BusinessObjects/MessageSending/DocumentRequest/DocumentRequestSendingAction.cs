using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.EU.H7.Business
{
	public class DocumentRequestSendingAction : MessageSendingObject
	{
		public DocumentRequestSendingAction(AsycudaBill bill) : base(bill)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
		}

		readonly AsycudaBill bill;

		public ZPropertyInfo MovementReferenceNumberInfo => GetZPropertyInfo(nameof(MovementReferenceNumber));

		[ResourceStringData("d641d23d-58fa-4fd8-b001-c4aab1d16e02", Caption = "MRN", FullDescription = "Movement Reference Number")]
		public ZString MovementReferenceNumber => bill.MovementReferenceNumber;
	}
}
