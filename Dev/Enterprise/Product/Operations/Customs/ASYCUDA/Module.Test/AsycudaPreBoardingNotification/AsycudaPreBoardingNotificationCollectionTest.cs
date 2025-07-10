using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Module.Testing
{
	[TestedType(typeof(AsycudaPreBoardingNotificationCollection))]
	class AsycudaPreBoardingNotificationCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestIncludedManifestTypeAndBranchFilter()
		{
			var testCollection = GetCollectionToTest();
			AssertContains("AMA_ManifestType = 'PBN' and AMA_GB IN (SELECT GB_PK FROM dbo.GlbBranch WHERE GB_GC =", testCollection.CompleteFilter.LiteralTextADO);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new AsycudaPreBoardingNotificationCollection(Factory, otherCompany.PK);

		protected override void SetUp()
		{
			base.SetUp();
			otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			otherCompany.SetCountry("CA");
			var otherBranch = Factory.New<GlbBranch>();
			otherBranch.GB_RL_NKHomePort = "USLAX";
			otherBranch.GB_GC = otherCompany.PK;
			otherBranch.GB_Code = "OTH";
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var testHeader1 = (AsycudaManifestHeader)newFactory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			testHeader1.AMA_JobReference = "VV1";
			testHeader1.AMA_GB = GlbBranch.CurrentBranch.PK;
			var testHeader2 = (ManifestBase.AsycudaManifestHeader)Factory.New<Integration.Customs.ZA.IAsycudaManifestHeader>();
			testHeader2.AMA_JobReference = "VV2";
			testHeader2.AMA_GB = GlbBranch.CurrentBranch.PK;
			var testHeader3 = (ManifestBase.AsycudaManifestHeader)Factory.New<Integration.Customs.ZA.IAsycudaManifestHeader>();
			testHeader3.AMA_JobReference = "VV3";
			var testHeader5 = (AsycudaManifestHeader)newFactory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			testHeader5.AMA_JobReference = "VV4";
			var testHeader6 = (AsycudaManifestHeader)newFactory.New<Integration.Customs.ASYCUDA.TWBriefCustomsDeclaration.IAsycudaManifestHeader>();
			testHeader6.AMA_JobReference = "VV5";
			newFactory.Save();
		}
		GlbCompany otherCompany;
	}
}
