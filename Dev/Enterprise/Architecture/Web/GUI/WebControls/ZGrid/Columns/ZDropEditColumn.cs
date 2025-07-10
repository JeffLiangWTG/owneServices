using System.ComponentModel;
using System.Web.UI;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZDropEditColumn : ZTemplateColumn
	{
		public ZDropEditColumn(string headerText, string bindTo) : base(headerText, bindTo)
		{
		}

		protected internal override ITemplate GetItemTemplate()
		{
			return new ZDropEditColumnItemTemplate(this);
		}

		protected internal override ITemplate GetEditItemTemplate()
		{
			return new ZDropEditColumnEditItemTemplate(this) { ShowDescription = (DisplayStyle != OComboBoxDropDownStyle.CodeOnly) };
		}

		public string BindToList
		{
			get { return fBindToList; }
			set { fBindToList = value; }
		}
		string fBindToList = "";

		[DefaultValue(OComboBoxDropDownStyle.CodeOnly)]
		public OComboBoxDropDownStyle DisplayStyle
		{
			get { return fDisplayStyle; }
			set { fDisplayStyle = value; }
		}
		protected OComboBoxDropDownStyle fDisplayStyle;

		public string ValueFieldName
		{
			get { return fValueFieldName; }
			set { fValueFieldName = value; }
		}
		protected string fValueFieldName;

		public string CodeFieldName
		{
			get { return fCodeFieldName; }
			set { fCodeFieldName = value; }
		}
		protected string fCodeFieldName;

		public bool AutoPostBack
		{
			get { return fAutoPostBack; }
			set { fAutoPostBack = value; }
		}
		protected bool fAutoPostBack;
	}
}
