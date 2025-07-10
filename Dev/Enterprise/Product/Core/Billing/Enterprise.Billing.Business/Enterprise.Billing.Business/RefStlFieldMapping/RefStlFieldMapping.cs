using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Billing.Business
{
	public sealed class RefStlFieldMapping : AutoRefStlFieldMapping
	{
		public RefStlFieldMapping(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
