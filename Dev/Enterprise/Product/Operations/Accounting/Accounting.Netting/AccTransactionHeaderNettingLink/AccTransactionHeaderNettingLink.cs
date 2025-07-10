using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Netting
{
	public class AccTransactionHeaderNettingLink : AutoAccTransactionHeaderNettingLink
	{
		public AccTransactionHeaderNettingLink(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
