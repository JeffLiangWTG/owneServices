using System;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(SellApportionmentForGatewayController))]
	public class SellApportionmentForGatewayControllerTest : JobInvoicingControllerTest
	{
		public override Type ControllerToBashType
		{
			get { return typeof(SellApportionmentForGatewayController); }
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.SellApportionmentForGateway;
		}
	}
}
