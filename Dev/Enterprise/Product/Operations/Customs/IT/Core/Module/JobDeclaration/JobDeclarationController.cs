using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.GUI;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.IT.Module;

public class JobDeclarationController : EU.Module.JobDeclarationController
{
	public override Type TypeOfTopLevelBusinessObject
	{
		get { return typeof(JobDeclaration); }
	}

	protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity)
	{
		return new BrokeragePlugIn((ForwardingShipment)businessEntity);
	}

	protected override ZArchitecture.GUI.IZForm GetFormCore(IBusiness businessEntity)
	{
		return new JobDeclarationForm((JobDeclaration)businessEntity);
	}
}
