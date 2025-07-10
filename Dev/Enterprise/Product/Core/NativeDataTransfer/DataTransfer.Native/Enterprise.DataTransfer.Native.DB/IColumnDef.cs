using Enterprise.DataTransfer.Native.DB.Validators;

namespace Enterprise.DataTransfer.Native.DB
{
	/// <summary>
	/// ReadOnly Interface for Column Definition
	/// </summary>
	public interface IColumnDef
	{
		string Name { get; }
		string DataType { get; }
		Table Table { get; }
		object DefaultValue { get; }
		bool DoesNotRequireAValue { get; }
		bool Nullable { get; }
		int Length { get; }
		int Scale { get; }
		int Precision { get; }
		string HumanName { get; }
		ColumnType Type { get; }
		IColumnValidator Validator { get; }
	}
}