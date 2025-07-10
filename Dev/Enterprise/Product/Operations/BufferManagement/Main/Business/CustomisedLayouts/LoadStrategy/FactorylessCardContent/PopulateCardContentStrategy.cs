using System.Drawing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.Business
{
	public abstract class PopulateCardContentStrategy
	{
		protected internal abstract ZGuid GetIdentifier(ProcessTask task, ProcessHeader workflow);
		protected internal abstract CardType CardType { get; }
		protected internal abstract ZString GetNoteText(ProcessTask task, PropertyCache cache);
		protected internal abstract bool GetIsCurrent(ICardContent cardContent, BMBoardSectionViewModel viewModel);
		protected internal abstract SizedButtonBorderStyle GetBorderStyle(ICardContent cardContent, ProcessTask task, ProcessHeader workflow, BMBoardSectionViewModel viewModel);
		protected internal abstract Color GetBorderColor(ICardContent cardContent, ProcessTask task, ProcessHeader workflow, BMBoardSectionViewModel viewModel);
		protected internal virtual ProcessHeader GetProcessHeaderForCardType(ProcessTask processTask, bool showJobCards)
		{
			return processTask.GetProcessHeaderForCardType(showJobCards);
		}
	}
}
