using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.GUI;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.CH.Module;

public class JobDeclarationController : Customs.Module.JobDeclarationController
{
	public override Type TypeOfTopLevelBusinessObject => typeof(JobDeclaration);

	protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity) => new BrokeragePlugIn((ForwardingShipment)businessEntity);

	protected override ZArchitecture.GUI.IZForm GetFormCore(IBusiness businessEntity) => new JobDeclarationForm((JobDeclaration)businessEntity);
}
