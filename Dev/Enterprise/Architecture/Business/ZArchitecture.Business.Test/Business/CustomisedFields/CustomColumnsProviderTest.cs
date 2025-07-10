using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class CustomColumnsProviderTest : TestCaseWithFactory
	{
		public void TestGetCustomColumnDefinitonsWhenNoColumns()
		{
			var processTaskTemplateType = ObjectFactory.GetType<IProcessTaskTemplate>();
			var processTaskTemplate1 = Factory.NewWithValidTestData(processTaskTemplateType, TestBusinessObjectKind.MinimumRequiredToSave);
			processTaskTemplate1[ProcessTaskTemplateSchema.P0_ProcessType] = "SAR";
			processTaskTemplate1[ProcessTaskTemplateSchema.P0_IsActive] = true;
			processTaskTemplate1[ProcessTaskTemplateSchema.P0_GC] = EnvProxy.Instance.CurrentCompany.PK;
			processTaskTemplate1[ProcessTaskTemplateSchema.P0_GB] = EnvProxy.Instance.CurrentBranch.PK;
			processTaskTemplate1[ProcessTaskTemplateSchema.P0_GE] = EnvProxy.Instance.CurrentDepartment.PK;
			Factory.Save();

			var result = new CustomColumnsProvider().GetCustomColumnDefinitions(
				Factory,
				"SAR",
				EnvProxy.Instance.CurrentCompany.PK,
				EnvProxy.Instance.CurrentBranch.PK,
				EnvProxy.Instance.CurrentDepartment.PK);

			AssertEquals(result.Count(), 0);
		}

		public void TestGetCustomColumnDefinitonsShouldReturnExpectedResult()
		{
			var processTaskTemplateType = ObjectFactory.GetType<IProcessTaskTemplate>();

			// Add Process Task Templates
			var processTaskTemplate1 = Factory.NewWithValidTestData(processTaskTemplateType, TestBusinessObjectKind.MinimumRequiredToSave);
			processTaskTemplate1[ProcessTaskTemplateSchema.P0_ProcessType] = "SAR";
			processTaskTemplate1[ProcessTaskTemplateSchema.P0_IsActive] = true;
			processTaskTemplate1[ProcessTaskTemplateSchema.P0_GC] = EnvProxy.Instance.CurrentCompany.PK;
			processTaskTemplate1[ProcessTaskTemplateSchema.P0_GB] = EnvProxy.Instance.CurrentBranch.PK;
			processTaskTemplate1[ProcessTaskTemplateSchema.P0_GE] = EnvProxy.Instance.CurrentDepartment.PK;

			var processTaskTemplate2 = Factory.NewWithValidTestData(processTaskTemplateType, TestBusinessObjectKind.MinimumRequiredToSave);
			processTaskTemplate2[ProcessTaskTemplateSchema.P0_ProcessType] = "SAR";
			processTaskTemplate2[ProcessTaskTemplateSchema.P0_IsActive] = true;
			processTaskTemplate2[ProcessTaskTemplateSchema.P0_GC] = EnvProxy.Instance.CurrentCompany.PK;
			processTaskTemplate2[ProcessTaskTemplateSchema.P0_GB] = EnvProxy.Instance.CurrentBranch.PK;
			processTaskTemplate2[ProcessTaskTemplateSchema.P0_GE] = EnvProxy.Instance.CurrentDepartment.PK;

			var processTaskTemplate3 = Factory.NewWithValidTestData(processTaskTemplateType, TestBusinessObjectKind.MinimumRequiredToSave);
			processTaskTemplate3[ProcessTaskTemplateSchema.P0_ProcessType] = "SAR";
			processTaskTemplate3[ProcessTaskTemplateSchema.P0_IsActive] = true;

			var processTaskTemplate4 = Factory.NewWithValidTestData(processTaskTemplateType, TestBusinessObjectKind.MinimumRequiredToSave);
			processTaskTemplate4[ProcessTaskTemplateSchema.P0_ProcessType] = "ATM";
			processTaskTemplate4[ProcessTaskTemplateSchema.P0_IsActive] = true;
			processTaskTemplate4[ProcessTaskTemplateSchema.P0_GC] = EnvProxy.Instance.CurrentCompany.PK;
			processTaskTemplate4[ProcessTaskTemplateSchema.P0_GB] = EnvProxy.Instance.CurrentBranch.PK;
			processTaskTemplate4[ProcessTaskTemplateSchema.P0_GE] = EnvProxy.Instance.CurrentDepartment.PK;

			// Add AddOnRules
			var addOnRule1 = Factory.New(Type.GetType("Enterprise.MasterFiles.Business.CustomValues.GenCustomAddOnRule, Enterprise.MasterFiles.Business"));
			addOnRule1[GenCustomAddOnRuleSchema.XR_Code] = "ONE";
			addOnRule1[GenCustomAddOnRuleSchema.XR_IsActive] = true;
			addOnRule1[GenCustomAddOnRuleSchema.XR_SourceCode] = @"<sourceCode>
  <rules>
    <rule code='InvalidCode' enabled='false'>
      <details>
        <codeDescriptionList />
      </details>
    </rule>
    <rule code='CreateEvent' enabled='true'>
      <details>
        <CreateEventRuleCode>Z45</CreateEventRuleCode>
        <CreateEventRuleReference>REFONE</CreateEventRuleReference>
      </details>
    </rule>
    <rule code='DateTimeFormat' enabled='false'>
      <details>
        <format>Short</format>
      </details>
    </rule>
    <rule code='CheckEntered' enabled='false'>
      <details />
    </rule>
  </rules>
</sourceCode>";

			// Add CustomColumnDefinitions
			var addNew1 = processTaskTemplate1["GenCustomColumnDefinitions"].GetType().GetMethod("AddNew");
			var field1 = (BusinessObject)addNew1.Invoke(processTaskTemplate1["GenCustomColumnDefinitions"], null);
			field1[GenCustomColumnDefinitionSchema.XC_Name] = "field 1 template 1";
			field1[GenCustomColumnDefinitionSchema.XC_Type] = "STR";
			field1[GenCustomColumnDefinitionSchema.XC_DisplaySequence] = 1;
			field1[GenCustomColumnDefinitionSchema.XC_XR] = addOnRule1.PK;
			var field2 = (BusinessObject)addNew1.Invoke(processTaskTemplate1["GenCustomColumnDefinitions"], null);
			field2[GenCustomColumnDefinitionSchema.XC_Name] = "field 2 template 1";
			field2[GenCustomColumnDefinitionSchema.XC_Type] = "STR";
			field2[GenCustomColumnDefinitionSchema.XC_DisplaySequence] = 2;
			field2[GenCustomColumnDefinitionSchema.XC_XR] = addOnRule1.PK;

			var addNew2 = processTaskTemplate2["GenCustomColumnDefinitions"].GetType().GetMethod("AddNew");
			var field3 = (BusinessObject)addNew2.Invoke(processTaskTemplate2["GenCustomColumnDefinitions"], null);
			field3[GenCustomColumnDefinitionSchema.XC_Name] = "field 3 template 2";
			field3[GenCustomColumnDefinitionSchema.XC_Type] = "STR";
			field3[GenCustomColumnDefinitionSchema.XC_DisplaySequence] = 2;
			field3[GenCustomColumnDefinitionSchema.XC_XR] = addOnRule1.PK;
			var field4 = (BusinessObject)addNew2.Invoke(processTaskTemplate2["GenCustomColumnDefinitions"], null);
			field4[GenCustomColumnDefinitionSchema.XC_Name] = "field 4 template 2";
			field4[GenCustomColumnDefinitionSchema.XC_Type] = "STR";
			field4[GenCustomColumnDefinitionSchema.XC_DisplaySequence] = 1;
			field4[GenCustomColumnDefinitionSchema.XC_XR] = addOnRule1.PK;

			Factory.Save();

			var result = new CustomColumnsProvider().GetCustomColumnDefinitions(
				Factory,
				"SAR",
				EnvProxy.Instance.CurrentCompany.PK,
				EnvProxy.Instance.CurrentBranch.PK,
				EnvProxy.Instance.CurrentDepartment.PK);

			AssertEquals(result.Count(), 4);

			var template1ColumnDefinitions = result.Where(x => x.XC_ParentID == processTaskTemplate1.PK).ToList();
			var template2ColumnDefinitions = result.Where(x => x.XC_ParentID == processTaskTemplate2.PK).ToList();

			AssertEquals(template1ColumnDefinitions[0].XC_Name, "field 1 template 1");
			AssertEquals(template1ColumnDefinitions[1].XC_Name, "field 2 template 1");

			AssertEquals(template2ColumnDefinitions[0].XC_Name, "field 4 template 2");
			AssertEquals(template2ColumnDefinitions[1].XC_Name, "field 3 template 2");
		}
	}
}
