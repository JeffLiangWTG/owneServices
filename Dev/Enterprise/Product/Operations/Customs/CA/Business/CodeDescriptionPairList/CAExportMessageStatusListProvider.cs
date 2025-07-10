using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CA.Business;

public class CAExportMessageStatusListProvider : Integration.Customs.CA.ICAExportMessageStatusListProvider, DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
{
	public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
	{
		var result = new MessageStatusList(MessageTypeList.Descriptions.G7Export);
		result.RemoveCode(MessageStatusList.Codes.Sent);
		result.RemoveCode(MessageStatusList.Codes.NotSent);
		result.RemoveCode(MessageStatusList.Codes.Unknown);
		return result;
	}
}
