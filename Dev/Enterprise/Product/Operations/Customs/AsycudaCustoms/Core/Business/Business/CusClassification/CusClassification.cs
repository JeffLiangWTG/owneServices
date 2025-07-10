using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public partial class CusClassification : Customs.Business.BaseCusClassification, Integration.Customs.AsycudaCustoms.ICusClassification
	{
		public CusClassification(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
