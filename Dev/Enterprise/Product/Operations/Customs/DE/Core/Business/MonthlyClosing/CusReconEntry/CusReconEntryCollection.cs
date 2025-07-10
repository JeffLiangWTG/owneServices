using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business
{
	public class CusReconEntryCollection : Customs.Business.CusReconEntryCollection
	{
		public CusReconEntryCollection(CusReconDeclaration reconDeclaration) : base(reconDeclaration)
		{
			this.EnableMaxCountValidation(999, string.Empty, false);
		}

		public CusReconEntryCollection(BusinessObjectFactory factory, ZGuid branchPk, ZDate periodFrom, ZDate periodTo) : base(factory, SetCollectionFilter(branchPk, periodFrom, periodTo))
		{
		}

		static ZQuery SetCollectionFilter(ZGuid branchPk, ZDate periodFrom, ZDate periodTo)
		{
			var query = new ZQuery();
			if (!branchPk.IsValid || !periodFrom.IsValid || !periodTo.IsValid)
			{
				query.IsNoResultQuery = true;
			}
			else
			{
				query.AddToFilter(CusReconEntrySchema.CRE_CRD, null);
				query.AddToFilter(CusReconEntrySchema.CRE_EntryDate, SQLComparisonOperator.GreaterThanOrEqualTo, periodFrom);
				query.AddToFilter(CusReconEntrySchema.CRE_EntryDate, SQLComparisonOperator.LessThanOrEqualTo, periodTo);
				query.AddToFilter(CusReconEntrySchema.CRE_GB_Branch, branchPk);
			}
			return query;
		}

		public new CusReconEntry this[int index] => (CusReconEntry)base[index];

		public new CusReconEntry AddNew() => (CusReconEntry)base.AddNew();

		public int MaximumAvailableToAdd => Math.Max(0, ((ISupportMaxCountValidation)this).MaxCountValidator.MaxCount - Count);
	}
}
