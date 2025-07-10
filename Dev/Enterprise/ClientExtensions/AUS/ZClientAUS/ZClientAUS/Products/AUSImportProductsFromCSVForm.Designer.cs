using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.AUS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.AUS.Products
{
	public partial class AUSImportProductsFromCSVForm : DataLoaderForm
	{
		ZGuidFindBox SupplierFindBox;
		ZLabel ImporterLabel;
		ZLabel SupplierLabel;
		ZGuidFindBox ImporterFindBox;

		new void InitializeComponent()
		{
			this.SupplierFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ImporterFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ImporterLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SupplierLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// FileNameTextBox
			//
			this.BindingSource.SetBindingMember(this.FileNameTextBox, "DummyStringField");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Client.AUS.Products.AUSImportProductBusinessObject)(null)).DummyStringField)));
			//
			// label2
			//
			this.label2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 115, true);
			//
			// OutputListBox
			//
			this.OutputListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 136, true);
			this.OutputListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 342, true);
			//
			// CloseButton
			//
			this.CloseButton.ReadOnly = false;
			//
			// CopyLogToClipboardButton
			//
			this.CopyLogToClipboardButton.ReadOnly = false;
			//
			// MainStatusBar
			//
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 523, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 28, true);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.AUS.Products.AUSImportProductBusinessObject);
			//
			// SupplierFindBox
			//
			this.BindingSource.SetBindingMember(this.SupplierFindBox, "SupplierPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.AUS.Products.AUSImportProductBusinessObject)(null)).SupplierPK)));
			this.SupplierFindBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("AUSImportProductsFromCSVForm|2686bef0-f09b-40f0-ba91-97785541ff94", "Supplier");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SupplierFindBox, false);
			this.SupplierFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 88, true);
			this.SupplierFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.SupplierFindBox.Name = "SupplierFindBox";
			this.SupplierFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 21, true);
			this.SupplierFindBox.TabIndex = 4;
			//
			// ImporterFindBox
			//
			this.BindingSource.SetBindingMember(this.ImporterFindBox, "ImporterPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.AUS.Products.AUSImportProductBusinessObject)(null)).ImporterPK)));
			this.ImporterFindBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("AUSImportProductsFromCSVForm|55daa413-7a26-46cd-a4c3-77d0dc7664d9", "Importer");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ImporterFindBox, false);
			this.ImporterFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 64, true);
			this.ImporterFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.ImporterFindBox.Name = "ImporterFindBox";
			this.ImporterFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 21, true);
			this.ImporterFindBox.TabIndex = 3;
			//
			// ImporterLabel
			//
			this.ImporterLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(25, 62, true);
			this.ImporterLabel.Name = "ImporterLabel";
			this.ImporterLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 23, true);
			this.ImporterLabel.TabIndex = 10;
			this.ImporterLabel.Text = Res.GetString("ImportProduct|Importer", "Importer");
			//
			// SupplierLabel
			//
			this.SupplierLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(25, 86, true);
			this.SupplierLabel.Name = "SupplierLabel";
			this.SupplierLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 23, true);
			this.SupplierLabel.TabIndex = 11;
			this.SupplierLabel.Text = Res.GetString("ImportProduct|Supplier", "Supplier");
			//
			// AUSImportProductsFromCSVForm
			//

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 551, true);
			this.Controls.Add(this.SupplierLabel);
			this.Controls.Add(this.ImporterLabel);
			this.Controls.Add(this.SupplierFindBox);
			this.Controls.Add(this.ImporterFindBox);
			this.DataSourceType = typeof(Enterprise.Client.AUS.Products.AUSImportProductBusinessObject);
			this.Name = "AUSImportProductsFromCSVForm";
			this.Controls.SetChildIndex(this.ProgressBar, 0);
			this.Controls.SetChildIndex(this.FileNameTextBox, 0);
			this.Controls.SetChildIndex(this.SelectFileButton, 0);
			this.Controls.SetChildIndex(this.StartButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.label2, 0);
			this.Controls.SetChildIndex(this.OutputListBox, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.CopyLogToClipboardButton, 0);
			this.Controls.SetChildIndex(this.ImporterFindBox, 0);
			this.Controls.SetChildIndex(this.SupplierFindBox, 0);
			this.Controls.SetChildIndex(this.ImporterLabel, 0);
			this.Controls.SetChildIndex(this.SupplierLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
