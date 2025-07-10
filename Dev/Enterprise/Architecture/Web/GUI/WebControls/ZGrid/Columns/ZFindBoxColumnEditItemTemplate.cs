using System;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZFindBoxColumnEditItemTemplate : ZFindBoxColumnItemTemplate, IEditItemTemplate
	{
		public ZFindBoxColumnEditItemTemplate(ZFindBoxColumn column) : base(column)
		{
		}

		protected new ZFindBoxColumn Column
		{
			get { return base.Column as ZFindBoxColumn; }
		}

		protected internal override ISelfBindingWebControl GetControl()
		{
			ZGuidFindBox edit = new ZGuidFindBox();
			edit.BindToList = Column.BindToList;
			edit.AutoPostBack = Column.AutoPostBack;
			edit.ModuleID = Column.ModuleID;
			edit.DisplayNotifications = Column.DisplayNotifications;
			edit.PostDataChanged += new EventHandler(Column.ZOwner.OnPostDataChanged);
			return edit;
		}
	}
}
