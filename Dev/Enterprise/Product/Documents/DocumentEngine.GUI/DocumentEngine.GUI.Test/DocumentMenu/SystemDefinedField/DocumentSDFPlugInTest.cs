using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.GUI.Testing;
using Enterprise.DocumentEngine.SDF;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Grid.Internal;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.SDF.Testing
{
	sealed class DocumentSDFPlugInTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestTextElementsAreLoadedProperly()
		{
			TestCaseHelper.ClearTable(StmSystemDefinedField.Schema.TableName);

			DummyConsolBusinessObject dummyConsol1 = Factory.New<DummyConsolBusinessObject>();
			dummyConsol1.DocumentSupporter.ReturnNullWrappers = true;
			dummyConsol1.Z0_Code = "DC1";

			AssertEquals("BusinessContext", BusinessContext.Consol, dummyConsol1.DocumentSupporter.BusinessContext);

			StmSystemDefinedFieldCollection sDFCollection = new StmSystemDefinedFieldCollection(Factory);
			StmSystemDefinedField textField1 = sDFCollection.AddNew();
			textField1.S1_BusinessContext = nameof(BusinessContext.Consol);
			textField1.S1_Name = "TextField1";
			textField1.S1_Type = StmSystemDefinedFieldLookups.TypeCodes.Text;
			textField1.S1_Hint = "TextField1 Hint";
			textField1.S1_Order = 1;

			StmSystemDefinedField dateOnlyField1 = sDFCollection.AddNew();
			dateOnlyField1.S1_BusinessContext = nameof(BusinessContext.Consol);
			dateOnlyField1.S1_Name = "DateOnlyField1";
			dateOnlyField1.S1_Type = StmSystemDefinedFieldLookups.TypeCodes.DateOnly;
			dateOnlyField1.S1_Hint = "DateOnlyField1 Hint";
			dateOnlyField1.S1_Order = 2;

			StmSystemDefinedField integerField1 = sDFCollection.AddNew();
			integerField1.S1_BusinessContext = nameof(BusinessContext.Consol);
			integerField1.S1_Name = "IntegerField1";
			integerField1.S1_Type = StmSystemDefinedFieldLookups.TypeCodes.Integer;
			integerField1.S1_Hint = "IntegerField1 Hint";
			integerField1.S1_Order = 3;

			StmSystemDefinedField dateTimeField1 = sDFCollection.AddNew();
			dateTimeField1.S1_BusinessContext = nameof(BusinessContext.Consol);
			dateTimeField1.S1_Name = "DateTimeField1";
			dateTimeField1.S1_Type = StmSystemDefinedFieldLookups.TypeCodes.DateTime;
			dateTimeField1.S1_Hint = "DateTimeField1 Hint";
			dateTimeField1.S1_Order = 4;

			StmSystemDefinedField decimalField1 = sDFCollection.AddNew();
			decimalField1.S1_BusinessContext = nameof(BusinessContext.Consol);
			decimalField1.S1_Name = "DecimalField1";
			decimalField1.S1_Type = StmSystemDefinedFieldLookups.TypeCodes.Decimal;
			decimalField1.S1_Hint = "DecimalField1 Hint";
			decimalField1.S1_Order = 5;

			StmSystemDefinedField booleanField1 = sDFCollection.AddNew();
			booleanField1.S1_BusinessContext = nameof(BusinessContext.Consol);
			booleanField1.S1_Name = "BooleanField1";
			booleanField1.S1_Type = StmSystemDefinedFieldLookups.TypeCodes.Boolean;
			booleanField1.S1_Hint = "BooleanField1 Hint";
			booleanField1.S1_Order = 6;

			Factory.Save();

			using (DocumentUDFPlugInTest.TestForm testForm = new DocumentUDFPlugInTest.TestForm(dummyConsol1))
			{
				testForm.MinimumSize = new System.Drawing.Size(800, 185);
				testForm.PlugIns.Add(ControllerIDs.DocumentSDFPlugIn);

				testForm.Show();
				UserIdleWorker.Flush();

				DocumentSDFControl testControl = (DocumentSDFControl)testForm.PlugIns.Instances[0].UserControl;
				AssertEquals("Rows count", 6, testControl.FieldGrid.List.Count);

				AssertEquals("DummyConsol.HasChanges", false, dummyConsol1.HasChanges);
				StmSystemDefinedFieldWrapperCollection collection = (StmSystemDefinedFieldWrapperCollection)testControl.FieldGrid.List;
				collection[0].S1_Value = "123.00";
				AssertEquals("DummyConsol.HasChanges", true, dummyConsol1.HasChanges);

				Factory.Save();
				AssertEquals("DummyConsol.HasChanges", false, dummyConsol1.HasChanges);

				DocumentSDFPlugIn sDFPlugIn = (DocumentSDFPlugIn)testForm.PlugIns.Instances[0];
				DocumentSDFControl sDFControl = (DocumentSDFControl)sDFPlugIn.UserControl;

				sDFControl.FieldGrid.CurrentCell = new DataGridCell(0, 3);

				ZMultiControlColumnStyle multiStyle = (ZMultiControlColumnStyle)sDFControl.FieldGrid.Columns[3].ColumnStyle;
				ZMultiCombinationControl comboControl = (ZMultiCombinationControl)multiStyle.EditControl;
				AssertEquals("Row 0 Cell 3 ControlType", FieldType.Text, comboControl.ControlType);

				comboControl.CurrentEditor.Text = "Changed";
				sDFControl.FieldGrid.CurrentCell = new DataGridCell(1, 3);
				AssertEquals("DummyConsol.HasChanges", true, dummyConsol1.HasChanges);

				Factory.Save();
				AssertEquals("DummyConsol.HasChanges", false, dummyConsol1.HasChanges);

				using (DocumentUDFPlugIn uDFPlugin = new DocumentUDFPlugIn(dummyConsol1))
				{
					uDFPlugin.OnUserControlShown();

					DocumentNote note = DocumentNote.LoadNote(dummyConsol1);
					AssertEquals("Note.HasChanges", false, note.HasChanges);

					sDFControl.FieldGrid.CurrentCell = new DataGridCell(0, 3);
					comboControl.CurrentEditor.Text = "ChangedAgain";
					sDFControl.FieldGrid.CurrentCell = new DataGridCell(1, 3);
					AssertEquals("Note.HasChanges", true, note.HasChanges);
					AssertEquals("DummyConsol.HasChanges", true, dummyConsol1.HasChanges);

					Factory.Save();
					AssertEquals("Note.HasChanges", false, note.HasChanges);
					AssertEquals("DummyConsol.HasChanges", false, dummyConsol1.HasChanges);
				}
			}
		}
	}
}
