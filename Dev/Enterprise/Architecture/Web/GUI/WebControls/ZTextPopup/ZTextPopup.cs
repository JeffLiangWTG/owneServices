using System;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// This control provides an abstract class for displaying pop up
	/// </summary>	
	public abstract class ZTextPopup : ZTextBoxButton, IContainResources
	{
		#region Properties

		protected override string ButtonClickHandler
		{
			get
			{
				EnsureChildControls();
				return string.Format("ZTextPopup_ShowPopup('{0}', '{1}');{2}", TextBoxControl.ClientID, PopupID, AdditionalButtonClickHandler); // Javascript code
			}
		}

		protected virtual string AdditionalButtonClickHandler
		{
			get { return string.Empty; }
		}

		string fPopupID;
		protected virtual string PopupID
		{
			get
			{
				if (fPopupID == null)
				{
					fPopupID = string.Format("ZTextPopup{0}_{1}", GetType().Name, UniqueSuffixID); // Javascript code
				}
				return fPopupID;
			}
		}

		protected int UniqueSuffixID
		{
			get
			{
				if (fUniqueSuffixID == 0)
				{
					if (Page != null)
					{
						fUniqueSuffixID = Page.NextUniqueID;
					}
				}
				return fUniqueSuffixID;
			}
		}
		int fUniqueSuffixID;

		protected virtual Color PopupBackgroundColor
		{
			get { return Color.Empty; }
		}

		string fDisplayStyle;
		protected virtual string DisplayStyle
		{
			get
			{
				if (fDisplayStyle == null)
				{
					fDisplayStyle = "display:none";
				}
				return fDisplayStyle;
			}
		}

		ZPage BasePage
		{
			get { return Page; }
		}

		protected abstract Unit PopupWidth { get; }
		protected abstract Unit PopupHeight { get; }

		#endregion

		#region Control Overrides

		protected override string ButtonBackgroundStyle
		{
			get { return string.Empty; }
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			RenderScriptBlocks();
		}

		protected override void Render(HtmlTextWriter writer)
		{
			base.Render(writer);
			RenderPopupContainer(writer);
		}

		#endregion

		#region IContainResources Members

		#region Opener

		protected ZWebResource Opener
		{
			get
			{
				if (fOpener == null)
				{
					fOpener = new ZWebResource(typeof(ZTextPopup), "opener.js", BasePage);
				}

				return fOpener;
			}
		}
		ZWebResource fOpener;

		#endregion

		public override ZWebResourceCollection Resources
		{
			get
			{
				ZWebResourceCollection result = base.Resources;
				result.Add(Opener);
				return result;
			}
		}

		#endregion

		#region Implementation

		protected virtual void RenderScriptBlocks()
		{
			if (!Page.ZClientScript.IsClientScriptBlockRegistered(typeof(ZTextPopup), "ZTextPopup_Scripts"))
			{
				if (BasePage != null)
				{
					string scriptString = string.Format("<SCRIPT src='{0}'></SCRIPT>", Opener.FileName); // Javascript code
					Page.ZClientScript.RegisterClientScriptBlock(typeof(ZTextPopup), "ZTextPopup_Scripts", scriptString);
				}
			}
		}
		protected abstract void RenderPopupContainer(HtmlTextWriter writer);

		#endregion

		#region Internal Properties

		internal string AdditionalButtonClickHandlerInternal => AdditionalButtonClickHandler;
		internal string PopupIDInternal => PopupID;
		internal Color PopupBackgroundColorInternal => PopupBackgroundColor;
		internal string ButtonBackgroundStyleInternal => ButtonBackgroundStyle;
		internal string ButtonClickHandlerInternal => ButtonClickHandler;

		#endregion
	}
}
