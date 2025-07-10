using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Customs.KR.Business
{
	public class OrgSupplierPartDataLoad : Customs.Business.GlobalOrgSupplierPartDataLoad
	{
		protected override IEnumerable<string> GetFieldNames()
		{
			return Enumerable.Empty<string>();
		}
	}
}
