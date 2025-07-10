using CargoWise.Types;

namespace Enterprise.Messaging.Business
{
	public class DiagnosticNote
	{
		public DiagnosticNote(ZString noteText)
		{
			ConsolKey = noteText.SubstringSafe(0, 36);
			ActionPending = noteText.SubstringSafe(36, 1);
			Status = noteText.SubstringSafe(37, 3);
			MessagePK = new ZGuid(noteText.SubstringSafe(40, 36));
			LogMessage = noteText.SubstringSafe(76, 80);
			OutInterchangePK = new ZGuid(noteText.SubstringSafe(156, 36));
			OutMailPK = new ZGuid(noteText.SubstringSafe(192, 36));
		}

		public DiagnosticNote(ZString consolKey, ZString status, ZString actionPending, ZGuid messagePK)
		{
			ConsolKey = consolKey;
			Status = status;
			ActionPending = actionPending;
			MessagePK = messagePK;
		}

		public ZString ConsolKey { get; set; }
		public ZString ActionPending { get; set; }
		public ZString Status { get; set; }
		public ZGuid MessagePK { get; set; }
		public ZString LogMessage { get; set; }
		public ZGuid OutInterchangePK { get; set; }
		public ZGuid OutMailPK { get; set; }

		public override string ToString()
		{
			return ConsolKey.PadRight(36) + ActionPending.PadRight(1) + Status.PadRight(3) + MessagePK.ToString().PadRight(36) + LogMessage.Left(80).PadRight(80) + OutInterchangePK.ToString().PadRight(36) +
				OutMailPK.ToString().PadRight(36);
		}
	}
}
