using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class MessageChooser : AutoMessageChooser
	{
		public MessageChooser(AsycudaManifestHeader header, IEnumerable<ISelectionItem> items, bool showStatus)
		{
			Header = Argument.NotNull(header, nameof(header));
			items.ForEach(x =>
			{
				ChooserItems.AddNew(this, x, showStatus);
			});
			ChooserItems.Sort(MessageChooserItem.Schema.Description);
			DefaultSelect(showStatus);
		}

		public readonly AsycudaManifestHeader Header;

		public ISelectionItem[] GetSelectedItems()
		{
			return GetSelectedMessageChooserItems()
				.Select(x => x.BizO)
				.ToArray();
		}

		public IEnumerable<MessageChooserItem> GetSelectedMessageChooserItems()
		{
			return ChooserItems.Cast<MessageChooserItem>()
				.Where(x => x.Checked);
		}

		public void SelectAll()
		{
			foreach (MessageChooserItem chooserItem in ChooserItems)
			{
				chooserItem.Checked = ZBool.True;
			}
		}

		public void DeSelectAll()
		{
			foreach (MessageChooserItem chooserItem in ChooserItems)
			{
				chooserItem.Checked = ZBool.False;
			}
		}

		public MessageChooserItemCollection ChooserItems
		{
			get
			{
				if (chooserItems == null)
				{
					chooserItems = CreateNewMessageChooserItemCollection();
					RegisterEditableChildObject(chooserItems);
				}

				return chooserItems;
			}
		}
		MessageChooserItemCollection chooserItems;

		protected virtual MessageChooserItemCollection CreateNewMessageChooserItemCollection() => new MessageChooserItemCollection();

		protected virtual bool ShouldSelectChooserItem(MessageChooserItem chooserItem) => !chooserItem.BizoHasMessageErrors;

		public ZInt SelectedCount => GetSelectedItems().Length;

		public override ZString SelectedDescription => ResString.GetMultilingualString("D92EB94C-6C0A-42D0-B88A-AA5F53431A16", "{0} of {1} bill(s) selected.", SelectedCount, ChooserItems.Count);

		public MessageChooserLookups Lookups => fLookups ?? (fLookups = GetNewLookups());
		MessageChooserLookups fLookups;

		protected virtual MessageChooserLookups GetNewLookups() => new MessageChooserLookups(this);

		protected virtual void DefaultSelect(bool showStatus)
		{
			foreach (MessageChooserItem chooserItem in ChooserItems)
			{
				chooserItem.Checked = ShouldSelectChooserItem(chooserItem);
			}
		}
	}
}
