namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Class responsible for grid columns controls
	/// </summary>
	public class ZFindBoxColumnItemTemplate : ZItemTemplate
	{
		public ZFindBoxColumnItemTemplate(IGuidBindToListSupport column) : base(column as ZTemplateColumn)
		{
		}

		protected new IGuidBindToListSupport Column
		{
			get { return base.Column as IGuidBindToListSupport; }
		}

		protected internal override ISelfBindingWebControl GetControl()
		{
			ZFindBoxLabel label = new ZFindBoxLabel();
			label.BindToList = Column.BindToList;
			label.DisplayStyle = Column.DisplayStyle;
			return label;
		}
	}
}
