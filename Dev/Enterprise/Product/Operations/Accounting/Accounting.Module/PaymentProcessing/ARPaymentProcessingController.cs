using System;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class ARPaymentProcessingController : PaymentProcessingController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.ARPaymentProcessing; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.ARPaymentProcessing; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ARPaymentApprovalWithAuthorisation); }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ARPaymentProcessingView; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ARPaymentProcessingNew; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ARPaymentProcessingEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ARPaymentProcessingDelete; }
		}
	}
}
