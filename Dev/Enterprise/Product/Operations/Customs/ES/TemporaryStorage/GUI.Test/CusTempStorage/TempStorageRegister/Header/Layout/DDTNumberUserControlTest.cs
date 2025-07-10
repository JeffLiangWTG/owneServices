using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Registry;
using Enterprise.Customs.ES.TemporaryStorage.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using CusTempStorageRegPremisesTypeList = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegPremisesTypeList;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(DDTNumberUserControl))]
	class DDTNumberUserControlTest : TestCaseWithFactory
	{
		public void TestDDTNumberTextBox()
		{
			using var control = new DDTNumberUserControl();
			AssertEquals("GetBindingMember", nameof(CusTempStorageRegHeader.SRH_Reference), control.DDTNumberTextBox.GetBindingMember());
		}

		public void TestGotToURLButtonVisibility()
		{
			var register = CreateTempStorageregHeader();

			AssertVisibility(true, CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse);
			AssertVisibility(false, CusTempStorageRegPremisesTypeList.Codes.ExportStorageFacility);

			void AssertVisibility(bool expectedVisibility, ZString srpType)
			{
				register.Premises.SRP_Type = srpType;
				using var form = new ZForm(register);
				using var control = new DDTNumberUserControl();
				form.Controls.Add(control);
				form.Show();
				AssertEquals($"SRP_Type={srpType} Visible", expectedVisibility, control.GoToUrlButton.Visible);
			}
		}

		public void TestGotToURLButtonClicked()
		{
			const string url = "http://example.org";

			var register = CreateTempStorageregHeader();

			using (ESCustomsDataRegistry.Instance.SummaryDeclarationStatusQueryURL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, url))
			{
				using var form = new ZForm(register);
				using var control = new DDTNumberUserControl();
				form.Controls.Add(control);
				form.Show();
				WebUrlLauncher.ClearLastUrlLaunched();
				control.GoToUrlButton.PerformClick();
				AssertEquals(url, WebUrlLauncher.LastUrlLaunched);
			}
		}

		CusTempStorageRegHeader CreateTempStorageregHeader()
		{
			var premises = Factory.New<CusTempStorageRegPremises>();
			premises.SRP_Type = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
			var register = Factory.New<CusTempStorageRegHeader>();
			register.SRH_SRP_Premises = premises.PK;
			return register;
		}
	}
}
