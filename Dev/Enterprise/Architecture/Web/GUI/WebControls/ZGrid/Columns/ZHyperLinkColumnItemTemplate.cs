namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZHyperLinkColumnItemTemplate : ZItemTemplate
	{
		public ZHyperLinkColumnItemTemplate(ZHyperLinkColumn column) : base(column)
		{
		}

		new ZHyperLinkColumn Column
		{
			get { return base.Column as ZHyperLinkColumn; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "javascript event handler name should not be translated")]
		protected internal override ISelfBindingWebControl GetControl()
		{
			ZHyperlink link = new ZHyperlink();
			link.BindTo = Column.BindTo;
			link.Text = Column.Text;

			link.ImageUrl = Column.ImageUrl;
			link.DataTextFields = Column.DataTextFields;
			link.DataImageUrlFields = Column.DataImageUrlFields;
			link.NavigateUrl = Column.NavigateUrl;
			link.DataNavigateUrlFields = Column.DataNavigateUrlFields;
			link.DataNavigateUrlFormatString = Column.DataNavigateUrlFormatString;
			link.DataTextFormatString = Column.DataTextFormatString;
			link.DataImageUrlFormatString = Column.DataImageUrlFormatString;

			if (!string.IsNullOrEmpty(Column.ClientClickHandler))
			{
				link.Attributes.Add("onclick", Column.ClientClickHandler);
			}

			link.IsExternalHyperLink = Column.IsExternalHyperlink;
			link.Target = Column.Target;
			link.WindowStyle = Column.WindowStyle;

			return link;
		}
	}
}
