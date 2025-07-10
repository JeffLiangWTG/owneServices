using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.DE;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.GUI.Testing
{
	class ExportCustomsPackingUserControlTest : TestCaseWithFactory
	{
		public void TestPackingDetailsGrid()
		{
			using (var control = new ExportCustomsPackingUserControl())
			{
				CombineAssertions(() =>
				{
					var packingDetailsGrid = control.FindSingle<ZGrid>("PackingDetailsGrid");

					var sealColumnStyleInfo = packingDetailsGrid.GetColumnStyle(CusDecHouseContainerPackSchema.Constants.CW_Seal) as ZTextBoxColumnStyleInfo;
					AssertEquals("SealColumnStyleInfo CharacterCasing", System.Windows.Forms.CharacterCasing.Normal, sealColumnStyleInfo.CharacterCasing);
					AssertEquals("SealColumnStyleInfo Width", 129, sealColumnStyleInfo.Width);
				});
			}
		}

		public void TestSealColumnVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = DEJobMessageTypeList.Codes.Export;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var userControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				userControl.MainTabControl.SelectedTab = userControl.PackingTabPage;
				var packingDetailsGrid = userControl.FindSingle<ZGrid>("PackingDetailsGrid");

				CombineAssertions(() =>
				{
					declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
					AssertNull("JE_ContainerMode containerized, CW_Seal should be hidden", packingDetailsGrid.Columns[CusDecHouseContainerPackSchema.Constants.CW_Seal]);

					declaration.JE_ContainerMode = Core.Constants.ContainerModes.Bulk;
					AssertNotNull("JE_ContainerMode non-containerized, CW_Seal should be visible", packingDetailsGrid.Columns[CusDecHouseContainerPackSchema.Constants.CW_Seal]);
				});
			}
		}
	}
}
