using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.CusStatement
{
	public class CusStatementEntryCollection : DependentBusinessObjectCollection<CusStatementEntry, CusStatementHeader>
	{
		public CusStatementEntryCollection(CusStatementHeader parent) : base(parent)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			SetDefaultEntryType((CusStatementEntry)child);
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var query = base.CreateAdditionalFilter();
			query.AddToFilter(CusStatementLineSchema.B3_EntryType, new StatementEntryTypeImpExpList().GetAllCodes());
			return query;
		}

		void SetDefaultEntryType(CusStatementEntry child)
		{
			child.B3_EntryType = child.StatementHeader.B2_BranchDesignation;
		}
	}
}
