using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class CustomFieldHelperTest : TestCaseWithFactory
	{
		public void TestGetCustomFields()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			template.P0_Name = "Template";
			var customField1 = template.GenCustomColumnDefinitions.AddNew();
			customField1.XC_Name = "customFieldBoolean";
			customField1.XC_Type = AddOnColumnDataType.Codes.Boolean;
			var customField2 = template.GenCustomColumnDefinitions.AddNew();
			customField2.XC_Name = "customFieldDatetime";
			customField2.XC_Type = AddOnColumnDataType.Codes.Datetime;
			var customField3 = template.GenCustomColumnDefinitions.AddNew();
			customField3.XC_Name = "customFieldDecimal";
			customField3.XC_Type = AddOnColumnDataType.Codes.Decimal;
			var customField4 = template.GenCustomColumnDefinitions.AddNew();
			customField4.XC_Name = "customFieldInteger";
			customField4.XC_Type = AddOnColumnDataType.Codes.Integer;
			var customField5 = template.GenCustomColumnDefinitions.AddNew();
			customField5.XC_Name = "customFieldString";
			customField5.XC_Type = AddOnColumnDataType.Codes.String;
			Factory.Save();
			var properties = CustomFieldHelper.GetCustomFields(DummyWorkflowDescriptor.Instance.Code);
			AssertContainsExactElementsInAnyOrder(new string[] { "__CUSTOMFIELDBOOLEAN__prop__ZBool", "__CUSTOMFIELDDATETIME__prop__ZDateTime", "__CUSTOMFIELDDECIMAL__prop__ZDecimal", "__CUSTOMFIELDINTEGER__prop__ZInt", "__CUSTOMFIELDSTRING__prop__ZString" }, properties.Select(p => p.Identifier));
		}
	}
}
