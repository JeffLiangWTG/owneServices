namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZCheckBoxColumnEditItemTemplate : ZItemTemplate, IEditItemTemplate
	{
		public ZCheckBoxColumnEditItemTemplate(ZTemplateColumn column) : base(column)
		{
		}

		protected new ZCheckBoxColumn Column
		{
			get { return base.Column as ZCheckBoxColumn; }
		}

		protected internal override ISelfBindingWebControl GetControl()
		{
			ZCheckBox checkBox = new ZCheckBox();
			checkBox.AutoPostBack = Column.AutoPostBack;
			return checkBox;
		}
	}
}
