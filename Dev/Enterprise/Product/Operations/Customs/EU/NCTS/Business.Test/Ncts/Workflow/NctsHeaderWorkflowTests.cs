using System;
using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.WorkflowDescriptor;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(ProcessTaskCollection<NctsHeaderProcessTask, NctsHeader>))]
	class NctsHeaderProcessTaskCollectionTest : ProcessTaskCollectionTest<ProcessTaskCollection<NctsHeaderProcessTask, NctsHeader>>
	{
		protected override ProcessTaskCollection<NctsHeaderProcessTask, NctsHeader> GetCollectionToTestCore()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			return header.WorkflowItems;
		}
	}

	[TestedType(typeof(NctsHeaderProcessTask))]
	class NctsHeaderProcessTaskTest : ProcessTaskTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return WorkflowItems.AddNew();
		}

		ProcessTaskCollection WorkflowItems
		{
			get { return ((IWorkflowProvider)Header).WorkflowItems; }
		}

		NctsHeader Header
		{
			get
			{
				if (header == null)
				{
					header = Factory.New<NctsHeader>();
					header.SetMovementType(NctsMovementType.Codes.Departure);
				}

				return header;
			}
		}
		NctsHeader header;
	}

	[TestedType(typeof(NctsHeader))]
	class NctsDepartureWorkflowProviderTest : WorkflowProviderTest<NctsHeader, ProcessTaskCollection<NctsHeaderProcessTask, NctsHeader>>
	{
		public void TestEverything()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			IWorkflowProvider departure = header;
			AssertType(typeof(NctsHeaderWorkflowInformationProvider), departure.GetWorkflowInformationProvider());
			AssertType(typeof(ProcessTaskCollection<NctsHeaderProcessTask, NctsHeader>), departure.WorkflowItems);
			AssertType(typeof(ColumnValueRanker), departure.GetTemplateSelectionCriteria());
		}

		[TestDate(1986, 3, 12, 4, 27, 0)]
		public void TestSetStatusFiresWorkflow()
		{
			var departure = Factory.New<NctsHeader>();
			departure.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			departure.SetMovementType(NctsMovementType.Codes.Departure);
			var triggerProcess = ((IWorkflowProvider)departure).WorkflowItems.Triggers.AddNew();
			triggerProcess.TriggerConditions.TriggerEventCode = Events.CustomsEntryStatusCode;
			triggerProcess.TriggerConditions.TriggerCondition = "REF";
			triggerProcess.TriggerConditions.TriggerConditionValue = NctsTransitStatusList.Codes.DeclarationMrnAllocated;
			Factory.Save();
			AssertEquals(ZDateTime.Empty, triggerProcess.P9_ActualDate);
			departure.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationRejected;
			Factory.Save();
			AssertEquals(ZDateTime.Empty, triggerProcess.P9_ActualDate);
			departure.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationMrnAllocated;
			Factory.Save();
			AssertEquals(new ZDateTime(1986, 3, 12, 4, 27, 0), triggerProcess.P9_ActualDate);
		}

		protected override ZString ExpectedWorkflowType
		{
			get { return WorkflowDescriptors.NctsHeaderWorkflowDescriptorCode; }
		}

		public void TestNctsHeaderFiresWorkflowOnShipment()
		{
			NctsHeaderFiresWorkflowOnParent<ForwardingShipment>();
		}

		public void TestNctsHeaderFiresWorkflowOnConsol()
		{
			NctsHeaderFiresWorkflowOnParent<ForwardingConsol>();
		}

		public void NctsHeaderFiresWorkflowOnParent<T>() where T : BusinessObject
		{
			BusinessObject parent = Factory.New<T>();
			var nctsheader = Factory.New<Integration.Customs.EU.NCTS.ICusInBondHeader>();
			nctsheader.BH_ParentID = parent.PK;
			nctsheader.BH_ParentTableCode = parent.TablePrefix;

			var trigger = ((IWorkflowProvider)parent).WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Trigger";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00.Code;
			AssertEquals("Trigger not fired", (short)100, trigger.P9_TriggerFiredCountdown);

			var nctsWorkflowProvider = (IWorkflowProvider)nctsheader;
			nctsWorkflowProvider.Logs.AddNew(Events.CustomisableEvent00, "Test reference 1 from NCTS");
			AssertEquals("Trigger fired once", (short)99, trigger.P9_TriggerFiredCountdown);

			nctsheader.BH_ParentID = ZGuid.Empty;
			nctsWorkflowProvider.Logs.AddNew(Events.CustomisableEvent00, "Test reference 2 from NCTS");
			AssertEquals("Trigger not fired when missing parent object", (short)99, trigger.P9_TriggerFiredCountdown);
		}
	}

	[TestedType(typeof(NctsHeaderWorkflowDescriptor))]
	public class NctsHeaderWorkflowDescriptorTest : WorkflowDescriptorTestCase<NctsHeaderWorkflowDescriptor>
	{
		public override void TestID()
		{
			AssertEquals("Correct Code", WorkflowDescriptors.NctsHeaderWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public void TestClientName()
		{
			AssertEquals("Client", WorkflowDescriptor.ClientName);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "NCTS - New Computerised Transit System (Europe)", WorkflowDescriptor.Description);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		public override void TestRequiresBranch()
		{
			Assert(true);
		}

		public override void TestRequiresClient()
		{
			Assert(true);
		}

		public override void TestRequiresDepartment()
		{
			Assert(true);
		}

		public override void TestRequiresPorts()
		{
			Assert(true);
		}

		public override void TestSubTypes()
		{
			AssertEquals("0 sub type", 0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		public void TestSupportsBufferManagement()
		{
			AssertEquals("NctsHeaderWorkflowDescriptor should support BufferManagement.", true, WorkflowDescriptor.SupportsBufferManagement);
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			return new[] { header };
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return MessageRecipientPartyType.Email | MessageRecipientPartyType.OrgProxy;
			}
		}

		protected override SchemaColumn[] ExpectedWorkflowTriggerFieldColumns
		{
			get
			{
				return new SchemaColumn[]
				{
					CusInBondMoveHeaderSchema.BM_CustomsStatus,
					CusInBondHeaderSchema.BH_MessageStatus,
					CusInBondMoveHeaderSchema.BM_MessageStatus,
				};
			}
		}

		public new void TestGetFieldColumnDescription() // because the base test is retarded
		{
			AssertEquals("NCTS Transit Status", WorkflowDescriptor.GetFieldColumnDescription(Factory, CusInBondMoveHeaderSchema.BM_CustomsStatus));
			AssertEquals("NCTS Message Status", WorkflowDescriptor.GetFieldColumnDescription(Factory, CusInBondHeaderSchema.BH_MessageStatus));
			AssertEquals("NCTS Message Status", WorkflowDescriptor.GetFieldColumnDescription(Factory, CusInBondMoveHeaderSchema.BM_MessageStatus));

			foreach (var fieldColumn in WorkflowDescriptor.GetWorkflowTriggerFieldColumns())
			{
				var description = WorkflowDescriptor.GetFieldColumnDescription(Factory, fieldColumn);
				AssertEquals("A description for field column '" + fieldColumn.Name + "' must be specified", false, description.Contains("_"));
			}
		}

		public void TestGetWorkflowTriggerAction()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			var trigger = nctsHeader.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Emu";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			var notification = trigger.ProcessTaskNotifications.AddNew();
			Assert(notification.Lookups.WorkflowTriggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs));
			Assert(notification.Lookups.WorkflowTriggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendDocument));

			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendNCTSMessage;

			var log = new ExampleLog(notification);
			var source = new WorkflowTriggerActionSource(nctsHeader, trigger, notification, log, null);
			AssertType(ExpectedWorkflowTriggerActionType(nctsHeader), WorkflowDescriptor.GetWorkflowTriggerAction(source, log));

			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendNCTSArrivalNotification;

			log = new ExampleLog(notification);
			source = new WorkflowTriggerActionSource(nctsHeader, trigger, notification, log, null);
			AssertType(ExpectedWorkflowTriggerActionTypeForArrivalNotification(nctsHeader), WorkflowDescriptor.GetWorkflowTriggerAction(source, log));
		}

		protected virtual Type ExpectedWorkflowTriggerActionType(NctsHeader nctsHeader) => typeof(LogAction);

		protected virtual Type ExpectedWorkflowTriggerActionTypeForArrivalNotification(NctsHeader nctsHeader) => typeof(LogAction);

		public void TestDocumentBusinessContext()
		{
			AssertContainsExactElementsInAnyOrder(new BusinessContext[] { BusinessContext.CusInBondHeader }, WorkflowDescriptor.DocumentBusinessContext);
		}

		protected override BusinessObject NewBusinessObjectInTable(ITableSchema table)
		{
			BusinessObject result = null;
			var header = Factory.NewWithValidTestData<NctsHeader>();
			header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			switch (table.TableName)
			{
				case CusInBondHeader.Schema.TableName:
					result = header;
					break;
				case CusInBondMoveHeader.Schema.TableName:
					result = header.MovementHeader;
					break;
				default:
					result = base.NewBusinessObjectInTable(table);
					break;
			}
			return result;
		}

		public void TestSANInTriggerActionList_InNCTSHeader()
		{
			AssertSANInTriggerActionListInNCTSHeader(NctsMovementType.Codes.Departure, false);
			AssertSANInTriggerActionListInNCTSHeader(NctsMovementType.Codes.Arrival, true);
			AssertSANInTriggerActionListInNCTSHeader(NctsMovementType.Codes.DepartureAndArrival, true);

			void AssertSANInTriggerActionListInNCTSHeader(ZString movementType, bool isSANCodeInList)
			{
				var nctsheader = Factory.NewWithValidTestData<NctsHeader>();
				nctsheader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
				nctsheader.SetMovementType(movementType);

				var processTask = nctsheader.WorkflowItems.Triggers.AddNew();
				processTask.P9_Description = "Start Work";
				processTask.ReferenceCode = "REF";
				var action = processTask.ProcessTaskNotifications.AddNew();

				var triggerActionCodeList = new CodeDescriptionPairList();
				triggerActionCodeList.AddRange(WorkflowDescriptor.GetWorkflowTriggerActionTypes(action.Parent, nctsheader));

				AssertEquals($"Movement type is {movementType} then SAN code visibility in Workflow Trigger action list should be {isSANCodeInList}.", isSANCodeInList, triggerActionCodeList.CodesAsString.Contains(WorkflowTriggerActionTypeConstants.Codes.SendNCTSArrivalNotification));
			}
		}

		public void TestSendingActionsInTriggerActionList_InWorkflowTemplate()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_ProcessType = "NCT";
			var processTask = template.WorkflowItems.Triggers.AddNew();
			processTask.P9_Description = "XXX";
			processTask.ReferenceCode = "REF";
			var action = processTask.ProcessTaskNotifications.AddNew();

			var triggerActionCodeList = new CodeDescriptionPairList();
			triggerActionCodeList.AddRange(WorkflowDescriptor.GetWorkflowTriggerActionTypes(action.Parent, template));

			var allCodes = triggerActionCodeList.GetAllCodes();
			CombineAssertions("SAN, SNM & VCM should be visible if the Workflow Trigger action list is from Workflow Template.",
				() =>
				{
					AssertCollectionContains(WorkflowTriggerActionTypeConstants.Codes.SendNCTSArrivalNotification, allCodes);
					AssertCollectionContains(WorkflowTriggerActionTypeConstants.Codes.SendNCTSMessage, allCodes);
					AssertCollectionContains(WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging, allCodes);
				});
		}

		public void TestSupportsValidateForCustomsMessagingTriggerAction()
		{
			AssertVCMInTriggerActionList(true, Core.Constants.CountryCodes.Germany);
			AssertVCMInTriggerActionList(true, Core.Constants.CountryCodes.France);
			AssertVCMInTriggerActionList(true, Core.Constants.CountryCodes.UnitedKingdom);
			AssertVCMInTriggerActionList(true, Core.Constants.CountryCodes.Ireland);
			AssertVCMInTriggerActionList(true, Core.Constants.CountryCodes.Netherlands);
			AssertVCMInTriggerActionList(true, Core.Constants.CountryCodes.Norway);
			AssertVCMInTriggerActionList(false, Core.Constants.CountryCodes.Belgium);
			AssertVCMInTriggerActionList(false, Core.Constants.CountryCodes.Switzerland);
			AssertVCMInTriggerActionList(false, Core.Constants.CountryCodes.Poland);

			void AssertVCMInTriggerActionList(bool expectVCM, string countryCode)
			{
				using (GlbCompany.TemporaryLoginInNewCompanyForCountry(countryCode))
				{
					var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
					nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
					nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

					var processTask = nctsHeader.WorkflowItems.Triggers.AddNew();
					processTask.P9_Description = "Start Work";
					processTask.ReferenceCode = "REF";
					var action = processTask.ProcessTaskNotifications.AddNew();

					var triggerActionCodeList = new CodeDescriptionPairList();
					triggerActionCodeList.AddRange(WorkflowDescriptor.GetWorkflowTriggerActionTypes(action.Parent, nctsHeader));

					var allCodes = triggerActionCodeList.GetAllCodes();
					if(expectVCM)
					{
						AssertCollectionContains(WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging, allCodes);
					}
					else
					{
						AssertCollectionNotContains(WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging, allCodes);
					}
				}
			}
		}

		public void TestMessagingTriggerActions()
		{
			AssertMessageActionsInTriggerActionList("Phase4 Supports SNM Action", Core.Constants.CountryCodes.France, Common.CusInBondApplicationCodeList.Codes.NCTS4, NctsMovementType.Codes.Departure, expectSNM: true, expectSAN: false);
			AssertMessageActionsInTriggerActionList("Phase4 Supports SNM Action", Core.Constants.CountryCodes.France, Common.CusInBondApplicationCodeList.Codes.NCTS4, NctsMovementType.Codes.Arrival, expectSNM: true, expectSAN: true);

			AssertMessageActionsInTriggerActionList("Phase5 Departure Declaration in Supported Country has SNM Action", Core.Constants.CountryCodes.Germany, Common.CusInBondApplicationCodeList.Codes.NCTS5, NctsMovementType.Codes.Arrival, expectSNM: false, expectSAN: true);
			AssertMessageActionsInTriggerActionList("Phase5 Arrival Declaration in Supported Country has SAN Action", Core.Constants.CountryCodes.Germany, Common.CusInBondApplicationCodeList.Codes.NCTS5, NctsMovementType.Codes.Departure, expectSNM: true, expectSAN: false);

			AssertMessageActionsInTriggerActionList("Phase5 Arrival Declaration in Unsupported Country has no Action", Core.Constants.CountryCodes.Latvia, Common.CusInBondApplicationCodeList.Codes.NCTS5, NctsMovementType.Codes.Arrival, expectSNM: false, expectSAN: false);
			AssertMessageActionsInTriggerActionList("Phase5 Departure Declaration in Unsupported Country has no Action", Core.Constants.CountryCodes.Latvia, Common.CusInBondApplicationCodeList.Codes.NCTS5, NctsMovementType.Codes.Departure, expectSNM: false, expectSAN: false);
		}

		void AssertMessageActionsInTriggerActionList(string assertionMessage, string countryCode, string applicationCode, string movementType, bool expectSNM, bool expectSAN)
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(countryCode))
			{
				var nctsheader = Factory.NewWithValidTestData<NctsHeader>();

				nctsheader.BH_ApplicationCode = applicationCode;
				nctsheader.SetMovementType(movementType);

				var processTask = nctsheader.WorkflowItems.Triggers.AddNew();
				processTask.P9_Description = "Start Work";
				processTask.ReferenceCode = "REF";
				var action = processTask.ProcessTaskNotifications.AddNew();

				var triggerActionCodeList = new CodeDescriptionPairList();
				triggerActionCodeList.AddRange(WorkflowDescriptor.GetWorkflowTriggerActionTypes(action.Parent, nctsheader));

				var allCodes = triggerActionCodeList.GetAllCodes();

				CombineAssertions(assertionMessage, () =>
				{
					if (expectSNM)
					{
						AssertCollectionContains($"SNM expected for {countryCode} {applicationCode} {movementType}", WorkflowTriggerActionTypeConstants.Codes.SendNCTSMessage, allCodes);
					}
					else
					{
						AssertCollectionNotContains($"SNM not expected for {countryCode} {applicationCode} {movementType}", WorkflowTriggerActionTypeConstants.Codes.SendNCTSMessage, allCodes);
					}

					if (expectSAN)
					{
						AssertCollectionContains($"SAN Expected for {countryCode} {applicationCode} {movementType}", WorkflowTriggerActionTypeConstants.Codes.SendNCTSArrivalNotification, allCodes);
					}
					else
					{
						AssertCollectionNotContains($"SAN Expected for {countryCode} {applicationCode} {movementType}", WorkflowTriggerActionTypeConstants.Codes.SendNCTSArrivalNotification, allCodes);
					}
				});
			}
		}

		protected override CodeDescriptionPair[] ExpectedAdditionalWorkflowTriggerActionTypes
		{
			get
			{
				var list = new List<CodeDescriptionPair>();
				list.AddRange(base.ExpectedAdditionalWorkflowTriggerActionTypes);
				list.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendNCTSMessage, WorkflowTriggerActionTypeConstants.Descriptions.SendNCTSMessage));
				return list.ToArray();
			}
		}
	}
}
