using System.Collections.Generic;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.TemporaryStorage.Module;

public class TemporaryStorageFilterStripBusinessObject : InflatableFilterStripBusinessObject
{
	protected override List<IFilterInflator> GetFilterInflators() => [];
}
