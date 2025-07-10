using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing
{
	public class ABMCustomsDataImporter
	{
		public ABMCustomsDataImporter(BusinessObjectFactory factory, ZDateTime periodStart, string fileName, INotifications notifications)
		{
			Factory = factory;
			PeriodStart = periodStart;
			FileName = fileName;
			Notifications = new NotificationBuffer(notifications);
		}

		const string SystemCode = BillingConstants.BillingSystem.ABMCustoms;

		readonly BusinessObjectFactory Factory;
		readonly ZDateTime PeriodStart;
		readonly string FileName;
		readonly NotificationBuffer Notifications;

		public List<ABMCustomsData> Transactions
		{
			get { return transactions ?? (transactions = new List<ABMCustomsData>()); }
		}
		List<ABMCustomsData> transactions;

		public void Import()
		{
			if (CheckFilenameContainsPeriodStart())
			{
				LoadWorkSheet();
			}
		}

		public void Save()
		{
			AddBillingTransactions();
			AddChargeableUsage();
		}

		bool CheckFilenameContainsPeriodStart()
		{
			bool result = FileName.Contains(PeriodStartAsText, StringComparison.OrdinalIgnoreCase);
			if (!result)
			{
				Notifications.AddError("Filename doesn't contain selected period '" + PeriodStartAsText + "'. The selected file or date from may be incorrect.");
			}
			return result;
		}

		string PeriodStartAsText
		{
			get { return PeriodStart.ToDateTime().ToString("MMM yyyy", CultureInfo.CurrentCulture); }
		}

		#region Load From Excel

		void LoadWorkSheet()
		{
			UnknownCompanyCodes.Clear();

			using (var excelFile = new ExcelInterface())
			{
				try
				{
					excelFile.LoadExcelFile(FileName);
				}
				catch (ExcelInterfaceException)
				{
					Notifications.AddError("Error while loading excel file. Please contact internal team for support.");
					return;
				}

				if (excelFile.WorkSheets.Count >= 3)
				{
					LoadTransactions(ABMCustomsTransactionTypes.Codes.Customs, excelFile.WorkSheets[0], new string[] { "CompanyCode", "Jurisdiction", "Department", "ProcedureCode", "NumDeclarations" }, false);
					LoadTransactions(ABMCustomsTransactionTypes.Codes.PortCommunity, excelFile.WorkSheets[1], new string[] { "CompanyCode", "JurisdictionCode", "Department", "ProcedureCode", "DocumentRef", "Count" }, true);
					LoadTransactions(ABMCustomsTransactionTypes.Codes.FiscalRep, excelFile.WorkSheets[2], new string[] { "CompanyCode", "JurisdictionCode", "Department", "ProcedureCode", "DocumentRef", "Count" }, true);

					foreach (var companyCode in UnknownCompanyCodes)
					{
						Notifications.AddError("Unknown company code : " + companyCode);
					}
				}
				else
				{
					Notifications.AddError("Incomplete excel file. Please contact internal team for support.");
				}
			}
		}

		void LoadTransactions(string transactionType, ExcelWorkSheet worksheet, string[] columnHeaderText, bool includeReferenceColumn)
		{
			int columnHeaderIndex = 0;
			int companyCodeCol = FindColumnByText(worksheet, columnHeaderText[columnHeaderIndex++], 0);
			int jurisdictionCodeCol = FindColumnByText(worksheet, columnHeaderText[columnHeaderIndex++], companyCodeCol + 1);
			int departmentCol = FindColumnByText(worksheet, columnHeaderText[columnHeaderIndex++], jurisdictionCodeCol + 1);
			int providerCodeCol = FindColumnByText(worksheet, columnHeaderText[columnHeaderIndex++], departmentCol + 1);
			int referenceCol = includeReferenceColumn ? FindColumnByText(worksheet, columnHeaderText[columnHeaderIndex++], providerCodeCol + 1) : providerCodeCol;
			int transactionCol = FindColumnByText(worksheet, columnHeaderText[columnHeaderIndex++], referenceCol + 1);

			int firstDataRow = 1;
			for (int row = firstDataRow; row < worksheet.RowCount; ++row)
			{
				string companyCode = worksheet[row, companyCodeCol].ToString();
				string jurisdictionCode = worksheet[row, jurisdictionCodeCol].ToString();
				string department = worksheet[row, departmentCol].ToString();
				string providerCode = worksheet[row, providerCodeCol].ToString();
				string reference = includeReferenceColumn ? worksheet[row, referenceCol].ToString() : "";
				string transactionCountAsText = worksheet[row, transactionCol].ToString();

				if (!string.IsNullOrWhiteSpace(companyCode) && !companyCode.EndsWith(" Total", StringComparison.OrdinalIgnoreCase) && !companyCode.EndsWith(" Count", StringComparison.OrdinalIgnoreCase))
				{
					AddTransaction(transactionType, companyCode, jurisdictionCode, providerCode, department, reference, transactionCountAsText);
				}
			}
		}

		int FindColumnByText(ExcelWorkSheet worksheet, string searchText, int defaultIndex)
		{
			const int HeaderRow = 0;
			int result = defaultIndex;
			int n = worksheet.ColCountInRow(HeaderRow);
			for (int col = 0; col < worksheet.ColumnCount && n >= 0; ++col)
			{
				string text = worksheet[HeaderRow, col].ToString().Trim();
				if (!string.IsNullOrEmpty(text))
				{
					--n;
					if (string.Compare(text, searchText, StringComparison.OrdinalIgnoreCase) == 0)
					{
						result = col;
						break;
					}
				}
			}

			return result;
		}

		void AddTransaction(string transactionType, string companyCode, string jurisdictionCode, string procedureCode, string department, string reference, string transactionCountAsText)
		{
			var licence = GetLicence(companyCode);
			if (licence != null)
			{
				int transactionCount;
				if (!int.TryParse(transactionCountAsText, out transactionCount))
				{
					transactionCount = 1;
				}

				var transaction = new ABMCustomsData()
				{
					TransactionType = transactionType,
					ClientID = licence.LicenceCode,
					ClientNumber = licence.ClientNumber,
					ABMCompanyCode = companyCode,
					PeriodStart = PeriodStart.ToDateTime(),
					JurisdictionCode = jurisdictionCode,
					ProcedureCode = procedureCode,
					Department = department,
					DocumentReference = reference,
					TransactionCount = transactionCount,
					LicHeader = licence.LicHeader
				};
				Transactions.Add(transaction);
			}
			else
			{
				if (!UnknownCompanyCodes.Contains(companyCode))
				{
					UnknownCompanyCodes.Add(companyCode);
				}
			}
		}

		class LicenceInfo
		{
			public LicenceInfo(LicenceHeader licHeader, string licenceCode, string clientNumber)
			{
				LicHeader = licHeader;
				LicenceCode = licenceCode;
				ClientNumber = clientNumber;
			}

			public readonly LicenceHeader LicHeader;
			public readonly string LicenceCode;
			public readonly string ClientNumber;
		}

		LicenceInfo GetLicence(string companyCode)
		{
			LicenceInfo licence;
			if (!ABMCustomsCompanyCodeMap.TryGetValue(companyCode, out licence))
			{
				licence = null;

				var query = new ZDBOnlyQuery(typeof(EDIOrgHeader));
				var cusCodeSubQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OH);
				cusCodeSubQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, SystemCode);
				cusCodeSubQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, companyCode);
				query.AddSubQuery(cusCodeSubQuery, JoinCondition.And);

				var org = Factory.LoadTop1<EDIOrgHeader>(query);
				if (org != null && org.LicEnterprise != null && org.LicCompany != null)
				{
					var recentDate = ZDateTime.Today.AddDays(-30);
					var productionDatabase = org.LicCompany.ActiveOrAllLicDatabases.Cast<LicenceDatabase>()
						.Where(x => x.IsEnterpriseFamilyDatabase && x.LD_LicenceType == DatabaseTypes.Codes.Production)
						.OrderBy(x => x.LD_IsActive ? 0 : 1) // prefer active
						.ThenBy(x => x.LD_LastHeartbeat > recentDate ? 0 : 1) // prefer recent heartbeat
						.ThenBy(x => x.LD_Billable == DatabaseBillableFlagList.Codes.YesCustomer || x.LD_Billable == DatabaseBillableFlagList.Codes.YesPartner ? 0 : 1) // prefer billable
						.ThenBy(x => x.BillingModel == BillingConstants.BillingModel.STL ? 0 : 1) // prefer STL
						.FirstOrDefault();
					if (productionDatabase != null)
					{
						var licenceHeader = org.LicCompany.GetHeader(productionDatabase);
						var clientCompany = ClientCompany.FindClosestMatch(Factory, licenceHeader);
						var clientCompanyCode = clientCompany != null ? (string)(clientCompany.LCC_Code) : "???";
						var licenceCode = org.LicEnterprise.LE_EnterpriseCode + clientCompanyCode + productionDatabase.LD_ServerCode;
						var clientNumber = productionDatabase.DatabaseId + "." + clientCompanyCode;
						licence = new LicenceInfo(licenceHeader, licenceCode, clientNumber);
					}
				}

				ABMCustomsCompanyCodeMap[companyCode] = licence;
			}
			return licence;
		}

		List<ZString> UnknownCompanyCodes
		{
			get { return unknownCompanyCodes ?? (unknownCompanyCodes = new List<ZString>()); }
		}
		List<ZString> unknownCompanyCodes;

		Dictionary<string, LicenceInfo> ABMCustomsCompanyCodeMap
		{
			get { return abmCustomsCompanyCodeMap ?? (abmCustomsCompanyCodeMap = new Dictionary<string, LicenceInfo>()); }
		}
		Dictionary<string, LicenceInfo> abmCustomsCompanyCodeMap;

		#endregion

		#region Add BillingTransaction

		void AddBillingTransactions()
		{
			string insertQuery =
@"INSERT 
	dbo.BillingTransactionStaging 
	(TX_PriceItemCode, TX_BillableCount, TX_ReportingSource, TX_ServiceOccuredUTC, TX_ClientID, TX_ClientNumber, TX_ClientStaffCode, 
	TX_Reference1, TX_Reference2, TX_Reference3, TX_Reference4, TX_Reference5,
	TX_Version, TX_Category)
VALUES
";
			string insertQueryValueTemplate = "('{0}', {1}, 'ABM', '{2}', '{3}', '{4}', '', '{5}', '{6}', '{7}', '{8}', '{9}', 1, 'ABM'),";

			const int batchCount = 500;
			int count = 0;
			var builder = new ZStringBuilder(insertQuery);

			try
			{
				foreach (var data in Transactions)
				{
					var transactionTime = PeriodStart.AddMilliseconds(count);
					builder.Append(ZString.Format(insertQueryValueTemplate,
						data.TransactionType,       //{0}:TX_PriceItemCode
						data.TransactionCount,      //{1}:TX_BillableCount
						transactionTime.SqlFormat,  //{2}:TX_ServiceOccuredUTC
						data.ClientID,              //{3}:TX_ClientID
						data.ClientNumber,          //{4}:TX_ClientNumber
						data.JurisdictionCode,      //{5}:TX_Reference1
						data.ProcedureCode,         //{6}:TX_Reference2
						data.DocumentReference,     //{7}:TX_Reference3
						data.ABMCompanyCode,        //{8}:TX_Reference4
						data.Department));          //{9}:TX_Reference5
					count++;

					if (count % batchCount == 0 || count == Transactions.Count)
					{
						using (var command = Db.Connection.Command(builder.ToString().TrimEnd(',')))
						{
							command.ExecuteNonQuery();
							builder = new ZStringBuilder(insertQuery);
						}
					}
				}
			}
			catch
			{
				throw;
			}
		}

		#endregion

		#region Add Chargeable Usage

		public bool HasExistingChargeableUsage()
			=> HasExistingChargeableUsage(Factory, PeriodStart);

		static public bool HasExistingChargeableUsage(BusinessObjectFactory factory, ZDateTime periodStart)
		{
			var query = new ZQuery(ClientChargeableUsageSchema.U1_PeriodStart, periodStart);
			query.AddToFilter(ClientChargeableUsageSchema.U1_Code, SystemCode);
			return factory.ExistsInDatabase(ClientChargeableUsage.Schema.TableName, query);
		}

		void AddChargeableUsage()
		{
			var transanctionTypeGroups = Transactions.GroupBy(t => t.TransactionType);
			foreach (var typeGroup in transanctionTypeGroups)
			{
				var usageGroups = typeGroup.GroupBy(t => new { t.LicHeader, t.GroupingKey1, t.GroupingKey2, t.GroupingKey3 });
				foreach (var group in usageGroups)
				{
					var usage = Factory.New<ClientChargeableUsage>();
					usage.U1_UnitCount = group.Sum(t => t.TransactionCount);
					usage.U1_Code = SystemCode;
					usage.U1_SubCode = typeGroup.Key;
					usage.U1_PeriodStart = PeriodStart;
					usage.U1_LC = group.Key.LicHeader.LA_LC;
					usage.U1_LD = group.Key.LicHeader.LA_LD;
					usage.U1_LCC = group.First().ClientCompanyPk;
					usage.U1_Reference1 = group.Key.GroupingKey1;
					usage.U1_Reference2 = group.Key.GroupingKey2;
					usage.U1_Reference3 = group.Key.GroupingKey3;
					usage.U1_UpdateTime = ZDateTime.UtcNow;
				}
			}

			Factory.Save();
		}

		#endregion

		#region Delete

		static public bool HasExistingInvoicedChargeableUsage(BusinessObjectFactory factory, ZDateTime periodStart)
		{
			var query = new ZDBOnlyQuery(typeof(ClientChargeableUsage));
			query.AddToFilter(ClientChargeableUsageSchema.U1_PeriodStart, periodStart);
			query.AddToFilter(ClientChargeableUsageSchema.U1_Code, SystemCode);
			var invoiceQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), ClientChargeableUsageSchema.U1_AH_Invoice);
			invoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_IsCancelled, ZBool.False);
			query.AddSubQuery(invoiceQuery, JoinCondition.And);
			return factory.ExistsInDatabase(ClientChargeableUsage.Schema.TableName, query);
		}

		static public bool HasExistingBillingTransactions(ZDateTime periodStart)
		{
			var sql1 = $"select top 1 cast(1 as bit) from BillingTransactionStaging where TX_Category = '{SystemCode}'";
			var period = periodStart.Year * 100 + periodStart.Month;
			var sql2 = $"select top 1 cast(1 as bit) from BillingViewChargeable where TX_Period = {period} and TX_Category = '{SystemCode}'";
			return ExistsInBillingDatabase(sql1)
				|| ExistsInBillingDatabase(sql2);
		}

		static bool ExistsInBillingDatabase(string sql)
		{
			using (var cmd = Db.Connection.Command(sql))
			{
				var result = cmd.ExecuteScalar();
				return (result is bool) && (bool)result;
			}
		}

		public void DeleteChargeableUsages()
		{
			string sql = $"delete from dbo.ClientChargeableUsage where U1_PeriodStart = @PeriodStart and U1_Code = '{SystemCode}'";
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameterBasedOnDbColumn("@PeriodStart", PeriodStart, ClientChargeableUsageSchema.U1_PeriodStart);
				cmd.ExecuteNonQuery();
			}
		}

		#endregion
	}
}

