using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.SAD;

public interface IETLineSpecialMentionInfoAdditionalInformation
{
	ZString AdditionalInformation { get; }
	ZString AdditionalInformationCoded { get; }
	ZBool? IsExportFromCE { get; }
	ZString ExportCountry { get; }
}
