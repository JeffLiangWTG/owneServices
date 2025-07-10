using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

public interface ICheckablePackage
{
	ZString UnitType { get; }
	ZString MarksAndNumbers { get; }
	ZLong UnitCount { get; }
	ZPropertyInfo UnitCountInfo { get; }
}
