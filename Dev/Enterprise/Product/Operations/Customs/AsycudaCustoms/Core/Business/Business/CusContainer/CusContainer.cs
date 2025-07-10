using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public partial class CusContainer : Customs.Business.BaseCusContainer, Integration.Customs.AsycudaCustoms.ICusContainer
	{
		public CusContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
