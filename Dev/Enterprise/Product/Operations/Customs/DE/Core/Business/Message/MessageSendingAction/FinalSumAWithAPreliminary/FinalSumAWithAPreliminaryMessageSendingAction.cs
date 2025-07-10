using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.CusTempStorage;

namespace Enterprise.Customs.DE.Business
{
	public class FinalSumAWithAPreliminaryMessageSendingAction : BaseMessageSendingObject
	{
		public FinalSumAWithAPreliminaryMessageSendingAction(CusTempStorageLine messagingObject) : base(messagingObject.Factory)
		{
			MessagingObject = messagingObject;
		}
		public CusTempStorageLine MessagingObject { get; }

		public ZInt LineNo => MessagingObject.TSL_LineNo;

		public ZString Description => MessagingObject.TSL_GoodsDescription;

		public ZString OwnerReferenceType => MessagingObject.TSL_OwnerReferenceType;

		public ZString OwnerReferenceNumber => MessagingObject.TSL_OwnerReferenceNumber;

		public ZInt PackageCount => MessagingObject.TSL_PackageQty;

		public ZString PackageType => MessagingObject.TSL_PackageType;

		public ZString CustodianEORI => MessagingObject.TSL_CustodianIdentifier;

		public ZString CustodianBranch => MessagingObject.TSL_CustodianIdentifierBranchNo;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ShouldSend = true;
		}
	}
}
