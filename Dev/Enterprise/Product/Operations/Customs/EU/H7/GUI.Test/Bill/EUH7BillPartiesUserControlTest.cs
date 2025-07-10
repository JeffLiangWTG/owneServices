using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	[TestedType(typeof(EUH7BillPartiesUserControl))]
	sealed class EUH7BillPartiesUserControlTest : TestCaseWithFactory
	{
		public void TestIdentificationNoTextBox()
		{
			using (var control = new EUH7BillPartiesUserControl())
			{
				control.Show();

				var identificationNoTextBox = control.FindSingle<ZTextBox>("IdentificationNoTextBox");

				AssertEquals("BindingMember", "ABL_ConsigneeRegNo", identificationNoTextBox.GetBindingMember());
				AssertEquals("Caption", "Identification No.", identificationNoTextBox.CaptionResourceString.Caption);
			}
		}

		public void TestImporterIdentificationTypeDropEdit()
		{
			using (var control = new EUH7BillPartiesUserControl())
			{
				control.Show();
				var dropEdit = control.FindSingle<ZDropEdit>("ImporterIdentificationTypeDropEdit");

				AssertEquals("BindingMember", "ABL_ConsigneeRegNoType", dropEdit.GetBindingMember());
				AssertEquals("Caption", "ID No. Type", dropEdit.CaptionResourceString.Caption);
			}
		}

		public void TestCountryOfImportDropEdit()
		{
			using (var control = new EUH7BillPartiesUserControl())
			{
				control.Show();
				var dropEdit = control.FindSingle<ZDropEdit>("CountryOfImportDropEdit");

				AssertEquals("BindingMember", "ABL_RN_NKConsigneeCountry", dropEdit.GetBindingMember());
			}
		}

		public void TestCountryOfExportDropEdit()
		{
			using (var control = new EUH7BillPartiesUserControl())
			{
				control.Show();
				var dropEdit = control.FindSingle<ZDropEdit>("CountryOfExportDropEdit");

				AssertEquals("BindingMember", "ABL_RN_NKShipperCountry", dropEdit.GetBindingMember());
			}
		}

		public void TestCountryOfSellerDropEdit()
		{
			using (var control = new EUH7BillPartiesUserControl())
			{
				control.Show();
				var dropEdit = control.FindSingle<ZDropEdit>("CountryOfSellerDropEdit");

				AssertEquals("BindingMember", "ABL_RN_NKSellerCountry", dropEdit.GetBindingMember());
			}
		}
	}
}
