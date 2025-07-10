using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class NctsPackage : EU.NCTS.Business.NctsPackage
	{
		public NctsPackage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override CusInvPackValidation GetNewPhase5Validation() => new NctsPackagePhase5Validation(this);
	}
}
