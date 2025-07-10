using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.Customs.AsycudaCustoms.GUI;
using Enterprise.Freight.Forwarding.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.AsycudaCustoms.Module
{
	[CodeAlive("Controller dynamically hooked up for AsycudaCustoms countries.")]
	public class JobDeclarationController : Customs.Module.JobDeclarationController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(JobDeclaration);

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity) => new BrokeragePlugIn((ForwardingShipment)businessEntity);

		protected override ZArchitecture.GUI.IZForm GetFormCore(IBusiness topLevelObject) => new JobDeclarationForm((JobDeclaration)topLevelObject);
	}
}
