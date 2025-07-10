using System;
using System.Web.UI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	#region SuppressResourceStringsCheckRegion

	public class ZLightBox : Panel
	{
		#region Constructors

		public ZLightBox()
			: base()
		{
			ID = "_LightBox";
		}

		#endregion

		#region New

		public new ZPage Page
		{
			get
			{
				return base.Page as ZPage;
			}
			set
			{
				base.Page = value;
			}
		}

		#endregion

		#region Overrides

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			Table boxTable = new Table();
			boxTable.CssClass = "LightBoxMessage";
			TableRow messageRow = new TableRow();
			TableRow buttonsRow = new TableRow();

			TableCell messageCell = new TableCell();
			messageCell.HorizontalAlign = HorizontalAlign.Center;
			messageCell.VerticalAlign = VerticalAlign.Top;
			messageCell.Controls.Add(MessageBox);
			messageRow.Cells.Add(messageCell);

			TableCell buttonsCell = new TableCell();
			buttonsCell.HorizontalAlign = HorizontalAlign.Center;
			buttonsCell.VerticalAlign = VerticalAlign.Middle;
			buttonsCell.Controls.Add(ButtonsBox);
			buttonsRow.Cells.Add(buttonsCell);

			boxTable.Rows.Add(messageRow);
			boxTable.Rows.Add(buttonsRow);

			this.Controls.Add(TitleBox);
			this.Controls.Add(boxTable);
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			CssClass = "LightBox";
			TitleBox.CssClass = "LightBoxTitle";
			Style["display"] = "none";
			Style["position"] = "absolute";
		}

		#endregion

		#region Implementation

		#region RenderScript

		const string ScriptKey = "LightBoxScript";

		public void RenderScript()
		{
			if (Page != null && Page.ZClientScript != null)
			{
				if (!Page.ZClientScript.IsClientScriptBlockRegistered(GetType(), ScriptKey))
				{
					string script =
						@"<SCRIPT>

                    function PositionLightBox()
                    {
                        var LightBox = $('" + ClientID + @"');
                        if (LightBox)
                        {
                            var LightBoxLeft = posLeft()+(pageWidth()-parseInt(LightBox.getStyle('width')))/2;
                            var LightBoxTop = posTop()+300;
                            if (LightBoxLeft<0)
                            {
                                LightBoxLeft=0;
                            }
                            $(LightBox).setStyle('left', LightBoxLeft + 'px');
                            $(LightBox).setStyle('top', LightBoxTop + 'px');
                        }
                    }                    

					function ShowLightBox(Title, Message, Buttons, ResponseHolderID, AutoPostBack, ImageURL)
					{
                        var LightBox = $('" + ClientID + @"');
                        if (LightBox)
                        {
                            var LightBoxMessage = $('" + MessageBox.ClientID + @"');
                            var TitleBoxMessage = $('" + TitleBox.ClientID + @"');
                            var LightBoxButtons = $('" + ButtonsBox.ClientID + @"');
                            if (LightBoxMessage && LightBoxButtons && TitleBoxMessage)
                            {
                                TitleBoxMessage.innerHTML = Title;
                                var MessageHTML = '<span class=""LightBoxMessageText"">' + Message + '</span>';
                                if (ImageURL!=null && ImageURL!='')
                                {
                                    MessageHTML='<table cellpading=""0"" cellspacing=""0""><tr><td valign=""top"" valign=""center"" width=""50""><img src=""' + ImageURL + '"" alt=""""></td><td valign=""top"" align=""center"">' + MessageHTML + '</td></tr></table>';
                                }
                                LightBoxMessage.innerHTML = MessageHTML;
                                var ButtonElement;
                                var ButtonsHTML = '';
                                var ButtonID = '';
                                var FirstButtonID = '';
                                for(var i=0; i<Buttons.length; i++)
                                {
                                    ButtonID = '" + ButtonsBox.ClientID + @"_' + i;
                                    if (i==0)
                                    {
                                        FirstButtonID=ButtonID;
                                    }
                                    ButtonElement = '<input type=""button"" id=""' + ButtonID + '"" value=""' + Buttons[i].caption + '"" style=""width: 100px;height:25px;margin:5px;"" onclick=""HideLightBox(); ' + Buttons[i].onclick + '"">';
                                    ButtonsHTML = ButtonsHTML + ButtonElement;
                                }
                                LightBoxButtons.innerHTML = ButtonsHTML;
                                DisablePage();
                                PositionLightBox();
                                $(LightBox).setStyle('z-index', '1700');
                                $(LightBox).setStyle('display', 'block');
                                setTimeout(""SetFocusOnLightBoxButton('"" + FirstButtonID + ""');"", 500);
                            }
                        }
					}

                    function SetFocusOnLightBoxButton(ButtonID)
                    {
                        try
                        {
                            $(ButtonID).focus();
                        }
                        catch(e) {}
                    }

                    AttachToScrollEvent(PositionLightBox);

                    AttachToResizeEvent(PositionLightBox);                    

					function HideLightBox()
					{
                        var LightBox = $('" + ClientID + @"');
                        if (LightBox)
                        {
                            $(LightBox).setStyle('display', 'none');
                        }
                        EnablePage();
					}

					</SCRIPT>";

					Page.ZClientScript.RegisterClientScriptBlock(GetType(), ScriptKey, script);
				}
			}
		}

		#endregion

		protected Panel TitleBox
		{
			get
			{
				if (titleBox == null)
				{
					titleBox = GetNewTitleBox();
					titleBox.ID = "TitleBox";
				}
				return titleBox;
			}
		}

		protected virtual Panel GetNewTitleBox()
		{
			return new Panel();
		}

		protected Panel MessageBox
		{
			get
			{
				if (messageBox == null)
				{
					messageBox = GetNewMessageBox();
					messageBox.ID = "MessageBox";
				}
				return messageBox;
			}
		}

		protected virtual Panel GetNewMessageBox()
		{
			return new Panel();
		}

		protected Panel ButtonsBox
		{
			get
			{
				if (buttonsBox == null)
				{
					buttonsBox = GetNewButtonsBox();
					buttonsBox.ID = "ButtonBox";
				}
				return buttonsBox;
			}
		}

		protected virtual Panel GetNewButtonsBox()
		{
			return new Panel();
		}

		Panel titleBox;
		Panel messageBox;
		Panel buttonsBox;

		#endregion
	}

	#endregion
}
