using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal
{
	public class AMREventProcessor : AFREventProcessor
	{
		public AMREventProcessor(IXmlEventValueObject eventDataObject, IXmlImportLogger logger, BusinessObjectFactory factory)
			: base(eventDataObject, logger, factory)
		{
		}

		protected override string GetMessageTypeDesc(JPAFRHeader header)
		{
			return "Advance Cargo Information Registration";
		}

		protected override void ProcessCore(JPAFRHeader header)
		{
			if (header != null)
			{
				var billNumber = eventDataObject.Context.MBOLNumber.GetValueOrDefault();
				if (!billNumber.IsEmpty)
				{
					var bill = header.Bills.FirstOrDefault(x => x.JPB_BillNumber == billNumber);
					if (bill == null)
					{
						logger.Log(Integration.LogType.Warning, ZString.Format("Bill '{0}' could not be found on AFR Job '{1}'.", billNumber, header.JPH_JobReference));
					}
					else
					{
						UpdateBillStatus(bill);
					}
				}
			}
		}

		protected virtual void UpdateBillStatus(JPAFRBills bill)
		{
			var isSuccess = IsSuccessResponse;
			bill.JPB_ReleaseStatus = StatusManager.GetReleaseStatus(eventDataObject, logger, bill.JPB_ReleaseStatus, isSuccess);
			bill.JPB_MessageStatus = GetMessageStatus(bill, isSuccess);
		}

		protected virtual ZString GetMessageStatus(JPAFRBills bill, bool isSuccess)
		{
			return isSuccess ?
			MessageStatusList.Codes.ClearMasterBillRegistration :
			MessageStatusList.Codes.ErrorMasterBillRegistration;
		}
	}
}
