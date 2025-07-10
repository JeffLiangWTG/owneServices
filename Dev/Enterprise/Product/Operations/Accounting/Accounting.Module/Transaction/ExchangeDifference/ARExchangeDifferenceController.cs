using System;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class ARExchangeDifferenceController : MiscellaneousTransactionController
	{
		public ARExchangeDifferenceController()
		{
		}

		protected override ControllerID IDCore
		{
			get { return ControllerIDs.ARExchangeDifference; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ARExchangeDifference); }
		}
	}
}
