using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.BR.GUI;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.BR.Module
{
	public class JobDeclarationShipmentController : Customs.Module.JobDeclarationShipmentController
	{
		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(JobDeclaration); }
		}

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new BrokeragePlugIn((ForwardingShipment)businessEntity);
		}
	}
}
