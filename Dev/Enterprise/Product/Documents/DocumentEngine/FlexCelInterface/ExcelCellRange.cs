namespace Enterprise.DocumentEngine.FlexCelInterface
{
	class ExcelCellRange
	{
		internal ExcelCellRange(int top, int left, int bottom, int right)
		{
			this.Top = top;
			this.Left = left;
			this.Bottom = bottom;
			this.Right = right;
		}

		internal readonly int Top;
		internal readonly int Left;
		internal readonly int Bottom;
		internal readonly int Right;

		public override bool Equals(object obj)
		{
			var range = obj as ExcelCellRange;
			return range != null && range.Top == this.Top && range.Left == this.Left && range.Bottom == this.Bottom && range.Right == this.Right;
		}

		public override int GetHashCode()
		{
			return Top ^ Left ^ Bottom ^ Right;
		}
	}
}
