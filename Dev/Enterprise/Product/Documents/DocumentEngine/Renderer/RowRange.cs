
namespace Enterprise.DocumentEngine
{
	internal class RowRange
	{
		public RowRange()
		{
		}

		public RowRange(int start, int end)
		{
			this.Start = start;
			this.End = end;
		}

		public int Start;
		public int End;
	}
}
