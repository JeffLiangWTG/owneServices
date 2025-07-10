using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public abstract class PRLCONCusTempStorageLine : CusTempStorageLine
	{
		protected PRLCONCusTempStorageLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Dec

		public new PRLCONCusTempStorageDec Dec => Factory.Load<PRLCONCusTempStorageDec>(TSL_STH);

		#endregion
	}
}
