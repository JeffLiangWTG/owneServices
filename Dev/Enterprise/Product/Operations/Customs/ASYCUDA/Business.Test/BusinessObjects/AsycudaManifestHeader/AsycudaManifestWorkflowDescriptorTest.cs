using System.Collections;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.BatchProcessor;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(AsycudaManifestWorkflowDescriptor))]
	sealed class AsycudaManifestWorkflowDescriptorTest : WorkflowDescriptorTestCase<AsycudaManifestWorkflowDescriptor>
	{
		public void TestGetSupportsValidateForCustomsMessagingTriggerActions()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "IAM";
			var trigger = header.WorkflowItems.Triggers.AddNew();
			AssertEquals(true, WorkflowDescriptor.GetSupportsValidateForCustomsMessagingTriggerActions(trigger, header).Any(x => x == WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging));

			header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			trigger = header.WorkflowItems.Triggers.AddNew();
			AssertEquals(false, WorkflowDescriptor.GetSupportsValidateForCustomsMessagingTriggerActions(trigger, header).Any(x => x == WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging));
		}

		public override void TestID()
		{
			AssertEquals("Correct Code", "AMW", AsycudaManifestWorkflowDescriptor.Constants.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", AsycudaManifestWorkflowDescriptor.Constants.Description, WorkflowDescriptor.Description);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresPort1);
			AssertEquals(true, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestSubTypes()
		{
			CombineAssertions(() =>
			{
				var template = Factory.New<ProcessTaskTemplate>();
				template.P0_ProcessType = WorkflowDescriptor.Code;
				WorkflowDescriptor.LastProcessTaskTemplate = template;
				AssertEquals("4 sub type", 4, WorkflowDescriptor.SubTypeInformation.Length);
				AssertEquals("Country for sub type", 1, WorkflowDescriptor.SubTypeInformation.Count(_ => _.Description == "Country/Region"));
				AssertEquals("Manifest type for sub type", 1, WorkflowDescriptor.SubTypeInformation.Count(_ => _.Description == "Manifest Type"));
				var dictManifestTypes = ApplicationBusinessProvider.GetApplicationBusinessProvidersDictionary(Factory).Values.Where(_ => !_.ManifestTypes.IsNullOrEmpty()).SelectMany(p => p.ManifestTypes.Select(m => m.Code)).Distinct().ToList();
				var dictTypeCountry = ApplicationBusinessProvider.GetApplicationBusinessProvidersDictionary(Factory).Keys.Where(_ => dictManifestTypes.Contains(_.ManifestTypeCode)).GroupBy(_ => _.ManifestTypeCode);
				var actualTypeList = template.Lookups.List3;
				AssertEquals("Manifest type list on template contains all valid values", true, dictTypeCountry.All(grouping => (actualTypeList as ICodeDescriptionPairList).ContainsCode(grouping.Key)));
				AssertEquals("Manifest type list on template contains right number of values", dictTypeCountry.Count(), actualTypeList.Count);
				foreach (var grouping in dictTypeCountry)
				{
					template.P0_SubType3 = grouping.Key;
					var expectedCountries = grouping.Select(_ => _.CountryOrGrouping).Distinct();
					if (grouping.Key == "ENS")
					{
						expectedCountries = expectedCountries.Except(new ZString[] { Core.Constants.CountryCodes.EuropeanUnion, Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes });
					}
					AssertContainsExactElementsInAnyOrder($"Country list for {grouping.Key}", expectedCountries, template.Lookups.List4.Cast<ICodeDescription>().Select(_ => _.Code));
				}
			});
		}

		public void TestSubTypeInformation_InvalidCountryCodeInGlobalCodes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "MAN", Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "ABC", "Testing", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptor.Code;
			WorkflowDescriptor.LastProcessTaskTemplate = template;
			AssertNoExceptionThrown(() =>
			{
				template.P0_SubType3 = "ASY";
			});
		}

		public void TestTriggerTypes_VCM_SGM()
		{
			CombineAssertions(() =>
			{
				using (ObjectFactory.Substitute("GlobalManifestApplicationBusinessProvider", ObjectFactory.Get<IEnumerable>("GlobalManifestApplicationBusinessProvider").Cast<ApplicationBusinessProvider>().Append(new DummyApplicationBusinessProvider("XYZ", Core.Constants.CountryCodes.UnitedStates)).ToList()))
				{
					var template = new BusinessObjectFactory().New<ProcessTaskTemplate>();
					template.P0_ProcessType = WorkflowDescriptor.Code;
					var trigger = template.CompletionStatementTasks.AddNew();
					template.P0_SubType3 = "XYZ";
					template.P0_SubType4 = Enterprise.Core.Constants.CountryCodes.UnitedStates;
					AssertEquals("Matching template can set VCM", true, WorkflowDescriptor.GetWorkflowTriggerActionTypes(trigger, template).ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging));
					AssertEquals("Matching template can set SGM", true, WorkflowDescriptor.GetWorkflowTriggerActionTypes(trigger, template).ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendGlobalManifest));

					var header = Factory.New<AsycudaManifestHeader>();
					using (header.GetCheckBusinessObjectTypeSuspender())
					{
						header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
						header.AMA_ManifestType = "XYZ";
						header.AMA_RN_NKCountry = Enterprise.Core.Constants.CountryCodes.UnitedStates;
					}
					AssertEquals("Matching carrier manifest header can set VCM", true, WorkflowDescriptor.GetWorkflowTriggerActionTypes(trigger, header).ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging));
					AssertEquals("Matching carrier manifest header can set SGM", true, WorkflowDescriptor.GetWorkflowTriggerActionTypes(trigger, header).ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendGlobalManifest));

					header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
					AssertEquals("Matching consoliator manifest header can set VCM", true, WorkflowDescriptor.GetWorkflowTriggerActionTypes(trigger, header).ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging));
					AssertEquals("Matching consoliator manifest header can set SGM", true, WorkflowDescriptor.GetWorkflowTriggerActionTypes(trigger, header).ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendGlobalManifest));

					trigger = header.WorkflowItems.Triggers.AddNew();
					var notification = trigger.ProcessTaskNotifications.AddNew();
					notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendGlobalManifest;
					var triggerActionProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(notification, null);
					AssertType<CustomsStmProcessQueueCreatorProcessor>("Matching manifest header has processor for SGM", triggerActionProcessor);
					triggerActionProcessor.Process(new LoggingInformation());
					var processQueueJob = Factory.LoadTop1<StmProcessQueue>(new ZQuery(StmProcessQueueSchema.SW_ReferenceID, header.PK));
					Factory.Save();
					AssertEquals("Process queue job links to Asycuda Manifest", AsycudaManifestHeaderSchema.Constants.Prefix, processQueueJob.SW_ReferenceTableCode);
					AssertEquals("Process queue job has SGM trigger action type", WorkflowTriggerActionTypeConstants.Codes.SendGlobalManifest, processQueueJob.SW_ActionCode);
					AssertEquals("Process queue job will be consumed by ASC service task", CustomsStmProcessQueueLoader.Constants.AutoSendCustomsMessaging, processQueueJob.SW_ApplicationCode);
					var testApplicationProvider = header.ApplicationBusinessProvider as DummyApplicationBusinessProvider;
					testApplicationProvider.BizoPKPassed = ZGuid.Empty;
					new AutoSendCustomsMessagingBatchProcessor(new LoggingInformation()).ExecuteBatch();
					AssertEquals("SGM processor should be linked to the same header", header.PK, testApplicationProvider.BizoPKPassed);
				}
			});
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest() => new[] { Factory.NewWithValidTestData<AsycudaManifestHeader>() };

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties => MessageRecipientPartyType.Email | MessageRecipientPartyType.OrgProxy;

		protected override CodeDescriptionPair[] ExpectedAdditionalWorkflowTriggerActionTypes
		{
			get
			{
				return new[]
				{
					new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentManifestXML, WorkflowTriggerActionTypeConstants.Descriptions.SendUniversalShipmentManifestXML),
					new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendUniversalManifestEventXML, WorkflowTriggerActionTypeConstants.Descriptions.SendUniversalManifestEventXML),
				};
			}
		}
	}
}
