using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AQISProducerCodeCollection : AQISSingleValueCollection
	{
		public AQISProducerCodeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new AQISProducerCode this[int index]
		{
			get { return (AQISProducerCode)(Elements[index]); }
		}

		public new AQISProducerCode AddNew()
		{
			return (AQISProducerCode)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AQISProducerCode(Factory);
		}
	}
}
