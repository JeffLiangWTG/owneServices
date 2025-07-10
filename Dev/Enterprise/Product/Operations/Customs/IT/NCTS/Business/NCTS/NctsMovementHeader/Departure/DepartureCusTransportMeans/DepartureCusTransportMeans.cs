using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.NCTS.Business;

public sealed class DepartureCusTransportMeans : EU.NCTS.Business.DepartureCusTransportMeans
{
	public DepartureCusTransportMeans(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new DepartureCusTransportMeansLookups Lookups => (DepartureCusTransportMeansLookups)base.Lookups;

	protected override Customs.Business.CusTransportMeansLookups GetNewLookups() => new DepartureCusTransportMeansLookups(this);
}
