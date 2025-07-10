namespace Enterprise.VisualBoards.Business
{
	public class TabSpec
	{
		public string Text { get; set; }
		public string Name { get; set; }

		public object TabContentControl { get; set; }
		public bool IsAlwaysEnabled { get; set; }
	}
}
