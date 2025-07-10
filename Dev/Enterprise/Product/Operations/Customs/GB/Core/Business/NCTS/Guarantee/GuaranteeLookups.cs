using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business
{
	public class GuaranteeLookups : NctsGuaranteeLookups
	{
		public GuaranteeLookups(Guarantee parent) : base(parent)
		{
		}

		protected new Guarantee Parent => (Guarantee)base.Parent;

		protected override IReadOnlyList<ZString> GuaranteeTypeFilter => new ZString[] { GBGuaranteeTypeList.Codes.TRA, GBGuaranteeTypeList.Codes.GEN, GBGuaranteeTypeList.Codes.COM };
	}
}
