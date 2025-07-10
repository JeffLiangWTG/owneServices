using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing
{
	public class ClientMappingBillingSystem : TransactionBillingSystem
	{
		public ClientMappingBillingSystem()
		{
			IncludeSubCodeInSystemUsage = true;
		}

		public override string SystemCode
		{
			get { return BillingConstants.BillingSystem.ClientMapping; }
		}

		protected override bool ShouldCreateSystemBillPerCurrency
		{
			get { return true; }
		}

		protected override SystemBill CreateSystemBill()
		{
			return new ClientMappingBill(Context.Factory);
		}

		#region Create System Usages

		protected override SystemUsage[] CreateSystemUsages(ClientChargeableUsage[] chargeableUsages)
		{
			var result = new List<ClientMappingUsage>();
			foreach (var chargeableUsageByPeriod in chargeableUsages.GroupBy(x => x.U1_PeriodStart))
			{
				foreach (var chargeableUsageByPeriodByDatabase in chargeableUsageByPeriod.GroupBy(x => x.U1_LD))
				{
					foreach (var usagePerInterface in chargeableUsageByPeriodByDatabase.GroupBy(x => x.U1_SubCode))
					{
						IEnumerable<ClientChargeableUsage> usages = usagePerInterface;
						var maxCount = usages.Max(x => x.U1_UnitCount);
						var biggest = usages.First(x => x.U1_UnitCount == maxCount);
						var user = new UsingParty(biggest);
						var systemUsage = new ClientMappingUsage(SystemCode, Context.Factory, user, chargeableUsageByPeriod.Key);
						if (Context.IsPreviewOnly)
						{
							systemUsage.CalculatePriceItem();
							if (!systemUsage.IsHubPriceHeaderCode)
							{
								systemUsage.ResetPriceHeader();
								systemUsage.SetPreviewOnly(Context.PreviewPriceHeader);
							}
						}

						systemUsage.TransactionCount = usages.Sum(x => x.U1_UnitCountAsInt);
						systemUsage.ChargeableUsagePKs.AddRange(usages.Select(x => x.PK));
						systemUsage.SubCode = biggest.U1_SubCode;
						systemUsage.Reference3 = biggest.U1_Reference3;
						result.Add(systemUsage);
					}
				}
			}

			CombineUsagesByInterfaceGroup(result);
			return result.ToArray();
		}

		void CombineUsagesByInterfaceGroup(List<ClientMappingUsage> usageList)
		{
			var usagesGroupedByInterfaceGroup = usageList.Where(x => x.HasPriceItem && !x.PriceItem.L7_ParentCode.IsEmpty).GroupBy(x => new { x.PeriodStart, x.PriceItem.L7_ParentCode });

			var combinedUsageToRemoveList = new List<ClientMappingUsage>(1);
			foreach (var usageGroup in usagesGroupedByInterfaceGroup)
			{
				var firstUsage = usageGroup.First();
				var combinedUsage = new ClientMappingUsage(firstUsage.SystemCode, firstUsage.Factory, firstUsage.User, firstUsage.PeriodStart);
				combinedUsage.SubCode = usageGroup.Key.L7_ParentCode;
				combinedUsage.Reference3 = firstUsage.Reference3;

				foreach (var usage in usageGroup)
				{
					combinedUsage.TransactionCount += usage.TransactionCount;
					combinedUsage.ChargeableUsagePKs.AddRange(usage.ChargeableUsagePKs);
					combinedUsageToRemoveList.Add(usage);
				}

				usageList.Add(combinedUsage);
			}

			foreach (var usageToRemove in combinedUsageToRemoveList)
			{
				usageList.Remove(usageToRemove);
			}
		}

		#endregion

		#region Load Raw Usage

		protected override SystemRawUsage LoadOdplRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
		{
			var rawUsage = new MultiSectionSystemRawUsage(context, SystemCode);
			string currentInterfaceName = "";
			SummarySection summarySection = null;

			while (reader.Read())
			{
				string clientId = (string)reader["TX_ClientID"];
				int billableCount = (int)reader["TX_BillableCount"];
				string priceItemCode = (string)reader["TX_PriceItemCode"];
				string ref2 = (string)reader["TX_Reference2"];
				string ref3 = (string)reader["TX_Reference3"];
				string interfaceName = (string)reader["TX_Reference1"];
				ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];
				string interfacePriceDescription = (string)reader["InterfacePriceDescription"];

				if (string.Compare(interfaceName, currentInterfaceName, StringComparison.OrdinalIgnoreCase) != 0)
				{
					summarySection = new SummarySection(context.Factory);
					summarySection.Header.TopLevelDescription = System.Environment.NewLine + (!string.IsNullOrEmpty(interfacePriceDescription) ? interfacePriceDescription : interfaceName);
					summarySection.Header.Column1 = "Client ID";

					if (priceItemCode == "SCO")
					{
						summarySection.Header.Column2 = "Order No.";
						summarySection.Header.Column3 = "Buyer";
						summarySection.Header.Column4 = "Message Time (UTC)";
					}
					else if (priceItemCode == "SCS")
					{
						summarySection.Header.Column2 = "Shipment ID";
						summarySection.Header.Column3 = "Message Time (UTC)";
					}
					else
					{
						summarySection.Header.Column2 = "Element";
						summarySection.Header.Column3 = "Units";
						summarySection.Header.Column4 = "Filename";
						summarySection.Header.Column5 = "Message Time (UTC)";
					}

					rawUsage.SummarySections.Add(summarySection);
					currentInterfaceName = interfaceName;
				}

				if (summarySection != null)
				{
					SummaryLine line = summarySection.Lines.AddNew();
					line.Column1 = clientId;

					if (priceItemCode == "SCO")
					{
						line.Column2 = ref2;
						line.Column3 = ref3;
						line.Column4 = messageTime.ToLongTimeString();
					}
					else if (priceItemCode == "SCS")
					{
						line.Column2 = ref2;
						line.Column3 = messageTime.ToLongTimeString();
					}
					else
					{
						line.Column2 = ref2;
						line.Column3 = billableCount.ToString(CultureInfo.InvariantCulture);
						line.Column4 = ref3;
						line.Column5 = messageTime.ToLongTimeString();
					}
				}
			}

			return rawUsage;
		}

		protected override StlRawUsage LoadStlRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
		{
			var rawUsage = new MultiSectionStlRawUsage(context, SystemCode);
			string currentInterfaceName = "";
			SummarySection summarySection = null;

			while (reader.Read())
			{
				string companyCode = (string)reader["CompanyCode"];
				int billableCount = (int)reader["TX_BillableCount"];
				string priceItemCode = (string)reader["TX_PriceItemCode"];
				string ref2 = (string)reader["TX_Reference2"];
				string ref3 = (string)reader["TX_Reference3"];
				string interfaceName = (string)reader["TX_Reference1"];
				ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];
				string interfacePriceDescription = (string)reader["InterfacePriceDescription"];

				if (string.Compare(interfaceName, currentInterfaceName, StringComparison.OrdinalIgnoreCase) != 0)
				{
					summarySection = new SummarySection(context.Factory);
					summarySection.Header.TopLevelDescription = System.Environment.NewLine + (!string.IsNullOrEmpty(interfacePriceDescription) ? interfacePriceDescription : interfaceName);
					summarySection.Header.Column1 = "Company Code";
					summarySection.Header.Column9 = "Message Time (UTC)";

					if (priceItemCode == "SCO")
					{
						summarySection.Header.Column2 = "Order No.";
						summarySection.Header.Column3 = "Buyer";
					}
					else if (priceItemCode == "SCS")
					{
						summarySection.Header.Column2 = "Shipment ID";
					}
					else
					{
						summarySection.Header.Column2 = "Element";
						summarySection.Header.Column3 = "Units";
						summarySection.Header.Column4 = "Filename";
					}

					rawUsage.SummarySections.Add(summarySection);
					currentInterfaceName = interfaceName;
				}

				if (summarySection != null)
				{
					SummaryLine line = summarySection.Lines.AddNew();
					line.Column1 = companyCode;
					line.Column9 = messageTime.ToLongTimeString();

					if (priceItemCode == "SCO")
					{
						line.Column2 = ref2;
						line.Column3 = ref3;
					}
					else if (priceItemCode == "SCS")
					{
						line.Column2 = ref2;
					}
					else
					{
						line.Column2 = ref2;
						line.Column3 = billableCount.ToString(CultureInfo.InvariantCulture);
						line.Column4 = ref3;
					}
				}
			}

			return rawUsage;
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, Action<string> action)
		{
			string currentItemCode = "";

			using (var command = GetRawUsageQuery(context))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					string clientId = (string)reader["TX_ClientID"];
					string companyCode = (string)reader["CompanyCode"];
					int billableCount = (int)reader["TX_BillableCount"];
					string ref2 = (string)reader["TX_Reference2"];
					string ref3 = (string)reader["TX_Reference3"];
					string interfaceName = (string)reader["TX_Reference1"];
					string priceItemCode = (string)reader["TX_PriceItemCode"];
					string interfacePriceDescription = (string)reader["InterfacePriceDescription"];
					var interfaceDescription = (!string.IsNullOrEmpty(interfacePriceDescription) ? interfacePriceDescription : interfaceName).Replace("\r\n", " ");
					ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];

					if (string.Compare(priceItemCode, currentItemCode, StringComparison.OrdinalIgnoreCase) != 0)
					{
						var headerColumns = GetCsvHeaderColumns(priceItemCode, isStlBilling);
						var headerCsvLine = new OCsvLine(headerColumns);
						action(headerCsvLine.ToString());
						currentItemCode = priceItemCode;
					}

					var dataValues = GetCsvDataValues(priceItemCode, isStlBilling, clientId, companyCode, ref2, ref3, interfaceDescription, billableCount, messageTime);
					var dataCsvLine = new OCsvLine(dataValues);
					action(dataCsvLine.ToString());
				}
			}
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, ICsvUsageReportWriter writer)
		{
			using (var command = GetRawUsageQuery(context))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					string clientId = (string)reader["TX_ClientID"];
					string companyCode = (string)reader["CompanyCode"];
					int billableCount = (int)reader["TX_BillableCount"];
					string ref2 = (string)reader["TX_Reference2"];
					string ref3 = (string)reader["TX_Reference3"];
					string interfaceName = (string)reader["TX_Reference1"];
					string priceItemCode = (string)reader["TX_PriceItemCode"];
					string interfacePriceDescription = (string)reader["InterfacePriceDescription"];
					var interfaceDescription = (!string.IsNullOrEmpty(interfacePriceDescription) ? interfacePriceDescription : interfaceName).Replace("\r\n", " ");
					ZDateTime messageTime = (DateTime)reader["TX_ServiceOccuredUTC"];
					string branch = (string)reader["TX_Branch"];
					string staff = (string)reader["TX_ClientStaffCode"];

					string client = isStlBilling ? companyCode : clientId;
					writer.WriteCsvUsageReport(messageTime, client, branch, staff, string.Concat(interfaceName, ' ', ref2, ' ', ref3).Trim(), priceItemCode, interfaceDescription, billableCount);
				}
			}
		}

		string[] GetCsvHeaderColumns(string priceItemCode, bool isStlBilling)
		{
			var clientHeaderColumn = isStlBilling ? "Company Code" : "Client ID";

			if (priceItemCode == "SCO")
			{
				return new string[] { "Interface", clientHeaderColumn, "Order No.", "Buyer", "Message Time (UTC)" };
			}
			else if (priceItemCode == "SCS")
			{
				return new string[] { "Interface", clientHeaderColumn, "Shipment ID", "Message Time (UTC)" };
			}
			else
			{
				return new string[] { "Interface", clientHeaderColumn, "Element", "Units", "Filename", "Message Time (UTC)" };
			}
		}

		string[] GetCsvDataValues(string priceItemCode, bool isStlBilling, string clientId, string companyCode, string ref2, string ref3, string interfaceDescription, int billableCount, ZDateTime messageTime)
		{
			string client = isStlBilling ? companyCode : clientId;

			if (priceItemCode == "SCO")
			{
				return new string[] { interfaceDescription, client, ref2, ref3, messageTime.ToLongTimeString() };
			}
			else if (priceItemCode == "SCS")
			{
				return new string[] { interfaceDescription, client, ref2, messageTime.ToLongTimeString() };
			}
			else
			{
				return new string[] { interfaceDescription, client, ref2, billableCount.ToString(CultureInfo.InvariantCulture), ref3, messageTime.ToLongTimeString() };
			}
		}

		#endregion

		#region SQL

		protected override string Query_Raw_Usage
		{
			get { return sql_Raw_Usage; }
		}

		const string sql_Raw_Usage = @"
SELECT 
	TX_ClientID,
	CompanyCode = ISNULL(LCC_Code, ''),
	TX_PriceItemCode,
	TX_BillableCount,
	TX_Reference1,
	TX_Reference2 = ISNULL(TX_Reference2, ''),
	TX_Reference3 = ISNULL(TX_Reference3, ''),
	TX_Reference4 = ISNULL(TX_Reference4, ''),
	TX_ServiceOccuredUTC,
	ISNULL(L7_Description, '') AS InterfacePriceDescription,
	TX_Branch,
	TX_ClientStaffCode
FROM
	(
		SELECT
			TX_PriceItemCode,
			TX_ClientId,
			TX_LCC,
			TX_BillableCount,
			TX_Reference1,
			TX_Reference2,
			TX_Reference3,
			TX_Reference4,
			TX_ServiceOccuredUTC,
			TX_Branch = ISNULL(TX_Branch, ''),
			TX_ClientStaffCode = ISNULL(TX_ClientStaffCode, '')
		FROM
			BillingViewChargeable
		WHERE
			TX_Category = 'CMP'
			AND TX_PriceItemCode in ('CMP', 'SCO', 'SCS')
			AND TX_SystemId = @DatabaseId
			AND (@ClientCompanyPk IS NULL OR TX_LCC = @ClientCompanyPk)
			AND TX_Period = @Period
	) a
	LEFT JOIN
	(
		SELECT L7_Ref4, L7_Description
		FROM
		(
			SELECT L7_Ref4, L7_Description, Row_Number() OVER (PARTITION BY L7_Ref4 ORDER BY L7_description) AS Ranking 
			FROM dbo.ClientLicencePriceItem 
			WHERE L7_Code = 'CMP'
		) InterfacePriceDescriptions
		WHERE Ranking = 1
	) InterfacePriceDescriptionMapping ON TX_Reference1 = L7_Ref4 COLLATE DATABASE_DEFAULT
	LEFT JOIN dbo.ClientCompany ON TX_LCC = LCC_PK
ORDER By
	TX_PriceItemCode, TX_Reference1, TX_ClientID, TX_ServiceOccuredUTC
";

		#endregion
	}
}

