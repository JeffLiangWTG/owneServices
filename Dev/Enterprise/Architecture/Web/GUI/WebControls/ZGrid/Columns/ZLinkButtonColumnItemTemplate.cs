namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZLinkButtonColumnItemTemplate : ZItemTemplate
	{
		public ZLinkButtonColumnItemTemplate(ZLinkButtonColumn column) : base(column)
		{
		}

		new ZLinkButtonColumn Column
		{
			get { return base.Column as ZLinkButtonColumn; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "javascript event handler name should not be translate")]
		protected internal override ISelfBindingWebControl GetControl()
		{
			ZLinkButton ctrl = new ZLinkButton();
			ctrl.Text = Column.HeaderText;
			ctrl.CommandName = Column.Command;
			if (!string.IsNullOrEmpty(Column.ClientClickHandler))
			{
				ctrl.Attributes.Add("onclick", Column.ClientClickHandler);
			}
			return ctrl;
		}
	}
}
