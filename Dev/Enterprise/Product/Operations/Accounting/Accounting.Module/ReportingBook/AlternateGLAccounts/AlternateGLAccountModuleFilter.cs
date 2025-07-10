using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class AlternateGLAccountModuleFilter : ModuleGuidFilter
	{
		public AlternateGLAccountModuleFilter(ZString description, ModuleIdentifier id, SchemaGuidColumn filterColumn,
			IBusinessObjectCollection list)
			: base(description, id, filterColumn, list)
		{
		}

		protected override FilterCategory DefaultCategory => FilterCategories.NumbersAndReferences;

		protected override ZQuery GetQuery()
		{
			if (ComparisonOperator != ComparisonConstants.Exact || IsEmpty)
			{
				return base.GetQuery();
			}

			var query = new ZDBOnlyQuery(typeof(AccAlternateGLAccount));
			var subQuery = new ZDBOnlySubQuery(typeof(AccAlternateGLAccount), AccAlternateGLAccountSchema.PK);

			switch (FilterColumn.Name)
			{
				case nameof(AccAlternateGLAccountSchema.AGA_AGA_PercentNum):
					subQuery.AddToFilter(AccAlternateGLAccountSchema.AGA_AccountType, new List<string>() { Core.Constants.AccountType.Consolidation, Core.Constants.AccountType.Total });
					break;
				case nameof(AccAlternateGLAccountSchema.AGA_AGA_ConsolidationNum):
					subQuery.AddToFilter(AccAlternateGLAccountSchema.AGA_AccountType, Core.Constants.AccountType.Consolidation);
					break;
				case nameof(AccAlternateGLAccountSchema.AGA_AGA_AlternateNum):
					subQuery.AddToFilter(AccAlternateGLAccountSchema.AGA_AccountType, Core.Constants.AccountType.Alternate);
					break;
				case nameof(AccAlternateGLAccountSchema.AGA_AGA_HeaderDependsOnTotal):
					subQuery.AddToFilter(AccAlternateGLAccountSchema.AGA_AccountType, Core.Constants.AccountType.Total);
					break;
			}

			if (!Property.IsEmpty)
			{
				subQuery.AddToFilter(AccAlternateGLAccountSchema.PK, Property);
			}

			query.AddSubQuery(FilterColumn, AccAlternateGLAccountSchema.PK, subQuery, JoinCondition.And);

			return query;
		}
	}
}
