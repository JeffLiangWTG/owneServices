using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.GUI.Testing;
using Enterprise.DocumentEngine.SDF;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.SDF.Testing
{
	sealed class DocumentSDFControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestRows()
		{
			TestCaseHelper.ClearTable(StmSystemDefinedField.Schema.TableName);

			DummyConsolBusinessObject dummyConsol1 = Factory.New<DummyConsolBusinessObject>();
			dummyConsol1.Z0_Code = "DC1";
			StmSystemDefinedFieldCollection collection = new StmSystemDefinedFieldCollection(Factory);

			StmSystemDefinedField textField1 = collection.AddNew();
			textField1.S1_BusinessContext = nameof(BusinessContext.Consol);
			textField1.S1_Name = "TextField1";
			textField1.S1_Type = StmSystemDefinedFieldLookups.TypeCodes.Text;
			textField1.S1_Hint = "TextField1 Hint";

			StmSystemDefinedField dateOnlyField1 = collection.AddNew();
			dateOnlyField1.S1_BusinessContext = nameof(BusinessContext.Consol);
			dateOnlyField1.S1_Name = "DateOnlyField1";
			dateOnlyField1.S1_Type = StmSystemDefinedFieldLookups.TypeCodes.DateOnly;
			dateOnlyField1.S1_Hint = "DateOnlyField1 Hint";

			StmSystemDefinedField integerField1 = collection.AddNew();
			integerField1.S1_BusinessContext = nameof(BusinessContext.Consol);
			integerField1.S1_Name = "IntegerField1";
			integerField1.S1_Type = StmSystemDefinedFieldLookups.TypeCodes.Integer;
			integerField1.S1_Hint = "IntegerField1 Hint";

			StmSystemDefinedField dateTimeField1 = collection.AddNew();
			dateTimeField1.S1_BusinessContext = nameof(BusinessContext.Consol);
			dateTimeField1.S1_Name = "DateTimeField1";
			dateTimeField1.S1_Type = StmSystemDefinedFieldLookups.TypeCodes.DateTime;
			dateTimeField1.S1_Hint = "DateTimeField1 Hint";

			StmSystemDefinedField decimalField1 = collection.AddNew();
			decimalField1.S1_BusinessContext = nameof(BusinessContext.Consol);
			decimalField1.S1_Name = "DecimalField1";
			decimalField1.S1_Type = StmSystemDefinedFieldLookups.TypeCodes.Decimal;
			decimalField1.S1_Hint = "DecimalField1 Hint";

			StmSystemDefinedField booleanField1 = collection.AddNew();
			booleanField1.S1_BusinessContext = nameof(BusinessContext.Consol);
			booleanField1.S1_Name = "BooleanField1";
			booleanField1.S1_Type = StmSystemDefinedFieldLookups.TypeCodes.Boolean;
			booleanField1.S1_Hint = "BooleanField1 Hint";
			Factory.Save();

			using (DocumentUDFPlugInTest.TestForm testForm = new DocumentUDFPlugInTest.TestForm(dummyConsol1))
			{
				testForm.MinimumSize = new System.Drawing.Size(800, 185);
				testForm.PlugIns.Add(ControllerIDs.DocumentSDFPlugIn);

				testForm.Show();
				UserIdleWorker.Flush();

				DocumentSDFControl testControl = (DocumentSDFControl)testForm.PlugIns.Instances[0].UserControl;
				AssertEquals("Rows count", 6, testControl.FieldGrid.List.Count);
			}
		}
	}
}
