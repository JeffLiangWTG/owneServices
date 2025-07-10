using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.IN.Business;
using Enterprise.Customs.IN.GUI;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.IN.Module;

public class JobDeclarationShipmentController : Customs.Module.JobDeclarationShipmentController
{
	public override Type TypeOfTopLevelBusinessObject => typeof(JobDeclaration);

	protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity) => new BrokeragePlugIn((ForwardingShipment)businessEntity);
}
