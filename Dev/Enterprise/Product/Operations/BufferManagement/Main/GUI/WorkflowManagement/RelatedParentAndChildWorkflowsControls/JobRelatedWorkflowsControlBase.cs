using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI
{
	public class JobRelatedWorkflowsControlBase : ZUserControl
	{
		protected void SetDataBindingCore(object dataSource, RelationshipDirection relationshipDirection)
		{
			if (!(dataSource is IWorkflowProviderCore workflowProvider) || !(dataSource is BusinessObject bizo))
			{
				return;
			}

			var jobHeader = ObjectFactory.Get<IProcessJobHeaderProvider>().GetForParent(workflowProvider, bizo.Factory) as ProcessJobHeader;

			if (jobHeader != null)
			{
				viewModel = new RelatedParentChildWorkflowsViewModel(jobHeader, relationshipDirection);
				base.SetDataBinding(viewModel, string.Empty);
			}
		}

		protected RelatedParentChildWorkflowsViewModel viewModel;

		protected void GridMouseDoubleClickCore(object sender, Func<ProcessHeaderLink, ProcessHeader> processHeaderGetter)
		{
			var diagramsGrid = (ZGrid)sender;
			var currentLink = diagramsGrid.ListManager.GetCurrent() as ProcessHeaderLink;

			if (currentLink != null)
			{
				var workflow = processHeaderGetter(currentLink);
				WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(workflow);
			}
		}

		protected virtual void ChooseProcessHeadersAndAddLinks(Action<IReadOnlyCollection<ProcessHeader>> handler)
		{
			var recordChooser = new ZRecordChooser<ProcessHeader>(ModuleIDs.ProcessHeader, new ProcessHeaderCollection(viewModel.Factory));
			recordChooser.ShowModal(ParentForm, selectedProcessHeaders => handler(selectedProcessHeaders));
		}
	}
}
