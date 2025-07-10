using System.Linq;
using CargoWise.Customs.IN.MessageDefinitions.AirCgmAckCHCMI02;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.IN.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IN.Manifest.Business;

public class AirCgmCHCMI02MessageProcessor : BaseManifestMessageProcessor<AirCgmAckChcmi02, CGMAsycudaManifestHeader>
{
	protected override string MessageID => IN.Business.Constants.MessageID.AirCgmAcknowledgement;

	protected override string ManifestApplicationCode => ApplicationCodeTypeList.Codes.Consolidator;

	protected override string ManifestType => INManifestTypes.Codes.CGM;

	protected override string TransportMode => Core.Constants.TransportModes.Air;

	protected override string MessageType => EDIMessageTypeList.Codes.ConsolGeneralManifest;

	protected override bool IsPositive(AirCgmAckChcmi02 airCgmCHCMI02) => airCgmCHCMI02.IsPositive();

	protected override string GetMessageSubType(AirCgmAckChcmi02 airCgmCHCMI02)
		=> airCgmCHCMI02.IsPositive() ? EDIMessageSubTypeList.Codes.AirCgmPositiveAcknowledgement : EDIMessageSubTypeList.Codes.AirCgmNegativeAcknowledgement;

	ZString GetBillNumber(AirCgmAckChcmi02 airCgmCHCMI02) => airCgmCHCMI02.Consacks?.FirstOrDefault()?.MasterAirwayBillNo;

	ZString GetFilghtNumber(AirCgmAckChcmi02 airCgmCHCMI02) => airCgmCHCMI02.Consacks?.FirstOrDefault()?.FlightNo;

	ZString GetIGMNumber(AirCgmAckChcmi02 airCgmCHCMI02) => airCgmCHCMI02.Consacks?.FirstOrDefault()?.IgmNo;

	ZString GetSenderID(AirCgmAckChcmi02 airCgmCHCMI02) => airCgmCHCMI02.Header?.Senderid;

	protected override bool ResponseDataIsValid(AirCgmAckChcmi02 airCgmCHCMI02)
	{
		return !string.IsNullOrWhiteSpace(GetBillNumber(airCgmCHCMI02));
	}

	protected override ZQuery GetManifestHeaderQuery(AirCgmAckChcmi02 airCgmCHCMI02)
	{
		var igmNumber = GetIGMNumber(airCgmCHCMI02);
		var entryNumberQuery = (ZDBOnlySubQuery)new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID, igmNumber.IsEmpty)
			.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.India)
			.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.CustomsPermitClearanceNumber)
			.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Indian.ImportGeneralManifest);

		if (igmNumber.IsEmpty)
		{
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.NotEqual, ZString.Empty);
		}
		else
		{
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, igmNumber);
		}

		var headerQuery = (ZDBOnlyQuery)base.GetManifestHeaderQuery(airCgmCHCMI02)
			.AddToFilter(AsycudaManifestHeaderSchema.AMA_Voyage, GetFilghtNumber(airCgmCHCMI02))
			.AddToFilter(AsycudaManifestHeaderSchema.AMA_CustomsOffice, GetSenderID(airCgmCHCMI02));
		headerQuery.AddSubQuery(entryNumberQuery, JoinCondition.And);
		return headerQuery;
	}

	protected override ZDBOnlySubQuery GetMasterBillQuery(AirCgmAckChcmi02 airCgmCHCMI02)
	{
		return (ZDBOnlySubQuery)base.GetMasterBillQuery(airCgmCHCMI02)
			.AddToFilter(AsycudaBillSchema.ABL_BillNumber, GetBillNumber(airCgmCHCMI02));
	}

	protected override string GetManifestMatchingCriteriaForLog(AirCgmAckChcmi02 airCgmCHCMI02)
		=> $"Bill Number: {GetBillNumber(airCgmCHCMI02)}, Filght Number: {GetFilghtNumber(airCgmCHCMI02)}, IGM Number: {GetIGMNumber(airCgmCHCMI02)}, Sender ID: {GetSenderID(airCgmCHCMI02)}";

	protected override void SetBillStatus(BusinessObject linkObject, AirCgmAckChcmi02 airCgmCHCMI02)
	{
		if (linkObject is CGMAsycudaManifestHeader header)
		{
			foreach (var bill in header.Bills)
			{
				bill.ABL_MessageStatus = airCgmCHCMI02.IsPositive(bill.ABL_BillNumber, bill.ABL_BillIssueDate.ToString("ddMMyyyy")) ? MessageStatusList.Codes.MessageAccepted : MessageStatusList.Codes.ErrorResponseReceived;
			}
		}
	}
}
