namespace Enterprise.DocumentEngineCore.DocWrappers
{
	public class TitleCopyCountPair
	{
		public TitleCopyCountPair()
			: this("")
		{
		}

		public TitleCopyCountPair(string title)
			: this(title, 1)
		{
		}

		public TitleCopyCountPair(string title, short copyCount)
		{
			this.Title = title;
			this.CopyCount = copyCount;
		}

		public string Title;
		public short CopyCount;
	}
}
