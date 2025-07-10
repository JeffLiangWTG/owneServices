using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NctsPackage : EU.NCTS.Business.NctsPackage, Integration.Customs.DE.INctsPackage
	{
		public NctsPackage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new NctsPackageValidation Validation
		{
			get { return (NctsPackageValidation)base.Validation; }
		}

		protected override CusInvPackValidation GetNewPhase5Validation() => new NctsPackageValidation(this);

		public new int B5_MarksAndNumbersMaxLength =>
			IsPhase5Departure
			? IsInPhase5TransitionPeriod ? 42 : 512
			: base.B5_MarksAndNumbersMaxLength;
	}
}
