using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting
{
	class AddOnRuleTest : TestCaseWithFactory
	{
		public void TestImportAddOnRules()
		{
			using (var stream = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.ConcreteUnitTesting.Testing.Workflow.Rule_ADD_Z00_HELLO_MANDATORY.xml")))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream.BaseStream);
				var insertLog = manager.GetLogs();
				AssertMultilineASCIIEquals("Insert Log Text", @"
--- Start Import Process --------------------------------------------------------------
Processed: AddOnRule
--- Import Process Finished -----------------------------------------------------------
GenCustomAddOnRule - 1 inserts, 0 updates, 0 deletes
				".Trim(), insertLog);
			}

			var query1 = new ZQuery(GenCustomAddOnRuleSchema.XR_Code, "ADD_Z00_HELLO_MANDATORY");
			var loadedRule1 = Factory.Load<GenCustomAddOnRule>(query1).Single();
			CombineAssertions("loadedRule1 Contents", () =>
			{
				AssertEquals(".XR_Code", "ADD_Z00_HELLO_MANDATORY", loadedRule1.XR_Code);
				AssertEquals(".XR_IsActive", true, loadedRule1.XR_IsActive);
				AssertMultilineASCIIEquals(".GetRules()", @"
CheckEntered - Mandatory - Yes
CreateEvent - Create Event on Edit - Yes
DateTimeFormat - Date time format - No
InvalidCode - Code and description list - No
".Trim(), string.Join("\r\n", loadedRule1.GetRules().OrderBy(o => o.Code).Select(o => $"{o.Code} - {o.Name} - {o.IsEnabled.ToYesNoString()}")));
			});

			loadedRule1.XR_Description = "ANYTHING ELSE"; // Will forces an update if we import the same XML again
			Factory.Save();

			using (var stream = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.ConcreteUnitTesting.Testing.Workflow.Rule_ADD_Z00_HELLO_MANDATORY.xml")))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream.BaseStream);
				var insertLog = manager.GetLogs();
				AssertMultilineASCIIEquals("Update Log - Same Code", @"
--- Start Import Process --------------------------------------------------------------
Processed: AddOnRule
--- Import Process Finished -----------------------------------------------------------
GenCustomAddOnRule - 0 inserts, 1 updates, 0 deletes
				".Trim(), insertLog);
			}

			using (var stream = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.ConcreteUnitTesting.Testing.Workflow.Rule_YES_NO_MAYBE.xml")))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream.BaseStream);
				var insertLog = manager.GetLogs();
				AssertMultilineASCIIEquals("Second Insert Log - Different Code", @"
--- Start Import Process --------------------------------------------------------------
Processed: AddOnRule
--- Import Process Finished -----------------------------------------------------------
GenCustomAddOnRule - 1 inserts, 0 updates, 0 deletes
				".Trim(), insertLog);
			}

			var query2 = new ZQuery(GenCustomAddOnRuleSchema.XR_Code, "YES_NO_MAYBE");
			var loadedRule2 = Factory.Load<GenCustomAddOnRule>(query2).Single();
			CombineAssertions("loadedRule2 Contents", () =>
			{
				AssertEquals(".XR_Code", "YES_NO_MAYBE", loadedRule2.XR_Code);
				AssertEquals(".XR_IsActive", true, loadedRule2.XR_IsActive);
				AssertMultilineASCIIEquals(".GetRules()", @"
CheckEntered - Mandatory - No
CreateEvent - Create Event on Edit - No
DateTimeFormat - Date time format - No
InvalidCode - Code and description list - Yes
".Trim(), string.Join("\r\n", loadedRule2.GetRules().OrderBy(o => o.Code).Select(o => $"{o.Code} - {o.Name} - {o.IsEnabled.ToYesNoString()}")));
			});
		}

		public void TestExportAddOnRules()
		{
			const string emptySourceCodeXML = @"<sourceCode>
  <rules>
    <rule code=""InvalidCode"" enabled=""false"">
      <details>
        <codeDescriptionList>
        </codeDescriptionList>
      </details>
    </rule>
    <rule code=""CreateEvent"" enabled=""false"">
      <details>
        <CreateEventRuleCode>Z00</CreateEventRuleCode>
      </details>
    </rule>
    <rule code=""DateTimeFormat"" enabled=""false"">
      <details>
        <format>Short</format>
      </details>
    </rule>
    <rule code=""CheckEntered"" enabled=""false"">
      <details />
    </rule>
  </rules>
</sourceCode>";

			var rule = Factory.New<GenCustomAddOnRule>();
			rule.XR_Code = "BOOYAH!";
			rule.XR_Description = "MULLET HARTS";
			rule.XR_SourceCode = emptySourceCodeXML;

			Factory.Save();

			using (var baseStream = NativeDataTransferTestHelper.ExportToStream(rule))
			{
				rule.Delete();
				Factory.Save();

				var insertLog = NativeDataTransferTestHelper.ImportAndGetInsertLog(baseStream);
				AssertMultilineASCIIEquals("Insert after deleting Log Text", @"
--- Start Import Process --------------------------------------------------------------
Processed: AddOnRule
--- Import Process Finished -----------------------------------------------------------
GenCustomAddOnRule - 1 inserts, 0 updates, 0 deletes
				".Trim(), insertLog);

				var query = new ZQuery();
				query.AddToFilter(GenCustomAddOnRuleSchema.XR_Code, "BOOYAH!");

				var reloadedRule = Factory.Load<GenCustomAddOnRule>(query).Single();

				CombineAssertions("reloadedRule Contents", () =>
				{
					AssertEquals(".XR_Code", "BOOYAH!", reloadedRule.XR_Code);
					AssertEquals(".XR_Description", "MULLET HARTS", reloadedRule.XR_Description);
					AssertEquals(".XR_IsActive", true, reloadedRule.XR_IsActive);
					AssertMultilineASCIIEquals(".GetRules()", @"
CheckEntered - Mandatory - No
CreateEvent - Create Event on Edit - No
DateTimeFormat - Date time format - No
InvalidCode - Code and description list - No
".Trim(), string.Join("\r\n", reloadedRule.GetRules().OrderBy(o => o.Code).Select(o => $"{o.Code} - {o.Name} - {o.IsEnabled.ToYesNoString()}")));
				});
			}
		}
	}
}
