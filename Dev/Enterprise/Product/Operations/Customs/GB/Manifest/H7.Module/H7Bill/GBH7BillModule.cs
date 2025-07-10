using CargoWise.EntityFramework;
using Enterprise.Customs.EU.H7.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.H7.Module
{
	public class GBH7BillModule : EUH7BillModule
	{
		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.GB.H7Bill);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new GBH7BillFilterBusinessObject();
	}
}
