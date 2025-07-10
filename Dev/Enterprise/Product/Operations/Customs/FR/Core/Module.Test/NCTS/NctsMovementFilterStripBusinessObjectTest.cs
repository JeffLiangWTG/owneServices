using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;
using NUnit.Framework;
using NctsTransitStatusList = Enterprise.Customs.FR.Business.NctsTransitStatusList;

namespace Enterprise.Customs.FR.Module.NCTS.Testing
{
	[TestedType(typeof(NctsMovementFilterStripBusinessObject))]
	class NctsMovementFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestDetailedDepartureStatusCodeFilter()
		{
			departure1.BH_JobReference = "NCT0000110";
			departure1.DetailedDepartureStatusCode = NctsDetailedStatusList.Codes.AmendmentAccepted;
			departure2.BH_JobReference = "NCT0000111";
			arrival1.BH_JobReference = "NCT0000112";
			arrival1.DetailedDepartureStatusCode = NctsDetailedStatusList.Codes.ArrivalNotification;
			arrival2.BH_JobReference = "NCT0000113";
			Factory.Save();

			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var textFilter = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FRFilterConstants.DetailedStatus];
			AssertNotNull(textFilter);
			textFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith;
			textFilter.IsActive = true;

			textFilter.Property = NctsDetailedStatusList.Codes.AmendmentAccepted;
			AssertEquals("departure1 does match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure2 does not match filter", false, departure2.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrival1 does not match filter", false, arrival1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrival2 does not match filter", false, arrival2.MatchesFilter(filterStripBO.Filter));

			textFilter.Property = NctsDetailedStatusList.Codes.ArrivalNotification;
			AssertEquals("departure1 does not match filter", false, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure2 does not match filter", false, departure2.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrival1 does match filter", true, arrival1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrival2 does not match filter", false, arrival2.MatchesFilter(filterStripBO.Filter));

			textFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank;
			AssertEquals("departure1 does not match filter", false, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure2 does match filter", true, departure2.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrival1 does not match filter", false, arrival1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrival2 does match filter", true, arrival2.MatchesFilter(filterStripBO.Filter));
		}

		public void TestNctsTransitStatusList()
		{
			var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
			mockSettings.Setup(x => x.IsNctsEnabled).Returns(true);
			mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(false);
			using (ObjectFactory.Substitute(mockSettings.Object))
			{
				var stripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				var departureStatusFilter = (ModuleTextFilter)stripBO[NctsMovementFilterStripBusinessObject.FilterConstants.DepartureStatus];
				var departureStatusFilterCodeList = (CodeDescriptionPairList)departureStatusFilter.List;
				AssertContainsExactElementsInAnyOrder("DepartureStatus", new NctsTransitStatusList().GetAllCodes(), departureStatusFilterCodeList.GetAllCodes());
				var arrivalStatusFilter = (ModuleTextFilter)stripBO[NctsMovementFilterStripBusinessObject.FilterConstants.ArrivalStatus];
				var arrivalStatusFilterCodeList = (CodeDescriptionPairList)arrivalStatusFilter.List;
				AssertContainsExactElementsInAnyOrder("ArrivalStatus", new NctsTransitStatusList().GetAllCodes(), arrivalStatusFilterCodeList.GetAllCodes());
			}
		}

		public void TestGetMessageStatusList() => CombineAssertions(() =>
		{
			var stripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var applicationCodeFilter = (ModuleTextFilter)stripBO[NctsMovementFilterStripBusinessObject.FilterConstants.ApplicationCode];
			var messageStatusFilter = (ModuleTextFilter)stripBO[NctsMovementFilterStripBusinessObject.FilterConstants.MessagingStatus];

			applicationCodeFilter.Property = CusInBondApplicationCodeList.Codes.NCTS4;
			var frNcts4Codes = new FrNctsMessageStatusList().GetAllCodes();
			AssertContainsExactElementsInAnyOrder("ApplicationCode NCTS4", frNcts4Codes, ((CodeDescriptionPairList)messageStatusFilter.List).GetAllCodes());

			applicationCodeFilter.Property = CusInBondApplicationCodeList.Codes.NCTS5;
			var euNcts5Codes = new LogicalStatusList().GetAllCodes();
			var euNcts4Codes = new EU.NCTS.Business.NctsMessageStatusList().GetAllCodes();
			AssertContainsExactElementsInAnyOrder("ApplicationCode NCTS5", euNcts5Codes.Union(euNcts4Codes), ((CodeDescriptionPairList)messageStatusFilter.List).GetAllCodes());
		});

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new NctsMovementFilterStripBusinessObject();
		}

		protected override void SetUp()
		{
			base.SetUp();

			departure1 = Factory.New<NctsHeader>();
			departure1.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			departure2 = Factory.New<NctsHeader>();
			departure2.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			arrival1 = Factory.New<NctsHeader>();
			arrival1.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);

			arrival2 = Factory.New<NctsHeader>();
			arrival2.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		}

		NctsHeader departure1;
		NctsHeader departure2;
		NctsHeader arrival1;
		NctsHeader arrival2;
	}
}
