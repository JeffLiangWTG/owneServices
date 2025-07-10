using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class ETLineSpecialMentionInfoAdditionalInformation
{
	public ETLineSpecialMentionInfoAdditionalInformation(IETLineSpecialMentionInfoAdditionalInformation iETLineSpecialMentionInfoAdditionalInformation)
	{
		this.iETLineSpecialMentionInfoAdditionalInformation = Argument.NotNull(iETLineSpecialMentionInfoAdditionalInformation, "iETLineSpecialMentionInfoAdditionalInformation");
	}
	readonly IETLineSpecialMentionInfoAdditionalInformation iETLineSpecialMentionInfoAdditionalInformation;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 70, false)]
	[MessageFieldExportRules("O")]
	[MessageFieldExportWithTransitRules("O")]
	[MessageFieldTransitRules("O")]
	[MessageFieldInternationalRoadTransportsRules("O")]
	public ZString AdditionalInformation => iETLineSpecialMentionInfoAdditionalInformation.AdditionalInformation;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	public ZString AdditionalInformationLng => ZString.Empty;

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 5, false)]
	public ZString AdditionalInformationCoded => iETLineSpecialMentionInfoAdditionalInformation.AdditionalInformationCoded;

	[MessageLayout(Order = 3)]
	[MessageFieldBoolRepresentation]
	[MessageFieldExportWithTransitRules("D", "C75b")]
	[MessageFieldTransitRules("D", "C75b")]
	[MessageFieldInternationalRoadTransportsRules("D", "C75b")]
	public ZBool? IsExportFromCE => iETLineSpecialMentionInfoAdditionalInformation.IsExportFromCE;

	[MessageLayout(Order = 4)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	[MessageFieldExportWithTransitRules("D", "C75b")]
	[MessageFieldTransitRules("D", "C75b")]
	[MessageFieldInternationalRoadTransportsRules("D", "C75b")]
	public ZString ExportFromCountry => iETLineSpecialMentionInfoAdditionalInformation.ExportCountry;
}
