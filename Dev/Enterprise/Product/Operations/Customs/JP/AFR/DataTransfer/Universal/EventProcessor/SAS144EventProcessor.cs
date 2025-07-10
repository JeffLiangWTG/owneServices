using CargoWise.EntityFramework;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal
{
	public class SAS144EventProcessor : AFREventProcessor
	{
		public SAS144EventProcessor(IXmlEventValueObject eventDataObject, IXmlImportLogger logger, BusinessObjectFactory factory) : base(eventDataObject, logger, factory)
		{
		}

		protected override void ProcessCore(JPAFRHeader header)
		{
		}

		protected override string GetMessageTypeDesc(JPAFRHeader header)
		{
			return MessagingTypeList.Descriptions.PriorNotificationOfRelevantHouseBill;
		}

		protected override bool IsSuccessResponse => true;
	}
}
