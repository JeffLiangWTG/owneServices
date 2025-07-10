using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	partial class RFPPackagesUserControl
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

		private void InitializeComponent()
		{
			this.PackagesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.InnerPackGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.QL_InnerPackWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.QL_InnerPackAccuracyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.QL_InnerPackTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.QL_InnerPackCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.IntermediatePackGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.QL_IntermediatePackWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.QL_IntermediatePackAccuracyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.QL_IntermediatePackTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.QL_IntermediatePackCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OuterPackGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.QL_OuterPackWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.QL_OuterPackAccuracyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.QL_OuterPackTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.QL_OuterPackCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PackMeasurementLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AccuracyLabel = new Enterprise.ZArchitecture.ZLabel();
			this.EXDOCPackTypeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PackCountLabel = new Enterprise.ZArchitecture.ZLabel();
			this.WeightGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.QL_GrossMetricWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.QL_NetQuantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.QL_ImperialNetWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.GroupBoxShippingMarks = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TextBoxQL_BatchCode = new Enterprise.ZArchitecture.ZTextBox();
			this.TextBoxQL_ShippingMarks = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PackagesGroupBox.SuspendLayout();
			this.InnerPackGroupBox.SuspendLayout();
			this.QL_InnerPackWeightCalcDropEdit.SuspendLayout();
			this.QL_InnerPackAccuracyDropEdit.SuspendLayout();
			this.QL_InnerPackTypeDropEdit.SuspendLayout();
			this.IntermediatePackGroupBox.SuspendLayout();
			this.QL_IntermediatePackWeightCalcDropEdit.SuspendLayout();
			this.QL_IntermediatePackAccuracyDropEdit.SuspendLayout();
			this.QL_IntermediatePackTypeDropEdit.SuspendLayout();
			this.OuterPackGroupBox.SuspendLayout();
			this.QL_OuterPackWeightCalcDropEdit.SuspendLayout();
			this.QL_OuterPackAccuracyDropEdit.SuspendLayout();
			this.QL_OuterPackTypeDropEdit.SuspendLayout();
			this.WeightGroupBox.SuspendLayout();
			this.QL_GrossMetricWeightCalcDropEdit.SuspendLayout();
			this.QL_NetQuantityCalcDropEdit.SuspendLayout();
			this.QL_ImperialNetWeightCalcDropEdit.SuspendLayout();
			this.GroupBoxShippingMarks.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine);
			// 
			// PackagesGroupBox
			// 
			this.PackagesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.PackagesGroupBox.Controls.Add(this.InnerPackGroupBox);
			this.PackagesGroupBox.Controls.Add(this.IntermediatePackGroupBox);
			this.PackagesGroupBox.Controls.Add(this.OuterPackGroupBox);
			this.PackagesGroupBox.Controls.Add(this.PackMeasurementLabel);
			this.PackagesGroupBox.Controls.Add(this.AccuracyLabel);
			this.PackagesGroupBox.Controls.Add(this.EXDOCPackTypeLabel);
			this.PackagesGroupBox.Controls.Add(this.PackCountLabel);
			this.PackagesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 98, true);
			this.PackagesGroupBox.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(739, 120, true);
			this.PackagesGroupBox.Name = "PackagesGroupBox";
			this.PackagesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(689, 120, true);
			this.PackagesGroupBox.TabIndex = 2;
			this.PackagesGroupBox.TabStop = false;
			this.PackagesGroupBox.Text = "Packages";
			// 
			// InnerPackGroupBox
			// 
			this.InnerPackGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.InnerPackGroupBox.Controls.Add(this.QL_InnerPackWeightCalcDropEdit);
			this.InnerPackGroupBox.Controls.Add(this.QL_InnerPackAccuracyDropEdit);
			this.InnerPackGroupBox.Controls.Add(this.QL_InnerPackTypeDropEdit);
			this.InnerPackGroupBox.Controls.Add(this.QL_InnerPackCountCalcEdit);
			this.InnerPackGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(538, 10, true);
			this.InnerPackGroupBox.Name = "InnerPackGroupBox";
			this.InnerPackGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 104, true);
			this.InnerPackGroupBox.TabIndex = 6;
			this.InnerPackGroupBox.TabStop = false;
			this.InnerPackGroupBox.Text = "Inner Pack";
			// 
			// QL_InnerPackWeightCalcDropEdit
			// 
			this.QL_InnerPackWeightCalcDropEdit.AllowDrop = true;
			this.QL_InnerPackWeightCalcDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.QL_InnerPackWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_InnerPackWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_InnerPackWeightUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.Lookups.Weight)));
			this.QL_InnerPackWeightCalcDropEdit.BindToAmount = "QuarantineExDocLine+QL_InnerPackWeight";
			this.QL_InnerPackWeightCalcDropEdit.BindToList = "QuarantineExDocLine+Lookups+Weight";
			this.QL_InnerPackWeightCalcDropEdit.BindToUnit = "QuarantineExDocLine+QL_InnerPackWeightUnit";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.QL_InnerPackWeightCalcDropEdit, false);
			this.QL_InnerPackWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 79, true);
			this.QL_InnerPackWeightCalcDropEdit.Name = "QL_InnerPackWeightCalcDropEdit";
			this.QL_InnerPackWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 17, true);
			this.QL_InnerPackWeightCalcDropEdit.TabIndex = 3;
			// 
			// QL_InnerPackAccuracyDropEdit
			// 
			this.QL_InnerPackAccuracyDropEdit.AllowDrop = true;
			this.QL_InnerPackAccuracyDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.QL_InnerPackAccuracyDropEdit, "QuarantineExDocLine+QL_InnerPackAccuracy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_InnerPackAccuracy)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.Lookups.PackAccuracy)));
			this.QL_InnerPackAccuracyDropEdit.BindToList = "QuarantineExDocLine+Lookups+PackAccuracy";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.QL_InnerPackAccuracyDropEdit, false);
			this.QL_InnerPackAccuracyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 58, true);
			this.QL_InnerPackAccuracyDropEdit.Name = "QL_InnerPackAccuracyDropEdit";
			this.QL_InnerPackAccuracyDropEdit.ShouldResizeByMaxLength = true;
			this.QL_InnerPackAccuracyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 17, true);
			this.QL_InnerPackAccuracyDropEdit.TabIndex = 2;
			// 
			// QL_InnerPackTypeDropEdit
			// 
			this.QL_InnerPackTypeDropEdit.AllowDrop = true;
			this.QL_InnerPackTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.QL_InnerPackTypeDropEdit, "QuarantineExDocLine+QL_InnerPackType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_InnerPackType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.Lookups.PackageTypes)));
			this.QL_InnerPackTypeDropEdit.BindToList = "QuarantineExDocLine+Lookups+PackageTypes";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.QL_InnerPackTypeDropEdit, false);
			this.QL_InnerPackTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 37, true);
			this.QL_InnerPackTypeDropEdit.Name = "QL_InnerPackTypeDropEdit";
			this.QL_InnerPackTypeDropEdit.ShouldResizeByMaxLength = true;
			this.QL_InnerPackTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 17, true);
			this.QL_InnerPackTypeDropEdit.TabIndex = 1;
			// 
			// QL_InnerPackCountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.QL_InnerPackCountCalcEdit, "QuarantineExDocLine+QL_InnerPackCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_InnerPackCount)));
			this.QL_InnerPackCountCalcEdit.CaptionResourceString = null;
			this.QL_InnerPackCountCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.QL_InnerPackCountCalcEdit, false);
			this.QL_InnerPackCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 16, true);
			this.QL_InnerPackCountCalcEdit.Name = "QL_InnerPackCountCalcEdit";
			this.QL_InnerPackCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.QL_InnerPackCountCalcEdit.TabIndex = 0;
			this.QL_InnerPackCountCalcEdit.Text = "0";
			this.QL_InnerPackCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// IntermediatePackGroupBox
			// 
			this.IntermediatePackGroupBox.Controls.Add(this.QL_IntermediatePackWeightCalcDropEdit);
			this.IntermediatePackGroupBox.Controls.Add(this.QL_IntermediatePackAccuracyDropEdit);
			this.IntermediatePackGroupBox.Controls.Add(this.QL_IntermediatePackTypeDropEdit);
			this.IntermediatePackGroupBox.Controls.Add(this.QL_IntermediatePackCountCalcEdit);
			this.IntermediatePackGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 10, true);
			this.IntermediatePackGroupBox.Name = "IntermediatePackGroupBox";
			this.IntermediatePackGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(207, 104, true);
			this.IntermediatePackGroupBox.TabIndex = 5;
			this.IntermediatePackGroupBox.TabStop = false;
			this.IntermediatePackGroupBox.Text = "Intermediate Pack";
			// 
			// QL_IntermediatePackWeightCalcDropEdit
			// 
			this.QL_IntermediatePackWeightCalcDropEdit.AllowDrop = true;
			this.QL_IntermediatePackWeightCalcDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.QL_IntermediatePackWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_IntermediatePackWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_IntermediatePackWeightUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.Lookups.Weight)));
			this.QL_IntermediatePackWeightCalcDropEdit.BindToAmount = "QuarantineExDocLine+QL_IntermediatePackWeight";
			this.QL_IntermediatePackWeightCalcDropEdit.BindToList = "QuarantineExDocLine+Lookups+Weight";
			this.QL_IntermediatePackWeightCalcDropEdit.BindToUnit = "QuarantineExDocLine+QL_IntermediatePackWeightUnit";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.QL_IntermediatePackWeightCalcDropEdit, false);
			this.QL_IntermediatePackWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 79, true);
			this.QL_IntermediatePackWeightCalcDropEdit.Name = "QL_IntermediatePackWeightCalcDropEdit";
			this.QL_IntermediatePackWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 17, true);
			this.QL_IntermediatePackWeightCalcDropEdit.TabIndex = 3;
			// 
			// QL_IntermediatePackAccuracyDropEdit
			// 
			this.QL_IntermediatePackAccuracyDropEdit.AllowDrop = true;
			this.QL_IntermediatePackAccuracyDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.QL_IntermediatePackAccuracyDropEdit, "QuarantineExDocLine+QL_IntermediatePackAccuracy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_IntermediatePackAccuracy)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.Lookups.PackAccuracy)));
			this.QL_IntermediatePackAccuracyDropEdit.BindToList = "QuarantineExDocLine+Lookups+PackAccuracy";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.QL_IntermediatePackAccuracyDropEdit, false);
			this.QL_IntermediatePackAccuracyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 58, true);
			this.QL_IntermediatePackAccuracyDropEdit.Name = "QL_IntermediatePackAccuracyDropEdit";
			this.QL_IntermediatePackAccuracyDropEdit.ShouldResizeByMaxLength = true;
			this.QL_IntermediatePackAccuracyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 17, true);
			this.QL_IntermediatePackAccuracyDropEdit.TabIndex = 2;
			// 
			// QL_IntermediatePackTypeDropEdit
			// 
			this.QL_IntermediatePackTypeDropEdit.AllowDrop = true;
			this.QL_IntermediatePackTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.QL_IntermediatePackTypeDropEdit, "QuarantineExDocLine+QL_IntermediatePackType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_IntermediatePackType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.Lookups.PackageTypes)));
			this.QL_IntermediatePackTypeDropEdit.BindToList = "QuarantineExDocLine+Lookups+PackageTypes";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.QL_IntermediatePackTypeDropEdit, false);
			this.QL_IntermediatePackTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 37, true);
			this.QL_IntermediatePackTypeDropEdit.Name = "QL_IntermediatePackTypeDropEdit";
			this.QL_IntermediatePackTypeDropEdit.ShouldResizeByMaxLength = true;
			this.QL_IntermediatePackTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 17, true);
			this.QL_IntermediatePackTypeDropEdit.TabIndex = 1;
			// 
			// QL_IntermediatePackCountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.QL_IntermediatePackCountCalcEdit, "QuarantineExDocLine+QL_IntermediatePackCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_IntermediatePackCount)));
			this.QL_IntermediatePackCountCalcEdit.CaptionResourceString = null;
			this.QL_IntermediatePackCountCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.QL_IntermediatePackCountCalcEdit, false);
			this.QL_IntermediatePackCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 16, true);
			this.QL_IntermediatePackCountCalcEdit.Name = "QL_IntermediatePackCountCalcEdit";
			this.QL_IntermediatePackCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.QL_IntermediatePackCountCalcEdit.TabIndex = 0;
			this.QL_IntermediatePackCountCalcEdit.Text = "0";
			this.QL_IntermediatePackCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OuterPackGroupBox
			// 
			this.OuterPackGroupBox.Controls.Add(this.QL_OuterPackWeightCalcDropEdit);
			this.OuterPackGroupBox.Controls.Add(this.QL_OuterPackAccuracyDropEdit);
			this.OuterPackGroupBox.Controls.Add(this.QL_OuterPackTypeDropEdit);
			this.OuterPackGroupBox.Controls.Add(this.QL_OuterPackCountCalcEdit);
			this.OuterPackGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 10, true);
			this.OuterPackGroupBox.Name = "OuterPackGroupBox";
			this.OuterPackGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(202, 104, true);
			this.OuterPackGroupBox.TabIndex = 4;
			this.OuterPackGroupBox.TabStop = false;
			this.OuterPackGroupBox.Text = "Outer Pack";
			// 
			// QL_OuterPackWeightCalcDropEdit
			// 
			this.QL_OuterPackWeightCalcDropEdit.AllowDrop = true;
			this.QL_OuterPackWeightCalcDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.QL_OuterPackWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_OuterPackWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_OuterPackWeightUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.Lookups.Weight)));
			this.QL_OuterPackWeightCalcDropEdit.BindToAmount = "QuarantineExDocLine+QL_OuterPackWeight";
			this.QL_OuterPackWeightCalcDropEdit.BindToList = "QuarantineExDocLine+Lookups+Weight";
			this.QL_OuterPackWeightCalcDropEdit.BindToUnit = "QuarantineExDocLine+QL_OuterPackWeightUnit";
			this.QL_OuterPackWeightCalcDropEdit.Decimals = 3;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.QL_OuterPackWeightCalcDropEdit, false);
			this.QL_OuterPackWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 79, true);
			this.QL_OuterPackWeightCalcDropEdit.Name = "QL_OuterPackWeightCalcDropEdit";
			this.QL_OuterPackWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 17, true);
			this.QL_OuterPackWeightCalcDropEdit.TabIndex = 3;
			// 
			// QL_OuterPackAccuracyDropEdit
			// 
			this.QL_OuterPackAccuracyDropEdit.AllowDrop = true;
			this.QL_OuterPackAccuracyDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.QL_OuterPackAccuracyDropEdit, "QuarantineExDocLine+QL_OuterPackAccuracy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_OuterPackAccuracy)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.Lookups.PackAccuracy)));
			this.QL_OuterPackAccuracyDropEdit.BindToList = "QuarantineExDocLine+Lookups+PackAccuracy";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.QL_OuterPackAccuracyDropEdit, false);
			this.QL_OuterPackAccuracyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 58, true);
			this.QL_OuterPackAccuracyDropEdit.Name = "QL_OuterPackAccuracyDropEdit";
			this.QL_OuterPackAccuracyDropEdit.ShouldResizeByMaxLength = true;
			this.QL_OuterPackAccuracyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 17, true);
			this.QL_OuterPackAccuracyDropEdit.TabIndex = 2;
			// 
			// QL_OuterPackTypeDropEdit
			// 
			this.QL_OuterPackTypeDropEdit.AllowDrop = true;
			this.QL_OuterPackTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.QL_OuterPackTypeDropEdit, "QuarantineExDocLine+QL_OuterPackType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_OuterPackType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.Lookups.PackageTypes)));
			this.QL_OuterPackTypeDropEdit.BindToList = "QuarantineExDocLine+Lookups+PackageTypes";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.QL_OuterPackTypeDropEdit, false);
			this.QL_OuterPackTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 37, true);
			this.QL_OuterPackTypeDropEdit.Name = "QL_OuterPackTypeDropEdit";
			this.QL_OuterPackTypeDropEdit.ShouldResizeByMaxLength = true;
			this.QL_OuterPackTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 17, true);
			this.QL_OuterPackTypeDropEdit.TabIndex = 1;
			// 
			// QL_OuterPackCountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.QL_OuterPackCountCalcEdit, "QuarantineExDocLine.QL_OuterPackCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_OuterPackCount)));
			this.QL_OuterPackCountCalcEdit.CaptionResourceString = null;
			this.QL_OuterPackCountCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.QL_OuterPackCountCalcEdit, false);
			this.QL_OuterPackCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 16, true);
			this.QL_OuterPackCountCalcEdit.Name = "QL_OuterPackCountCalcEdit";
			this.QL_OuterPackCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.QL_OuterPackCountCalcEdit.TabIndex = 0;
			this.QL_OuterPackCountCalcEdit.Text = "0";
			this.QL_OuterPackCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PackMeasurementLabel
			// 
			this.PackMeasurementLabel.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUQuarantineInvoiceLineUserControl|861974fc-6090-4643-828e-c5e1a9e324eb", "Pack Weight");
			this.PackMeasurementLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PackMeasurementLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 89, true);
			this.PackMeasurementLabel.Name = "PackMeasurementLabel";
			this.PackMeasurementLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 20, true);
			this.PackMeasurementLabel.TabIndex = 3;
			// 
			// AccuracyLabel
			// 
			this.AccuracyLabel.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUQuarantineInvoiceLineUserControl|8e32f5b7-5a18-4eb8-b61d-370f2d954b10", "Accuracy");
			this.AccuracyLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.AccuracyLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 68, true);
			this.AccuracyLabel.Name = "AccuracyLabel";
			this.AccuracyLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 20, true);
			this.AccuracyLabel.TabIndex = 2;
			// 
			// EXDOCPackTypeLabel
			// 
			this.EXDOCPackTypeLabel.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUQuarantineInvoiceLineUserControl|d3a4140c-c288-48de-ae26-f75109e30db9", "Pack Type");
			this.EXDOCPackTypeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.EXDOCPackTypeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 47, true);
			this.EXDOCPackTypeLabel.Name = "EXDOCPackTypeLabel";
			this.EXDOCPackTypeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 20, true);
			this.EXDOCPackTypeLabel.TabIndex = 1;
			this.EXDOCPackTypeLabel.Text = "Package Type";
			// 
			// PackCountLabel
			// 
			this.PackCountLabel.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUQuarantineInvoiceLineUserControl|6526c9a0-9f73-48ed-a838-4cd0b1380b8c", "Pack Count");
			this.PackCountLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PackCountLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 26, true);
			this.PackCountLabel.Name = "PackCountLabel";
			this.PackCountLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 20, true);
			this.PackCountLabel.TabIndex = 0;
			// 
			// WeightGroupBox
			// 
			this.WeightGroupBox.Controls.Add(this.QL_GrossMetricWeightCalcDropEdit);
			this.WeightGroupBox.Controls.Add(this.QL_NetQuantityCalcDropEdit);
			this.WeightGroupBox.Controls.Add(this.QL_ImperialNetWeightCalcDropEdit);
			this.WeightGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 3, true);
			this.WeightGroupBox.Name = "WeightGroupBox";
			this.WeightGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 89, true);
			this.WeightGroupBox.TabIndex = 0;
			this.WeightGroupBox.TabStop = false;
			this.WeightGroupBox.Text = "Weights";
			// 
			// QL_GrossMetricWeightCalcDropEdit
			// 
			this.QL_GrossMetricWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QL_GrossMetricWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_GrossMetricWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_GrossMetricWeightUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.Lookups.MetricWeight)));
			this.QL_GrossMetricWeightCalcDropEdit.BindToAmount = "QuarantineExDocLine.QL_GrossMetricWeight";
			this.QL_GrossMetricWeightCalcDropEdit.BindToList = "QuarantineExDocLine+Lookups+MetricWeight";
			this.QL_GrossMetricWeightCalcDropEdit.BindToUnit = "QuarantineExDocLine+QL_GrossMetricWeightUnit";
			this.QL_GrossMetricWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 61, true);
			this.QL_GrossMetricWeightCalcDropEdit.Name = "QL_GrossMetricWeightCalcDropEdit";
			this.QL_GrossMetricWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 17, true);
			this.QL_GrossMetricWeightCalcDropEdit.TabIndex = 5;
			// 
			// QL_NetQuantityCalcDropEdit
			// 
			this.QL_NetQuantityCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QL_NetQuantityCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_NetQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_NetQuantityUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.Lookups.NetQuantityUnits)));
			this.QL_NetQuantityCalcDropEdit.BindToAmount = "QuarantineExDocLine.QL_NetQuantity";
			this.QL_NetQuantityCalcDropEdit.BindToList = "QuarantineExDocLine+Lookups+NetQuantityUnits";
			this.QL_NetQuantityCalcDropEdit.BindToUnit = "QuarantineExDocLine+QL_NetQuantityUnit";
			this.QL_NetQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 19, true);
			this.QL_NetQuantityCalcDropEdit.Name = "QL_NetQuantityCalcDropEdit";
			this.QL_NetQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 17, true);
			this.QL_NetQuantityCalcDropEdit.TabIndex = 1;
			// 
			// QL_ImperialNetWeightCalcDropEdit
			// 
			this.QL_ImperialNetWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QL_ImperialNetWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_ImperialNetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_ImperialNetWeightUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.Lookups.NetImperialWeightUnit)));
			this.QL_ImperialNetWeightCalcDropEdit.BindToAmount = "QuarantineExDocLine+QL_ImperialNetWeight";
			this.QL_ImperialNetWeightCalcDropEdit.BindToList = "QuarantineExDocLine+Lookups+NetImperialWeightUnit";
			this.QL_ImperialNetWeightCalcDropEdit.BindToUnit = "QuarantineExDocLine+QL_ImperialNetWeightUnit";
			this.QL_ImperialNetWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 40, true);
			this.QL_ImperialNetWeightCalcDropEdit.Name = "QL_ImperialNetWeightCalcDropEdit";
			this.QL_ImperialNetWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 17, true);
			this.QL_ImperialNetWeightCalcDropEdit.TabIndex = 3;
			// 
			// GroupBoxShippingMarks
			// 
			this.GroupBoxShippingMarks.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.GroupBoxShippingMarks.Controls.Add(this.TextBoxQL_BatchCode);
			this.GroupBoxShippingMarks.Controls.Add(this.TextBoxQL_ShippingMarks);
			this.GroupBoxShippingMarks.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(308, 3, true);
			this.GroupBoxShippingMarks.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 89, true);
			this.GroupBoxShippingMarks.Name = "GroupBoxShippingMarks";
			this.GroupBoxShippingMarks.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(387, 89, true);
			this.GroupBoxShippingMarks.TabIndex = 1;
			this.GroupBoxShippingMarks.TabStop = false;
			this.GroupBoxShippingMarks.Text = "Shipping Marks and Batch Code";
			// 
			// TextBoxQL_BatchCode
			// 
			this.TextBoxQL_BatchCode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TextBoxQL_BatchCode, "QuarantineExDocLine+QL_BatchCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_BatchCode)));
			this.TextBoxQL_BatchCode.CaptionResourceString = null;
			this.TextBoxQL_BatchCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(54, 40, true);
			this.TextBoxQL_BatchCode.Name = "TextBoxQL_BatchCode";
			this.TextBoxQL_BatchCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(327, 17, true);
			this.TextBoxQL_BatchCode.TabIndex = 2;
			// 
			// TextBoxQL_ShippingMarks
			// 
			this.TextBoxQL_ShippingMarks.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TextBoxQL_ShippingMarks, "QuarantineExDocLine+QL_ShippingMarks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_ShippingMarks)));
			this.TextBoxQL_ShippingMarks.CaptionResourceString = null;
			this.TextBoxQL_ShippingMarks.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(54, 19, true);
			this.TextBoxQL_ShippingMarks.Name = "TextBoxQL_ShippingMarks";
			this.TextBoxQL_ShippingMarks.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(327, 17, true);
			this.TextBoxQL_ShippingMarks.TabIndex = 1;
			// 
			// RFPPackagesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.PackagesGroupBox);
			this.Controls.Add(this.WeightGroupBox);
			this.Controls.Add(this.GroupBoxShippingMarks);
			this.Name = "RFPPackagesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 246, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PackagesGroupBox.ResumeLayout(false);
			this.PackagesGroupBox.PerformLayout();
			this.InnerPackGroupBox.ResumeLayout(false);
			this.InnerPackGroupBox.PerformLayout();
			this.QL_InnerPackWeightCalcDropEdit.ResumeLayout(true);
			this.QL_InnerPackWeightCalcDropEdit.PerformLayout();
			this.QL_InnerPackAccuracyDropEdit.ResumeLayout(true);
			this.QL_InnerPackAccuracyDropEdit.PerformLayout();
			this.QL_InnerPackTypeDropEdit.ResumeLayout(true);
			this.QL_InnerPackTypeDropEdit.PerformLayout();
			this.IntermediatePackGroupBox.ResumeLayout(false);
			this.IntermediatePackGroupBox.PerformLayout();
			this.QL_IntermediatePackWeightCalcDropEdit.ResumeLayout(true);
			this.QL_IntermediatePackWeightCalcDropEdit.PerformLayout();
			this.QL_IntermediatePackAccuracyDropEdit.ResumeLayout(true);
			this.QL_IntermediatePackAccuracyDropEdit.PerformLayout();
			this.QL_IntermediatePackTypeDropEdit.ResumeLayout(true);
			this.QL_IntermediatePackTypeDropEdit.PerformLayout();
			this.OuterPackGroupBox.ResumeLayout(false);
			this.OuterPackGroupBox.PerformLayout();
			this.QL_OuterPackWeightCalcDropEdit.ResumeLayout(true);
			this.QL_OuterPackWeightCalcDropEdit.PerformLayout();
			this.QL_OuterPackAccuracyDropEdit.ResumeLayout(true);
			this.QL_OuterPackAccuracyDropEdit.PerformLayout();
			this.QL_OuterPackTypeDropEdit.ResumeLayout(true);
			this.QL_OuterPackTypeDropEdit.PerformLayout();
			this.WeightGroupBox.ResumeLayout(false);
			this.WeightGroupBox.PerformLayout();
			this.QL_GrossMetricWeightCalcDropEdit.ResumeLayout(true);
			this.QL_GrossMetricWeightCalcDropEdit.PerformLayout();
			this.QL_NetQuantityCalcDropEdit.ResumeLayout(true);
			this.QL_NetQuantityCalcDropEdit.PerformLayout();
			this.QL_ImperialNetWeightCalcDropEdit.ResumeLayout(true);
			this.QL_ImperialNetWeightCalcDropEdit.PerformLayout();
			this.GroupBoxShippingMarks.ResumeLayout(false);
			this.GroupBoxShippingMarks.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZGroupBox GroupBoxShippingMarks;
		private ZTextBox TextBoxQL_ShippingMarks;
		private ZGroupBox PackagesGroupBox;
		private ZGroupBox InnerPackGroupBox;
		private ZCalcDropEdit QL_InnerPackWeightCalcDropEdit;
		private ZDropEdit QL_InnerPackAccuracyDropEdit;
		private ZDropEdit QL_InnerPackTypeDropEdit;
		private ZCalcEdit QL_InnerPackCountCalcEdit;
		private ZGroupBox IntermediatePackGroupBox;
		private ZCalcDropEdit QL_IntermediatePackWeightCalcDropEdit;
		private ZDropEdit QL_IntermediatePackAccuracyDropEdit;
		private ZDropEdit QL_IntermediatePackTypeDropEdit;
		private ZCalcEdit QL_IntermediatePackCountCalcEdit;
		private ZGroupBox OuterPackGroupBox;
		private ZCalcDropEdit QL_OuterPackWeightCalcDropEdit;
		private ZDropEdit QL_OuterPackAccuracyDropEdit;
		private ZDropEdit QL_OuterPackTypeDropEdit;
		private ZCalcEdit QL_OuterPackCountCalcEdit;
		private ZLabel PackMeasurementLabel;
		private ZLabel AccuracyLabel;
		private ZLabel EXDOCPackTypeLabel;
		private ZLabel PackCountLabel;
		private ZGroupBox WeightGroupBox;
		private ZCalcDropEdit QL_GrossMetricWeightCalcDropEdit;
		private ZCalcDropEdit QL_NetQuantityCalcDropEdit;
		private ZCalcDropEdit QL_ImperialNetWeightCalcDropEdit;
		private ZTextBox TextBoxQL_BatchCode;

	}
}
