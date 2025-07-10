using System;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZDropDownListColumnEditItemTemplate : ZDropDownListColumnItemTemplate, IEditItemTemplate
	{
		public ZDropDownListColumnEditItemTemplate(ZDropDownListColumn column) : base(column)
		{
		}

		protected internal override ISelfBindingWebControl GetControl()
		{
			ZDropDownList edit = GetDropDownList();
			edit.BindToList = Column.BindToList;
			edit.DataTextField = Column.TextFieldName;
			edit.DataValueField = Column.ValueFieldName;
			edit.AutoPostBack = Column.AutoPostBack;
			edit.ShowEmptyItem = Column.ShowEmptyItem;
			edit.ID = Column.ID;
			edit.DisplayStyle = Column.ValueFieldName == "PK" ? OComboBoxDropDownStyle.DescriptionOnly : Column.DisplayStyle;
			if (edit.AutoPostBack)
			{
				edit.PostDataChanged += new EventHandler(Column.ZOwner.OnPostDataChanged);
			}
			return edit;
		}

		protected virtual ZDropDownList GetDropDownList()
		{
			return new ZDropDownList();
		}
	}
}
