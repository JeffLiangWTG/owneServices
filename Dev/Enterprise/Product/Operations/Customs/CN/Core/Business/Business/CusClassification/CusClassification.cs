using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CN.Business
{
	public partial class CusClassification : Customs.Business.BaseCusClassification, Integration.Customs.CN.ICusClassification
	{
		public CusClassification(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
