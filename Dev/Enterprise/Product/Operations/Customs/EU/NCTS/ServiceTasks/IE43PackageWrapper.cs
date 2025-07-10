using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.ServiceTasks;

public class IE43PackageWrapper
{
	public ZString MarksAndNumbers { get; set; }
	public ZString UnitType { get; set; }
	public ZLong UnitCount { get; set; }
	public ZLong NumberOfPieces { get; set; }
}
