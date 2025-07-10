using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.JP.MessageContracts;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.Customs.JP.MessageDefinitions.Inbound;
using CargoWise.Types;
using Enterprise.Customs.JP.Common;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.JP.Manifest.Business.MessageProcessors;

public static class NACCSDataImporter
{
	public static void TryImport(this AsycudaManifestHeader header, IJPInboundMessageParseResult parseResult, EDIMessage naccsMessage)
	{
		switch (parseResult.MessageProvider)
		{
			case IResultWithOneMasterBillAndHouseBillsInNormalRepeat resultWithOneMasterBillAndHouseBillsInNormalRepeat:
				header.ImportResultWithOneMasterBillAndHouseBillsInNormalRepeat(resultWithOneMasterBillAndHouseBillsInNormalRepeat, naccsMessage, parseResult.IsSuccess);
				break;
			case IResultWithOneHouseBill resultWithOneHouseBill:
				header.ImportResultWithOneHouseBill(resultWithOneHouseBill, naccsMessage, parseResult.IsSuccess);
				break;
			case IResponseWithOneMasterBillAndHouseBillsInNormalRepeat responseWithOneMasterBillAndHouseBillsInNormalRepeat:
				header.ImportResponseWithOneMasterBillAndHouseBillsInNormalRepeat(responseWithOneMasterBillAndHouseBillsInNormalRepeat, naccsMessage);
				break;
			case IResponseWithOneMasterBill responseWithOneMasterBill:
				header.ImportResponseWithOneMasterBill(responseWithOneMasterBill, naccsMessage);
				break;
			case IResponseWithOneMasterBillAndHouseBillsInDoubleRepeat responseWithOneMasterBillAndHouseBillsInDoubleRepeat:
				header.ImportResponseWithOneMasterBillAndHouseBillsInDoubleRepeat(responseWithOneMasterBillAndHouseBillsInDoubleRepeat, naccsMessage);
				break;
			case IResultWithOneMasterBill response:
				header.ImportResultWithOneMasterBill(response, parseResult.IsSuccess);
				break;
		}
	}

	static void ImportResponseWithOneMasterBillAndHouseBillsInDoubleRepeat(this AsycudaManifestHeader header, IResponseWithOneMasterBillAndHouseBillsInDoubleRepeat responseWithOneMasterBillAndHouseBillsInDoubleRepeat, EDIMessage naccsMessage)
	{
		var houseBillFields = responseWithOneMasterBillAndHouseBillsInDoubleRepeat.HouseBillFields?.ToArray() ?? [];
		var hasMatchedBill = false;
		if (houseBillFields.Length == 0)
		{
			throw new JPMessageImportException(NoBillMatchedWarningMessage);
		}

		var bills = header.Bills.Cast<AsycudaBill>().ToArray();
		switch (responseWithOneMasterBillAndHouseBillsInDoubleRepeat.Schema)
		{
			case TransshipmentNoticeSubmissionInformation:
				foreach (var houseBillField in houseBillFields)
				{
					var responseRecords = houseBillField.ResponseRecords;
					foreach (var responseRecord in responseRecords)
					{
						var matchingBill = bills.FirstOrDefault(x => x.ABL_BillNumber == responseRecord.HouseBillNumber);
						if (matchingBill != null)
						{
							matchingBill.TemporaryLandingNumber = responseRecord.TemporaryLandingRegistrationNumber;
							matchingBill.TemporaryLandingStatus = TemporaryLandingStatusCodeList.Codes.REG;
							hasMatchedBill = true;
						}
					}
				}
				break;
		}

		if (!hasMatchedBill)
		{
			throw new JPMessageImportException(NoBillMatchedWarningMessage);
		}
	}

	static void ImportResponseWithOneMasterBill(this AsycudaManifestHeader header, IResponseWithOneMasterBill responseWithOneMasterBill, EDIMessage naccsMessage)
	{
		if (responseWithOneMasterBill.Schema is HBLCancellationInformation)
		{
			if (responseWithOneMasterBill.MasterBillNumber == header.AMA_MasterBill.ToString())
			{
				header.RegistrationStatus = JPCustomsStatusList.Codes.DEL;
				foreach (var bill in header.Bills)
				{
					bill.ABL_BillStatus = JPCustomsStatusList.Codes.DEL;
				}

				header.AddEventIgnoringMsgNum(AutoEvents.BillInformationDeleted, naccsMessage);
			}
			else
			{
				throw new JPMessageImportException(NoBillMatchedWarningMessage);
			}
		}
	}

	static void ImportResponseWithOneMasterBillAndHouseBillsInNormalRepeat(this AsycudaManifestHeader header, IResponseWithOneMasterBillAndHouseBillsInNormalRepeat responseWithOneMasterBillAndHouseBillsInNormalRepeat, EDIMessage naccsMessage)
	{
		var houseBillNumbers = responseWithOneMasterBillAndHouseBillsInNormalRepeat.HouseBillNumbers?.ToArray() ?? [];
		var matchedBills = FindMatchedHouseBills(header, houseBillNumbers);

		switch (responseWithOneMasterBillAndHouseBillsInNormalRepeat.Schema)
		{
			case MismatchInformation:
			case HCH01ErrorNotificationAdvanceCargoInformationForHouseManifest:
				matchedBills.ForEach(x => x.ABL_BillStatus = JPCustomsStatusList.Codes.Mismatch);
				header.AddEventIgnoringMsgNum(Events.CargoCheckinDiscrepancy, naccsMessage);
				break;
			case NotificationOfCarryInStatus:
				matchedBills.ForEach(x => x.ABL_BillStatus = JPCustomsStatusList.Codes.MOV);
				header.AddEventIgnoringMsgNum(Events.CargoCheckin, naccsMessage);
				break;
			case HBLRegistrationInformation:
				matchedBills.ForEach(x => x.ABL_BillStatus = JPCustomsStatusList.Codes.REG);
				header.AddEventIgnoringMsgNum(AutoEvents.BillInformationRegistered, naccsMessage);
				break;
			case HBLAmendmentInformation:
				matchedBills.ForEach(x => x.ABL_BillStatus = JPCustomsStatusList.Codes.AMD);
				header.AddEventIgnoringMsgNum(AutoEvents.BillInformationAmended, naccsMessage);
				break;
			case CancellationOfTransshipmentReport:
				matchedBills.ForEach(x => x.TemporaryLandingStatus = TemporaryLandingStatusCodeList.Codes.CAN);
				break;
		}
	}

	static void ImportResultWithOneMasterBillAndHouseBillsInNormalRepeat(this AsycudaManifestHeader header, IResultWithOneMasterBillAndHouseBillsInNormalRepeat resultWithOneMasterBillAndHouseBillsInNormalRepeat, EDIMessage naccsMessage, bool isSuccess)
	{
		var houseBillNumbers = resultWithOneMasterBillAndHouseBillsInNormalRepeat.HouseBillNumbers?.ToArray() ?? [];
		var matchedBills = FindMatchedHouseBills(header, houseBillNumbers);

		switch (resultWithOneMasterBillAndHouseBillsInNormalRepeat.Schema)
		{
			case NVC01Result:
				matchedBills.ForEach(x => x.UpdateBillMessageStatus(isSuccess));
				if (!isSuccess)
				{
					matchedBills.ForEach(x => x.ABL_BillStatus = ZString.Empty);
				}
				break;
			case NVC02Result:
				matchedBills.ForEach(x => x.UpdateBillMessageStatus(isSuccess));
				break;
		}
	}

	static void ImportResultWithOneHouseBill(this AsycudaManifestHeader header, IResultWithOneHouseBill resultWithOneHouseBill, EDIMessage naccsMessage, bool isSuccess)
	{
		var matchedBill = FindMatchedHouseBills(header, resultWithOneHouseBill.HouseBillNumber).FirstOrDefault();
		matchedBill.UpdateBillMessageStatus(isSuccess);

		switch (resultWithOneHouseBill.Schema)
		{
			case HCH01Result:
				if (isSuccess)
				{
					matchedBill.ABL_BillStatus = JPCustomsStatusList.Codes.REG;
					header.AddEventIgnoringMsgNum(AutoEvents.BillInformationRegistered, naccsMessage);
					if (header.MasterBill.ABL_BillStatus.Equals(JPMasterBillStatusList.Codes.AWE))
					{
						header.MasterBill.ABL_BillStatus = JPMasterBillStatusList.Codes.END;
					}
				}
				else
				{
					matchedBill.ABL_BillStatus = ZString.Empty;
					header.MasterBill.ABL_BillStatus = ZString.Empty;
				}
				break;
			case HDF01Result:
				if (isSuccess)
				{
					matchedBill.ABL_BillStatus = matchedBill.ABL_BillStatus.ToString() switch
					{
						JPCustomsStatusList.Codes.AWR => JPCustomsStatusList.Codes.REG,
						JPCustomsStatusList.Codes.AWC => JPCustomsStatusList.Codes.CAN,
						JPCustomsStatusList.Codes.AWD => JPCustomsStatusList.Codes.Deleted,
						_ => matchedBill.ABL_BillStatus
					};

					switch (matchedBill.ABL_BillStatus)
					{
						case JPCustomsStatusList.Codes.REG:
							header.AddEventIgnoringMsgNum(AutoEvents.BillInformationRegistered, naccsMessage);
							break;
						case JPCustomsStatusList.Codes.CAN:
							header.AddEventIgnoringMsgNum(AutoEvents.BillInformationCanceled, naccsMessage);
							break;
						case JPCustomsStatusList.Codes.DEL:
							header.AddEventIgnoringMsgNum(AutoEvents.BillInformationDeleted, naccsMessage);
							break;
					}
				}
				else
				{
					matchedBill.ABL_BillStatus = ZString.Empty;
				}
				break;
		}
	}

	static void ImportResultWithOneMasterBill(this AsycudaManifestHeader header, IResultWithOneMasterBill response, bool isSuccess)
	{
		if (response.Schema is HDEResult)
		{
			if (header.MasterBill is AsycudaBill masterBill && header.AMA_MasterBill == response.MasterBillNumber)
			{
				if (isSuccess)
				{
					masterBill.ABL_BillStatus = JPMasterBillStatusList.Codes.END;
				}
				else
				{
					masterBill.ABL_BillStatus = ZString.Empty;
				}
				masterBill.UpdateBillMessageStatus(isSuccess);
			}
			else
			{
				throw new JPMessageImportException(NoBillMatchedWarningMessage);
			}
		}
	}

	static AsycudaBill[] FindMatchedHouseBills(AsycudaManifestHeader header, params string[] houseBillNumbers)
	{
		var matchedHouseBills = header.Bills.Where(x => houseBillNumbers.Any(c => x.ABL_BillNumber.EqualsIgnoringCase(c))).ToArray();
		if (matchedHouseBills.Length == 0)
		{
			throw new JPMessageImportException(NoBillMatchedWarningMessage);
		}
		return matchedHouseBills;
	}

	static void UpdateBillMessageStatus(this AsycudaBill bill, bool isSuccess)
	{
		bill.ABL_MessageStatus = isSuccess ? JPMessageStatusList.Codes.Acknowledged : JPMessageStatusList.Codes.Rejected;
	}

	static string NoBillMatchedWarningMessage => Res.GetString("0613BE3D-A652-4824-A466-93A621C4C7BD", "The imported message contains no bills.");
}
