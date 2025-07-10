using Enterprise.Customs.ES.Registry;

namespace Enterprise.Customs.ES.Business;

public static class MessageVersionRegistryProvider
{
	public static bool IsExportVersionAes() => ESCustomsDataRegistry.Instance.ESExportMessageVersion.Value == EXPORTVersionNumberList.Codes.Aes;

	public static bool IsExportVersionAes11() => ESCustomsDataRegistry.Instance.ESExportMessageVersion.Value == EXPORTVersionNumberList.Codes.Aes11;

	public static bool IsExportAndAnyVersionAes() => IsExportVersionAes() || IsExportVersionAes11();

	public static bool IsT2LVersionPOUS() => ESCustomsDataRegistry.Instance.EST2LMessageVersion.Value == T2LVersionNumberList.Codes.ProofOfUnionStatus;

	public static bool IsT2LVersionPOUS2() => ESCustomsDataRegistry.Instance.EST2LMessageVersion.Value == T2LVersionNumberList.Codes.RequestJecAndReceptionPous;

	public static bool IsT2LAndAnyVersionPOUS() => IsT2LVersionPOUS() || IsT2LVersionPOUS2();

	public static bool IsT2LVersionNoPOUS() => ESCustomsDataRegistry.Instance.EST2LMessageVersion.Value == T2LVersionNumberList.Codes.NoProofOfUnionStatus;

	public static bool IsImportVersionICS() => ESCustomsDataRegistry.Instance.ESImportMessageVersion.Value == IMPORTVersionNumberList.Codes.Ics;

	public static bool IsImportVersionH1() => ESCustomsDataRegistry.Instance.ESImportMessageVersion.Value == IMPORTVersionNumberList.Codes.H1;
}
