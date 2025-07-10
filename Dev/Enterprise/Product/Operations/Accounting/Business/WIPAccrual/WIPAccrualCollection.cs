using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.WIPAccrual
{
	public class WIPAccrualCollection : BusinessObjectCollection<BaseWIPAccrual>
	{
		public WIPAccrualCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public WIPAccrualCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected static ZQuery CombineFilterWithStaticFilter(ZQuery filter)
		{
			filter.AddToFilter(new ZQuery(WIPFilterStatic(), JoinCondition.Or, AccrualFilterStatic()));
			return filter;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery wipAccrual = WIPFilter();
			wipAccrual.AddToFilter(AccrualFilter(), JoinCondition.Or);
			return new ZQuery(wipAccrual, JoinCondition.And, new ZQuery(AccTransactionLinesSchema.AL_GC, GlbCompany.CurrentCompany.PK));
		}

		protected override BusinessObject AddNewCore()
		{
			throw new NotSupportedException("This collection contains base type.");
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public bool ContainsApportionedAccruals
		{
			get
			{
				bool result = false;

				foreach (BaseWIPAccrual line in this)
				{
					if (line.IsApportioned)
					{
						result = true;
						break;
					}
				}

				return result;
			}
		}

		#region Implementation

		protected BusinessObject Base_AddNewCore()
		{
			return base.AddNewCore();
		}

		protected static ZQuery WIPFilterStatic()
		{
			return new ZQuery(AccTransactionLinesSchema.AL_LineType, SQLComparisonOperator.Equal, ZArchitecture.Core.TransactionLineTypes.WIP);
		}

		protected virtual ZQuery WIPFilter()
		{
			return new ZQuery(AccTransactionLinesSchema.AL_LineType, SQLComparisonOperator.Equal, ZArchitecture.Core.TransactionLineTypes.WIP);
		}

		protected static ZQuery AccrualFilterStatic()
		{
			return new ZQuery(AccTransactionLinesSchema.AL_LineType, SQLComparisonOperator.Equal, ZArchitecture.Core.TransactionLineTypes.Accrual);
		}

		protected virtual ZQuery AccrualFilter()
		{
			return new ZQuery(AccTransactionLinesSchema.AL_LineType, SQLComparisonOperator.Equal, ZArchitecture.Core.TransactionLineTypes.Accrual);
		}

		#endregion
	}
}
