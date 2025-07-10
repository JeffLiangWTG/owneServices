using System.ComponentModel;
using Enterprise.Customs.IN.Business;

namespace Enterprise.Customs.IN.GUI;

partial class NonStandardExchangeRatesGridUserControl
{
	/// <summary>
	/// Required designer variable.
	/// </summary>
	private IContainer components = null;

	/// <summary>
	/// Clean up any resources being used.
	/// </summary>
	/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
	protected override void Dispose(bool disposing)
	{
		if (disposing && (components != null))
		{
			components.Dispose();
		}

		base.Dispose(disposing);
	}

	#region Component Designer generated code

	/// <summary>
	/// Required method for Designer support - do not modify
	/// the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent()
	{
		this.NonStandardExchangeRatesGrid = new Enterprise.ZArchitecture.ZGrid();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		((System.ComponentModel.ISupportInitialize)(this.NonStandardExchangeRatesGrid)).BeginInit();
		this.NonStandardExchangeRatesGrid.SuspendLayout();
		this.SuspendLayout();
		//
		// BindingSource
		//
		this.BindingSource.DataSourceType = typeof(NonStandardExchangeRateCollection);
		//
		// NonStandardExchangeRatesGrid
		//
		this.NonStandardExchangeRatesGrid.AllowNavigation = false;
		this.BindingSource.SetBindingMember(this.NonStandardExchangeRatesGrid, ".");
		this.NonStandardExchangeRatesGrid.CaptionVisible = false;
		this.NonStandardExchangeRatesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
		this.NonStandardExchangeRatesGrid.GridId = "A0E57C59-63E2-4B98-A677-0C5C0BE7E6E9";
		this.NonStandardExchangeRatesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
		this.NonStandardExchangeRatesGrid.LayoutKey = "NonStandardExchangeRatesGrid";
		this.NonStandardExchangeRatesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.NonStandardExchangeRatesGrid.Name = "NonStandardExchangeRatesGrid";
		this.NonStandardExchangeRatesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(620, 135, true);
		this.NonStandardExchangeRatesGrid.TabIndex = 0;
		//
		// NonStandardExchangeRatesGridUserControl
		//
		this.CaptionRenderingEnabled = true;
		this.Controls.Add(this.NonStandardExchangeRatesGrid);
		this.Name = "NonStandardExchangeRatesGridUserControl";
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(620, 135, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		((System.ComponentModel.ISupportInitialize)(this.NonStandardExchangeRatesGrid)).EndInit();
		this.NonStandardExchangeRatesGrid.ResumeLayout(false);
		this.NonStandardExchangeRatesGrid.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();
	}

	#endregion

	protected internal ZArchitecture.ZGrid NonStandardExchangeRatesGrid;
}

