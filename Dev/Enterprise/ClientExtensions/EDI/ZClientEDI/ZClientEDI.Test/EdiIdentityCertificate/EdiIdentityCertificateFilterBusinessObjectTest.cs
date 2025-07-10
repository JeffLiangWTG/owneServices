using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IdentityCertificate.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.ZArchitecture.Business.ModuleNumberRangeFilter;

namespace Enterprise.Client.EDI.IdentityCertificate.Module.Testing
{
	[TestedType(typeof(EdiIdentityCertificateFilterBusinessObject))]
	public class EdiIdentityCertificateFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new EdiIdentityCertificateFilterBusinessObject();
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheck()
		{
			var exclusions = new List<Tuple<string, string>>();
			exclusions.Add(TableFilter(EdiIdentityApplicationSchema.Constants.TableName, "Application"));
			return exclusions;
		}

		public void TestApplicationFilter()
		{
			var now = ZDateTime.UtcNow;
			var cert1 = AddCert(now.AddDays(-5), now.AddDays(15), "123", "issuer1", "certified1", true, false, "COM", "App1", clientId: "client1");
			var cert2 = AddCert(now.AddDays(-5), now.AddDays(15), "123", "issuer1", "certified3", true, false, "COM", "App3", clientId: "client3");
			var cert3 = AddCert(now.AddDays(-5), now.AddDays(15), "111", "issuer3", "certified1", true, true, "COM");

			Factory.Save();

			var filter = new EdiIdentityCertificateFilterBusinessObject();
			filter.AddActiveStatusFilters(typeof(EdiIdentityCertificate));
			var applicationFilter = (ModuleGuidForeignCollectionFilter)filter["Application"];
			applicationFilter.IsActive = true;
			applicationFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			var applicationNameFilter = applicationFilter.SelectedFilters.AddTextFilterStrip("Application Name", "App1");
			var collection = new EdiIdentityCertificateCollection(Factory, filter.Filter);
			AssertEquals("AnyMatch - application name filter with \"App1\" applied", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { cert1 }, collection);

			applicationFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			collection = new EdiIdentityCertificateCollection(Factory, filter.Filter);
			AssertEquals("NoneMatch", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { cert2, cert3 }, collection);

			applicationNameFilter.Property = "A";
			applicationFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			collection = new EdiIdentityCertificateCollection(Factory, filter.Filter);
			AssertEquals("AnyMatch - application name filter starts with \"A\" applied", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { cert1, cert2 }, collection);

			applicationNameFilter.Property = "";
			collection = new EdiIdentityCertificateCollection(Factory, filter.Filter);
			AssertEquals("No filter applied", 3, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { cert1, cert2, cert3 }, collection);

			var clientIdFilter = applicationFilter.SelectedFilters.AddTextFilterStrip("Client Id", "client1");
			collection = new EdiIdentityCertificateCollection(Factory, filter.Filter);
			AssertEquals("AnyMatch - client id filter with \"client1\" applied", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { cert1 }, collection);

			clientIdFilter.Property = "client3";
			collection = new EdiIdentityCertificateCollection(Factory, filter.Filter);
			AssertEquals("AnyMatch - client id filter with \"client3\" applied", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { cert2 }, collection);
		}

		public void TestSequenceNumberFilter()
		{
			var now = ZDateTime.UtcNow;
			var cert1 = AddCert(now.AddDays(-5), now.AddDays(15), "111", "issuer3", "certified1", true, true, "COM");
			Factory.Save();
			var cert2 = AddCert(now.AddDays(-5), now.AddDays(15), "111", "issuer3", "certified1", true, true, "COM");
			Factory.Save();
			var cert3 = AddCert(now.AddDays(-5), now.AddDays(15), "111", "issuer3", "certified1", true, true, "COM");
			Factory.Save();

			Assert(cert1.ICE_SequenceNumber < cert2.ICE_SequenceNumber);
			Assert(cert2.ICE_SequenceNumber < cert3.ICE_SequenceNumber);

			var filter = new EdiIdentityCertificateFilterBusinessObject();
			filter.AddActiveStatusFilters(typeof(EdiIdentityCertificate));
			var clientIdFilter = (ModuleNumberRangeFilter)filter["Sequence Number"];
			clientIdFilter.IsActive = true;

			clientIdFilter.Property1 = (ZDecimal)cert1.ICE_SequenceNumber;
			clientIdFilter.PropertySearch = SearchTexts.GreaterThanOrEqualTo;
			var collection = new EdiIdentityCertificateCollection(Factory, filter.Filter);
			AssertEquals("GreaterThanOrEqualTo", 3, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { cert1, cert2, cert3 }, collection);

			clientIdFilter.PropertySearch = SearchTexts.LessThanOrEqualTo;
			clientIdFilter.Property2 = (ZDecimal)cert2.ICE_SequenceNumber;
			collection = new EdiIdentityCertificateCollection(Factory, filter.Filter);
			AssertEquals("LessThanOrEqualTo", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { cert1, cert2 }, collection);

			clientIdFilter.PropertySearch = SearchTexts.EqualTo;
			clientIdFilter.Property1 = (ZDecimal)cert3.ICE_SequenceNumber;
			collection = new EdiIdentityCertificateCollection(Factory, filter.Filter);
			AssertEquals("EqualTo", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { cert3 }, collection);

			clientIdFilter.PropertySearch = SearchTexts.Between;
			clientIdFilter.Property1 = (ZDecimal)cert2.ICE_SequenceNumber;
			clientIdFilter.Property2 = (ZDecimal)cert3.ICE_SequenceNumber;
			collection = new EdiIdentityCertificateCollection(Factory, filter.Filter);
			AssertEquals("Between", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { cert2, cert3 }, collection);
		}

		public void TestIsRevoked()
		{
			var now = ZDateTime.UtcNow;
			var cert1 = AddCert(now.AddDays(-15), now.AddDays(15), "123", "issuer1", "certified1", true, true, "COM");
			var cert2 = AddCert(now.AddDays(-5), now.AddDays(15), "123", "issuer2", "certified2", true, false, "COM");
			var cert3 = AddCert(now.AddDays(-5), now.AddDays(15), "123", "issuer1", "certified3", true, true, "COM");

			Factory.Save();
			var filter = new EdiIdentityCertificateFilterBusinessObject();
			var revokedFilter = (ModuleTextFilter)filter["Revoked Status"];
			revokedFilter.Property = EdiIdentityCertificateFilterBusinessObject.StatusRevoked;
			revokedFilter.IsActive = true;

			var collection = new EdiIdentityCertificateCollection(Factory, filter.Filter);
			AssertEquals(2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { cert1, cert3 }, collection);

			revokedFilter.Property = EdiIdentityCertificateFilterBusinessObject.StatusNonRevoked;

			collection = new EdiIdentityCertificateCollection(Factory, filter.Filter);
			AssertEquals(1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { cert2 }, collection);

			revokedFilter.Property = FilterStripBusinessObject.StatusAll;

			collection = new EdiIdentityCertificateCollection(Factory, filter.Filter);
			AssertEquals(3, collection.Count);
		}

		public void TestIsProcessingStatus()
		{
			var now = ZDateTime.UtcNow;
			var cert1 = AddCert(now.AddDays(-15), now.AddDays(15), "123", "issuer1", "certified1", true, false, "COM");
			var cert2 = AddCert(now.AddDays(-5), now.AddDays(15), "123", "issuer2", "certified2", false, false, "PRC");
			var cert3 = AddCert(now.AddDays(-5), now.AddDays(15), "123", "issuer1", "certified3", false, false, "COM");

			Factory.Save();
			var filter = new EdiIdentityCertificateFilterBusinessObject();
			var processingFilter = (ModuleTextFilter)filter["Processing Status"];
			processingFilter.Property = EdiIdentityCertificateLookups.Statuses.Completed;
			processingFilter.IsActive = true;

			var collection = new EdiIdentityCertificateCollection(Factory, filter.Filter);
			AssertEquals(2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { cert1, cert3 }, collection);

			processingFilter.Property = EdiIdentityCertificateLookups.Statuses.Processing;

			collection = new EdiIdentityCertificateCollection(Factory, filter.Filter);
			AssertEquals(1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { cert2 }, collection);

			processingFilter.Property = FilterStripBusinessObject.StatusAll;

			collection = new EdiIdentityCertificateCollection(Factory, filter.Filter);
			AssertEquals(3, collection.Count);
		}

		public void TestFiltersCert()
		{
			var now = ZDateTime.UtcNow;
			var cert1 = AddCert(now.AddDays(-5), now.AddDays(15), "123", "issuer1", "certified1", true, false, "COM");
			var cert2 = AddCert(now.AddDays(-5), now.AddDays(15), "123", "issuer2", "certified2", true, false, "COM");
			var cert3 = AddCert(now.AddDays(-5), now.AddDays(15), "123", "issuer1", "certified3", true, false, "COM");
			var cert4 = AddCert(now.AddDays(-15), now.AddDays(-5), "123", "issuer1", "certified1", true, false, "COM");
			var cert5 = AddCert(now.AddDays(-5), now.AddDays(5), "456", "issuer1", "certified1", false, true, "COM");
			var cert6 = AddCert(now.AddDays(-5), now.AddDays(5), "789", "issuer1", "certified1", true, false, "COM");
			var cert7 = AddCert(now.AddDays(-5), now.AddDays(5), "111", "issuer3", "certified1", true, true, "COM");

			Factory.Save();

			var filter = new EdiIdentityCertificateFilterBusinessObject();
			filter.AddActiveStatusFilters(typeof(EdiIdentityCertificate));
			var thumbprintFilter = (ModuleTextFilter)filter["Thumbprint"];
			thumbprintFilter.Property = "123";
			thumbprintFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			thumbprintFilter.IsActive = true;

			var collection = new EdiIdentityCertificateCollection(Factory, filter.Filter);
			AssertEquals(4, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { cert1, cert2, cert3, cert4 }, collection);

			var issuedByFilter = (ModuleTextFilter)filter["Issued By"];
			issuedByFilter.Property = "issuer2";
			issuedByFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			issuedByFilter.IsActive = true;

			collection = new EdiIdentityCertificateCollection(Factory, filter.Filter);
			AssertEquals(1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { cert2 }, collection);

			var revokedFilter = (ModuleTextFilter)filter["Revoked Status"];
			revokedFilter.Property = "Revoked";
			revokedFilter.IsActive = true;

			collection = new EdiIdentityCertificateCollection(Factory, filter.Filter);
			AssertEquals(0, collection.Count);

			thumbprintFilter.IsActive = false;
			collection = new EdiIdentityCertificateCollection(Factory, filter.Filter);
			AssertEquals(0, collection.Count);

			var isActiveFilter = (ModuleTextFilter)filter["Active Status"];
			isActiveFilter.Property = "All";
			isActiveFilter.IsActive = true;

			issuedByFilter.Property = "issuer1";
			collection = new EdiIdentityCertificateCollection(Factory, filter.Filter);
			AssertEquals(1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { cert5 }, collection);

			issuedByFilter.IsActive = false;
			isActiveFilter.Property = "Active";

			collection = new EdiIdentityCertificateCollection(Factory, filter.Filter);
			AssertEquals(1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { cert7 }, collection);

			revokedFilter.Property = "Non-Revoked";
			collection = new EdiIdentityCertificateCollection(Factory, filter.Filter);
			AssertEquals(5, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { cert1, cert2, cert3, cert4, cert6 }, collection);

			revokedFilter.IsActive = false;

			var validFilter = (ModuleDateFilter)filter["Valid Date"];
			validFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Last7Days;
			validFilter.IsActive = true;

			collection = new EdiIdentityCertificateCollection(Factory, filter.Filter);
			AssertEquals(5, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { cert1, cert2, cert3, cert6, cert7 }, collection);

			var expiryFilter = (ModuleDateFilter)filter["Expiry Date"];
			expiryFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Next7Days;
			expiryFilter.IsActive = true;

			collection = new EdiIdentityCertificateCollection(Factory, filter.Filter);
			AssertEquals(2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { cert6, cert7 }, collection);
		}

		public void TestAutoVsManuallyGeneratedFilter()
		{
			var now = ZDateTime.UtcNow;
			var certEmptyLDBlankAppName = AddCert(now.AddDays(-5), now.AddDays(15), "123", "issuer1", "certified1", true, false, "COM");
			certEmptyLDBlankAppName.Application.IDA_LD = ZGuid.Empty;

			var certEmptyLDValidAppName = AddCert(now.AddDays(-5), now.AddDays(15), "123", "issuer1", "certified1", true, false, "COM", "MyAppName");
			certEmptyLDValidAppName.Application.IDA_LD = ZGuid.Empty;

			var certEmptyLDValidAppNameInActive = AddCert(now.AddDays(-5), now.AddDays(15), "123", "issuer1", "certified1", false, false, "COM", "MyAppName1");
			certEmptyLDValidAppNameInActive.Application.IDA_LD = ZGuid.Empty;

			var certValidLDBlankAppName = AddCert(now.AddDays(-5), now.AddDays(15), "123", "issuer1", "certified1", true, false, "COM");
			var certValidLDValidAppName = AddCert(now.AddDays(-5), now.AddDays(15), "123", "issuer1", "certified1", true, false, "COM", "MyAppName2");

			var certValidLDValidAppNameInActive = AddCert(now.AddDays(-5), now.AddDays(15), "123", "issuer1", "certified1", false, false, "COM", "MyAppName3");

			var manuallyGeneratedCollection = new[] { certEmptyLDBlankAppName, certEmptyLDValidAppName };
			var autoGeneratedCollection = new[] { certValidLDBlankAppName, certValidLDValidAppName, };

			Factory.Save();

			var filter = new EdiIdentityCertificateFilterBusinessObject();
			filter.AddActiveStatusFilters(typeof(EdiIdentityCertificate));
			var createdUserFilter = (ModuleTextFilter)filter["Auto/Manually Generated"];
			createdUserFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			createdUserFilter.IsActive = true;

			createdUserFilter.Property = "Manually Generated";
			var testCollection = new EdiIdentityCertificateCollection(Factory, filter.Filter);
			AssertEquals(manuallyGeneratedCollection.Length, testCollection.Count);
			AssertContainsExactElementsInAnyOrder(manuallyGeneratedCollection, testCollection);

			createdUserFilter.Property = "Auto-Generated";
			testCollection = new EdiIdentityCertificateCollection(Factory, filter.Filter);
			AssertEquals(2, testCollection.Count);
			AssertContainsExactElementsInAnyOrder(autoGeneratedCollection, testCollection);
		}

		EdiIdentityCertificate AddCert(ZDateTime valid, ZDateTime expiry, ZString thumbprint, ZString issuedBy, ZString issuedTo, ZBool isActive, ZBool isRevoked, ZString processingStatus, string appName = "", bool isRolledBack = false, string csr = "", string clientId = "")
		{
			var cert = Factory.NewWithValidTestData<EdiIdentityCertificate>();
			cert.ICE_CertificateValidDate = valid;
			cert.ICE_CertificateExpiryDate = expiry;
			cert.ICE_CertificateThumbprint = thumbprint;
			cert.ICE_CertificateIssuedBy = issuedBy;

			cert.ICE_CertificateIssuedTo = issuedTo;
			cert.ICE_IsActive = isActive;
			cert.ICE_IsCertificateRevoked = isRevoked;
			cert.ICE_ProcessingStatus = processingStatus;
			cert.Application.IDA_ClientID = clientId;
			cert.Application.IDA_IsRollback = isRolledBack;
			cert.ICE_CertificateSigningRequest = string.IsNullOrEmpty(csr) ? Guid.NewGuid().ToString() : csr;
			cert.Application.IDA_ApplicationName = appName;

			return cert;
		}
	}
}
