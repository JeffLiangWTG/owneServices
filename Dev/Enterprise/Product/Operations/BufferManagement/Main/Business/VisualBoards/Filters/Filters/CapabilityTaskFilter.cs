using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class CapabilityTaskFilter : CardVisibilityFilter, IMultiOptionFilter
	{
		public TaskCapabilityFilter TaskOption
		{
			get { return CurrentlySelectedOption == null ? TaskCapabilityFilter.None : (TaskCapabilityFilter)CurrentlySelectedOption; }
			set { CurrentlySelectedOption = value; }
		}

		#region BoardFilterBase Overrides

		public override bool AllowMultiple
		{
			get { return true; }
		}

		public override string FilterName
		{
			get
			{
				if (TaskOption == TaskCapabilityFilter.ShowOnlyUnassignedCapabilityTasks)
				{
					return Res.GetString("2b54129b-9155-4e8e-99a8-ce92e85584c5", "Showing Capability Tasks");
				}
				else if (TaskOption == TaskCapabilityFilter.ExcludeUnassignedCapabilityTasks)
				{
					return Res.GetString("53143b05-f65b-4f49-972a-f25d54e1a354", "Excluding Capability Tasks");
				}
				else
				{
					return Res.GetString("16178c3a-9a14-420d-85b5-2641eaecbfe1", "Showing All Tasks");
				}
			}
		}

		public override void FetchForFilter(BMBoardSectionViewModel viewModel, BusinessObjectFactory factory, IEnumerable<ICardContent> cards)
		{
			foreach (var card in cards)
			{
				factory.AddFetchHint(ProcessTasksSchema.PK, card.TaskIdentifier);
			}
		}

		public override CellVisibilityApplicator GetCellVisibilityApplicator(BMBoardSectionViewModel viewModel, BusinessObjectFactory factory)
		{
			return (cardContent, cell) =>
			{
				var task = cardContent.GetTask(factory);

				if (task != null)
				{
					switch (TaskOption)
					{
						case TaskCapabilityFilter.ShowOnlyUnassignedCapabilityTasks:
							return task.P9_GS_NKAssignedStaffMember.IsEmpty && task.P9_G4_RequiredCapability.IsValid;
						case TaskCapabilityFilter.ExcludeUnassignedCapabilityTasks:
							return !(task.P9_G4_RequiredCapability.IsValid && task.P9_GS_NKAssignedStaffMember.IsEmpty);
						default:
							return false;
					}
				}
				else
				{
					return false;
				}
			};
		}

		public override bool Equals(object obj)
		{
			return obj is CapabilityTaskFilter;
		}

		public override int GetHashCode()
		{
			return GetType().Name.GetHashCode();
		}

		#endregion

		#region IMultiOptionFilter Members

		public object CurrentlySelectedOption { get; set; }

		#endregion
	}
}
