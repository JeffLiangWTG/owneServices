using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI
{
	public class WorkflowParentJobCreator
	{
		public WorkflowParentJobCreator(string workflowType)
		{
			Argument.NotNullOrEmpty(workflowType, "workflowType");
			this.workflowType = workflowType;
		}

		readonly string workflowType;

		public IWorkflowProvider Job
		{
			get { return (IWorkflowProvider)BusinessObject; }
		}

		public BusinessObject BusinessObject { get; private set; }

		public ProcessJobHeader CreateJob(BusinessObjectFactory factory, string name = null)
		{
			if (Job != null)
			{
				throw new InvalidOperationException("Job already created");
			}

			controller = WorkflowProviderHelper.GetControllerForWorkflowType(workflowType);
			var controllerInternals = (ZControllerInternals)controller;
			BusinessObject = controllerInternals?.GetNewBusinessEntityInFactory(factory) as BusinessObject;

			if (BusinessObject == null)
			{
				BusinessObject = factory.New(controller.TypeOfTopLevelBusinessObject);
			}

			var jobHeader = ProcessJobHeader.GetForParent(Job, factory, addDefaultProcessHeaderIfNone: false);
			if (jobHeader != null && !string.IsNullOrEmpty(name))
			{
				((IProposedNetworkEntity)jobHeader).JobName = name;
			}

			return jobHeader;
		}

		public ProcessJobHeader ShowNewFormAsDialogAndGetSaved()
		{
			if (BusinessObject == null)
			{
				throw new InvalidOperationException("Must create job first");
			}

			try
			{
				controller.ShowChildrenAsDialog = true;
				controller.ShowFormForNewEntity(BusinessObject);
			}
			finally
			{
				controller.ShowChildrenAsDialog = false;
			}

			if (BusinessObject.IsInDatabase)
			{
				return ProcessJobHeader.GetForParent(Job, BusinessObject.Factory);
			}
			else
			{
				return null;
			}
		}

		public void DeleteJob()
		{
			BusinessObject.Delete();
		}

		public ZForm ShowNewForm(Action formSaveAction)
		{
			if (BusinessObject == null)
			{
				throw new InvalidOperationException("Must create job first");
			}

			var form = (ZForm)controller.ShowFormForNewEntity(BusinessObject);

			if (formSaveAction != null && form != null)
			{
				HookFormSaveAction(form, formSaveAction);
			}

			return form;
		}

		static void HookFormSaveAction(ZForm form, Action formSaveAction)
		{
			EventHandler eventHandler = null;
			eventHandler = (s, e) =>
			{
				formSaveAction();
				form.Saved -= eventHandler;
			};

			form.Saved += eventHandler;
		}

		ZController controller;
	}
}
