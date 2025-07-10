using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.Accounting.GUI.JobManagement;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Accounting.Module
{
	public class JobInvoicingFormController : JobManagementControllerBase
	{
		public JobInvoicingFormController()
		{
		}

		public override ControllerID ID => ControllerIDs.JobInvoicingForm;

		public override ModuleIdentifier ModuleID => ModuleIDs.JobManagement;

		public override Type TypeOfTopLevelBusinessObject => typeof(Job);

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			if (sourceEntity is Job job)
			{
				this.job = job;
			}

			return sourceEntity.IsInDatabase
				? base.ShowEditForm(sourceEntity)
				: ShowFormForNewEntity(sourceEntity);
		}

		#region CRM Security

		public override SecurityCheckpoint GetCheckPointForView(BusinessObject bizObject) => GetJobInvoicingCheckPoint(bizObject as Job);

		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizObject) => GetJobInvoicingCheckPoint(bizObject as Job);

		public override SecurityCheckpoint GetCheckPointForDelete(BusinessObject bizObject) => GetJobInvoicingCheckPoint(bizObject as Job);

		#endregion

		#region Security

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return jobInvoicingFormSecurityCheckPoint; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return jobInvoicingFormSecurityCheckPoint; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return jobInvoicingFormSecurityCheckPoint; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return jobInvoicingFormSecurityCheckPoint; }
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new InvoicingFormPlugin(businessEntity);

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			if (businessEntity is Job job)
			{
				this.job = job;

				return job.JobType?.SupportGlowBilling ?? false
					? new JobInvoicingForm(job)
					: base.GetForm(businessEntity);
			}

			return null;
		}

		protected override IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity) => sourceEntity;

		#endregion

		Job job;

		SecurityCheckpoint jobInvoicingFormSecurityCheckPoint => GetJobInvoicingCheckPoint(job);

		SecurityCheckpoint GetJobInvoicingCheckPoint(Job job) => job?.JobType?.JobInvoicingCheckPoint ?? Env.Security.None;
	}
}
