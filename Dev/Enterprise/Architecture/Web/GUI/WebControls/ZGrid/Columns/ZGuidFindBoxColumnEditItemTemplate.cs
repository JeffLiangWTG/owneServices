using System;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZGuidFindBoxColumnEditItemTemplate : ZItemTemplate, IEditItemTemplate
	{
		public ZGuidFindBoxColumnEditItemTemplate(ZGuidFindBoxColumn column) : base(column)
		{
		}

		new protected ZGuidFindBoxColumn Column
		{
			get { return base.Column as ZGuidFindBoxColumn; }
		}

		protected internal override ISelfBindingWebControl GetControl()
		{
			var edit = new ZGuidFindBox();
			edit.BindToList = Column.BindToList;
			edit.AutoPostBack = Column.AutoPostBack;
			edit.ModuleID = Column.ModuleID;
			edit.PostDataChanged += new EventHandler(Column.ZOwner.OnPostDataChanged);
			return edit;
		}
	}
}
