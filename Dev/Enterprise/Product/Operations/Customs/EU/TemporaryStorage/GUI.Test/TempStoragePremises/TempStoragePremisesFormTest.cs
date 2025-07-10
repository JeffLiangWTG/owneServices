using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(TempStoragePremisesForm))]
	class TempStoragePremisesFormTest : ZFormBasherTest
	{
		public void TestMainDynamicLayoutPanel()
		{
			using var form = (TempStoragePremisesForm)GetFormToBash();

			var panel = form.MainDynamicLayoutPanel;
			AssertNotNull(panel);
			AssertEquals("Padding top is 20", 20, panel.Padding.Top);
		}

		public void TestFormCaption()
		{
			var tempStorage = Factory.New<CusTempStorageRegPremises>();
			tempStorage.SRP_Code = "JobReference";

			using var form = new TempStoragePremisesForm(tempStorage);
			AssertEquals("Temporary Storage Premises - JobReference", form.FormCaption);
		}

		public void TestNumberRangeSettingTabPage()
		{
			using var form = (TempStoragePremisesForm)GetFormToBash();

			var tabPage = form.NumberRangeSettingTabPage;
			AssertNotNull(tabPage);
		}

		public void TestCustomsNumberViewStmNumsTabPageUserControl()
		{
			using var form = (TempStoragePremisesForm)GetFormToBash();

			form.Show();

			var customsNumViewStmNumsUserControl = form.customsNumberViewStmNumsTabPageUserControl;
			AssertNotNull(customsNumViewStmNumsUserControl);
			AssertType<MasterFiles.GUI.CustomsNumberViewStmNumsTabPageUserControl>(customsNumViewStmNumsUserControl);
		}

		protected override Form GetFormToBashCore()
		{
			var premises = Factory.New<CusTempStorageRegPremises>();
			premises.SRP_Code = "X";
			premises.SRP_Description = "DESC";

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "AAA";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address";
			premises.SRP_OA_PremisesAddress = orgAddress.PK;
			premises.AuthorizationNumber = "AH3";

			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var loadedPremises = newFactory.Load<CusTempStorageRegPremises>(premises.PK);
			var result = new TempStoragePremisesForm(loadedPremises);
			result.ControllerID = ControllerIDs.Customs.EU.TempStoragePremises;
			return result;
		}
	}
}
