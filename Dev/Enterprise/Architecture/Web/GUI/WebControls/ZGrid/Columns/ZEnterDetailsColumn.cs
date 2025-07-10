using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// A Button Column that displays Delete buttons and an Add button in the Footer.
	/// </summary>
	public class ZEnterDetailsColumn : TemplateColumn
	{
		public ZEnterDetailsColumn(ButtonColumnType buttonType, string commandName)
			: this(buttonType, commandName, "")
		{
		}

		public ZEnterDetailsColumn(ButtonColumnType buttonType, string commandName, string caption)
		{
			this.FooterTemplate = new ZEnterDetailsColumnFooterTemplate(buttonType, commandName, caption);
		}
	}

	public class ZEnterDetailsColumnFooterTemplate : ITemplate
	{
		public ZEnterDetailsColumnFooterTemplate(ButtonColumnType buttonType, string commandName, string caption)
			: base()
		{
			this.ButtonType = buttonType;
			this.CommandName = commandName;
			this.Caption = caption;
		}

		readonly ButtonColumnType ButtonType;
		readonly string CommandName;
		readonly string Caption;

		#region ITemplate Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "html element id should not be translated")]
		public void InstantiateIn(Control container)
		{
			IButtonControl ctrl;
			if (ButtonType == ButtonColumnType.LinkButton)
			{
				ctrl = new LinkButton();
			}
			else
			{
				ctrl = new Button();
			}
			((WebControl)ctrl).ID = "ga";

			ctrl.Text = string.IsNullOrEmpty(CaptionText) ?
						Res.GetString("bba89852-11ad-4978-9571-5cc34098a595", "Enter Details") :
						Res.GetString("e0d2fdee-c8b4-41e3-9d82-22e6b98ef448", "Enter {0} Details", CaptionText);

			ctrl.CommandName = CommandName;

			container.Controls.Add((Control)ctrl);
		}

		string CaptionText
		{
			get
			{
				string captionText;
				if (string.IsNullOrWhiteSpace(Caption))
				{
					captionText = "";
				}
				else
				{
					captionText = Caption.Trim();
					ZPage page = HttpContext.Current.Handler as ZPage;
					if (page != null)
					{
						captionText = page.Translate(captionText);
					}
				}
				return captionText;
			}
		}

		#endregion
	}
}
