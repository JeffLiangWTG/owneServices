using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business
{
	public class CusExitControlHeader : EU.Business.CusExitControlHeader, Integration.Customs.FR.ICusExitControlHeader
	{
		public CusExitControlHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Implementation

		public new CusExitDetailCollection CusExitDetails => (CusExitDetailCollection)base.CusExitDetails;
		protected override EU.Business.CusExitDetailCollection GetNewCusExitDetailsCollectionCore() => new CusExitDetailCollection(this);

		#endregion
	}
}
