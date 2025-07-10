namespace Enterprise.Customs.FR.Business.Interfaces.Snapshot
{
	public interface ISnapshottedCusEntryLine
	{
		int LineNumber { get; }
		IChildData[] Children { get; }
	}
}
