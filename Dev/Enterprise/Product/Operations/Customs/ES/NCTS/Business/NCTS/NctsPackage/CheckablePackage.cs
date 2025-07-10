using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public interface ICheckablePackage
	{
		ZString UnitType { get; }
		ZString MarksAndNumbers { get; }
		ZLong UnitCount { get; }
		ZPropertyInfo UnitCountInfo { get; }

		IEnumerable<ICheckablePackage> GetRelatedEntryPackages();
	}
}
