
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class PRAContainerCollection : NonPersistentBusinessObjectCollection<NonPersistentBusinessObject>
	{
		public PRAContainerCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new IPRAContainerMessaging this[int index] => (IPRAContainerMessaging)Elements[index];

		public new IPRAContainerMessaging AddNew() => (IPRAContainerMessaging)base.AddNew();

		protected override void AddRowToDataTableIfDetached(BusinessObject row)
		{
		}

		protected override bool AllowNewCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject() => new DummyIContainerMessaging(Factory);
	}
}
