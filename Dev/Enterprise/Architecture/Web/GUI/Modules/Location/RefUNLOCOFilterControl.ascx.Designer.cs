using System;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.Modules.Location
{
	public partial class RefUNLOCOFilterControl
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
			Description.BindTo = "RL_PortName";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Web.Modules.WebRefUNLOCOFilterBusinessObject)(null)).RL_PortName);
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Web.Modules.WebRefUNLOCOFilterBusinessObject)(null)).RL_PortName);
			Description.BindTo = "RL_PortName";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Web.Modules.WebRefUNLOCOFilterBusinessObject)(null)).RL_PortName);
			Ztextbox1.BindTo = "RL_Code";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Web.Modules.WebRefUNLOCOFilterBusinessObject)(null)).RL_Code);
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Web.Modules.WebRefUNLOCOFilterBusinessObject)(null)).RL_Code);
			Ztextbox1.BindTo = "RL_Code";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Web.Modules.WebRefUNLOCOFilterBusinessObject)(null)).RL_Code);
			Ztextbox2.BindTo = "RL_IATA";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Web.Modules.WebRefUNLOCOFilterBusinessObject)(null)).RL_IATA);
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Web.Modules.WebRefUNLOCOFilterBusinessObject)(null)).RL_IATA);
			Ztextbox2.BindTo = "RL_IATA";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Web.Modules.WebRefUNLOCOFilterBusinessObject)(null)).RL_IATA);
			this.DataSourceAssemblyName = "Enterprise.ZArchitecture.Web.Modules";
			this.DataSourceTypeName = "Enterprise.ZArchitecture.Web.Modules.WebRefUNLOCOFilterBusinessObject";
		}
		#endregion
	}
}
