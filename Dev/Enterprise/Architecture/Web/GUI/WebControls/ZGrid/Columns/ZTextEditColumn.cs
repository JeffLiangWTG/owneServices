using System.Web.UI;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Column to display bound text with edit functionality.
	/// </summary>
	public class ZTextEditColumn : ZTemplateColumn
	{
		public ZTextEditColumn(string headerText, string bindTo) : base(headerText, bindTo)
		{
			EnableHtmlEncoding = true;
		}

		protected internal override ITemplate GetItemTemplate()
		{
			return new ZTextEditColumnItemTemplate(this);
		}

		protected internal override ITemplate GetEditItemTemplate()
		{
			return new ZTextEditColumnEditItemTemplate(this);
		}

		public bool CanBeEnabledByClient { get; set; }

		public bool EnableHtmlEncoding
		{
			get { return fEnableHtmlEncoding; }
			set { fEnableHtmlEncoding = value; }
		}
		bool fEnableHtmlEncoding;

		public bool AutoPostBack
		{
			get { return fAutoPostBack; }
			set { fAutoPostBack = value; }
		}
		protected bool fAutoPostBack;
	}
}
