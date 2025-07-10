using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.CommissionManagement.Module
{
	public class StaffCommissionAgreementsPlugInController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region ID

		public override ControllerID ID
		{
			get { return ControllerIDs.GlbStaffCommissionPlugIn; }
		}

		#endregion

		#region Form

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("not supported");
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { throw new ModuleGuiNotSupportedException("not supported"); }
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		#endregion

		#region PlugIn

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			var staff = (GlbStaff)businessEntity;
			return new StaffCommissionAgreementsPlugIn(staff);
		}

		#endregion

		#region Security

		protected override Security.SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.CommissionAgreementApproval; }
		}

		protected override Security.SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CommissionAgreementApproval; }
		}

		protected override Security.SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.CommissionAgreementApproval; }
		}

		protected override Security.SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CommissionAgreementApproval; }
		}

		#endregion
	}
}
