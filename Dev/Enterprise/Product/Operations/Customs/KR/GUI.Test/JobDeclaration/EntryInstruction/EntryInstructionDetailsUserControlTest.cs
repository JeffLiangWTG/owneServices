using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(EntryInstructionDetailsUserControl))]
	sealed class EntryInstructionDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestFTADetailsGroupBox()
		{
			using (var control = new EntryInstructionDetailsUserControl())
			{
				DynamicLayoutPanel dynamicFTADetailsLayoutPanel = (DynamicLayoutPanel)control.Controls.Find("DynamicFTADetailsLayoutPanel", true)[0];
				AssertNotNull(dynamicFTADetailsLayoutPanel);
			}
		}

		public void TestInstructions()
		{
			using (var control = new EntryInstructionDetailsUserControl())
			{
				var grid = control.FindSingle<ZGrid>("EntryInstructionsGrid");
				var index = 0;
				AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index++]).ColumnName, CusEntryInstruction.Schema.CEI_Style);
				AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index++]).ColumnName, CusEntryInstruction.Schema.CEI_Description);
				AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index++]).ColumnName, nameof(CusEntryInstruction.AcceptedDate));
			}
		}

		public void TestDetailsGroupBox()
		{
			using (var control = new EntryInstructionDetailsUserControl())
			{
				var detailGroupBox = control.FindSingle<ZGroupBox>("DetailsGroupBox");

				AssertEquals(false, detailGroupBox.FindSingle<ZTextBox>("CPCTextBox").ReadOnly);
				AssertEquals(false, detailGroupBox.FindSingle<ZTextBox>("DescriptionTextBox").ReadOnly);
				AssertEquals(true, detailGroupBox.FindSingle<ZDateEdit>("AssessmentDateEdit").ReadOnly);
			}
		}
		public void TestMisc929DetailsItem()
		{
			using (var userControl = new EntryInstructionDetailsUserControl())
			{
				var dynamicLayoutPanel = userControl.FindSingle<DynamicLayoutPanel>("DynamicMisc929DetailsItemLayoutPanel");
				AssertNotNull(dynamicLayoutPanel);
			}
		}

		public void TestBondedFactory()
		{
			using (var userControl = new EntryInstructionDetailsUserControl())
			{
				var dynamicLayoutPanel = userControl.FindSingle<DynamicLayoutPanel>("DynamicBondedFactoryLayoutPanel");
				AssertNotNull(dynamicLayoutPanel);
			}
		}

		public void TestOnlineOrders()
		{
			using (var userControl = new EntryInstructionDetailsUserControl())
			{
				var grid = userControl.FindSingle<ZGrid>("OnlineOrdersGrid");
				var index = 0;

				AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index++]).ColumnName, OnlineOrder.Schema.CY_Order);
				AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index++]).ColumnName, OnlineOrder.Schema.CY_Data);
			}
		}
	}
}
