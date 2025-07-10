namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Item style for Text Column
	/// </summary>
	public class ZDocAddressColumnItemTemplate : ZItemTemplate
	{
		public ZDocAddressColumnItemTemplate(ZTemplateColumn column) : base(column)
		{
		}

		protected internal override ISelfBindingWebControl GetControl()
		{
			ZDocAddressWebControl docAddress = new ZDocAddressWebControl();
			docAddress.HorizonatalLayout = false;
			return docAddress;
		}

		public new ZDocAddressColumn Column
		{
			get
			{
				return (ZDocAddressColumn)base.Column;
			}
		}
	}
}
