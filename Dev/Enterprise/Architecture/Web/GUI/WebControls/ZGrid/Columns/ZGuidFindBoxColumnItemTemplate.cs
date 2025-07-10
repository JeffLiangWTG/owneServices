namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZGuidFindBoxColumnItemTemplate : ZItemTemplate
	{
		public ZGuidFindBoxColumnItemTemplate(ZGuidFindBoxColumn column)
			: base(column)
		{
		}

		new protected ZGuidFindBoxColumn Column
		{
			get { return base.Column as ZGuidFindBoxColumn; }
		}

		protected internal override ISelfBindingWebControl GetControl()
		{
			ISelfBindingWebControl result;
			if (string.IsNullOrEmpty(Column.BindToList))
			{
				result = new ZTextLabel();
			}
			else
			{
				var lookupCtrl = new ZGuidFindBoxLabel();
				lookupCtrl.BindToList = Column.BindToList;
				lookupCtrl.DisplayStyle = Column.DisplayStyle;
				result = lookupCtrl;
			}
			return result;
		}
	}
}
