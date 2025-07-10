using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.GUI.Testing;
using Enterprise.DocumentEngine.SDF;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu.DocDataPlugIn.Testing
{
	sealed class DocDataUserControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestControl()
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
			Factory.Save();

			using (DocumentUDFPlugInTest.TestForm testForm = new DocumentUDFPlugInTest.TestForm(dummyConsol1))
			{
				testForm.MinimumSize = new System.Drawing.Size(800, 185);
				testForm.PlugIns.Add(ControllerIDs.DocDataPlugIn);

				testForm.Show();
				UserIdleWorker.Flush();

				DocDataUserControl testControl = (DocDataUserControl)testForm.PlugIns.Instances[0].UserControl;
				AssertEquals(2, testControl.MainTabControl.PlugIns.Instances.Length);
			}
		}
	}
}
