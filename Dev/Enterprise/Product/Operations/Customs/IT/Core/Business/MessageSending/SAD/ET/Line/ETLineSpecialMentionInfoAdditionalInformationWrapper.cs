using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class ETLineSpecialMentionInfoAdditionalInformationWrapper : IETLineSpecialMentionInfoAdditionalInformation
{
	public ZString AdditionalInformation => ZString.Empty;

	public ZString AdditionalInformationCoded => ZString.Empty;

	public ZBool? IsExportFromCE => null;

	public ZString ExportCountry => ZString.Empty;
}
