using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	public class AHECCTariffColumnStyleInfoTest : TestCaseWithFactory
	{
		public void TestEditControl()
		{
			bool isImport = false;

			var columnInfo = new AHECCTariffColumnStyleInfo(() => isImport);
			AssertEquals(typeof(AHECCTariffColumnStyleInfo.AHECCTariffColumnStyle), columnInfo.ColumnStyleType);

			using (var columnStyle = (AHECCTariffColumnStyleInfo.AHECCTariffColumnStyle)Activator.CreateInstance(columnInfo.ColumnStyleType, new object[] { columnInfo }))
			using (var editControl = columnStyle.EditControl)
			{
				AssertType<AHECCTariffGridFindBox>(editControl);

				var findBox = (AHECCTariffGridFindBox)editControl;
				Assert(!findBox.IsImport);
				isImport = true;
				Assert(findBox.IsImport);
			}
		}

		public void TestFindBoxPopupForm()
		{
			var originalExternalBorderComplianceToolValue = Env.Registry.ExternalBorderComplianceTool;
			try
			{
				Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.None;
				bool isImport = true;

				using (var form = new Form())
				using (var findBox = new TariffGridFindBoxForTest(() => isImport))
				{
					form.Show();
					findBox.Parent = form;

					findBox.SelectFromPopupForm();
					AssertType(typeof(AUCClassForm), findBox.PopupForm);
					ZFormModaliser.LastFormShownForTest.Close();

					isImport = false;

					findBox.SelectFromPopupForm();
					AssertType(typeof(AHECCForm), findBox.PopupForm);
					ZFormModaliser.LastFormShownForTest.Close();
				}
			}
			finally
			{
				Env.Registry.ExternalBorderComplianceTool = originalExternalBorderComplianceToolValue;
			}
		}

		class TariffGridFindBoxForTest : AHECCTariffGridFindBox
		{
			public TariffGridFindBoxForTest(IsImportDelegate isImportDelegate) : base(isImportDelegate)
			{
			}

			new public IFindBoxPopup PopupForm => base.PopupForm;
		}
	}
}
