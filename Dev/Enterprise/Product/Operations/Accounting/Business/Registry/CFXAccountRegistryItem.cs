using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.Accounting.Business.Res;

namespace Enterprise.Accounting.Registry.Business
{
	public class CFXAccountRegistryItem : GuidRegistryItem
	{
		public CFXAccountRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, Guid defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new CFXAccountDataType(), new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.PandLOrBSH), storage, options, defaultValue))
		{
		}
	}

	public class CFXAccountDataType : GuidRegistryDataType
	{
		protected override void ValidateCore(IRegistryItem registryItem, Guid proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			if (proposedValue == Guid.Empty)
			{
				throw new RegistryValidationException(Res.GetString("cff5e37b-e321-4be3-91be-e85f73c8398a", "Please enter a value."));
			}

			if ((Guid)registryItem.GetValueWithoutFallback(companyPK, branchPK, departmentPK) != proposedValue && IsCFXTransactionsExist(companyPK, departmentPK))
			{
				throw new RegistryValidationException(Res.GetString("8e2f05e9-b18e-42f4-80af-7382fb823d61", "You cannot change this setting because there are CFX transactions posted for the current fallback level."));
			}

			var factory = new BusinessObjectFactory();
			var glHeader = factory.Load<AccGLHeader>(proposedValue);
			if (glHeader != null)
			{
				if (glHeader.AlternateGLAccountDissections.Any())
				{
					throw new RegistryValidationException(Res.GetString("35503BA4-A4C9-460E-90C6-CA2771CCF55A", "GL Accounts with Dissections cannot be selected"));
				}

				var query = new ZDBOnlyQuery(typeof(AccAlternateGLAccountAttribute));
				var subQuery = new ZDBOnlySubQuery(typeof(AccAlternateGLAccountAttribute), AccAlternateGLAccountAttributeSchema.AAA_AGA_AlternateGLAccount, AccAlternateGLAccountAttributeSchema.AAA_AGA_AlternateGLAccount);
				subQuery.AddToFilter(AccAlternateGLAccountAttributeSchema.AAA_AG_GLHeader, glHeader.PK);
				query.AddSubQuery(subQuery, JoinCondition.And);
				query.AddToFilter(AccAlternateGLAccountAttributeSchema.AAA_AG_GLHeader, SQLComparisonOperator.NotEqual, glHeader.PK);
				var attributes = factory.Load<AccAlternateGLAccountAttribute>(query);
				if (attributes.Length > 0)
				{
					throw new RegistryValidationException(Res.GetString("6C6F0A26-991B-41F3-968E-B8AE8B2E94AC", "You cannot select this GL Account Number '{0}' as it is mapped to an Alternate Account linked to multiple Parent Accounts.", glHeader.AG_AccountNum));
				}
			}

			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
		}

		protected override bool IsValidatedOnSetEvenIfEqualDefaultValueCore
		{
			get { return true; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL Strings")]
		public bool IsCFXTransactionsExist(Guid companyPK, Guid departmentPK)
		{
			string additionalWhere = string.Empty;

			BusinessObjectFactory factory = new BusinessObjectFactory();
			DynamicBusinessObjectCollection headers = new DynamicBusinessObjectCollection(factory);
			ZSqlParameterCollection parameters = new ZSqlParameterCollection();
			parameters.Add(ZSqlParameter.New("@Ledger", LedgerTypes.JobCosting, AccTransactionHeaderSchema.AH_Ledger));
			parameters.Add(ZSqlParameter.New("@TransactionType", TransactionTypes.Journal, AccTransactionHeaderSchema.AH_TransactionType));
			parameters.Add(ZSqlParameter.New("@PostToGL", "Y", AccTransactionHeaderSchema.AH_PostToGL));

			if (companyPK != Guid.Empty)
			{
				parameters.Add(ZSqlParameter.New("@Company", companyPK, AccTransactionHeaderSchema.AH_GC));
				additionalWhere += @" AND " + AccTransactionHeaderSchema.Constants.AH_GC + " = @Company";
			}

			if (departmentPK != Guid.Empty)
			{
				parameters.Add(ZSqlParameter.New("@Department", departmentPK, AccTransactionLinesSchema.AL_GE));
				additionalWhere += @" AND " + AccTransactionLinesSchema.Constants.AL_GE + " = @Department";
			}

			string sql = @"
SELECT 
	COUNT(*) as Count
FROM 
	" + AccTransactionHeaderSchema.Constants.SqlSchemaName + "." + AccTransactionHeaderSchema.Constants.TableName + @" JOIN  " +
	  AccTransactionLinesSchema.Constants.SqlSchemaName + "." + AccTransactionLinesSchema.Constants.TableName + " ON " + AccTransactionHeaderSchema.Constants.PK + " = " + AccTransactionLinesSchema.Constants.AL_AH +
@"
WHERE 
	" + AccTransactionHeaderSchema.Constants.AH_Ledger + @" = @Ledger 
	AND " + AccTransactionHeaderSchema.Constants.AH_TransactionType + @" = @TransactionType 
	AND " + AccTransactionHeaderSchema.Constants.AH_PostToGL + " = @PostToGL" + additionalWhere;

			headers.Load(sql, parameters);

			return ((ZInt)headers[0]["Count"] > 0);
		}
	}
}
