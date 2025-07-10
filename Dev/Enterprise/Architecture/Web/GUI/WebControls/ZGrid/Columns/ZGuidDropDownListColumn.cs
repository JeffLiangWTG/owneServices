using System.Web.UI;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZGuidDropDownListColumn : ZDropDownListColumn, IGuidBindToListSupport
	{
		public ZGuidDropDownListColumn(string headerText, string bindTo) : base(headerText, bindTo)
		{
		}

		public ZGuidDropDownListColumn(string headerText, string bindTo, string bindToList) : base(headerText, bindTo, bindToList)
		{ }

		public ZGuidDropDownListColumn(string headerText, string bindTo, OComboBoxDropDownStyle displayStyle)
			: base(headerText, bindTo, displayStyle)
		{ }

		protected internal override ITemplate GetItemTemplate()
		{
			return new ZFindBoxColumnItemTemplate(this);
		}

		protected internal override ITemplate GetEditItemTemplate()
		{
			return new ZGuidDropDownListEditItemTemplate(this);
		}
	}
}
