namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZDropEditColumnItemTemplate : ZItemTemplate
	{
		public ZDropEditColumnItemTemplate(ZDropEditColumn column) : base(column)
		{
		}

		new ZDropEditColumn Column
		{
			get { return base.Column as ZDropEditColumn; }
		}

		protected internal override ISelfBindingWebControl GetControl()
		{
			ZCodeLookupLabel label = new ZCodeLookupLabel();
			label.BindToList = Column.BindToList;
			label.DisplayStyle = Column.DisplayStyle;
			return label;
		}
	}
}
