using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.CN.GUI;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.CN.Module
{
	public class JobDeclarationShipmentController : Customs.Module.JobDeclarationShipmentController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(JobDeclaration);

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity) => new BrokeragePlugIn((ForwardingShipment)businessEntity);
	}
}
