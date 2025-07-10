using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal
{
	public class SAS111EventProcessor : AHREventProcessor
	{
		public SAS111EventProcessor(IXmlEventValueObject eventDataObject, IXmlImportLogger logger, BusinessObjectFactory factory)
			: base(eventDataObject, logger, factory, DefaultDataObjectWriterStrategy.Instance)
		{
		}

		protected override string GetMessageTypeDesc(JPAFRHeader header)
		{
			return MessagingTypeList.Descriptions.RiskAssessmentResult;
		}

		protected override bool IsSuccessResponse
		{
			get { return true; }
		}

		protected override ZString GetMessageStatus(JPAFRBills bill, bool isSuccess)
		{
			var code = eventDataObject.EventReference;
			switch (code)
			{
				case AFRBillCustomsStatusList.Codes.DoNotLoad:
				case AFRBillCustomsStatusList.Codes.DoNotUnload:
				case AFRBillCustomsStatusList.Codes.HLD:
					return MessageStatusList.Codes.RiskAssessmentResultReceived;
				default:
					return bill.JPB_MessageStatus;
			}
		}
	}
}
