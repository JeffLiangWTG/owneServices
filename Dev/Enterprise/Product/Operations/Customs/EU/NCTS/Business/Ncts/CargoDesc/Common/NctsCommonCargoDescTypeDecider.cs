using System;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsCommonCargoDescTypeDecider : CusInBondCargoDescTypeDecider
	{
		public override Type GetTypeForNew() => typeof(NctsCommonCargoDesc);

		public override Type GetTypeForBinding() => typeof(NctsCommonCargoDesc);
	}
}
