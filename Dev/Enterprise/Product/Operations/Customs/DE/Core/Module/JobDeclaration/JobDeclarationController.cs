using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.GUI;
using Enterprise.Customs.DE.GUI.PlugIn;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.DE.Module
{
	public class JobDeclarationController : EU.Module.JobDeclarationController
	{
		public JobDeclarationController()
			: base()
		{ }

		public override System.Type TypeOfTopLevelBusinessObject => typeof(JobDeclaration);

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity) => new BrokeragePlugIn((ForwardingShipment)businessEntity);

		protected override ZArchitecture.GUI.IZForm GetFormCore(IBusiness businessEntity) => new JobDeclarationForm((JobDeclaration)businessEntity);
	}
}
