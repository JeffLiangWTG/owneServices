namespace Enterprise.Core.Forms
{
	public delegate void QueryToolTipEventHandler(object sender, ToolTipInfo info);

	public class ToolTipInfo
	{
		public ToolTipInfo(System.Drawing.Point coords)
		{
			this.Coords = coords;
			Handled = false;
			DefaultLocation = false;
			ToolTipText = string.Empty;
			ToolTipCaption = "";
		}
		public bool Handled;
		public bool DefaultLocation;
		public string ToolTipText;
		public string ToolTipCaption;
		public System.Drawing.Point Coords;
	}

	public interface IDynamicToolTip
	{
		event QueryToolTipEventHandler QueryToolTip;
		void GetToolTip(ToolTipInfo info);
	}
}
