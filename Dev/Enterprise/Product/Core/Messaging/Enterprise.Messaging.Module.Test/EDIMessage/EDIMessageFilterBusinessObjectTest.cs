using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Messaging.Module.Testing
{
	[TestedType(typeof(EDIMessageFilterBusinessObject))]
	class EDIMessageFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheckForCommonTables()
		{
			var result = new List<Tuple<string, string>>();
			// --- Both parameters are case sensitive
			result.Add(TableFilter("EDIInterchange", "Sender"));
			result.Add(TableFilter("EDIInterchange", "Receiver"));

			return result;
		}

		public void TestFilterCategories()
		{
			AssertCategory(EDIMessageFilterBusinessObject.Constants.Direction, FilterCategories.ModesAndTypes);
			AssertCategory(EDIMessageFilterBusinessObject.Constants.MessageType, FilterCategories.ModesAndTypes);
			AssertCategory(EDIMessageFilterBusinessObject.Constants.MessageSubType, FilterCategories.ModesAndTypes);
			AssertCategory(EDIMessageFilterBusinessObject.Constants.Status, FilterCategories.StatusAndFlags);
		}

		void AssertCategory(string filterColumn, FilterCategory expectedCategory)
		{
			var filter = new EDIMessageFilterBusinessObject();
			var interchangeFilter = (ModuleTextFilter)filter[filterColumn];
			AssertNotNull(interchangeFilter);
			AssertEquals(expectedCategory, interchangeFilter.Category);
		}

		public void TestGetInterchangeNumQuery()
		{
			EDIInterchange interchange = Factory.New<EDIInterchange>();
			interchange.EI_InterchangeNum = "~1";
			EDIMessage message1 = EDIMessageTestFactory.New(Factory);
			message1.FillWithValidTestData();
			message1.EM_EI = interchange.PK;

			EDIMessage message2 = EDIMessageTestFactory.New(Factory);
			message2.FillWithValidTestData();
			message2.EM_EI = interchange.PK;

			EDIInterchange interchange2 = Factory.New<EDIInterchange>();
			interchange2.EI_InterchangeNum = "~2";
			EDIMessage message3 = EDIMessageTestFactory.New(Factory);
			message3.FillWithValidTestData();
			message3.EM_EI = interchange2.PK;

			Factory.Save();

			EDIMessageFilterBusinessObject filter = new EDIMessageFilterBusinessObject();
			ModuleNumberFilter interchangeFilter = (ModuleNumberFilter)filter[EDIMessageFilterBusinessObject.Constants.InterchangeNumber];
			interchangeFilter.Property = "~1";
			interchangeFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			interchangeFilter.IsActive = true;

			NonDependentEDIMessageCollection coll = new NonDependentEDIMessageCollection(Factory);
			coll.Load(filter.Filter);
			AssertEquals("Two messages should have been loaded", 2, coll.Count);
			AssertEquals("message 1 should be contained", true, coll.Contains(message1));
			AssertEquals("message 2 should be contained", true, coll.Contains(message2));

			interchangeFilter.Property = "~";
			interchangeFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			coll = new NonDependentEDIMessageCollection(Factory);
			coll.Load(filter.Filter);
			AssertEquals("All three messages should have been loaded", 3, coll.Count);
			interchangeFilter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			coll = new NonDependentEDIMessageCollection(Factory);
			coll.Load(filter.Filter);
			AssertEquals("No messages should have been loaded", 0, coll.Count);
		}

		public void TestGetInterchangeDateTimeSentQuery()
		{
			EDIInterchange interchange = Factory.New<EDIInterchange>();
			interchange.EI_InterchangeNum = "~1";
			EDIMessage message1 = EDIMessageTestFactory.New(Factory);
			message1.FillWithValidTestData();
			message1.EM_EI = interchange.PK;

			EDIMessage message2 = EDIMessageTestFactory.New(Factory);
			message2.FillWithValidTestData();
			message2.EM_EI = interchange.PK;

			EDIInterchange interchange2 = Factory.New<EDIInterchange>();
			interchange2.EI_InterchangeNum = "~2";
			EDIMessage message3 = EDIMessageTestFactory.New(Factory);
			message3.FillWithValidTestData();
			message3.EM_EI = interchange2.PK;

			Factory.Save();

			var time1 = ZDateTime.Now;
			interchange.EI_SystemCreateTimeUtc = Env.Time.GetUtcFromLocalTime(time1.ToDateTime());

			var time2 = time1.AddHours(2);
			interchange2.EI_SystemCreateTimeUtc = Env.Time.GetUtcFromLocalTime(time2.ToDateTime());

			Factory.Save();

			EDIMessageFilterBusinessObject filter = new EDIMessageFilterBusinessObject();
			ModuleDateFilter interchangeFilter = (ModuleDateFilter)filter[EDIMessageFilterBusinessObject.Constants.InterchangeDateTimeSent];
			interchangeFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			interchangeFilter.Property1 = time1;
			interchangeFilter.Property2 = time1;
			interchangeFilter.IsActive = true;

			NonDependentEDIMessageCollection coll = new NonDependentEDIMessageCollection(Factory);
			coll.Load(filter.Filter);
			AssertEquals("two messages should have been loaded", 2, coll.Count);
			AssertEquals("message 1 should be contained", true, coll.Contains(message1));
			AssertEquals("message 2 should be contained", true, coll.Contains(message2));

			interchangeFilter.Property1 = time2;
			interchangeFilter.Property2 = time2;

			coll.Load(filter.Filter);
			AssertEquals("two messages should have been loaded", 1, coll.Count);
			AssertEquals("message 3 should be contained", true, coll.Contains(message3));
		}

		public void TestInterchangeDateTimeFilter_AppliesDateTimeCorrectly()
		{
			EDIInterchange interchange = Factory.New<EDIInterchange>();
			interchange.EI_InterchangeNum = "~1";
			EDIMessage message1 = EDIMessageTestFactory.New(Factory);
			message1.FillWithValidTestData();
			message1.EM_EI = interchange.PK;

			EDIInterchange interchange2 = Factory.New<EDIInterchange>();
			interchange2.EI_InterchangeNum = "~2";
			EDIMessage message2 = EDIMessageTestFactory.New(Factory);
			message2.FillWithValidTestData();
			message2.EM_EI = interchange2.PK;

			Factory.Save();

			var localTime1 = ZDateTime.Now;
			var localTime2 = localTime1.AddHours(2);

			interchange.EI_SystemCreateTimeUtc = Env.Time.GetUtcFromLocalTime(localTime1.ToDateTime());
			interchange2.EI_SystemCreateTimeUtc = Env.Time.GetUtcFromLocalTime(localTime2.ToDateTime());
			Factory.Save();

			EDIMessageFilterBusinessObject filter = new EDIMessageFilterBusinessObject();
			ModuleDateFilter interchangeFilter = (ModuleDateFilter)filter[EDIMessageFilterBusinessObject.Constants.InterchangeDateTimeSent];
			interchangeFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			interchangeFilter.Property1 = localTime1.AddHours(-1);
			interchangeFilter.Property2 = localTime1.AddHours(1);
			interchangeFilter.IsActive = true;

			NonDependentEDIMessageCollection coll = new NonDependentEDIMessageCollection(Factory);
			coll.Load(filter.Filter);
			AssertEquals("one messages should have been loaded", 1, coll.Count);
			AssertEquals("message 1 should be returned", true, coll.Contains(message1));
			AssertEquals("message 2 should not be returned", false, coll.Contains(message2));

			interchangeFilter.Property1 = localTime1.AddHours(-1);
			interchangeFilter.Property2 = localTime2.AddHours(1);

			coll.Load(filter.Filter);
			AssertEquals("two messages should have been loaded", 2, coll.Count);
			AssertEquals("message 2 should be returned", true, coll.Contains(message2));
		}

		public void TestGetStatusQuery()
		{
			EDIMessage message1 = EDIMessageTestFactory.New(Factory);
			message1.EM_Status = EDIMessage.Status.Acknowledged;

			EDIMessage message2 = EDIMessageTestFactory.New(Factory);
			message2.EM_Status = EDIMessage.Status.Cancelled;

			EDIMessage message3 = EDIMessageTestFactory.New(Factory);
			message3.EM_Status = EDIMessage.Status.Error;

			EDIMessage message4 = EDIMessageTestFactory.New(Factory);
			message4.EM_Status = EDIMessage.Status.Sent;

			EDIMessageFilterBusinessObject filter = new EDIMessageFilterBusinessObject();
			ModuleTextFilter statusFilter = (ModuleTextFilter)filter[EDIMessageFilterBusinessObject.Constants.Status];
			statusFilter.Property = EDIMessageFilterBusinessObject.Constants.AllExceptAcknowledgments;
			statusFilter.IsActive = true;

			AssertEquals("Filter should contain 18 options", 18, statusFilter.List.Count);
			String filterOptionList = "|";
			foreach (var fil in statusFilter.List)
			{
				filterOptionList += ((Enterprise.ZArchitecture.Core.CodeDescriptionPair)fil).Code + "|";
			}

			NonDependentEDIMessageCollection coll = new NonDependentEDIMessageCollection(Factory);
			coll.Load(filter.Filter);
			AssertEquals("message1 should be excluded from the collection", false, coll.Contains(message1));

			statusFilter.Property = EDIMessage.Status.Cancelled;
			coll.Load(filter.Filter);
			AssertEquals("message2 should be in the collection", true, coll.Contains(message2));

			statusFilter.Property = EDIMessage.Status.Sent;
			coll.Load(filter.Filter);
			AssertEquals("message1 should not be in the collection", false, coll.Contains(message1));
			AssertEquals("message2 should not be in the collection", false, coll.Contains(message2));
			AssertEquals("message3 should not be in the collection", false, coll.Contains(message3));
			AssertEquals("message4 should be in the collection", true, coll.Contains(message4));
		}

		public void TestFilterByMessageText()
		{
			TestCaseHelper.ClearTable(EDIMessage.Schema.TableName);

			var testInterchange1 = Factory.NewWithValidTestData<EDIInterchange>();
			testInterchange1.EI_ApplicationCode = EDIInterchange.ApplicationCodes.XMS;
			var testMessage1 = Factory.NewWithValidTestData<EDIMessage>();
			testMessage1.EM_EI = testInterchange1.PK;
			testMessage1.ForceDeprecatedNTextUsageForTesting = true;
			testMessage1.EM_MessageNText = "我们";

			var testInterchange2 = Factory.NewWithValidTestData<EDIInterchange>();
			testInterchange2.EI_ApplicationCode = EDIInterchange.ApplicationCodes.AirCargo;
			var testMessage2 = Factory.NewWithValidTestData<EDIMessage>();
			testMessage2.EM_EI = testInterchange2.PK;
			testMessage2.EM_MessageText = "us";
			Factory.Save();

			EDIMessageFilterBusinessObject messageFilterBusinessObject = new EDIMessageFilterBusinessObject();
			EDIMessageTextFilter filter = (EDIMessageTextFilter)messageFilterBusinessObject["Message Text"];

			AssertFilterByMessageText(filter, ModuleTextFilter.ComparisonConstants.StartsWith, "我们", 1, testMessage1.PK);
			AssertFilterByMessageText(filter, ModuleTextFilter.ComparisonConstants.StartsWith, "us", 1, testMessage2.PK);
			AssertFilterByMessageText(filter, ModuleTextFilter.ComparisonConstants.NotStartsWith, "我们", 1, testMessage2.PK);
			AssertFilterByMessageText(filter, ModuleTextFilter.ComparisonConstants.NotStartsWith, "us", 1, testMessage1.PK);

			AssertFilterByMessageText(filter, ModuleTextFilter.ComparisonConstants.Exact, "我们", 1, testMessage1.PK);
			AssertFilterByMessageText(filter, ModuleTextFilter.ComparisonConstants.Exact, "us", 1, testMessage2.PK);
			AssertFilterByMessageText(filter, ModuleTextFilter.ComparisonConstants.NotEqual, "我们", 1, testMessage2.PK);
			AssertFilterByMessageText(filter, ModuleTextFilter.ComparisonConstants.NotEqual, "us", 1, testMessage1.PK);

			AssertFilterByMessageText(filter, ModuleTextFilter.ComparisonConstants.Contains, "我们", 1, testMessage1.PK);
			AssertFilterByMessageText(filter, ModuleTextFilter.ComparisonConstants.Contains, "us", 1, testMessage2.PK);
			AssertFilterByMessageText(filter, ModuleTextFilter.ComparisonConstants.NotContain, "我们", 1, testMessage2.PK);
			AssertFilterByMessageText(filter, ModuleTextFilter.ComparisonConstants.NotContain, "us", 1, testMessage1.PK);

			AssertFilterByMessageText(filter, ModuleTextFilter.ComparisonConstants.IsBlank, "", 0);
			AssertFilterByMessageText(filter, ModuleTextFilter.ComparisonConstants.IsNotBlank, "", 2);
		}

		public void TestFilterBySender()
		{
			EDIMessage message1;
			EDIMessage message2;
			EDIMessage message3;
			EDIMessage message4;
			CreateMessages(out message1, out message2, out message3, out message4);

			EDIMessageFilterBusinessObject filterBusinessObject = new EDIMessageFilterBusinessObject();
			ModuleTextFilter filter = (ModuleTextFilter)filterBusinessObject[EDIMessageFilterBusinessObject.Constants.Sender];
			{
				filter.Property = "EDIEDITST";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				EDIMessage[] messages = Factory.Load<EDIMessage>(filter.Query);
				AssertEquals("Sender Exact 'EDIEDITST' should have 2 messages", 2, messages.Length);
				Assert("Sender Exact 'EDIEDITST' should contain message1", messages.Contains<EDIMessage>(message1));
				Assert("Sender Exact 'EDIEDITST' should contain message2", messages.Contains<EDIMessage>(message2));
			}

			{
				filter.Property = "EDI";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				EDIMessage[] messages = Factory.Load<EDIMessage>(filter.Query);
				AssertEquals("Sender StartsWith 'EDI' should have 2 messages", 2, messages.Length);
				Assert("Sender StartsWith 'EDI' should contain message1", messages.Contains<EDIMessage>(message1));
				Assert("Sender StartsWith 'EDI' should contain message2", messages.Contains<EDIMessage>(message2));
			}

			{
				filter.Property = "EDI";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
				EDIMessage[] messages = Factory.Load<EDIMessage>(filter.Query);
				AssertEquals("Sender Contains 'EDI' should have 3 messages", 3, messages.Length);
				Assert("Sender Contains 'EDI' should contain message1", messages.Contains<EDIMessage>(message1));
				Assert("Sender Contains 'EDI' should contain message2", messages.Contains<EDIMessage>(message2));
				Assert("Sender Contains 'EDI' should contain message3", messages.Contains<EDIMessage>(message3));
			}

			{
				filter.Property = "EDIEDITST";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
				EDIMessage[] messages = Factory.Load<EDIMessage>(filter.Query);
				AssertEquals("Sender NotEqual 'EDIEDITST' should have 2 messages", 2, messages.Length);
				Assert("Sender NotEqual 'EDIEDITST' should contain message3", messages.Contains<EDIMessage>(message3));
				Assert("Sender NotEqual 'EDIEDITST' should contain message4", messages.Contains<EDIMessage>(message4));
			}

			{
				filter.Property = "EDI";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
				EDIMessage[] messages = Factory.Load<EDIMessage>(filter.Query);
				AssertEquals("Sender NotStartsWith 'EDI' should have 2 messages", 2, messages.Length);
				Assert("Sender NotStartsWith 'EDI' should contain message3", messages.Contains<EDIMessage>(message3));
				Assert("Sender NotStartsWith 'EDI' should contain message4", messages.Contains<EDIMessage>(message4));
			}

			{
				filter.Property = "EDI";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
				EDIMessage[] messages = Factory.Load<EDIMessage>(filter.Query);
				AssertEquals("Sender NotContain 'EDI' should have 1 message", 1, messages.Length);
				Assert("Sender NotContain 'EDI' should contain message4", messages.Contains<EDIMessage>(message4));
			}

			{
				filter.Property = "";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
				EDIMessage[] messages = Factory.Load<EDIMessage>(filter.Query);
				AssertEquals("Sender IsBlank should have 1 message", 1, messages.Length);
				Assert("Sender IsBlank should contain message4", messages.Contains<EDIMessage>(message4));
			}

			{
				filter.Property = "";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
				EDIMessage[] messages = Factory.Load<EDIMessage>(filter.Query);
				AssertEquals("Sender IsNotBlank should have 3 messages", 3, messages.Length);
				Assert("Sender IsNotBlank should contain message1", messages.Contains<EDIMessage>(message1));
				Assert("Sender IsNotBlank should contain message2", messages.Contains<EDIMessage>(message2));
				Assert("Sender IsNotBlank should contain message3", messages.Contains<EDIMessage>(message3));
			}
		}

		public void TestFilterByReceiver()
		{
			EDIMessage message1;
			EDIMessage message2;
			EDIMessage message3;
			EDIMessage message4;
			CreateMessages(out message1, out message2, out message3, out message4);

			EDIMessageFilterBusinessObject filterBusinessObject = new EDIMessageFilterBusinessObject();
			ModuleTextFilter filter = (ModuleTextFilter)filterBusinessObject[EDIMessageFilterBusinessObject.Constants.Receiver];
			{
				filter.Property = "EDIEDITST";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				EDIMessage[] messages = Factory.Load<EDIMessage>(filter.Query);
				AssertEquals("Receiver Exact 'EDIEDITST' should have 1 message", 1, messages.Length);
				Assert("Receiver Exact 'EDIEDITST' should contain message1", messages.Contains<EDIMessage>(message3));
			}

			{
				filter.Property = "EDI";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				EDIMessage[] messages = Factory.Load<EDIMessage>(filter.Query);
				AssertEquals("Receiver StartsWith 'EDI' should have 1 message", 1, messages.Length);
				Assert("Receiver StartsWith 'EDI' should contain message1", messages.Contains<EDIMessage>(message3));
			}

			{
				filter.Property = "EDI";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
				EDIMessage[] messages = Factory.Load<EDIMessage>(filter.Query);
				AssertEquals("Receiver Contains 'EDI' should have 3 messages", 3, messages.Length);
				Assert("Receiver Contains 'EDI' should contain message1", messages.Contains<EDIMessage>(message1));
				Assert("Receiver Contains 'EDI' should contain message2", messages.Contains<EDIMessage>(message2));
				Assert("Receiver Contains 'EDI' should contain message3", messages.Contains<EDIMessage>(message3));
			}

			{
				filter.Property = "EDIEDITST";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
				EDIMessage[] messages = Factory.Load<EDIMessage>(filter.Query);
				AssertEquals("Receiver NotEqual 'EDIEDITST' should have 3 messages", 3, messages.Length);
				Assert("Receiver NotEqual 'EDIEDITST' should contain message1", messages.Contains<EDIMessage>(message1));
				Assert("Receiver NotEqual 'EDIEDITST' should contain message2", messages.Contains<EDIMessage>(message2));
				Assert("Receiver NotEqual 'EDIEDITST' should contain message4", messages.Contains<EDIMessage>(message4));
			}

			{
				filter.Property = "EDI";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
				EDIMessage[] messages = Factory.Load<EDIMessage>(filter.Query);
				AssertEquals("Receiver NotStartsWith 'EDI' should have 3 messages", 3, messages.Length);
				Assert("Receiver NotStartsWith 'EDI' should contain message1", messages.Contains<EDIMessage>(message1));
				Assert("Receiver NotStartsWith 'EDI' should contain message2", messages.Contains<EDIMessage>(message2));
				Assert("Receiver NotStartsWith 'EDI' should contain message4", messages.Contains<EDIMessage>(message4));
			}

			{
				filter.Property = "EDI";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
				EDIMessage[] messages = Factory.Load<EDIMessage>(filter.Query);
				AssertEquals("Sender NotContain 'EDI' should have 1 message", 1, messages.Length);
				Assert("Receiver NotContain 'EDI' should contain message4", messages.Contains<EDIMessage>(message4));
			}

			{
				filter.Property = "";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
				EDIMessage[] messages = Factory.Load<EDIMessage>(filter.Query);
				AssertEquals("Receiver IsBlank should have 1 message", 1, messages.Length);
				Assert("Receiver IsBlank should contain message4", messages.Contains<EDIMessage>(message4));
			}

			{
				filter.Property = "";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
				EDIMessage[] messages = Factory.Load<EDIMessage>(filter.Query);
				AssertEquals("Receiver IsNotBlank should have 3 messages", 3, messages.Length);
				Assert("Receiver IsNotBlank should contain message1", messages.Contains<EDIMessage>(message1));
				Assert("Receiver IsNotBlank should contain message2", messages.Contains<EDIMessage>(message2));
				Assert("Receiver IsNotBlank should contain message3", messages.Contains<EDIMessage>(message3));
			}
		}

		public void TestUpdateComparisionOperator()
		{
			EDIMessageFilterBusinessObject filterBusinessObject = new EDIMessageFilterBusinessObject();
			ModuleTextFilter filter = new ModuleFilterCollection().AddNumberFilter("Test", EDIMessageSchema.EM_MessageNum);

			AssertEquals("PRE: There are 8 comparison operators originally", 8, filter.ComparisonOperator_List.Count);

			filterBusinessObject.UpdateComparisionOperator(filter);

			AssertEquals("There should be only 1 comparison operators after update", 1, filter.ComparisonOperator_List.Count);
			Assert("Comparison operators", filter.ComparisonOperator_List.ContainsOnly("exact"));
		}

		public void TestFilterByEHubID()
		{
			CreateMessages(out var message1, out var message2, out _, out _);

			var filterBusinessObject = new EDIMessageFilterBusinessObject();
			var filter = (EDIInterchangeEHubIdFilter)filterBusinessObject[EDIMessageFilterBusinessObject.Constants.EHubID];
			var subGroup = (ModuleFilterSubGroup)filter.SubGroup;
			{
				filter.Property = "C7E2E1A4-F26A-48F7-9FDB-FA9ADB3FBDB2";
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				EDIMessage[] messages = Factory.Load<EDIMessage>(subGroup.GetSubQuery(filter.Query));
				AssertEquals("eHub ID Exact 'C7E2E1A4-F26A-48F7-9FDB-FA9ADB3FBDB2' should have 2 messages", 2,
					messages.Length);
				Assert("eHub ID Exact 'C7E2E1A4-F26A-48F7-9FDB-FA9ADB3FBDB2' should contain message1",
					messages.Contains<EDIMessage>(message1));
				Assert("eHub ID Exact 'C7E2E1A4-F26A-48F7-9FDB-FA9ADB3FBDB2' should contain message2",
					messages.Contains<EDIMessage>(message2));
			}
		}

		public void TestHeldUntilDate()
		{
			var message1 = EDIMessageTestFactory.New(Factory);
			var message2 = EDIMessageTestFactory.New(Factory);

			var time1 = ZDateTime.Now;
			message1.EM_HeldUntilDate = Env.Time.GetUtcFromLocalTime(time1.ToDateTime());
			var time2 = time1.AddHours(2);
			message2.EM_HeldUntilDate = Env.Time.GetUtcFromLocalTime(time2.ToDateTime());

			var filter = new EDIMessageFilterBusinessObject();
			var dateFilter = (ModuleDateFilter)filter[EDIMessageFilterBusinessObject.Constants.HeldUntilDate];
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			dateFilter.Property1 = time1.AddHours(-1);
			dateFilter.Property2 = time1.AddHours(1);
			dateFilter.IsActive = true;

			var queryCollection = new NonDependentEDIMessageCollection(Factory);
			queryCollection.Load(filter.Filter);
			AssertEquals("one messages should have been loaded", 1, queryCollection.Count);
			AssertEquals("message 1 should be returned", true, queryCollection.Contains(message1));
			AssertEquals("message 2 should not be returned", false, queryCollection.Contains(message2));

			dateFilter.Property1 = time1.AddHours(-1);
			dateFilter.Property2 = time2.AddHours(1);
			queryCollection.Load(filter.Filter);
			AssertEquals("two messages should have been loaded", 2, queryCollection.Count);
			AssertEquals("message 2 should be returned", true, queryCollection.Contains(message2));
		}

		public void TestFilterByExternalReferenceNumber()
		{
			CreateMessages(out var message1, out var message2, out _, out _);
			message1.EM_ExternalReferenceNumber = "8a64619e-9a9b-40b8-aa52-330511287dc6";
			message2.EM_ExternalReferenceNumber = "9d2f231f-c006-49c3-8928-94823fe7b6d5";

			var filterBusinessObject = new EDIMessageFilterBusinessObject();
			var filter = (ModuleNumberFilter)filterBusinessObject[EDIMessageFilterBusinessObject.Constants.ExternalReferenceNumber];

			AssertFilterByMessageText(filter, ModuleTextFilter.ComparisonConstants.Exact, "8a64619e-9a9b-40b8-aa52-330511287dc6", 1, message1.PK);
		}

		void AssertFilterByMessageText(ModuleTextFilter filter, string comparisonOperator, string property, int expectedMessageNumber, ZGuid expectedMessagePK = default(ZGuid))
		{
			filter.ComparisonOperator = comparisonOperator;
			filter.Property = property;
			EDIMessage[] messages;
			var filterSubGroup = filter.SubGroup;

			if (filterSubGroup != null)
			{
				var subGroup = (ModuleFilterSubGroup)filterSubGroup;
				messages = Factory.Load<EDIMessage>(subGroup.GetSubQuery(filter.Query));
			}
			else
			{
				messages = Factory.Load<EDIMessage>(filter.Query);
			}

			var assertMessageNumberError = string.Format("{0} '{1}'", comparisonOperator, property);
			AssertEquals(assertMessageNumberError, expectedMessageNumber, messages.Length);

			var assertMessagePKError = assertMessageNumberError + " - messagePK";
			if (expectedMessagePK != default(ZGuid))
			{
				AssertEquals(assertMessagePKError, expectedMessagePK, messages[0].PK);
			}
		}

		public void TestBaseFilter_SystemMessages()
		{
			UserForTest user = new UserForTest();
			UserContext context = new UserContextForTest(user, Env.CurrentCompany);

			EDIMessage message1 = EDIMessageTestFactory.New(Factory);
			message1.EM_ApplicationCode = ApplicationCodeList.Codes.SYS;

			EDIMessage message2 = EDIMessageTestFactory.New(Factory);
			message2.EM_ApplicationCode = ApplicationCodeList.Codes.XDS;

			EDIMessageFilterBusinessObject filter = new EDIMessageFilterBusinessObject();
			NonDependentEDIMessageCollection coll = new NonDependentEDIMessageCollection(Factory);

			user.LoggedInWithMasterPassword = false;
			using (Env.SetTemporaryUserContext(context))
			{
				coll.Load(filter.Filter);
				AssertEquals("message1 excluded", false, coll.Contains(message1));
				AssertEquals("message2 included", true, coll.Contains(message2));
			}

			user.LoggedInWithMasterPassword = true;
			using (Env.SetTemporaryUserContext(context))
			{
				filter.ModuleFilters.InvalidateCachedQuery();
				coll.Load(filter.Filter);
				AssertEquals("message1 included", true, coll.Contains(message1));
				AssertEquals("message2 included", true, coll.Contains(message2));
			}
		}

		void CreateMessages(out EDIMessage message1, out EDIMessage message2, out EDIMessage message3, out EDIMessage message4)
		{
			var testInterchange1 = Factory.NewWithValidTestData<EDIInterchange>();
			testInterchange1.EI_SessionGUID = new Guid("C7E2E1A4-F26A-48F7-9FDB-FA9ADB3FBDB2");
			testInterchange1.EI_From = "EDIEDITST";
			testInterchange1.EI_To = "TSTEDIEDI";

			message1 = Factory.NewWithValidTestData<EDIMessage>();
			message1.EM_EI = testInterchange1.PK;

			message2 = Factory.NewWithValidTestData<EDIMessage>();
			message2.EM_EI = testInterchange1.PK;

			var testInterchange2 = Factory.NewWithValidTestData<EDIInterchange>();
			testInterchange2.EI_SessionGUID = new Guid("C7E2E1A4-F26A-48F7-9FDB-FA9ADB3FBDa0");
			testInterchange2.EI_From = "TSTEDIEDI";
			testInterchange2.EI_To = "EDIEDITST";

			message3 = Factory.NewWithValidTestData<EDIMessage>();
			message3.EM_EI = testInterchange2.PK;

			message4 = Factory.NewWithValidTestData<EDIMessage>();

			Factory.Save();
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new EDIMessageFilterBusinessObject();
		}

		#endregion
	}
}
