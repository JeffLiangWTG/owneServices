using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IN.Business.MessageSending.ExportSb;

sealed class ExportSbCACHE01AdditionalDataProvider : IExportSbCACHE01AdditionalDataProvider
{
	public ExportSbCACHE01AdditionalDataProvider(DeclarationMessageSendingObject messageSendingObject)
	{
		sendingObject = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
	}
	readonly DeclarationMessageSendingObject sendingObject;

	public string GetNatureOfCargo(CusEntryHeader businessObject)
	{
		var result = string.Empty;
		var isSea = businessObject?.Declaration?.IsSea ?? ZBool.False;
		if (isSea)
		{
			var containerMode = businessObject?.Declaration?.JE_ContainerMode ?? ZString.Empty;
			result = (string)containerMode switch
			{
				Common.IN.INContainerModeList.Codes.Containerised => Constants.NatureOfCargo.Containerised,
				Common.IN.INContainerModeList.Codes.BreakBulk => Constants.NatureOfCargo.BreakBulk,
				Common.IN.INContainerModeList.Codes.Bulk => Constants.NatureOfCargo.Bulk,
				Common.IN.INContainerModeList.Codes.Liquid => Constants.NatureOfCargo.Liquid,
				Common.IN.INContainerModeList.Codes.ContainerisedAndPackaged => Constants.NatureOfCargo.ContainerisedAndPackaged,
				_ => string.Empty,
			};
		}
		return result;
	}

	public string GetMarksNumbers(CusEntryHeader businessObject)
	{
		var invoiceHeaders = businessObject?.InvoiceHeaders;
		var result = string.Empty;

		if (invoiceHeaders?.Length > 0)
		{
			result = ZString.Join(" ", invoiceHeaders.Select(invoice => invoice.JZ_MarksAndNumbers).Where(marks => !marks.IsEmpty).ToArray()).Left(300);
		}

		return result;
	}

	public string GetNatureOfContract(JobComInvoiceHeader businessObject)
	{
		var result = string.Empty;
		var incoTermValue = businessObject?.JZ_IncoTerm ?? ZString.Empty;
		result = (string)incoTermValue switch
		{
			INIncoTermList.Codes.FreeOnBoard => Constants.IncoTerm.FOB,
			INIncoTermList.Codes.CostInsuranceAndFreight => Constants.IncoTerm.CIF,
			INIncoTermList.Codes.CostAndFreight => Constants.IncoTerm.CF,
			INIncoTermList.Codes.CostAndInsurance => Constants.IncoTerm.CI,
			_ => string.Empty,
		};
		return result;
	}

	public string GetFactoryStuffed(CusEntryHeader businessObject)
	{
		var isSeaAndContainerised = businessObject?.Declaration?.IsSeaAndContainerised ?? ZBool.False;
		if (isSeaAndContainerised)
		{
			var stuffingAt = businessObject?.Declaration?.JE_StuffingAt ?? ZString.Empty;
			return (string)stuffingAt switch
			{
				StuffingAtList.Codes.FAC => YesNoList.Codes.Yes,
				_ => YesNoList.Codes.No,
			};
		}

		return string.Empty;
	}

	public string GetJobNumber(JobComInvoiceHeader businessObject) => sendingObject.Header?.CH_BGMReference;

	public DateTime? GetJobDate(JobComInvoiceHeader businessObject) => sendingObject.Header?.CreateTime.ToDateTimeOrNullIfEmpty();

	public string GetSbNo(JobComInvoiceHeader businessObject) => sendingObject.Header?.EntryInstruction?.ShippingBillNumber;

	public DateTime? GetSbDate(JobComInvoiceHeader businessObject) => sendingObject.Header?.EntryInstruction?.ShippingBillDate.ToDateTimeOrNullIfEmpty();

	public string GetStateOfOriginExporter(CusEntryHeader businessObject)
	{
		var originState = businessObject?.Declaration?.JE_RW_NKOriginState ?? ZString.Empty;
		return ZZRefCusMapCombined.MapCW1CodeToCustomsCode(businessObject?.Factory, Core.Constants.CountryCodes.India, RefCusMapTypeList.Codes.STATE, originState, businessObject?.EffectiveValuationDate ?? ZDateTime.Empty);
	}

	public string GetSourceState(CusEntryLine businessObject)
	{
		var sourceState = businessObject?.RandomLine?.JI_StateOrRegionOfOrigin ?? ZString.Empty;
		return ZZRefCusMapCombined.MapCW1CodeToCustomsCode(businessObject?.Factory, Core.Constants.CountryCodes.India, RefCusMapTypeList.Codes.STATE, sourceState, businessObject?.Header.EffectiveValuationDate ?? ZDateTime.Empty);
	}

	public string GetPortOfFinalDestination(CusEntryHeader businessObject) => GetPortCode(businessObject.Declaration, businessObject.Declaration?.FinalDestination);

	public string GetPortOfDischarge(CusEntryHeader businessObject) => GetPortCode(businessObject.Declaration, businessObject.Declaration?.PortOfArrival);

	string GetPortCode(JobDeclaration declaration, RefUNLOCO refUNLOCO)
	{
		var result = ZString.Empty;

		if (declaration != null && refUNLOCO != null)
		{
			if (declaration.IsAir)
			{
				result = refUNLOCO.RL_IATA;
			}
			else if (declaration.IsSea)
			{
				result = refUNLOCO.RL_Code;
			}
		}
		return result;
	}

	public string GetIgstPaymentStatus(CusEntryLine businessObject)
	{
		var result = ZString.Empty;
		if (businessObject?.RandomLine is JobComInvoiceLine invoiceLine)
		{
			result = invoiceLine.JI_GSTPayNotApplicable ? IGSTPaymentStatusCodeList.Codes.NotApplicable : invoiceLine.InvoiceHeader?.JZ_GSTPaymentStatus ?? ZString.Empty;
		}
		return result;
	}

	bool IsAmendment => sendingObject.MessageType != DeclarationMessageTypeList.Codes.Fresh;

	public string GetAmendmentType(CusEntryHeader businessObject) => IsAmendment ? sendingObject.MessageType : ZString.Empty;

	public string GetAmendmentNo(CusEntryHeader businessObject)
	{
		return IsAmendment ? Constants.Messaging.INMessageNumPlaceHolder : ZString.Empty;
	}

	public DateTime? GetAmendmentDate(CusEntryHeader businessObject)
	{
		return IsAmendment ? ZDateTime.Now.ToDateTime() : null;
	}

	public string GetMessageType(JobComInvoiceHeader businessObject) => sendingObject.MessageType;

	public string GetMessageType(SWConstituent businessObject) => sendingObject.MessageType;

	public string GetMessageType(JobWork businessObject) => sendingObject.MessageType;

	public string GetMessageType(CusEntryHeader businessObject) => sendingObject.MessageType;

	public string GetMessageType(CusEntryLine businessObject) => sendingObject.MessageType;

	public string GetMessageType(CusContainerOnEntryInstruction businessObject) => sendingObject.MessageType;
}
