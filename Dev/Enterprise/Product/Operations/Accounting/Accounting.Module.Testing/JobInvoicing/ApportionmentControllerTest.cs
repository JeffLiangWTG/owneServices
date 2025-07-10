using System;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ApportionmentController))]
	public class ApportionmentControllerTest : JobInvoicingControllerTest
	{
		public override Type ControllerToBashType
		{
			get { return typeof(ApportionmentController); }
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Apportionment;
		}
	}
}
