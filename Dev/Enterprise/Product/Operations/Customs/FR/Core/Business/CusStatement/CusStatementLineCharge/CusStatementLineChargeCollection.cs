using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.CusStatement
{
	public class CusStatementLineChargeCollection : DependentBusinessObjectCollection<CusStatementLineCharge, CusStatementLine>
	{
		public CusStatementLineChargeCollection(CusStatementChargesDetail closingDeclarationDetail)
			: base(closingDeclarationDetail)
		{
		}
	}
}
