using CargoWise.EntityFramework;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal
{
	public class SAS135EventProcessor : AFREventProcessor
	{
		public SAS135EventProcessor(IXmlEventValueObject eventDataObject, IXmlImportLogger logger, BusinessObjectFactory factory)
			: base(eventDataObject, logger, factory)
		{
		}

		protected override string GetMessageTypeDesc(JPAFRHeader header)
		{
			return MessagingTypeList.Descriptions.NotificationOfHouseBillOfLadingRegisterCompletion;
		}

		protected override bool IsSuccessResponse
		{
			get { return true; }
		}

		protected override void ProcessCore(JPAFRHeader header)
		{
		}
	}
}
