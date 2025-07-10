using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal
{
	public class SAS155EventProcessor : AFREventProcessor
	{
		public SAS155EventProcessor(IXmlEventValueObject eventDataObject, IXmlImportLogger logger, BusinessObjectFactory factory) : base(eventDataObject, logger, factory)
		{
			sas155EventInfo = new SAS155EventInfo(eventDataObject);
		}

		protected override void ProcessCore(JPAFRHeader header)
		{
			if (header != null)
			{
				VesselInfoUpdater.UpdateVesselInformationDetails(logger, header,
					sas155EventInfo.NewCarrierCode,
					sas155EventInfo.NewVesselCallSign,
					sas155EventInfo.NewVoyageNumber,
					sas155EventInfo.NewLoadingPortCode,
					sas155EventInfo.NewLoadingPortSuffix);

				var originalCMVMessage = GetOriginalMessage(header);
				if (originalCMVMessage != null)
				{
					var cmvMessageHelper = new CMVUniversalShipmentHelper(logger, header, originalCMVMessage);

					var billsSent = cmvMessageHelper.BillsSent;
					var billsUnsent = cmvMessageHelper.BillsUnsent;
					var billsInSuccess = GetBillsInSuccess(logger, header, sas155EventInfo, billsSent, out List<JPAFRBills> billsInError);

					foreach (var bill in billsInSuccess)
					{
						VesselInfoUpdater.UpdateBillReleaseStatus(logger, bill, GetNewReleaseStatusForBillInSuccess(bill.JPB_ReleaseStatus));
					}

					foreach (var bill in billsInError)
					{
						VesselInfoUpdater.UpdateBillReleaseStatus(logger, bill, GetNewReleaseStatusForBillInError(bill.JPB_ReleaseStatus));
					}

					foreach (var bill in billsUnsent)
					{
						VesselInfoUpdater.UpdateBillReleaseStatus(logger, bill, GetNewReleaseStatusForUnsentBill(bill.JPB_ReleaseStatus));
					}
				}
			}
		}

		static IEnumerable<JPAFRBills> GetBillsInSuccess(IXmlImportLogger logger, JPAFRHeader header, SAS155EventInfo eventInfo, List<JPAFRBills> billsSent, out List<JPAFRBills> billsInError)
		{
			List<JPAFRBills> billsInSuccess;
			billsInError = new List<JPAFRBills>();
			if (eventInfo.IsNON)
			{
				billsInSuccess = billsSent;
			}
			else
			{
				foreach (var billToUpdate in eventInfo.BillsToUpdate)
				{
					var billNo = billToUpdate.BillNo;
					var bill = header.Bills.FirstOrDefault(x => x.JPB_BillNumber == billNo);
					if (bill == null)
					{
						logger.Log(Integration.LogType.Warning, string.Format(CultureInfo.InvariantCulture, "Bill '{0}' could not be found on AFR Job '{1}'.", billNo, header.JPH_JobReference));
						continue;
					}

					var processResultCode = billToUpdate.ProcessResultCode;
					if (!processResultCode.IsEmpty)
					{
						billsInError.Add(bill);
					}
				}
				billsInSuccess = billsSent.Except(billsInError).ToList();
			}
			return billsInSuccess;
		}

		static ZString GetNewReleaseStatusForUnsentBill(ZString oldStatus)
		{
			var result = oldStatus;
			switch (oldStatus)
			{
				case AFRBillCustomsStatusList.Codes.Registered:
					result = AFRBillCustomsStatusList.Codes.NL3;
					break;
				case AFRBillCustomsStatusList.Codes.NL1:
					result = AFRBillCustomsStatusList.Codes.NL4;
					break;
				case AFRBillCustomsStatusList.Codes.NL2:
					result = AFRBillCustomsStatusList.Codes.NL5;
					break;
			}
			return result;
		}

		static ZString GetNewReleaseStatusForBillInSuccess(ZString oldStatus)
		{
			var result = oldStatus;
			switch (oldStatus)
			{
				case AFRBillCustomsStatusList.Codes.NL2:
					result = AFRBillCustomsStatusList.Codes.NL5;
					break;
				case AFRBillCustomsStatusList.Codes.NL3:
					result = AFRBillCustomsStatusList.Codes.Registered;
					break;
				case AFRBillCustomsStatusList.Codes.NL4:
					result = AFRBillCustomsStatusList.Codes.NL1;
					break;
				case AFRBillCustomsStatusList.Codes.NL5:
					result = AFRBillCustomsStatusList.Codes.NL2;
					break;
			}
			return result;
		}

		static ZString GetNewReleaseStatusForBillInError(ZString oldStatus)
		{
			var result = oldStatus;
			switch (oldStatus)
			{
				case AFRBillCustomsStatusList.Codes.Registered:
					result = AFRBillCustomsStatusList.Codes.NL3;
					break;
				case AFRBillCustomsStatusList.Codes.NL1:
					result = AFRBillCustomsStatusList.Codes.NL4;
					break;
				case AFRBillCustomsStatusList.Codes.NL2:
					result = AFRBillCustomsStatusList.Codes.NL5;
					break;
			}
			return result;
		}

		protected override string GetMessageTypeDesc(JPAFRHeader header)
		{
			return MessagingTypeList.Descriptions.VesselChangeResult;
		}

		protected override bool IsSuccessResponse => true;

		readonly SAS155EventInfo sas155EventInfo;
	}
}
