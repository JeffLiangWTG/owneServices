namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZTextLabelNoEncode : ZTextLabel
	{
		public ZTextLabelNoEncode()
		{
			base.EnableHtmlEncoding = false;
		}

		public ZTextLabelNoEncode(string text) : base(text)
		{
			base.EnableHtmlEncoding = false;
		}

		public new bool EnableHtmlEncoding
		{
			get { return base.EnableHtmlEncoding; }
		}
	}
}
