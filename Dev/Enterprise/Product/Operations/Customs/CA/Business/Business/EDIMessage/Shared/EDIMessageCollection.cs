using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.CA.Business
{
	public class EDIMessageCollection : Enterprise.Messaging.Business.EDIMessageCollection
	{
		public EDIMessageCollection(BusinessObject master)
			: base(master)
		{
		}

		public EDIMessageCollection(BusinessObject master, ZQuery filter)
			: base(master)
		{
			((ILegacyBusinessObjectCollectionInternals)this).SetOverriddenAdditionalFilter(filter);
		}

		public new EDIMessage this[int index]
		{
			get { return (EDIMessage)base[index]; }
		}

		public new EDIMessage AddNew()
		{
			return (EDIMessage)base.AddNew();
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
