using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.BufferManagement.Business
{
	public delegate IEnumerable<ITagable> GetTagables(BusinessObjectFactory factory, bool isForRemovingTag);
}
