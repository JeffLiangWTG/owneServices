using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Customs.IN.Business;

public class OrgSupplierPartDataLoad : Customs.Business.GlobalOrgSupplierPartDataLoad
{
	protected override IEnumerable<string> GetFieldNames() => Enumerable.Empty<string>();
}
