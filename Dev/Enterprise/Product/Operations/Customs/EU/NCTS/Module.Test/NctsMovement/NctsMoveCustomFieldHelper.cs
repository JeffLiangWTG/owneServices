using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Customs.EU.NCTS.Module.Testing
{
	public static class NctsMoveCustomFieldHelper
	{
		public static void CreateNCTSPhase5WorkflowWithCustomFields(BusinessObjectFactory factory)
		{
			ProcessTaskTemplate processTaskTemplate = factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = WorkflowDescriptors.NctsDepartureMovementHeaderWorkflowDescriptor;
			processTaskTemplate.P0_Name = WorkflowDescriptors.NctsDepartureMovementHeaderWorkflowDescriptor;
			processTaskTemplate.P0_IsActive = true;
			GenCustomColumnDefinition customField1 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField1.XC_Name = "Custom DepartureHeader string";
			customField1.XC_Type = AddOnColumnDataType.Codes.String;
			GenCustomColumnDefinition customField2 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField2.XC_Name = "custom int";
			customField2.XC_Type = AddOnColumnDataType.Codes.Integer;
			GenCustomColumnDefinition customField3 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField3.XC_Name = "custom decimal";
			customField3.XC_Type = AddOnColumnDataType.Codes.Decimal;
			GenCustomColumnDefinition customField4 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField4.XC_Name = "custom datetime";
			customField4.XC_Type = AddOnColumnDataType.Codes.Datetime;

			ProcessTaskTemplate processTaskTemplate2 = factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate2.P0_ProcessType = WorkflowDescriptors.NctsArrivalMovementHeaderWorkflowDescriptor;
			processTaskTemplate2.P0_Name = WorkflowDescriptors.NctsArrivalMovementHeaderWorkflowDescriptor;
			processTaskTemplate2.P0_IsActive = true;
			GenCustomColumnDefinition customField5 = processTaskTemplate2.GenCustomColumnDefinitions.AddNew();
			customField5.XC_Name = "Custom ArrivalHeader string";
			customField5.XC_Type = AddOnColumnDataType.Codes.String;

			ProcessTaskTemplate processTaskTemplate3 = factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate3.P0_ProcessType = WorkflowDescriptors.NctsHeaderWorkflowDescriptorCode;
			processTaskTemplate3.P0_Name = WorkflowDescriptors.NctsHeaderWorkflowDescriptorCode;
			GenCustomColumnDefinition customField = processTaskTemplate3.GenCustomColumnDefinitions.AddNew();
			customField.XC_Name = "Custom Header string";
			customField.XC_Type = AddOnColumnDataType.Codes.String;
			factory.Save();
			WorkflowCustomFieldsFilter.ClearCache();
		}
	}
}
