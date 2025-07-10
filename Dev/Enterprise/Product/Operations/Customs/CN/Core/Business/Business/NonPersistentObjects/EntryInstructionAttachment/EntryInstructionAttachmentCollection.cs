using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CN.Business
{
	public class EntryInstructionAttachmentCollection : NonPersistentBusinessObjectCollection<EntryInstructionAttachment>
	{
		public EntryInstructionAttachmentCollection(CusEntryInstruction instruction)
		{
			EntryInstruction = Argument.NotNull(instruction, nameof(instruction));
		}

		public CusEntryInstruction EntryInstruction { get; }

		public override void Load()
		{
			using (SuspendListChanged())
			{
				RemoveAllButLeaveRelationshipsIntact();

				EntryInstruction.CusStorageDocPivots.Cast<CusStorageDocPivot>().ForEach(x => Add(new EntryInstructionAttachment(this, x)));
				EntryInstruction.CusAttachments.Cast<CusAttachment>().ForEach(x => Add(new EntryInstructionAttachment(this, x)));
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => new EntryInstructionAttachment(this);

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			var element = (EntryInstructionAttachment)elementToDelete;
			element.RemoveAndDeleteFromParent();
			base.RemoveAndDelete(elementToDelete);
		}

		public CusStorageDocPivot CreateNewCusStorageDocPivot() => EntryInstruction.CusStorageDocPivots.AddNew();

		public void RemoveAndDeleteCusStorageDocPivot(CusStorageDocPivot cusStorageDocPivot)
		{
			if (EntryInstruction.CusStorageDocPivots.Contains(cusStorageDocPivot))
			{
				EntryInstruction.CusStorageDocPivots.RemoveAndDelete(cusStorageDocPivot);
			}
		}

		public CusAttachment CreateNewCusAttachment() => EntryInstruction.CusAttachments.AddNew();

		public void RemoveAndDeleteCusAttachment(CusAttachment cusAttachment)
		{
			if (EntryInstruction.CusAttachments.Contains(cusAttachment))
			{
				EntryInstruction.CusAttachments.RemoveAndDelete(cusAttachment);
			}
		}
	}
}
