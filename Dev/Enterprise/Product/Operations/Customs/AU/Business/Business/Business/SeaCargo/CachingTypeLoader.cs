using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class WrapperTypeLoader<TBusinessObject, TWrapper> : TypeLoader
		where TBusinessObject : BusinessObject
		where TWrapper : BusinessObjectWrapper
	{
		public WrapperTypeLoader()
			: base(typeof(TBusinessObject))
		{
		}

		protected override BusinessObject LoadCore(BusinessObjectFactory factory, ZGuid pK)
		{
			TBusinessObject bizO = factory.Load<TBusinessObject>(pK);
			return bizO != null ? BusinessObjectWrapper.Load(typeof(TWrapper), bizO) : null;
		}
	}
}
