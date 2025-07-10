using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class Import5FNMessageDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestDetailsTabPage()
		{
			using (var userControl = new Import5FNMessageDetailsUserControl())
			{
				var detailsPanel = userControl.FindSingle<DynamicLayoutPanel>("DetailsPanel");
				AssertNotNull(detailsPanel.FindSingle<ZDropEdit>(nameof(DetailsControlBag.DutyReductionTypeDropEdit)));
				AssertNotNull(detailsPanel.FindSingle<ZCheckBox>(nameof(DetailsControlBag.SpecificUseCheckBox)));
				AssertNotNull(detailsPanel.FindSingle<ZCodeFindBox>(nameof(DetailsControlBag.DutyReductionCodeFindBox)));
				AssertNotNull(detailsPanel.FindSingle<ZCodeFindBox>(nameof(DetailsControlBag.InstalmentCodeFindBox)));
				AssertNotNull(detailsPanel.FindSingle<ZTextBox>(nameof(DetailsControlBag.RemarkTextBox)));

				var postClearanceDetailsPanel = userControl.FindSingle<DynamicLayoutPanel>("PostClearanceDetailsPanel");
				AssertNotNull(postClearanceDetailsPanel.FindSingle<ZDropEdit>(nameof(PostClearanceDetailsControlBag.PostClearanceYNDropEdit)));
				AssertNotNull(postClearanceDetailsPanel.FindSingle<ZTextBox>(nameof(PostClearanceDetailsControlBag.UseCodeDescriptionTextBox)));
				AssertNotNull(postClearanceDetailsPanel.FindSingle<ZDropEdit>(nameof(PostClearanceDetailsControlBag.ProductTypeDropEdit)));
				AssertNotNull(postClearanceDetailsPanel.FindSingle<ZTextBox>(nameof(PostClearanceDetailsControlBag.SerialNumberTextBox)));
				AssertNotNull(postClearanceDetailsPanel.FindSingle<ZCodeFindBox>(nameof(PostClearanceDetailsControlBag.CustomsOfficeCodeFindBox)));
				AssertNotNull(postClearanceDetailsPanel.FindSingle<ZAddressControl>(nameof(PostClearanceDetailsControlBag.GoodsLocationAddressControl)));

				var dutyReductionDetailsPanel = userControl.FindSingle<DynamicLayoutPanel>("DutyReductionDetailsPanel");
				AssertNotNull(dutyReductionDetailsPanel.FindSingle<ZDropEdit>(nameof(DutyReductionDetailsControlBag.GroupNumberDropEdit)));
				AssertNotNull(dutyReductionDetailsPanel.FindSingle<ZTextBox>(nameof(DutyReductionDetailsControlBag.SeqNumberTextBox)));
				AssertNotNull(dutyReductionDetailsPanel.FindSingle<ZTextBox>(nameof(DutyReductionDetailsControlBag.ItemNumberTextBox)));

				var reExportReductionDetailsPanel = userControl.FindSingle<DynamicLayoutPanel>("ReExportReductionDetailsPanel");
				AssertNotNull(reExportReductionDetailsPanel.FindSingle<ZCodeFindBox>(nameof(ReExportReductionDetailsControlBag.CustomsOfficeCodeFindBox)));
				AssertNotNull(reExportReductionDetailsPanel.FindSingle<ZCodeFindBox>(nameof(ReExportReductionDetailsControlBag.DestCountryCodeFindBox)));
				AssertNotNull(reExportReductionDetailsPanel.FindSingle<ZDateEdit>(nameof(ReExportReductionDetailsControlBag.EstimateDateEdit)));
			}
		}
	}
}
