using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public interface IMergedPreviousDocument
{
	ZString Type { get; }
	ZString Category { get; }
	ZString Mrn { get; }
	ZString Register { get; }
	ZString ReferenceNumber { get; }
	ZString ReferenceNumberCin { get; }
	ZDate Date { get; }
	ZString Series { get; }
	ZString CustomsOffice { get; }
	ZInt ItemNumber { get; }
	ZBool IsSummaryDeclarationDocument { get; }
	ZBool IsPreviousProcedureDocument { get; }
	ZInt PackageQuantity { get; }
	ZDecimal GrossMass { get; }
	ZDecimal NetMass { get; }
	ZDecimal SupplementaryQuantity { get; }
	ZString Tariff { get; }
}
