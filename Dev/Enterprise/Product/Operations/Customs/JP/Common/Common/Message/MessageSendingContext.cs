using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.JP.Common;

public enum SendTarget
{
	Normal,
	FlatFile
}

public interface IMessageSendingContext
{
	SendTarget SendTarget { get; }
	string ProcedureCode { get; }
	bool EnableMessageVisual { get; }
	string Action { get; }
	IEnumerable<CusEntryHeader> EntryHeadersToBeSent { get; }
	ZBool EndSendMessage { get; set; }
}

public sealed record MessageSendingContext : IMessageSendingContext
{
	public SendTarget SendTarget { get; set; } = SendTarget.Normal;

	public bool EnableMessageVisual { get; set; }

	public string ProcedureCode { get; set; } = string.Empty;

	public string Action { get; set; } = string.Empty;

	public ZBool EndSendMessage { get; set; }

	public IEnumerable<CusEntryHeader> EntryHeadersToBeSent { get; set; }
}
