using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal
{
	public class SAS112EventProcessor : SAS111EventProcessor
	{
		public SAS112EventProcessor(IXmlEventValueObject eventDataObject, IXmlImportLogger logger, BusinessObjectFactory factory)
			: base(eventDataObject, logger, factory)
		{
		}

		protected override string GetMessageTypeDesc(JPAFRHeader header)
		{
			return MessagingTypeList.Descriptions.RiskAssessmentCancellation;
		}

		protected override ZString GetMessageStatus(JPAFRBills bill, bool isSuccess)
		{
			return bill.JPB_MessageStatus;
		}
	}
}
