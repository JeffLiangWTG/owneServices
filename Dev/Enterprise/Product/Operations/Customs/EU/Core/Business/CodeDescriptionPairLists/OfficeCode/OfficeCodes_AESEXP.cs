using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.EU.Business
{
	[CodeAlive("This class was first used for DE, now DE has its own office codes.")]
	public class OfficeCodes_AESEXP : CodeDescriptionPairList
	{
		public OfficeCodes_AESEXP()
		{
			AddPair(EuOfficeCodesTypes.Codes.ActualExitOffice, EuOfficeCodesTypes.Descriptions.ActualExitOffice);
			AddPair(EuOfficeCodesTypes.Codes.SupplementaryDeclarationOffice, EuOfficeCodesTypes.Descriptions.SupplementaryDeclarationOffice);
			AddPair(EuOfficeCodesTypes.Codes.OfficeOfExport, EuOfficeCodesTypes.Descriptions.OfficeOfExport);
			AddPair(EuOfficeCodesTypes.Codes.OfficeOfExit, EuOfficeCodesTypes.Descriptions.OfficeOfExit);
		}
	}
}
