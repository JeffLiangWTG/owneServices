using System;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class APPaymentProcessingController : PaymentProcessingController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.APPaymentProcessing; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.APPaymentProcessing; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(APPaymentApprovalWithAuthorisation); }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.APPaymentProcessingView; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.APPaymentProcessingNew; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.APPaymentProcessingEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.APPaymentProcessingDelete; }
		}
	}
}
