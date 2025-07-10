using System;

using CargoWise.EntityFramework;
using Enterprise.Client.JAS.Business;
using Enterprise.Client.JAS.GUI;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.JAS.Module
{
	public class JASJobShipmentController : JobShipmentController
	{
		protected override IZForm GetFormCore(IBusiness businessEntity)
		{
			return new JASShipmentForm(businessEntity as JASForwardingShipment);
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(JASForwardingShipment); }
		}
	}
}
