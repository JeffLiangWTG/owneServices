using CargoWise.EntityFramework;

namespace Enterprise.Registry.Business.Warehouse
{
	public interface IPutawaySequenceValidatorConsumer
	{
		ZPropertyInfo ClientAreaInfo { get; }
		ZPropertyInfo ColumnInfo { get; }
		ZPropertyInfo LocationInfo { get; }
		ZPropertyInfo ProductAreaInfo { get; }
		ZPropertyInfo LevelInfo { get; }
		ZPropertyInfo PickFaceInfo { get; }
		ZPropertyInfo RowInfo { get; }
	}
}
