using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.GUI.WorkflowManagement;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	public class ProcessHeaderController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.ProcessHeader; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.ProcessHeader; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ProcessHeader); }
		}

		protected override Guid GetIDForBusinessEntity(IBusiness businessEntity)
		{
			var id = base.GetIDForBusinessEntity(businessEntity);
			var processHeader = (IProcessHeader)businessEntity;

			if (processHeader.FH_P0_Template.IsValid)
			{
				var template = Factory.Load<ProcessTaskTemplate>(processHeader.FH_P0_Template);
				if (template != null)
				{
					id = template.PK.ToGuid();
				}
			}
			else if (string.IsNullOrWhiteSpace(processHeader.FH_ParentTableCode))
			{
				var shape = Factory.LoadTop1<BMNCNShape>(new ZQuery(BMNCNShapeSchema.BNS_RelatedEntityID, processHeader.PK));
				if (shape != null)
				{
					id = shape.PK.ToGuid();
				}
			}
			else
			{
				var parent = processHeader.Parent;
				if (parent != null)
				{
					id = parent.PK.ToGuid();
				}
			}

			return id;
		}

		protected override string GetIDForFormCache(IBusiness businessEntity)
		{
			var id = base.GetIDForFormCache(businessEntity);
			var processHeader = (IProcessHeader)businessEntity;

			if (processHeader.FH_P0_Template.IsValid)
			{
				id = ControllerIDs.ProcessTemplates.ToString();
			}
			else if (string.IsNullOrWhiteSpace(processHeader.FH_ParentTableCode))
			{
				if (Factory.Exists(typeof(BMNCNShape), new ZQuery(BMNCNShapeSchema.BNS_RelatedEntityID, processHeader.PK), mergeDbAndCacheResult: false))
				{
					id = ControllerIDs.NetworkDiagram.ToString();
				}
			}
			else
			{
				var controller = GetController(processHeader);

				if (controller != null)
				{
					id = controller.ID.ToString();
				}
			}

			return id;
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var processHeader = businessEntity as IProcessHeader;
			IZForm form = null;
			try
			{
				var getter = new WorkflowParentFormGetter();
				form = getter.GetForm(processHeader, form);

				if (form != null)
				{
					LastShownForm = form;
					return null;
				}
			}
			catch (ModuleGuiNotSupportedException ex)
			{
				throw GetModuleGuiNotSupportedException(businessEntity, ex);
			}

			Globals.Message.Show(Res.GetString("ED00EFEF-D34F-4777-AEF9-19E8357AFF5F", @"Unable to find the job this workflow belongs to.
Technical details: parent table code = {0}", processHeader != null ? processHeader.FH_ParentTableCode : ZString.Empty));

			return form;
		}

		static ModuleGuiNotSupportedException GetModuleGuiNotSupportedException(IBusiness businessEntity, Exception innerException)
		{
			var processHeader = businessEntity as IProcessHeader;

			var parent = processHeader != null && processHeader.JobHeader != null
				? processHeader.JobHeader.Parent
				: null;

			var message = string.Format(CultureInfo.InvariantCulture, (NoResString)"Could not get form for entity type [{0}], parent table code [{1}], parent type [{2}], description [{3}].", // Developer Only Information
				businessEntity.GetType().Name,
				processHeader.FH_ParentTableCode,
				parent != null ? parent.GetType().FullName : (NoResString)"<Unknown>", // Developer Only Information
				processHeader != null ? processHeader.Description.ToString() : (NoResString)"<Unknown>"); // Developer Only Information

			return new ModuleGuiNotSupportedException(message, innerException);
		}

		protected override void SetControllerID(IZForm form, ControllerID proposedControllerID)
		{
			// The form has it's own ControllerID - we don't want to set it to ProcessHeader.
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
			get { return Env.Security.WorkflowHeaders; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.WorkflowHeadersDelete; }
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
			var processHeader = (ProcessHeader)bizObject;
			var controller = GetController(processHeader);

			return controller != null ? checkpointGetter(controller) : Env.Security.WorkflowTasks;
		}

		static ZController GetController(IProcessHeader processHeader)
		{
			var parent = processHeader.Parent;
			return parent != null ? WorkflowProviderHelper.GetControllerForWorkflowType(parent.WorkflowType) : null;
		}

		#endregion
	}
}
