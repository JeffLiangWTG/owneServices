using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business
{
	public class ExitNotificationMessageSendingActionLookups : ZLookups
	{
		public ExitNotificationMessageSendingActionLookups(ExitNotificationMessageSendingAction action) : base(action)
		{
		}

		public CodeDescriptionPairList MessageTypeList => ExitSummaryMessageTypeList.DepartureMessageTypes;

		public CustomsOfficeCodeCollection IntendedExitCustomsOfficeList => CustomsOfficeCodeCollection.LocalCountryOnlyCustomsOfficesWithRequiredRoles(Factory, Core.Constants.CountryCodes.Germany, EuOfficeCodesTypes.Codes.OfficeOfExit);
	}
}
