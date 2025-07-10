using System;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Aggregator
{
	public class ReAggregator : IReAggregator
	{
		public ReAggregator()
		{
		}

		public const int TimeOutLimit = 7200;

		void IReAggregator.ReAggregate()
		{
			ReAggregateCore();
		}

		protected void ReAggregateCore()
		{
			using (new AccountingUtils.CommandTimeoutInitializer(TimeOutLimit))
			using (var manager = Db.Connection.BeginTransactionWithManager()) // Custom call to DB necessary since aggregator must issue command to DB directly
			{
				ClearAggregate();
				ReQueueCashVAT();
				ReQueueTaxGLMovement();
				UpdateFlags();
				ReAggregateGL();

				manager.CommitTransaction();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected internal void ClearAggregate()
		{
			using (var command = Db.Connection.Command(GetSqlForClearAggregateTable()))  // Custom call to DB necessary since aggregator must issue command to DB directly
			{
				AddCommandParameters(command);
				command.ExecuteNonQuery();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected internal void ReQueueCashVAT()
		{
			using (var command = Db.Connection.Command(GetSqlForReQueueCashVAT()))  // Custom call to DB necessary since aggregator must issue command to DB directly
			{
				AddCommandParameters(command);
				command.ExecuteNonQuery();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected void ReQueueTaxGLMovement()
		{
			using (var command = Db.Connection.Command(GetSqlForReQueueTaxGLMovement()))  // Custom call to DB necessary since aggregator must issue command to DB directly
			{
				AddCommandParameters(command);
				command.ExecuteNonQuery();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected internal void UpdateFlags()
		{
			using (var command = Db.Connection.Command(GetSqlForUpdateHeaderFlags()))  // Custom call to DB necessary since aggregator must issue command to DB directly
			{
				AddCommandParameters(command);
				command.ExecuteNonQuery();
			}

			using (var command = Db.Connection.Command(GetSqlForUpdateLineFlags()))  // Custom call to DB necessary since aggregator must issue command to DB directly
			{
				AddCommandParameters(command);
				command.ExecuteNonQuery();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected internal void ReAggregateGL()
		{
			using (var command = Db.Connection.Command(GetSqlForReAggregateGL()))  // Custom call to DB necessary since aggregator must issue command to DB directly
			{
				AddCommandParameters(command);
				command.ExecuteNonQuery();
			}
		}

		#region SQL

		protected virtual ZString GetSqlForUpdateHeaderFlags()
		{
			return $"UPDATE {AccTransactionHeaderSchema.Constants.SqlSchemaName}.{AccTransactionHeaderSchema.Constants.TableName} SET {AccTransactionHeaderSchema.Constants.AH_PostToGL} = '{Constants.BooleanFalseString}', {AccTransactionHeaderSchema.Constants.AH_SystemLastEditUser} = '~BP', {AccTransactionHeaderSchema.Constants.AH_SystemLastEditTimeUtc} = GETUTCDATE()";
		}

		protected virtual ZString GetSqlForUpdateLineFlags()
		{
			return FormattableString.Invariant(
					$@"UPDATE {AccTransactionLinesSchema.Constants.SqlSchemaName}.{AccTransactionLinesSchema.Constants.TableName} 
					SET {AccTransactionLinesSchema.Constants.AL_PostToGL} = '{Constants.BooleanFalseString}' ,
						{AccTransactionLinesSchema.Constants.AL_ReverseToGL} = '{Constants.BooleanFalseString}',
						{AccTransactionLinesSchema.Constants.AL_SystemLastEditUser} = '~BP',
						{AccTransactionLinesSchema.Constants.AL_SystemLastEditTimeUtc} = GETUTCDATE()
					WHERE {AccTransactionLinesSchema.Constants.AL_AG} IS NOT NULL");
		}

		protected virtual ZString GetSqlForClearAggregateTable()
		{
			return $"DELETE FROM {AccGLAggregateSchema.Constants.SqlSchemaName}.{AccGLAggregateSchema.Constants.TableName} ";
		}

		protected virtual ZString GetSqlForReQueueCashVAT()
		{
			return FormattableString.Invariant(
						$@"INSERT INTO {AccCashBasisVATQueueSchema.Constants.SqlSchemaName}.{AccCashBasisVATQueueSchema.Constants.TableName} ({AccCashBasisVATQueueSchema.Constants.PK}) 
						SELECT {AccCashBasisVATSchema.Constants.PK} 
						FROM {AccCashBasisVATSchema.Constants.SqlSchemaName}.{AccCashBasisVATSchema.Constants.TableName} 
						LEFT JOIN {AccCashBasisVATQueueSchema.Constants.SqlSchemaName}.{AccCashBasisVATQueueSchema.Constants.TableName} ON {AccCashBasisVATQueueSchema.Constants.PK} = {AccCashBasisVATSchema.Constants.PK} 
						WHERE {AccCashBasisVATQueueSchema.Constants.PK} IS NULL ");
		}

		protected virtual ZString GetSqlForReQueueTaxGLMovement()
		{
			return FormattableString.Invariant(
						$@"INSERT INTO {AccTaxGLMovementQueueSchema.Constants.SqlSchemaName}.{AccTaxGLMovementQueueSchema.Constants.TableName} ({AccTaxGLMovementQueueSchema.Constants.PK})
						SELECT {AccTaxGLMovementSchema.Constants.PK}
						FROM {AccTaxGLMovementSchema.Constants.SqlSchemaName}.{AccTaxGLMovementSchema.Constants.TableName}
						LEFT JOIN {AccTaxGLMovementQueueSchema.Constants.SqlSchemaName}.{AccTaxGLMovementQueueSchema.Constants.TableName} ON {AccTaxGLMovementQueueSchema.Constants.PK} = {AccTaxGLMovementSchema.Constants.PK}
						WHERE {AccTaxGLMovementQueueSchema.Constants.PK} IS NULL ");
		}

		ZString GetSqlForReAggregateGL()
		{
			return GetSqlForReAggregateGJLQuery() + GetSqlForReAggregateNJLQuery() + GetSqlForReAggregateRJLQuery() + GetSqlForReAggregateAJLQuery();
		}

		ZString GetSqlForReAggregateGJLQuery()
		{
			return FormattableString.Invariant(
					$@"INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory)
					SELECT NEWID(), SUM(AL_LineAmount), AM_Period, AL_AG, AL_GB, AL_GC, AL_GE, AH_TransactionCategory
					FROM dbo.AccTransactionLines
					JOIN dbo.AccTransactionHeader ON AL_AH = AH_PK
					JOIN dbo.AccPeriodManagement ON AM_GC_Company = AH_GC
					WHERE 
					AH_TransactionType = 'GJL'
					AND AM_Period = dbo.GetPeriodFromDate(AH_PostDate, AH_GC)
					{AddAdditionalFiltersForReAggregateGLQuery()}
					GROUP BY AM_Period, AL_AG, AL_GB, AL_GC, AL_GE, AH_TransactionCategory;");
		}

		ZString GetSqlForReAggregateRJLQuery()
		{
			return FormattableString.Invariant(
					$@"INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory)
					SELECT NEWID(), SUM(AL_LineAmount), AM_Period, AL_AG, AL_GB, AL_GC, AL_GE, AH_TransactionCategory
					FROM dbo.AccTransactionLines
					JOIN dbo.AccTransactionHeader ON AL_AH = AH_PK
					JOIN dbo.AccPeriodManagement ON AM_GC_Company = AH_GC
					WHERE
					AH_TransactionType = 'RJL'
					AND AM_Period = dbo.GetPeriodFromDate(AH_PostDate, AH_GC)
					{AddAdditionalFiltersForReAggregateGLQuery()}
					GROUP BY AM_Period, AL_AG, AL_GB, AL_GC, AL_GE, AH_TransactionCategory;

					INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory)
					SELECT NEWID(), -SUM(AL_LineAmount), AM_Period, AL_AG, AL_GB, AL_GC, AL_GE, AH_TransactionCategory
					FROM dbo.AccTransactionLines
					JOIN dbo.AccTransactionHeader ON AL_AH = AH_PK
					JOIN dbo.AccPeriodManagement ON AM_GC_Company = AH_GC
					WHERE
					AH_TransactionType = 'RJL'
					AND AM_Period = dbo.GetPeriodFromDate(AH_DueDate, AH_GC)
					{AddAdditionalFiltersForReAggregateGLQuery()}
					GROUP BY AM_Period, AL_AG, AL_GB, AL_GC, AL_GE, AH_TransactionCategory;");
		}

		ZString GetSqlForReAggregateAJLQuery()
		{
			return FormattableString.Invariant(
					$@"INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory)
					SELECT NEWID(), SUM(AL_LineAmount), AM_Period, AL_AG, AL_GB, AL_GC, AL_GE, AH_TransactionCategory
					FROM dbo.AccTransactionLines
					JOIN dbo.AccTransactionHeader ON AL_AH = AH_PK
					JOIN dbo.AccPeriodManagement ON AM_GC_Company = AH_GC
					WHERE
					AH_TransactionType = 'AJL'
					AND AM_Period BETWEEN dbo.GetPeriodFromDate(AH_PostDate, AH_GC) AND dbo.GetPeriodFromDate(AH_DueDate, AH_GC)
					{AddAdditionalFiltersForReAggregateGLQuery()}
					GROUP BY AM_Period, AL_AG, AL_GB, AL_GC, AL_GE, AH_TransactionCategory;");
		}

		ZString GetSqlForReAggregateNJLQuery()
		{
			return FormattableString.Invariant(
					$@"INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory)
					SELECT NEWID(), SUM(AL_LineAmount), AM_Period, AL_AG, AL_GB, AL_GC, AL_GE, AH_TransactionCategory
					FROM dbo.AccTransactionLines
					JOIN dbo.AccTransactionHeader ON AL_AH = AH_PK
					JOIN dbo.AccPeriodManagement ON AM_GC_Company = AH_GC
					WHERE 
					AH_TransactionType = 'NJL'
					AND AM_Period = dbo.GetPeriodFromDate(AH_PostDate, AH_GC)
					{AddAdditionalFiltersForReAggregateGLQuery()}
					GROUP BY AM_Period, AL_AG, AL_GB, AL_GC, AL_GE, AH_TransactionCategory;");
		}

		protected virtual ZString AddAdditionalFiltersForReAggregateGLQuery()
		{
			return ZString.Empty;
		}

		protected virtual void AddCommandParameters(DbCommand command)
		{
		}

		#endregion
	}
}
