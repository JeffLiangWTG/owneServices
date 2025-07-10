using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(IncidentTriage))]
	public class IncidentTriageWorkflowProviderTest : WorkflowProviderTest<IncidentTriage, ProcessTaskCollection<IncidentTriageProcessTask, IncidentTriage>>
	{
		protected override ZString ExpectedWorkflowType => IncidentTriageConstants.WorkflowDescriptorInformation.Code;

		public void TestGetTemplateFilterCriteria_ForIMT_Product()
		{
			EDIDataRegistry.CreateProductsAndModulesForTest();
			BusinessObject.Factory.Save();
			AssertGetTemplateFilterCriteria<ZString>(BusinessObject.IMT_ProductInfo, ProcessTaskTemplate.P0_SubType1Info,
				"AAA",
				ProductTypes.Codes.Enterprise,
				ZString.Empty);
		}

		public void TestGetTemplateFilterCriteria_ForIMT_ProductArea()
		{
			BusinessObject.Factory.Save();
			AssertGetTemplateFilterCriteria<ZString>(BusinessObject.IMT_ProductAreaInfo, ProcessTaskTemplate.P0_SubType2Info,
				"AAA",
				IncidentConstants.ProgramArea.General,
				ZString.Empty);
		}

		public void TestGetTemplateFilterCriteria_ForIMT_Module()
		{
			BusinessObject.Factory.Save();
			AssertGetTemplateFilterCriteria<ZString>(BusinessObject.IMT_ModuleInfo, ProcessTaskTemplate.P0_SubType3Info,
				"AAA",
				"ALL",
				ZString.Empty);
		}
	}
}
