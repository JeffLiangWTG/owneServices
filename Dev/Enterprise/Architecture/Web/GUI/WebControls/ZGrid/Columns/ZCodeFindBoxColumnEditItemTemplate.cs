using System;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZCodeFindBoxColumnEditItemTemplate : ZItemTemplate, IEditItemTemplate
	{
		public ZCodeFindBoxColumnEditItemTemplate(ZCodeFindBoxColumn column) : base(column)
		{
		}

		new protected ZCodeFindBoxColumn Column
		{
			get { return base.Column as ZCodeFindBoxColumn; }
		}

		protected internal override ISelfBindingWebControl GetControl()
		{
			ZFindBox edit = new ZFindBox();
			edit.BindToList = Column.BindToList;
			edit.AutoPostBack = Column.AutoPostBack;
			edit.ModuleID = Column.ModuleID;
			edit.PostDataChanged += new EventHandler(Column.ZOwner.OnPostDataChanged);
			return edit;
		}
	}
}
