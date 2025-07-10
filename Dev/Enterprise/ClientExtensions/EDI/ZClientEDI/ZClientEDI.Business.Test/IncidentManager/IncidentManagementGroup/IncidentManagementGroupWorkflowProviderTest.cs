using System;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(IncidentManagementGroup))]
	public class IncidentManagementGroupWorkflowProviderTest : WorkflowProviderTest<IncidentManagementGroup, IncidentManagementGroupProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType => IncidentManagementGroupConstants.WorkflowDescriptorInformation.Code;

		public void TestGetTemplateFilterCriteria_ForING_Type()
		{
			var otherIncidentTypeCode = "OTH";
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var otherGroup = registryValue.AddNew(otherIncidentTypeCode, "Other Management group type");
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			BusinessObject.Factory.Save();
			AssertGetTemplateFilterCriteria<ZString>(BusinessObject.ING_TypeInfo, ProcessTaskTemplate.P0_SubType1Info,
				IncidentGroupStatusConfigurationConstants.MajorIncidentCode,
				otherIncidentTypeCode,
				ZString.Empty);
		}

		public void TestGetTemplateFilterCriteria_ForING_Product()
		{
			EDIDataRegistry.CreateProductsAndModulesForTest();
			BusinessObject.Factory.Save();
			AssertGetTemplateFilterCriteria<ZString>(BusinessObject.ING_ProductInfo, ProcessTaskTemplate.P0_SubType2Info,
				"AAA",
				ProductTypes.Codes.Enterprise,
				ZString.Empty);
		}

		public void TestGetTemplateFilterCriteria_ForING_ProductArea()
		{
			BusinessObject.Factory.Save();
			AssertGetTemplateFilterCriteria<ZString>(BusinessObject.ING_ProductAreaInfo, ProcessTaskTemplate.P0_SubType3Info,
				"AAA",
				IncidentConstants.ProgramArea.General,
				ZString.Empty);
		}
	}
}
