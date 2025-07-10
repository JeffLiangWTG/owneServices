using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Module.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(K84MessageFilterBusinessObject))]
	sealed class K84MessageFilterBusinessObjectTest : EDIMessageFilterBusinessObjectTest
	{
		public void TestGetMessageSubTypeTextQuery()
		{
			var message1 = Factory.New<ARLMessage>();
			message1.FillWithValidTestData();
			message1.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch;
			var column1 = Factory.New<GenAddOnColumn>();
			column1.FillWithValidTestData();
			column1.XA_ParentTableCode = EDIMessageSchema.Constants.Prefix;
			column1.XA_Name = EDIMessage.Schema.XMLCustomsMessageType;
			column1.XA_Data = "DN";
			column1.XA_ParentID = message1.PK;

			var message2 = Factory.New<ARLMessage>();
			message2.FillWithValidTestData();
			message2.EM_MessageSubType = EDIMessageSubTypeList.Codes.Brokerage;
			var column2 = Factory.New<GenAddOnColumn>();
			column2.FillWithValidTestData();
			column2.XA_ParentTableCode = EDIMessageSchema.Constants.Prefix;
			column2.XA_Name = EDIMessage.Schema.XMLCustomsMessageType;
			column2.XA_Data = "SOA";
			column2.XA_ParentID = message2.PK;

			var message3 = Factory.New<ARLMessage>();
			message3.FillWithValidTestData();
			message3.EM_MessageSubType = ARLMessageTypes.Codes.StatementOfAccount;

			var message4 = Factory.New<ARLMessage>();
			message4.FillWithValidTestData();
			message4.EM_MessageSubType = ARLMessageTypes.Codes.DailyNotice;

			Factory.Save();

			var filter1 = new K84MessageFilterBusinessObject();
			var interchangeFilter1 = (ModuleTextFilter)filter1["Message Sub Type"];
			interchangeFilter1.Property = "DN";
			interchangeFilter1.SqlComparisonOperator = SQLComparisonOperator.Equal;
			interchangeFilter1.IsActive = true;

			var coll1 = new K84MessageCollection(Factory);
			coll1.Load(filter1.Filter);
			AssertEquals("one messages should have been loaded", 2, coll1.Count);

			var filter2 = new K84MessageFilterBusinessObject();
			var interchangeFilter2 = (ModuleTextFilter)filter2["Message Sub Type"];
			interchangeFilter2.Property = "SOA";
			interchangeFilter2.SqlComparisonOperator = SQLComparisonOperator.Equal;
			interchangeFilter2.IsActive = true;

			var coll = new K84MessageCollection(Factory);
			coll.Load(filter2.Filter);
			AssertEquals("one messages should have been loaded", 1, coll.Count);
		}

		public void TestGetAccountangAndStatementDateQuery()
		{
			var message1 = Factory.New<K84Message>();
			message1.FillWithValidTestData();
			message1.EM_MessageSubType = K84ReportTypes.Codes.Monthly;
			message1.SetSystemDefinedValue(EDIMessage.Schema.K84StatementDate, new ZDateTime(2014, 5, 5));

			var message2 = Factory.New<K84Message>();
			message2.FillWithValidTestData();
			message2.EM_MessageSubType = K84ReportTypes.Codes.Overdue;

			var message3 = Factory.New<K84Message>();
			message3.FillWithValidTestData();
			message3.EM_MessageSubType = K84ReportTypes.Codes.Daily;
			message3.SetSystemDefinedValue(EDIMessage.Schema.K84StatementDate, new ZDateTime(2014, 5, 5));
			message3.SetSystemDefinedValue(EDIMessage.Schema.K84AccountingDate, new ZDateTime(2014, 5, 4));

			var message4 = Factory.New<K84Message>();
			message4.FillWithValidTestData();
			message4.EM_MessageSubType = K84ReportTypes.Codes.Daily;
			message4.SetSystemDefinedValue(EDIMessage.Schema.K84StatementDate, new ZDateTime(2014, 5, 6));
			message4.SetSystemDefinedValue(EDIMessage.Schema.K84AccountingDate, new ZDateTime(2014, 5, 5));

			Factory.Save();

			var filter = new K84MessageFilterBusinessObject();
			var interchangeFilter = (ModuleDateFilter)filter[K84MessageFilterBusinessObject.Constants.K84AccountingDateID];
			interchangeFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			interchangeFilter.Property1 = new ZDateTime(2014, 5, 5, 0, 0, 0);
			interchangeFilter.Property2 = new ZDateTime(2014, 5, 5, 23, 59, 59);
			interchangeFilter.IsActive = true;

			var coll = new K84MessageCollection(Factory);
			coll.Load(filter.Filter);
			AssertEquals("one messages should have been loaded", 1, coll.Count);
			Assert("message 4 should be contained", coll.Contains(message4));

			interchangeFilter.Property1 = new ZDateTime(2014, 5, 4, 0, 0, 0);
			interchangeFilter.Property2 = new ZDateTime(2014, 5, 5, 23, 59, 59);
			coll.Load(filter.Filter);
			AssertEquals("two messages should have been loaded", 2, coll.Count);
			Assert("message 3 should be contained", coll.Contains(message3));
			Assert("message 4 should be contained", coll.Contains(message4));

			interchangeFilter.Property1 = ZDateTime.Empty;
			interchangeFilter.Property2 = ZDateTime.Empty;

			interchangeFilter = (ModuleDateFilter)filter[K84MessageFilterBusinessObject.Constants.K84StatementDateID];
			interchangeFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			interchangeFilter.Property1 = new ZDateTime(2014, 5, 5, 0, 0, 0);
			interchangeFilter.Property2 = new ZDateTime(2014, 5, 5, 23, 59, 59);
			interchangeFilter.IsActive = true;
			coll.Load(filter.Filter);
			AssertEquals("two messages should have been loaded", 2, coll.Count);
			Assert("message 1 should be contained", coll.Contains(message1));
			Assert("message 3 should be contained", coll.Contains(message3));
		}

		public void TestGetAccountangAndStatementDateQueryForARLMessage()
		{
			var message1 = Factory.New<ARLMessage>();
			message1.FillWithValidTestData();
			message1.EM_MessageSubType = ARLMessageTypes.Codes.DailyNotice;
			message1.SetSystemDefinedValue(EDIMessage.Schema.K84StatementDate, new ZDateTime(2014, 5, 5));
			message1.SetSystemDefinedValue(EDIMessage.Schema.K84AccountingDate, new ZDateTime(2014, 5, 6));

			var message2 = Factory.New<ARLMessage>();
			message2.FillWithValidTestData();
			message2.EM_MessageSubType = ARLMessageTypes.Codes.DailyNotice;
			message2.SetSystemDefinedValue(EDIMessage.Schema.K84StatementDate, new ZDateTime(2014, 5, 6));
			message2.SetSystemDefinedValue(EDIMessage.Schema.K84AccountingDate, new ZDateTime(2014, 5, 6));

			var message3 = Factory.New<ARLMessage>();
			message3.FillWithValidTestData();
			message3.EM_MessageSubType = ARLMessageTypes.Codes.DailyNotice;
			message3.SetSystemDefinedValue(EDIMessage.Schema.K84StatementDate, new ZDateTime(2014, 5, 5));
			message3.SetSystemDefinedValue(EDIMessage.Schema.K84AccountingDate, new ZDateTime(2014, 5, 4));

			var message4 = Factory.New<ARLMessage>();
			message4.FillWithValidTestData();
			message4.EM_MessageSubType = ARLMessageTypes.Codes.DailyNotice;
			message4.SetSystemDefinedValue(EDIMessage.Schema.K84StatementDate, new ZDateTime(2014, 5, 6));
			message4.SetSystemDefinedValue(EDIMessage.Schema.K84AccountingDate, new ZDateTime(2014, 5, 5));

			Factory.Save();

			var filter = new K84MessageFilterBusinessObject();
			var interchangeFilter = (ModuleDateFilter)filter[K84MessageFilterBusinessObject.Constants.K84AccountingDateID];
			interchangeFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			interchangeFilter.Property1 = new ZDateTime(2014, 5, 5, 0, 0, 0);
			interchangeFilter.Property2 = new ZDateTime(2014, 5, 5, 23, 59, 59);
			interchangeFilter.IsActive = true;

			var coll = new K84MessageCollection(Factory);
			coll.Load(filter.Filter);
			AssertEquals("one messages should have been loaded", 1, coll.Count);
			Assert("message 4 should be contained", coll.Contains(message4));

			interchangeFilter.Property1 = new ZDateTime(2014, 5, 4, 0, 0, 0);
			interchangeFilter.Property2 = new ZDateTime(2014, 5, 5, 23, 59, 59);
			coll.Load(filter.Filter);
			AssertEquals("two messages should have been loaded", 2, coll.Count);
			Assert("message 3 should be contained", coll.Contains(message3));
			Assert("message 4 should be contained", coll.Contains(message4));

			interchangeFilter.Property1 = ZDateTime.Empty;
			interchangeFilter.Property2 = ZDateTime.Empty;

			interchangeFilter = (ModuleDateFilter)filter[K84MessageFilterBusinessObject.Constants.K84StatementDateID];
			interchangeFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			interchangeFilter.Property1 = new ZDateTime(2014, 5, 5, 0, 0, 0);
			interchangeFilter.Property2 = new ZDateTime(2014, 5, 5, 23, 59, 59);
			interchangeFilter.IsActive = true;
			coll.Load(filter.Filter);
			AssertEquals("two messages should have been loaded", 2, coll.Count);
			Assert("message 1 should be contained", coll.Contains(message1));
			Assert("message 3 should be contained", coll.Contains(message3));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new K84MessageFilterBusinessObject();
	}
}
