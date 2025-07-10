using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal
{
	public class AHREventProcessor : AFREventProcessor
	{
		public AHREventProcessor(IXmlEventValueObject eventDataObject, IXmlImportLogger logger, BusinessObjectFactory factory, IDataObjectWriterStrategy writerStrategy)
			: base(eventDataObject, logger, factory)
		{
			this.writerStrategy = writerStrategy;
		}
		protected readonly IDataObjectWriterStrategy writerStrategy;

		protected override string GetMessageTypeDesc(JPAFRHeader header)
		{
			return IsABillRegistrationCompletionResponse(header) ? "Advance Cargo Information Registration Completion" : "Advance Cargo Information Registration";
		}

		protected override void ProcessCore(JPAFRHeader header)
		{
			if (header != null)
			{
				var billNumber = eventDataObject.Context.HBOLNumber.GetValueOrDefault();
				if (!billNumber.IsEmpty)
				{
					var bill = header.Bills.FirstOrDefault(x => x.JPB_BillNumber == billNumber);
					if (bill == null)
					{
						logger.Log(Integration.LogType.Warning, ZString.Format("Bill '{0}' could not be found on AFR Job '{1}'.", billNumber, header.JPH_JobReference));
					}
					else
					{
						var areAllBillsRegisteredPreUpdate = header.AreAllBillsRegistered;
						var originalBillMessageStatus = bill.JPB_MessageStatus;
						UpdateBillStatus(bill);
						if (IsSuccessResponse && !areAllBillsRegisteredPreUpdate)
						{
							SendCompletionMessage(header, bill, originalBillMessageStatus);
						}
					}
				}
				else if (IsABillRegistrationCompletionResponse(header))
				{
					var success = IsSuccessResponse;
					if (IsSuccessResponse)
					{
						header.CancelBillRegistrationCompletionLog();
						header.LogBillRegistrationCompletion();
					}
					header.JPH_MessageStatus = success ? MessageStatusList.Codes.ClearHouseBillRegistrationCompletion : MessageStatusList.Codes.ErrorHouseBillRegistrationCompletion;
				}
			}
		}

		protected virtual void SendCompletionMessage(JPAFRHeader header, JPAFRBills bill, ZString originalBillMessageStatus)
		{
			if (header.AreAllBillsRegistered)
			{
				new AFRMessageGenerator(header, writerStrategy, true).SendCompletionMessageToCustoms(ActionCode.RegisterCompletionByRegistration);
			}
		}

		bool IsABillRegistrationCompletionResponse(JPAFRHeader header)
		{
			var result = false;
			if (header != null)
			{
				var originalMessage = GetOriginalMessage(header);
				result = originalMessage != null && originalMessage.EM_MessageOwner == MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouseCompletion;
			}
			return result;
		}

		protected virtual void UpdateBillStatus(JPAFRBills bill)
		{
			var isSuccess = IsSuccessResponse;
			bill.JPB_ReleaseStatus = StatusManager.GetReleaseStatus(eventDataObject, logger, bill.JPB_ReleaseStatus, isSuccess);
			bill.JPB_MessageStatus = GetMessageStatus(bill, isSuccess);
		}

		protected virtual ZString GetMessageStatus(JPAFRBills bill, bool isSuccess)
		{
			return isSuccess ? MessageStatusList.Codes.ClearHouseBillRegistration : MessageStatusList.Codes.ErrorHouseBillRegistration;
		}
	}
}
