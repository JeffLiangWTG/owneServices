using CargoWise.Types;

namespace Enterprise.DocumentWrappers.Customs.EU.ImportDeclarationDocument
{
	public interface IIDDImputationSheet
	{
		ZString GoodsItemNumber { get; }
		ZString DocumentType { get; }
		ZString DocumentReference { get; }
		ZString LineItemNumber { get; }
		ZString Information { get; }
		ZString Quantity { get; }
		ZString MeasurementUnitAndQualifier { get; }
		ZString Amount { get; }
		ZString Currency { get; }
	}
}
