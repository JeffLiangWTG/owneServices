using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.Business
{
	public class FactorylessCardContent : ICardContent
	{
		public FactorylessCardContent(ProcessHeader workflow, ProcessTask task, BMBoardSectionViewModel viewModel, CustomisedControlDataCache customisationData, TagDefinitionCache definitions, PopulateCardContentStrategy strategy)
		{
			var cache = viewModel.Cache;

			Identifier = strategy.GetIdentifier(task, workflow);
			TaskIdentifier = task.PK;
			WorkflowIdentifier = workflow.PK;
			CardType = strategy.CardType;
			WorkflowType = task.WorkflowType;
			LastEditTime = workflow.FH_SystemLastEditTimeUtc;
			noteText = strategy.GetNoteText(task, viewModel.Cache);
			IsCurrent = strategy.GetIsCurrent(this, viewModel);

			ApplicableTagMagnitudes = TaskJobWorkflowCacheHelper.GetApplicableTagMagnitudes(Identifier, cache) ?? new HashSet<ZGuid>().ToImmutableHashSet();
			CustomisedControlData = customisationData;
			Bindable = new CardDto(this, customisationData.LineCaches);
			TaskOrderable = new TaskOrderableSnapshot(this, task, cache);
			CapacityDto = new CardCapacityDto(task, task.GetProcessHeader(), viewModel);
			Definitions = definitions;

			// Reliant on previous properties being set for TaskCard.
			borderStyle = strategy.GetBorderStyle(this, task, workflow, viewModel);
			borderColor = strategy.GetBorderColor(this, task, workflow, viewModel);

			ICardContentExtensions.SetDebugInformation(this, task, workflow);
		}

		public FactorylessCardContent(FactorylessCardContentDto dto)
			: this(dto.Workflow, dto.Task, dto.ViewModel, dto.CustomisationData, dto.Definitions, dto.Strategy)
		{
		}

		readonly ZString noteText;
		readonly SizedButtonBorderStyle borderStyle;
		readonly Color borderColor;

		#region Properties for Render logic

		public ZGuid Identifier { get; }
		public ZGuid TaskIdentifier { get; }
		public ZGuid WorkflowIdentifier { get; }
		public ZDateTime LastEditTime { get; }
		public ZString WorkflowType { get; }
		public object Bindable { get; }
		public CustomisedControlDataCache CustomisedControlData { get; }
		public ImmutableHashSet<ZGuid> ApplicableTagMagnitudes { get; }
		public bool IsCurrent { get; }
		public CardType CardType { get; }

		public ITaskOrderable TaskOrderable { get; }
		public ICardCapacityDto CapacityDto { get; }
		public TagDefinitionCache Definitions { get; }

#if DEBUG
		public ZString DisplayTextForDebugging { get; set; }
#endif

		#endregion

		#region ICardContent

		SizedButtonBorderStyle ICardContent.BorderStyle
		{
			get { return borderStyle; }
		}

		TValue ICardContent.GetCustomAttribute<TValue>(StaticControlProperty key)
		{
			return CustomisedControlData.GetValue<TValue>(key, this);
		}

		#endregion

		#region ICardContentBase

		Color ICardContentBase.BorderColor
		{
			get { return borderColor; }
		}

		ZString ICardContentBase.NoteText
		{
			get { return noteText; }
		}

		#endregion

		#region Implementation

		public override bool Equals(object obj)
		{
			var card = obj as ICardContent;
			if (card != null)
			{
				return card.Identifier == Identifier;
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

	public class FactorylessCardContentDto
	{
		public FactorylessCardContentDto(ProcessHeader workflow, ProcessTask task, BMBoardSectionViewModel viewModel, CustomisedControlDataCache customisationData, TagDefinitionCache definitions, PopulateCardContentStrategy strategy)
		{
			Workflow = workflow;
			Task = task;
			ViewModel = viewModel;
			CustomisationData = customisationData;
			Definitions = definitions;
			Strategy = strategy;
		}

		//if Strategy is of type PopulateTaskCardStrategy, it will always return the same task (IE, the one we put in). If it is of type PopulateworkflowCardStrategy, it does some extra calculation.
		public void SetTask(ProcessTask task)
		{
			Task = task;
		}

		public ProcessHeader Workflow { get; set; }
		public ProcessTask Task { get; set; }
		public BMBoardSectionViewModel ViewModel { get; set; }
		public CustomisedControlDataCache CustomisationData { get; set; }
		public TagDefinitionCache Definitions { get; set; }
		public PopulateCardContentStrategy Strategy { get; set; }
	}
}
