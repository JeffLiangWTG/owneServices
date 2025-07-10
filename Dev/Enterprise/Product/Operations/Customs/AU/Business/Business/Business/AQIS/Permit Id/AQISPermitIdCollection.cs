
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AQISPermitIdCollection : AQISSingleValueCollection
	{
		public AQISPermitIdCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new AQISPermitId this[int index]
		{
			get { return (AQISPermitId)(Elements[index]); }
		}

		public new AQISPermitId AddNew()
		{
			return (AQISPermitId)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AQISPermitId(Factory);
		}
	}
}
