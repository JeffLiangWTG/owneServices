using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Business.Workflow.ValidationAction.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	[TestedType(typeof(EUH7AsycudaManifestHeaderWorkflowDescriptor))]
	sealed class EUH7AsycudaManifestHeaderWorkflowDescriptorTest : WorkflowDescriptorTestCase<EUH7AsycudaManifestHeaderWorkflowDescriptor>
	{
		public override void TestDescription() => AssertEquals("Low Value (H7)", WorkflowDescriptor.Description);

		public override void TestID() => AssertEquals("ELV", WorkflowDescriptor.Code);

		public override void TestRequiresBranch() => AssertEquals(false, WorkflowDescriptor.RequiresBranch);

		public override void TestRequiresClient() => AssertEquals(false, WorkflowDescriptor.RequiresClient);

		public override void TestRequiresDepartment() => AssertEquals(false, WorkflowDescriptor.RequiresDepartment);

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestSubTypes()
		{
			AssertEquals("The length of the Sub Type should be 1", 1, WorkflowDescriptor.SubTypeInformation.Length);

			AssertEquals("Description should be Country Code", "Country Code", WorkflowDescriptor.SubTypeInformation[0].Description);

			var countryCodeList = WorkflowDescriptor.SubTypeInformation[0].List;
			Assert("The country code list includes UnitedKingdom.", countryCodeList.ContainsCode(Core.Constants.CountryCodes.UnitedKingdom));
		}

		public override void TestWorkflowProviderType()
		{
			var factory = new BusinessObjectFactory();
			var provider = (IWorkflowProvider)factory.New<AsycudaManifestHeader>();
			Assert(WorkflowDescriptor.WorkflowProviderType.IsAssignableFrom(provider.GetType()));
			AssertEquals(WorkflowDescriptor.Code, provider.WorkflowType);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		public void TestGetWorkflowTriggerActionCore()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var processTask = header.WorkflowItems.Triggers.AddNew();
			var action = processTask.ProcessTaskNotifications.AddNew();
			var descriptor = new EUH7AsycudaManifestHeaderWorkflowDescriptor();

			CombineAssertions(() =>
			{
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendCustomsDeclaration;
				var processor = descriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));

				AssertType<EUH7SendCustomsDeclarationMessageProcessor>(processor);

				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendG3CustomsDeclaration;
				processor = descriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));

				AssertNull(processor);
			});
		}

		protected override CodeDescriptionPair[] ExpectedAdditionalWorkflowTriggerActionTypes
		{
			get
			{
				return base.ExpectedAdditionalWorkflowTriggerActionTypes.Concat(new[]
				{
					new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendCustomsDeclaration, WorkflowTriggerActionTypeConstants.Descriptions.SendCustomsDeclaration),
				}).ToArray();
			}
		}

		public void TestGetWorkflowTriggerActionTypesWhenParentIsProcessTaskTemplate()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_SubType1 = Core.Constants.CountryCodes.Ireland;

			var trigger = template.WorkflowItems.Triggers.AddNew();
			var workflowDescriptor = new EUH7AsycudaManifestHeaderWorkflowDescriptor();
			var triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, template);

			CombineAssertions(() =>
			{
				Assert("When country code is Ireland, triggerActionTypes not contains SC3.", !triggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendG3CustomsDeclaration));

				template.P0_SubType1 = Core.Constants.CountryCodes.Spain;
				triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, template);
				Assert("When country code is Spain, triggerActionTypes contains SC3.", triggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendG3CustomsDeclaration));
			});
		}

		public void TestGetWorkflowTriggerActionTypesWhenParentIsAsycudaManifestHeader_NotES()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var trigger = header.WorkflowItems.Triggers.AddNew();
			var workflowDescriptor = new EUH7AsycudaManifestHeaderWorkflowDescriptor();
			var triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, header);

			Assert("When SupportsSendG3CustomsDeclaration is false, triggerActionTypes not contains SC3.", !triggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendG3CustomsDeclaration));
		}

		public void TestGetWorkflowTriggerActionTypesWhenParentIsAsycudaManifestHeader_ES()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ESH7.IAsycudaManifestHeader>();
			var trigger = header.WorkflowItems.Triggers.AddNew();
			var workflowDescriptor = new EUH7AsycudaManifestHeaderWorkflowDescriptor();
			var triggerActionTypes = workflowDescriptor.GetWorkflowTriggerActionTypes(trigger, header);

			Assert("When SupportsSendG3CustomsDeclaration is true, triggerActionTypes contains SC3.", triggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendG3CustomsDeclaration));
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest() => new IWorkflowProvider[] { AsycudaManifestHeaderWithOrganisations };

		AsycudaManifestHeader AsycudaManifestHeaderWithOrganisations => outerPackageWithOrganisations ?? (outerPackageWithOrganisations = CreateOuterPackageWithOrganisations());
		AsycudaManifestHeader outerPackageWithOrganisations;

		AsycudaManifestHeader CreateOuterPackageWithOrganisations()
		{
			return Factory.New<AsycudaManifestHeader>();
		}
	}

	class H7AsycudaManifestHeaderMessageValidationSupporterTest : CustomsMessageValidationSupporterTest<AsycudaManifestHeader>
	{
		protected override AsycudaManifestHeader GetValidateForCustomsMessagingSupporter()
		{
			return Factory.New<AsycudaManifestHeader>();
		}

		protected override IEnumerable<string> ExpectedTriggerTypes
		{
			get { yield return WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging; }
		}
	}
}
