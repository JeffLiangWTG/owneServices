namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Item template for ZDateTimeColumn
	/// </summary>
	public class ZDateTimeColumnItemTemplate : ZItemTemplate
	{
		public ZDateTimeColumnItemTemplate(ZDateTimeColumn column) : base(column)
		{
		}

		new ZDateTimeColumn Column
		{
			get { return base.Column as ZDateTimeColumn; }
		}

		protected internal override ISelfBindingWebControl GetControl()
		{
			ZDateTimeLabel label = new ZDateTimeLabel();
			label.DateTimeFormat = Column.DateTimeFormat;
			return label;
		}
	}
}
