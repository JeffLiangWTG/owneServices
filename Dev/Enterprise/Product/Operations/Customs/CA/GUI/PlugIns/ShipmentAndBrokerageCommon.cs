using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.Environment;
using SharedCustoms = Enterprise.Customs.Business;
using SupervisorOverrides = Enterprise.Customs.CA.Business.SupervisorOverrides;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public class ShipmentAndBrokerageCommon : BaseShipmentAndBrokerageCommon
	{
		public ShipmentAndBrokerageCommon(SharedCustoms.BaseJobDeclaration declaration)
			: base(declaration)
		{
		}

		public override SharedCustoms.SupervisorOverrides GetSupervisorOverrides()
		{
			return new SupervisorOverrides(declaration, SharedCustoms.SupervisorOverridesContext.SavingDeclaration);
		}

		public override ContinueWithSave IsSupervisorApproved()
		{
			var dec = (JobDeclaration)declaration;
			ContinueWithSave result;
			var supervisorOverrides = (SupervisorOverrides)GetSupervisorOverrides();
			if (supervisorOverrides.AccountDateHasChanged(dec) && !supervisorOverrides.AnyStaffHasAccountModifyRight())
			{
				Globals.Message.ShowError(Res.GetString("43A7DF7B-3B0E-4559-84EE-F4EA77B9E030",
					"You may not change the Accounting date without supervisor approval. No supervisors have been setup so these dates may not be changed. To grant supervisor override capability to a supervisor user go to the users Security Rights->Operate->Customs->Supervisor Override, and grant the CA Accounting dates right."));
				result = ContinueWithSave.No;
			}
			else
			{
				result = base.IsSupervisorApproved();
			}
			return result;
		}
	}
}
