using Enterprise.Customs.AU.Module.AirCargo;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.UPE.Module
{
	public class UPEAirCargoMasterModule : AUCustomsAirCargoModule
	{
		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new UPEConsolReportingFilterBusinessObject();
		}
	}
}
