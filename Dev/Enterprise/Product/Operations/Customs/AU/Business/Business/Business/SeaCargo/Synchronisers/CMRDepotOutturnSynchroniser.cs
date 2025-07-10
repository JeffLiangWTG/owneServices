using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class CMRDepotOutturnSynchroniser : BusinessObjectSynchroniser
	{
		public CMRDepotOutturnSynchroniser(CusOutturn destination, BusinessObject source)
			: base(destination, source)
		{
			this.Outturn = destination;
			this.CFSSource = source;
		}

		public readonly CusOutturn Outturn;
		public readonly BusinessObject CFSSource;
	}
}
