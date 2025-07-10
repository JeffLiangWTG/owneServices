using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using static Enterprise.Billing.Business.UsageCollector;

namespace Enterprise.Billing.Business.Testing
{
	public sealed class UsageCollectorTest : TestCaseWithFactory
	{
		readonly DisposableList disposables = new DisposableList(0);

		static IDisposable OverrideFeaturePropertiesForTest(IEnumerable<UsageFeatureProperties> featurePropertiesForTest)
		{
			lock (featuresLockObj)
			{
				FeaturePropertiesForTest = featurePropertiesForTest;
				features = null;
			}

			return new DisposableAction(() =>
			{
				lock (featuresLockObj)
				{
					FeaturePropertiesForTest = null;
					features = null;
				}
			});
		}

		public static void ClearFeaturesForTest()
		{
			lock (featuresLockObj)
			{
				features = null;
			}
		}

		IEnumerable<UsageFeatureProperties> GetFeaturePropertiesForTest()
		{
			yield return new UsageFeatureProperties(UsageFeaturesForTest.Usages, "UnitTest", "This feature is for unit testing only");
			yield return new UsageFeatureProperties(UsageFeaturesForTest.SummarisedUsages, "UnitTestSummarised", "This feature is for unit testing only", produceDailySummaries: true);
			yield return new UsageFeatureProperties(UsageFeaturesForTest.BillableMappedFields, "BillableMappedFields", "Billable Feature Code with Mapped Fields");
			yield return new UsageFeatureProperties(UsageFeaturesForTest.BillableDefaultFields, "BillableDefaultFields", "Billable Feature Code with Default Fields");
		}

		public static class UsageFeaturesForTest
		{
			public const string Usages = "TST";
			public const string SummarisedUsages = "TSS";
			public const string BillableMappedFields = "BMF";
			public const string BillableDefaultFields = "BDF";
		}

		protected override void SetUp()
		{
			var billableDefaultFieldsMappings = Factory.New<RefStlFieldMapping>();
			billableDefaultFieldsMappings.SFM_FeatureCode = UsageFeaturesForTest.BillableDefaultFields;
			var billableMappedFieldsMappings = Factory.New<RefStlFieldMapping>();
			billableMappedFieldsMappings.SFM_FeatureCode = UsageFeaturesForTest.BillableMappedFields;
			billableMappedFieldsMappings.SFM_Category = "BMF";
			billableMappedFieldsMappings.SFM_BillableCount = "ReportedCount";
			billableMappedFieldsMappings.SFM_Reference1 = "Reported1";
			billableMappedFieldsMappings.SFM_Reference2 = "Reported2";
			billableMappedFieldsMappings.SFM_Reference3 = "Reported3";
			billableMappedFieldsMappings.SFM_Reference4 = "Reported4";
			billableMappedFieldsMappings.SFM_Reference5 = "Reported5";
			billableMappedFieldsMappings.SFM_ClientStaffCode = "ReportedUser";
			billableMappedFieldsMappings.SFM_ServiceOccuredUTC = "SystemCreateTime";
			billableMappedFieldsMappings.SFM_PriceItemCode = "ReportedCode";
			Factory.Save();

			disposables.Add(OverrideFeaturePropertiesForTest(GetFeaturePropertiesForTest()));
			base.SetUp();
		}

		protected override void TearDown()
		{
			disposables.Dispose();
			base.TearDown();
		}

		[TestDate(2021, 1, 1, 1, 1, 1)]
		public void TestReport()
		{
			UsageCollector.Report(UsageFeaturesForTest.Usages, ("Property1", "Val1"), ("Property2", 1L), ("Property3", new[] { 1, 2, 3 }));
			var messages = Helper.LoadUsageMessages();
			AssertEquals("Wrong number of messages loaded after being reported", 1, messages.Length);
			var reportedMessage = messages[0];
			AssertReportedMessageProperties(reportedMessage, ("Property1", "Val1"), ("Property2", 1L), ("Property3", new JArray { 1, 2, 3 }));
			AssertEquals(TestFeatureCommonPropsJSON + ",\r\n  \"Property1\": \"Val1\",\r\n  \"Property2\": 1,\r\n  \"Property3\": [\r\n    1,\r\n    2,\r\n    3\r\n  ]\r\n}", reportedMessage.EM_MessageText);
		}

		[TestDate(2021, 1, 1, 1, 1, 1)]
		public void TestReport_Branch()
		{
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var company = factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "SIN"));
			var branch = company.Branches[0];
			UsageCollector.Report(factory, "TST", branch, ("Property1", "Val1"), ("Property2", 1L), ("Property3", new[] { 1, 2, 3 }));

			var helper = new UsageCollectorTestHelper(factory);
			var messages = helper.LoadUsageMessages();
			AssertEquals("Wrong number of messages loaded after being reported", 1, messages.Length);
			var reportedMessage = messages[0];
			AssertReportedMessagePropertiesSIN(branch, reportedMessage, ("Property1", "Val1"), ("Property2", 1L), ("Property3", new JArray { 1, 2, 3 }));
			AssertEquals("{\r\n  \"FeatureCode\": \"TST\",\r\n  \"Module\": \"UnitTest\",\r\n  \"FeatureDescription\": \"This feature is for unit testing only\",\r\n  \"OrganisationName\": \"Singapore Co\",\r\n  \"Property1\": \"Val1\",\r\n  \"Property2\": 1,\r\n  \"Property3\": [\r\n    1,\r\n    2,\r\n    3\r\n  ]\r\n}", reportedMessage.EM_MessageText);
		}

		const string TestFeatureCommonPropsJSON = "{\r\n  \"FeatureCode\": \"TST\",\r\n  \"Module\": \"UnitTest\",\r\n  \"FeatureDescription\": \"This feature is for unit testing only\",\r\n  \"OrganisationName\": \"EDI CUSTOMS BROKERS\"";
		const string TestFeatureCommonPropsSummarisedJSON = "{\r\n  \"FeatureCode\": \"TSS\",\r\n  \"Module\": \"UnitTestSummarised\",\r\n  \"FeatureDescription\": \"This feature is for unit testing only\",\r\n  \"OrganisationName\": \"EDI CUSTOMS BROKERS\"";

		[TestDate(2021, 1, 1, 1, 1, 1)]
		public void TestReportListOfPropertyTuples()
		{
			var properties = new List<(string name, object value)>();
			var innerProperties = new List<(string name, object value)>();
			innerProperties.Add(("InnerProp1", "Val2"));
			innerProperties.Add(("InnerProp2", 57L));
			properties.Add(("Prop1", "Val1"));
			properties.Add(("Prop2", 1L));
			properties.Add(("Prop3", innerProperties));
			UsageCollector.Report(UsageFeaturesForTest.Usages, ("MyList", properties));
			var messages = Helper.LoadUsageMessages();
			AssertEquals("Wrong number of messages loaded after being reported", 1, messages.Length);
			var reportedMessage = messages[0];
			AssertEquals(TestFeatureCommonPropsJSON + ",\r\n  \"MyList\": {\r\n    \"Prop1\": \"Val1\",\r\n    \"Prop2\": 1,\r\n    \"Prop3\": {\r\n      \"InnerProp1\": \"Val2\",\r\n      \"InnerProp2\": 57\r\n    }\r\n  }\r\n}", reportedMessage.EM_MessageText);
		}

		[TestDate(2021, 1, 1, 1, 1, 1)]
		public void TestReportArrayOfPropertyTuples()
		{
			var properties = new (string name, object value)[3];
			var innerProperties = new (string name, object value)[2];
			innerProperties[0] = ("InnerProp1", "Val2");
			innerProperties[1] = ("InnerProp2", 57L);
			properties[0] = ("Prop1", "Val1");
			properties[1] = ("Prop2", 1L);
			properties[2] = ("Prop3", innerProperties);
			UsageCollector.Report(UsageFeaturesForTest.Usages, ("MyArray", properties));
			var messages = Helper.LoadUsageMessages();
			AssertEquals("Wrong number of messages loaded after being reported", 1, messages.Length);
			var reportedMessage = messages[0];
			AssertEquals(TestFeatureCommonPropsJSON + ",\r\n  \"MyArray\": {\r\n    \"Prop1\": \"Val1\",\r\n    \"Prop2\": 1,\r\n    \"Prop3\": {\r\n      \"InnerProp1\": \"Val2\",\r\n      \"InnerProp2\": 57\r\n    }\r\n  }\r\n}", reportedMessage.EM_MessageText);
		}

		[TestDate(2021, 1, 1, 1, 1, 1)]
		public void TestReportListOfPropertyKeyValuePairs()
		{
			var properties = new List<KeyValuePair<string, object>>();
			var innerProperties = new List<KeyValuePair<string, object>>();
			innerProperties.Add(new KeyValuePair<string, object>("InnerProp1", "Val2"));
			innerProperties.Add(new KeyValuePair<string, object>("InnerProp2", 57L));
			properties.Add(new KeyValuePair<string, object>("Prop1", "Val1"));
			properties.Add(new KeyValuePair<string, object>("Prop2", 1L));
			properties.Add(new KeyValuePair<string, object>("Prop3", innerProperties));
			UsageCollector.Report(UsageFeaturesForTest.Usages, ("MyListKeyPairs", properties));
			var messages = Helper.LoadUsageMessages();
			AssertEquals("Wrong number of messages loaded after being reported", 1, messages.Length);
			var reportedMessage = messages[0];
			AssertEquals(TestFeatureCommonPropsJSON + ",\r\n  \"MyListKeyPairs\": {\r\n    \"Prop1\": \"Val1\",\r\n    \"Prop2\": 1,\r\n    \"Prop3\": {\r\n      \"InnerProp1\": \"Val2\",\r\n      \"InnerProp2\": 57\r\n    }\r\n  }\r\n}", reportedMessage.EM_MessageText);
		}

		public void TestScope()
		{
			UsageCollector.Report(UsageFeaturesForTest.Usages, ("ReportLocation", "BeforeScope"));

			using (UsageCollector.Scope(("OuterScope", "OuterScopeVal")))
			{
				UsageCollector.Report(UsageFeaturesForTest.Usages, ("ReportLocation", "BeginOuterScope"));

				using (UsageCollector.Scope(("InnerScope", "InnerScopeVal")))
				{
					UsageCollector.Report(UsageFeaturesForTest.Usages, ("ReportLocation", "InnerScope"));
				}

				UsageCollector.Report(UsageFeaturesForTest.Usages, ("ReportLocation", "EndOuterScope"));
			}

			UsageCollector.Report(UsageFeaturesForTest.Usages, ("ReportLocation", "EndScope"));

			var messages = Helper.LoadUsageMessages();
			AssertEquals("Wrong number of messages loaded after being reported", 5, messages.Length);

			var currentReport = messages.First(m => m.UsageProperties.Properties().Any(p => p.Name.Equals("ReportLocation") && p.Value.ToString().Equals("BeforeScope")));
			AssertReportedMessageProperties(currentReport, ("ReportLocation", "BeforeScope"));
			currentReport = messages.First(m => m.UsageProperties.Properties().Any(p => p.Name.Equals("ReportLocation") && p.Value.ToString().Equals("BeginOuterScope")));
			AssertReportedMessageProperties(currentReport, ("ReportLocation", "BeginOuterScope"), ("OuterScope", "OuterScopeVal"));
			currentReport = messages.First(m => m.UsageProperties.Properties().Any(p => p.Name.Equals("ReportLocation") && p.Value.ToString().Equals("InnerScope")));
			AssertReportedMessageProperties(currentReport, ("ReportLocation", "InnerScope"), ("OuterScope", "OuterScopeVal"), ("InnerScope", "InnerScopeVal"));
			currentReport = messages.First(m => m.UsageProperties.Properties().Any(p => p.Name.Equals("ReportLocation") && p.Value.ToString().Equals("EndOuterScope")));
			AssertReportedMessageProperties(currentReport, ("ReportLocation", "EndOuterScope"), ("OuterScope", "OuterScopeVal"));
			currentReport = messages.First(m => m.UsageProperties.Properties().Any(p => p.Name.Equals("ReportLocation") && p.Value.ToString().Equals("EndScope")));
			AssertReportedMessageProperties(currentReport, ("ReportLocation", "EndScope"));
		}

		public void TestReportPropertyOverridesScope()
		{
			using (UsageCollector.Scope(("OverridenProperty", "ScopeVal")))
			{
				UsageCollector.Report(UsageFeaturesForTest.Usages, ("OverridenProperty", "ReportVal"));
			}

			var messages = Helper.LoadUsageMessages();
			AssertEquals("Wrong number of messages loaded after being reported", 1, messages.Length);
			AssertReportedMessageProperties(messages[0], ("OverridenProperty", "ReportVal"));
		}

		public void TestReportFeatureCodeNotInList()
		{
			AssertExceptionThrown<ArgumentException>(() => UsageCollector.Report("XXX"));
		}

		public void TestReportPropertiesWithDefaultValuesAreOmited()
		{
			using (UsageCollector.Scope(("MyScope", "ScopeVal")))
			{
				var propsWithSomeOmitted = new (string name, object value)[2];
				propsWithSomeOmitted[0] = ("Empty", "");
				propsWithSomeOmitted[1] = ("NotEmpty", "NotEmptyVal");
				var propsAllOmitted = new (string name, object value)[2];
				propsAllOmitted[0] = ("Zero", 0);
				UsageCollector.Report(UsageFeaturesForTest.Usages, ("Null", null), ("Empty", string.Empty), ("ZeroInt", 0), ("ZeroDouble", 0.0), ("BoolFalse", false), ("PropsWithSomeOmitted", propsWithSomeOmitted), ("PropsAllOmitted", propsAllOmitted));

				var expectedPropsWithSomeOmitted = new JObject();
				expectedPropsWithSomeOmitted.Add("NotEmpty", "NotEmptyVal");
				var messages = Helper.LoadUsageMessages();
				AssertEquals("Wrong number of messages loaded after being reported", 1, messages.Length);
				AssertReportedMessageProperties(messages[0], ("MyScope", "ScopeVal"), ("PropsWithSomeOmitted", expectedPropsWithSomeOmitted));
			}
		}

		public void TestReportPropertiesWithDefaultValuesAreOmitedFromCustomObjects()
		{
			using (UsageCollector.Scope(("MyScope", "ScopeVal")))
			{
				UsageCollector.Report(UsageFeaturesForTest.Usages, ("CustomAllOmitted", new CustomObjectForTest()), ("CustomSomeOmitted", new CustomObjectForTest() { SomeInt = 5 }));

				var expectedPropsWithSomeOmitted = new JObject();
				expectedPropsWithSomeOmitted.Add("SomeInt", 5);
				var messages = Helper.LoadUsageMessages();
				AssertEquals("Wrong number of messages loaded after being reported", 1, messages.Length);
				AssertReportedMessageProperties(messages[0], ("MyScope", "ScopeVal"), ("CustomSomeOmitted", expectedPropsWithSomeOmitted));
			}
		}

		public void TestReportPropertiesWithDefaultValuesAreOmitedFromArraysOfCustomObjects()
		{
			using (UsageCollector.Scope(("MyScope", "ScopeVal")))
			{
				UsageCollector.Report(
					UsageFeaturesForTest.Usages,
					("CustomArray", new CustomObjectForTest[] { new CustomObjectForTest(), new CustomObjectForTest() { SomeInt = 5 } }),
					("EmptyCustomArray", new CustomObjectForTest[] { new CustomObjectForTest() }));

				var expectedPropsWithSomeOmitted = new JObject();
				expectedPropsWithSomeOmitted.Add("SomeInt", 5);
				var expectedCustomArray = new JArray();
				expectedCustomArray.Add(expectedPropsWithSomeOmitted);
				var messages = Helper.LoadUsageMessages();
				AssertEquals("Wrong number of messages loaded after being reported", 1, messages.Length);
				AssertReportedMessageProperties(messages[0], ("MyScope", "ScopeVal"), ("CustomArray", expectedCustomArray));
			}
		}

		public void TestReportSummarisedUsageRecords()
		{
			UsageCollector.Report(UsageFeaturesForTest.SummarisedUsages, ("Property1", "Val1"));
			var messages = Helper.LoadUsageSummaryMessages();
			AssertEquals("Wrong number of messages loaded after being reported", 1, messages.Length);
			var reportedMessage = messages[0];
			var usageFeatureProperties = new UsageFeatureProperties(UsageFeaturesForTest.SummarisedUsages, "UnitTestSummarised", "This feature is for unit testing only", produceDailySummaries: true);
			AssertReportedMessageProperties(reportedMessage, usageFeatureProperties, ("Property1", "Val1"));
			AssertEquals(TestFeatureCommonPropsSummarisedJSON + ",\r\n  \"Property1\": \"Val1\"\r\n}", reportedMessage.EM_MessageText);
		}

		public void TestReportThrowsIfPropertiesClashWithGeneric()
		{
			AssertExceptionThrown<ArgumentOutOfRangeException>($"We shouldn't allow a property to be reported that clashes with property = 'FeatureCode'", () => UsageCollector.Report(UsageFeaturesForTest.Usages, ("FeatureCode", "Value")));
			AssertExceptionThrown<ArgumentOutOfRangeException>($"We shouldn't allow a property to be reported that clashes with property = 'Module'", () => UsageCollector.Report(UsageFeaturesForTest.Usages, ("Module", "Value")));
			AssertExceptionThrown<ArgumentOutOfRangeException>($"We shouldn't allow a property to be reported that clashes with property = 'Description'", () => UsageCollector.Report(UsageFeaturesForTest.Usages, ("FeatureDescription", "Value")));
			AssertExceptionThrown<ArgumentOutOfRangeException>($"We shouldn't allow a property to be reported that clashes with property = 'OrganisationName'", () => UsageCollector.Report(UsageFeaturesForTest.Usages, ("OrganisationName", "Value")));
		}

		public void TestReportThrowsIfPropertiesClashWithBillingTransaction()
		{
			var propertyObject = typeof(BillingTransaction);
			var properties = propertyObject.GetProperties();
			foreach (var property in properties)
			{
				AssertExceptionThrown<ArgumentOutOfRangeException>($"We shouldn't allow a property to be reported that clashes with property = '{property.Name}' from object = '{propertyObject.Name}'", () => UsageCollector.Report(UsageFeaturesForTest.Usages, (property.Name, "Value")));
			}
		}

		public void TestReportThrowsIfPropertiesClashWithUsageTransaction()
		{
			var propertyObject = typeof(UsageTransaction);
			var properties = propertyObject.GetProperties();
			foreach (var property in properties)
			{
				AssertExceptionThrown<ArgumentOutOfRangeException>($"We shouldn't allow a property to be reported that clashes with property = '{property.Name}' from object = '{propertyObject.Name}'", () => UsageCollector.Report(UsageFeaturesForTest.Usages, (property.Name, "Value")));
			}
		}

		public void TestScopeThrowsIfPropertiesClashWithGeneric()
		{
			AssertExceptionThrown<ArgumentOutOfRangeException>($"We shouldn't allow a property to be reported that clashes with property = 'FeatureCode'", () => UsageCollector.Scope(("FeatureCode", "Value")));
			AssertExceptionThrown<ArgumentOutOfRangeException>($"We shouldn't allow a property to be reported that clashes with property = 'Module'", () => UsageCollector.Scope(("Module", "Value")));
			AssertExceptionThrown<ArgumentOutOfRangeException>($"We shouldn't allow a property to be reported that clashes with property = 'Description'", () => UsageCollector.Scope(("FeatureDescription", "Value")));
			AssertExceptionThrown<ArgumentOutOfRangeException>($"We shouldn't allow a property to be reported that clashes with property = 'EnterpriseCode'", () => UsageCollector.Scope(("EnterpriseCode", "Value")));
			AssertExceptionThrown<ArgumentOutOfRangeException>($"We shouldn't allow a property to be reported that clashes with property = 'OrganisationName'", () => UsageCollector.Scope(("OrganisationName", "Value")));
			AssertExceptionThrown<ArgumentOutOfRangeException>($"We shouldn't allow a property to be reported that clashes with property = 'ServerCode'", () => UsageCollector.Scope(("ServerCode", "Value")));
			AssertExceptionThrown<ArgumentOutOfRangeException>($"We shouldn't allow a property to be reported that clashes with property = 'CompanyCode'", () => UsageCollector.Scope(("CompanyCode", "Value")));
			AssertExceptionThrown<ArgumentOutOfRangeException>($"We shouldn't allow a property to be reported that clashes with property = 'CompanyName'", () => UsageCollector.Scope(("CompanyName", "Value")));
			AssertExceptionThrown<ArgumentOutOfRangeException>($"We shouldn't allow a property to be reported that clashes with property = 'Environment'", () => UsageCollector.Scope(("Environment", "Value")));
		}

		public void TestScopeThrowsIfPropertiesClashWithBillingTransaction()
		{
			var propertyObject = typeof(BillingTransaction);
			var properties = propertyObject.GetProperties();
			foreach (var property in properties)
			{
				AssertExceptionThrown<ArgumentOutOfRangeException>($"We shouldn't allow a property to be reported that clashes with property = '{property.Name}' from object = '{propertyObject.Name}'", () => UsageCollector.Scope((property.Name, "Value")));
			}
		}

		public void TestScopeTighterOverridesBroader()
		{
			using (UsageCollector.Scope(("OverridenProperty", "OuterScopeVal")))
			{
				using (UsageCollector.Scope(("OverridenProperty", "InnerScopeVal")))
				{
					UsageCollector.Report(UsageFeaturesForTest.Usages);
				}
			}

			var messages = Helper.LoadUsageMessages();
			AssertEquals("Wrong number of messages loaded after being reported", 1, messages.Length);
			AssertReportedMessageProperties(messages[0], ("OverridenProperty", "InnerScopeVal"));
		}

		[UseSnapshotProtection]
		public void TestScopeAroundAsyncAwait()
		{
			var task = Task.Run(async () =>
			{
				using (UsageCollector.Scope(("Scope", "ScopeVal")))
				{
					await Task.Delay(100);
					using (Db.DisposableActionForDbConnection())
					{
						UsageCollector.Report(UsageFeaturesForTest.Usages);
					}
				}
			});
			task.Wait();

			var messages = Helper.LoadUsageMessages();
			AssertEquals("Wrong number of messages loaded after being reported", 1, messages.Length);
			AssertReportedMessageProperties(messages[0], ("Scope", "ScopeVal"));
		}

		public void TestReport_FactorySpecified_ShouldCreateUsageEventsInsideThePassedFactory()
		{
			var factory = new BusinessObjectFactory();

			UsageCollector.Report(factory, UsageFeaturesForTest.Usages, ("Name", "Value"));

			var helper = new UsageCollectorTestHelper(factory);
			var messages = helper.LoadUsageMessages(UsageFeaturesForTest.Usages);
			AssertEquals(1, messages.Length);
			AssertEquals("Should be in this factory but not in DB since we didn't save the factory", false, messages.First().IsInDatabase);
		}

		[TestDate(2009, 10, 20, 10, 20, 15, 100)]
		public void TestReportBilledUsagesWithMappedValues()
		{
			UsageCollector.Report(UsageFeaturesForTest.BillableMappedFields, ("ReportedCount", 5), ("Reported1", "Reported1Value"), ("Reported2", "Reported2Value"), ("Reported3", "Reported3Value"), ("Reported4", "Reported4Value"), ("Reported5", "Reported5Value"), ("ReportedUser", "ITN"), ("SystemCreateTime", "2015-05-12T05:22:07.5Z"), ("ReportedCode", "RPC"));

			var helper = new UsageCollectorTestHelper(Factory);
			var messages = helper.LoadUsageMessages(UsageFeaturesForTest.BillableMappedFields);
			AssertEquals("No usage messages should have been created", 0, messages.Length);

			var billingManager = new BillingManager();
			var stmUsages = billingManager.GetTransactions(Factory, 10);
			AssertEquals("Should have created a billing transaction", 1, stmUsages.Count());
			var billingTransactionXML = BillingManager.GetTransactionXml(stmUsages.First());
			var expectedXML = string.Format(CultureInfo.InvariantCulture,
@"<?xml version=""1.0"" encoding=""utf-8""?>
<BillingTransaction xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://www.edi.com.au/EnterpriseService/#Billing_1.4"">
  <BillableCount>5</BillableCount>
  <Branch>BNE</Branch>
  <Category>BMF</Category>
  <ClientID>{0}</ClientID>
  <ClientNumber>J.EDI</ClientNumber>
  <ClientStaffCode>ITN</ClientStaffCode>
  <PriceItemCode>RPC</PriceItemCode>
  <Reference1>Reported1Value</Reference1>
  <Reference2>Reported2Value</Reference2>
  <Reference3>Reported3Value</Reference3>
  <Reference4>Reported4Value</Reference4>
  <Reference5>Reported5Value</Reference5>
  <ReportingSource>ENT</ReportingSource>
  <ServiceOccuredUTC>2015-05-12T05:22:07.5Z</ServiceOccuredUTC>
  <Version>0</Version>
  <AdditionalRefs>{1}</AdditionalRefs>
</BillingTransaction>",
GlbCompany.CurrentCompany.LicenceKeyIdentifier, GetExpectedAdditionalRefs(UsageFeaturesForTest.BillableMappedFields, "\"ReportedCount\": 5,\r\n  \"Reported1\": \"Reported1Value\",\r\n  \"Reported2\": \"Reported2Value\",\r\n  \"Reported3\": \"Reported3Value\",\r\n  \"Reported4\": \"Reported4Value\",\r\n  \"Reported5\": \"Reported5Value\",\r\n  \"ReportedUser\": \"ITN\",\r\n  \"SystemCreateTime\": \"2015-05-12T05:22:07.5Z\",\r\n  \"ReportedCode\": \"RPC\""));
			AssertXMLEquals(expectedXML, billingTransactionXML);
		}

		const string ExpectedBillableMappedProperties = "\"FeatureCode\": \"BMF\",\r\n  \"Module\": \"BillableMappedFields\",\r\n  \"FeatureDescription\": \"Billable Feature Code with Mapped Fields\"";
		const string ExpectedBillableDefaultProperties = "\"FeatureCode\": \"BDF\",\r\n  \"Module\": \"BillableDefaultFields\",\r\n  \"FeatureDescription\": \"Billable Feature Code with Default Fields\"";
		const string ExpectedGenericProperties = "\"OrganisationName\": \"EDI CUSTOMS BROKERS\"";

		static string GetExpectedAdditionalRefs(string featureCode, string additionalProperties = null)
		{
			string expectedFeatureProperties;
			switch (featureCode)
			{
				case UsageFeaturesForTest.BillableDefaultFields:
					expectedFeatureProperties = ExpectedBillableDefaultProperties;
					break;
				case UsageFeaturesForTest.BillableMappedFields:
					expectedFeatureProperties = ExpectedBillableMappedProperties;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			var additionalRefs = "{\r\n  " + expectedFeatureProperties + ",\r\n  " + ExpectedGenericProperties;
			if (additionalProperties != null)
			{
				additionalRefs += ",\r\n  " + additionalProperties;
			}
			additionalRefs += "\r\n}";
			return additionalRefs;
		}

		[TestDate(2009, 10, 20, 10, 20, 15, 100)]
		public void TestReportBilledUsagesWithDefaultValuesForUnMappedFields()
		{
			AssertReportBilledUsagesWithNoPropertiesInReport(UsageFeaturesForTest.BillableDefaultFields, expectedCategory: "STL");
		}

		[TestDate(2009, 10, 20, 10, 20, 15, 100)]
		public void TestReportBilledUsagesWithDefaultValuesForMappedFields()
		{
			AssertReportBilledUsagesWithNoPropertiesInReport(UsageFeaturesForTest.BillableMappedFields, expectedCategory: "BMF");
		}

		[TestDate(2009, 10, 20, 10, 20, 15, 100)]
		public void TestReportBilledMappingsUpdateEvery10Minutes()
		{
			var stmUsage = AssertReportBilledUsagesWithNoPropertiesInReport(UsageFeaturesForTest.BillableDefaultFields, expectedCategory: "STL").First();
			stmUsage.Delete();
			stmUsage.Factory.Save();

			var laterTime = ZDateTime.UtcNow.AddMinutes(5).ToDateTime();
			var billingRemovedTime = ZDateTime.UtcNow.AddMinutes(15).ToDateTime();

			TestDateAttribute.Date = laterTime;
			AssertReportBilledUsagesWithNoPropertiesInReport(UsageFeaturesForTest.BillableDefaultFields, expectedCategory: "STL", expectedServiceOccuredUTC: "2009-10-20T10:25:15.1Z");

			var fieldMappings = Factory.Load<RefStlFieldMapping>(new ZQuery());
			fieldMappings.DeleteAll();
			Factory.Save();

			TestDateAttribute.Date = billingRemovedTime;
			UsageCollector.Report(UsageFeaturesForTest.BillableDefaultFields);
			var messages = Helper.LoadUsageMessages();
			AssertEquals("Wrong number of messages loaded after being reported", 1, messages.Length);
			AssertReportedMessagePropertiesBDF(messages[0]);
		}

		IEnumerable<BusinessObject> AssertReportBilledUsagesWithNoPropertiesInReport(string featureCode, string expectedCategory, string expectedServiceOccuredUTC = "2009-10-20T10:20:15.1Z")
		{
			UsageCollector.Report(featureCode);

			var helper = new UsageCollectorTestHelper(Factory);
			var messages = helper.LoadUsageMessages(featureCode);
			AssertEquals("No usage messages should have been created", 0, messages.Length);

			var billingManager = new BillingManager();
			var stmUsages = billingManager.GetTransactions(Factory, 10);
			AssertEquals("Should have created a billing transaction", 1, stmUsages.Count());
			var billingTransactionXML = BillingManager.GetTransactionXml(stmUsages.First());
			var expectedXML = string.Format(CultureInfo.InvariantCulture,
@"<?xml version=""1.0"" encoding=""utf-8""?>
<BillingTransaction xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://www.edi.com.au/EnterpriseService/#Billing_1.4"">
  <BillableCount>1</BillableCount>
  <Branch>BNE</Branch>
  <Category>{0}</Category>
  <ClientID>{1}</ClientID>
  <ClientNumber>J.EDI</ClientNumber>
  <ClientStaffCode>{2}</ClientStaffCode>
  <PriceItemCode>{3}</PriceItemCode>
  <Reference1>.</Reference1>
  <ReportingSource>ENT</ReportingSource>
  <ServiceOccuredUTC>{5}</ServiceOccuredUTC>
  <Version>0</Version>
  <AdditionalRefs>{4}</AdditionalRefs>
</BillingTransaction>",
expectedCategory, GlbCompany.CurrentCompany.LicenceKeyIdentifier, GlbStaff.CurrentUser.GS_Code, featureCode, GetExpectedAdditionalRefs(featureCode), expectedServiceOccuredUTC);
			AssertXMLEquals(expectedXML, billingTransactionXML);

			return stmUsages;
		}

		void AssertReportedMessageProperties(IUsageEDIMessage message, params (string name, object value)[] expectedProperties)
		{
			AssertReportedMessageProperties(message, new UsageFeatureProperties("TST", "UnitTest", "This feature is for unit testing only"), expectedProperties);
		}

		void AssertReportedMessageProperties(IUsageEDIMessage message, UsageFeatureProperties usageFeatureProperties, params (string name, object value)[] expectedProperties)
		{
			AssertEquals("Wrong organisation name", "EDI CUSTOMS BROKERS", message.GetProperty<string>(UsageProperties.OrganisationName));
			AssertReportedMessagePropertiesBase(message, usageFeatureProperties, expectedProperties);
		}

		void AssertReportedMessagePropertiesBDF(IUsageEDIMessage message, params (string name, object value)[] expectedProperties)
		{
			AssertEquals("Wrong organisation name", "EDI CUSTOMS BROKERS", message.GetProperty<string>(UsageProperties.OrganisationName));

			AssertReportedMessagePropertiesBase(message, new UsageFeatureProperties("BDF", "BillableDefaultFields", "Billable Feature Code with Default Fields"), expectedProperties);
		}

		void AssertReportedMessagePropertiesSIN(GlbBranch branch, IUsageEDIMessage message, params (string name, object value)[] expectedProperties)
		{
			AssertEquals("Wrong organisation name", "Singapore Co", message.GetProperty<string>(UsageProperties.OrganisationName));
			AssertEquals("Wrong branch", branch.PK, message.EM_GB);
			AssertReportedMessagePropertiesBase(message, new UsageFeatureProperties("TST", "UnitTest", "This feature is for unit testing only"), expectedProperties);
		}

		void AssertReportedMessagePropertiesBase(IUsageEDIMessage message, UsageFeatureProperties expectedFeatureProperties, params (string name, object value)[] expectedProperties)
		{
			AssertEquals("Wrong FeatureCode", expectedFeatureProperties.Code, message.GetProperty<string>(UsageProperties.FeatureCode));
			AssertEquals("Wrong Module", expectedFeatureProperties.Module, message.GetProperty<string>(UsageProperties.Module));
			AssertEquals("Wrong Description", expectedFeatureProperties.Description, message.GetProperty<string>(UsageProperties.FeatureDescription));
			AssertEquals("Wrong number of additional properties", expectedProperties.Length + 4, message.UsageProperties.Properties().Count());

			foreach (var expectedProperty in expectedProperties)
			{
				var foundPropertyValue = message.GetProperty(expectedProperty.name);
				if (foundPropertyValue is JArray foundArray)
				{
					var expectedArray = expectedProperty.value as JArray;
					AssertEquals("Different array lengths for property values", expectedArray.Count, foundArray.Count);
					for (int i = 0; i < expectedArray.Count; ++i)
					{
						if (expectedArray[i] is JObject expectedObject && foundArray[i] is JObject foundObject)
						{
							var expectedPropertyArray = expectedObject.Properties().ToArray();
							var foundPropertyArray = foundObject.Properties().ToArray();
							AssertEquals("Different number of properties on array objects", expectedPropertyArray.Length, foundPropertyArray.Length);
							for (int j = 0; j < expectedPropertyArray.Length; ++j)
							{
								AssertEquals("Different array value property name", expectedPropertyArray[j].Name, foundPropertyArray[j].Name);
								AssertEquals("Different array value property value", expectedPropertyArray[j].Value, foundPropertyArray[j].Value);
							}
						}
						else
						{
							AssertEquals("Different array values", expectedArray[i], foundArray[i]);
						}
					}
				}
				else
				{
					AssertEquals($"Wrong value for property {expectedProperty.name}", expectedProperty.value.ToString(), foundPropertyValue.ToString());
				}
			}
		}

		UsageCollectorTestHelper Helper => helper ?? (helper = new UsageCollectorTestHelper(Factory));
		UsageCollectorTestHelper helper;
	}
}
