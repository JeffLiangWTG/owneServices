using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Client.EDI.IssueManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Business.Test;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(WorkTaskRelatedItemGenPivotCollection<SupportIncident>))]
	public class EDIWorkTaskRelatedItemCollectionGenericTest : WorkTaskRelatedItemCollectionTestCase<NewWorkItem>
	{
		public void TestAddIncidentToWI_ShouldIncrementNudgeAmount()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();

			helper.EnableBMSInRegistry();
			var system = helper.CreateSystem(Factory, "INC", "WKI");

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(workItem, Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			AssertEquals((short)0, workflow1.FH_VoteUpDownAmount);
			AssertEquals((short)0, workflow2.FH_VoteUpDownAmount);

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();

			workItem.RelatedItems.Add(incident1);
			AssertEquals((short)1, jobHeader.FH_VoteUpDownAmount);

			workItem.RelatedItems.Add(incident2);
			AssertEquals((short)2, jobHeader.FH_VoteUpDownAmount);
		}

		public void TestAddIssueToWI_ShouldNotIncrementNudgeAmount()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();

			helper.EnableBMSInRegistry();
			helper.CreateSystem(Factory, "WKI", "INC");

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(workItem, Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			AssertEquals((short)0, workflow1.FH_VoteUpDownAmount);
			AssertEquals((short)0, workflow2.FH_VoteUpDownAmount);

			var issue = Factory.NewWithValidTestData<EdiHelpErrorLog>();

			workItem.RelatedItems.Add(issue);

			AssertEquals((short)0, workflow1.FH_VoteUpDownAmount);
			AssertEquals((short)0, workflow2.FH_VoteUpDownAmount);

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();

			workItem.RelatedItems.Add(incident1);
			AssertEquals((short)1, jobHeader.FH_VoteUpDownAmount);

			workItem.RelatedItems.Add(incident2);
			AssertEquals((short)2, jobHeader.FH_VoteUpDownAmount);
		}

		public void TestAddWIToIssue_ShouldNotIncrementNudgeAmount()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();

			helper.EnableBMSInRegistry();
			helper.CreateSystem(Factory, "WKI", "INC");

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(workItem, Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			AssertEquals((short)0, workflow1.FH_VoteUpDownAmount);
			AssertEquals((short)0, workflow2.FH_VoteUpDownAmount);

			var issue = Factory.NewWithValidTestData<EdiHelpErrorLog>();

			workItem.RelatedItems.Add(issue);

			AssertEquals((short)0, workflow1.FH_VoteUpDownAmount);
			AssertEquals((short)0, workflow2.FH_VoteUpDownAmount);

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();

			workItem.RelatedItems.Add(incident1);
			AssertEquals((short)1, jobHeader.FH_VoteUpDownAmount);

			workItem.RelatedItems.Add(incident2);
			AssertEquals((short)2, jobHeader.FH_VoteUpDownAmount);
		}

		public void TestAddWIToIncident_ShouldIncrementNudgeAmount()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();

			helper.EnableBMSInRegistry();
			var system = helper.CreateSystem(Factory, "INC", "WKI");

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(workItem, Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			AssertEquals((short)0, jobHeader.FH_VoteUpDownAmount);

			incident1.RelatedItems.Add(workItem);
			AssertEquals((short)1, jobHeader.FH_VoteUpDownAmount);
		}

		public override void TestShouldAddToCollection()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();

			AssertEquals(0, Collection.Count);

			Collection.Add(incident);
			Collection.Add(workItem);

			AssertEquals("Should contain only Support Incident", 1, Collection.Count);
			Assert("Should have incident", Collection.Contains(incident));
		}

		public void TestShouldLoadToCollection()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.SetupForNewCreatedDefect();
			NewWorkItem workitem = Factory.NewWithValidTestData<NewWorkItem>();
			incident.DefectCausedByWorkItemPK = workitem.PK;

			NewWorkItem workitem2 = Factory.NewWithValidTestData<NewWorkItem>();
			incident.RelatedItems.Add(workitem2);

			Factory.Save();

			BusinessObjectFactory newfactory = new BusinessObjectFactory();
			SupportIncident loadIncident = newfactory.Load<SupportIncident>(incident.PK);

			AssertEquals("Should contain only one workitem", 1, loadIncident.RelatedItems.Count);
			Assert("Should have workitem not marked as caused workitem", loadIncident.RelatedItems.Contains(workitem2));
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(WorkTaskRelatedItemGenPivotCollection<SupportIncident>);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new WorkTaskRelatedItemGenPivotCollection<SupportIncident>(Factory.NewWithValidTestData<SupportIncident>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.NewWithValidTestData<SupportIncident>();
		}
	}

	[TestedType(typeof(WorkTaskRelatedItemCollection))]
	public class WorkTaskRelatedItemCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestGetElements()
		{
			NewWorkItem workItem = Factory.New<NewWorkItem>();
			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			ProfessionalServicesQuote quote = Factory.New<ProfessionalServicesQuote>();
			ProfessionalServicesQuote quote2 = Factory.New<ProfessionalServicesQuote>();
			EdiHelpErrorLog issue = Factory.New<EdiHelpErrorLog>();
			workItem.RelatedItems.Add(incident);
			workItem.RelatedItems.Add(quote);
			workItem.RelatedItems.Add(quote2);
			workItem.RelatedItems.Add(issue);

			AssertEquals(2, workItem.RelatedItems.GetElements<ProfessionalServicesQuote>().Count());
			Assert(workItem.RelatedItems.GetElements<ProfessionalServicesQuote>().Contains(quote));
			Assert(workItem.RelatedItems.GetElements<ProfessionalServicesQuote>().Contains(quote2));

			AssertEquals(1, workItem.RelatedItems.GetElements<ProfessionalServicesQuote>(psq => psq.PK == quote.PK).Count());
			Assert(workItem.RelatedItems.GetElements<ProfessionalServicesQuote>().Contains(quote));

			AssertEquals(1, workItem.RelatedItems.GetElements<SupportIncident>().Count());
			Assert(workItem.RelatedItems.GetElements<SupportIncident>().Contains(incident));

			AssertEquals(1, workItem.RelatedItems.GetElements<EdiHelpErrorLog>().Count());
			Assert(workItem.RelatedItems.GetElements<EdiHelpErrorLog>().Contains(issue));
		}

		public void TestLoadDoesNotCallOnWorkItemAdded()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, ZString.Empty);
			incident.AssignToStaff(GlbStaff.CurrentUser, "Assignment Comment");
			NewWorkItem workItem = incident.RelatedWorkItems.AddNew();
			workItem.WorkflowItems.AddNew().P9_Status = "CLS";
			incident.SetUpgradeDelivered();
			AssertEquals("incident.IM_ResolutionCode", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
			var collection = new NewWorkItemRelatedCollection(workItem);
			collection.Load();
			AssertEquals("incident.IM_ResolutionCode", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
		}

		#region Implementation

		protected override Type GetExpectedCollectionType()
		{
			return typeof(WorkTaskRelatedItemCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new WorkTaskRelatedItemGenPivotCollection(Factory.NewWithValidTestData<SupportIncident>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.NewWithValidTestData<SupportIncident>();
		}

		#endregion
	}
}
