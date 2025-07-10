using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaBillScreening : ManifestBase.AsycudaBillScreening
	{
		public AsycudaBillScreening(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
