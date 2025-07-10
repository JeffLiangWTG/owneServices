using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class CusMiscRequestUserControlTest : TestCaseWithFactory
	{
		public void TestControls_ExportHeaderArea()
		{
			var header = Factory.New<CusMiscRequestHeader>();
			header.CMR_MessageType = ElectronicDocumentTypeList.Codes._5AC;

			using (var form = new CusMiscRequestHeaderViewForm(header))
			using (var control = new CusMiscRequestHeaderViewUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var dynamicHeaderDetailsPanel = control.FindSingle<DynamicLayoutPanel>("DynamicHeaderDetailsPanel");
				DynamicLayoutPanelTest.AssertControlsOrder(dynamicHeaderDetailsPanel,
					nameof(CusMiscRequestHeaderViewControlBag.MessageTypeDropEdit),
					nameof(CusMiscRequestHeaderViewControlBag.ApplicationNumberTextBox),
					nameof(CusMiscRequestHeaderViewControlBag.CustomsOfficeCodeFindBox),
					nameof(CusMiscRequestHeaderViewControlBag.StatusDropEdit),
					nameof(CusMiscRequestHeaderViewControlBag.CustomsDivisionCodeFindBox),
					nameof(CusMiscRequestHeaderViewControlBag.RequestDateEdit),
					nameof(CusMiscRequestHeaderViewControlBag.BranchGuidFindBox),
					nameof(CusMiscRequestHeaderViewControlBag.EntryCountCalcEdit),
					nameof(CusMiscRequestHeaderViewControlBag.RequestDetailsTextBox));
			}
		}

		public void TestControls_RelatedEntriesArea_5AC()
		{
			var header = Factory.New<CusMiscRequestHeader>();
			header.CMR_MessageType = ElectronicDocumentTypeList.Codes._5AC;

			using (var form = new CusMiscRequestHeaderViewForm(header))
			using (var control = new CusMiscRequestHeaderViewUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var dynamicLinePanel = control.FindSingle<DynamicLayoutPanel>("DynamicRequestLinePanel");
				DynamicLayoutPanelTest.AssertControlsOrder(dynamicLinePanel,
					nameof(MiscRequestRelatedEntriesControlBag.EntryNumberTextBox),
					nameof(MiscRequestRelatedEntriesControlBag.EntryDetailsTextBox));

				var grid = control.FindSingle<ZGrid>("CusMiscRequestLinesBoundGrid");
				var index = 0;

				AssertEquals(grid.Columns[index++].ColumnName, "FormattedEntryNumber");
				AssertEquals(grid.Columns[index++].ColumnName, CusMiscRequestLine.Schema.CML_Remarks);
			}
		}
		public void TestControls_RelatedEntriesArea_5GW()
		{
			var header = Factory.New<CusMiscRequestHeader>();
			header.CMR_MessageType = ElectronicDocumentTypeList.Codes._5GW;

			using (var form = new CusMiscRequestHeaderViewForm(header))
			using (var control = new CusMiscRequestHeaderViewUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var dynamicLinePanel = control.FindSingle<DynamicLayoutPanel>("DynamicRequestLinePanel");
				DynamicLayoutPanelTest.AssertControlsOrder(dynamicLinePanel,
					nameof(MiscRequestRelatedEntriesControlBag.EntryTypeDropEdit),
					nameof(MiscRequestRelatedEntriesControlBag.EntryNumberTextBox),
					nameof(MiscRequestRelatedEntriesControlBag.EntryDetailsTextBox));

				var grid = control.FindSingle<ZGrid>("CusMiscRequestLinesBoundGrid");
				var index = 0;

				AssertEquals(grid.Columns[index++].ColumnName, nameof(KR.Business.CusMiscRequestLine.EntryType));
				AssertEquals(grid.Columns[index++].ColumnName, "FormattedEntryNumber");
				AssertEquals(grid.Columns[index++].ColumnName, CusMiscRequestLine.Schema.CML_Remarks);
			}
		}

		public void TestControls_ImportHeaderArea5GW()
		{
			var header = Factory.New<CusMiscRequestHeader>();
			header.CMR_MessageType = ElectronicDocumentTypeList.Codes._5GW;

			using (var form = new CusMiscRequestHeaderViewForm(header))
			using (var control = new CusMiscRequestHeaderViewUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var dynamicHeaderDetailsPanel = control.FindSingle<DynamicLayoutPanel>("DynamicHeaderDetailsPanel");
				DynamicLayoutPanelTest.AssertControlsOrder(dynamicHeaderDetailsPanel,
					nameof(CusMiscRequestHeaderViewControlBag.MessageTypeDropEdit),
					nameof(CusMiscRequestHeaderViewControlBag.ApplicationNumberTextBox),
					nameof(CusMiscRequestHeaderViewControlBag.CustomsOfficeCodeFindBox),
					nameof(CusMiscRequestHeaderViewControlBag.StatusDropEdit),
					nameof(CusMiscRequestHeaderViewControlBag.CustomsDivisionCodeFindBox),
					nameof(CusMiscRequestHeaderViewControlBag.RequestDateEdit),
					nameof(CusMiscRequestHeaderViewControlBag.BranchGuidFindBox),
					nameof(CusMiscRequestHeaderViewControlBag.EntryCountCalcEdit),
					nameof(CusMiscRequestHeaderViewControlBag.RequestDetailsTextBox));
			}
		}

		public void TestControls_ImportHeaderArea5SG()
		{
			var header = Factory.New<CusMiscRequestHeader>();
			header.CMR_MessageType = ElectronicDocumentTypeList.Codes._5SG;

			using (var form = new CusMiscRequestHeaderViewForm(header))
			using (var control = new CusMiscRequestHeaderViewUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var dynamicHeaderDetailsPanel = control.FindSingle<DynamicLayoutPanel>("DynamicHeaderDetailsPanel");
				DynamicLayoutPanelTest.AssertControlsOrder(dynamicHeaderDetailsPanel,
					nameof(CusMiscRequestHeaderViewControlBag.MessageTypeDropEdit),
					nameof(CusMiscRequestHeaderViewControlBag.ApplicationNumberTextBox),
					nameof(CusMiscRequestHeaderViewControlBag.CustomsOfficeCodeFindBox),
					nameof(CusMiscRequestHeaderViewControlBag.StatusDropEdit),
					nameof(CusMiscRequestHeaderViewControlBag.BranchGuidFindBox),
					nameof(CusMiscRequestHeaderViewControlBag.CustomsReviewStatusDropEdit),
					nameof(CusMiscRequestHeaderViewControlBag.RequestDateEdit),
					nameof(CusMiscRequestHeaderViewControlBag.ReviewDateEdit),
					nameof(CusMiscRequestHeaderViewControlBag.EntryCountCalcEdit));
			}
		}

		public void TestCaptionsByMessageType()
		{
			var header = Factory.New<CusMiscRequestHeader>();
			header.CMR_MessageType = ElectronicDocumentTypeList.Codes._5SG;

			using (var form = new CusMiscRequestHeaderViewForm(header))
			{
				using (var control = new CusMiscRequestHeaderViewUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					AssertEquals("Import Entry Number", control.FindSingle<ZTextBox>("EntryNumberTextBox").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("Request Details", control.FindSingle<ZTextBox>("EntryDetailsTextBox").GetExtension<ILabelCaptionRenderer>().Caption);
				}

				header.CMR_MessageType = ElectronicDocumentTypeList.Codes._5AC;

				using (var control = new CusMiscRequestHeaderViewUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					AssertEquals("Entry Number", control.FindSingle<ZTextBox>("EntryNumberTextBox").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("Entry Details", control.FindSingle<ZTextBox>("EntryDetailsTextBox").GetExtension<ILabelCaptionRenderer>().Caption);
				}

				header.CMR_MessageType = ElectronicDocumentTypeList.Codes._5GW;

				using (var control = new CusMiscRequestHeaderViewUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					AssertEquals("Entry Number", control.FindSingle<ZTextBox>("EntryNumberTextBox").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("Entry Details", control.FindSingle<ZTextBox>("EntryDetailsTextBox").GetExtension<ILabelCaptionRenderer>().Caption);
				}
			}
		}
	}
}
