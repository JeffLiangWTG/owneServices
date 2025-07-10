using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class CusBondDetailCollection : MasterFiles.Business.CusBondDetailCollection
	{
		public CusBondDetailCollection(OrgHeader orgHeader)
			: base(orgHeader)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			query.AddToFilter(CusBondDetailSchema.PW_ApplicationCode, SQLComparisonOperator.Equal, ApplicationCodeList.Codes.CACustoms);
			return query;
		}

		public new CusBondDetail this[int index]
		{
			get { return (CusBondDetail)base[index]; }
		}

		public new CusBondDetail AddNew()
		{
			return (CusBondDetail)base.AddNew();
		}

		static readonly Dictionary<string, int> BondTypePriorities = new Dictionary<string, int>
		{
			{ BondTypeList.Codes.NotOnPortal, 4 },
			{ BondTypeList.Codes.ContinuousBond, 3 },
			{ BondTypeList.Codes.SingleTransactionBond, 2 },
			{ BondTypeList.Codes.OnPortal, 1 }
		};

		public CusBondDetail GetActiveBondDetailData(ZString bondType, ZDateTime effectiveDate)
		{
			return bondType.IsEmpty ?
				this.Cast<CusBondDetail>().Where(x => x.IsBondActive(effectiveDate))
					.OrderByDescending(o => BondTypePriorities.TryGetValue(o.PW_BondType, out var priority) ? priority : 0)
					.ThenByDescending(x => x.PW_BondEffectiveDate)
					.FirstOrDefault()
				: this.Cast<CusBondDetail>().Where(x => x.IsBondActive(effectiveDate) && x.PW_BondType == bondType)
					.OrderBy(x => x.PW_BondEffectiveDate)
					.LastOrDefault();
		}

		public BondDetailsStatus GetBondDetailsStatus(ZString bondType, ZDateTime effectiveDate)
		{
			var bondDetails = this.Cast<CusBondDetail>();
			if (!bondType.IsEmpty)
			{
				bondDetails = bondDetails.Where(x => x.PW_BondType == bondType);
			}
			if (bondDetails.Any())
			{
				if (bondDetails.All(x => !x.IsBondActive(effectiveDate)))
				{
					return BondDetailsStatus.AllExpired;
				}
				return BondDetailsStatus.BondExist;
			}
			return BondDetailsStatus.NoBond;
		}

		public enum BondDetailsStatus
		{
			AllExpired,
			BondExist,
			NoBond
		}

		protected override BusinessObject AddNewCore(System.Type bizOType)
		{
			return base.AddNewCore(typeof(CusBondDetail));
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((CusBondDetail)child).PW_ApplicationCode = ApplicationCodeList.Codes.CACustoms;
		}
	}
}
