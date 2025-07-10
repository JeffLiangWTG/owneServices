using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class NctsGuarantee : EU.NCTS.Business.NctsGuarantee
	{
		public NctsGuarantee(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new CusGuaranteeHeader CusGuarantee => (CusGuaranteeHeader)base.CusGuarantee;

		public new NctsHeader NctsHeader => (NctsHeader)base.NctsHeader;

		protected override EU.NCTS.Business.NctsGuaranteeValidation GetNewPhase4Validation() => new NctsGuaranteeValidation(this);

		protected override Func<EU.Business.CusGuaranteeHeader, bool> CusGuaranteeTypeFilter => _ => true;

		protected override ZDecimal Phase5DefaultLiabilityAmount => 10000m;
	}
}
