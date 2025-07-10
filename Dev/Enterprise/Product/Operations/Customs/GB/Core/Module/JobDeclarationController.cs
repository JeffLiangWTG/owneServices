using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.GB.Module
{
	// **** GB ****
	public class JobDeclarationController : EU.Module.JobDeclarationController
	{
		// **** GB ****
		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(JobDeclaration); }
		}

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity)
		{   // **** GB ****
			return new GUI.GbBrokeragePlugIn((ForwardingShipment)businessEntity);
		}

		protected override ZArchitecture.GUI.IZForm GetFormCore(IBusiness businessEntity)
		{   // **** GB ****
			return new GUI.JobDeclarationForm((JobDeclaration)businessEntity);
		}
	}
}
