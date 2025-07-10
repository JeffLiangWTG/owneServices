using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing
{
	sealed class ExitSummaryMainPanelFieldsUserControlTest : TestCase
	{
		public void TestReferenceNumberTextBox()
		{
			AssertType<ZTextBox>(control.ReferenceNumberTextBox);
		}

		public void TestHeaderCustomsOfficeCodeFindBox()
		{
			AssertType<ZCodeFindBox>(control.HeaderCustomsOfficeCodeFindBox);
		}

		public void TestHeaderArrivalNotificationDateDateEdit()
		{
			AssertType<ZDateEdit>(control.HeaderArrivalNotificationDateDateEdit);
		}

		public void TestHeaderArrivalNotificationPlaceTextBox()
		{
			AssertType<ZTextBox>(control.HeaderArrivalNotificationPlaceTextBox);
		}

		public void TestHeaderExitDateDateEdit()
		{
			AssertType<ZDateEdit>(control.HeaderExitDateDateEdit);
		}

		public void TestHeaderTransportIdTextBox()
		{
			AssertType<ZTextBox>(control.HeaderTransportIdTextBox);
		}

		public void TestAgentOrgAddressControl()
		{
			AssertType<ZOrgAddressControl>(control.AgentOrgAddressControl);
		}

		public void TestHeaderCarrierOrgAddressControl()
		{
			AssertType<ZOrgAddressControl>(control.HeaderCarrierOrgAddressControl);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new ExitSummaryMainPanelFieldsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		ExitSummaryMainPanelFieldsUserControl control;
	}
}
