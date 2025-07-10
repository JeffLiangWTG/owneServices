using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Module;

public class JobDeclarationShipmentController : Customs.Module.JobDeclarationShipmentController
{
	public override Type TypeOfTopLevelBusinessObject
	{
		get { return typeof(JobDeclaration); }
	}

	protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity)
	{
		return new AUBrokeragePluginToFreight((ForwardingShipment)businessEntity);
	}
}
