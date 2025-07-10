using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.Testing;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class EntryInstructionGridUserControlTest : TestCaseWithFactory
	{
		public void TestAvailableColumns()
		{
			AssertEquals("Count", 6, control.EntryInstructionsGrid.ColumnStyles.Count);
		}

		public void TestColumnCEI_Style()
		{
			var columnstyle = control.EntryInstructionsGrid.GetColumnStyle(CusEntryInstruction.Schema.CEI_Style);
			CombineAssertions(() =>
			{
				AssertEquals("Visible", true, columnstyle.IsVisible);
				AssertEquals("Width", 107, columnstyle.Width);
				AssertEquals("Character Casing", CharacterCasing.Normal, columnstyle.CharacterCasing);
			});
		}

		public void TestColumnCEI_SubStyle()
		{
			var columnstyle = control.EntryInstructionsGrid.GetColumnStyle(CusEntryInstruction.Schema.CEI_SubStyle);
			CombineAssertions(() =>
			{
				AssertEquals("Width", 96, columnstyle.Width);
				AssertEquals("Visible", true, columnstyle.IsVisible);
			});
		}

		public void TestColumnCEI_Description()
		{
			var columnstyle = control.EntryInstructionsGrid.GetColumnStyle(CusEntryInstruction.Schema.CEI_Description);
			CombineAssertions(() =>
			{
				AssertEquals("Visible", true, columnstyle.IsVisible);
				AssertEquals("Width", 290, columnstyle.Width);
			});
		}

		public void TestCEI_TotalInnerPackages()
		{
			using (var control = new EntryInstructionGridUserControl())
			{
				var columnstyle = control.EntryInstructionsGrid.GetColumnStyle(CusEntryInstruction.Schema.CEI_TotalInnerPackages);
				CombineAssertions(() =>
				{
					AssertEquals("Visible", true, columnstyle.IsVisible);
					AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90), columnstyle.Width);
				});
			}
		}

		public void TestCEI_Procedure_Support()
		{
			var declaration = Factory.New<JobDeclarationForTestOnly_RequestedProcedure>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				declaration.CustomsEntryInstructions.AddNew();
				using (var control = new EntryInstructionGridUserControl())
				{
					control.SetDataBinding(declaration, "");
					var procedureColumnstyle = control.EntryInstructionsGrid.GetColumnStyle(CusEntryInstruction.Schema.CEI_Procedure);
					var descriptionColumnstyle = control.EntryInstructionsGrid.GetColumnStyle(CusEntryInstruction.Schema.ProcedureDescription);
					CombineAssertions(() =>
					{
						AssertEquals("Procedure Should be available", false, procedureColumnstyle.IsUnavailable);
						AssertEquals("Procedure Should be visible", true, procedureColumnstyle.IsVisible);
						AssertEquals("Procedure Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90), procedureColumnstyle.Width);
						AssertEquals("Procedure CharacterCasing", CharacterCasing.Normal, procedureColumnstyle.CharacterCasing);
						AssertEquals("Procedure Requested Procedure", "Requested Procedure", procedureColumnstyle.GroupName.Caption);

						AssertEquals("Description Should be available", false, descriptionColumnstyle.IsUnavailable);
						AssertEquals("Description Should be visible", true, descriptionColumnstyle.IsVisible);
						AssertEquals("Description Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200), descriptionColumnstyle.Width);
						AssertEquals("Description CharacterCasing", CharacterCasing.Normal, descriptionColumnstyle.CharacterCasing);
						AssertEquals("Description Requested Procedure", "Requested Procedure", descriptionColumnstyle.GroupName.Caption);
					});
				}
			}
		}

		public void TestCEI_Procedure_NotSupport()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				declaration.CustomsEntryInstructions.AddNew();
				using (var control = new EntryInstructionGridUserControl())
				{
					control.SetDataBinding(declaration, "");
					var columnstyle = control.EntryInstructionsGrid.GetColumnStyle(CusEntryInstruction.Schema.CEI_Procedure);
					CombineAssertions(() =>
					{
						AssertEquals("Should not be available", true, columnstyle.IsUnavailable);
						AssertEquals("Should not be visible", false, columnstyle.IsVisible);
						AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90), columnstyle.Width);
						AssertEquals("CharacterCasing", CharacterCasing.Normal, columnstyle.CharacterCasing);
					});
				}
			}
		}

		public void TestEntryInstructionsGrid_RefreshWhenListCurrentChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();

			using var form = new ZForm(declaration);
			using var userControl = new EntryInstructionGridUserControl();
			form.Controls.Add(userControl);
			form.Show();

			userControl.EntryInstructionsGrid.Select(0);
			declaration.CustomsEntryInstructions.RemoveAndDelete(instruction1);
			AssertNoExceptionThrown("Should not throw exception when trying edit CurrentEntryInstruction", () =>
			{
				userControl.CurrentEntryInstruction.CEI_Style = "X";
			});
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
	}
}
