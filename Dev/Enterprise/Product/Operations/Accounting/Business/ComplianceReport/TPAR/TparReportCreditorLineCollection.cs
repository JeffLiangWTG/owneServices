using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ComplianceReport.TPAR
{
	[DependentBusinessObject(typeof(TparReport), "Lines")]
	public class TparReportCreditorLineCollection : DependentBusinessObjectCollection<TparReportCreditorLine, TparReport>
	{
		public TparReportCreditorLineCollection(TparReport master) : base(master)
		{
		}

		public override void Load()
		{
			base.Load(new ZQuery(AccTaxReturnLineSchema.ARL_ATR_AccTaxReturn, Master.PK));
		}

		protected override string FkColumnName => AccTaxReturnLineSchema.ARL_ATR_AccTaxReturn.Name;
	}
}
