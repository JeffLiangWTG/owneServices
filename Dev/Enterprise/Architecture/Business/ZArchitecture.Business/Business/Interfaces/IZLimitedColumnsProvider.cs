using CargoWise.Schema;

namespace Enterprise.ZArchitecture.Business
{
	public interface IZLimitedColumnsProvider
	{
		SchemaColumn CodeSchemaColumn { get; }
		SchemaColumn DescriptionSchemaColumn { get; }
	}
}
