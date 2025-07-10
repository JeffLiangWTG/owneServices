using System;
using System.Drawing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	class DummyCardStrategy : PopulateCardContentStrategy
	{
		public DummyCardStrategy(bool forceWorkflowReturnNull)
		{
			ForceWorkflowReturnNull = forceWorkflowReturnNull;
		}

		bool ForceWorkflowReturnNull { get; }

		protected internal override CardType CardType => throw new NotImplementedException();

		protected internal override Color GetBorderColor(ICardContent cardContent, ProcessTask task, ProcessHeader workflow, BMBoardSectionViewModel viewModel)
		{
			throw new NotImplementedException("This is a dummy CardContentStrategy object. Please get an actual CardContentStrategy object, and try again");
		}

		protected internal override SizedButtonBorderStyle GetBorderStyle(ICardContent cardContent, ProcessTask task, ProcessHeader workflow, BMBoardSectionViewModel viewModel)
		{
			throw new NotImplementedException("This is a dummy CardContentStrategy object. Please get an actual CardContentStrategy object, and try again");
		}

		protected internal override ZGuid GetIdentifier(ProcessTask task, ProcessHeader workflow)
		{
			throw new NotImplementedException("This is a dummy CardContentStrategy object. Please get an actual CardContentStrategy object, and try again");
		}

		protected internal override bool GetIsCurrent(ICardContent cardContent, BMBoardSectionViewModel viewModel)
		{
			throw new NotImplementedException("This is a dummy CardContentStrategy object. Please get an actual CardContentStrategy object, and try again");
		}

		protected internal override ZString GetNoteText(ProcessTask task, PropertyCache cache)
		{
			throw new NotImplementedException("This is a dummy CardContentStrategy object. Please get an actual CardContentStrategy object, and try again");
		}

		protected internal override ProcessHeader GetProcessHeaderForCardType(ProcessTask processTask, bool showJobCards)
		{
			return ForceWorkflowReturnNull ? null : processTask.GetProcessHeaderForCardType(showJobCards);
		}
	}
}
