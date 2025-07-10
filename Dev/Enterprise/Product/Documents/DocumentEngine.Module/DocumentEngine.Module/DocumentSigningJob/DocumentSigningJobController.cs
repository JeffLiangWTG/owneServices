using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.GUI;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.Module
{
	public class DocumentSigningJobController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.DocumentSigningJob; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.DocumentSigningJob; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(StmPrintJob); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new StmPrintJobForm((StmPrintJob)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.DocumentSigningJobs; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.DocumentSigningJobs; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.DocumentSigningJobs; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.DocumentSigningJobs; }
		}

		public override IZForm ShowViewForm(BusinessObject sourceEntity)
		{
			return null;
		}
	}
}
