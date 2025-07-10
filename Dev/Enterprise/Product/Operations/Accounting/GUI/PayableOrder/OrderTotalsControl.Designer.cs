using Enterprise.ZArchitecture.GUI;
namespace Enterprise.Accounting.GUI.PayableOrder
{
	partial class OrderTotalsControl : ZUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

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
			this.APH_Calc_TotalQuantityRemainingTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.APH_Calc_TotalQuantityReceivedTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.APH_Calc_TotalQuantityInvoicedTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.APH_Calc_TotalQuantityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.APH_Calc_CountOuterPacksTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.APH_Calc_InnerPacksTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.APH_Calc_LineCountTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.APH_Calc_TotalLinePriceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.APH_Calc_TotalInvoicedPriceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.APH_RX_TextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.APH_RX_TextBox2 = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader);
			// 
			// APH_Calc_TotalQuantityRemainingTextBox
			// 
			this.BindingSource.SetBindingMember(this.APH_Calc_TotalQuantityRemainingTextBox, "APH_Calc_TotalQuantityRemaining");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).APH_Calc_TotalQuantityRemaining)));
			this.APH_Calc_TotalQuantityRemainingTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(609, 3, true);
			this.APH_Calc_TotalQuantityRemainingTextBox.Name = "APH_Calc_TotalQuantityRemainingTextBox";
			this.APH_Calc_TotalQuantityRemainingTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(51, 17, true);
			this.APH_Calc_TotalQuantityRemainingTextBox.TabIndex = 7;
			// 
			// APH_Calc_TotalQuantityReceivedTextBox
			// 
			this.BindingSource.SetBindingMember(this.APH_Calc_TotalQuantityReceivedTextBox, "APH_Calc_TotalQuantityReceived");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).APH_Calc_TotalQuantityReceived)));
			this.APH_Calc_TotalQuantityReceivedTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(381, 3, true);
			this.APH_Calc_TotalQuantityReceivedTextBox.Name = "APH_Calc_TotalQuantityReceivedTextBox";
			this.APH_Calc_TotalQuantityReceivedTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(51, 17, true);
			this.APH_Calc_TotalQuantityReceivedTextBox.TabIndex = 6;
			// 
			// APH_Calc_TotalQuantityInvoicedTextBox
			// 
			this.BindingSource.SetBindingMember(this.APH_Calc_TotalQuantityInvoicedTextBox, "APH_Calc_TotalQuantityInvoiced");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).APH_Calc_TotalQuantityInvoiced)));
			this.APH_Calc_TotalQuantityInvoicedTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(493, 3, true);
			this.APH_Calc_TotalQuantityInvoicedTextBox.Name = "APH_Calc_TotalQuantityInvoicedTextBox";
			this.APH_Calc_TotalQuantityInvoicedTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(51, 17, true);
			this.APH_Calc_TotalQuantityInvoicedTextBox.TabIndex = 5;
			// 
			// APH_Calc_TotalQuantityTextBox
			// 
			this.BindingSource.SetBindingMember(this.APH_Calc_TotalQuantityTextBox, "APH_Calc_TotalQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).APH_Calc_TotalQuantity)));
			this.APH_Calc_TotalQuantityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(269, 3, true);
			this.APH_Calc_TotalQuantityTextBox.Name = "APH_Calc_TotalQuantityTextBox";
			this.APH_Calc_TotalQuantityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(51, 17, true);
			this.APH_Calc_TotalQuantityTextBox.TabIndex = 4;
			// 
			// APH_Calc_CountOuterPacksTextBox
			// 
			this.BindingSource.SetBindingMember(this.APH_Calc_CountOuterPacksTextBox, "APH_Calc_OuterPacks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).APH_Calc_OuterPacks)));
			this.APH_Calc_CountOuterPacksTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(198, 3, true);
			this.APH_Calc_CountOuterPacksTextBox.Name = "APH_Calc_CountOuterPacksTextBox";
			this.APH_Calc_CountOuterPacksTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 17, true);
			this.APH_Calc_CountOuterPacksTextBox.TabIndex = 3;
			// 
			// APH_Calc_InnerPacksTextBox
			// 
			this.BindingSource.SetBindingMember(this.APH_Calc_InnerPacksTextBox, "APH_Calc_InnerPacks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).APH_Calc_InnerPacks)));
			this.APH_Calc_InnerPacksTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 3, true);
			this.APH_Calc_InnerPacksTextBox.Name = "APH_Calc_InnerPacksTextBox";
			this.APH_Calc_InnerPacksTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 17, true);
			this.APH_Calc_InnerPacksTextBox.TabIndex = 2;
			// 
			// APH_Calc_LineCountTextBox
			// 
			this.BindingSource.SetBindingMember(this.APH_Calc_LineCountTextBox, "APH_Calc_LineCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).APH_Calc_LineCount)));
			this.APH_Calc_LineCountTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(43, 3, true);
			this.APH_Calc_LineCountTextBox.Name = "APH_Calc_LineCountTextBox";
			this.APH_Calc_LineCountTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 17, true);
			this.APH_Calc_LineCountTextBox.TabIndex = 1;
			// 
			// APH_Calc_TotalLinePriceTextBox
			// 
			this.BindingSource.SetBindingMember(this.APH_Calc_TotalLinePriceTextBox, "APH_Calc_TotalLinePrice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).APH_Calc_TotalLinePrice)));
			this.APH_Calc_TotalLinePriceTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1948405e-b52f-4367-8061-8ef48c4664d1", "Order Total");
			this.APH_Calc_TotalLinePriceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(895, 3, true);
			this.APH_Calc_TotalLinePriceTextBox.Name = "APH_Calc_TotalLinePriceTextBox";
			this.APH_Calc_TotalLinePriceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 17, true);
			this.APH_Calc_TotalLinePriceTextBox.TabIndex = 8;
			this.APH_Calc_TotalLinePriceTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// APH_Calc_TotalInvoicedPriceTextBox
			// 
			this.BindingSource.SetBindingMember(this.APH_Calc_TotalInvoicedPriceTextBox, "APH_Calc_TotalInvoicedPrice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).APH_Calc_TotalInvoicedPrice)));
			this.APH_Calc_TotalInvoicedPriceTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1fc77887-bd33-47b0-a843-4195f5b9881a", "Invoice Total");
			this.APH_Calc_TotalInvoicedPriceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(739, 3, true);
			this.APH_Calc_TotalInvoicedPriceTextBox.Name = "APH_Calc_TotalInvoicedPriceTextBox";
			this.APH_Calc_TotalInvoicedPriceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 17, true);
			this.APH_Calc_TotalInvoicedPriceTextBox.TabIndex = 9;
			this.APH_Calc_TotalInvoicedPriceTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// APH_RX_TextBox1
			// 
			this.BindingSource.SetBindingMember(this.APH_RX_TextBox1, "APH_OrderCurrencyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).APH_OrderCurrencyCode)));
			this.APH_RX_TextBox1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("43ef1a94-8203-419d-8739-15fbacb71d67", "Currency");
			this.APH_RX_TextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(794, 3, true);
			this.APH_RX_TextBox1.Name = "APH_RX_TextBox1";
			this.APH_RX_TextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(31, 17, true);
			this.APH_RX_TextBox1.TabIndex = 10;
			// 
			// APH_RX_TextBox2
			// 
			this.BindingSource.SetBindingMember(this.APH_RX_TextBox2, "APH_OrderCurrencyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).APH_OrderCurrencyCode)));
			this.APH_RX_TextBox2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("b5301374-986c-43b3-8142-9caf9dc378de", "Currency");
			this.APH_RX_TextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(950, 3, true);
			this.APH_RX_TextBox2.Name = "APH_RX_TextBox2";
			this.APH_RX_TextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(31, 17, true);
			this.APH_RX_TextBox2.TabIndex = 11;
			// 
			// OrderTotalsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("69a1aec1-df17-48dd-a141-5983ba8d7f68", "Totals");
			this.Controls.Add(this.APH_RX_TextBox2);
			this.Controls.Add(this.APH_RX_TextBox1);
			this.Controls.Add(this.APH_Calc_TotalInvoicedPriceTextBox);
			this.Controls.Add(this.APH_Calc_TotalLinePriceTextBox);
			this.Controls.Add(this.APH_Calc_TotalQuantityRemainingTextBox);
			this.Controls.Add(this.APH_Calc_TotalQuantityReceivedTextBox);
			this.Controls.Add(this.APH_Calc_TotalQuantityInvoicedTextBox);
			this.Controls.Add(this.APH_Calc_LineCountTextBox);
			this.Controls.Add(this.APH_Calc_TotalQuantityTextBox);
			this.Controls.Add(this.APH_Calc_InnerPacksTextBox);
			this.Controls.Add(this.APH_Calc_CountOuterPacksTextBox);
			this.Name = "OrderTotalsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(988, 25, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox APH_Calc_TotalQuantityRemainingTextBox;
		private ZArchitecture.ZTextBox APH_Calc_TotalQuantityReceivedTextBox;
		private ZArchitecture.ZTextBox APH_Calc_TotalQuantityInvoicedTextBox;
		private ZArchitecture.ZTextBox APH_Calc_TotalQuantityTextBox;
		private ZArchitecture.ZTextBox APH_Calc_CountOuterPacksTextBox;
		private ZArchitecture.ZTextBox APH_Calc_InnerPacksTextBox;
		private ZArchitecture.ZTextBox APH_Calc_LineCountTextBox;
		private ZArchitecture.ZTextBox APH_Calc_TotalLinePriceTextBox;
		private ZArchitecture.ZTextBox APH_Calc_TotalInvoicedPriceTextBox;
		private ZArchitecture.ZTextBox APH_RX_TextBox1;
		private ZArchitecture.ZTextBox APH_RX_TextBox2;
	}
}
