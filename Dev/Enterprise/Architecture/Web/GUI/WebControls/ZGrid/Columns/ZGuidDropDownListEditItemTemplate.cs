namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZGuidDropDownListEditItemTemplate : ZDropDownListColumnEditItemTemplate
	{
		public ZGuidDropDownListEditItemTemplate(ZGuidDropDownListColumn column) : base(column)
		{
		}

		protected override ZDropDownList GetDropDownList()
		{
			return new ZGuidDropDownList();
		}
	}
}
