using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(PSQuoteProcessTaskCollection))]
	public class PSQuoteProcessTaskCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDefaults()
		{
			OrgHeader client = Factory.New<OrgHeader>();
			client.Contacts.AddNew();

			ProfessionalServicesQuote psq = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			PSQuoteProcessTaskCollection collection = psq.WorkflowItems;

			psq.IM_OH_Client = client.PK;
			psq.IM_OA_BranchAddress = client.MainAddress.PK;
			psq.IM_OC_Contact = client.Contacts[0].PK;

			ProcessTask task = collection.AddNew();
			AssertEquals(client.PK, task.OrganisationPK);
			AssertEquals(client.MainAddress.PK, task.P9_OA);
			AssertEquals(client.Contacts[0].PK, task.P9_OC);
		}

		public void TestIndexer()
		{
			AssertEquals(typeof(PSQuoteProcessTask), Collection.AddNew().GetType());
			AssertEquals(typeof(PSQuoteProcessTask), ((PSQuoteProcessTaskCollection)Collection)[0].GetType());
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(PSQuoteProcessTaskCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return Factory.New<ProfessionalServicesQuote>().WorkflowItems;
		}
	}
}
