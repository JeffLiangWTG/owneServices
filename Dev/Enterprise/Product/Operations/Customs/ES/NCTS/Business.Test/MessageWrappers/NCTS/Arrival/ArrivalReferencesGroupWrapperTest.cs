using System;
using System.Linq;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class ArrivalReferencesGroupWrapperTest : Customs.Business.Testing.DataProviderTestCase<ArrivalReferencesGroupWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Throw exception if nctsHeader is null", () => new ArrivalReferencesGroupWrapper(null));
		}

		public void TestTransitNumber()
		{
			nctsHeader.ArrivalMrnFromUser = "AH";
			AssertEquals("Expected filled TransitNumber", "AH", wrapper.TransitNumber);
		}

		public void TestPreviousSummaryDeclarationNumber()
		{
			nctsHeader.ESNctsHeader.CEN_PreviousSummaryDeclaration = "AH";
			AssertEquals("Expected filled PreviousSummaryDeclarationNumber", "AH", wrapper.PreviousSummaryDeclarationNumber);
		}

		public void TestRouteEvents()
		{
			nctsHeader.EnRouteIncidents.AddNew().BN_CustomsStatus = "X";
			nctsHeader.EnRouteSeals.AddNew().BN_CustomsStatus = "X";
			nctsHeader.EnRouteTransshipments.AddNew().BN_CustomsStatus = "X";
			Factory.Save();

			AssertEquals("Expected 3 RouteEvents", 3, (wrapper.RouteEvents.ToList()).Count);
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			wrapper = new ArrivalReferencesGroupWrapper(nctsHeader);
		}
		NctsHeader nctsHeader;
		ArrivalReferencesGroupWrapper wrapper;

		protected override ArrivalReferencesGroupWrapper GetProvider() => wrapper;
	}
}
