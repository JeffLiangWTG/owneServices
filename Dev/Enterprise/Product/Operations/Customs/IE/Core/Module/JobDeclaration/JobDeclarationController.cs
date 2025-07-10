using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.GUI;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.IE.Module
{
	public class JobDeclarationController : EU.Module.JobDeclarationController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(JobDeclaration);

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity) => new BrokeragePlugIn((ForwardingShipment)businessEntity);

		protected override ZArchitecture.GUI.IZForm GetFormCore(IBusiness businessEntity) => new JobDeclarationForm((JobDeclaration)businessEntity);
	}
}
