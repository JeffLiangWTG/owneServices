using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal
{
	public class CMREventProcessor : AMREventProcessor
	{
		public CMREventProcessor(IXmlEventValueObject eventDataObject, IXmlImportLogger logger, BusinessObjectFactory factory)
			: base(eventDataObject, logger, factory)
		{
		}

		protected override string GetMessageTypeDesc(JPAFRHeader header)
		{
			return "Update Advance Cargo Information Registration";
		}

		protected override void UpdateBillStatus(JPAFRBills bill)
		{
			base.UpdateBillStatus(bill);
			if (bill.JPB_MessageStatus == MessageStatusList.Codes.ClearMasterBillDelete)
			{
				bill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.NotRegistered;
			}
		}

		protected override ZString GetMessageStatus(JPAFRBills bill, bool isSuccess)
		{
			var result = bill.JPB_MessageStatus;
			switch (result)
			{
				case MessageStatusList.Codes.AwaitingMasterBillAddAfterATD:
					result = isSuccess ? MessageStatusList.Codes.ClearMasterBillAddAfterATD : MessageStatusList.Codes.ErrorMasterBillAddAfterATD;
					break;
				case MessageStatusList.Codes.AwaitingMasterBillDelete:
					result = isSuccess ? MessageStatusList.Codes.ClearMasterBillDelete : MessageStatusList.Codes.ErrorMasterBillDelete;
					break;
				case MessageStatusList.Codes.AwaitingMasterBillUpdate:
					result = isSuccess ? MessageStatusList.Codes.ClearMasterBillUpdate : MessageStatusList.Codes.ErrorMasterBillUpdate;
					break;
			}
			return result;
		}
	}
}
