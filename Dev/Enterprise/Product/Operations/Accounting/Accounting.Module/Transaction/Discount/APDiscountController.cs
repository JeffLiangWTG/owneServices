using System;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class APDiscountController : MiscellaneousTransactionController
	{
		public APDiscountController()
		{
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(APDiscount); }
		}

		protected override ControllerID IDCore
		{
			get { return ControllerIDs.APDiscount; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}
	}
}
