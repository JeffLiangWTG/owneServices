using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Module
{
	public class ProcessTaskForJobController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.ProcessTasks; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.ProcessTaskForJob; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ProcessTask); }
		}

		protected override Guid GetIDForBusinessEntity(IBusiness businessEntity)
		{
			var id = base.GetIDForBusinessEntity(businessEntity);
			var processTask = (ProcessTask)businessEntity;
			var parent = processTask.Parent;

			if (parent != null)
			{
				id = parent.PK.ToGuid();
			}

			return id;
		}

		protected override string GetIDForFormCache(IBusiness businessEntity)
		{
			var id = base.GetIDForFormCache(businessEntity);
			var processTask = (ProcessTask)businessEntity;
			var controller = GetParentController(processTask);

			if (controller != null)
			{
				id = controller.ID.ToString();
			}

			return id;
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var form = WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(businessEntity);

			if (form != null)
			{
				LastShownForm = form;
				return null;
			}

			var task = businessEntity as ProcessTask;

			Globals.Message.Show(Res.GetString("9D7A3EAE-FF31-4E7E-8088-887D7634437C", @"Unable to find the job this task belongs to.
Technical details: parent table code = {0}", task?.P9_ParentTableCode));

			return form;
		}

		public override void SwitchToFormFor(IBusiness entity)
		{
			base.SwitchToFormFor(entity);
			WorkflowParentFormFactory.NavigateToWorkflowItem((IZForm)GetOpenedForm(entity), (ProcessTask)entity);
		}

		#region Security Checkpoints
		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { throw new InvalidOperationException("Checkpoints require context of the type of workflow parent business object"); }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { throw new InvalidOperationException("Checkpoints require context of the type of workflow parent business object"); }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { throw new ControllerShowNewFormNotSupportedException("ShowNewForm is not supported for this controller."); }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { throw new ControllerShowDeleteFormNotSupportedException("ShowDeleteForm is not supported for this controller."); }
		}

		public override SecurityCheckpoint GetCheckPointForView(BusinessObject bizObject)
		{
			return GetCheckpoint(bizObject, c => c.GetCheckPointForView(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizObject)
		{
			return GetCheckpoint(bizObject, c => c.GetCheckPointForEdit(bizObject));
		}

		static SecurityCheckpoint GetCheckpoint(BusinessObject bizObject, Func<ZController, SecurityCheckpoint> checkpointGetter)
		{
			var processTask = (ProcessTask)bizObject;
			var controller = GetParentController(processTask);

			return controller != null ? checkpointGetter(controller) : Env.Security.WorkflowTasks;
		}

		#endregion

		#region implementation

		static ZController GetParentController(ProcessTask processTask)
		{
			var parent = processTask.Parent;

			return parent != null ? WorkflowProviderHelper.GetControllerForWorkflowType(parent.WorkflowType) : null;
		}

		#endregion

		#region Not Supported

		public override IZForm ShowNewForm()
		{
			throw new ControllerShowNewFormNotSupportedException("ShowNewForm is not supported for this controller.");
		}

		public override IZForm ShowDeleteForm(BusinessObject businessEntity)
		{
			throw new ControllerShowDeleteFormNotSupportedException("ShowDeleteForm is not supported for this controller.");
		}

		#endregion
	}
}
