using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.CommissionManagement.Business
{
	public class CommissionApprovalRequestDocumentSupporter : DocumentSupporter
	{
		public CommissionApprovalRequestDocumentSupporter(AccCommissionApprovalRequest approvalRequest)
			: base(approvalRequest)
		{
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CommissApprovalReq; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (dataContext == Constants.DataContext.CommissionApprovalReq || dataContext == Constants.DataContext.CommissionPayment)
			{
				return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(dataContext, BusinessObject) };
			}
			else
			{
				return null;
			}
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Constants.DataContext[] { Constants.DataContext.CommissionApprovalReq, Constants.DataContext.CommissionPayment };
		}

		protected override bool GetIsNonPersistent()
		{
			return true;
		}

		protected override bool IgnoreHasChangesCore()
		{
			return true;
		}
	}
}
