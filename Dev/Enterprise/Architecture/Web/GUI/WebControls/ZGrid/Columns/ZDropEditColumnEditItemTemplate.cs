namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZDropEditColumnEditItemTemplate : ZDropEditColumnItemTemplate, IEditItemTemplate
	{
		public ZDropEditColumnEditItemTemplate(ZDropEditColumn column) : base(column)
		{
		}

		protected new ZDropEditColumn Column
		{
			get { return base.Column as ZDropEditColumn; }
		}

		public bool ShowDescription
		{
			set { showDescription = value; }
		}
		bool showDescription;

		protected internal override ISelfBindingWebControl GetControl()
		{
			ZDropEditList edit = new ZDropEditList();
			edit.BindToList = Column.BindToList;
			edit.ShowDescription = showDescription;
			edit.AutoPostBack = Column.AutoPostBack;
			return edit;
		}
	}
}
