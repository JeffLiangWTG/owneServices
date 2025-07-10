using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.CA.Business
{
	public class RSFMessageCollection : EDIMessageCollection
	{
		public RSFMessageCollection(BusinessObject master) : base(master)
		{
		}

		public RSFMessageCollection(BusinessObject master, ZQuery filter) : base(master, filter)
		{
			((ILegacyBusinessObjectCollectionInternals)this).SetOverriddenAdditionalFilter(filter);
		}

		public new RSFMessage this[int index]
		{
			get { return (RSFMessage)base[index]; }
		}

		public new RSFMessage AddNew()
		{
			return (RSFMessage)base.AddNew();
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected override ZBool IsMessageNewer(Enterprise.Messaging.Business.EDIMessage messageX, Enterprise.Messaging.Business.EDIMessage messageY)
		{
			return MessageComparer.Compare(messageX, messageY) > 0;
		}

		EDIMessageComparer MessageComparer
		{
			get
			{
				if (fMessageComparer == null)
				{
					fMessageComparer = new EDIMessageComparer(ListSortDirection.Ascending);
				}
				return fMessageComparer;
			}
		}
		EDIMessageComparer fMessageComparer;
	}
}
