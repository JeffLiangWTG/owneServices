using CargoWise.EntityFramework;

namespace Enterprise.Customs.JP.Business
{
	public class CusSealCollection : Common.CusSealCollection<CusSeal>
	{
		public CusSealCollection(BusinessObject master) : base(master, 4)
		{
		}
	}
}
