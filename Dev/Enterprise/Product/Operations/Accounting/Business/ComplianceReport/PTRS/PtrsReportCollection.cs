using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ComplianceReport.PTRS
{
	public class PtrsReportCollection : BusinessObjectCollection<PtrsReport>
	{
		internal PtrsReportCollection(BusinessObjectFactory factory, ZGuid reportPK) : base(factory, new ZQuery(AccTaxReturnSchema.PK, reportPK))
		{
			this.Load();
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
