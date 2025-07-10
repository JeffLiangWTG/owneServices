using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.MX.Business;
using Enterprise.Customs.MX.GUI;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.MX.Module
{
	public class JobDeclarationController : Customs.Module.JobDeclarationController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(JobDeclaration);

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity) => new BrokeragePlugIn((ForwardingShipment)businessEntity);

		protected override ZArchitecture.GUI.IZForm GetFormCore(IBusiness businessEntity) => new JobDeclarationForm((JobDeclaration)businessEntity);
	}
}
