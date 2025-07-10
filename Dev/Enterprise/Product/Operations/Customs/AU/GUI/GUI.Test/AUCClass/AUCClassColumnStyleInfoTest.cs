using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class AUCClassColumnStyleInfoTest : TestCaseWithFactory
	{
		public void TestEditControl()
		{
			var columnInfo = new AUCClassColumnStyleInfo();
			AssertEquals(typeof(AUCClassColumnStyle), columnInfo.ColumnStyleType);

			using (var columnStyle = (AUCClassColumnStyle)Activator.CreateInstance(columnInfo.ColumnStyleType, new object[] { columnInfo }))
			using (var editControl = columnStyle.EditControl)
			{
				AssertType<AUCClassGridFindBox>(editControl);
			}
		}

		public void TestFindBoxPopupForm()
		{
			var originalExternalBorderComplianceToolValue = Env.Registry.ExternalBorderComplianceTool;
			try
			{
				Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.None;

				using (var form = new Form())
				using (var findBox = new AUCClassGridFindBoxForTest())
				{
					form.Show();
					findBox.Parent = form;

					findBox.SelectFromPopupForm();
					AssertType(typeof(AUCClassForm), findBox.PopupForm);
					ZFormModaliser.LastFormShownForTest.Close();
				}
			}
			finally
			{
				Env.Registry.ExternalBorderComplianceTool = originalExternalBorderComplianceToolValue;
			}
		}

		class AUCClassGridFindBoxForTest : AUCClassGridFindBox
		{
			public AUCClassGridFindBoxForTest() : base()
			{
			}

			new public IFindBoxPopup PopupForm => base.PopupForm;
		}
	}
}

