using System;
using Enterprise.Client.JAS.Business;
using Enterprise.Client.JAS.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Forwarding.Module;

namespace Enterprise.Client.JAS.Module
{
	public class JASJobConsolController : JobConsolController
	{
		protected override ConsolForm GetFormCore(ForwardingConsol businessEntity)
		{
			return new JASConsolForm(businessEntity as JASForwardingConsol);
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(JASForwardingConsol); }
		}
	}
}
