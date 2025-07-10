using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.Business
{
	public abstract class CardContentBase<T> : IBizoCardContent
		where T : BusinessObject, ITagable
	{
		protected CardContentBase(T parentMember, ProcessTask task, ProcessHeader workflow, BMBoardSectionViewModel viewModel)
		{
			Argument.NotNull(parentMember, "parentMember");
			Argument.NotNull(task, "task");

			parent = parentMember;
			parentPk = parentMember.PK;

			this.viewModel = viewModel;

			if (viewModel == null)
			{
				cacheOverride = new PropertyCache();
			}

			taskPk = task.PK;

			if (workflow != null)
			{
				workflowPk = workflow.PK;
				lastEditTime = workflow.FH_SystemLastEditTimeUtc;
			}

			capacityDto = new CardCapacityDto(task, workflow, viewModel);

			ICardContentExtensions.SetDebugInformation(this, task, workflow);
		}

		readonly ICardCapacityDto capacityDto;
		readonly ZGuid taskPk;
		readonly ZGuid workflowPk;
		readonly ZGuid parentPk;
		readonly ZDateTime lastEditTime;
		readonly T parent;
		readonly BMBoardSectionViewModel viewModel;
		readonly PropertyCache cacheOverride;

		protected BMBoardSectionViewModel ViewModel
		{
			get { return viewModel; }
		}

		protected T Parent
		{
			get { return parent; }
		}

		protected PropertyCache Cache
		{
			get { return cacheOverride ?? viewModel.Cache; }
		}

		#region Tags

		public TagDefinitionCache Definitions
		{
			get { return Cache.GetCachedValue(ZGuid.Empty, BMBoardSectionViewModel.CacheConstants.TagDefinitions, () => TagProvider.GetAllTagDefinitions(Parent.Factory)); }
		}

		public ImmutableHashSet<ZGuid> ApplicableTagMagnitudes
		{
			get { return Cache.GetCachedValue(parentPk, TaskJobWorkflowCacheHelper.CacheConstants.ApplicableTags, () => GetApplicableTagMagnitudes(Parent)); }
		}

		static ImmutableHashSet<ZGuid> GetApplicableTagMagnitudes(ITagable tagable)
		{
			if (tagable != null)
			{
				return new HashSet<ZGuid>(tagable.GetApplicableTags().Select(m => m.PK)).ToImmutableHashSet();
			}
			else
			{
				return new HashSet<ZGuid>().ToImmutableHashSet();
			}
		}

		#endregion

		#region IBizoCardContentMembers

		public abstract ProcessTask Task { get; }
		public abstract ProcessHeader Workflow { get; }

		#endregion

		#region ICardContent Members

		public TValue GetCustomAttribute<TValue>(StaticControlProperty key)
		{
			var task = Task;
			var workflow = Workflow;

			if (task != null && !task.IsDeleted && workflow != null && !workflow.IsDeleted)
			{
				var showJobCards = viewModel != null && viewModel.ShowJobCards;
				var showWorkflowCards = viewModel != null && viewModel.ShowWorkflowCards;

				return StaticCustomisationLineCache.GetValue<TValue>(key, task, workflow, showJobCards, showWorkflowCards);
			}
			else
			{
				return default(TValue);
			}
		}

		public ZGuid Identifier
		{
			get { return parentPk; }
		}

		public ZGuid TaskIdentifier
		{
			get { return taskPk; }
		}

		public ZGuid WorkflowIdentifier
		{
			get { return workflowPk; }
		}

		public ZDateTime LastEditTime
		{
			get { return lastEditTime; }
		}

		public ZString WorkflowType
		{
			get { return Task.WorkflowType; }
		}

		public ICardCapacityDto CapacityDto
		{
			get { return capacityDto; }
		}

#if DEBUG
		public ZString DisplayTextForDebugging { get; set; }
#endif

		public abstract CardType CardType { get; }

		public abstract ZString NoteText { get; }
		public abstract SizedButtonBorderStyle BorderStyle { get; }
		public abstract Color BorderColor { get; }
		public abstract bool IsCurrent { get; }

		public object Bindable
		{
			get { return Task; }
		}

		public ITaskOrderable TaskOrderable
		{
			get { return new NaiveTaskOrderable(Task, viewModel); }
		}

		#endregion

		#region Implementation

		public override bool Equals(object obj)
		{
			var other = obj as CardContentBase<T>;
			if (other != null)
			{
				return Identifier.Equals(other.Identifier);
			}
			else
			{
				return base.Equals(obj);
			}
		}

		public override int GetHashCode()
		{
			return Identifier.GetHashCode();
		}

		#endregion
	}
}
