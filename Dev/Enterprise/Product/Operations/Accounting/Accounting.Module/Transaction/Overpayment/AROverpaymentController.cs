using System;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class AROverpaymentController : MiscellaneousTransactionController
	{
		public AROverpaymentController()
		{
		}

		protected override ControllerID IDCore
		{
			get { return ControllerIDs.AROverpayment; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(AROverpayment); }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}
	}
}
