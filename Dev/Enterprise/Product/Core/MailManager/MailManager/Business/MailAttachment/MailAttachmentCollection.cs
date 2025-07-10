using System.Collections;
using System.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.MailManager.Business
{
	public class MailAttachmentCollection : DependentBusinessObjectCollection<MailAttachment, MailItem>
	{
		public MailAttachmentCollection(MailItem master, BusinessObjectFactory factory) : base(master, factory)
		{
		}

		protected override IComparer GetComparerForSort(PropertyDescriptor property, ListSortDirection direction)
		{
			if (property.Name == nameof(MailAttachment.HumanReadableAttachmentSize))
			{
				return new PropertyComparer(typeof(MailAttachment), nameof(MailAttachment.AttachmentSizeInBytes), direction);
			}
			return base.GetComparerForSort(property, direction);
		}
	}
}
