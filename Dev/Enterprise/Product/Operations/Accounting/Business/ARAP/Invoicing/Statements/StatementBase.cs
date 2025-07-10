using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public abstract partial class StatementBase : NonPersistentBusinessObject, IObsoleteValidation
	{
		public BusinessObjectFactory businessObjectFactory;

		public ZBool IncludeTransactionsInActiveBatch
		{
			get { return includeTransactionsInActiveBatch; }
			set { SetNonPersistentPropertyValue(IncludeTransactionsInActiveBatchInfo, ref includeTransactionsInActiveBatch, value); }
		}

		ZBool includeTransactionsInActiveBatch = true;

		public ZPropertyInfo IncludeTransactionsInActiveBatchInfo
		{
			get { return GetZPropertyInfo(nameof(IncludeTransactionsInActiveBatch)); }
		}

		protected StatementBase(BusinessObjectFactory businessObjectFactory) : base(businessObjectFactory)
		{
			this.businessObjectFactory = businessObjectFactory;
		}

		protected void AddFilterForTransactionsInActiveBatch(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, (NoResString)"AND ( {0}.{1} NOT IN ", Statement.Constants.AccTransactionHeader, Statement.Constants.AH_PK));
			stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "( SELECT {0} FROM {1} WHERE {2} = 0 ) ", Statement.Constants.AOL_AH, Statement.Constants.AccCollectionOrderLine, Statement.Constants.AOL_IsCancelled));

			if (IncludeTransactionsInActiveBatch)
			{
				stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, (NoResString)"OR ({0}.{1} IN ", Statement.Constants.AccTransactionHeader, Statement.Constants.AH_PK));
				stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "(SELECT {0} FROM {1} INNER JOIN {2} ON {3}.{4} = {5}.{6} ", new [] { Statement.Constants.AOL_AH, Statement.Constants.AccCollectionOrderLine, Statement.Constants.AccCollectionOrder, Statement.Constants.AccCollectionOrderLine, Statement.Constants.AOL_ACO, Statement.Constants.AccCollectionOrder, Statement.Constants.ACO_PK }));
				stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "INNER JOIN {0} ON {1}.{2} = {3}.{4} ", new Object[] { Statement.Constants.AccCollectionBatch, Statement.Constants.AccCollectionOrder, Statement.Constants.ACO_ACB, Statement.Constants.AccCollectionBatch, Statement.Constants.ACB_PK }));
				stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, (NoResString)"WHERE {0}.{1} NOT IN( SELECT Value FROM @BatchTypes ) AND {2}.{3} = 0)) ", new Object[] { Statement.Constants.AccCollectionBatch, Statement.Constants.ACB_Type, Statement.Constants.AccCollectionOrderLine, Statement.Constants.AOL_IsCancelled }));

				AddParameter(sqlParams, ref BatchTypeParam, delegate { return ZSqlParameter.New("@BatchTypes", GetNotIncludedBatchTypeCodes(), AccCollectionBatchSchema.ACB_Type, true); });
			}
			stringBuilder.Append(") ");
		}

		protected string[] GetNotIncludedBatchTypeCodes()
		{
			var list = AccountingMasterFilesRegistry.Instance.CollectionBatchTypes.GetFallBackValueAtAllLevels(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty);
			var rows = list.GetCodeDescriptionPairList();
			if (rows == null)
			{
				return Array.Empty<string>();
			}
			else
			{
				return rows.Cast<CodeDescriptionPair>()
				.Where(x => !list.GetBoolFromCode(x.Code))
				.Select(x => x.Code.Trim()).ToArray();
			}
		}

		protected ZSqlParameter BatchTypeParam;

		protected delegate ZSqlParameter ZSqlParameterCreator();

		[SuppressMessage("Microsoft.Design", "CA1045: Do not pass types by reference")]
		protected void AddParameter(ZSqlParameterCollection sqlParams, ref ZSqlParameter param, ZSqlParameterCreator createParam)
		{
			if (param == null)
			{
				param = createParam();
			}

			if (!sqlParams.Contains(param))
			{
				sqlParams.Add(param);
			}
		}
	}
}
