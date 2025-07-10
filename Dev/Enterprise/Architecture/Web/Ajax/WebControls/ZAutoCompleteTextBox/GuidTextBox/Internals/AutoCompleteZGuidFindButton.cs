using System;
using System.Collections.Specialized;
using System.Web.UI.WebControls;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.GuidTextBox.Internals
{
	internal class AutoCompleteZGuidFindButton : ZGuidFindBox, IButtonWithHelper
	{
		#region Public

		public ZAutoCompleteTextBox AutoCompleteTextBox { get; set; }
		public AutoCompleteHelper Helper;

		#endregion

		#region Overrides

		#region Functionality

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			this.DisplayNotifications = false;
		}

		protected override string GetTextFromValue(IZType value)
		{
			return string.Empty;
		}

		protected override NameValueCollection AdditionalParameters
		{
			get
			{
				if (additionalParameters == null)
				{
					additionalParameters = base.AdditionalParameters;
					if (Helper is DependentBizOAutoCompleteHelper)
					{
						additionalParameters.Add(ZFilterPage.ParentPKQuery, ((DependentBizOAutoCompleteHelper)Helper).ParentPK.ToString());
					}
				}

				return additionalParameters;
			}
		}
		NameValueCollection additionalParameters;

		protected override string PopupID
		{
			get
			{
				if (Helper is DependentBizOAutoCompleteHelper)
				{
					return base.PopupID + this.ClientID;
				}
				else
				{
					return base.PopupID;
				}
			}
		}

		#endregion

		#region Appearance

		protected override Unit MinWidth
		{
			get { return Unit.Empty; }
		}

		public override Unit DefaultWidth
		{
			get { return Unit.Empty; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			TextBoxControl.Style["display"] = "none";
			if (AutoCompleteTextBox != null)
			{
				TextBoxControl.Attributes["AutoCompleteTextBox"] = AutoCompleteTextBox.ClientID;
			}

			ButtonControl.Attributes["tabindex"] = "-1";

			Width = ButtonWidth;
		}

		#endregion

#endregion

#region IButtonWithHelper Members

		public void SetHelper(AutoCompleteHelper helper)
		{
			this.Helper = helper;
		}

#endregion
	}
}
