using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>))]
	sealed class AsycudaBillCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSetDefaultsForNewChild()
		{
			var manifestHeader = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "HAB");
			manifestHeader.FillWithValidTestData();
			var bill = manifestHeader.Bills.AddNew();
			AssertEquals("ZA", bill.CountryCode);
		}

		public void TestSetOriginDestinationDefaults()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_RL_NKPortOfLoading = "AUSYD";
			manifestHeader.AMA_RL_NKPortOfDischarge = "VUVLI";

			var bill = manifestHeader.Bills.AddNew();
			AssertEquals(1, manifestHeader.Bills.AsEnumerable().Count());

			AssertEquals("AUSYD", bill.ABL_RL_NKOrigin);
			AssertEquals("VUVLI", bill.ABL_RL_NKFinalDestination);

			manifestHeader.AMA_RL_NKPortOfLoading = "AUMEB";
			manifestHeader.AMA_RL_NKPortOfDischarge = "ZAAGZ";

			bill = manifestHeader.Bills.AddNew();
			AssertEquals(2, manifestHeader.Bills.AsEnumerable().Count());

			AssertEquals("AUMEB", bill.ABL_RL_NKOrigin);
			AssertEquals("ZAAGZ", bill.ABL_RL_NKFinalDestination);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			return new AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>(manifestHeader);
		}
	}
}
