using System;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class APExchangeDifferenceController : MiscellaneousTransactionController
	{
		public APExchangeDifferenceController()
		{
		}

		protected override ControllerID IDCore
		{
			get { return ControllerIDs.APExchangeDifference; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(APExchangeDifference); }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}
	}
}
