
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AQISCommodityCodeCollection : AQISSingleValueCollection
	{
		public AQISCommodityCodeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new AQISCommodityCode this[int index]
		{
			get { return (AQISCommodityCode)(Elements[index]); }
		}

		public new AQISCommodityCode AddNew()
		{
			return (AQISCommodityCode)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AQISCommodityCode(Factory);
		}
	}
}
