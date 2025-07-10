using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class DocumentRequestSendingActionParent<TMessageSendingObject> : EU.H7.Business.DocumentRequestSendingActionParent<TMessageSendingObject> where TMessageSendingObject : DocumentRequestSendingAction
	{
		public DocumentRequestSendingActionParent(EU.H7.Business.AsycudaManifestHeader manifestHeader) : base(manifestHeader)
		{
		}

		protected override NonPersistentBusinessObjectCollection<TMessageSendingObject> GetSendingObjectsCollectionCore()
		{
			var result = new MessageSendingObjectCollection<TMessageSendingObject>(Factory);
			foreach (AsycudaBill bill in manifestHeader.Bills)
			{
				if (!bill.ClearanceReferenceNumber.IsEmpty && !HasReleaseDocument(bill))
				{
					result.Add(CreateNewMessageSendingObject(bill));
				}
			}

			return result;
		}

		bool HasReleaseDocument(AsycudaBill bill)
		{
			var docManagerInfo = (bill as IDocManagerSupport)?.DocManagerInfo;
			if (docManagerInfo != null)
			{
				var fileName = bill.H7MovementReferenceNumber + ESConstants.DocumentCaptureRequestFileNameSuffixes.H7ClearanceDoc + DocumentExtension;
				return docManagerInfo.GetRelatedEDocs().Any(x => x.FileName.EqualsIgnoringCase(fileName));
			}

			return false;
		}

		const string DocumentExtension = ".pdf";
	}
}
