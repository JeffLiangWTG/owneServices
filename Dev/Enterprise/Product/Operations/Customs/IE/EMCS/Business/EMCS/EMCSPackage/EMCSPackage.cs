using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class EMCSPackage : EU.EMCS.Business.EMCSPackage
	{
		public EMCSPackage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override CusInvPackValidation GetNewValidation() => new EMCSPackageValidation(this);
	}
}
