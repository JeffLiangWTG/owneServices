using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public partial class APCashAdvanceModule : CashAdvanceModule
	{
		public override ModuleIdentifier ID => ModuleIDs.APCashAdvance;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.APCashAdvance);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new APCashAdvanceFilterBusinessObject();
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.APCashAdvance; }
		}

		protected override SecurityCheckpoint MarkAsCancelSecurityCheckpoint => Env.Security.APCashAdvanceRequestCancel;
		protected override SecurityCheckpoint MarkAsPaidSecurityCheckpoint => Env.Security.APCashAdvanceRequestMarkAsPaid;
		protected override SecurityCheckpoint MarkAsUnpaidSecurityCheckpoint => Env.Security.APCashAdvanceRequestMarkAsUnPaid;

		protected override bool IsManualSettingOfCashAdvanceRequestStatusToPaidAllowed
		{
			get
			{
				var cashAdvanceFunctionalityChecker = ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>();
				return cashAdvanceFunctionalityChecker != null &&
					cashAdvanceFunctionalityChecker.IsManualSettingOfPayablesCashAdvanceRequestStatusToPaidAllowed;
			}
		}
	}
}
