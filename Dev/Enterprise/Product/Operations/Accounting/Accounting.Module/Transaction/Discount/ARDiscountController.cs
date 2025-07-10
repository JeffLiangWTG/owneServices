using System;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class ARDiscountController : MiscellaneousTransactionController
	{
		public ARDiscountController()
		{
		}

		protected override ControllerID IDCore
		{
			get { return ControllerIDs.ARDiscount; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ARDiscount); }
		}
	}
}
