using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(GlobalManifestBillsWorkflowDescriptor))]
	public class GlobalManifestBillsWorkflowDescriptorTest : WorkflowDescriptorTestCase<GlobalManifestBillsWorkflowDescriptor>
	{
		public override void TestID()
		{
			AssertEquals("Correct Code", "GMB", WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Description", "Global Manifest Bills", WorkflowDescriptor.Description);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsEventTracking);
		}

		public override void TestSupportsTasks()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsTasks);
		}

		public override void TestSupportsScreenLayout()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsScreenLayout);
		}

		public override void TestSubTypes()
		{
			AsycudaManifestHeaderLookupsTest.EnsureOrCreateRefZZRecordsToControlVisibilityBasedOnModeAndCountry(Factory, new ModeAndCountry(ZString.Empty, Core.Constants.CountryCodes.Eritrea));
			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_SubType1 = Core.Constants.CountryCodes.Eritrea;
			WorkflowDescriptor.LastProcessTaskTemplate = processTaskTemplate;

			AssertEquals("3 sub types", 3, WorkflowDescriptor.SubTypeInformation.Length);
			AssertEquals("Manifest Country", WorkflowDescriptor.SubTypeInformation[0].Description);
			AssertEquals("Manifest Type", WorkflowDescriptor.SubTypeInformation[1].Description);
			AssertEquals("Bill Shipment Type", WorkflowDescriptor.SubTypeInformation[2].Description);

			AssertNull(WorkflowDescriptor.SubTypeInformation[0].Collection);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[0].List);

			AssertNull(WorkflowDescriptor.SubTypeInformation[1].Collection);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[1].List);
			AssertContainsExactElementsInAnyOrder(new[] { "ATAIHR", "ATAITH", "CIKONC", "DEMIHR", "DEMITH", "DENIHR", "DENITH", "DIGIHR", "DIGITH", "EMANIF", "GRUPAJ", "HAVIHR", "HAVITH", "TESLIM", "TIRIHR", "TIRITH", "VARONC", "ASY", "IAM", "GVM", "PBN", "ICS", "S&S", "ETR", "MAN", "MGI", "MGE", "ALH", "ALM", "AQM", "BBB", "COH", "COM", "ECL", "FFM", "FWB", "HAB", "RFM", "RMA", "X1", "X2", "X3", "X6", "X7", "EH7", "ENS" }, WorkflowDescriptor.SubTypeInformation[1].List.OfType<ICodeDescription>().Select(x => x.Code));

			AssertNull(WorkflowDescriptor.SubTypeInformation[2].Collection);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[2].List);
			AssertContainsExactElementsInAnyOrder(new ShipmentTypeList().GetAllCodes(), WorkflowDescriptor.SubTypeInformation[2].List.OfType<ICodeDescription>().Select(x => x.Code));
		}

		public void TestMilestoneTemplateHintCaption()
		{
			AssertEquals("", WorkflowDescriptor.MilestoneTemplateHintCaption);
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest() => new IWorkflowProvider[] { Factory.NewWithValidTestData<AsycudaBill>() };
	}
}
