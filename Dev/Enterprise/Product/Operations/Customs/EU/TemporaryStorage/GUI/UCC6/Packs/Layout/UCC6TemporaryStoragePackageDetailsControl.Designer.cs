using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public partial class UCC6TemporaryStoragePackageDetailsControl
	{
		internal ZGuidDropEditWithFixedWidth containerPKGuidDropEditWithFixedWidth;
		internal ZDropEditWithFixedWidth packUQDropEditWithFixedWidth;
		internal ZArchitecture.ZCalcEdit packQtyCalcEdit;
		internal ZArchitecture.ZTextBox marksAndNumbersTextBox;

		void InitializeComponent()
		{
			this.containerPKGuidDropEditWithFixedWidth = new Enterprise.ZArchitecture.GUI.ZGuidDropEditWithFixedWidth();
			this.packUQDropEditWithFixedWidth = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.packQtyCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.marksAndNumbersTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.containerPKGuidDropEditWithFixedWidth.SuspendLayout();
			this.packUQDropEditWithFixedWidth.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader);
			// 
			// zGuidDropEditWithFixedWidthContainerPK
			// 
			this.containerPKGuidDropEditWithFixedWidth.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.containerPKGuidDropEditWithFixedWidth, "Bills.Packs.ContainerPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStoragePack)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).ContainerPK)));
			this.containerPKGuidDropEditWithFixedWidth.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 16, true);
			this.containerPKGuidDropEditWithFixedWidth.Name = "containerPKGuidDropEditWithFixedWidth";
			this.containerPKGuidDropEditWithFixedWidth.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 20, true);
			this.containerPKGuidDropEditWithFixedWidth.TabIndex = 0;
			// 
			// zCalcEditPackQty
			// 
			this.BindingSource.SetBindingMember(this.packQtyCalcEdit, "Bills.Packs.APA_PackQty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStoragePack)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).APA_PackQty)));
			this.packQtyCalcEdit.DecimalPlaces = 2;
			this.packQtyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 42, true);
			this.packQtyCalcEdit.Name = "packQtyCalcEdit";
			this.packQtyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.packQtyCalcEdit.TabIndex = 1;
			this.packQtyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zDropEditWithFixedWidthPackUQ
			// 
			this.packUQDropEditWithFixedWidth.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.packUQDropEditWithFixedWidth, "Bills.Packs.APA_PackUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStoragePack)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).APA_PackUQ)));
			this.packUQDropEditWithFixedWidth.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 68, true);
			this.packUQDropEditWithFixedWidth.Name = "packUQDropEditWithFixedWidth";
			this.packUQDropEditWithFixedWidth.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.packUQDropEditWithFixedWidth.TabIndex = 2;
			// 
			// zTextBoxMarksAndNumbers
			// 
			this.BindingSource.SetBindingMember(this.marksAndNumbersTextBox, "Bills.Packs.APA_MarksAndNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStoragePack)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).APA_MarksAndNumbers)));
			this.marksAndNumbersTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 94, true);
			this.marksAndNumbersTextBox.Name = "marksAndNumbersTextBox";
			this.marksAndNumbersTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 20, true);
			this.marksAndNumbersTextBox.TabIndex = 3;
			//
			// UCC6TemporaryStoragePackageDetailsControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.containerPKGuidDropEditWithFixedWidth);
			this.Controls.Add(this.packQtyCalcEdit);
			this.Controls.Add(this.packUQDropEditWithFixedWidth);
			this.Controls.Add(this.marksAndNumbersTextBox);
			this.Name = "UCC6TemporaryStoragePackageDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1118, 421, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.containerPKGuidDropEditWithFixedWidth.ResumeLayout(true);
			this.containerPKGuidDropEditWithFixedWidth.PerformLayout();
			this.packUQDropEditWithFixedWidth.ResumeLayout(true);
			this.packUQDropEditWithFixedWidth.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
