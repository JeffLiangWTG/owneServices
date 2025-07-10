using System;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ApportionmentForTransitTransportationUnitController))]
	public class ApportionmentForTransitTransportationUnitControllerTest : JobInvoicingControllerTest
	{
		public override Type ControllerToBashType => typeof(ApportionmentForTransitTransportationUnitController);

		protected override ControllerID GetControllerID() => ControllerIDs.ApportionmentForTransitTransportationUnit;
	}
}
