using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentVisualizer.Business
{
	sealed class EventInfo : IEvent
	{
		public EventInfo(StmEvent stmEvent)
		{
			Argument.NotNull(stmEvent, nameof(stmEvent));
			this.stmEvent = stmEvent;
		}

		readonly StmEvent stmEvent;

		public ZString Code => stmEvent.SE_Code;
		public ZString Description => stmEvent.SE_DescMultilingual;
	}
}