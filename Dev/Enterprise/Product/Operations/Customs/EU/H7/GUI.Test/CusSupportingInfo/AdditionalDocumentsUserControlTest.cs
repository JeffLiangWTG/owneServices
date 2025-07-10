using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	[TestedType(typeof(AdditionalDocumentsUserControl))]
	sealed class AdditionalDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestAdditionalTabPage()
		{
			using (var control = new AdditionalDocumentsUserControl() as IAdditionalTabPage)
			{
				CombineAssertions("Additional Tab Page", () =>
				{
					AssertEquals("Caption", "Additional Documents", control.AdditionalTabPageCaption.Caption);
					AssertEquals("Tab page sequence", 21, control.TabPageSequence);
				});
			}
		}

		[RequiresSTA]
		public void TestLayout()
		{
			var bill = Factory.New<AsycudaBill>();

			using (var form = new ZForm(bill))
			using (var control = new AdditionalDocumentsUserControl())
			{
				control.SetDataBinding(bill, string.Empty);

				form.Controls.Add(control);
				form.Show();

				var grid = control.FindSingle<ZGrid>("AdditionalInfosGrid");

				CombineAssertions(() =>
				{
					EUH7GUITestHelper.AssertGridLayout(grid,
						[
							(CusSupportingInfo.Schema.CSI_SubType, typeof(ZDropEditColumnStyleInfo)),
							(CusSupportingInfo.Schema.CSI_Code, typeof(ZCodeFindBoxColumnStyleInfo)),
							(CusSupportingInfo.Schema.CSI_ReferenceNumber, typeof(ZTextBoxColumnStyleInfo)),
							(CusSupportingInfo.Schema.CSI_Description, typeof(ZTextBoxColumnStyleInfo))
						]);
				});
			}
		}
	}
}
