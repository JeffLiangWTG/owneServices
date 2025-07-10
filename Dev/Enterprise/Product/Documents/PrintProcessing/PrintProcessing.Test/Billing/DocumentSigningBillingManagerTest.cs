using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Billing.Business;
using Enterprise.Billing.Business.Testing;
using Enterprise.Billing.Integration;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.PrintProcessing.Billing.Testing
{
	public class DocumentSigningBillingManagerTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			UsageCollectorTest.ClearFeaturesForTest();
			base.SetUp();
		}

		public void TestBillingProperties()
		{
			var branch = Factory.LoadTop1<GlbBranch>(new ZQuery());
			var printJob = Factory.New<StmPrintJob>();
			printJob.SP_SignBy = "DOS";
			printJob.SP_IsSigned = true;
			printJob.SP_GB = branch.PK;
			printJob.SP_DocumentType = "INV";
			printJob.SP_JobType = "EML";
			printJob.SP_DocumentName = "document name";
			printJob.SP_ParentGuid = ZGuid.NewZGuid();
			printJob.SP_ParentTableName = "XXX";
			ZDateTime now = ZDateTime.UtcNow;
			printJob.SP_RunDateTime = now;

			Factory.Save();

			var manager = new DocumentSigningBillingManagerForTest();
			manager.LogUsage(printJob, "ABC", DocumentSigningStatus.Success, "123", new LoggerForTest());
			AssertEquals("log succeed and called API", 1, manager.CallApiCounter);
			AssertBillingProperties("ABC", "AU1");

			var branchFromAnotherCountry = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_RN_NKCountryCode, CountryCodes.Singapore));
			printJob.SP_GB = branchFromAnotherCountry.PK;
			manager.LogUsage(printJob, "DEF", DocumentSigningStatus.Success, "123", new LoggerForTest());
			AssertEquals("log succeed and called API", 2, manager.CallApiCounter);
			AssertBillingProperties("DEF", "SG1");

			void AssertBillingProperties(string expectedProcessorName, string expectedCountryDocumentCode)
			{
				AssertEquals("INV", GetValueFromProperty(manager.LastProperties, UsageProperties.Mode));
				AssertEquals("EML", GetValueFromProperty(manager.LastProperties, UsageProperties.JobType));
				AssertEquals("document name", GetValueFromProperty(manager.LastProperties, UsageProperties.DocumentName));
				AssertEquals(printJob.SP_ParentGuid, GetValueFromProperty(manager.LastProperties, UsageProperties.ParentGuid));
				AssertEquals("XXX", GetValueFromProperty(manager.LastProperties, UsageProperties.ParentTableName));
				AssertEquals(now.ToLongTimeString(), GetValueFromProperty(manager.LastProperties, UsageProperties.RunDateTime));
				AssertEquals(printJob.SP_SystemCreateUser, GetValueFromProperty(manager.LastProperties, UsageProperties.SystemCreateUser));
				AssertEquals(expectedCountryDocumentCode, GetValueFromProperty(manager.LastProperties, UsageProperties.CountryDocumentCode));
				AssertEquals(expectedProcessorName, GetValueFromProperty(manager.LastProperties, UsageProperties.ProcessorName));
			}
		}

		object GetValueFromProperty(List<(string name, object value)> properties, string name)
		{
			return properties.FirstOrDefault(p => p.name == name).value;
		}

		public void TestReportErrorIfApiNotWorking()
		{
			var branch = Factory.LoadTop1<GlbBranch>(new ZQuery());
			var printJob = Factory.New<StmPrintJob>();
			printJob.SP_SignBy = "DOS";
			printJob.SP_IsSigned = true;
			printJob.SP_GB = branch.PK;
			var manager = new DocumentSigningBillingManagerForTest();
			manager.ThrowOnCall = true;

			var logger = new LoggerForTest();
			manager.LogUsage(printJob, "ABC", DocumentSigningStatus.Success, "111", logger);
			AssertEquals("API is called", 1, manager.CallApiCounter);
			AssertEquals("API throw an error", "Could not report usage. Error: bombarda maxima", ErrorReporter.LastMessageReported);
			AssertEquals("No log expected", 0, logger.LogEntries.Count());

			ErrorReporter.Clear();
		}

		public void TestGoodItemCallsApi()
		{
			var printJob = Factory.New<StmPrintJob>();
			printJob.SP_SignBy = "DOS";
			printJob.SP_IsSigned = true;

			var manager = new DocumentSigningBillingManagerForTest();
			var logger = new LoggerForTest();
			manager.LogUsage(printJob, "ABC", DocumentSigningStatus.Success, "111", logger);
			AssertEquals("log succeed and called API", 1, manager.CallApiCounter);
			AssertEquals("No log expected", 0, logger.LogEntries.Count());
		}

		public void TestDocumentNotSignedShouldNotBeLogged()
		{
			var printJob = Factory.New<StmPrintJob>();
			printJob.SP_SignBy = "DOS";
			printJob.SP_IsSigned = false;

			var manager = new DocumentSigningBillingManagerForTest();
			var logger = new LoggerForTest();
			manager.LogUsage(printJob, "ABC", DocumentSigningStatus.Success, "111", logger);
			AssertEquals("Could not report usage. Warning: Print Job must be signed or Failed.", logger.ToString());
			AssertEquals(0, manager.CallApiCounter);
		}

		public void TestValidation()
		{
			var branch = Factory.LoadTop1<GlbBranch>(new ZQuery());
			var manager = new DocumentSigningBillingManagerForTest();
			var logger = new LoggerForTest();

			manager.LogUsage(null, "ABC", DocumentSigningStatus.Success, "111", logger);
			AssertEquals(0, manager.CallApiCounter);
			AssertEquals("Could not report usage. Warning: Print Job cannot be null.", logger.ToString());
			logger.ClearLog();

			var printJob = Factory.New<StmPrintJob>();
			printJob.SP_GB = branch.PK;
			printJob.SP_SignBy = "NON";
			manager.LogUsage(printJob, "ABC", DocumentSigningStatus.Success, "111", logger);
			AssertEquals(0, manager.CallApiCounter);
			AssertEquals("Could not report usage. Warning: Print Job isn't set for document signing: NON.", logger.ToString());
			logger.ClearLog();

			printJob.SP_SignBy = "PFX";
			manager.LogUsage(printJob, "ABC", DocumentSigningStatus.Success, "111", logger);
			AssertEquals(0, manager.CallApiCounter);
			AssertEquals("Could not report usage. Warning: Print Job isn't set for document signing: PFX.", logger.ToString());
			logger.ClearLog();

			printJob.SP_SignBy = "DOS";
			manager.LogUsage(printJob, "ABC", DocumentSigningStatus.Success, "111", logger);
			AssertEquals(0, manager.CallApiCounter);
			AssertEquals("Could not report usage. Warning: Print Job must be signed or Failed.", logger.ToString());
			logger.ClearLog();

			printJob.SP_IsSigned = true;
			manager.LogUsage(printJob, string.Empty, DocumentSigningStatus.Success, "111", logger);
			AssertEquals(0, manager.CallApiCounter);
			AssertEquals("Could not report usage. Warning: Missing provider code.", logger.ToString());
			logger.ClearLog();

			manager.LogUsage(printJob, "ABC", DocumentSigningStatus.Success, "111", logger);
			AssertEquals("log succeed and called API", 1, manager.CallApiCounter);
			AssertEquals(string.Empty, logger.ToString());

			printJob.SP_GB = ZGuid.Empty;
			manager.LogUsage(printJob, "ABC", DocumentSigningStatus.Success, "111", logger);
			AssertEquals(1, manager.CallApiCounter);
			AssertEquals("Could not report usage. Warning: Branch does not exist: 00000000-0000-0000-0000-000000000000.", logger.ToString());
			logger.ClearLog();

			printJob.SP_GB = branch.PK;
			printJob.SP_IsSigned = false;
			manager.LogUsage(printJob, "ABC", DocumentSigningStatus.Fail, "111", logger);
			AssertEquals("log succeed and called API", 2, manager.CallApiCounter);
			AssertEquals(string.Empty, logger.ToString());
		}

		[TestDate(2009, 10, 20, 10, 20, 15, 100)]
		public void TestReportBilledUsagesWithMappedValues()
		{
			InitializeBillingFieldMapping(Factory);

			var branch = Factory.LoadTop1<GlbBranch>(new ZQuery());
			branch.GB_RN_NKCountryCode = CountryCodes.France;

			var printJob = Factory.New<StmPrintJob>();
			printJob.SP_GB = branch.PK;
			printJob.SP_SignBy = "DOS";
			printJob.SP_IsSigned = true;
			printJob.SP_SystemCreateUser = "XXX";
			printJob.SP_DocumentName = "doc";
			printJob.SP_ParentGuid = Guid.NewGuid();
			printJob.SP_ParentTableName = "Parent";

			var manager = new DocumentSigningBillingManager();
			manager.LogUsage(printJob, "ABC", DocumentSigningStatus.Success, "321", new LoggerForTest());

			var billingManager = new BillingManager();
			var stmUsages = billingManager.GetTransactions(manager.Factory, 10);
			AssertEquals("Should have created a billing transaction", 1, stmUsages.Count());
			var billingTransactionXML = BillingManager.GetTransactionXml(stmUsages.First());

			var additionalRefs = $@"{{
  ""FeatureCode"": ""DOS"",
  ""Module"": ""DocumentSigning"",
  ""FeatureDescription"": ""Document Signing"",
  ""OrganisationName"": ""EDI CUSTOMS BROKERS"",
  ""CountryDocumentCode"": ""FR1"",
  ""JobType"": ""PRN"",
  ""DocumentName"": ""doc"",
  ""ParentGuid"": ""{printJob.SP_ParentGuid}"",
  ""ParentTableName"": ""Parent"",
  ""ProcessorName"": ""ABC"",
  ""SystemCreateUser"": ""XXX"",
  ""SignStatus"": ""Success"",
  ""TransactionId"": ""321""
}}";

			var expectedXML =
$@"<?xml version=""1.0"" encoding=""utf-8""?>
<BillingTransaction xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://www.edi.com.au/EnterpriseService/#Billing_1.4"">
  <BillableCount>1</BillableCount>
  <Branch>SYD</Branch>
  <Category>DOS</Category>
  <ClientID>{GlbCompany.CurrentCompany.LicenceKeyIdentifier}</ClientID>
  <ClientNumber>J.EDI</ClientNumber>
  <ClientStaffCode>XXX</ClientStaffCode>
  <PriceItemCode>FR1</PriceItemCode>
  <Reference1>doc</Reference1>
  <Reference2>{printJob.SP_ParentGuid}</Reference2>
  <Reference3>{printJob.SP_ParentTableName}</Reference3>
  <Reference4>Success</Reference4>
  <Reference5>321</Reference5>
  <ReportingSource>ENT</ReportingSource>
  <ServiceOccuredUTC>2009-10-20T10:20:15.1Z</ServiceOccuredUTC>
  <Version>0</Version>
  <AdditionalRefs>{additionalRefs}</AdditionalRefs>
</BillingTransaction>";

			AssertXMLEquals(expectedXML, billingTransactionXML);
		}

		public static void InitializeBillingFieldMapping(BusinessObjectFactory factory)
		{
			if (factory.Exists(typeof(RefStlFieldMapping), new ZQuery(RefStlFieldMappingSchema.SFM_FeatureCode, UsageFeatures.Codes.DocumentSigning)))
			{
				return;
			}
			var dosMappings = factory.New<RefStlFieldMapping>();
			dosMappings.SFM_FeatureCode = UsageFeatures.Codes.DocumentSigning;
			dosMappings.SFM_Category = "DOS";
			dosMappings.SFM_PriceItemCode = UsageProperties.CountryDocumentCode;
			dosMappings.SFM_Reference1 = UsageProperties.DocumentName;
			dosMappings.SFM_Reference2 = UsageProperties.ParentGuid;
			dosMappings.SFM_Reference3 = UsageProperties.ParentTableName;
			dosMappings.SFM_Reference4 = UsageProperties.SignStatus;
			dosMappings.SFM_Reference5 = UsageProperties.TransactionId;
			dosMappings.SFM_ServiceOccuredUTC = UsageProperties.RunDateTime;
			dosMappings.SFM_ClientStaffCode = UsageProperties.SystemCreateUser;
			factory.Save();
		}

		class DocumentSigningBillingManagerForTest : DocumentSigningBillingManager
		{
			public bool ThrowOnCall;
			public int CallApiCounter;
			public List<(string name, object value)> LastProperties;
			protected override void CallApi(List<(string name, object value)> properties, GlbBranch branch)
			{
				CallApiCounter++;

				LastProperties = properties;

				if (ThrowOnCall)
				{
					throw new Exception("bombarda maxima");
				}
			}
		}
	}
}
