using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.GenericCharge
{
	[ModuleID(ModuleId.GenericCharge)]
	public class GenericChargeCollection : BusinessObjectCollection<GenericCharge>
	{
		public GenericChargeCollection(BusinessObjectFactory factory)
			: this(factory, new ZQuery())
		{
		}

		public GenericChargeCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public GenericChargeCollection(BusinessObjectFactory factory, ZQuery filter, Action<AccGLHeaderCollection, List<AccGLHeader>> showGLAccountsForImportAction) : base(factory, filter)
		{
			ShowGLAccountsForImportAction = showGLAccountsForImportAction;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			result.AddToFilter(JoinCondition.And, ViewGenericChargeSchema.VC_IsActive, SQLComparisonOperator.Equal, true);

			ZQuery companyFilter = new ZQuery(ViewGenericChargeSchema.VC_GC, SQLComparisonOperator.Equal, null);
			companyFilter.AddToFilter(JoinCondition.Or, ViewGenericChargeSchema.VC_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);
			result.AddToFilter(companyFilter, JoinCondition.And);

			return result;
		}

		#region FindBoxListProvider

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return new AccGLHeaderFindBoxListProvider(this, showGLAccountsForImportAction: ShowGLAccountsForImportAction); }
		}

		public Action<AccGLHeaderCollection, List<AccGLHeader>> ShowGLAccountsForImportAction;

		#endregion
	}
}
