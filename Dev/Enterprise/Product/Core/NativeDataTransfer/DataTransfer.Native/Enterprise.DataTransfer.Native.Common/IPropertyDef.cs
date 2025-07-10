using Enterprise.DataTransfer.Native.DB;

namespace Enterprise.DataTransfer.Native.Common
{
	public interface IPropertyDef
	{
		string PropertyName { get; }
		IColumnDef ColumnDef { get; }
		bool IsForExport { get; }
		bool StripCRLF { get; }
	}
}