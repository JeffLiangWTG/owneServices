
using CargoWise.EntityFramework;
using Enterprise.MailManager.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Mail.Business
{
	public abstract class EDIMailItemCollection : MailItemCollection
	{
		protected EDIMailItemCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected EDIMailItemCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery query = base.CreateRelationshipFilter();
			query.AddToFilter(MailDBItemsSchema.MI_Application, MailApplicationCode);
			return query;
		}

		public new EDIMailItem this[int index]
		{
			get { return (EDIMailItem)base[index]; }
		}

		public new EDIMailItem AddNew()
		{
			return (EDIMailItem)base.AddNew();
		}

		protected abstract string MailApplicationCode { get; }
	}
}

