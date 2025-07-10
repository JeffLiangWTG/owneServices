using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	class MessagingTypeListTest : TestCaseWithFactory
	{
		public void TestIsShippingLineManifestingMessaging()
		{
			var targetCodes = new[] {
				MessagingTypeList.Codes.AdvanceCargoInformationRegistrationMaster,
				MessagingTypeList.Codes.UpdateRegisteredAdvanceCargoInformationMaster
			};

			foreach (ICodeDescription pair in new MessagingTypeList())
			{
				AssertEquals(pair.Code, targetCodes.Contains(pair.Code), MessagingTypeList.IsShippingLineManifestingMessaging(pair.Code));
			}
		}

		public void TestIsShippingLineDepartureTimeMessaging()
		{
			var targetCodes = new[] {
				MessagingTypeList.Codes.DepartureTimeRegistration,
				MessagingTypeList.Codes.DepartureTimeCorrection
			};

			foreach (ICodeDescription pair in new MessagingTypeList())
			{
				AssertEquals(pair.Code, targetCodes.Contains(pair.Code), MessagingTypeList.IsShippingLineDepartureTimeMessaging(pair.Code));
			}
		}
	}
}
