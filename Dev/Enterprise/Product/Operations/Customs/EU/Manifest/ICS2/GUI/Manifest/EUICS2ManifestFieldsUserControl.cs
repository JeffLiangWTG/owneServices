using System.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class EUICS2ManifestFieldsUserControl : ZUserControl
	{
		public EUICS2ManifestFieldsUserControl()
		{
			InitializeComponent();
			BranchGuidFindBox.ParentModuleID = ModuleIDs.NotAssigned;
			OriginCodeFindBox.ParentModuleID = ModuleIDs.NotAssigned;
			FinalDestinationCodeFindBox.ParentModuleID = ModuleIDs.NotAssigned;

			VehicleRegistrationAndNationalityUserControl.SuspendLayout();
			VehicleRegistrationAndNationalityUserControl.VehicleNationalityCodeFindBox.ShowDescriptionBox = true;
			VehicleRegistrationAndNationalityUserControl.VehicleNationalityCodeFindBox.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			VehicleRegistrationAndNationalityUserControl.ResumeLayout(true);
			VehicleRegistrationAndNationalityUserControl.PerformLayout();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var newDataSource = dataSource as AsycudaManifestHeader;
			base.SetDataBinding(newDataSource, "");
		}
	}
}
