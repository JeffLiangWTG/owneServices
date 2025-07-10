using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public abstract class ZPageConfirmation
	{
		protected static string YesButtonCaption { get { return Res.GetString("34183f2a-b10f-4f5f-8cc9-bcec7089f6cb", "Yes"); } }
		protected static string NoButtonCaption { get { return Res.GetString("b5541ad2-1565-4d67-84a3-abf46d621c2b", "No"); } }

		public enum ConfirmationTypes
		{
			None,
			Warning,
			Error
		}

		public ZPageConfirmation(ZPage page) => this.page = page;

		public bool YesButtonClicked => ResponseHolder.Value == YesButtonCaption;

		public bool NoButtonClicked => ResponseHolder.Value == NoButtonCaption;

		public bool IsSaveConfirmation = true;

		public HiddenField ResponseHolder => responseHolder ?? (responseHolder = new HiddenField() { ID = UserReponseHolderID });

		HiddenField responseHolder;

		public string Title => GetTitle();

		public string ConfirmationImageURL => GetConfirmationImageURL();

		public List<ZPageConfirmationButton> Buttons => GetButtons();

		public ZPage Page => page;

		public bool HasResponse => !string.IsNullOrEmpty(ResponseHolder.Value);

		public bool IsRequired => GetRequired();

		public string UserReponseHolderID => GetUserResponseHolderID();

		public string ConfirmationMessage => GetConfirmationMessage();

		public void RenderControl(HtmlTextWriter writer) => ResponseHolder.RenderControl(writer);

		public void HandleResponse() => HandleResponseCore();

		protected virtual void HandleResponseCore() { }

		protected virtual BusinessObject DataSource => Page?.DataSource;

		readonly ZPage page;

		protected virtual string GetConfirmationImageURL()
		{
			if (!string.IsNullOrEmpty(GetConfirmationTypeImage()))
			{
				ZWebResource resource = new ZWebResource(typeof(ZPageConfirmation), GetConfirmationTypeImage(), Page);
				resource.Extract();

				return resource.FileName;
			}

			return "";
		}

		string GetConfirmationTypeImage()
		{
			switch (ConfirmationType)
			{
				case ConfirmationTypes.Warning:
					return "warning.png";
				case ConfirmationTypes.Error:
					return "error.png";
			}

			return "";
		}

		protected virtual ConfirmationTypes ConfirmationType => ConfirmationTypes.None;

		public virtual bool AlwaysRegisterConfirmationScript => false;

		protected abstract string GetTitle();

		protected abstract string GetUserResponseHolderID();

		protected abstract string GetConfirmationMessage();

		protected abstract bool GetRequired();

		protected virtual List<ZPageConfirmationButton> GetButtons()
		{
			var buttonClickScript = ButtonClickScript;
			var result = new List<ZPageConfirmationButton>() { new ZPageConfirmationButton(YesButtonCaption, buttonClickScript), new ZPageConfirmationButton(NoButtonCaption, buttonClickScript) };

			return result;
		}

		string ButtonClickScript => IsSaveConfirmation ? ButtonClickSaveScript : ButtonClickPostbackScript;

		string ButtonClickPostbackScript => $"__doPostBack();return true;"; // javascript code should not be translated

		string ButtonClickSaveScript => $"$(\\'{Page.SaveButtonClientID}\\').click();"; // javascript code should not be translated

		public class ZPageConfirmationButton
		{
			public ZPageConfirmationButton(string caption)
				: this(caption, "")
			{
			}

			public ZPageConfirmationButton(string caption, string onClickScript)
			{
				Caption = caption;
				OnClickScript = onClickScript;
			}

			public string Caption { get; set; }

			public string OnClickScript { get; set; }
		}
	}
}
