using CargoWise.Types;

namespace Enterprise.Customs.JP.AFR.Business
{
	partial class MessagingTypeList
	{
		public static bool IsShippingLineManifestingMessaging(ZString code)
		{
			return code == Codes.AdvanceCargoInformationRegistrationMaster ||
				code == Codes.UpdateRegisteredAdvanceCargoInformationMaster;
		}

		public static bool IsShippingLineDepartureTimeMessaging(ZString code)
		{
			return code == Codes.DepartureTimeRegistration ||
				code == Codes.DepartureTimeCorrection;
		}
	}
}
