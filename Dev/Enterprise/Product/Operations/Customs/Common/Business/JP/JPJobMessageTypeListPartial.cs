using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.JP;

public partial class JPJobMessageTypeList
{
	public static CodeDescriptionPairList ImportOnly()
	{
		return
		[
			new CodeDescriptionPair(Codes.Import, Descriptions.Import)
		];
	}

	public static CodeDescriptionPairList ExportOnly()
	{
		return
		[
			new CodeDescriptionPair(Codes.Export, Descriptions.Export)
		];
	}
}
