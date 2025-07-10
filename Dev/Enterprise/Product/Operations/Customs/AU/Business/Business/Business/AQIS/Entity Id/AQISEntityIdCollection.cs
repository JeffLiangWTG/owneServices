
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AQISEntityIdCollection : AQISSingleValueCollection
	{
		public AQISEntityIdCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new AQISEntityId this[int index]
		{
			get { return (AQISEntityId)(Elements[index]); }
		}

		public new AQISEntityId AddNew()
		{
			return (AQISEntityId)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AQISEntityId(Factory);
		}
	}
}
