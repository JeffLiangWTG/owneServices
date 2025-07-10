using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.BR.Business
{
	public partial class CusContainer : Customs.Business.BaseCusContainer
	{
		public CusContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
