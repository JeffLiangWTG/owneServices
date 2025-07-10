using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AE.Business;
using Enterprise.Customs.AE.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.AE.Module;

public class JobDeclarationController : Customs.Module.JobDeclarationController
{
	public override Type TypeOfTopLevelBusinessObject => typeof(JobDeclaration);

	protected override IZForm GetFormCore(IBusiness topLevelObject) => new JobDeclarationForm((JobDeclaration)topLevelObject);

	protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new BrokeragePlugIn((ForwardingShipment)businessEntity);
}
