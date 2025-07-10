using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal
{
	public class CHREventProcessor : AHREventProcessor
	{
		public CHREventProcessor(IXmlEventValueObject eventDataObject, IXmlImportLogger logger, BusinessObjectFactory factory, IDataObjectWriterStrategy writerStrategy)
			: base(eventDataObject, logger, factory, writerStrategy)
		{
		}

		protected override string GetMessageTypeDesc(JPAFRHeader header)
		{
			return "Update Advance Cargo Information Registration";
		}

		protected override void UpdateBillStatus(JPAFRBills bill)
		{
			base.UpdateBillStatus(bill);
			if (bill.JPB_MessageStatus == MessageStatusList.Codes.ClearHouseBillDelete)
			{
				// TODO: If the status is "HLD" and users send a delete; should we still keep HLD?
				bill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.NotRegistered;
				var header = bill.Header;
				if (header.IsBillRegistrationCompleted && !header.IsAnyBillAlreadyRegistered)
				{
					header.CancelBillRegistrationCompletionLog();
				}
			}
		}

		protected override void SendCompletionMessage(JPAFRHeader header, JPAFRBills bill, ZString originalBillMessageStatus)
		{
			if (originalBillMessageStatus == MessageStatusList.Codes.AwaitingHouseBillAdd && header.Bills.Where(x => x.JPB_BillNumber != bill.JPB_BillNumber).All(x => x.IsBillAlreadyRegistered))
			{
				new AFRMessageGenerator(header, writerStrategy, true).SendCompletionMessageToCustoms(ActionCode.RegisterCompletionByAmendment);
			}
			else
			{
				base.SendCompletionMessage(header, bill, originalBillMessageStatus);
			}
		}

		protected override ZString GetMessageStatus(JPAFRBills bill, bool isSuccess)
		{
			var result = bill.JPB_MessageStatus;
			switch (result)
			{
				case MessageStatusList.Codes.AwaitingHouseBillAdd:
					result = isSuccess ? MessageStatusList.Codes.ClearHouseBillAdd : MessageStatusList.Codes.ErrorHouseBillAdd;
					break;
				case MessageStatusList.Codes.AwaitingHouseBillDelete:
					result = isSuccess ? MessageStatusList.Codes.ClearHouseBillDelete : MessageStatusList.Codes.ErrorHouseBillDelete;
					break;
				case MessageStatusList.Codes.AwaitingHouseBillUpdate:
					result = isSuccess ? MessageStatusList.Codes.ClearHouseBillUpdate : MessageStatusList.Codes.ErrorHouseBillUpdate;
					break;
			}
			return result;
		}
	}
}
