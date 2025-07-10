using System.Linq;
using CargoWise.Customs.IN.MessageDefinitions.SeaCgmAckCHCMI21A;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IN.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IN.Manifest.Business;

public class SeaCgmCHCMI21AMessageProcessor : BaseManifestMessageProcessor<SeaCgmAckChcmi21A, CGMAsycudaManifestHeader>
{
	protected override string MessageID => IN.Business.Constants.MessageID.SeaCgmAcknowledgement;

	protected override string ManifestApplicationCode => ApplicationCodeTypeList.Codes.Consolidator;

	protected override string ManifestType => INManifestTypes.Codes.CGM;

	protected override string TransportMode => Core.Constants.TransportModes.Sea;

	protected override string MessageType => EDIMessageTypeList.Codes.ConsolGeneralManifest;

	protected override bool IsPositive(SeaCgmAckChcmi21A seaCgmCHCMI21A) => seaCgmCHCMI21A.IsPositive();

	protected override string GetMessageSubType(SeaCgmAckChcmi21A seaCgmCHCMI21A)
		=> seaCgmCHCMI21A.IsPositive() ? EDIMessageSubTypeList.Codes.SeaCgmPositiveAcknowledgement : EDIMessageSubTypeList.Codes.SeaCgmNegativeAcknowledgement;

	ZString GetBillNumber(SeaCgmAckChcmi21A seaCgmCHCMI21A) => seaCgmCHCMI21A.Consacks?.FirstOrDefault()?.MasterBlNo;

	ZString GetVesselCode(SeaCgmAckChcmi21A seaCgmCHCMI21A) => seaCgmCHCMI21A.Consacks?.FirstOrDefault()?.VesselCode;

	ZString GetVoyageNumber(SeaCgmAckChcmi21A seaCgmCHCMI21A) => seaCgmCHCMI21A.Consacks?.FirstOrDefault()?.VoyageNumber;

	ZString GetLineNumber(SeaCgmAckChcmi21A seaCgmCHCMI21A) => seaCgmCHCMI21A.Consacks?.FirstOrDefault()?.LineNumber;

	protected override bool ResponseDataIsValid(SeaCgmAckChcmi21A seaCgmCHCMI21A)
	{
		return !string.IsNullOrWhiteSpace(GetBillNumber(seaCgmCHCMI21A));
	}

	protected override ZQuery GetManifestHeaderQuery(SeaCgmAckChcmi21A seaCgmCHCMI21A)
	{
		return base.GetManifestHeaderQuery(seaCgmCHCMI21A)
				.AddToFilter(AsycudaManifestHeaderSchema.AMA_Voyage, GetVoyageNumber(seaCgmCHCMI21A))
				.AddToFilter(AsycudaManifestHeaderSchema.AMA_RadioCallSign, GetVesselCode(seaCgmCHCMI21A));
	}

	protected override ZDBOnlySubQuery GetMasterBillQuery(SeaCgmAckChcmi21A seaCgmCHCMI21A)
	{
		return (ZDBOnlySubQuery)base.GetMasterBillQuery(seaCgmCHCMI21A)
			.AddToFilter(AsycudaBillSchema.ABL_BillNumber, GetBillNumber(seaCgmCHCMI21A))
			.AddToFilter(AsycudaBillSchema.ABL_CarrierReference, GetLineNumber(seaCgmCHCMI21A));
	}

	protected override string GetManifestMatchingCriteriaForLog(SeaCgmAckChcmi21A seaCgmCHCMI21A)
		=> $"Bill Number: {GetBillNumber(seaCgmCHCMI21A)}, Vessel Code: {GetVesselCode(seaCgmCHCMI21A)}, Voyage Number: {GetVoyageNumber(seaCgmCHCMI21A)}, Line Number: {GetLineNumber(seaCgmCHCMI21A)}";
}
