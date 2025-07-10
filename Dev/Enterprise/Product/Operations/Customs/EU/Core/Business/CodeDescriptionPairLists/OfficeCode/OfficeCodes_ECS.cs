using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business
{
	public class OfficeCodes_ECS : CodeDescriptionPairList
	{
		public OfficeCodes_ECS()
		{
			AddPair(EuOfficeCodesTypes.Codes.OfficeOfExit, EuOfficeCodesTypes.Descriptions.OfficeOfExit);
			AddPair(EuOfficeCodesTypes.Codes.OfficeOfExport, EuOfficeCodesTypes.Descriptions.OfficeOfExport);
		}
	}
}
