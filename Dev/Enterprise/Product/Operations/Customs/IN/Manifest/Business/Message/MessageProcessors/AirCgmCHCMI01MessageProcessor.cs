using System;
using CargoWise.Common;
using CargoWise.Customs.IN.MessageDefinitions.AirCgmCMCHI01;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IN.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IN.Manifest.Business;

public class AirCgmCHCMI01MessageProcessor : BaseManifestMessageProcessor<AirCgmCmchi01, CGMAsycudaManifestHeader>
{
	protected override string MessageID => IN.Business.Constants.MessageID.AirCgm;

	protected override string ManifestApplicationCode => ApplicationCodeTypeList.Codes.Consolidator;

	protected override string ManifestType => INManifestTypes.Codes.CGM;

	protected override string TransportMode => Core.Constants.TransportModes.Air;

	protected override string MessageType => EDIMessageTypeList.Codes.ConsolGeneralManifest;

	protected override bool IsPositive(AirCgmCmchi01 airCgmCHCMI01) => false;

	protected override string GetMessageSubType(AirCgmCmchi01 airCgmCHCMI01) => EDIMessageSubTypeList.Codes.AirCgmNegativeAcknowledgement;

	ZString GetBillNumber(AirCgmCmchi01 airCgmCHCMI01) => airCgmCHCMI01.Consoligm?.ConsMaster?.MawbNo;

	ZDateTime GetBillIssueDate(AirCgmCmchi01 airCgmCHCMI01) => ConvertToZDateTime(airCgmCHCMI01.Consoligm?.ConsMaster?.MawbDate);

	ZString GetFlightNumber(AirCgmCmchi01 airCgmCHCMI01) => airCgmCHCMI01.Consoligm?.ConsMaster?.FlightNo;

	ZString GetMessageNumber(AirCgmCmchi01 airCgmCHCMI01) => airCgmCHCMI01.Header?.SequenceOrControlNo;

	ZString GetCustomsHouse(AirCgmCmchi01 airCgmCHCMI01) => airCgmCHCMI01.Header?.ReceiverId;

	protected override bool ResponseDataIsValid(AirCgmCmchi01 airCgmCHCMI01)
	{
		return !string.IsNullOrWhiteSpace(GetBillNumber(airCgmCHCMI01));
	}

	protected override ZQuery GetManifestHeaderQuery(AirCgmCmchi01 airCgmCHCMI01)
	{
		var messageQuery = (ZDBOnlySubQuery)new ZDBOnlySubQuery(typeof(EDIMessage), EDIMessageSchema.EM_LinkUniqueID)
			.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, Messaging.Business.EDIMessage.Direction.Transmit)
			.AddToFilter(EDIMessageSchema.EM_MessageSubType, EDIMessageSubTypeList.Codes.AirCgm)
			.AddToFilter(EDIMessageSchema.EM_MessageOwner, GetCustomsHouse(airCgmCHCMI01))
			.AddToFilter(EDIMessageSchema.EM_MessageNum, GetMessageNumber(airCgmCHCMI01));

		var headerQuery = (ZDBOnlyQuery)base.GetManifestHeaderQuery(airCgmCHCMI01)
			.AddToFilter(AsycudaManifestHeaderSchema.AMA_CustomsOffice, GetCustomsHouse(airCgmCHCMI01))
			.AddToFilter(AsycudaManifestHeaderSchema.AMA_Voyage, GetFlightNumber(airCgmCHCMI01));
		headerQuery.AddSubQuery(messageQuery, JoinCondition.And);
		return headerQuery;
	}

	protected override ZDBOnlySubQuery GetMasterBillQuery(AirCgmCmchi01 airCgmCHCMI01)
	{
		return (ZDBOnlySubQuery)base.GetMasterBillQuery(airCgmCHCMI01)
			.AddToFilter(AsycudaBillSchema.ABL_BillNumber, GetBillNumber(airCgmCHCMI01))
			.AddToFilter(AsycudaBillSchema.ABL_BillIssueDate, GetBillIssueDate(airCgmCHCMI01));
	}

	protected override string GetManifestMatchingCriteriaForLog(AirCgmCmchi01 airCgmCHCMI01)
		=> $"Bill Number: {GetBillNumber(airCgmCHCMI01)}, Bill Issue Date: {GetBillIssueDate(airCgmCHCMI01)}, Filght Number: {GetFlightNumber(airCgmCHCMI01)}, Message Number: {GetMessageNumber(airCgmCHCMI01)}, Customs House Code: {GetCustomsHouse(airCgmCHCMI01)}";

	ZDateTime ConvertToZDateTime(string date)
	{
		if (DateTime.TryParseExact(date, "ddMMyyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
		{
			return new ZDateTime(parsedDate);
		}
		else
		{
			ErrorReporter.ReportOnce("Invalid date format. Expected format should be ddMMyyyy for Bill Issue Date");
			return ZDateTime.Empty;
		}
	}
}
