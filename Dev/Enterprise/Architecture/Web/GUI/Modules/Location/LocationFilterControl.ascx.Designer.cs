using System;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.Modules.Location
{
	/// <summary>
	///		Summary description for LocationFilterControl.
	/// </summary>
	public partial class LocationFilterControl
	{
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
			LocationTypeLabel.BindTo = null;
			CodeLabel.BindTo = null;
			Code.BindTo = "Code";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Web.Modules.WebLocationFilterBusinessObject)(null)).Code);
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Web.Modules.WebLocationFilterBusinessObject)(null)).Code);
			Code.BindTo = "Code";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Web.Modules.WebLocationFilterBusinessObject)(null)).Code);
			DescriptionLabel.BindTo = null;
			Description.BindTo = "Description";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Web.Modules.WebLocationFilterBusinessObject)(null)).Description);
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Web.Modules.WebLocationFilterBusinessObject)(null)).Description);
			Description.BindTo = "Description";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Web.Modules.WebLocationFilterBusinessObject)(null)).Description);
			this.DataSourceAssemblyName = "Enterprise.MasterFiles.Module";
			this.DataSourceTypeName = "Enterprise.ZArchitecture.Web.Modules.WebLocationFilterBusinessObject";
			this.Load += new EventHandler(this.Page_Load);
		}
		#endregion
	}
}
