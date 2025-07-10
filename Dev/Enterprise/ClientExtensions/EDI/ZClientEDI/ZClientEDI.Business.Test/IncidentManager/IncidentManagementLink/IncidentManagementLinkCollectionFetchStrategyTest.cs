using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class IncidentManagementLinkCollectionFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForViewShouldReloadClients()
		{
			var client1 = Factory.NewWithValidTestData<OrgHeader>();
			var client2 = Factory.NewWithValidTestData<OrgHeader>();
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			var incident3 = Factory.NewWithValidTestData<SupportIncident>();
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();

			incident1.IM_OH_Client = client1.PK;
			incident2.IM_OH_Client = client2.PK;
			incident3.IM_OH_Client = client1.PK;
			var link1 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;
			var link2 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link2.INL_IM_Incident = incident2.PK;
			link2.INL_ING_Group = group.PK;
			var link3 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link3.INL_IM_Incident = incident3.PK;
			link3.INL_ING_Group = group.PK;

			Factory.Save();

			var factoryForChangingProperties = new BusinessObjectFactory { RefreshEnabled = false };
			var factoryForEmptyFetch = new BusinessObjectFactory();
			var link1ReloadedInChangeFactory = factoryForChangingProperties.Load<IncidentManagementLink>(link1.PK);
			_ = link1ReloadedInChangeFactory.IncidentManagementGroup; //Need to reload the group so it isn't reloaded later
			var link2ReloadedInChangeFactory = factoryForChangingProperties.Load<IncidentManagementLink>(link2.PK);
			_ = link2ReloadedInChangeFactory.IncidentManagementGroup; //Need to reload the group so it isn't reloaded later
			var link3ReloadedInEmptyFetchFactory = factoryForEmptyFetch.Load<IncidentManagementLink>(link3.PK);
			_ = link3ReloadedInEmptyFetchFactory.IncidentManagementGroup; //Need to reload the group so it isn't reloaded later
			var client1ReloadedInEmptyFetchFactory = link3ReloadedInEmptyFetchFactory.SupportIncident.Client;

			var newName = "new name";
			link1ReloadedInChangeFactory.SupportIncident.Client.OH_FullName = newName;
			link2ReloadedInChangeFactory.SupportIncident.Client.OH_FullName = newName + "X";
			factoryForChangingProperties.Save();

			AssertNotEquals("Precondition: Should not have synced yet", newName, client1.OH_FullName);
			AssertNotEquals("Precondition: Should not have synced yet", newName + "X", client2.OH_FullName);
			AssertNotEquals("Precondition: Should not have synced yet", newName, client1ReloadedInEmptyFetchFactory.OH_FullName);

			var collectionToUpdate = new IncidentManagementLinkCollection(Factory);
			collectionToUpdate.Add(link1);
			collectionToUpdate.Add(link2);
			var collectionWithEmptyFetch = new IncidentManagementLinkCollection(factoryForEmptyFetch);
			collectionWithEmptyFetch.Add(link3ReloadedInEmptyFetchFactory);

			var clientColumn = new TableColumn(string.Empty, "SupportIncident+Client");
			var emptyColumn = new TableColumn(string.Empty, string.Empty);
			Factory.ResetDatabaseLoadCount();
			factoryForEmptyFetch.ResetDatabaseLoadCount();
			AssertEquals("Precondition", 0, Factory.DatabaseLoadCount);
			AssertEquals("Precondition", 0, factoryForEmptyFetch.DatabaseLoadCount);

			collectionToUpdate.FetchStrategy.FetchForView(new[] { link1, link2 }, new[] { clientColumn });
			collectionWithEmptyFetch.FetchStrategy.FetchForView(new[] { link3ReloadedInEmptyFetchFactory }, new[] { emptyColumn });

			AssertEquals("Fetch with client column should have a single additional db round trip to reload the clients", factoryForEmptyFetch.DatabaseLoadCount + 1, Factory.DatabaseLoadCount);
			AssertEquals("Should have triggered sync", newName, client1.OH_FullName);
			AssertEquals("Should have triggered sync", newName + "X", client2.OH_FullName);
			AssertNotEquals("Should not have triggered sync in factoryForEmptyFetch since the client column was not included in the fetch", newName, client1ReloadedInEmptyFetchFactory.OH_FullName);
		}

		public void TestFetchForViewShouldReloadWorkItems()
		{
			var workItem1 = Factory.NewWithValidTestData<NewWorkItem>();
			var workItem2 = Factory.NewWithValidTestData<NewWorkItem>();
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			var incident3 = Factory.NewWithValidTestData<SupportIncident>();
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();

			incident1.RelatedWorkItems.Add(workItem1);
			incident2.RelatedWorkItems.Add(workItem2);
			incident3.RelatedWorkItems.Add(workItem1);
			var link1 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;
			var link2 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link2.INL_IM_Incident = incident2.PK;
			link2.INL_ING_Group = group.PK;
			var link3 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link3.INL_IM_Incident = incident3.PK;
			link3.INL_ING_Group = group.PK;

			Factory.Save();

			var factoryForChangingProperties = new BusinessObjectFactory { RefreshEnabled = false };
			var factoryForEmptyFetch = new BusinessObjectFactory();
			var link1ReloadedInChangeFactory = factoryForChangingProperties.Load<IncidentManagementLink>(link1.PK);
			_ = link1ReloadedInChangeFactory.IncidentManagementGroup; //Need to reload the group so it isn't reloaded later
			var link2ReloadedInChangeFactory = factoryForChangingProperties.Load<IncidentManagementLink>(link2.PK);
			_ = link2ReloadedInChangeFactory.IncidentManagementGroup; //Need to reload the group so it isn't reloaded later
			var link3ReloadedInEmptyFetchFactory = factoryForEmptyFetch.Load<IncidentManagementLink>(link3.PK);
			_ = link3ReloadedInEmptyFetchFactory.IncidentManagementGroup; //Need to reload the group so it isn't reloaded later
			var workItem1ReloadedInEmptyFetchFactory = link3ReloadedInEmptyFetchFactory.SupportIncident.RelatedWorkItems.Cast<NewWorkItem>().FirstOrDefault();

			var newSummary = "new summary";
			link1ReloadedInChangeFactory.SupportIncident.RelatedWorkItems.Cast<NewWorkItem>().FirstOrDefault().WKI_Summary = newSummary;
			link2ReloadedInChangeFactory.SupportIncident.RelatedWorkItems.Cast<NewWorkItem>().FirstOrDefault().WKI_Summary = newSummary + "X";
			factoryForChangingProperties.Save();

			AssertNotEquals("Precondition: Should not have synced yet", newSummary, workItem1.WKI_Summary);
			AssertNotEquals("Precondition: Should not have synced yet", newSummary + "X", workItem2.WKI_Summary);
			AssertNotEquals("Precondition: Should not have synced yet", newSummary, workItem1ReloadedInEmptyFetchFactory.WKI_Summary);

			var collectionToUpdate = new IncidentManagementLinkCollection(Factory);
			collectionToUpdate.Add(link1);
			collectionToUpdate.Add(link2);
			var collectionWithEmptyFetch = new IncidentManagementLinkCollection(factoryForEmptyFetch);
			collectionWithEmptyFetch.Add(link3ReloadedInEmptyFetchFactory);

			var workItemColumn = new TableColumn(string.Empty, "SupportIncident+WorkItem");
			var emptyColumn = new TableColumn(string.Empty, string.Empty);
			Factory.ResetDatabaseLoadCount();
			factoryForEmptyFetch.ResetDatabaseLoadCount();
			AssertEquals("Precondition", 0, Factory.DatabaseLoadCount);
			AssertEquals("Precondition", 0, factoryForEmptyFetch.DatabaseLoadCount);

			collectionToUpdate.FetchStrategy.FetchForView(new[] { link1, link2 }, new[] { workItemColumn });
			collectionWithEmptyFetch.FetchStrategy.FetchForView(new[] { link3ReloadedInEmptyFetchFactory }, new[] { emptyColumn });

			AssertEquals("Fetch with work item column should have additional db round trips to reload the GenPivots + WIs", factoryForEmptyFetch.DatabaseLoadCount + 2, Factory.DatabaseLoadCount);
			AssertEquals("Should have triggered sync", newSummary, workItem1.WKI_Summary);
			AssertEquals("Should have triggered sync", newSummary + "X", workItem2.WKI_Summary);
			AssertNotEquals("Should not have triggered sync in factoryForEmptyFetch since the work item column was not included in the fetch", newSummary, workItem1ReloadedInEmptyFetchFactory.WKI_Summary);
		}

		public void TestFetchForViewShouldReloadJobConversationMessages()
		{
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			var incident3 = Factory.NewWithValidTestData<SupportIncident>();
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Jan Michael Vincent";
			var participant1 = incident1.EConversation.Conversation.Participants.GetOrAdd(staff);
			var participant2 = incident3.EConversation.Conversation.Participants.GetOrAdd(staff);
			var message1 = incident1.EConversation.Conversation.Messages.AddNew(participant1, "Hello", isInternal: false);
			var link1 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;
			var link2 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link2.INL_IM_Incident = incident2.PK;
			link2.INL_ING_Group = group.PK;
			var link3 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link3.INL_IM_Incident = incident3.PK;
			link3.INL_ING_Group = group.PK;

			Factory.Save();

			AssertEquals("Precondition: 2 Messages, 1 is that participant has been added to conversation, other is Hello", 2, incident1.EConversation.Conversation.Messages.Count);
			AssertEquals("Precondition: No messages since no participants have been added", 0, incident2.EConversation.Conversation.Messages.Count);
			AssertEquals("Precondition: Only message is that participant has been added to conversation", 1, incident3.EConversation.Conversation.Messages.Count);
			var groupLogs = group.Logs.GetAllLogs().Where(x => x.IsInDatabase);
			var initialLogCount = group.Logs.GetAllLogs().Count(x => x.IsInDatabase);

			var factoryForChangingProperties = new BusinessObjectFactory { RefreshEnabled = false };
			var factoryForEmptyFetch = new BusinessObjectFactory();
			var participant1ReloadedInChangeFactory = factoryForChangingProperties.Load<JobConversationParticipant>(participant1.PK);
			var link1ReloadedInChangeFactory = factoryForChangingProperties.Load<IncidentManagementLink>(link1.PK);
			_ = link1ReloadedInChangeFactory.IncidentManagementGroup; //Need to reload the group so it isn't reloaded later
			var link3ReloadedInChangeFactory = factoryForChangingProperties.Load<IncidentManagementLink>(link3.PK);
			_ = link3ReloadedInChangeFactory.IncidentManagementGroup; //Need to reload the group so it isn't reloaded later
			var message1ReloadedInChangeFactory = factoryForChangingProperties.Load<JobConversationMessage>(message1.PK);

			var link3ReloadedInEmptyFetchFactory = factoryForEmptyFetch.Load<IncidentManagementLink>(link3.PK);
			var groupReloadedInEmptyFetchFactory = link3ReloadedInEmptyFetchFactory.IncidentManagementGroup; //Need to reload the group so it isn't reloaded later
			groupReloadedInEmptyFetchFactory.Logs.GetAllLogs();
			var incident3MessagesInEmptyFetchFactory = link3ReloadedInEmptyFetchFactory.SupportIncident.EConversation.Conversation.Messages;

			link1ReloadedInChangeFactory.SupportIncident.EConversation.AddMessageFromCurrentUser("Another", isInternal: false, isSystem: false);
			link3ReloadedInChangeFactory.SupportIncident.EConversation.AddMessageFromCurrentUser("Another", isInternal: false, isSystem: false);
			message1ReloadedInChangeFactory.JCM_Body = "Goodbye";
			factoryForChangingProperties.Save();

			AssertEquals("Precondition: Should not have synced to add new messages", 2, incident1.EConversation.Conversation.Messages.Count);
			AssertEquals("Precondition: Incident 2's conversation has no messages", 0, incident2.EConversation.Conversation.Messages.Count);
			AssertEquals("Precondition: Should not have synced to add new messages", 1, incident3.EConversation.Conversation.Messages.Count);
			AssertEquals("Precondition: Should not have updated log count yet", initialLogCount, groupLogs.Count());

			var collectionToUpdate = new IncidentManagementLinkCollection(Factory);
			collectionToUpdate.Add(link1);
			collectionToUpdate.Add(link2);
			var collectionWithEmptyFetch = new IncidentManagementLinkCollection(factoryForEmptyFetch);
			collectionWithEmptyFetch.Add(link3ReloadedInEmptyFetchFactory);

			var flaggedColumn = new TableColumn(string.Empty, "Flagged");
			var emptyColumn = new TableColumn(string.Empty, string.Empty);
			Factory.ResetDatabaseLoadCount();
			factoryForEmptyFetch.ResetDatabaseLoadCount();
			AssertEquals("Precondition", 0, Factory.DatabaseLoadCount);
			AssertEquals("Precondition", 0, factoryForEmptyFetch.DatabaseLoadCount);

			collectionToUpdate.FetchStrategy.FetchForView(new[] { link1, link2 }, new[] { flaggedColumn });
			collectionWithEmptyFetch.FetchStrategy.FetchForView(new[] { link3ReloadedInEmptyFetchFactory }, new[] { emptyColumn });

			AssertEquals("Fetch with flagged column should have additional db round trips to reload the job conversation + event logs", factoryForEmptyFetch.DatabaseLoadCount + 2, Factory.DatabaseLoadCount);
			AssertEquals("Should have triggered sync", 3, incident1.EConversation.Conversation.Messages.Count);
			AssertGreaterThan("Should have refreshed logs", group.Logs.GetAllLogs().Count, initialLogCount);
			Assert("Should have triggered sync, updating text", incident1.EConversation.Conversation.Messages.Any(x => x.JCM_Body == "Goodbye"));
			AssertEquals("Should not have any messages", 0, incident2.EConversation.Conversation.Messages.Count);
			AssertEquals("Should not have reloaded incident3 in factoryForEmptyFetch since the flagged column was not included in the fetch", 1, incident3MessagesInEmptyFetchFactory.Count);
			AssertEquals("Should not have refreshed logs in factoryForEmptyFetch since the flagged column was not included in the fetch", initialLogCount, groupReloadedInEmptyFetchFactory.Logs.GetAllLogs().Count);
		}

		public void TestFetchForViewShouldReloadLogs()
		{
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			var incident3 = Factory.NewWithValidTestData<SupportIncident>();
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Jan Michael Vincent";
			var participant1 = incident1.EConversation.Conversation.Participants.GetOrAdd(staff);
			var link1 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;
			var link2 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link2.INL_IM_Incident = incident2.PK;
			link2.INL_ING_Group = group.PK;
			var link3 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link3.INL_IM_Incident = incident3.PK;
			link3.INL_ING_Group = group.PK;

			Factory.Save();

			var groupLogs = group.Logs.GetAllLogs().Where(x => x.IsInDatabase);
			var initialLogCount = groupLogs.Count();

			var factoryForChangingProperties = new BusinessObjectFactory { RefreshEnabled = false };
			var factoryForEmptyFetch = new BusinessObjectFactory();
			var link1ReloadedInChangeFactory = factoryForChangingProperties.Load<IncidentManagementLink>(link1.PK);
			_ = link1ReloadedInChangeFactory.IncidentManagementGroup; //Need to reload the group so it isn't reloaded later
			var link3ReloadedInChangeFactory = factoryForChangingProperties.Load<IncidentManagementLink>(link3.PK);
			_ = link3ReloadedInChangeFactory.IncidentManagementGroup; //Need to reload the group so it isn't reloaded later

			var link3ReloadedInEmptyFetchFactory = factoryForEmptyFetch.Load<IncidentManagementLink>(link3.PK);
			var groupReloadedInEmptyFetchFactory = link3ReloadedInEmptyFetchFactory.IncidentManagementGroup; //Need to reload the group so it isn't reloaded later
			groupReloadedInEmptyFetchFactory.Logs.GetAllLogs();
			_ = link3ReloadedInEmptyFetchFactory.SupportIncident; //Need to reload the incident so it isn't reloaded later

			link1ReloadedInChangeFactory.SupportIncident.EConversation.AddMessageFromCurrentUser("Another", isInternal: false, isSystem: false);
			link3ReloadedInChangeFactory.SupportIncident.EConversation.AddMessageFromCurrentUser("Another", isInternal: false, isSystem: false);
			factoryForChangingProperties.Save();

			AssertEquals("Precondition: Should not have updated log count yet", initialLogCount, groupLogs.Count());
			AssertEquals("Precondition: Should not have updated log count", initialLogCount, groupReloadedInEmptyFetchFactory.Logs.GetAllLogs().Count);

			var collectionToUpdate = new IncidentManagementLinkCollection(Factory);
			collectionToUpdate.Add(link1);
			collectionToUpdate.Add(link2);
			var collectionWithEmptyFetch = new IncidentManagementLinkCollection(factoryForEmptyFetch);
			collectionWithEmptyFetch.Add(link3ReloadedInEmptyFetchFactory);

			var lastCommunicationColumn = new TableColumn(string.Empty, "LastCommunication");
			var emptyColumn = new TableColumn(string.Empty, string.Empty);
			Factory.ResetDatabaseLoadCount();
			factoryForEmptyFetch.ResetDatabaseLoadCount();
			AssertEquals("Precondition", 0, Factory.DatabaseLoadCount);
			AssertEquals("Precondition", 0, factoryForEmptyFetch.DatabaseLoadCount);

			collectionToUpdate.FetchStrategy.FetchForView(new[] { link1, link2 }, new[] { lastCommunicationColumn });
			collectionWithEmptyFetch.FetchStrategy.FetchForView(new[] { link3ReloadedInEmptyFetchFactory }, new[] { emptyColumn });

			AssertEquals("Fetch with last communication column should have a single additional db round trip to reload the event logs", factoryForEmptyFetch.DatabaseLoadCount + 1, Factory.DatabaseLoadCount);
			AssertGreaterThan("Should have refreshed logs", group.Logs.GetAllLogs().Count, initialLogCount);
			AssertEquals("Should not have refreshed logs in factoryForEmptyFetch since the last communication column was not included in the fetch", initialLogCount, groupReloadedInEmptyFetchFactory.Logs.GetAllLogs().Count);
		}

		public void TestFetchForViewShouldReloadResponder()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			var incident3 = Factory.NewWithValidTestData<SupportIncident>();
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();

			var link1 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;
			link1.INL_GS_NKResponder = staff1.GS_Code;
			var link2 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link2.INL_IM_Incident = incident2.PK;
			link2.INL_ING_Group = group.PK;
			link2.INL_GS_NKResponder = staff2.GS_Code;
			var link3 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link3.INL_IM_Incident = incident3.PK;
			link3.INL_ING_Group = group.PK;
			link3.INL_GS_NKResponder = staff1.GS_Code;

			Factory.Save();

			var factoryForChangingProperties = new BusinessObjectFactory { RefreshEnabled = false };
			var factoryForEmptyFetch = new BusinessObjectFactory();
			var link1ReloadedInChangeFactory = factoryForChangingProperties.Load<IncidentManagementLink>(link1.PK);
			_ = link1ReloadedInChangeFactory.IncidentManagementGroup; //Need to reload the group so it isn't reloaded later
			var link2ReloadedInChangeFactory = factoryForChangingProperties.Load<IncidentManagementLink>(link2.PK);
			_ = link2ReloadedInChangeFactory.IncidentManagementGroup; //Need to reload the group so it isn't reloaded later
			var link3ReloadedInEmptyFetchFactory = factoryForEmptyFetch.Load<IncidentManagementLink>(link3.PK);
			_ = link3ReloadedInEmptyFetchFactory.IncidentManagementGroup; //Need to reload the group so it isn't reloaded later
			var staff1ReloadedInEmptyFetchFactory = link3ReloadedInEmptyFetchFactory.Responder;

			var newName1 = "NewName";
			var newName2 = "EmanWen";
			link1ReloadedInChangeFactory.Responder.GS_FullName = newName1;
			link2ReloadedInChangeFactory.Responder.GS_FullName = newName2;
			factoryForChangingProperties.Save();

			AssertNotEquals("Precondition: Should not have synced yet", newName1, staff1.GS_FullName);
			AssertNotEquals("Precondition: Should not have synced yet", newName2, staff2.GS_FullName);
			AssertNotEquals("Precondition: Should not have synced yet", newName1, staff1.GS_FullName);

			var collectionToUpdate = new IncidentManagementLinkCollection(Factory);
			collectionToUpdate.Add(link1);
			collectionToUpdate.Add(link2);
			var collectionWithEmptyFetch = new IncidentManagementLinkCollection(factoryForEmptyFetch);
			collectionWithEmptyFetch.Add(link3ReloadedInEmptyFetchFactory);

			var responderColumn = new TableColumn(string.Empty, "Responder");
			var emptyColumn = new TableColumn(string.Empty, string.Empty);
			Factory.ResetDatabaseLoadCount();
			factoryForEmptyFetch.ResetDatabaseLoadCount();
			AssertEquals("Precondition", 0, Factory.DatabaseLoadCount);
			AssertEquals("Precondition", 0, factoryForEmptyFetch.DatabaseLoadCount);

			collectionToUpdate.FetchStrategy.FetchForView(new[] { link1, link2 }, new[] { responderColumn });
			collectionWithEmptyFetch.FetchStrategy.FetchForView(new[] { link3ReloadedInEmptyFetchFactory }, new[] { emptyColumn });

			AssertEquals("Fetch with responder column should have a single additional db round trip to reload the responder", factoryForEmptyFetch.DatabaseLoadCount + 1, Factory.DatabaseLoadCount);
			AssertEquals("Should have triggered sync", newName1, staff1.GS_FullName);
			AssertEquals("Should have triggered sync", newName2, staff2.GS_FullName);
			AssertNotEquals("Should not have triggered sync in factoryForEmptyFetch since the responder column was not included in the fetch", newName1, staff1ReloadedInEmptyFetchFactory.GS_FullName);
		}

		public void TestFetchForViewShouldReloadStages()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			var incident3 = Factory.NewWithValidTestData<SupportIncident>();
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = registryValue[0].GroupType;
			var initialStageCount = registryValue[0].IncidentGroupStatusConfigurations.Count;

			var link1 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;
			var link3 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link3.INL_IM_Incident = incident3.PK;
			link3.INL_ING_Group = group.PK;

			Factory.Save();

			var factoryForEmptyFetch = new BusinessObjectFactory();
			var link3ReloadedInEmptyFetchFactory = factoryForEmptyFetch.Load<IncidentManagementLink>(link3.PK);
			var groupReloadedInEmptyFetchFactory = link3ReloadedInEmptyFetchFactory.IncidentManagementGroup;
			_ = groupReloadedInEmptyFetchFactory.Stages; //Need to load the stage before changes are made

			//Update Stages
			registryValue[0].IncidentGroupStatusConfigurations.AddNew("AAA", "Apples", IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue, enabled: true);
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			AssertEquals("Precondition: Should not have synced yet", initialStageCount, group.Stages.Count);
			AssertEquals("Precondition: Should not have synced", initialStageCount, groupReloadedInEmptyFetchFactory.Stages.Count);

			var collectionToUpdate = new IncidentManagementLinkCollection(Factory);
			collectionToUpdate.Add(link1);
			var collectionWithEmptyFetch = new IncidentManagementLinkCollection(factoryForEmptyFetch);
			collectionWithEmptyFetch.Add(link3ReloadedInEmptyFetchFactory);

			var isControlledColumn = new TableColumn(string.Empty, "IsControlled");
			var emptyColumn = new TableColumn(string.Empty, string.Empty);
			Factory.ResetDatabaseLoadCount();
			factoryForEmptyFetch.ResetDatabaseLoadCount();
			AssertEquals("Precondition", 0, Factory.DatabaseLoadCount);
			AssertEquals("Precondition", 0, factoryForEmptyFetch.DatabaseLoadCount);

			collectionToUpdate.FetchStrategy.FetchForView(new[] { link1 }, new[] { isControlledColumn });
			collectionWithEmptyFetch.FetchStrategy.FetchForView(new[] { link3ReloadedInEmptyFetchFactory }, new[] { emptyColumn });

			AssertEquals("Should have triggered sync", initialStageCount + 1, group.Stages.Count);
			AssertEquals("Should not have triggered sync in factoryForEmptyFetch since the is controlled column was not included in the fetch", initialStageCount, groupReloadedInEmptyFetchFactory.Stages.Count);
		}
	}
}
