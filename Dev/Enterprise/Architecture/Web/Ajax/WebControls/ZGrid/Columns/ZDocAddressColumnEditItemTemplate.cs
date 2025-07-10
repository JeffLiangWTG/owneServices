using System;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Summary description for ZTextEditColumnEditItemStyle.
	/// </summary>
	public class ZDocAddressColumnEditItemTemplate : ZDocAddressColumnItemTemplate, IEditItemTemplate
	{
		public ZDocAddressColumnEditItemTemplate(ZTemplateColumn column)
			: base(column)
		{
		}

		protected internal override ISelfBindingWebControl GetControl()
		{
			ZDocAddressWebControl docAddress = new ZDocAddressWebControl();
			docAddress.HorizonatalLayout = true;
			docAddress.GovermentRegNoVisible = Column.GovermentRegNoVisible;
			docAddress.PostDataChanged += new EventHandler(Column.Owner.OnPostDataChanged);
			return docAddress;
		}
	}
}
