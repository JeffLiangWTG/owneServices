using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Module.Testing
{
	[TestedType(typeof(AsycudaManifestModuleCollection))]
	sealed class AsycudaManifestModuleCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestGetAsycudaManifestHeader()
		{
			var testCollection = GetCollectionToTest();
			testCollection.Load();
			AssertEquals(1, testCollection.Count);
		}

		public void TestIncludedApplicationCodesFilter()
		{
			var testCollection = GetCollectionToTest();
			AssertEquals("Filter include IncludedApplicationCodes", "AMA_RN_NKCountry <> 'SG' and (AMA_ApplicationCode in ('NVC', 'VOC'))", testCollection.CompleteFilter.LiteralTextADO);
		}

		public void TestRelationshipFilter_India()
		{
			var testCollection = GetCollectionToTest();
			var companyFilter = $"and (AMA_RN_NKCountry <> 'IN' or AMA_GB IN (SELECT GB_PK FROM dbo.GlbBranch WHERE GB_GC = CONVERT('{GlbCompany.CurrentCompany.PK}', 'System.Guid')))";
			AssertNotContains("Should not include company filter for other countries", companyFilter, testCollection.CompleteFilter.LiteralTextADO);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				testCollection = GetCollectionToTest();
				AssertNotContains("Should not include company filter for support user", companyFilter, testCollection.CompleteFilter.LiteralTextADO);

				testCollection = GetCollectionToTest();
				GlbStaff.CurrentUser.GS_LoginName = "Enterprise User";
				AssertContains("Should include company filter for IN", companyFilter, testCollection.CompleteFilter.LiteralTextADO);
			}
		}

		public override void TestAdd()
		{
			Assert(true);
		}

		public override void TestDelete()
		{
			Assert(true);
		}

		public override void TestRemoveFromRelationship()
		{
			Assert(true);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new AsycudaManifestModuleCollection(Factory);

		protected override void SetUp()
		{
			base.SetUp();
			var newFactory = new BusinessObjectFactory();
			var testHeader1 = (AsycudaManifestHeader)newFactory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			testHeader1.AMA_JobReference = "VV1";
			var testHeader2 = (ManifestBase.AsycudaManifestHeader)Factory.New<Integration.Customs.ZA.IAsycudaManifestHeader>();
			testHeader2.AMA_JobReference = "VV2";
			var testHeader3 = (ManifestBase.AsycudaManifestHeader)Factory.New<Integration.Customs.ZA.IAsycudaManifestHeader>();
			testHeader3.AMA_JobReference = "VV3";
			var testHeader4 = (AsycudaManifestHeader)newFactory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			testHeader4.AMA_JobReference = "VV4";
			var testHeader5 = (AsycudaManifestHeader)newFactory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			testHeader5.AMA_JobReference = "VV5";
			var testHeader6 = (AsycudaManifestHeader)newFactory.New<Integration.Customs.ASYCUDA.TWBriefCustomsDeclaration.IAsycudaManifestHeader>();
			testHeader6.AMA_JobReference = "VV6";
			newFactory.Save();
		}
	}
}
