namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZDropDownListColumnItemTemplate : ZItemTemplate
	{
		public ZDropDownListColumnItemTemplate(ZDropDownListColumn column) : base(column)
		{
		}

		new protected ZDropDownListColumn Column
		{
			get { return base.Column as ZDropDownListColumn; }
		}

		protected internal override ISelfBindingWebControl GetControl()
		{
			if (Column.ShowHint)
			{
				ZCodeLookupLabel label = new ZCodeLookupLabel();
				label.BindToList = Column.BindToList;
				label.DisplayStyle = Column.DisplayStyle;
				return label;
			}
			else
			{
				return new ZTextLabel();
			}
		}
	}
}
