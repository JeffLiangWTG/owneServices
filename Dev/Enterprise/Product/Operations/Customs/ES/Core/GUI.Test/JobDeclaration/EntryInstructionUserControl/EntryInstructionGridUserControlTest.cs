using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Testing
{
	sealed class EntryInstructionGridUserControlTest : TestCaseWithFactory
	{
		public void TestAvailableColumns()
		{
			var entryInstructionsGrid = GetEntryInstructionsGrid();
			AssertEquals("Count", 3, entryInstructionsGrid.ColumnStyles.Count);
		}

		public void TestColumnCEI_SubStyle()
		{
			var entryInstructionsGrid = GetEntryInstructionsGrid();
			AssertEquals(typeof(ZDropEditColumnStyle), entryInstructionsGrid.GetColumnStyle(CusEntryInstruction.Schema.CEI_SubStyle).ColumnStyleType);
		}

		public void TestCEI_Description()
		{
			var entryInstructionsGrid = GetEntryInstructionsGrid();
			AssertEquals(typeof(ZTextBoxColumnStyle), entryInstructionsGrid.GetColumnStyle(CusEntryInstruction.Schema.CEI_Description).ColumnStyleType);
		}

		public void TestCEI_Style()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.CustomsEntryInstructions.AddNew();
			using (var form = new ZForm(declaration))
			{
				form.SetDataBinding(declaration, ".");
				form.Controls.Add(control);
				form.Show();

				var entryInstructionsGrid = GetEntryInstructionsGrid();
				var styleColumn = entryInstructionsGrid.GetColumnStyle(CusEntryInstruction.Schema.CEI_Style);
				AssertEquals(typeof(ZDropEditColumnStyle), styleColumn.ColumnStyleType);
				AssertEquals("column available when Import declaration", false, styleColumn.IsUnavailable);

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				entryInstructionsGrid = GetEntryInstructionsGrid();
				styleColumn = entryInstructionsGrid.GetColumnStyle(CusEntryInstruction.Schema.CEI_Style);
				AssertEquals("column unavailable when Export declaration", true, styleColumn.IsUnavailable);
			}
		}

		EntryInstructionGridUserControl control;
		protected override void SetUp()
		{
			base.SetUp();
			control = new EntryInstructionGridUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		ZGrid GetEntryInstructionsGrid() => control.FindSingle<ZGrid>("EntryInstructionsGrid");
	}
}
