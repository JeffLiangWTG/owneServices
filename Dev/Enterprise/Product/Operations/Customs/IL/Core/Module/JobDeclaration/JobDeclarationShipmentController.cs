using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.IL.GUI;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.IL.Module
{
	public class JobDeclarationShipmentController : Customs.Module.JobDeclarationShipmentController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(JobDeclaration);

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity) => new BrokeragePlugIn((ForwardingShipment)businessEntity);
	}
}
