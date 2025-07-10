using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ES.ExitControl.Business
{
	public class CusExitSeal : EU.ExitControl.Business.CusExitSeal
	{
		public CusExitSeal(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Customs.Business.CusSealValidation GetNewValidation() => new CusExitSealValidation(this);

		protected override bool IsUCC6Core => Container?.IsUCC6 ?? true;
	}
}
