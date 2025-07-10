using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business
{
	public class AdditionalInfoSendingObjectCollection : NonPersistentBusinessObjectCollection<AdditionalInfoSendingObject>
	{
		public AdditionalInfoSendingObjectCollection(CusEntryHeader entryHeader, CusEntryHeaderMessageSendingAction action)
		{
			EntryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
			Action = Argument.NotNull(action, nameof(action));
			MaxCountValidationEnable(99);
		}
		CusEntryHeader EntryHeader { get; }
		CusEntryHeaderMessageSendingAction Action { get; }

		protected override BusinessObject CreateNonPersistentBusinessObject() => new AdditionalInfoSendingObject(EntryHeader, Action, null);

		public void LoadElements(Func<EU.Business.RequestedDocument, bool> filter = null)
		{
			RemoveAndDeleteAll();
			if (EntryHeader.EntryInstruction is CusEntryInstruction cusEntryInstruction)
			{
				foreach (var requested in cusEntryInstruction.RequestedDocuments.Cast<EU.Business.RequestedDocument>().Where(req => req.IsOpen && (filter == null || filter(req))))
				{
					var sendingObject = new AdditionalInfoSendingObject(EntryHeader, Action, requested);
					sendingObject.Validation.ValidateDocumentInformation();
					Add(sendingObject);
				}
			}
		}

		protected override void OnAdded(BusinessObject bizO)
		{
			base.OnAdded(bizO);
			Action.ShouldSendInfo.ValueChanged += ((AdditionalInfoSendingObject)bizO).ValidateAllAndRefreshBinding;
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			Action.ShouldSendInfo.ValueChanged -= ((AdditionalInfoSendingObject)bizO).ValidateAllAndRefreshBinding;
		}
	}
}
