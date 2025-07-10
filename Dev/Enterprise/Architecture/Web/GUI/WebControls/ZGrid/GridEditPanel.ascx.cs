using System.Diagnostics.CodeAnalysis;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	///	Control to capture log in information
	/// </summary>
	public class GridEditPanel : BaseUserControl
	{
		public System.Web.UI.WebControls.Button CancelButton;
		public System.Web.UI.WebControls.Button SaveButton;

		#region Auto

		[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Probably not used, but raised WI00629332 to get Core team to investigate")]
		void InitializeComponent()
		{
			this.DataSourceAssemblyName = "Enterprise.ZArchitecture.Web.GUI";
			this.DataSourceTypeName = "Enterprise.ZArchitecture.Web.GUI.LoginManager";
		}

		#endregion

	}
}
