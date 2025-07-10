namespace Enterprise.DocumentEngine.Areas
{
	internal abstract class HeaderArea : Area
	{
		public HeaderArea(int start, int end, Report report, string parameters) : base(start, end, report, parameters)
		{
		}

		protected HeaderArea() { }
	}
}
