using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusStorageDocPivotMessageSendingActionParent : BaseMessageSendingObjectParent<CusStorageDocPivotMessageSendingAction>
	{
		public CusStorageDocPivotMessageSendingActionParent(QuarantineColsHeader colsHeader) : base(colsHeader.Factory)
		{
			this.colsHeader = colsHeader;
		}
		readonly QuarantineColsHeader colsHeader;

		public override BusinessObject TopLevelBusinessObject => colsHeader;

		public override SecurityCheckpoint SecurityCheckpointToSendWithMessageError => Env.Security.None;

		public bool ShouldAllowUsersToSelectSendingObjects => SendingObjectsCollection.Cast<CusStorageDocPivotMessageSendingAction>().Any(x => !x.MessageStatus.IsEmpty);

		public CusStorageDocPivot[] SelectedDocuments => SelectedSendingObjects.Cast<CusStorageDocPivotMessageSendingAction>().Select(x => x.Document).ToArray();

		protected override NonPersistentBusinessObjectCollection<CusStorageDocPivotMessageSendingAction> GetSendingObjectsCollectionCore()
		{
			return new CusStorageDocPivotMessageSendingActionCollection(colsHeader);
		}
	}
}
