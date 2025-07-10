using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Integration;

namespace Enterprise.ZArchitecture.Business
{
	public interface IPivotBusinessObjectCollection : IActiveBusinessObjectCollection, IEnumerable<IPivotBusinessObject>
	{
		IPivotBusinessObject AddRelatedIfNotExist(BusinessObject item, bool addAlwaysAsParent = false);
		IPivotBusinessObject FindRelated(ZGuid pk);
	}
}
