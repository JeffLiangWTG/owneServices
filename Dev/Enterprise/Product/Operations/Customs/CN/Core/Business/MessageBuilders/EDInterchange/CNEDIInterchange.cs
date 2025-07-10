using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.CN.Business
{
	public class CNEDIInterchange : EDIInterchange
	{
		public CNEDIInterchange(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		protected override ZString GetInterchangeNumber()
		{
			return base.GetInterchangeNumber().PadLeft(20, '0');
		}
	}
}
