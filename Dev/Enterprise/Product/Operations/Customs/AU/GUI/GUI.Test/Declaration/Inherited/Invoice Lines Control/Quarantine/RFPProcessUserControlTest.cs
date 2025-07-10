using System;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class RFPProcessUserControlTest : TestCaseWithFactory
	{
		public void TestEstablishmentPostedStatus()
		{
			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			var declaration = helper.Declaration;
			var quarantineLine = helper.Line1.QuarantineExDocLine;
			var quarantineProcess = quarantineLine.Processes.AddNew();
			quarantineProcess.EE_AuthorisationEstablishmentID = "1234";
			quarantineProcess.EE_EstablishmentPostedStatus = NEXDOCEstablishmentPostedStatus.Codes.Lodged;
			helper.Header1.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;

			using (var form = new ZForm(declaration))
			using (var testControl = new RFPProcessUserControl())
			{
				form.Controls.Add(testControl);
				form.Show();
				testControl.SetProcessingAndTreatmentGridsVisibility(true);
				var processingGrid = testControl.FindSingle<ZGrid>("ProcessingGrid");
				var establishmentPostedStatusColumn = processingGrid.Columns[QuarantineExDocEstablishmentAndTime.Schema.EE_EstablishmentPostedStatusDescription];
				AssertEquals("Posted Status visible", true, establishmentPostedStatusColumn.IsVisible);
				AssertEquals("Posted Status caption", "Posted Status", establishmentPostedStatusColumn.ColumnStyle.HeaderText);

				var focusedQuarantineProcess = (QuarantineExDocEstablishmentAndTime)processingGrid.ListManager.GetCurrent();
				AssertEquals("Focused row readonly when Lodged", true, focusedQuarantineProcess.ReadOnly);

				var colourDecidingEventHandler = (EventHandler<ColourDecidingEventArgs>)typeof(ZGrid).GetField("ColourDeciding", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(processingGrid);
				var colourDecidingEventArgs = new ColourDecidingEventArgs(quarantineProcess);
				colourDecidingEventHandler(processingGrid, colourDecidingEventArgs);
				AssertNotEquals("Not DeletePending. Should not be highlighted", System.Drawing.Color.DarkGray, colourDecidingEventArgs.Colour);

				quarantineProcess.EE_EstablishmentPostedStatus = ZString.Empty;
				AssertEquals("Focused row editable when empty", false, focusedQuarantineProcess.ReadOnly);
				colourDecidingEventHandler(processingGrid, colourDecidingEventArgs);
				AssertNotEquals("Not DeletePending. Should not be highlighted", System.Drawing.Color.DarkGray, colourDecidingEventArgs.Colour);

				quarantineProcess.EE_EstablishmentPostedStatus = NEXDOCEstablishmentPostedStatus.Codes.DeletePending;
				AssertEquals("Focused row readonly when DeletePending", true, focusedQuarantineProcess.ReadOnly);
				colourDecidingEventHandler(processingGrid, colourDecidingEventArgs);
				AssertEquals("DeletePending. Should be highlighted", System.Drawing.Color.DarkGray, colourDecidingEventArgs.Colour);
			}
		}

		public void TestRemoveEntryMenuItem()
		{
			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			var declaration = helper.Declaration;
			var quarantineLine = helper.Line1.QuarantineExDocLine;
			var quarantineProcess = quarantineLine.Processes.AddNew();
			quarantineProcess.EE_AuthorisationEstablishmentID = "1234";
			quarantineProcess.EE_EstablishmentPostedStatus = NEXDOCEstablishmentPostedStatus.Codes.Lodged;
			helper.Header1.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;

			using (var form = new ZForm(declaration))
			using (var testControl = new RFPProcessUserControl())
			{
				form.Controls.Add(testControl);
				form.Show();
				testControl.SetProcessingAndTreatmentGridsVisibility(true);
				var processingGrid = testControl.FindSingle<ZGrid>("ProcessingGrid");
				processingGrid.SelectAllElements();

				processingGrid.SetCurrentHitTestForTest(0, 0);

				var contextMenu = processingGrid.ContextMenu;
				var contextMenuPopupEventHandler = (EventHandler)typeof(System.Windows.Forms.ContextMenu).GetField("onPopup", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(contextMenu);
				contextMenuPopupEventHandler.Invoke(processingGrid, new EventArgs());
				AssertEquals(true, testControl.removeEntryMenuItem.Enabled);
				AssertEquals(false, processingGrid.DeleteMenuItem.Enabled);

				testControl.removeEntryMenuItem.PerformClick();
				AssertEquals(NEXDOCEstablishmentPostedStatus.Codes.DeletePending, quarantineProcess.EE_EstablishmentPostedStatus);
				contextMenuPopupEventHandler.Invoke(processingGrid, new EventArgs());
				AssertEquals(false, testControl.removeEntryMenuItem.Enabled);
				AssertEquals(false, processingGrid.DeleteMenuItem.Enabled);

				quarantineProcess.EE_EstablishmentPostedStatus = ZString.Empty;
				contextMenuPopupEventHandler.Invoke(processingGrid, new EventArgs());
				AssertEquals(false, testControl.removeEntryMenuItem.Enabled);
				AssertEquals(true, processingGrid.DeleteMenuItem.Enabled);
			}
		}

		public void TestProcessingGrid_Errata54Columns()
		{
			var columns = new[]
			{
				QuarantineExDocEstablishmentAndTime.Schema.EE_TreatmentConcentration, QuarantineExDocEstablishmentAndTime.Schema.EE_TreatmentConcentrationUQ,
				QuarantineExDocEstablishmentAndTime.Schema.EE_TreatmentDuration, QuarantineExDocEstablishmentAndTime.Schema.EE_TreatmentDurationUQ,
				QuarantineExDocEstablishmentAndTime.Schema.EE_TreatmentTemperature, QuarantineExDocEstablishmentAndTime.Schema.EE_TreatmentTemperatureUQ,
			};

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EXDOCS_Errata54, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				CheckProcessingGridColumns(columns, true, "Errata 54 - true, EXDOC", false);
				CheckProcessingGridColumns(columns, false, "Errata 54 - true, NEXDOC", true);
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EXDOCS_Errata54, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
			{
				CheckProcessingGridColumns(columns, false, "Errata 54 - false, EXDOC", false);
			}
		}

		public void TestProcessingGrid_Errata54Columns_Grouping()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EXDOCS_Errata54, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				RunForControl(testControl =>
				{
					var processingGrid = testControl.FindSingle<ZGrid>("ProcessingGrid");
					var treatmentConcentrationColumn = processingGrid.Columns[QuarantineExDocEstablishmentAndTime.Schema.EE_TreatmentConcentration];
					var treatmentConcentrationUqColumn = processingGrid.Columns[QuarantineExDocEstablishmentAndTime.Schema.EE_TreatmentConcentrationUQ];
					var treatmentDurationColumn = processingGrid.Columns[QuarantineExDocEstablishmentAndTime.Schema.EE_TreatmentDuration];
					var treatmentDurationUqColumn = processingGrid.Columns[QuarantineExDocEstablishmentAndTime.Schema.EE_TreatmentDurationUQ];
					var treatmentTemperatureColumn = processingGrid.Columns[QuarantineExDocEstablishmentAndTime.Schema.EE_TreatmentTemperature];
					var treatmentTemperatureUqColumn = processingGrid.Columns[QuarantineExDocEstablishmentAndTime.Schema.EE_TreatmentTemperatureUQ];
					AssertEquals("Concentration columns group name", "Treatment Concentration", treatmentConcentrationColumn.GroupName.Caption);
					AssertEquals("Concentration columns in same group", true, treatmentConcentrationColumn.GroupName.Equals(treatmentConcentrationUqColumn.GroupName));
					AssertEquals("Duration columns group name", "Treatment Duration", treatmentDurationColumn.GroupName.Caption);
					AssertEquals("Duration columns in same group", true, treatmentDurationColumn.GroupName.Equals(treatmentDurationUqColumn.GroupName));
					AssertEquals("Temperature columns group name", "Treatment Temperature", treatmentTemperatureColumn.GroupName.Caption);
					AssertEquals("Temperature columns in same group", true, treatmentTemperatureColumn.GroupName.Equals(treatmentTemperatureUqColumn.GroupName));
				});
			}
		}

		void CheckProcessingGridColumns(string[] columns, bool columnsVisible, string message, bool isNextDocActive)
		{
			RunForControl(testControl =>
			{
				testControl.SetProcessingAndTreatmentGridsVisibility(isNextDocActive);
				var processingGrid = testControl.FindSingle<ZGrid>("ProcessingGrid");
				foreach (var column in columns)
				{
					var gridColumn = processingGrid.Columns[column];
					if (columnsVisible)
					{
						AssertNotNull($"{message}: Contains columns {column}", gridColumn);
						Assert($"{message}: Column {column} is visible", gridColumn.IsVisible);
					}
					else
					{
						AssertNull($"{message}: Contains columns {column}", gridColumn);
					}
				}
			});
		}

		void RunForControl(Action<RFPProcessUserControl> actionToRun)
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var testControl = new RFPProcessUserControl())
			{
				form.Controls.Add(testControl);
				form.Show();

				actionToRun.Invoke(testControl);
			}
		}
	}
}
