using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ManifestBase;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeaderCollection))]
	sealed class AsycudaManifestHeaderCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestGetHeader()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var header1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header1.SetParent(consol);
			header1.AMA_RN_NKCountry = Core.Constants.CountryCodes.Vanuatu;
			header1.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			header1.AMA_ManifestType = "AAA";

			var header2 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header2.SetParent(consol);
			header2.AMA_RN_NKCountry = Core.Constants.CountryCodes.Vanuatu;
			header2.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			header2.AMA_ManifestType = "BBB";

			var collection = new AsycudaManifestHeaderCollection(consol);
			collection.Load();

			AssertEquals(header1, collection.GetHeader(Core.Constants.CountryCodes.Vanuatu, "AAA"));
			AssertEquals(header2, collection.GetHeader(Core.Constants.CountryCodes.Vanuatu, "BBB"));
		}

		public void TestCollection()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var header1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header1.SetParent(consol);
			header1.AMA_ApplicationCode = AsycudaManifestHeader.ApplicationCode_ZAOutturnGateInOut;

			var header2 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header2.SetParent(consol);
			header2.AMA_ApplicationCode = "NVC";

			var collection = new AsycudaManifestHeaderCollection(consol);
			collection.Load();

			AssertCollectionNotContains(header1, collection);
			AssertCollectionContains(header2, collection);
		}

		public void TestRelationshipFilter_India()
		{
			var testCollection = GetCollectionToTest();
			var companyFilter = $"and (AMA_RN_NKCountry <> 'IN' or AMA_GB IN (SELECT GB_PK FROM dbo.GlbBranch WHERE GB_GC = CONVERT('{GlbCompany.CurrentCompany.PK}', 'System.Guid')))";
			AssertNotContains("Should not include company filter for other countries", companyFilter, testCollection.CompleteFilter.LiteralTextADO);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				testCollection = GetCollectionToTest();
				AssertNotContains("Should not include company filter when master not in database", companyFilter, testCollection.CompleteFilter.LiteralTextADO);
				testCollection = GetCollectionToTest();
				Factory.Save();
				AssertNotContains("Should not include company filter for support user", companyFilter, testCollection.CompleteFilter.LiteralTextADO);

				GlbStaff.CurrentUser.GS_LoginName = "Enterprise User";
				AssertContains("Should include company filter for IN and master in database", companyFilter, testCollection.CompleteFilter.LiteralTextADO);
			}
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			return header;
		}

		protected override Type GetExpectedCollectionType() => typeof(AsycudaManifestHeaderCollection);

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			return new AsycudaManifestHeaderCollection(consol);
		}
	}
}
