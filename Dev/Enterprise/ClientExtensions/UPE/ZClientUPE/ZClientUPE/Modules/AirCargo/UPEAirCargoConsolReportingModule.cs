
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Module.AirCargo;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.UPE.Module
{
	public class UPEAirCargoConsolReportingModule : AUCustomsAirCargoModule
	{
		public override ModuleIdentifier ID
		{
			get { return ClientModuleRegistration.AirCargo; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return new UPEAirCargoConsolController();
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new UPEConsolReportingFilterBusinessObject();
		}
	}
}
