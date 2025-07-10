using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Module.Testing
{
	[TestedType(typeof(DocumentListMessagesFilterStripBusinessObject))]
	public class DocumentListMessagesFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new DocumentListMessagesFilterStripBusinessObject();

		public void TestFilters()
		{
			var filter = new DocumentListMessagesFilterStripBusinessObject();
			AssertNotNull(filter[DocumentListMessagesFilterStripBusinessObject.Schema.MessageNumber]);
			AssertNotNull(filter[DocumentListMessagesFilterStripBusinessObject.Schema.Status]);
			AssertNotNull(filter[DocumentListMessagesFilterStripBusinessObject.Schema.MessageTime]);
			AssertNotNull(filter[DocumentListMessagesFilterStripBusinessObject.Schema.InterchangeNumber]);
			AssertNotNull(filter[DocumentListMessagesFilterStripBusinessObject.Schema.InterchangeDateTimeSent]);
			AssertNotNull(filter[DocumentListMessagesFilterStripBusinessObject.Schema.EHubID]);
			AssertNotNull(filter[DocumentListMessagesFilterStripBusinessObject.Schema.DocumentType]);
			AssertNotNull(filter[DocumentListMessagesFilterStripBusinessObject.Schema.RequestStatus]);
		}

		public void TestFilterMessageNumber()
		{
			SetupData();

			var filter = new DocumentListMessagesFilterStripBusinessObject();
			var messageNumFilter = (ModuleTextFilter)filter[DocumentListMessagesFilterStripBusinessObject.Schema.MessageNumber];
			messageNumFilter.Property = "1";
			messageNumFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			messageNumFilter.IsActive = true;

			var collection = new NonDependentDLTMessageCollection(Factory, GlbCompany.CurrentCompany);
			collection.Load(filter.Filter);

			AssertEquals(1, collection.Count);
			AssertEquals("1", collection[0].EM_MessageNum);

			messageNumFilter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			collection.Load(filter.Filter);

			AssertEquals(2, collection.Count);
			AssertEquals("2", collection[0].EM_MessageNum);
			AssertEquals("3", collection[1].EM_MessageNum);
		}
		public void TestFilterStatus()
		{
			SetupData();

			var filter = new DocumentListMessagesFilterStripBusinessObject();
			var statusFilter = (ModuleTextFilter)filter[DocumentListMessagesFilterStripBusinessObject.Schema.Status];
			statusFilter.Property = "RCV";
			statusFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			statusFilter.IsActive = true;

			var collection = new NonDependentDLTMessageCollection(Factory, GlbCompany.CurrentCompany);
			collection.Load(filter.Filter);

			AssertEquals(1, collection.Count);
			AssertEquals("1", collection[0].EM_MessageNum);
			AssertEquals("RCV", collection[0].EM_Status);

			statusFilter.Property = "XXX";
			collection.Load(filter.Filter);

			AssertEquals(0, collection.Count);
		}
		public void TestFilterMessageTime()
		{
			SetupData();

			var filter = new DocumentListMessagesFilterStripBusinessObject();
			var messageTimeFilter = (ModuleDateFilter)filter[DocumentListMessagesFilterStripBusinessObject.Schema.MessageTime];
			messageTimeFilter.Property1 = new ZDateTime(2021, 10, 30);
			messageTimeFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			messageTimeFilter.IsActive = true;

			var collection = new NonDependentDLTMessageCollection(Factory, GlbCompany.CurrentCompany);
			collection.Load(filter.Filter);

			AssertEquals(2, collection.Count);
			AssertEquals(new DateTime(2021, 11, 1), Environment.Env.Time.GetUtcFromLocalTime(collection[0].EM_MessageDateTime.ToDateTime()));
			AssertEquals(new DateTime(2021, 12, 1), Environment.Env.Time.GetUtcFromLocalTime(collection[1].EM_MessageDateTime.ToDateTime()));

			messageTimeFilter.Property2 = new ZDateTime(2021, 11, 30);
			collection.Load(filter.Filter);

			AssertEquals(1, collection.Count);
			AssertEquals(new DateTime(2021, 11, 1), Environment.Env.Time.GetUtcFromLocalTime(collection[0].EM_MessageDateTime.ToDateTime()));
		}
		public void TestFilterInterchangeNumber()
		{
			SetupData();

			var filter = new DocumentListMessagesFilterStripBusinessObject();
			var interchangeNumberFilter = (ModuleTextFilter)filter[DocumentListMessagesFilterStripBusinessObject.Schema.InterchangeNumber];
			interchangeNumberFilter.Property = "1";
			interchangeNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			interchangeNumberFilter.IsActive = true;

			var collection = new NonDependentDLTMessageCollection(Factory, GlbCompany.CurrentCompany);
			collection.Load(filter.Filter);

			AssertEquals(1, collection.Count);
			AssertEquals("11", collection[0].EM_InterchangeNumber);

			interchangeNumberFilter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			collection.Load(filter.Filter);

			AssertEquals(2, collection.Count);
			AssertEquals("22", collection[0].EM_InterchangeNumber);
			AssertEquals("33", collection[1].EM_InterchangeNumber);
		}
		public void TestFilterInterchangeDateTimeSent()
		{
			SetupData();

			var filter = new DocumentListMessagesFilterStripBusinessObject();
			var interchangeDateTimeSentFilter = (ModuleDateFilter)filter[DocumentListMessagesFilterStripBusinessObject.Schema.InterchangeDateTimeSent];
			interchangeDateTimeSentFilter.Property1 = new ZDateTime(2022, 10, 30);
			interchangeDateTimeSentFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			interchangeDateTimeSentFilter.IsActive = true;

			var collection = new NonDependentDLTMessageCollection(Factory, GlbCompany.CurrentCompany);
			collection.Load(filter.Filter);

			AssertEquals(2, collection.Count);
			AssertEquals(new DateTime(2022, 11, 1), Environment.Env.Time.GetUtcFromLocalTime(collection[0].EM_DateTimeInterchangeSent.ToDateTime()));
			AssertEquals(new DateTime(2022, 12, 1), Environment.Env.Time.GetUtcFromLocalTime(collection[1].EM_DateTimeInterchangeSent.ToDateTime()));

			interchangeDateTimeSentFilter.Property1 = new ZDateTime(2022, 11, 30);
			collection.Load(filter.Filter);

			AssertEquals(1, collection.Count);
			AssertEquals(new DateTime(2022, 12, 1), Environment.Env.Time.GetUtcFromLocalTime(collection[0].EM_DateTimeInterchangeSent.ToDateTime()));
		}
		public void TestFilterEHubID()
		{
			SetupData();

			var filter = new DocumentListMessagesFilterStripBusinessObject();
			var eHubIDFilter = (ModuleTextFilter)filter[DocumentListMessagesFilterStripBusinessObject.Schema.EHubID];
			eHubIDFilter.Property = "C214E153-EE8A-4D04-9B36-AF1BBC9483D4";
			eHubIDFilter.IsActive = true;

			var collection = new NonDependentDLTMessageCollection(Factory, GlbCompany.CurrentCompany);
			collection.Load(filter.Filter);

			AssertEquals(1, collection.Count);
			AssertEquals("11", collection[0].EM_InterchangeNumber);

			eHubIDFilter.Property = ZGuid.NewZGuid().ToString();
			collection.Load(filter.Filter);

			AssertEquals(0, collection.Count);
		}

		public void TestFilterDocumentType()
		{
			SetupData();

			var filter = new DocumentListMessagesFilterStripBusinessObject();
			var applicationReferenceFilter = (ModuleTextFilter)filter[DocumentListMessagesFilterStripBusinessObject.Schema.DocumentType];
			applicationReferenceFilter.Property = "5AF";
			applicationReferenceFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			applicationReferenceFilter.IsActive = true;

			var collection = new NonDependentDLTMessageCollection(Factory, GlbCompany.CurrentCompany);
			collection.Load(filter.Filter);

			AssertEquals(1, collection.Count);
			AssertEquals("1", collection[0].EM_MessageNum);
			AssertEquals(1, collection[0].CusPollingTransactions.Count);
			Assert(collection[0].CusPollingTransactions.Any(item => item.CPT_Reference == "5AF"));

			applicationReferenceFilter.Property = "5DT";
			collection.Load(filter.Filter);

			AssertEquals(2, collection.Count);
			AssertEquals("2", collection[0].EM_MessageNum);
			AssertEquals(2, collection[0].CusPollingTransactions.Count);
			Assert(collection[0].CusPollingTransactions.Any(item => item.CPT_Reference == "5AA"));
			Assert(collection[0].CusPollingTransactions.Any(item => item.CPT_Reference == "5DT"));
			AssertEquals("3", collection[1].EM_MessageNum);
			AssertEquals(1, collection[1].CusPollingTransactions.Count);
			Assert(collection[1].CusPollingTransactions.Any(item => item.CPT_Reference == "5DT"));
		}

		public void TestFilterRequestStatus()
		{
			SetupData();

			var filter = new DocumentListMessagesFilterStripBusinessObject();
			var applicationReferenceFilter = (ModuleTextFilter)filter[DocumentListMessagesFilterStripBusinessObject.Schema.RequestStatus];
			applicationReferenceFilter.Property = "OPN";
			applicationReferenceFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			applicationReferenceFilter.IsActive = true;

			var collection = new NonDependentDLTMessageCollection(Factory, GlbCompany.CurrentCompany);
			collection.Load(filter.Filter);

			AssertEquals(3, collection.Count);
			AssertEquals("1", collection[0].EM_MessageNum);
			AssertEquals(1, collection[0].CusPollingTransactions.Count);
			AssertEquals("OPN", collection[0].CusPollingTransactions[0].CPT_Status);
			AssertEquals("2", collection[1].EM_MessageNum);
			AssertEquals(2, collection[1].CusPollingTransactions.Count);
			AssertEquals("OPN", collection[1].CusPollingTransactions[0].CPT_Status);
			AssertEquals("OPN", collection[1].CusPollingTransactions[1].CPT_Status);
			AssertEquals("3", collection[2].EM_MessageNum);
			AssertEquals(1, collection[2].CusPollingTransactions.Count);
			AssertEquals("OPN", collection[2].CusPollingTransactions[0].CPT_Status);

			applicationReferenceFilter.Property = "ERR";
			collection.Load(filter.Filter);

			AssertEquals(0, collection.Count);
		}

		void SetupData()
		{
			var interchange1 = Factory.NewWithValidTestData<Enterprise.Messaging.Business.EDIInterchange>();
			interchange1.EI_InterchangeNum = "11";
			interchange1.EI_SystemCreateTimeUtc = new ZDateTime(2022, 10, 01);
			interchange1.EI_From = "EDITST1";
			interchange1.EI_To = "TSTEDI1";
			interchange1.EI_SessionGUID = new ZGuid("C214E153-EE8A-4D04-9B36-AF1BBC9483D4");

			var msg1 = Factory.NewWithValidTestData<EDIMessage>();
			msg1.EM_MessageType = "DLT";
			msg1.EM_ApplicationCode = "KRC";
			msg1.EM_ApplicationReference = "123";
			msg1.EM_MessageNum = "1";
			msg1.EM_ReceiveTransmit = "RCV";
			msg1.EM_MessageSubType = "TST";
			msg1.EM_Status = "RCV";
			msg1.EM_MessageText = "2020040909570320200409-ELI-edb54e29-d2aa-49ea-849c-87f29bbf1f42,GOVCBR5AF";
			msg1.EM_SystemCreateTimeUtc = new ZDateTime(2021, 10, 01);
			msg1.EM_EI = interchange1.PK;

			var cusPollingTransaction1 = Factory.NewWithValidTestData<CusPollingTransaction>();
			cusPollingTransaction1.CPT_ParentID = msg1.PK;
			cusPollingTransaction1.CPT_ApplicationCode = "KRC";
			cusPollingTransaction1.CPT_Reference = "5AF";
			cusPollingTransaction1.CPT_Status = "OPN";
			cusPollingTransaction1.CPT_NumberOfAttempts = 1;
			cusPollingTransaction1.CPT_TransactionID = "2020040909570320200409-ELI-edb54e29-d2aa-49ea-849c-87f29bbf1f42";

			var interchange2 = Factory.NewWithValidTestData<Enterprise.Messaging.Business.EDIInterchange>();
			interchange2.EI_InterchangeNum = "22";
			interchange2.EI_SystemCreateTimeUtc = new ZDateTime(2022, 11, 01);
			interchange2.EI_From = "EDITST2";
			interchange2.EI_To = "TSTEDI2";
			interchange2.EI_SessionGUID = new ZGuid("3CCEF310-14A9-44BB-9A48-4D3D4AEC0F6C");

			var msg2 = Factory.NewWithValidTestData<EDIMessage>();
			msg2.EM_MessageType = "DLT";
			msg2.EM_ApplicationCode = "KRC";
			msg2.EM_ApplicationReference = "456";
			msg2.EM_MessageNum = "2";
			msg2.EM_ReceiveTransmit = "RCV";
			msg2.EM_MessageSubType = "830";
			msg2.EM_Status = "QUE";
			msg2.EM_MessageText = "2020040909570320200409-ELI-896BB202-9BC0-4204-A47A-31DD6F67579C,GOVCBR5AA" + System.Environment.NewLine
				+ "2020040909570320200409-ELI-7BB4B77B-FAE0-4702-8A78-3570807C9EA3,GOVCBR5DT";
			msg2.EM_SystemCreateTimeUtc = new ZDateTime(2021, 11, 01);
			msg2.EM_EI = interchange2.PK;

			var cusPollingTransaction2 = Factory.NewWithValidTestData<CusPollingTransaction>();
			cusPollingTransaction2.CPT_ParentID = msg2.PK;
			cusPollingTransaction2.CPT_ApplicationCode = "KRC";
			cusPollingTransaction2.CPT_Reference = "5AA";
			cusPollingTransaction2.CPT_Status = "OPN";
			cusPollingTransaction2.CPT_NumberOfAttempts = 1;
			cusPollingTransaction2.CPT_TransactionID = "2020040909570320200409-ELI-896BB202-9BC0-4204-A47A-31DD6F67579C";

			var cusPollingTransaction3 = Factory.NewWithValidTestData<CusPollingTransaction>();
			cusPollingTransaction3.CPT_ParentID = msg2.PK;
			cusPollingTransaction3.CPT_ApplicationCode = "KRC";
			cusPollingTransaction3.CPT_Reference = "5DT";
			cusPollingTransaction3.CPT_Status = "OPN";
			cusPollingTransaction3.CPT_NumberOfAttempts = 1;
			cusPollingTransaction3.CPT_TransactionID = "2020040909570320200409-ELI-7BB4B77B-FAE0-4702-8A78-3570807C9EA3";

			var interchange3 = Factory.NewWithValidTestData<Enterprise.Messaging.Business.EDIInterchange>();
			interchange3.EI_InterchangeNum = "33";
			interchange3.EI_SystemCreateTimeUtc = new ZDateTime(2022, 12, 01);
			interchange3.EI_From = "EDITST3";
			interchange3.EI_To = "TSTEDI3";
			interchange3.EI_SessionGUID = new ZGuid("D98768C8-A316-4275-9847-189CCD2191EC");

			var msg3 = Factory.NewWithValidTestData<EDIMessage>();
			msg3.EM_MessageType = "DLT";
			msg3.EM_ApplicationCode = "KRC";
			msg3.EM_ApplicationReference = "789";
			msg3.EM_MessageNum = "3";
			msg3.EM_ReceiveTransmit = "RCV";
			msg3.EM_MessageSubType = "5AS";
			msg3.EM_Status = "SNT";
			msg3.EM_MessageText = "2020040909570320200409-ELI-719EB8C4-429E-4073-90FD-95B195424D78,GOVCBR5DT";
			msg3.EM_SystemCreateTimeUtc = new ZDateTime(2021, 12, 01);
			msg3.EM_EI = interchange3.PK;

			var cusPollingTransaction4 = Factory.NewWithValidTestData<CusPollingTransaction>();
			cusPollingTransaction4.CPT_ParentID = msg3.PK;
			cusPollingTransaction4.CPT_ApplicationCode = "KRC";
			cusPollingTransaction4.CPT_Reference = "5DT";
			cusPollingTransaction4.CPT_Status = "OPN";
			cusPollingTransaction4.CPT_NumberOfAttempts = 1;
			cusPollingTransaction4.CPT_TransactionID = "2020040909570320200409-ELI-719EB8C4-429E-4073-90FD-95B195424D78";

			Factory.Save();
		}
	}
}
