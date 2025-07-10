using System;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class APOverpaymentController : MiscellaneousTransactionController
	{
		public APOverpaymentController()
		{
		}

		protected override ControllerID IDCore
		{
			get { return ControllerIDs.APOverpayment; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(APOverpayment); }
		}
	}
}
