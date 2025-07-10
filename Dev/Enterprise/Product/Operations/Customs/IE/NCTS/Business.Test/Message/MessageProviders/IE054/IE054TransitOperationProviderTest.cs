using System;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class IE054TransitOperationProviderTest : Customs.Business.Testing.DataProviderTestCase<IE054TransitOperationProvider>
	{
		[TestDate(2023, 04, 21, 14, 30, 15)]
		public void TestReleaseRequestDateAndTime()
		{
			AssertEquals(new DateTime(2023, 04, 21, 14, 30, 15), Provider.ReleaseRequestDateAndTime);
		}

		public void TestIsReleaseRequested_Yes()
		{
			releaseRequest = "Y";
			AssertEquals(true, Provider.IsReleaseRequested);
		}

		public void TestIsReleaseRequested_No()
		{
			releaseRequest = "N";
			AssertEquals(false, Provider.IsReleaseRequested);
		}

		public void TestMRN()
		{
			nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "MRNIERO1234";
			AssertEquals("MRN", "MRNIERO1234", Provider.MRN);
		}

		protected override IE054TransitOperationProvider GetProvider() => new IE054TransitOperationProvider(nctsHeader, releaseRequest);

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		}

		NctsHeader nctsHeader;
		string releaseRequest = "1";
	}
}
