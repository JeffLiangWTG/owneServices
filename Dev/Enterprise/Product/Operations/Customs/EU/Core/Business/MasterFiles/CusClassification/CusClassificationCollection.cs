using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.MasterFiles
{
	public class CusClassificationCollection<T> : Customs.Business.BaseClassificationCollection<T> where T : CusClassification
	{
		public CusClassificationCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
