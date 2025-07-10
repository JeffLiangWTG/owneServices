using System;
using System.Data;
using System.IO;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.DataTransfer.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer.GLJournals
{
	public class GLTransactionExporter : ISupportHighWaterMark
	{
		public GLTransactionExporter(GLTransactionBusinessObject bizObj, NotificationBuffer notification)
		{
			this.BizObj = bizObj;
			this.Notification = notification;
		}

		public GLTransactionExporter(GLTransactionBusinessObject bizObj, NotificationBuffer notification, bool isAutomaticExport)
			: this(bizObj, notification)
		{
			IsAutomaticExport = isAutomaticExport;
		}

		public bool ExportData()
		{
			bool isExportSomething;
			var batchCreateDate = ZDateTime.UtcNow;

			try
			{
				if (!IsAutomaticExport)
				{
					if (UsingHighWaterMark)
					{
						Notification.Notify(new InfoNotification(Res.GetString("93360D4A-638F-4507-8C89-087DB7FA81C5", @"To aid performance of the export, the system will only search for un-batched transactions that were created or edited since {0}.
If you need to export transactions from before this date, please use the date filtering and this will search for all un-batched transactions.{1}", HighWaterMarkRegistry.Value, System.Environment.NewLine)));
					}
					string message = Res.GetString("5a7de9c6-f9ce-414d-8824-9919d250af47", "Export in progress - please be patient, this may take some time.{0}", System.Environment.NewLine);
					Notification.Notify(new InfoNotification(message));
				}
				isExportSomething = ExportDataCore();
				if (CanSaveHighWaterMark)
				{
					using (HighWaterMarkRegistry.DataType.SuspendValidation())
					{
						HighWaterMarkRegistry.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, batchCreateDate.ToDateTime().Subtract(HighWaterMarkBuffer));
					}
				}
			}
			catch (System.Data.Common.DbException e)
			{
				isExportSomething = false;
				Globals.Message.ShowDeveloperException(e);
				Notification.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("528e22c9-9db3-42a3-8b21-03ffb7fd533a", "A problem happened while getting data from the database.") + e.Message));
			}
			catch (UnauthorizedAccessException e)
			{
				isExportSomething = false;
				NotifyExportDataFileException(e);
			}
			catch (IOException e)
			{
				isExportSomething = false;
				NotifyExportDataFileException(e);
			}
			finally
			{
				AccountingUtils.DeleteFileSafe(TempFile);
			}
			return isExportSomething;
		}

		#region HighWaterMark

		#region IHighWaterMark

		public virtual bool IsHighWaterMarkEnabled
		{
			get { return true; }
		}

		public virtual DateTimeRegistryItem HighWaterMarkRegistry
		{
			get
			{
				return SystemDataRegistry.Instance.GLTransactionsCSVExportHighWaterMark;
			}
		}

		readonly TimeSpan HighWaterMarkBuffer = new TimeSpan(48, 0, 0);

		#endregion

		public bool UsingHighWaterMark
		{
			get
			{
				return HighWaterMarkRegistry.Value != DateTime.MinValue && CanSaveHighWaterMark;
			}
		}

		public bool CanSaveHighWaterMark
		{
			get
			{
				return IsHighWaterMarkEnabled && BizObj.CreateAndExportBatch && !BizObj.HasRestrictiveFilters;
			}
		}

		#endregion

		#region Implementation

		protected GLTransactionBusinessObject BizObj;
		protected NotificationBuffer Notification;
		protected bool IsAutomaticExport;

		#region ExportDataCore

		protected virtual bool ExportDataCore()
		{
			bool isExportSomething = false;

			TempFile = Env.GetTempFileName(ObjectFactory.Get<IFileMapper>().IsRemote ? string.Empty : BizObj.ExportDirectory.ToString());
			using (StreamWriter writer = new StreamWriter(TempFile, false))
			using (new AccountingUtils.CommandTimeoutInitializer(7200000))
			{
				DbCommand glTransactionsDbCommand;
				glTransactionsDbCommand = CreateGLTransactionsBatchDbCommand();
				if (glTransactionsDbCommand != null)
				{
					writer.WriteLine(HeadingLine);
					using (var reader = glTransactionsDbCommand.ExecuteReader())
					{
						while (reader.Read())
						{
							writer.WriteLine(ConvertDataReaderToLine(reader));
							isExportSomething = true;
						}
					}
				}
			}
			AfterDataExport(isExportSomething);
			return isExportSomething;
		}

		protected virtual void AfterDataExport(bool isExportSomething)
		{
			if (isExportSomething)
			{
				var newFileName = FileName;
				using (var sourceStream = File.OpenRead(TempFile))
				using (var targetStream = ObjectFactory.Get<IFileMapper>().OpenWrite(newFileName))
				{
					sourceStream.CopyTo(targetStream);
				}
				Notification.Notify(new InfoNotification(Res.GetString("6fca4968-9b52-4213-bc12-e16aabd98966", "Data exported successfully to the file '{0}'.", newFileName)));
				if (BizObj.CreateAndExportBatch)
				{
					Notification.Notify(new InfoNotification(Res.GetString("572D9E3E-FDCA-40af-9559-F374AA977E45", "New export Batch Number: {0}", BizObj.BatchNumber)));
				}
			}
			else
			{
				Notification.Notify(new InfoNotification(Res.GetString("7cdd18fa-54a0-4b62-9259-98c8d82e95cd", "There are no transactions to export.")));
			}
		}

		void SetupCommonFilterParameters(DbCommand resultDbCmd, object batchNumberToSet, object batchNumberToGet)
		{
			resultDbCmd.AddParameter("@StartPeriod", SqlDbType.Int, BizObj.FromPeriod.IsEmpty ? DBNull.Value : (int)BizObj.FromPeriod);
			resultDbCmd.AddParameter("@EndPeriod", SqlDbType.Int, BizObj.ToPeriod.IsEmpty ? DBNull.Value : (int)BizObj.ToPeriod);

			resultDbCmd.AddParameter("@StartDate", SqlDbType.DateTime, FromDate.IsEmpty ? DBNull.Value : FromDate.ToDateTime());
			resultDbCmd.AddParameter("@EndDate", SqlDbType.DateTime, ToDate.IsEmpty ? DBNull.Value : ToDate.ToDateTime());

			resultDbCmd.AddParameter("@StartGLAccountPK", SqlDbType.UniqueIdentifier, BizObj.StartGLAccountPK.IsEmpty ? DBNull.Value : BizObj.StartGLAccountPK.ToGuid());
			resultDbCmd.AddParameter("@EndGLAccountPK", SqlDbType.UniqueIdentifier, BizObj.EndGLAccountPK.IsEmpty ? DBNull.Value : BizObj.EndGLAccountPK.ToGuid());

			resultDbCmd.AddParameter("@BranchPK", SqlDbType.UniqueIdentifier, BizObj.BranchPK.IsEmpty ? DBNull.Value : BizObj.BranchPK.ToGuid());
			resultDbCmd.AddParameter("@DepartmentPK", SqlDbType.UniqueIdentifier, BizObj.DepartmentPK.IsEmpty ? DBNull.Value : BizObj.DepartmentPK.ToGuid());

			resultDbCmd.AddParameter("@DisplayDescription", SqlDbType.VarChar, 1, BizObj.DescriptionDisplay.Left(1).ToString());
			resultDbCmd.AddParameter("@TransactionCategory", SqlDbType.VarChar, 3, "");

			resultDbCmd.AddParameter("@BatchNumberToGet", SqlDbType.Int, batchNumberToGet);
			resultDbCmd.AddParameter("@BatchNumberToSet", SqlDbType.Int, batchNumberToSet);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
#if DEBUG
		internal
#endif
		protected DbCommand CreateGLTransactionsBatchDbCommand()
		{
			DbCommand resultDbCmd = null;
			ZQuery exportBatchSequenceFilter = new ZQuery(GenExportBatchSequenceSchema.XB_BatchNumber, BizObj.BatchNumber);
			ZQuery queryTypeFilter = new ZQuery(GenExportBatchSequenceSchema.XB_Type, Core.Constants.DataExportBatchSubTypes.Codes.GeneralLedgerPost);
			queryTypeFilter.AddToFilter(JoinCondition.Or, GenExportBatchSequenceSchema.XB_Type, Core.Constants.DataExportBatchSubTypes.Codes.GeneralLedgerReverse);
			queryTypeFilter.AddToFilter(JoinCondition.Or, GenExportBatchSequenceSchema.XB_Type, "");
			exportBatchSequenceFilter.AddToFilter(queryTypeFilter, JoinCondition.And);

			ZDBOnlySubQuery transactionHeaderFilter = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.PK);
			transactionHeaderFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, Env.CurrentCompany.PK);

			ZDBOnlySubQuery transactionLineFilter = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.PK);
			transactionLineFilter.AddToFilter(AccTransactionLinesSchema.AL_GC, GlbCompany.CurrentCompany.PK);

			ZDBOnlyQuery genExportBatchSequenceFilterForHeaders = new ZDBOnlyQuery(typeof(GenExportBatchSequence));
			genExportBatchSequenceFilterForHeaders.AddSubQuery(GenExportBatchSequenceSchema.XB_ParentID, transactionHeaderFilter, JoinCondition.And);
			genExportBatchSequenceFilterForHeaders.AddToFilter(exportBatchSequenceFilter);

			ZDBOnlyQuery genExportBatchSequenceFilterForLines = new ZDBOnlyQuery(typeof(GenExportBatchSequence));
			genExportBatchSequenceFilterForLines.AddSubQuery(GenExportBatchSequenceSchema.XB_ParentID, transactionLineFilter, JoinCondition.And);
			genExportBatchSequenceFilterForLines.AddToFilter(exportBatchSequenceFilter);

			if (BizObj.CreateAndExportBatch && BizObj.BatchNumber == 0)
			{
				Notification.Notify(new InfoNotification(Res.GetString("214046e6-3dde-4d00-bce0-e03eba76fe06", "Warning: Batch Number is 0.")));
			}

			bool isBatchNumberNotInDB = true;

			if (BizObj.CreateAndExportBatch && BizObj.BatchNumber != 0)
			{
				int count = Factory.GetDatabaseCount(typeof(GenExportBatchSequence), genExportBatchSequenceFilterForHeaders);
				isBatchNumberNotInDB = count == 0;

				if (!isBatchNumberNotInDB)
				{
					Notification.Notify(new InfoNotification(Res.GetString("deb28478-7727-4fae-8b07-e77765b0c11f", "Whilst exporting Batch Number {0}, {1} existing headers for that batch were found.  Company: {2}", BizObj.BatchNumber, count, GlbCompany.CurrentCompany.GC_Code)));
				}

				if (isBatchNumberNotInDB)
				{
					count = Factory.GetDatabaseCount(typeof(GenExportBatchSequence), genExportBatchSequenceFilterForLines);
					isBatchNumberNotInDB = count == 0;

					if (!isBatchNumberNotInDB)
					{
						Notification.Notify(new InfoNotification(Res.GetString("d5155ce2-0433-436f-b90d-896b092d0386", "Whilst exporting Batch Number {0}, {1} existing lines for that batch were found.  Company: {2}", BizObj.BatchNumber, count, GlbCompany.CurrentCompany.GC_Code)));
					}
				}
			}

			if
			(
				(BizObj.CreateAndExportBatch && isBatchNumberNotInDB && BizObj.BatchNumber != 0)
				|| (BizObj.ExportExistingBatch && BizObj.BatchNumber != 0)
				|| (!BizObj.ExportExistingBatch && !BizObj.CreateAndExportBatch)
			)
			{
				resultDbCmd = Db.Connection.Command(GLTransactionsSPName); // Hitting database directly for better performance
				resultDbCmd.CommandType = CommandType.StoredProcedure;
				resultDbCmd.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, ToGuid(GlbCompany.CurrentCompany.PK));

				if (BizObj.CreateAndExportBatch || BizObj.ExportExistingBatch)
				{
					if (BizObj.CreateAndExportBatch)
					{
						SetupCommonFilterParameters(resultDbCmd, (int)BizObj.BatchNumber, DBNull.Value);
					}
					else if (BizObj.ExportExistingBatch)
					{
						resultDbCmd.AddParameter("@StartPeriod", SqlDbType.Int, DBNull.Value);
						resultDbCmd.AddParameter("@EndPeriod", SqlDbType.Int, DBNull.Value);

						resultDbCmd.AddParameter("@StartDate", SqlDbType.DateTime, DBNull.Value);
						resultDbCmd.AddParameter("@EndDate", SqlDbType.DateTime, DBNull.Value);

						resultDbCmd.AddParameter("@StartGLAccountPK", SqlDbType.UniqueIdentifier, DBNull.Value);
						resultDbCmd.AddParameter("@EndGLAccountPK", SqlDbType.UniqueIdentifier, DBNull.Value);

						resultDbCmd.AddParameter("@BranchPK", SqlDbType.UniqueIdentifier, DBNull.Value);
						resultDbCmd.AddParameter("@DepartmentPK", SqlDbType.UniqueIdentifier, DBNull.Value);

						resultDbCmd.AddParameter("@DisplayDescription", SqlDbType.Char, 1, BizObj.DescriptionDisplay.Left(1).ToString());
						resultDbCmd.AddParameterBasedOnDbColumn("@TransactionCategory", "", AccGLAggregateSchema.AA_TransactionCategory);
						resultDbCmd.AddParameter("@BatchNumberToGet", SqlDbType.Int, (int)BizObj.BatchNumber);
						resultDbCmd.AddParameter("@BatchNumberToSet", SqlDbType.Int, DBNull.Value);
					}
				}
				else
				{
					SetupCommonFilterParameters(resultDbCmd, DBNull.Value, DBNull.Value);
				}

				resultDbCmd.AddParameter("@IncludeZeroBalance", SqlDbType.Char, "");
				resultDbCmd.AddParameter("@IsExportingBatch", SqlDbType.Char, "Y");

				if (!BizObj.ExportExistingBatch && UsingHighWaterMark)
				{
					resultDbCmd.AddParameter("@HighWaterMark", SqlDbType.SmallDateTime, HighWaterMarkRegistry.Value);
				}
			}
			return resultDbCmd;
		}

		protected virtual string GLTransactionsSPName
		{
			get { return "GLTransactionsSP"; }
		}

		ZString ConvertDataReaderToLine(IDataReader reader)
		{
			return FileFormat.ConvertToLine(ConvertDataReaderToRow(reader));
		}

		protected virtual FlatFileDataRow ConvertDataReaderToRow(IDataReader reader)
		{
			//GL transaction export use local date format for exporting date. We have decided not to change the date format to invariant culture, as it might introduce incidents as things might start to fail.
			//The unit tests written expect current culture to be en-AU, we also focibly set culture to en-AU in the setup of the tests.
			FlatFileDataRow row = new FlatFileDataRow(FieldCapacity);
			row.SetField(0, reader["TransactionType"].ToString());
			row.SetField(1, reader["InvoiceDate"].ToString());
			row.SetField(2, reader["PostDate"].ToString());
			row.SetField(3, reader["DueDate"].ToString());
			row.SetField(4, reader["Branch"].ToString().Replace("\"", "\"\""));
			row.SetField(5, reader["Department"].ToString().Replace("\"", "\"\""));
			row.SetField(6, reader["Ledger"].ToString());
			row.SetField(7, reader["TransactionNum"].ToString().Replace("\"", "\"\""));
			row.SetField(8, reader["TransactionDesc"].ToString().Replace("\"", "\"\""));
			row.SetField(9, reader["GLAccount"].ToString());
			row.SetField(10, reader["GLAccountDesc"].ToString().Replace("\"", "\"\""));
			row.SetField(11, reader["Job"].ToString().Replace("\"", "\"\""));
			row.SetField(12, reader["Account"].ToString().Replace("\"", "\"\""));
			row.SetField(13, reader["ChargeCode"].ToString().Replace("\"", "\"\""));
			row.SetField(14, reader["Period"].ToString());
			row.SetField(15, reader["ReversePeriod"].ToString());
			row.SetField(16, reader["Amount"].ToString());
			row.SetField(17, reader["GSTAmount"].ToString());
			row.SetField(18, reader["Debit"].ToString());
			row.SetField(19, reader["Credit"].ToString());
			row.SetField(20, reader["Balance"].ToString());
			row.SetField(21, reader["IsLine"].ToString());
			row.SetField(22, reader["TRPK"].ToString());
			if (BizObj.ExportExistingBatch || BizObj.CreateAndExportBatch)
			{
				row.SetField(23, "");
				row.SetField(24, "");
				row.SetField(25, "");
				row.SetField(26, "");
			}
			else
			{
				row.SetField(23, reader["OpeningPeriodDate"].ToString());
				row.SetField(24, reader["ClosingPeriodDate"].ToString());
				row.SetField(25, reader["OpeningBalance"].ToString());
				row.SetField(26, reader["ClosingBalance"].ToString());
			}
			row.SetField(27, reader["IsControlTotal"].ToString().Replace("\"", "\"\""));
			if (IncludeNewFields)
			{
				row.SetField(28, reader["BankCode"].ToString().Replace("\"", "\"\""));
				row.SetField(29, reader["TaxID"].ToString().Replace("\"", "\"\""));
				row.SetField(30, reader["TaxIDType"].ToString().Replace("\"", "\"\""));

#pragma warning disable WTG3005 // Do not call ToString() on a string - false positive?
				var taxIdRate = reader["TaxIDRate"] != DBNull.Value ? ((ZDecimal)(decimal)reader["TaxIDRate"]).Normalize().ToString() : reader["TaxIDRate"].ToString();
#pragma warning restore WTG3005

				row.SetField(31, taxIdRate);
				row.SetField(32, reader["TransactionDebtorOrCreditorGroup"].ToString().Replace("\"", "\"\""));
				row.SetField(33, reader["JobLocalClient"].ToString().Replace("\"", "\"\""));
				row.SetField(34, reader["JobLocalClientDebtorGroup"].ToString().Replace("\"", "\"\""));
				row.SetField(35, reader["TransactionDebtorExternalCreditRatingCode"].ToString().Replace("\"", "\"\""));
				row.SetField(36, reader["OriginalTransactionNumberForReversals"].ToString().Replace("\"", "\"\""));
				row.SetField(37, reader["OriginalTransactionNumberConsolidatedInvoiceRef"].ToString().Replace("\"", "\"\""));
				row.SetField(38, reader["TransactionNumberConsolidatedInvoiceRef"].ToString().Replace("\"", "\"\""));
				row.SetField(39, reader["BatchNumberForExport"].ToString());
				row.SetField(40, reader["BatchSequenceNumber"].ToString());
				row.SetField(41, reader["TransactionHeaderBranch"].ToString());
				row.SetField(42, reader["TransactionHeaderDepartment"].ToString());
				row.SetField(43, reader["TransactionLineBranchOrgProxyCode"].ToString().Replace("\"", "\"\""));
				row.SetField(44, reader["TransactionHeaderBranchOrgProxyCode"].ToString().Replace("\"", "\"\""));
				row.SetField(45, reader["WasImportedFromExternalSystem"].ToString());
				row.SetField(46, reader["PaymentReferenceNumber"].ToString().Replace("\"", "\"\""));
				row.SetField(47, reader["Currency"].ToString());
				row.SetField(48, Utilities.Round((decimal)reader["ExRate"], GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces));
				row.SetField(49, reader["OsAmount"].ToString());
				row.SetField(50, reader["OsDebit"].ToString());
				row.SetField(51, reader["OsCredit"].ToString());
				row.SetField(52, reader["MultiSubAccountTypeCode"].ToString().Replace("\"", "\"\"").Trim());
				row.SetField(53, reader["OrganisationSubAccount"].ToString().Replace("\"", "\"\"").Trim());
				row.SetField(54, reader["SalesExpenseGroupsSubAccount"].ToString().Replace("\"", "\"\"").Trim());
				row.SetField(55, reader["StaffAndResourcesSubAccount"].ToString().Replace("\"", "\"\"").Trim());
				row.SetField(56, reader["StaffGroupSubAccount"].ToString().Replace("\"", "\"\"").Trim());
				row.SetField(57, reader["Units"].ToString());
			}
			return row;
		}

		protected string TempFile;

		protected virtual string HeadingLine => IncludeNewFields ? headingLineWithNewFields : headingLineWithoutNewFields;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "CSV Header Should Not Be Localized")]
		const string headingLineWithoutNewFields = "\"TransactionType\",\"InvoiceDate\",\"PostDate\",\"DueDate\",\"Branch\",\"Department\",\"Ledger\",\"TransactionNum\",\"TransactionDesc\",\"GLAccount\",\"GLAccountDesc\",\"Job\",\"Account\",\"ChargeCode\",\"Period\",\"ReversePeriod\",\"Amount\",\"GSTAmount\",\"Debit\",\"Credit\",\"Balance\",\"IsLine\",\"TRPK\",\"OpeningPeriodDate\",\"ClosingPeriodDate\",\"OpeningBalance\",\"ClosingBalance\",\"IsControlTotal\"";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "CSV Header Should Not Be Localized")]
		const string headingLineWithNewFields = "\"TransactionType\",\"InvoiceDate\",\"PostDate\",\"DueDate\",\"Branch\",\"Department\",\"Ledger\",\"TransactionNum\",\"TransactionDesc\",\"GLAccount\",\"GLAccountDesc\",\"Job\",\"Account\",\"ChargeCode\",\"Period\",\"ReversePeriod\",\"Amount\",\"GSTAmount\",\"Debit\",\"Credit\",\"Balance\",\"IsLine\",\"TRPK\",\"OpeningPeriodDate\",\"ClosingPeriodDate\",\"OpeningBalance\",\"ClosingBalance\",\"IsControlTotal\",\"BankCode\",\"TaxID\",\"TaxIDType\",\"TaxIDRate\",\"TransactionDebtorOrCreditorGroup\",\"JobLocalClient\",\"JobLocalClientDebtorGroup\",\"TransactionDebtorExternalCreditRatingCode\",\"OriginalTransactionNumberForReversals\",\"OriginalTransactionNumberConsolidatedInvoiceRef\",\"TransactionNumberConsolidatedInvoiceRef\",\"Unused1\",\"Unused2\",\"TransactionHeaderBranch\",\"TransactionHeaderDepartment\",\"TransactionLineBranchOrgProxyCode\",\"TransactionHeaderBranchOrgProxyCode\",\"WasImportedFromExternalSystem\",\"PaymentReferenceNumber\",\"Currency\",\"ExRate\",\"OsAmount\",\"OsDebit\",\"OsCredit\",\"SubAccounts\",\"OrganisationSubAccount\",\"SalesExpenseGroupsSubAccount\",\"StaffAndResourcesSubAccount\",\"StaffGroupSubAccount\",\"Units\"";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		protected virtual string FileName
		{
			get
			{
				return Path.Combine(BizObj.ExportDirectory, "GLTransactions" + ZDateTime.Now.ToString("yyyyMMddHHmmss") +
					(BizObj.CreateAndExportBatch || BizObj.ExportExistingBatch ? ZString.Format(" {0} batch {1}", Env.CurrentCompany.Code, BizObj.BatchNumber) : ZString.Empty) +
					"." + FileFormat.FileExtensionForExport.ToString().ToLower());
			}
		}

		protected virtual ZDateTime FromDate
		{
			get
			{
				if (BizObj.FromDate.IsEmpty)
				{
					Period startPeriod = Factory.LoadTop1<Period>(new ZQuery(AccPeriodManagementSchema.AM_Period, BizObj.ToPeriod).AddToFilter(AccPeriodManagementSchema.AM_GC_Company, GlbCompany.CurrentCompany.PK));
					return startPeriod != null ? startPeriod.AM_StartDate : ZDateTime.Empty;
				}
				return BizObj.FromDate.Date;
			}
		}

		protected virtual ZDateTime ToDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				if (BizObj.ToDate.IsEmpty)
				{
					Period endPeriod = Factory.LoadTop1<Period>(new ZQuery(AccPeriodManagementSchema.AM_Period, BizObj.ToPeriod).AddToFilter(AccPeriodManagementSchema.AM_GC_Company, GlbCompany.CurrentCompany.PK));
					if (endPeriod != null)
					{
						result = endPeriod.AM_EndDate;
					}
				}
				else
				{
					result = BizObj.ToDate;
				}

				if (!result.IsEmpty)
				{
					result = result.Date.AddDays(1);
				}

				return result;
			}
		}

		#endregion

		#region FileFormat

		CsvFlatFileFormat FileFormat
		{
			get { return fFileFormat ?? (fFileFormat = new CsvFlatFileFormat()); }
		}

		CsvFlatFileFormat fFileFormat;

		#endregion

		#region Factory

		BusinessObjectFactory Factory
		{
			get { return fFactory ?? (fFactory = new BusinessObjectFactory()); }
		}

		BusinessObjectFactory fFactory;

		#endregion

		#region ToGuid

		protected Guid ToGuid(ZGuid pK)
		{
			return pK.IsValid ? pK.ToGuid() : Guid.Empty;
		}

		#endregion

		void NotifyExportDataFileException(Exception e)
		{
			Notification.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("4646646d-dd9c-4aaa-9f3d-fc4dcb614674", "A problem happened while writing the export data into the file. More info: {0}", e.Message)));
		}

		#endregion

		protected virtual int FieldCapacity => IncludeNewFields ? 58 : 28;

		protected virtual bool IncludeNewFields
		{
			get { return true; }
		}
	}
}
