using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.MX.Business;
using Enterprise.Customs.MX.GUI;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.MX.Module
{
	public class JobDeclarationShipmentController : Customs.Module.JobDeclarationShipmentController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(JobDeclaration);

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity) => new BrokeragePlugIn((ForwardingShipment)businessEntity);
	}
}
