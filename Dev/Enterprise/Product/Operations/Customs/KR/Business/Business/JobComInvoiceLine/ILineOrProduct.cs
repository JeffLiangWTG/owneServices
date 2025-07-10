using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.KR.Business
{
	public interface ILineOrProduct
	{
		bool IsExport { get; }
		bool IsImport { get; }
		ZDateTime DeclarationDate { get; }
		bool IsIssueDateRelevant { get; }
		bool IsReferenceNumberRelevant { get; }
		ZString Tariff { get; }
		GAApprovalCollection GAApprovalDataCollection { get; }
		bool IsValidationEnabled { get; }
		TariffView UniversalTariff { get; }
		HSExtensionCodeCollection HSExtensionCodeCollection { get; }
	}
}
