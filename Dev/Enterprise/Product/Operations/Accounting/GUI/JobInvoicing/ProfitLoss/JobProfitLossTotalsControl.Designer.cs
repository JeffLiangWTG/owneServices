using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class JobProfitLossTotalsControl
	{
		protected ZGroupBox TotalsGroupBox;
		private Enterprise.ZArchitecture.ZLabel MarginProfitCostLabel;
		private Enterprise.ZArchitecture.ZTextBox MarginProfitCost;
		private Enterprise.ZArchitecture.ZTextBox MarginProfitCostNotRecognized;
		private Enterprise.ZArchitecture.ZTextBox MarginProfitCostRecognized;
		private Enterprise.ZArchitecture.ZLabel MarginProfitRevLabel;
		private Enterprise.ZArchitecture.ZLabel MarginsLabel;
		private Enterprise.ZArchitecture.ZTextBox MarginProfitRev;
		private Enterprise.ZArchitecture.ZTextBox MarginProfitRevNotRecognized;
		private Enterprise.ZArchitecture.ZTextBox MarginProfitRevRecognized;
		private Enterprise.ZArchitecture.ZLabel ProfitLossLabel;
		private Enterprise.ZArchitecture.ZLabel AccrualLabel;
		private Enterprise.ZArchitecture.ZLabel CostLabel;
		private Enterprise.ZArchitecture.ZLabel WIPLabel;
		private Enterprise.ZArchitecture.ZLabel TotalLabel;
		private Enterprise.ZArchitecture.ZLabel NotRecognizedLabel;
		private Enterprise.ZArchitecture.ZLabel RecognizedLabel;
		private Enterprise.ZArchitecture.ZCalcEdit TotalRevenue;
		private Enterprise.ZArchitecture.ZCalcEdit TotalRevenueNotRecognized;
		private Enterprise.ZArchitecture.ZLabel RevenueLabel;
		private Enterprise.ZArchitecture.ZCalcEdit TotalRevenueRecognized;
		private Enterprise.ZArchitecture.ZCalcEdit TotalWIP;
		private Enterprise.ZArchitecture.ZCalcEdit TotalWIPNotRecognized;
		private Enterprise.ZArchitecture.ZCalcEdit TotalWIPRecognized;
		private Enterprise.ZArchitecture.ZCalcEdit TotalCost;
		private Enterprise.ZArchitecture.ZCalcEdit TotalCostNotRecognized;
		private Enterprise.ZArchitecture.ZCalcEdit TotalCostRecognized;
		private Enterprise.ZArchitecture.ZCalcEdit TotalAccrual;
		private Enterprise.ZArchitecture.ZCalcEdit TotalAccrualNotRecognized;
		private Enterprise.ZArchitecture.ZCalcEdit TotalAccrualRecognized;
		private Enterprise.ZArchitecture.ZCalcEdit TotalLineAmount;
		private Enterprise.ZArchitecture.ZCalcEdit TotalLineAmountNotRecognized;
		private Enterprise.ZArchitecture.ZCalcEdit TotalLineAmountRecognized;

		private void InitializeComponent()
		{
			this.TotalsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TaxExpensePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TotalTaxExpenseCost = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalTaxExpenseRevenue = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TaxExpenseLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MarginProfitCostLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MarginProfitCost = new Enterprise.ZArchitecture.ZTextBox();
			this.MarginProfitCostNotRecognized = new Enterprise.ZArchitecture.ZTextBox();
			this.MarginProfitCostRecognized = new Enterprise.ZArchitecture.ZTextBox();
			this.MarginProfitRevLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MarginsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MarginProfitRev = new Enterprise.ZArchitecture.ZTextBox();
			this.MarginProfitRevNotRecognized = new Enterprise.ZArchitecture.ZTextBox();
			this.MarginProfitRevRecognized = new Enterprise.ZArchitecture.ZTextBox();
			this.ProfitLossLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AccrualLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CostLabel = new Enterprise.ZArchitecture.ZLabel();
			this.WIPLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TotalLabel = new Enterprise.ZArchitecture.ZLabel();
			this.NotRecognizedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.RecognizedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TotalRevenue = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalRevenueNotRecognized = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RevenueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TotalRevenueRecognized = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalWIP = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalWIPNotRecognized = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalWIPRecognized = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalCost = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalCostNotRecognized = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalCostRecognized = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalAccrual = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalAccrualNotRecognized = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalAccrualRecognized = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalLineAmount = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalLineAmountNotRecognized = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalLineAmountRecognized = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TotalsGroupBox.SuspendLayout();
			this.TaxExpensePanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Integration.IJobProfitLoss);
			// 
			// TotalsGroupBox
			// 
			this.TotalsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossTotalsControl|865a5be0-ef06-4401-bbc6-444543a44a5e", "Job Profit Totals Analysis");
			this.TotalsGroupBox.Controls.Add(this.TaxExpensePanel);
			this.TotalsGroupBox.Controls.Add(this.MarginProfitCostLabel);
			this.TotalsGroupBox.Controls.Add(this.MarginProfitCost);
			this.TotalsGroupBox.Controls.Add(this.MarginProfitCostNotRecognized);
			this.TotalsGroupBox.Controls.Add(this.MarginProfitCostRecognized);
			this.TotalsGroupBox.Controls.Add(this.MarginProfitRevLabel);
			this.TotalsGroupBox.Controls.Add(this.MarginsLabel);
			this.TotalsGroupBox.Controls.Add(this.MarginProfitRev);
			this.TotalsGroupBox.Controls.Add(this.MarginProfitRevNotRecognized);
			this.TotalsGroupBox.Controls.Add(this.MarginProfitRevRecognized);
			this.TotalsGroupBox.Controls.Add(this.ProfitLossLabel);
			this.TotalsGroupBox.Controls.Add(this.AccrualLabel);
			this.TotalsGroupBox.Controls.Add(this.CostLabel);
			this.TotalsGroupBox.Controls.Add(this.WIPLabel);
			this.TotalsGroupBox.Controls.Add(this.TotalLabel);
			this.TotalsGroupBox.Controls.Add(this.NotRecognizedLabel);
			this.TotalsGroupBox.Controls.Add(this.RecognizedLabel);
			this.TotalsGroupBox.Controls.Add(this.TotalRevenue);
			this.TotalsGroupBox.Controls.Add(this.TotalRevenueNotRecognized);
			this.TotalsGroupBox.Controls.Add(this.RevenueLabel);
			this.TotalsGroupBox.Controls.Add(this.TotalRevenueRecognized);
			this.TotalsGroupBox.Controls.Add(this.TotalWIP);
			this.TotalsGroupBox.Controls.Add(this.TotalWIPNotRecognized);
			this.TotalsGroupBox.Controls.Add(this.TotalWIPRecognized);
			this.TotalsGroupBox.Controls.Add(this.TotalCost);
			this.TotalsGroupBox.Controls.Add(this.TotalCostNotRecognized);
			this.TotalsGroupBox.Controls.Add(this.TotalCostRecognized);
			this.TotalsGroupBox.Controls.Add(this.TotalAccrual);
			this.TotalsGroupBox.Controls.Add(this.TotalAccrualNotRecognized);
			this.TotalsGroupBox.Controls.Add(this.TotalAccrualRecognized);
			this.TotalsGroupBox.Controls.Add(this.TotalLineAmount);
			this.TotalsGroupBox.Controls.Add(this.TotalLineAmountNotRecognized);
			this.TotalsGroupBox.Controls.Add(this.TotalLineAmountRecognized);
			this.TotalsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TotalsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TotalsGroupBox.Name = "TotalsGroupBox";
			this.TotalsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(873, 142, true);
			this.TotalsGroupBox.TabIndex = 0;
			this.TotalsGroupBox.TabStop = false;
			// 
			// TaxExpensePanel
			// 
			this.TaxExpensePanel.Controls.Add(this.TotalTaxExpenseCost);
			this.TaxExpensePanel.Controls.Add(this.TotalTaxExpenseRevenue);
			this.TaxExpensePanel.Controls.Add(this.TaxExpenseLabel);
			this.TaxExpensePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 108, true);
			this.TaxExpensePanel.Name = "TaxExpensePanel";
			this.TaxExpensePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 24, true);
			this.TaxExpensePanel.TabIndex = 34;
			// 
			// TotalTaxExpenseCost
			// 
			this.TotalTaxExpenseCost.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TotalTaxExpenseCost, "TotalTaxExpenseCost");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Integration.IJobProfitLoss)(null)).TotalTaxExpenseCost)));
			this.TotalTaxExpenseCost.BindToDecimalPlaces = "Decimals";
			this.TotalTaxExpenseCost.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.TotalTaxExpenseCost.CaptionResourceString = null;
			this.TotalTaxExpenseCost.DecimalPlaces = 2;
			this.TotalTaxExpenseCost.Font = new System.Drawing.Font("Tahoma", 7.5F, System.Drawing.FontStyle.Bold);
			this.TotalTaxExpenseCost.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(309, 1, true);
			this.TotalTaxExpenseCost.Name = "TotalTaxExpenseCost";
			this.TotalTaxExpenseCost.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TotalTaxExpenseCost.TabIndex = 15;
			this.TotalTaxExpenseCost.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalTaxExpenseRevenue
			// 
			this.TotalTaxExpenseRevenue.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TotalTaxExpenseRevenue, "TotalTaxExpenseRevenue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Integration.IJobProfitLoss)(null)).TotalTaxExpenseRevenue)));
			this.TotalTaxExpenseRevenue.BindToDecimalPlaces = "Decimals";
			this.TotalTaxExpenseRevenue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.TotalTaxExpenseRevenue.CaptionResourceString = null;
			this.TotalTaxExpenseRevenue.DecimalPlaces = 2;
			this.TotalTaxExpenseRevenue.Font = new System.Drawing.Font("Tahoma", 7.5F, System.Drawing.FontStyle.Bold);
			this.TotalTaxExpenseRevenue.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 1, true);
			this.TotalTaxExpenseRevenue.Name = "TotalTaxExpenseRevenue";
			this.TotalTaxExpenseRevenue.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TotalTaxExpenseRevenue.TabIndex = 7;
			this.TotalTaxExpenseRevenue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TaxExpenseLabel
			// 
			this.TaxExpenseLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("73a40011-4f57-4428-b1e3-a471a2ed8faa", "Total Expense");
			this.TaxExpenseLabel.Dock = System.Windows.Forms.DockStyle.Left;
			this.TaxExpenseLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.TaxExpenseLabel.IsFontBold = true;
			this.TaxExpenseLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TaxExpenseLabel.Name = "TaxExpenseLabel";
			this.TaxExpenseLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 24, true);
			this.TaxExpenseLabel.TabIndex = 3;
			// 
			// MarginProfitCostLabel
			// 
			this.MarginProfitCostLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossTotalsControl|518cebd7-df00-45f4-8316-2c3953e95eaf", "Profit/Cost");
			this.MarginProfitCostLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.MarginProfitCostLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(754, 26, true);
			this.MarginProfitCostLabel.Name = "MarginProfitCostLabel";
			this.MarginProfitCostLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.MarginProfitCostLabel.TabIndex = 28;
			this.MarginProfitCostLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// MarginProfitCost
			// 
			this.MarginProfitCost.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.MarginProfitCost, "MarginProfitCost");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Integration.IJobProfitLoss)(null)).MarginProfitCost)));
			this.MarginProfitCost.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.MarginProfitCost.CaptionResourceString = null;
			this.MarginProfitCost.Font = new System.Drawing.Font("Tahoma", 7.5F, System.Drawing.FontStyle.Bold);
			this.MarginProfitCost.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(754, 88, true);
			this.MarginProfitCost.Name = "MarginProfitCost";
			this.MarginProfitCost.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.MarginProfitCost.TabIndex = 31;
			this.MarginProfitCost.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MarginProfitCostNotRecognized
			// 
			this.MarginProfitCostNotRecognized.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.MarginProfitCostNotRecognized, "MarginProfitCostNotRecognized");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Integration.IJobProfitLoss)(null)).MarginProfitCostNotRecognized)));
			this.MarginProfitCostNotRecognized.CaptionResourceString = null;
			this.MarginProfitCostNotRecognized.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(754, 68, true);
			this.MarginProfitCostNotRecognized.Name = "MarginProfitCostNotRecognized";
			this.MarginProfitCostNotRecognized.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.MarginProfitCostNotRecognized.TabIndex = 30;
			this.MarginProfitCostNotRecognized.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MarginProfitCostRecognized
			// 
			this.MarginProfitCostRecognized.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.MarginProfitCostRecognized, "MarginProfitCostRecognized");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Integration.IJobProfitLoss)(null)).MarginProfitCostRecognized)));
			this.MarginProfitCostRecognized.CaptionResourceString = null;
			this.MarginProfitCostRecognized.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(754, 48, true);
			this.MarginProfitCostRecognized.Name = "MarginProfitCostRecognized";
			this.MarginProfitCostRecognized.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.MarginProfitCostRecognized.TabIndex = 29;
			this.MarginProfitCostRecognized.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MarginProfitRevLabel
			// 
			this.MarginProfitRevLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossTotalsControl|4d33f670-14e7-4fa3-80d8-37bb3f75fb56", "Profit/Rev");
			this.MarginProfitRevLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.MarginProfitRevLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(648, 26, true);
			this.MarginProfitRevLabel.Name = "MarginProfitRevLabel";
			this.MarginProfitRevLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.MarginProfitRevLabel.TabIndex = 24;
			this.MarginProfitRevLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// MarginsLabel
			// 
			this.MarginsLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossTotalsControl|adc84fd3-77dd-4143-827f-43fe6861b4e2", "Margins");
			this.MarginsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.MarginsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(720, 9, true);
			this.MarginsLabel.Name = "MarginsLabel";
			this.MarginsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 18, true);
			this.MarginsLabel.TabIndex = 23;
			this.MarginsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// MarginProfitRev
			// 
			this.MarginProfitRev.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.MarginProfitRev, "MarginProfitRev");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Integration.IJobProfitLoss)(null)).MarginProfitRev)));
			this.MarginProfitRev.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.MarginProfitRev.CaptionResourceString = null;
			this.MarginProfitRev.Font = new System.Drawing.Font("Tahoma", 7.5F, System.Drawing.FontStyle.Bold);
			this.MarginProfitRev.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(648, 88, true);
			this.MarginProfitRev.Name = "MarginProfitRev";
			this.MarginProfitRev.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.MarginProfitRev.TabIndex = 27;
			this.MarginProfitRev.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MarginProfitRevNotRecognized
			// 
			this.MarginProfitRevNotRecognized.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.MarginProfitRevNotRecognized, "MarginProfitRevNotRecognized");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Integration.IJobProfitLoss)(null)).MarginProfitRevNotRecognized)));
			this.MarginProfitRevNotRecognized.CaptionResourceString = null;
			this.MarginProfitRevNotRecognized.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(648, 68, true);
			this.MarginProfitRevNotRecognized.Name = "MarginProfitRevNotRecognized";
			this.MarginProfitRevNotRecognized.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.MarginProfitRevNotRecognized.TabIndex = 26;
			this.MarginProfitRevNotRecognized.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MarginProfitRevRecognized
			// 
			this.MarginProfitRevRecognized.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.MarginProfitRevRecognized, "MarginProfitRevRecognized");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Integration.IJobProfitLoss)(null)).MarginProfitRevRecognized)));
			this.MarginProfitRevRecognized.CaptionResourceString = null;
			this.MarginProfitRevRecognized.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(648, 48, true);
			this.MarginProfitRevRecognized.Name = "MarginProfitRevRecognized";
			this.MarginProfitRevRecognized.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.MarginProfitRevRecognized.TabIndex = 25;
			this.MarginProfitRevRecognized.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ProfitLossLabel
			// 
			this.ProfitLossLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossTotalsControl|6c0aa4a5-e5ba-44f2-8da6-48accb510278", "Profit/Loss");
			this.ProfitLossLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ProfitLossLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(532, 26, true);
			this.ProfitLossLabel.Name = "ProfitLossLabel";
			this.ProfitLossLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ProfitLossLabel.TabIndex = 19;
			this.ProfitLossLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// AccrualLabel
			// 
			this.AccrualLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossTotalsControl|aab360d8-aada-436c-86af-ee77c5e1a3d6", "Accrual");
			this.AccrualLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.AccrualLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(426, 26, true);
			this.AccrualLabel.Name = "AccrualLabel";
			this.AccrualLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.AccrualLabel.TabIndex = 15;
			this.AccrualLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// CostLabel
			// 
			this.CostLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossTotalsControl|5234705a-5ad7-4fe8-9e86-a5525b1950ba", "Cost");
			this.CostLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CostLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 26, true);
			this.CostLabel.Name = "CostLabel";
			this.CostLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.CostLabel.TabIndex = 11;
			this.CostLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// WIPLabel
			// 
			this.WIPLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossTotalsControl|535e1b19-8423-4adb-b130-99ac38059b3a", "WIP");
			this.WIPLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.WIPLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(214, 26, true);
			this.WIPLabel.Name = "WIPLabel";
			this.WIPLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.WIPLabel.TabIndex = 7;
			this.WIPLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// TotalLabel
			// 
			this.TotalLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossTotalsControl|8f57b86a-522b-4ccc-901e-d091b56e7df0", "TOTAL");
			this.TotalLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.TotalLabel.IsFontBold = true;
			this.TotalLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 88, true);
			this.TotalLabel.Name = "TotalLabel";
			this.TotalLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 20, true);
			this.TotalLabel.TabIndex = 2;
			// 
			// NotRecognizedLabel
			// 
			this.NotRecognizedLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossTotalsControl|20383f8e-3836-4db1-b1a1-aa6b45e31c6d", "Not Recognized");
			this.NotRecognizedLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.NotRecognizedLabel.IsFontBold = true;
			this.NotRecognizedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 69, true);
			this.NotRecognizedLabel.Name = "NotRecognizedLabel";
			this.NotRecognizedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 20, true);
			this.NotRecognizedLabel.TabIndex = 1;
			// 
			// RecognizedLabel
			// 
			this.RecognizedLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossTotalsControl|a364c74a-f8a3-4843-a118-7ace77dd00b0", "Recognized");
			this.RecognizedLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.RecognizedLabel.IsFontBold = true;
			this.RecognizedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 48, true);
			this.RecognizedLabel.Name = "RecognizedLabel";
			this.RecognizedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 20, true);
			this.RecognizedLabel.TabIndex = 0;
			// 
			// TotalRevenue
			// 
			this.TotalRevenue.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TotalRevenue, "TotalRevenue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Integration.IJobProfitLoss)(null)).TotalRevenue)));
			this.TotalRevenue.BindToDecimalPlaces = "Decimals";
			this.TotalRevenue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.TotalRevenue.CaptionResourceString = null;
			this.TotalRevenue.DecimalPlaces = 2;
			this.TotalRevenue.Font = new System.Drawing.Font("Tahoma", 7.5F, System.Drawing.FontStyle.Bold);
			this.TotalRevenue.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 88, true);
			this.TotalRevenue.Name = "TotalRevenue";
			this.TotalRevenue.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TotalRevenue.TabIndex = 6;
			this.TotalRevenue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalRevenueNotRecognized
			// 
			this.TotalRevenueNotRecognized.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TotalRevenueNotRecognized, "TotalRevenueNotRecognized");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Integration.IJobProfitLoss)(null)).TotalRevenueNotRecognized)));
			this.TotalRevenueNotRecognized.BindToDecimalPlaces = "Decimals";
			this.TotalRevenueNotRecognized.CaptionResourceString = null;
			this.TotalRevenueNotRecognized.DecimalPlaces = 2;
			this.TotalRevenueNotRecognized.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 68, true);
			this.TotalRevenueNotRecognized.Name = "TotalRevenueNotRecognized";
			this.TotalRevenueNotRecognized.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TotalRevenueNotRecognized.TabIndex = 5;
			this.TotalRevenueNotRecognized.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RevenueLabel
			// 
			this.RevenueLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossTotalsControl|7e0cc539-ce0c-4328-9520-d1bd171c3813", "Revenue");
			this.RevenueLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.RevenueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 26, true);
			this.RevenueLabel.Name = "RevenueLabel";
			this.RevenueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.RevenueLabel.TabIndex = 3;
			this.RevenueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// TotalRevenueRecognized
			// 
			this.TotalRevenueRecognized.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TotalRevenueRecognized, "TotalRevenueRecognized");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Integration.IJobProfitLoss)(null)).TotalRevenueRecognized)));
			this.TotalRevenueRecognized.BindToDecimalPlaces = "Decimals";
			this.TotalRevenueRecognized.CaptionResourceString = null;
			this.TotalRevenueRecognized.DecimalPlaces = 2;
			this.TotalRevenueRecognized.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 48, true);
			this.TotalRevenueRecognized.Name = "TotalRevenueRecognized";
			this.TotalRevenueRecognized.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TotalRevenueRecognized.TabIndex = 4;
			this.TotalRevenueRecognized.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalWIP
			// 
			this.TotalWIP.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TotalWIP, "TotalWIP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Integration.IJobProfitLoss)(null)).TotalWIP)));
			this.TotalWIP.BindToDecimalPlaces = "Decimals";
			this.TotalWIP.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.TotalWIP.CaptionResourceString = null;
			this.TotalWIP.DecimalPlaces = 2;
			this.TotalWIP.Font = new System.Drawing.Font("Tahoma", 7.5F, System.Drawing.FontStyle.Bold);
			this.TotalWIP.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(214, 88, true);
			this.TotalWIP.Name = "TotalWIP";
			this.TotalWIP.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TotalWIP.TabIndex = 10;
			this.TotalWIP.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalWIPNotRecognized
			// 
			this.TotalWIPNotRecognized.AcceptsReturn = true;
			this.TotalWIPNotRecognized.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TotalWIPNotRecognized, "TotalWIPNotRecognized");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Integration.IJobProfitLoss)(null)).TotalWIPNotRecognized)));
			this.TotalWIPNotRecognized.BindToDecimalPlaces = "Decimals";
			this.TotalWIPNotRecognized.CaptionResourceString = null;
			this.TotalWIPNotRecognized.DecimalPlaces = 2;
			this.TotalWIPNotRecognized.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(214, 68, true);
			this.TotalWIPNotRecognized.Name = "TotalWIPNotRecognized";
			this.TotalWIPNotRecognized.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TotalWIPNotRecognized.TabIndex = 9;
			this.TotalWIPNotRecognized.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalWIPRecognized
			// 
			this.TotalWIPRecognized.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TotalWIPRecognized, "TotalWIPRecognized");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Integration.IJobProfitLoss)(null)).TotalWIPRecognized)));
			this.TotalWIPRecognized.BindToDecimalPlaces = "Decimals";
			this.TotalWIPRecognized.CaptionResourceString = null;
			this.TotalWIPRecognized.DecimalPlaces = 2;
			this.TotalWIPRecognized.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(214, 48, true);
			this.TotalWIPRecognized.Name = "TotalWIPRecognized";
			this.TotalWIPRecognized.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TotalWIPRecognized.TabIndex = 8;
			this.TotalWIPRecognized.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalCost
			// 
			this.TotalCost.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TotalCost, "TotalCost");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Integration.IJobProfitLoss)(null)).TotalCost)));
			this.TotalCost.BindToDecimalPlaces = "Decimals";
			this.TotalCost.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.TotalCost.CaptionResourceString = null;
			this.TotalCost.DecimalPlaces = 2;
			this.TotalCost.Font = new System.Drawing.Font("Tahoma", 7.5F, System.Drawing.FontStyle.Bold);
			this.TotalCost.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 88, true);
			this.TotalCost.Name = "TotalCost";
			this.TotalCost.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TotalCost.TabIndex = 14;
			this.TotalCost.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalCostNotRecognized
			// 
			this.TotalCostNotRecognized.AcceptsReturn = true;
			this.TotalCostNotRecognized.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TotalCostNotRecognized, "TotalCostNotRecognized");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Integration.IJobProfitLoss)(null)).TotalCostNotRecognized)));
			this.TotalCostNotRecognized.BindToDecimalPlaces = "Decimals";
			this.TotalCostNotRecognized.CaptionResourceString = null;
			this.TotalCostNotRecognized.DecimalPlaces = 2;
			this.TotalCostNotRecognized.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 68, true);
			this.TotalCostNotRecognized.Name = "TotalCostNotRecognized";
			this.TotalCostNotRecognized.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TotalCostNotRecognized.TabIndex = 13;
			this.TotalCostNotRecognized.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalCostRecognized
			// 
			this.TotalCostRecognized.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TotalCostRecognized, "TotalCostRecognized");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Integration.IJobProfitLoss)(null)).TotalCostRecognized)));
			this.TotalCostRecognized.BindToDecimalPlaces = "Decimals";
			this.TotalCostRecognized.CaptionResourceString = null;
			this.TotalCostRecognized.DecimalPlaces = 2;
			this.TotalCostRecognized.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 48, true);
			this.TotalCostRecognized.Name = "TotalCostRecognized";
			this.TotalCostRecognized.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TotalCostRecognized.TabIndex = 12;
			this.TotalCostRecognized.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalAccrual
			// 
			this.TotalAccrual.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TotalAccrual, "TotalAccrual");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Integration.IJobProfitLoss)(null)).TotalAccrual)));
			this.TotalAccrual.BindToDecimalPlaces = "Decimals";
			this.TotalAccrual.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.TotalAccrual.CaptionResourceString = null;
			this.TotalAccrual.DecimalPlaces = 2;
			this.TotalAccrual.Font = new System.Drawing.Font("Tahoma", 7.5F, System.Drawing.FontStyle.Bold);
			this.TotalAccrual.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(426, 88, true);
			this.TotalAccrual.Name = "TotalAccrual";
			this.TotalAccrual.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TotalAccrual.TabIndex = 18;
			this.TotalAccrual.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalAccrualNotRecognized
			// 
			this.TotalAccrualNotRecognized.AcceptsReturn = true;
			this.TotalAccrualNotRecognized.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TotalAccrualNotRecognized, "TotalAccrualNotRecognized");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Integration.IJobProfitLoss)(null)).TotalAccrualNotRecognized)));
			this.TotalAccrualNotRecognized.BindToDecimalPlaces = "Decimals";
			this.TotalAccrualNotRecognized.CaptionResourceString = null;
			this.TotalAccrualNotRecognized.DecimalPlaces = 2;
			this.TotalAccrualNotRecognized.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(426, 68, true);
			this.TotalAccrualNotRecognized.Name = "TotalAccrualNotRecognized";
			this.TotalAccrualNotRecognized.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TotalAccrualNotRecognized.TabIndex = 17;
			this.TotalAccrualNotRecognized.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalAccrualRecognized
			// 
			this.TotalAccrualRecognized.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TotalAccrualRecognized, "TotalAccrualRecognized");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Integration.IJobProfitLoss)(null)).TotalAccrualRecognized)));
			this.TotalAccrualRecognized.BindToDecimalPlaces = "Decimals";
			this.TotalAccrualRecognized.CaptionResourceString = null;
			this.TotalAccrualRecognized.DecimalPlaces = 2;
			this.TotalAccrualRecognized.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(426, 48, true);
			this.TotalAccrualRecognized.Name = "TotalAccrualRecognized";
			this.TotalAccrualRecognized.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TotalAccrualRecognized.TabIndex = 16;
			this.TotalAccrualRecognized.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalLineAmount
			// 
			this.TotalLineAmount.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TotalLineAmount, "TotalLineAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Integration.IJobProfitLoss)(null)).TotalLineAmount)));
			this.TotalLineAmount.BindToDecimalPlaces = "Decimals";
			this.TotalLineAmount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.TotalLineAmount.CaptionResourceString = null;
			this.TotalLineAmount.DecimalPlaces = 2;
			this.TotalLineAmount.Font = new System.Drawing.Font("Tahoma", 7.5F, System.Drawing.FontStyle.Bold);
			this.TotalLineAmount.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(532, 88, true);
			this.TotalLineAmount.Name = "TotalLineAmount";
			this.TotalLineAmount.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TotalLineAmount.TabIndex = 22;
			this.TotalLineAmount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalLineAmountNotRecognized
			// 
			this.TotalLineAmountNotRecognized.AcceptsReturn = true;
			this.TotalLineAmountNotRecognized.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TotalLineAmountNotRecognized, "TotalLineAmountNotRecognized");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Integration.IJobProfitLoss)(null)).TotalLineAmountNotRecognized)));
			this.TotalLineAmountNotRecognized.BindToDecimalPlaces = "Decimals";
			this.TotalLineAmountNotRecognized.CaptionResourceString = null;
			this.TotalLineAmountNotRecognized.DecimalPlaces = 2;
			this.TotalLineAmountNotRecognized.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(532, 68, true);
			this.TotalLineAmountNotRecognized.Name = "TotalLineAmountNotRecognized";
			this.TotalLineAmountNotRecognized.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TotalLineAmountNotRecognized.TabIndex = 21;
			this.TotalLineAmountNotRecognized.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalLineAmountRecognized
			// 
			this.TotalLineAmountRecognized.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TotalLineAmountRecognized, "TotalLineAmountRecognized");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Integration.IJobProfitLoss)(null)).TotalLineAmountRecognized)));
			this.TotalLineAmountRecognized.BindToDecimalPlaces = "Decimals";
			this.TotalLineAmountRecognized.CaptionResourceString = null;
			this.TotalLineAmountRecognized.DecimalPlaces = 2;
			this.TotalLineAmountRecognized.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(532, 48, true);
			this.TotalLineAmountRecognized.Name = "TotalLineAmountRecognized";
			this.TotalLineAmountRecognized.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TotalLineAmountRecognized.TabIndex = 20;
			this.TotalLineAmountRecognized.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JobProfitLossTotalsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TotalsGroupBox);
			this.Name = "JobProfitLossTotalsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(873, 142, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TotalsGroupBox.ResumeLayout(false);
			this.TotalsGroupBox.PerformLayout();
			this.TaxExpensePanel.ResumeLayout(false);
			this.TaxExpensePanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZPanel TaxExpensePanel;
		private ZArchitecture.ZLabel TaxExpenseLabel;
		private ZArchitecture.ZCalcEdit TotalTaxExpenseRevenue;
		private ZArchitecture.ZCalcEdit TotalTaxExpenseCost;
	}
}
