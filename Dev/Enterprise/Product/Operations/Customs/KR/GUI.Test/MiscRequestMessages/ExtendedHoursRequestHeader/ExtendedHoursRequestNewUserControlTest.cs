using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class ExtendedHoursRequestNewUserControlTest : TestCaseWithFactory
	{
		public void TestControls_HeaderArea()
		{
			AssertControls_HeaderArea(ElectronicDocumentTypeList.Codes._5AC);
			AssertControls_HeaderArea(ElectronicDocumentTypeList.Codes._5GW);
		}

		public void AssertControls_HeaderArea(ZString type)
		{
			using (var form = new ExtendedHoursRequestNewForm(new Business.ExtendedHoursRequestHeader(Factory, type, GlbCompany.CurrentCompany.PK)))
			using (var control = new ExtendedHoursRequestNewUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var dynamicHeaderDetailsPanel = control.FindSingle<DynamicLayoutPanel>("ExtendedHoursRequestHeaderPanel");
				DynamicLayoutPanelTest.AssertControlsOrder(dynamicHeaderDetailsPanel,
					nameof(ExtendedHoursRequestNewControlBag.MessageTypeDropEdit),
					nameof(ExtendedHoursRequestNewControlBag.RequestReasonTextBox),
					nameof(ExtendedHoursRequestNewControlBag.CustomsOfficeCodeFindBox),
					nameof(ExtendedHoursRequestNewControlBag.CustomsDivisionCodeFindBox),
					nameof(ExtendedHoursRequestNewControlBag.RequestPeriodStartDateEdit),
					nameof(ExtendedHoursRequestNewControlBag.RequestPeriodEndDateEdit),
					nameof(ExtendedHoursRequestNewControlBag.BranchGuidFindBox));

				var reasonTextBox = dynamicHeaderDetailsPanel.FindSingle<ZTextBox>("RequestReasonTextBox");
				AssertEquals(true, reasonTextBox.Multiline);
				AssertEquals(true, reasonTextBox.AcceptsReturn);
				AssertEquals(System.Windows.Forms.ScrollBars.Vertical, reasonTextBox.ScrollBars);
			}
		}
	}
}
