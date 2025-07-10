using CargoWise.EntityFramework;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal
{
	public class ATDEventProcessor : AFREventProcessor
	{
		public ATDEventProcessor(IXmlEventValueObject eventDataObject, IXmlImportLogger logger, BusinessObjectFactory factory)
			: base(eventDataObject, logger, factory)
		{
		}

		protected override string GetMessageTypeDesc(JPAFRHeader header)
		{
			return MessagingTypeList.Descriptions.DepartureTimeRegistration;
		}

		protected override void ProcessCore(JPAFRHeader header)
		{
			if (header != null)
			{
				var isSuccess = IsSuccessResponse;
				if (eventDataObject.DataContext != null && eventDataObject.DataContext.ActionPurposeCode == MessagingTypeList.Codes.DepartureTimeRegistration)
				{
					header.JPH_MessageStatus = isSuccess ? MessageStatusList.Codes.ClearDepartureTimeRegistration : MessageStatusList.Codes.ErrorDepartureTimeRegistration;
				}

				if (isSuccess)
				{
					header.CancelDepartureTimeRegistrationLog();
					header.LogDepartureTimeRegistration();
				}
			}
		}
	}
}
