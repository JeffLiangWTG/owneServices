using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.JobDeclarationForms.Testing
{
	class EntryInstructionGridUserControlTest : TestCaseWithFactory
	{
		public void TestAvailableColumns()
		{
			var grid = control.FindSingle<ZGrid>("EntryInstructionsGrid");
			CombineAssertions(() =>
			{
				AssertEquals("Count", 9, grid.ColumnStyles.Count);
				AssertEquals("Caption Visible", false, grid.CaptionVisible);
			});
		}

		public void TestColumnCEI_SubStyle()
		{
			var columnStyle = control.FindSingle<ZGrid>("EntryInstructionsGrid").GetColumnStyle(CusEntryInstruction.Schema.CEI_SubStyle);
			CombineAssertions(() =>
			{
				AssertEquals("Visible", true, columnStyle.IsVisible);
				AssertEquals("Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(68), columnStyle.Width);
			});
		}

		public void TestColumnWarehouseIDFor27()
		{
			var columnStyle = control.FindSingle<ZGrid>("EntryInstructionsGrid").GetColumnStyle(CusEntryInstruction.Schema.WarehouseIDFor27);
			CombineAssertions(() =>
			{
				AssertEquals("Visible", false, columnStyle.IsVisible);
				AssertEquals("Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(133), columnStyle.Width);
				AssertEquals("Caption", "Warehouse 2/7", columnStyle.CaptionResourceString.Caption);
			});
		}

		public void TestColumnCEI_Style()
		{
			var columnStyle = control.FindSingle<ZGrid>("EntryInstructionsGrid").GetColumnStyle(CusEntryInstruction.Schema.CEI_Style);
			CombineAssertions(() =>
			{
				AssertEquals("Visible", true, columnStyle.IsVisible);
				AssertEquals("Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(105), columnStyle.Width);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnStyle.CharacterCasing);
			});
		}

		public void TestColumnCEI_Description()
		{
			var columnStyle = control.FindSingle<ZGrid>("EntryInstructionsGrid").GetColumnStyle(CusEntryInstruction.Schema.CEI_Description);
			CombineAssertions(() =>
			{
				AssertEquals("Visible", true, columnStyle.IsVisible);
				AssertEquals("Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(250), columnStyle.Width);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnStyle.CharacterCasing);
			});
		}

		public void TestColumnCEI_DisplaySequence()
		{
			var columnStyle = control.FindSingle<ZGrid>("EntryInstructionsGrid").GetColumnStyle(CusEntryInstruction.Schema.CEI_DisplaySequence);
			CombineAssertions(() =>
			{
				AssertEquals("Visible", true, columnStyle.IsVisible);
				AssertEquals("Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(100), columnStyle.Width);
			});
		}

		public void TestCEI_SplitReference()
		{
			var columnStyle = control.FindSingle<ZGrid>("EntryInstructionsGrid").GetColumnStyle(CusEntryInstruction.Schema.CEI_SplitReference);
			CombineAssertions(() =>
			{
				AssertEquals("Visible", true, columnStyle.IsVisible);
				AssertEquals("Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(133), columnStyle.Width);
				AssertEquals("CharacterCasing", CharacterCasing.Normal, columnStyle.CharacterCasing);
				AssertEquals("Caption", "Split Reference", columnStyle.CaptionResourceString.Caption);
			});
		}

		public void TestCEI_PackageCount()
		{
			var columnStyle = control.FindSingle<ZGrid>("EntryInstructionsGrid").GetColumnStyle(CusEntryInstruction.Schema.CEI_PackageCount);
			CombineAssertions(() =>
			{
				AssertEquals("Visible", true, columnStyle.IsVisible);
				AssertEquals("Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(200), columnStyle.Width);
				AssertEquals("Caption", "[UCC 6/18] Package Count", columnStyle.CaptionResourceString.Caption);
			});
		}

		public void TestIsPostponedVatViaFiscalReference()
		{
			var columnStyle = control.FindSingle<ZGrid>("EntryInstructionsGrid").GetColumnStyle(CusEntryInstruction.Schema.IsPostponedVatViaFiscalReference);
			CombineAssertions(() =>
			{
				AssertEquals("Visible", true, columnStyle.IsVisible);
				AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40), columnStyle.Width);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new EntryInstructionGridUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control?.Dispose();
		}

		EntryInstructionGridUserControl control;
	}
}
