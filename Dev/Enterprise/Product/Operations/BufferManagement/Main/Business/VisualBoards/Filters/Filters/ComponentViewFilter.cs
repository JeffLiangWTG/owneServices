using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public class ComponentViewFilter : CardVisibilityFilter, IMultiOptionFilter
	{
		#region TaskVisibilityFilter Overrides

		public override bool AllowMultiple
		{
			get { return false; }
		}

		public override string FilterName
		{
			get
			{
				if (CurrentlySelectedOption != null)
				{
					var option = (BMComponent)CurrentlySelectedOption;
					return Res.GetString("c672ba9d-9890-4feb-a51b-448ff90997ac", "Showing items in component: {0}", option.FC_Name);
				}

				return Res.GetString("36b7a9bd-7fcf-4d27-b0f0-8c5381d8a8d0", "Component View Filter");
			}
		}

		public override void FetchForFilter(BMBoardSectionViewModel viewModel, BusinessObjectFactory factory, IEnumerable<ICardContent> cards)
		{
			ConstrainedModeHelper.FetchForConstraintStatus(factory, cards);
		}

		public override CellVisibilityApplicator GetCellVisibilityApplicator(BMBoardSectionViewModel viewModel, BusinessObjectFactory factory)
		{
			return (cardContent, cell) =>
				{
					if (viewModel != null && viewModel.ShowJobCards)
					{
						var jobLevelWorkflows = cardContent.GetWorkflow(factory) as ProcessJobHeader;
						if (jobLevelWorkflows != null)
						{
							return jobLevelWorkflows.ProcessHeaders.Any(workflow => IsWorkflowInSelectedComponent(workflow));
						}
						return false;
					}
					else if (viewModel != null && viewModel.ShowWorkflowCards)
					{
						var workflow = cardContent.GetWorkflow(factory);
						return IsWorkflowInSelectedComponent(workflow);
					}
					else
					{
						var task = cardContent.GetTask(factory);
						return IsTaskInSelectedComponent(task);
					}
				};
		}

		bool IsTaskInSelectedComponent(ProcessTask task)
		{
			if (task == null)
			{
				return false;
			}

			var workflow = task.GetProcessHeader();
			return IsWorkflowInSelectedComponent(workflow, penetratingTask: task);
		}

		bool IsWorkflowInSelectedComponent(ProcessHeader workflow, ProcessTask penetratingTask = null)
		{
			if (workflow == null)
			{
				return false;
			}
			else if (CurrentlySelectedOption != null)
			{
				var selectedComponent = (BMComponent)CurrentlySelectedOption;

				if (workflow.CurrentComponent.PK == selectedComponent.PK)
				{
					return true;
				}
				else if (selectedComponent.IsChildBuffer || selectedComponent.IsConstraint)
				{
					var penetratedComponentPKs = workflow.GetPenetratedComponents(penetratingTask).Select(s => s.PK);
					return penetratedComponentPKs.Contains(selectedComponent.PK);
				}

				return false;
			}

			return true;
		}

		#endregion

		#region IMultiOptionFilter Members

		public object CurrentlySelectedOption { get; set; }

		#endregion

		#region Object Overrides

		public override bool Equals(object obj)
		{
			return obj is ComponentViewFilter;
		}

		public override int GetHashCode()
		{
			return typeof(ComponentViewFilter).GetHashCode();
		}

		#endregion
	}
}
