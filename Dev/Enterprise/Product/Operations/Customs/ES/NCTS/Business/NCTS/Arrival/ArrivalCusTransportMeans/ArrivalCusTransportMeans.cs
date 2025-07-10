using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public sealed class ArrivalCusTransportMeans : EU.NCTS.Business.ArrivalCusTransportMeans
	{
		public ArrivalCusTransportMeans(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new ArrivalCusTransportMeansLookups Lookups => (ArrivalCusTransportMeansLookups)base.Lookups;

		protected override Customs.Business.CusTransportMeansLookups GetNewLookups() => new ArrivalCusTransportMeansLookups(this);
	}
}
