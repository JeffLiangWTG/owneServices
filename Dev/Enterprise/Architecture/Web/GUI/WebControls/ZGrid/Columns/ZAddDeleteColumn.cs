using System.Web.UI;
using System.Web.UI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// A Button Column that displays Delete buttons and an Add button in the Footer.
	/// </summary>
	public class ZAddDeleteColumn : TemplateColumn
	{
		public ZAddDeleteColumn(ButtonColumnType buttonType)
		{
			this.FooterTemplate = new ZDeleteColumnFooterTemplate(buttonType);
			this.ItemTemplate = new ZDeleteColumnItemTemplate(buttonType);
			ItemStyle.Width = Unit.Pixel(10);
		}
	}

	public class ZDeleteColumnItemTemplate : ITemplate
	{
		public ZDeleteColumnItemTemplate(ButtonColumnType buttonType) : base()
		{
			this.ButtonType = buttonType;
		}
		readonly ButtonColumnType ButtonType;
		#region ITemplate Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "html element id should not be translated")]
		public void InstantiateIn(Control container)
		{
			Control ctrl;
			if (ButtonType == ButtonColumnType.LinkButton)
			{
				ctrl = new LinkButton();
				((LinkButton)ctrl).Text = Res.GetString("39fd60ef-281c-40a4-9fbd-0ec3bc2b9318", "Delete");
				((LinkButton)ctrl).CommandName = "Delete";
			}
			else
			{
				ctrl = new Button();
				((Button)ctrl).Text = Res.GetString("39fd60ef-281c-40a4-9fbd-0ec3bc2b9318", "Delete");
				((Button)ctrl).CommandName = "Delete";
			}
			((WebControl)ctrl).ID = "ga";
			container.Controls.Add(ctrl);
		}

		#endregion

	}

	public class ZDeleteColumnFooterTemplate : ITemplate
	{
		public ZDeleteColumnFooterTemplate(ButtonColumnType buttonType) : base()
		{
			this.ButtonType = buttonType;
		}
		readonly ButtonColumnType ButtonType;
		#region ITemplate Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "html element id should not be translated")]
		public void InstantiateIn(Control container)
		{
			Control ctrl;
			if (ButtonType == ButtonColumnType.LinkButton)
			{
				ctrl = new LinkButton();
				((LinkButton)ctrl).Text = Res.GetString("de6b9efe-2a5c-4d67-b5c5-a51f90b0ff44", "Add");
				((LinkButton)ctrl).CommandName = "Insert";
			}
			else
			{
				ctrl = new Button();
				((Button)ctrl).Text = Res.GetString("de6b9efe-2a5c-4d67-b5c5-a51f90b0ff44", "Add");
				((Button)ctrl).CommandName = "Insert";
			}
			ctrl.ID = "ga";
			container.Controls.Add(ctrl);
		}

		#endregion
	}
}