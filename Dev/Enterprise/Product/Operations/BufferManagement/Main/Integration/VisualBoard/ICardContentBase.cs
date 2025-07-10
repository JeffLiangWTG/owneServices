using System.Drawing;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Integration
{
	public enum CardType
	{
		Task,
		Workflow,
	}

	public interface ICardContentBase
	{
		ZGuid Identifier { get; }
		ZGuid TaskIdentifier { get; }
		ZGuid WorkflowIdentifier { get; }

		ZString NoteText { get; }
		Color BorderColor { get; }

		ZDateTime LastEditTime { get; }
		bool IsCurrent { get; }

		CardType CardType { get; }

#if DEBUG
		ZString DisplayTextForDebugging { get; set; }
#endif
	}
}
