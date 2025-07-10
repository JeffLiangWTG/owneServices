using CargoWise.EntityFramework;

namespace Enterprise.Customs.JP.Manifest.Business
{
	public class CusSealCollection : Common.CusSealCollection<CusSeal>
	{
		public CusSealCollection(BusinessObject master) : base(master, 3)
		{
		}
	}
}
