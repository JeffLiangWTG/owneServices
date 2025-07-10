using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class COAndFTADetailsUserControlTest : TestCaseWithFactory
	{
		public void TestFTADetailsUserControl()
		{
			using (var control = new COAndFTADetailsUserControl())
			{
				var groupBox = control.FindSingle<ZGroupBox>("FTADetailsGroupBox");
				AssertEquals(true, groupBox.Visible);
				AssertEquals(true, groupBox.FindSingle<ZAddressControl>("ManufacturerAddressControl").Visible);
			}
		}

		public void TestCOODetailsUserControl()
		{
			using (var control = new COAndFTADetailsUserControl())
			{
				var cooGroupBox = control.FindSingle<ZGroupBox>("CertificateOfOriginGroupBox");
				AssertEquals(true, cooGroupBox.Visible);
				var cooPanel = cooGroupBox.FindSingle<DynamicLayoutPanel>("CertificateOfOriginPanel");
				AssertNotNull(cooPanel);

				AssertEquals(true, cooPanel.FindSingle<ZCodeFindBox>("GoodsOriginCodeFindBox").Visible);
				AssertEquals(true, cooPanel.FindSingle<ZDropEdit>("CODeterminationRuleDropEdit").Visible);
				AssertEquals(true, cooPanel.FindSingle<ZDropEdit>("COLabelLocationDropEdit").Visible);
				AssertEquals(true, cooPanel.FindSingle<ZDropEdit>("COLabelTypeDropEdit").Visible);
				AssertEquals(true, cooPanel.FindSingle<ZDropEdit>("COLabelExemptionReasonDropEdit").Visible);
			}
		}

		public void TestCOOIssueUserControl()
		{
			using (var control = new COAndFTADetailsUserControl())
			{
				var cooIssueGroupBox = control.FindSingle<ZGroupBox>("CertificateOfOriginIssueGroupBox");
				AssertEquals(true, cooIssueGroupBox.Visible);
				var cooIssuePanel = cooIssueGroupBox.FindSingle<DynamicLayoutPanel>("CertificateOfOriginIssuePanel");
				AssertNotNull(cooIssuePanel);

				AssertEquals(true, cooIssuePanel.FindSingle<ZTextBox>("COReferenceNumberTextBox").Visible);
				AssertEquals(true, cooIssuePanel.FindSingle<ZDropEdit>("COSplitYNDropEdit").Visible);
				AssertEquals(true, cooIssuePanel.FindSingle<ZCodeFindBox>("COIssuingCountryCodeFindBox").Visible);
				AssertEquals(true, cooIssuePanel.FindSingle<ZTextBox>("IssuingAgencyNameTextBox").Visible);
				AssertEquals(true, cooIssuePanel.FindSingle<ZDateEdit>("COIssueDateEdit").Visible);
				AssertEquals(true, cooIssuePanel.FindSingle<ZTextBox>("IssuingAreaNameTextBox").Visible);
				AssertEquals(true, cooIssuePanel.FindSingle<ZTextBox>("IssuingPersonNameTextBox").Visible);
				AssertEquals(true, cooIssuePanel.FindSingle<ZDropEdit>("COCodeDropEdit").Visible);
			}
		}
	}
}
