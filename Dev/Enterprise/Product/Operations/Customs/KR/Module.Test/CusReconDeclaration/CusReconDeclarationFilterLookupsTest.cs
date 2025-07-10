using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Module.Testing
{
	[TestedType(typeof(CusReconDeclarationFilterLookups))]
	sealed class CusReconDeclarationFilterLookupsTest : TestCaseWithFactory
	{
		public void TestCustomsOffices()
		{
			AssertEquals("CustomsOffices", typeof(ZZRefCusCodeListCombinedCollection), lookups.CustomsOffices.GetType());
		}

		public void TestMessageStatusList()
		{
			AssertEquals("ESO, OST, ORJ, OAC, CAB", lookups.messageStatusList.CodesAsString);
		}

		public void TestEntryStatusList()
		{
			AssertEquals("NDC, DMS, ANT, PNR, PFL", lookups.entryStatusList.CodesAsString);
		}

		protected override void SetUp()
		{
			base.SetUp();
			lookups = new CusReconDeclarationFilterLookups(new CusReconDeclarationFilterStripBusinessObject());
		}
		CusReconDeclarationFilterLookups lookups;
	}
}
