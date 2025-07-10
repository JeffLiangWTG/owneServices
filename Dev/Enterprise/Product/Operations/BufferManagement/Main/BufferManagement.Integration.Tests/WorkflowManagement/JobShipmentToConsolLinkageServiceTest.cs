using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Integration.Tests
{
	class JobShipmentToConsolLinkageService_ForwardingTest : JobShipmentToConsolLinkageServiceTest<ForwardingConsol, ForwardingShipment>
	{
	}

	[GuiTest]
	abstract class JobShipmentToConsolLinkageServiceTest<TConsol, TShipment> : BMSTestCaseWithFactory
		where TConsol : CommonConsol, IWorkflowProvider
		where TShipment : CommonShipment, IWorkflowProvider
	{
		#region No Template

		public void TestLinkConsolToShipment_WhenNoTemplateLinkDefined_ShouldNotLink()
		{
			var consol = FreightTestHelper.CreateConsol<TConsol>(Factory);
			var shipment = FreightTestHelper.CreateShipment<TShipment>(Factory, consol: consol);

			Factory.Save();

			AssertEquals(shipment, consol.Shipments.SingleOrDefault());

			var consolJobHeader = ProcessJobHeader.GetForParentWithoutCreation(consol, Factory);
			var shipmentJobHeader = ProcessJobHeader.GetForParentWithoutCreation(shipment, Factory);

			AssertNotNull(consolJobHeader);
			AssertNotNull(shipmentJobHeader);

			AssertIsNotPrerequisite(consolJobHeader, shipmentJobHeader);
			AssertIsNotPrerequisite(shipmentJobHeader, consolJobHeader);
		}

		public void TestLinkShipmentToConsol_WhenNoTemplateLinkDefined_ShouldNotLink()
		{
			var shipment = FreightTestHelper.CreateShipment<TShipment>(Factory);
			var consol = FreightTestHelper.CreateConsol<TConsol>(Factory, shipment: shipment);

			Factory.Save();

			AssertEquals(shipment, consol.Shipments.SingleOrDefault());

			var shipmentJobHeader = ProcessJobHeader.GetForParentWithoutCreation(shipment, Factory);
			var consolJobHeader = ProcessJobHeader.GetForParentWithoutCreation(consol, Factory);

			AssertNotNull(shipmentJobHeader);
			AssertNotNull(consolJobHeader);

			AssertIsNotPrerequisite(shipmentJobHeader, consolJobHeader);
			AssertIsNotPrerequisite(consolJobHeader, shipmentJobHeader);
		}

		#endregion

		#region Attaching Shipment to Consol

		public void TestLinkConsolToShipment_WhenTemplateLinkDefined_ShouldLink()
		{
			CreateDependencyLink(shipmentTemplate.GetJobHeader(), consolTemplate.GetJobHeader());

			var consol = FreightTestHelper.CreateConsol<TConsol>(Factory);
			var shipment = FreightTestHelper.CreateShipment<TShipment>(Factory, consol: consol);

			Factory.Save();

			AssertEquals(shipment, consol.Shipments.SingleOrDefault());

			var consolJobHeader = ProcessJobHeader.GetForParentWithoutCreation(consol, Factory);
			var shipmentJobHeader = ProcessJobHeader.GetForParentWithoutCreation(shipment, Factory);

			AssertIsPrerequisite(shipmentJobHeader, consolJobHeader);

			shipment.Consols.Remove(consol);

			AssertIsNotPrerequisite(shipmentJobHeader, consolJobHeader);
		}

		public void TestLinkShipmentToConsol_WhenTemplateLinkDefined_ShouldLink()
		{
			CreateDependencyLink(consolTemplate.GetJobHeader(), shipmentTemplate.GetJobHeader());

			var shipment = FreightTestHelper.CreateShipment<TShipment>(Factory);
			var consol = FreightTestHelper.CreateConsol<TConsol>(Factory, shipment: shipment);

			Factory.Save();

			AssertEquals(shipment, consol.Shipments.SingleOrDefault());

			var shipmentJobHeader = ProcessJobHeader.GetForParentWithoutCreation(shipment, Factory);
			var consolJobHeader = ProcessJobHeader.GetForParentWithoutCreation(consol, Factory);

			AssertIsPrerequisite(consolJobHeader, shipmentJobHeader);

			consol.Shipments.Remove(shipment);

			AssertIsNotPrerequisite(consolJobHeader, shipmentJobHeader);
		}

		#endregion

		#region Applying Templates on Job Save

		public void TestLinkConsolToShipment_ShouldLinkOnConsolSave_WayAfterCreatingJobLink_WhenTemplateBecomesApplicable_AsTheResultOfModifyingConsol()
		{
			consolTemplate.P0_LoadPortCountry = "ADALV"; // this makes the template specific (corresponds to JK_RL_NKLoadPort)
			CreateDependencyLink(shipmentTemplate.GetJobHeader(), consolTemplate.GetJobHeader());

			var consol = FreightTestHelper.CreateConsol<TConsol>(Factory);
			var shipment = FreightTestHelper.CreateShipment<TShipment>(Factory, consol: consol);

			Factory.Save();

			AssertEquals(shipment, consol.Shipments.SingleOrDefault());

			var consolJobHeader = ProcessJobHeader.GetForParentWithoutCreation(consol, Factory);
			var shipmentJobHeader = ProcessJobHeader.GetForParentWithoutCreation(shipment, Factory);

			AssertIsNotPrerequisite(shipmentJobHeader, consolJobHeader);

			consol.JK_RL_NKLoadPort = "ADALV"; // modify so that the specific template is applicable now (corresponds to P0_LoadPortCountry)

			Factory.Save();

			AssertIsPrerequisite(shipmentJobHeader, consolJobHeader);
		}

		public void TestLinkConsolToShipment_ShouldLinkOnConsolSave_WayAfterCreatingJobLink_WhenTemplateBecomesApplicable_AsTheResultOfModifyingShipment()
		{
			shipmentTemplate.P0_LoadPortCountry = "ADALV"; // this makes the template specific (corresponds to JS_RL_NKOrigin)
			CreateDependencyLink(shipmentTemplate.GetJobHeader(), consolTemplate.GetJobHeader());

			var consol = FreightTestHelper.CreateConsol<TConsol>(Factory);
			var shipment = FreightTestHelper.CreateShipment<TShipment>(Factory, consol: consol);

			Factory.Save();

			AssertEquals(shipment, consol.Shipments.SingleOrDefault());

			var consolJobHeader = ProcessJobHeader.GetForParentWithoutCreation(consol, Factory);
			var shipmentJobHeader = ProcessJobHeader.GetForParentWithoutCreation(shipment, Factory);

			AssertIsNotPrerequisite(shipmentJobHeader, consolJobHeader);

			shipment.JS_RL_NKOrigin = "ADALV"; // modify so that the specific template is applicable now (corresponds to P0_LoadPortCountry)

			Factory.Save();

			AssertIsPrerequisite(shipmentJobHeader, consolJobHeader);
		}

		#endregion

		#region JobConShipLink Foreign Keys

		public void TestLinkConsolToShipment_WhenTemplateLinkDefined_ConsolFKUnSet_ShouldUnLink()
		{
			CreateDependencyLink(shipmentTemplate.GetJobHeader(), consolTemplate.GetJobHeader());

			var consol = FreightTestHelper.CreateConsol<TConsol>(Factory);
			var shipment = FreightTestHelper.CreateShipment<TShipment>(Factory, consol: consol);

			Factory.Save();

			AssertEquals(shipment, consol.Shipments.SingleOrDefault());

			var consolJobHeader = ProcessJobHeader.GetForParentWithoutCreation(consol, Factory);
			var shipmentJobHeader = ProcessJobHeader.GetForParentWithoutCreation(shipment, Factory);

			AssertIsPrerequisite(shipmentJobHeader, consolJobHeader);

			var pivot = FreightTestHelper.GetJobConShipLink(consol, shipment);
			pivot.JN_JK = ZGuid.Empty;

			AssertIsNotPrerequisite(shipmentJobHeader, consolJobHeader);
		}

		public void TestLinkShipmentToConsol_WhenTemplateLinkDefined_ConsolFKUnSet_ShouldUnLink()
		{
			CreateDependencyLink(consolTemplate.GetJobHeader(), shipmentTemplate.GetJobHeader());

			var shipment = FreightTestHelper.CreateShipment<TShipment>(Factory);
			var consol = FreightTestHelper.CreateConsol<TConsol>(Factory, shipment: shipment);

			Factory.Save();

			AssertEquals(shipment, consol.Shipments.SingleOrDefault());

			var shipmentJobHeader = ProcessJobHeader.GetForParentWithoutCreation(shipment, Factory);
			var consolJobHeader = ProcessJobHeader.GetForParentWithoutCreation(consol, Factory);

			AssertIsPrerequisite(consolJobHeader, shipmentJobHeader);

			var pivot = FreightTestHelper.GetJobConShipLink(consol, shipment);
			pivot.JN_JK = ZGuid.Empty;

			AssertIsNotPrerequisite(consolJobHeader, shipmentJobHeader);
		}

		public void TestLinkConsolToShipment_WhenTemplateLinkDefined_ShipmentFKUnSet_ShouldUnLink()
		{
			CreateDependencyLink(shipmentTemplate.GetJobHeader(), consolTemplate.GetJobHeader());

			var consol = FreightTestHelper.CreateConsol<TConsol>(Factory);
			var shipment = FreightTestHelper.CreateShipment<TShipment>(Factory, consol: consol);

			Factory.Save();

			AssertEquals(shipment, consol.Shipments.SingleOrDefault());

			var consolJobHeader = ProcessJobHeader.GetForParentWithoutCreation(consol, Factory);
			var shipmentJobHeader = ProcessJobHeader.GetForParentWithoutCreation(shipment, Factory);

			AssertIsPrerequisite(shipmentJobHeader, consolJobHeader);

			var pivot = FreightTestHelper.GetJobConShipLink(consol, shipment);
			pivot.JN_JS = ZGuid.Empty;

			AssertIsNotPrerequisite(shipmentJobHeader, consolJobHeader);
		}

		public void TestLinkShipmentToConsol_WhenTemplateLinkDefined_ShipmentFKUnSet_ShouldUnLink()
		{
			CreateDependencyLink(consolTemplate.GetJobHeader(), shipmentTemplate.GetJobHeader());

			var shipment = FreightTestHelper.CreateShipment<TShipment>(Factory);
			var consol = FreightTestHelper.CreateConsol<TConsol>(Factory, shipment: shipment);

			Factory.Save();

			AssertEquals(shipment, consol.Shipments.SingleOrDefault());

			var shipmentJobHeader = ProcessJobHeader.GetForParentWithoutCreation(shipment, Factory);
			var consolJobHeader = ProcessJobHeader.GetForParentWithoutCreation(consol, Factory);

			AssertIsPrerequisite(consolJobHeader, shipmentJobHeader);

			var pivot = FreightTestHelper.GetJobConShipLink(consol, shipment);
			pivot.JN_JS = ZGuid.Empty;

			AssertIsNotPrerequisite(consolJobHeader, shipmentJobHeader);
		}

		#endregion

		#region Implementation

		ProcessTaskTemplate shipmentTemplate;
		ProcessTaskTemplate consolTemplate;

		BusinessObjectFactory templateFactory;

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);
			BMSTestHelper.AddWorkflowType(config.System, WorkflowDescriptors.JobConsolWorkflowDescriptorCode);

			Factory.Save();

			templateFactory = Factory.CreateNewFactory();

			shipmentTemplate = BMSTestHelper.CreateWorkflowTemplate(templateFactory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode, Constants.TransportModes.Air);
			BMSTestHelper.CreateWorkflow(shipmentTemplate, "shipmentWorkflow1");
			BMSTestHelper.CreateWorkflow(shipmentTemplate, "shipmentWorkflow2");

			consolTemplate = BMSTestHelper.CreateWorkflowTemplate(templateFactory, WorkflowDescriptors.JobConsolWorkflowDescriptorCode, Constants.TransportModes.Air);
			BMSTestHelper.CreateWorkflow(consolTemplate, "consolWorkflow1");
			BMSTestHelper.CreateWorkflow(consolTemplate, "consolWorkflow2");

			templateFactory.Save();
		}

		void CreateDependencyLink(ProcessHeader fromWorkflow, ProcessHeader toWorkflow)
		{
			var loadedFromWorkflow = templateFactory.Load<ProcessHeader>(fromWorkflow.PK);
			var loadedToWorkflow = templateFactory.Load<ProcessHeader>(toWorkflow.PK);
			var loadedTemplate = templateFactory.Load<ProcessTaskTemplate>(shipmentTemplate.PK);

			BMSTestHelper.CreateDependencyLink(loadedTemplate, loadedFromWorkflow, loadedToWorkflow);

			templateFactory.Save();
		}

		#endregion
	}
}
