using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ES.Messaging.Test.CodeDescriptionPairList
{
	public class DeclarationMessageTypeListPartialTest : TestCaseWithFactory
	{
		public void TestIsArrivalWithOBS()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected true IsArrivalWithOBS (message type is OBS)", true, DeclarationMessageTypeList.IsArrivalWithOBS(DeclarationMessageTypeList.Codes.NctsUnloadingRemarks));
				AssertEquals("Expected false IsArrivalWithOBS (message type is AVI)", false, DeclarationMessageTypeList.IsArrivalWithOBS(DeclarationMessageTypeList.Codes.NctsArrivalNotification));
				AssertEquals("Expected true IsArrivalWithOBS (message type is AVO)", true, DeclarationMessageTypeList.IsArrivalWithOBS(DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithUnloadingRemarksAviPlusObs));
				AssertEquals("Expected false IsArrivalWithOBS (message type is TNA)", false, DeclarationMessageTypeList.IsArrivalWithOBS(DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureTnnPlusAvi));
				AssertEquals("Expected true IsArrivalWithOBS (message type is TAO)", true, DeclarationMessageTypeList.IsArrivalWithOBS(DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureAndUnloadingRemarksTnnPlusAviPlusOb));
			});
		}

		public void TestIsArrivalWithAVI()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected false IsArrivalWithAVI (message type is OBS)", false, DeclarationMessageTypeList.IsArrivalWithAVI(DeclarationMessageTypeList.Codes.NctsUnloadingRemarks));
				AssertEquals("Expected true IsArrivalWithAVI (message type is AVI)", true, DeclarationMessageTypeList.IsArrivalWithAVI(DeclarationMessageTypeList.Codes.NctsArrivalNotification));
				AssertEquals("Expected true IsArrivalWithAVI (message type is AVO)", true, DeclarationMessageTypeList.IsArrivalWithAVI(DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithUnloadingRemarksAviPlusObs));
				AssertEquals("Expected true IsArrivalWithAVI (message type is TNA)", true, DeclarationMessageTypeList.IsArrivalWithAVI(DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureTnnPlusAvi));
				AssertEquals("Expected true IsArrivalWithAVI (message type is TAO)", true, DeclarationMessageTypeList.IsArrivalWithAVI(DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureAndUnloadingRemarksTnnPlusAviPlusOb));
			});
		}

		public void TestIsArrivalWithTNN()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected false IsArrivalWithTNN (message type is OBS)", false, DeclarationMessageTypeList.IsArrivalWithTNN(DeclarationMessageTypeList.Codes.NctsUnloadingRemarks));
				AssertEquals("Expected false IsArrivalWithTNN (message type is AVI)", false, DeclarationMessageTypeList.IsArrivalWithTNN(DeclarationMessageTypeList.Codes.NctsArrivalNotification));
				AssertEquals("Expected false IsArrivalWithTNN (message type is AVO)", false, DeclarationMessageTypeList.IsArrivalWithTNN(DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithUnloadingRemarksAviPlusObs));
				AssertEquals("Expected true IsArrivalWithTNN (message type is TNA)", true, DeclarationMessageTypeList.IsArrivalWithTNN(DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureTnnPlusAvi));
				AssertEquals("Expected true IsArrivalWithTNN (message type is TAO)", true, DeclarationMessageTypeList.IsArrivalWithTNN(DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureAndUnloadingRemarksTnnPlusAviPlusOb));
			});
		}
	}
}
