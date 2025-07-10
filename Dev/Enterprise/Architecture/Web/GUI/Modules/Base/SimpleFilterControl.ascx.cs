using System;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.Modules.Base
{
	public class SimpleFilterControl : BaseUserControl
	{
		protected ZRadioButton rbStartsWith;
		protected ZRadioButton rbContains;
		protected WebControls.ZTextBox tbSoughtText;

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}

		/// <summary>
		///		Required method for Designer support - do not modify
		///		the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			tbSoughtText.BindTo = "SoughtText";
		}
		#endregion
	}
}
