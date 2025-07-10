using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.Customs.JP.MessageDefinitions.Inbound;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.JP.Business.MessageProcessors;

public static class NACCSDataImporter
{
	public static void TryImport(this CusEntryHeader header, IJPInboundMessageDataProvider data)
	{
		switch (data)
		{
			case IEntryResponse entryResponse:
				header.TryImportEntryResponse(entryResponse);
				break;
			case IImportClearanceNotice notice:
				header.TryImportImportClearanceNotice(notice);
				break;
			case IExportDeclarationRegistrationCopy expRegistrationCopy:
				header.TryImportExpDeclarationRegistrationCopy(expRegistrationCopy);
				break;
			case IImportDeclarationRegistrationCopy impRegistrationCopy:
				header.TryImportImpDeclarationRegistrationCopy(impRegistrationCopy);
				break;
			case IECRResponse ecrResponse:
				header.TryImportECRResponse(ecrResponse);
				break;
		}
	}

	public static void TryImportEntryResponse(this CusEntryHeader header, IEntryResponse entryResponse)
	{
		if (!string.IsNullOrEmpty(entryResponse.EntryNumber))
		{
			header.EntryNumber = entryResponse.EntryNumber;
		}
	}

	public static void TryImportImportClearanceNotice(this CusEntryHeader header, IImportClearanceNotice notice)
	{
		var date = notice.ApprovalDate.GetValueOrDefault();
		if (header.EntryInstruction is CusEntryInstruction instruction && (instruction.CEI_Style == JPImportDeclarationTypeList.Codes.S || instruction.CEI_Style == JPImportDeclarationTypeList.Codes.M || instruction.CEI_Style == JPImportDeclarationTypeList.Codes.A))
		{
			header.InvoiceLines.Cast<JobComInvoiceLine>().Where(x => x.JI_BondedDate.IsEmpty).ForEach(x => x.JI_BondedDate = date);
		}
	}

	public static void TryImportExpDeclarationRegistrationCopy(this CusEntryHeader header, IExportDeclarationRegistrationCopy edaRegistrationCopy)
	{
		var date = edaRegistrationCopy.ScheduleDeclarationDate;
		var entryInstruction = header.EntryInstruction;
		if (entryInstruction != null && entryInstruction.CEI_DateForDuty.IsEmpty)
		{
			entryInstruction.CEI_DateForDuty = date;
		}

		var items = edaRegistrationCopy.Items;
		var cusEntryLines = header.AllEntryLines;

		foreach (var cusEntryLine in cusEntryLines)
		{
			var lineNumberToStr = cusEntryLine.CL_LineNumber.ToString();
			var item = items.FirstOrDefault(i => i.ColumnNumber.Equals(lineNumberToStr));
			if (item != null)
			{
				cusEntryLine.CL_ConfirmedCustomsValue = item.CustomsValue.GetValueOrDefault();
				cusEntryLine.CL_ParentLineNumber = item.OriginalColumnNumber;
				cusEntryLine.CL_MergedCustomsValue = item.CustomsValueSummary.GetValueOrDefault();
				cusEntryLine.CL_PriceCheck = item.PriceReconfirmationType;
			}
		}
	}

	public static void TryImportImpDeclarationRegistrationCopy(this CusEntryHeader header, IImportDeclarationRegistrationCopy idaRegistrationCopy)
	{
		var date = idaRegistrationCopy.ScheduleDeclarationDate;
		var entryInstruction = header.EntryInstruction;
		if (entryInstruction != null && entryInstruction.CEI_DateForDuty.IsEmpty)
		{
			entryInstruction.CEI_DateForDuty = date;
		}

		var items = idaRegistrationCopy.Items;
		var cusEntryLines = header.AllEntryLines;

		foreach (var cusEntryLine in cusEntryLines)
		{
			var item = items.FirstOrDefault(i => i.ColumnNumber == cusEntryLine.CL_LineNumber);
			if (item != null)
			{
				cusEntryLine.CL_ConfirmedCustomsValue = item.DutyAmount.GetValueOrDefault();
				cusEntryLine.CL_ParentLineNumber = item.OriginalColumnNumber.ToString();
				cusEntryLine.CL_MergedCustomsValue = item.DutyAmountSummary.GetValueOrDefault();
				cusEntryLine.CL_PriceCheck = item.PriceReconfirmationType;
			}
		}
	}

	public static void TryImportECRResponse(this CusEntryHeader header, IECRResponse ecrResponse)
	{
		var exportControlNumber = ecrResponse.ExportControlNumber;
		var entryInstruction = header.EntryInstruction;
		if (entryInstruction != null)
		{
			switch (ecrResponse.Schema)
			{
				case ECRRegistrationInformationResponse:
					if (entryInstruction.ExportControlNumber.IsEmpty)
					{
						var entryNum = entryInstruction.CreateNewEntryNumber(CusEntryNumberTypes.JP.ExportControlNumber, exportControlNumber, isSystemGenerated: true);
						entryInstruction.RegisterEditableChildObject(entryNum);
					}
					break;
				case ECRCancellationInformation:
					if (entryInstruction.ExportControlNumber == exportControlNumber)
					{
						entryInstruction.ExportControlNumber = ZString.Empty;
					}
					break;
			}
		}
	}
}
