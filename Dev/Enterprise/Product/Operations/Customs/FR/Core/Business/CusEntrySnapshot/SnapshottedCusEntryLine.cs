using Enterprise.Customs.FR.Business.Interfaces.Snapshot;

namespace Enterprise.Customs.FR.Business.Snapshot
{
	public class SnapshottedCusEntryLine : ISnapshottedCusEntryLine
	{
		public SnapshottedCusEntryLine(int lineNumber, ChildData[] children)
		{
			LineNumber = lineNumber;
			Children = children;
		}

		public int LineNumber { get; }

		public IChildData[] Children { get; }
	}
}
