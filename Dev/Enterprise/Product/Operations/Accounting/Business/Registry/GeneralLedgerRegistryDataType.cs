using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.Accounting.Business.Res;

namespace Enterprise.Accounting.Registry.Business
{
	public abstract class GeneralLedgerRegistryDataType : GuidRegistryDataType
	{
		protected override void ValidateCore(IRegistryItem registryItem, Guid proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			var currentValueInDB = (Guid)registryItem.GetValueWithoutFallback(companyPK, branchPK, departmentPK);
			var sql = SQLFilterToCheckIfRecordsExist(currentValueInDB);
			if (currentValueInDB != Guid.Empty && currentValueInDB != proposedValue && !string.IsNullOrEmpty(sql) && (int)Db.Connection.ExecuteScalar(sql) > 0)
			{
				var selectQuery = $@"SELECT {AccGLHeaderSchema.Constants.AG_AccountNum}
									FROM {AccGLHeaderSchema.Constants.SqlSchemaName}.{AccGLHeaderSchema.Constants.TableName}
									WHERE {AccGLHeaderSchema.Constants.PK} = '{currentValueInDB}'";

				var accountNumber = (string)Db.Connection.ExecuteScalar(selectQuery);
				throw new RegistryValidationException(
					Res.GetString("b9881b14-8853-4d14-891b-d456fd84e114", "This Registry item cannot be changed because an accounting transaction has already been posted to General Ledger. Please revert the value back to the original value ({0}).", accountNumber));
			}

			var accounting = ObjectFactory.Get<IAccounting>();
			var factory = new BusinessObjectFactory();
			var glHeader = factory.Load<AccGLHeader>(proposedValue);
			if (glHeader != null)
			{
				if (glHeader.AlternateGLAccountDissections.Cast<AccAlternateGLAccountDissection>().Any(x => x.ADC_SeparateNumbering) && accounting.IsNotAllowedForSeparateNumberingRegistry(registryItem))
				{
					throw new RegistryValidationException(
						Res.GetString("1231286B-1842-4228-8752-28BC141C9381", "GL Accounts with Dissections ticked for Separate Numbering cannot be selected"));
				}

				if (glHeader.AlternateGLAccountDissections.Any() && accounting.IsNotAllowedForDissectionAttributesRegistry(registryItem))
				{
					throw new RegistryValidationException(
						Res.GetString("89D18C6B-7C51-4C98-B469-3C6B1D79F4FC", "GL Accounts with Dissections cannot be selected"));
				}

				var query = new ZDBOnlyQuery(typeof(AccAlternateGLAccountAttribute));
				var subQuery = new ZDBOnlySubQuery(typeof(AccAlternateGLAccountAttribute), AccAlternateGLAccountAttributeSchema.AAA_AGA_AlternateGLAccount, AccAlternateGLAccountAttributeSchema.AAA_AGA_AlternateGLAccount);
				subQuery.AddToFilter(AccAlternateGLAccountAttributeSchema.AAA_AG_GLHeader, glHeader.PK);
				query.AddSubQuery(subQuery, JoinCondition.And);
				query.AddToFilter(AccAlternateGLAccountAttributeSchema.AAA_AG_GLHeader, SQLComparisonOperator.NotEqual, glHeader.PK);
				var attributes = factory.Load<AccAlternateGLAccountAttribute>(query);
				if (attributes.Length > 0)
				{
					throw new RegistryValidationException(Res.GetString("37CAF297-CB59-4675-B362-6B32E8BF3EC7", "You cannot select this GL Account Number '{0}' as it is mapped to an Alternate Account linked to multiple Parent Accounts.", glHeader.AG_AccountNum));
				}

				if (CannotContainSPRDissection && glHeader.AlternateGLAccountDissections.Cast<AccAlternateGLAccountDissection>().Any(x => x.ADC_Attribute == AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR))
				{
					throw new RegistryValidationException(Res.GetString("50A5A4B3-A800-432F-9887-261E977F84D7", "{0} cannot have SPR dissection.", registryItem.Caption));
				}
			}
		}

		protected abstract string SQLFilterToCheckIfRecordsExist(Guid currentValueInDB);

		protected virtual bool CannotContainSPRDissection => false;
	}

	#region class JobRevenueJournalControlAccountDataType

	public class JobRevenueJournalControlAccountDataType : GeneralLedgerRegistryDataType
	{
		protected override string SQLFilterToCheckIfRecordsExist(Guid currentValueInDB)
		{
			return $@"IF EXISTS (
						SELECT NULL
						FROM {AccTransactionHeaderSchema.Constants.SqlSchemaName}.{AccTransactionHeaderSchema.Constants.TableName}
						WHERE {AccTransactionHeaderSchema.Constants.AH_Ledger} = 'JC'
						AND {AccTransactionHeaderSchema.Constants.AH_TransactionType} = 'JRJ'
						AND {AccTransactionHeaderSchema.Constants.AH_PostToGL} = 'Y'
					) SELECT 1
					ELSE SELECT 0";
		}
	}

	#endregion

	#region class ARControlAccountDataType

	public class ARControlAccountDataType : GeneralLedgerRegistryDataType
	{
		protected override string SQLFilterToCheckIfRecordsExist(Guid currentValueInDB)
		{
			return $@"IF EXISTS (
						SELECT NULL
						FROM {AccTransactionHeaderSchema.Constants.SqlSchemaName}.{AccTransactionHeaderSchema.Constants.TableName}
						WHERE {AccTransactionHeaderSchema.Constants.AH_Ledger} = 'AR' and {AccTransactionHeaderSchema.Constants.AH_PostToGL} = 'Y'
					) SELECT 1
					ELSE SELECT 0";
		}

		protected override bool CannotContainSPRDissection => true;
	}

	#endregion

	#region class APControlAccountDataType

	public class APControlAccountDataType : GeneralLedgerRegistryDataType
	{
		protected override string SQLFilterToCheckIfRecordsExist(Guid currentValueInDB)
		{
			return $@"IF EXISTS (
						SELECT NULL
						FROM {AccTransactionHeaderSchema.Constants.SqlSchemaName}.{AccTransactionHeaderSchema.Constants.TableName}
						WHERE {AccTransactionHeaderSchema.Constants.AH_Ledger} = 'AP' and {AccTransactionHeaderSchema.Constants.AH_PostToGL} = 'Y'
					) SELECT 1
					ELSE SELECT 0";
		}

		protected override bool CannotContainSPRDissection => true;
	}

	#endregion

	#region class ARSuspenseControlAccountDataType

	public class ARSuspenseControlAccountDataType : GeneralLedgerRegistryDataType
	{
		protected override string SQLFilterToCheckIfRecordsExist(Guid currentValueInDB)
		{
			return $@"IF EXISTS (
						SELECT NULL
						FROM {AccTransactionHeaderSchema.Constants.SqlSchemaName}.{AccTransactionHeaderSchema.Constants.TableName}
						INNER JOIN {AccTransactionLinesSchema.Constants.SqlSchemaName}.{AccTransactionLinesSchema.Constants.TableName}
							ON {AccTransactionLinesSchema.Constants.AL_AH} = {AccTransactionHeaderSchema.Constants.PK}
						WHERE {AccTransactionHeaderSchema.Constants.AH_Ledger} = 'AR'
						AND {AccTransactionHeaderSchema.Constants.AH_TransactionType} IN ('INV', 'CRD', 'ADJ')
						AND {AccTransactionHeaderSchema.Constants.AH_PostToGL} = 'Y'

						UNION ALL

						SELECT NULL
						FROM {AccTransactionHeaderSchema.Constants.SqlSchemaName}.{AccTransactionHeaderSchema.Constants.TableName}
						WHERE {AccTransactionHeaderSchema.Constants.AH_Ledger} = 'JC'
						AND {AccTransactionHeaderSchema.Constants.AH_TransactionType} in ('JRJ', 'JNL')
						AND {AccTransactionHeaderSchema.Constants.AH_PostToGL} = 'Y'
					) SELECT 1
					ELSE SELECT 0";
		}
	}

	#endregion

	#region class APSuspenseControlAccountDataType

	public class APSuspenseControlAccountDataType : GeneralLedgerRegistryDataType
	{
		protected override string SQLFilterToCheckIfRecordsExist(Guid currentValueInDB)
		{
			return $@"IF EXISTS (
						SELECT NULL
						FROM {AccTransactionHeaderSchema.Constants.SqlSchemaName}.{AccTransactionHeaderSchema.Constants.TableName}
						INNER JOIN {AccTransactionLinesSchema.Constants.SqlSchemaName}.{AccTransactionLinesSchema.Constants.TableName}
							ON {AccTransactionLinesSchema.Constants.AL_AH} = {AccTransactionHeaderSchema.Constants.PK}
						WHERE {AccTransactionHeaderSchema.Constants.AH_Ledger} = 'AP'
						AND {AccTransactionHeaderSchema.Constants.AH_TransactionType} IN ('INV', 'CRD', 'ADJ')
						AND {AccTransactionHeaderSchema.Constants.AH_PostToGL} = 'Y'
					) SELECT 1
					ELSE SELECT 0";
		}
	}

	#endregion

	#region class GSTInputControlAccountDataType

	public class GSTInputControlAccountDataType : GeneralLedgerRegistryDataType
	{
		protected override string SQLFilterToCheckIfRecordsExist(Guid currentValueInDB)
		{
			return $@"IF EXISTS (
						SELECT NULL
						FROM {AccTransactionHeaderSchema.Constants.SqlSchemaName}.{AccTransactionHeaderSchema.Constants.TableName}
						INNER JOIN {AccTransactionLinesSchema.Constants.SqlSchemaName}.{AccTransactionLinesSchema.Constants.TableName}
							ON {AccTransactionLinesSchema.Constants.AL_AH} = {AccTransactionHeaderSchema.Constants.PK}
						WHERE {AccTransactionHeaderSchema.Constants.AH_Ledger} = 'AP'
						AND {AccTransactionHeaderSchema.Constants.AH_TransactionType} IN ('INV', 'CRD', 'ADJ')
						AND {AccTransactionHeaderSchema.Constants.AH_PostToGL} = 'Y'
						AND {AccTransactionLinesSchema.Constants.AL_GSTVATBasis} = 'A'
						AND {AccTransactionLinesSchema.Constants.AL_GSTVAT} <> 0

						UNION ALL

						SELECT NULL
						FROM {AccCashBasisVATSchema.Constants.SqlSchemaName}.{AccCashBasisVATSchema.Constants.TableName}
						INNER JOIN {AccTransactionLinesSchema.Constants.SqlSchemaName}.{AccTransactionLinesSchema.Constants.TableName}
							ON {AccCashBasisVATSchema.Constants.YC_AL_TransactionLine} = {AccTransactionLinesSchema.Constants.PK}
						INNER JOIN {AccTransactionHeaderSchema.Constants.SqlSchemaName}.{AccTransactionHeaderSchema.Constants.TableName}
							ON {AccTransactionLinesSchema.Constants.AL_AH} = {AccTransactionHeaderSchema.Constants.PK}
						LEFT JOIN {AccCashBasisVATQueueSchema.Constants.SqlSchemaName}.{AccCashBasisVATQueueSchema.Constants.TableName}
							ON {AccCashBasisVATQueueSchema.Constants.PK} = {AccCashBasisVATSchema.Constants.PK}
						WHERE {AccCashBasisVATQueueSchema.Constants.PK} IS NULL
						AND {AccTransactionHeaderSchema.Constants.AH_Ledger} = 'AP'
						AND {AccTransactionHeaderSchema.Constants.AH_TransactionType} IN ('INV', 'CRD', 'ADJ')
						AND {AccTransactionLinesSchema.Constants.AL_GSTVAT} <> 0

						UNION ALL

						SELECT NULL
						FROM {AccTransactionHeaderSchema.Constants.SqlSchemaName}.{AccTransactionHeaderSchema.Constants.TableName}
						INNER JOIN {AccTransactionLinesSchema.Constants.SqlSchemaName}.{AccTransactionLinesSchema.Constants.TableName}
							ON {AccTransactionLinesSchema.Constants.AL_AH} = {AccTransactionHeaderSchema.Constants.PK}
						WHERE {AccTransactionHeaderSchema.Constants.AH_Ledger} = 'CB'
						AND {AccTransactionHeaderSchema.Constants.AH_TransactionType} = 'DPY'
						AND {AccTransactionHeaderSchema.Constants.AH_PostToGL} = 'Y'
						AND {AccTransactionLinesSchema.Constants.AL_GSTVAT} <> 0
					) SELECT 1
					ELSE SELECT 0";
		}
	}

	public class PendingGSTInputControlAccountDataType : GeneralLedgerRegistryDataType
	{
		protected override string SQLFilterToCheckIfRecordsExist(Guid currentValueInDB)
		{
			return $@"IF EXISTS (
						SELECT NULL
						FROM {AccTransactionHeaderSchema.Constants.SqlSchemaName}.{AccTransactionHeaderSchema.Constants.TableName}
						INNER JOIN {AccTransactionLinesSchema.Constants.SqlSchemaName}.{AccTransactionLinesSchema.Constants.TableName}
							ON {AccTransactionLinesSchema.Constants.AL_AH} = {AccTransactionHeaderSchema.Constants.PK}
						WHERE {AccTransactionHeaderSchema.Constants.AH_Ledger} = 'AP'
						AND {AccTransactionHeaderSchema.Constants.AH_TransactionType} IN ('INV', 'CRD', 'ADJ')
						AND {AccTransactionHeaderSchema.Constants.AH_PostToGL} = 'Y'
						AND {AccTransactionLinesSchema.Constants.AL_GSTVATBasis} = 'C'
						AND {AccTransactionLinesSchema.Constants.AL_GSTVAT} <> 0
					) SELECT 1
					ELSE SELECT 0";
		}
	}

	#endregion

	#region class GSTOutputControlAccountDataType

	public class GSTOutputControlAccountDataType : GeneralLedgerRegistryDataType
	{
		protected override string SQLFilterToCheckIfRecordsExist(Guid currentValueInDB)
		{
			return $@"IF EXISTS (
						SELECT NULL
						FROM {AccTransactionHeaderSchema.Constants.SqlSchemaName}.{AccTransactionHeaderSchema.Constants.TableName}
						INNER JOIN {AccTransactionLinesSchema.Constants.SqlSchemaName}.{AccTransactionLinesSchema.Constants.TableName}
							ON {AccTransactionLinesSchema.Constants.AL_AH} = {AccTransactionHeaderSchema.Constants.PK}
						WHERE {AccTransactionHeaderSchema.Constants.AH_Ledger} = 'AR'
						AND {AccTransactionHeaderSchema.Constants.AH_TransactionType} IN ('INV', 'CRD', 'ADJ')
						AND {AccTransactionHeaderSchema.Constants.AH_PostToGL} = 'Y'
						AND {AccTransactionLinesSchema.Constants.AL_GSTVATBasis} = 'A'
						AND {AccTransactionLinesSchema.Constants.AL_GSTVAT} <> 0

						UNION ALL

						SELECT NULL
						FROM {AccCashBasisVATSchema.Constants.SqlSchemaName}.{AccCashBasisVATSchema.Constants.TableName}
						INNER JOIN {AccTransactionLinesSchema.Constants.SqlSchemaName}.{AccTransactionLinesSchema.Constants.TableName}
							ON {AccCashBasisVATSchema.Constants.YC_AL_TransactionLine} = {AccTransactionLinesSchema.Constants.PK}
						INNER JOIN {AccTransactionHeaderSchema.Constants.SqlSchemaName}.{AccTransactionHeaderSchema.Constants.TableName}
							ON {AccTransactionLinesSchema.Constants.AL_AH} = {AccTransactionHeaderSchema.Constants.PK}
						LEFT JOIN {AccCashBasisVATQueueSchema.Constants.SqlSchemaName}.{AccCashBasisVATQueueSchema.Constants.TableName}
							ON {AccCashBasisVATQueueSchema.Constants.PK} = {AccCashBasisVATSchema.Constants.PK}
						WHERE {AccCashBasisVATQueueSchema.Constants.PK} IS NULL
						AND {AccTransactionHeaderSchema.Constants.AH_Ledger} = 'AR'
						AND {AccTransactionHeaderSchema.Constants.AH_TransactionType} IN ('INV', 'CRD', 'ADJ')
						AND {AccTransactionLinesSchema.Constants.AL_GSTVAT} <> 0

						UNION ALL

						SELECT NULL
						FROM {AccTransactionHeaderSchema.Constants.SqlSchemaName}.{AccTransactionHeaderSchema.Constants.TableName}
						INNER JOIN {AccTransactionLinesSchema.Constants.SqlSchemaName}.{AccTransactionLinesSchema.Constants.TableName}
							ON {AccTransactionLinesSchema.Constants.AL_AH} = {AccTransactionHeaderSchema.Constants.PK}
						WHERE {AccTransactionHeaderSchema.Constants.AH_Ledger} = 'CB'
						AND {AccTransactionHeaderSchema.Constants.AH_TransactionType} = 'DRC'
						AND {AccTransactionHeaderSchema.Constants.AH_PostToGL} = 'Y'
						AND {AccTransactionLinesSchema.Constants.AL_GSTVAT} <> 0
					) SELECT 1
					ELSE SELECT 0";
		}
	}

	public class PendingGSTOutputControlAccountDataType : GeneralLedgerRegistryDataType
	{
		protected override string SQLFilterToCheckIfRecordsExist(Guid currentValueInDB)
		{
			return $@"IF EXISTS (
						SELECT NULL
						FROM {AccTransactionHeaderSchema.Constants.SqlSchemaName}.{AccTransactionHeaderSchema.Constants.TableName}
						INNER JOIN {AccTransactionLinesSchema.Constants.SqlSchemaName}.{AccTransactionLinesSchema.Constants.TableName}
							ON {AccTransactionLinesSchema.Constants.AL_AH} = {AccTransactionHeaderSchema.Constants.PK}
						WHERE {AccTransactionHeaderSchema.Constants.AH_Ledger} = 'AR'
						AND {AccTransactionHeaderSchema.Constants.AH_TransactionType} IN ('INV', 'CRD', 'ADJ')
						AND {AccTransactionHeaderSchema.Constants.AH_PostToGL} = 'Y'
						AND {AccTransactionLinesSchema.Constants.AL_GSTVATBasis} = 'C'
						AND {AccTransactionLinesSchema.Constants.AL_GSTVAT} <> 0
					) SELECT 1
					ELSE SELECT 0";
		}
	}

	#endregion

	#region class WHTInputControlAccountDataType

	public class WHTInputControlAccountDataType : GeneralLedgerRegistryDataType
	{
		protected override string SQLFilterToCheckIfRecordsExist(Guid currentValueInDB)
		{
			return $@"IF EXISTS (
						SELECT NULL
						FROM {AccTransactionHeaderSchema.Constants.SqlSchemaName}.{AccTransactionHeaderSchema.Constants.TableName}
						INNER JOIN {AccTransactionLinesSchema.Constants.SqlSchemaName}.{AccTransactionLinesSchema.Constants.TableName}
							ON {AccTransactionHeaderSchema.Constants.PK} = {AccTransactionLinesSchema.Constants.AL_AH}
						WHERE {AccTransactionLinesSchema.Constants.AL_WithholdingTax} != 0
							AND {AccTransactionHeaderSchema.Constants.AH_PostToGL} = 'Y'
							AND
							(
								{AccTransactionHeaderSchema.Constants.AH_Ledger} = 'AP'
								OR ({AccTransactionHeaderSchema.Constants.AH_Ledger} = 'CB' AND {AccTransactionHeaderSchema.Constants.AH_TransactionType} = 'DPY')
							)
					) SELECT 1
					ELSE SELECT 0";
		}
	}

	#endregion

	#region class WHTOutputControlAccountDataType

	public class WHTOutputControlAccountDataType : GeneralLedgerRegistryDataType
	{
		protected override string SQLFilterToCheckIfRecordsExist(Guid currentValueInDB)
		{
			return $@"IF EXISTS (
						SELECT NULL
						FROM {AccTransactionHeaderSchema.Constants.SqlSchemaName}.{AccTransactionHeaderSchema.Constants.TableName}
						INNER JOIN {AccTransactionLinesSchema.Constants.SqlSchemaName}.{AccTransactionLinesSchema.Constants.TableName}
							ON {AccTransactionHeaderSchema.Constants.PK} = {AccTransactionLinesSchema.Constants.AL_AH}
						WHERE {AccTransactionLinesSchema.Constants.AL_WithholdingTax} != 0
							AND {AccTransactionHeaderSchema.Constants.AH_PostToGL} = 'Y'
							AND
							(
								{AccTransactionHeaderSchema.Constants.AH_Ledger} = 'AR'
								OR ({AccTransactionHeaderSchema.Constants.AH_Ledger} = 'CB' AND {AccTransactionHeaderSchema.Constants.AH_TransactionType} = 'DRC')
							)
					) SELECT 1
					ELSE SELECT 0";
		}
	}

	#endregion

	#region class AccruedRevenueControlAccountDataType

	public class AccruedRevenueControlAccountDataType : GeneralLedgerRegistryDataType
	{
		protected override string SQLFilterToCheckIfRecordsExist(Guid currentValueInDB)
		{
			return $@"IF EXISTS (
						SELECT NULL
						FROM {AccTransactionLinesSchema.Constants.SqlSchemaName}.{AccTransactionLinesSchema.Constants.TableName}
						WHERE {AccTransactionLinesSchema.Constants.AL_LineType} = 'WIP'
						AND {AccTransactionLinesSchema.Constants.AL_PostToGL} = 'Y'
					) SELECT 1
					ELSE SELECT 0";
		}
	}

	#endregion

	#region class AccruedCostControlAccountDataType

	public class AccruedCostControlAccountDataType : GeneralLedgerRegistryDataType
	{
		protected override string SQLFilterToCheckIfRecordsExist(Guid currentValueInDB)
		{
			return $@"IF EXISTS (
						SELECT NULL
						FROM {AccTransactionLinesSchema.Constants.SqlSchemaName}.{AccTransactionLinesSchema.Constants.TableName}
						WHERE {AccTransactionLinesSchema.Constants.AL_LineType} = 'ACR'
						AND {AccTransactionLinesSchema.Constants.AL_PostToGL} = 'Y'
					) SELECT 1
					ELSE SELECT 0";
		}
	}

	#endregion

	#region class WithoutCheckExistAccountDataType

	public class WithoutCheckExistAccountDataType : GeneralLedgerRegistryDataType
	{
		protected override string SQLFilterToCheckIfRecordsExist(Guid currentValueInDB)
		{
			return string.Empty;
		}
	}

	#endregion
}
