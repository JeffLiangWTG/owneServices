using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public partial class GridLayoutPage
	{
		#region Automatically generated

		protected ZListBox AvailableColumnsListBox;
		protected ZListBox CurrentLayoutListBox;

		public Button SelectOneButton;
		public Button RemoveOneButton;
		public Button MoveUpButton;
		public Button MoveDownButton;
		public Button DefaultButton;

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
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((GridLayoutContainer)(null)).SelectedAvailableColumn);
			AvailableColumnsListBox.BindTo = "SelectedAvailableColumn";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((GridLayoutContainer)(null)).AvailableColumns);
			AvailableColumnsListBox.BindToList = "AvailableColumns";
			AvailableColumnsListBox.DataValueField = "ColumnNumber";
			AvailableColumnsListBox.DataTextField = "HeaderText";

			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((GridLayoutContainer)(null)).SelectedLayoutColumn);
			CurrentLayoutListBox.BindTo = "SelectedLayoutColumn";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((GridLayoutContainer)(null)).CurrentLayout);
			CurrentLayoutListBox.BindToList = "CurrentLayout";
			CurrentLayoutListBox.DataValueField = "ColumnNumber";
			CurrentLayoutListBox.DataTextField = "HeaderText";
			CurrentLayoutListBox.AllowSorting = false;

			this.RemoveOneButton.Click += new EventHandler(this.RemoveOneButton_Click);
			this.SelectOneButton.Click += new EventHandler(this.SelectOneButton_Click);
			this.DefaultButton.Click += new EventHandler(this.DefaultButton_Click);
			this.MoveUpButton.Click += new EventHandler(this.MoveUpButton_Click);
			this.MoveDownButton.Click += new EventHandler(this.MoveDownButton_Click);

			this.DataSourceAssemblyName = "Enterprise.ZArchitecture.Web.Business";
			this.DataSourceTypeName = "Enterprise.ZArchitecture.Web.Business.Utilities.GridLayoutContainer";
		}

		#endregion

		#endregion
	}
}
