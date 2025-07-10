using System.Web.UI;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZCheckBoxColumn : ZTemplateColumn
	{
		public ZCheckBoxColumn(string headerText, string bindTo) : base(headerText, bindTo)
		{
		}

		public ZCheckBoxColumn(string headerText, string bindTo, string sortExpression) : this(headerText, bindTo)
		{
			this.SortExpression = sortExpression;
		}

		protected internal override ITemplate GetItemTemplate()
		{
			return new ZTextEditColumnItemTemplate(this);
		}

		protected internal override ITemplate GetEditItemTemplate()
		{
			return new ZCheckBoxColumnEditItemTemplate(this);
		}

		public bool AutoPostBack
		{
			get { return fAutoPostBack; }
			set { fAutoPostBack = value; }
		}
		protected bool fAutoPostBack;
	}
}
