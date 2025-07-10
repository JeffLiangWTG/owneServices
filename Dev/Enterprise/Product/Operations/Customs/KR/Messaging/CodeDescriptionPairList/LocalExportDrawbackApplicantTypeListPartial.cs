using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Messaging
{
	partial class LocalExportDrawbackApplicantTypeList :
		Integration.Customs.KR.IKRLocalExportDrawbackApplicantTypeCodeDescriptionPairProvider,
		DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		ReadOnlyCodeDescriptionPairList DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider.GetCodeDescriptionPairList() => new LocalExportDrawbackApplicantTypeList();
	}
}
