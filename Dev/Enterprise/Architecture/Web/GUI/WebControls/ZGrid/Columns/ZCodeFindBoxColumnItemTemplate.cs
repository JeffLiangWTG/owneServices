namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZCodeFindBoxColumnItemTemplate : ZItemTemplate
	{
		public ZCodeFindBoxColumnItemTemplate(ZCodeFindBoxColumn column)
			: base(column)
		{
		}

		new protected ZCodeFindBoxColumn Column
		{
			get { return base.Column as ZCodeFindBoxColumn; }
		}

		protected internal override ISelfBindingWebControl GetControl()
		{
			ISelfBindingWebControl rezult;
			if (string.IsNullOrEmpty(Column.BindToList))
			{
				rezult = new ZTextLabel();
			}
			else
			{
				ZCodeFindBoxLabel lookupCtrl = new ZCodeFindBoxLabel();
				lookupCtrl.BindToList = Column.BindToList;
				lookupCtrl.DisplayStyle = Column.DisplayStyle;
				rezult = lookupCtrl;
			}
			return rezult;
		}
	}
}
