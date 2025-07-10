using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	sealed class ProfessionalServicesQuoteLookupsTest : IncidentMainLookupsTestCase
	{
		public void TestActiveClientList()
		{
			string orgName = "Everyone's favourite Org";
			OrgHeader client1 = Factory.NewWithValidTestData<OrgHeader>();
			client1.OH_FullName = orgName;
			OrgHeader client2 = Factory.NewWithValidTestData<OrgHeader>();
			client2.OH_FullName = orgName;

			client1.OH_IsActive = true;
			client2.OH_IsActive = false;

			Lookups.ActiveClientList.LoadWithMoreFiltering(new ZQuery(OrgHeaderSchema.OH_FullName, orgName));

			AssertEquals("ActiveClientList should contain Client1.", true, Lookups.ActiveClientList.Contains(client1));
			AssertEquals("ActiveClientList should NOT contain Client2.", false, Lookups.ActiveClientList.Contains(client2));
		}

		public void TestCustomerServiceContacts()
		{
			GlbStaff staff1 = Factory.New<GlbStaff>();
			GlbStaff staff2 = Factory.New<GlbStaff>();

			staff1.GS_IsActive = true;
			staff2.GS_IsActive = false;

			GlbStaffCollection collection = Lookups.CustServiceContacts;

			AssertEquals("CustomerServiceContacts should contain Staff1.", true, collection.Contains(staff1));
			AssertEquals("CustomerServiceContacts should NOT contain Staff2.", false, collection.Contains(staff2));
		}

		public void TestWorkItemList()
		{
			NewWorkItem workItem1 = Factory.New<NewWorkItem>();
			NewWorkItem workItem2 = Factory.New<NewWorkItem>();

			Lookups.WorkItemList.Load();

			AssertEquals("WorkItemList should contain WorkItem1.", true, Lookups.WorkItemList.Contains(workItem1));
			AssertEquals("WorkItemList should contain WorkItem2.", true, Lookups.WorkItemList.Contains(workItem2));
		}

		public void TestProjectList()
		{
			EDIProject project1 = Factory.NewWithValidTestData<EDIProject>();
			EDIProject project2 = Factory.NewWithValidTestData<EDIProject>();

			Lookups.ProjectList.Load();

			Assert("ProjectList should contain project1", Lookups.ProjectList.Contains(project1));
			Assert("ProjectList should contain project2", Lookups.ProjectList.Contains(project2));
		}

		public void TestPriorityList()
		{
			AssertEquals("PriorityList.Count", 4, Lookups.PriorityList.Count);
			AssertEquals("PriorityList.GetDescriptionFromCode(\"CRT\").", "Critical", Lookups.PriorityList.GetDescriptionFromCode(IncidentConstants.Priority.Critical));
		}

		protected override IReadOnlyList<CodeDescriptionPair> ExpectedStatusList
		{
			get
			{
				return new CodeDescriptionPair[]
				{
					new CodeDescriptionPair(IncidentConstants.IncidentStatus.Cancelled, "Cancelled"),
					new CodeDescriptionPair(IncidentConstants.IncidentStatus.Closed, "Closed"),
					new CodeDescriptionPair(ProfessionalServicesQuoteLookups.ProfessionalServiceQuoteStatusCodes.Invoice, "Invoice"),
					new CodeDescriptionPair(ProfessionalServicesQuoteLookups.ProfessionalServiceQuoteStatusCodes.Quote, "Quote"),
					new CodeDescriptionPair(ProfessionalServicesQuoteLookups.ProfessionalServiceQuoteStatusCodes.WaitForCustomer, "Wait for Customer"),
					new CodeDescriptionPair(ProfessionalServicesQuoteLookups.ProfessionalServiceQuoteStatusCodes.WaitForOrder, "Wait for Order"),
					new CodeDescriptionPair(ProfessionalServicesQuoteLookups.ProfessionalServiceQuoteStatusCodes.WaitForPayment, "Wait for Payment"),
					new CodeDescriptionPair(ProfessionalServicesQuoteLookups.ProfessionalServiceQuoteStatusCodes.WaitForSomeone, "Wait for Someone"),
					new CodeDescriptionPair(ProfessionalServicesQuoteLookups.ProfessionalServiceQuoteStatusCodes.WorkInProgress, "Work in Progress")
				};
			}
		}

		ProfessionalServicesQuote Parent
		{
			get { return (ProfessionalServicesQuote)base.Incident; }
		}

		ProfessionalServicesQuoteLookups Lookups
		{
			get { return Parent.Lookups; }
		}

		protected override AutoIncidentMain GetNewIncidentMain() => Factory.New<ProfessionalServicesQuote>();
	}
}
