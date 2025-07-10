using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Accounting.Module
{
	public class JobConsolCostingController : ApportionmentController
	{
		public JobConsolCostingController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.JobConsolCostingForm; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.JobManagement; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return BusinessType; }
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new JobConsolCostingPlugin(businessEntity);
		}

		Type BusinessType;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new JobConsolCostingForm(businessEntity);
		}

		public override IZForm ShowEditForm(BusinessObject bizObject)
		{
			if (bizObject is IJobCostingPlugIn jobCostingPlugIn)
			{
				jobConsolCostingFormSecurityCheckPoint = jobCostingPlugIn.CostSupporter.JobConsolCostingCheckPoint ?? Env.Security.None;
			}
			BusinessType = bizObject.GetType();

			return base.ShowEditForm(bizObject);
		}

		protected override IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness bizObject) => bizObject;

		#region SecurityCheckpoint

		SecurityCheckpoint jobConsolCostingFormSecurityCheckPoint;

		protected override SecurityCheckpoint CheckPointForView => jobConsolCostingFormSecurityCheckPoint;

		protected override SecurityCheckpoint CheckPointForEdit => jobConsolCostingFormSecurityCheckPoint;

		protected override SecurityCheckpoint CheckPointForNew => jobConsolCostingFormSecurityCheckPoint;

		protected override SecurityCheckpoint CheckPointForDelete => jobConsolCostingFormSecurityCheckPoint;

		public override SecurityCheckpoint GetCheckPointForView(BusinessObject bizObject)
		{
			return (bizObject as IJobCostingPlugIn)?.CostSupporter.JobConsolCostingCheckPoint ?? Env.Security.None;
		}

		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizObject)
		{
			return (bizObject as IJobCostingPlugIn)?.CostSupporter.JobConsolCostingCheckPoint ?? Env.Security.None;
		}

		public override SecurityCheckpoint GetCheckPointForDelete(BusinessObject bizObject)
		{
			return (bizObject as IJobCostingPlugIn)?.CostSupporter.JobConsolCostingCheckPoint ?? Env.Security.None;
		}

		#endregion
	}
}
