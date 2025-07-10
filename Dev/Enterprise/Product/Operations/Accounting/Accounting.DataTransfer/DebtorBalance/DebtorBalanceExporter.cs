using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer.DebtorBalanceExport
{
	public class DebtorBalanceExporter : IProcessor
	{
		public DebtorBalanceExporter(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		readonly BusinessObjectFactory Factory;

		public void Process(INotifications notifications, CancellationToken token
#if DEBUG
			= new CancellationToken()
#endif
		)
		{
			ZQuery companyQuery = new ZQuery(GlbCompanySchema.GC_OH_OrgProxy, SQLComparisonOperator.NotEqual, null);
			companyQuery.AddToFilter(GlbCompanySchema.GC_IsActive, true);
			GlbCompany[] companies = Factory.Load<GlbCompany>(companyQuery);

			foreach (GlbCompany currentCompany in companies)
			{
				token.ThrowIfCancellationRequested();
				ZQuery branchQuery = new ZQuery(GlbBranchSchema.GB_GC, currentCompany.PK);
				branchQuery.AddToFilter(GlbBranchSchema.GB_IsActive, true);
				GlbBranch branch = Factory.LoadTop1<GlbBranch>(branchQuery);

				if (branch != null)
				{
					using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUser, branch.PK.ToGuid(), Env.CurrentDepartment != null ? Env.CurrentDepartment.PK : Guid.Empty)))
					{
						ZString exportDir = SystemDataRegistry.Instance.DebtorOutstandingBalancesExportDirectory.Value;
						if (exportDir != "" && Directory.Exists(exportDir))
						{
							notifications.Notify(new InfoNotification(Res.GetString("505992e0-3e53-47f1-9358-c1323d308a54", "Exporting Debtor's Outstanding Balances of Company {0}", GlbCompany.CurrentCompany.GC_Code)));
							ExportDataPerCompany(notifications);
						}
						else if (exportDir != "" && !Directory.Exists(exportDir))
						{
							notifications.Notify(new ErrorNotification(ErrorType.Error, GetRegistryNotSet(GlbCompany.CurrentCompany.GC_Code)));
						}
					}
				}
			}
		}

		void ExportDataPerCompany(INotifications notifications)
		{
			Dictionary<ZGuid, DebtorBalanceRecordForExport> debtorBalances = GetOutstandingBalancesRecords();
			GetWIPRecords(debtorBalances);

			ZString exportResult = ZString.Empty;

			if (debtorBalances.Count > 0)
			{
				ZString tempFileName = Env.GetTempFileName();

				DebtorBalanceRecordForExport[] debtorBalArray = new DebtorBalanceRecordForExport[debtorBalances.Count];
				debtorBalances.Values.CopyTo(debtorBalArray, 0);

				try
				{
					ExportDataToXml(notifications, tempFileName, debtorBalArray);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ZString errorMesg = ZString.Format(GetCompanyLevelErrorMesg(GlbCompany.CurrentCompany.GC_Code, ex.Message + "\r\n\r\n" + ex.StackTrace));
					notifications.Notify(new ErrorNotification(ErrorType.Error, errorMesg));
				}
				finally
				{
					DeleteFileIfExists(tempFileName);
				}
			}
			else
			{
				notifications.Notify(new InfoNotification(Res.GetString("d615627a-e988-4c20-ba52-a7e2f1387b28", "No Debtor's Balance was Exported")));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "temp checkin for testing")]
#if DEBUG
		protected virtual
#endif
 void ExportDataToXml(INotifications notifications, ZString tempFileName, DebtorBalanceRecordForExport[] debtorBalArray)
		{
			NotificationBuffer inner = new NotificationBuffer(notifications);
			notifications.Notify(new InfoNotification("Opening file " + tempFileName.Replace("\\", "/")));
			using (Stream stream = File.Open(tempFileName, FileMode.Create))
			{
				XmlValueObjectSerializer serialiser = new XmlValueObjectSerializer(DataAdapter.ValueObjectType);
				serialiser.ExportXmlData(stream, DataAdapter, debtorBalArray, new ValueObjectExportContext(inner));
			}
			notifications.Notify(new InfoNotification("Processed file " + tempFileName.Replace("\\", "/")));
			if (!inner.HasErrors)
			{
				MoveOrOverwriteFile(tempFileName, GetOutputFileName(), notifications);

				notifications.Notify(new InfoNotification(GetNoOfDebtorBalExportedMesg(debtorBalArray.Length)));
			}
			else
			{
				ZString errorMesg = GetCompanyLevelErrorMesg(GlbCompany.CurrentCompany.GC_Code, GetErrorMessage(inner));
				notifications.Notify(new ErrorNotification(ErrorType.Error, errorMesg));
			}
		}

		void DeleteFileIfExists(ZString tempFileName)
		{
			try
			{
				if (File.Exists(tempFileName))
				{
					File.Delete(tempFileName);
				}
			}
			catch (UnauthorizedAccessException) { }
			catch (IOException) { }
		}

		ZString GetOutputFileName()
		{
			return Path.Combine(SystemDataRegistry.Instance.DebtorOutstandingBalancesExportDirectory.Value, "DebtorBalances_" + ZDateTime.UtcNow.ToString("yyyyMMddHHmmss") + ".xml");
		}

		ZString GetErrorMessage(NotificationBuffer notifications)
		{
			ZStringBuilder result = new ZStringBuilder();

			foreach (INotification @event in notifications.Events)
			{
				if (@event is ErrorNotification)
				{
					result.Append(@event.Message.Replace("Error:", "").Trim());
				}
			}
			return result.ToStringWithDelimiterBetweenAppends(";");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "temp checkin for testing")]
		void MoveOrOverwriteFile(ZString source, ZString target, INotifications notifications)
		{
			try
			{
				notifications.Notify(new InfoNotification("Copying file from " + source.Replace("\\", "/") + " to " + target.Replace("\\", "/")));
				File.Copy(source, target, true);
				File.Delete(source);
			}
			catch (UnauthorizedAccessException) { }
			catch (IOException) { }
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				notifications.Notify(new ErrorNotification(ErrorType.Error, "Error copying file from " + source.Replace("\\", "/") + " to " + target.Replace("\\", "/") + "\r\n\r\nError: " + ex.Message + "\r\n\r\n" + ex.StackTrace));
				throw;
			}
		}

		#region SQL
		void GetWIPRecords(Dictionary<ZGuid, DebtorBalanceRecordForExport> debtors)
		{
			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(Factory);
			ZSqlParameterCollection parameters = new ZSqlParameterCollection();
			parameters.Add("@Company", GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranchSchema.GB_GC);
			parameters.Add("@LineType", ZArchitecture.Core.TransactionLineTypes.WIP, AccTransactionLinesSchema.AL_LineType);

			collection.Load(DebtorBalanceExporter.WIPSSQLScript, parameters);

			foreach (DynamicBusinessObject current in collection)
			{
				ZGuid debtorPK = (ZGuid)current["DebtorPK"];

				OrgHeader debtorOrg = Factory.Load<OrgHeader>(debtorPK);
				if (debtorOrg != null)
				{
					DebtorBalanceRecordForExport balances;
					if (!debtors.TryGetValue(debtorPK, out balances))
					{
						balances = new DebtorBalanceRecordForExport(debtorOrg, Factory);
						debtors.Add(debtorPK, balances);
					}

					debtors[debtorPK].WIPsAmount = ZArchitecture.Core.Utilities.Round((ZDecimal)current["WIPAmount"], GlbCompany.CurrentCompany.LocalCurrency.Decimals);
				}
			}
		}

		Dictionary<ZGuid, DebtorBalanceRecordForExport> GetOutstandingBalancesRecords()
		{
			var result = new Dictionary<ZGuid, DebtorBalanceRecordForExport>();

			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(Factory);
			ZSqlParameterCollection parameters = new ZSqlParameterCollection();
			AccountingPeriodCalculator calculator = new AccountingPeriodCalculator(Factory);

			parameters.Add("@Company", GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranchSchema.GB_GC);
			parameters.Add("@Ledger", ZArchitecture.Core.LedgerTypes.AccountsReceivable, AccTransactionHeaderSchema.AH_Ledger);
			parameters.Add("@Period", calculator.GetPeriodFromDate(ZDateTime.Now, GlbCompany.CurrentCompany.PK), AccPeriodManagementSchema.AM_Period);
			collection.Load(DebtorBalanceExporter.BalanceSQLScript, parameters);

			foreach (DynamicBusinessObject current in collection)
			{
				ZGuid debtorPK = (ZGuid)current["DebtorPK"];
				OrgHeader debtor = Factory.Load<OrgHeader>(debtorPK);
				if (debtor != null)
				{
					DebtorBalanceRecordForExport balances;
					if (!result.TryGetValue(debtorPK, out balances))
					{
						balances = new DebtorBalanceRecordForExport(debtor, Factory);
						result.Add(debtorPK, balances);
					}

					result[debtorPK].OutstandingBalanceAmount = ZArchitecture.Core.Utilities.Round((ZDecimal)current["BalanceInLocal"], GlbCompany.CurrentCompany.LocalCurrency.Decimals);
				}
			}

			return result;
		}

		#region SQL Script

		public const string BalanceSQLScript = @"SELECT
																							AH_OH as DebtorPK,
																							SUM(AH_BalanceInLocal) AS BalanceInLocal
																							FROM
																							csfn_TransactionsBalances(@Period, @Company, @Ledger)
																							Group by AH_OH
																							Having SUM(AH_BalanceInLocal) != 0";

		public const string WIPSSQLScript = @"SELECT 
																					AL_OH as DebtorPK,	
																					SUM(CASE WHEN AL_LineType = @LineType THEN -AL_LineAmount ELSE 0 END) as WIPAmount
																					FROM dbo.AccTransactionLines   
																					INNER JOIN dbo.GlbBranch ON AL_GB = GB_PK
																					WHERE AL_ReverseDate IS NULL
																					AND GB_GC = @Company
																					Group by AL_OH 
																					HAVING SUM(CASE WHEN AL_LineType = @LineType THEN -AL_LineAmount ELSE 0 END) != 0
";

		#endregion

		#endregion

		DebtorBalanceValueObjectDataAdapter DataAdapter
		{
			get { return dataAdapter ?? (dataAdapter = new DebtorBalanceValueObjectDataAdapter()); }
		}
		DebtorBalanceValueObjectDataAdapter dataAdapter;

		internal static string GetRegistryNotSet(ZString companyCode)
		{
			return Res.GetString("139d39c3-3add-4492-a0e0-f827d9477728", "Export  Directory of Company {0} doesn't exist. Please verify the value in Admin->Registry->System->Export Directories->Debtor's Outstanding Balances", companyCode);
		}

		internal static string GetNoOfDebtorBalExportedMesg(int numberOfOrgs)
		{
			return Res.GetString("4efa1386-e471-45cf-88b1-b26715692499", "Balances of {0} Organization(s) were Exported", numberOfOrgs);
		}

		internal static string GetCompanyLevelErrorMesg(ZString companyCode, string errorMessage)
		{
			return Res.GetString("478e327e-ddc5-449d-9de1-2463e33ab092", "Errors Occurred when Exporting Debtor 's Balances of Company {0} - {1}", companyCode, errorMessage);
		}
	}
}
