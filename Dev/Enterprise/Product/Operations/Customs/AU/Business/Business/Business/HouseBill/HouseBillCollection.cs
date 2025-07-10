using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class BillCollection : Customs.Business.BillCollection<Bill, JobDeclaration>
	{
		public BillCollection(JobDeclaration jobDeclaration, BusinessObjectFactory factory)
			: base(jobDeclaration, factory)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			if (Declaration.IsPost)
			{
				((Bill)child).CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			}
		}
	}
}
