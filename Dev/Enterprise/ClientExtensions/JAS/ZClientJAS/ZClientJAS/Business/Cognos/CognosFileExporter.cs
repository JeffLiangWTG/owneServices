using System;
using System.ComponentModel;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.JAS.Business.Cognos
{
	public class CognosFileExporter
	{
		public CognosFileExporter(ZDateTime exportStartDateTime, ICognosNotificationSubscriber notifications)
		{
			this.Factory = new BusinessObjectFactory();
			this.ExportStartDateTime = exportStartDateTime;
			this.Notifications = notifications;
		}

		public bool ExportToFile(ZString exportFilePath)
		{
			bool result = false;

			if (exportFilePath.IsEmpty)
			{
				throw new ArgumentException("ExportFilePath has to be specified");
			}

			try
			{
				ExportToFileCore(exportFilePath);
				result = true;
			}
			catch (IOException ex)
			{
				Notifications.Notify(new ErrorNotification(ErrorType.IOError, ex.Message));
			}

			return result;
		}

		protected virtual void ExportToFileCore(ZString exportFilePath)
		{
			using (StreamWriter writer = new StreamWriter(exportFilePath))
			{
				WriteAccountingCognosLines(writer);
				WriteNonAccountingCognosLines(writer);
			}
		}

		#region Accounting Data

		void WriteAccountingCognosLines(StreamWriter writer)
		{
			using (new CognosTempTableCreator())
			{
				CognosAccountsAggregator aggregator = GetNewCognosAccountsAggregator();
				aggregator.AggregateToExportTempTable();
				WriteAccountingCognosLinesFromExportTempTable(writer);
			}
		}

		void WriteAccountingCognosLinesFromExportTempTable(StreamWriter writer)
		{
			const string Description = "Writing Accounting Data to CSV file";
			Notifications.Notify(new InfoNotification(Description));

			DynamicBusinessObjectCollection<CognosLineBizO> cognosLineCollection = new DynamicBusinessObjectCollection<CognosLineBizO>(Factory);
			cognosLineCollection.Load(SQLText);
			cognosLineCollection.ApplySort(new SortInfo(CognosLineBizO.Schema.AccountCode, ListSortDirection.Ascending));
			SortedCognosLines sortedCognosLines = new SortedCognosLines(Factory);
			foreach (CognosLineBizO line in cognosLineCollection)
			{
				sortedCognosLines.Add(line);
			}
			ZString accountingDataLines = sortedCognosLines.GetLinesAsString();
			if (!accountingDataLines.IsEmpty)
			{
				writer.WriteLine(accountingDataLines);
			}

			Notifications.AdvanceProgressBy(3);
		}

		protected virtual CognosAccountsAggregator GetNewCognosAccountsAggregator()
		{
			return new CognosAccountsAggregator(ExportStartDateTime, Notifications);
		}

		const string SQLText = @"
SELECT      AJ_PK AS AccountPK,
            AJ_AccountDescription AS AccountName, 
            AJ_LocalAccountNumber AS AccountCode, 
            T6_CompanyCode AS CounterCompany,
            T6_Mode AS Mode, 
            T6_Branch AS Branch, 
            T6_BusinessType AS Business, 
            T6_Amount AS Amount,
            T6_TransactionCurrency AS TransactionCurrency, 
            T6_TransactionAmount AS TransactionAmount, 
            T6_Geographical AS Geographical

FROM        #CognosExport
            INNER JOIN dbo.AccGLAccountDescriptor ON T6_AJ = AJ_PK

UNION ALL

SELECT      NEWID() AS AccountPK,
            MIN(AJ_AccountDescription) AS AccountName,
            T9_ReconciliationTotalAccount AS AccountCode,
            '' AS CounterCompany,
            '' AS Mode, 
            '' AS Branch,
            '' AS Business,
            SUM(T6_Amount) AS Amount,
            '' AS TransactionCurrency,
            0 AS TransactionAmount,
            '' AS Geographical
            
FROM        #CognosExport
            INNER JOIN dbo.AccGLAccountDescriptor ON T6_AJ = AJ_PK
            INNER JOIN ClientCognosAccGLAccountDescriptorExtraInfo ON T6_AJ = T9_AJ AND T9_ReconciliationTotalAccount != ''
GROUP BY    T9_ReconciliationTotalAccount
";

		#endregion

		#region Non Accounting Data

		void WriteNonAccountingCognosLines(StreamWriter writer)
		{
			const string Description = "Writing Statistical Data to CSV file";
			Notifications.Notify(new InfoNotification(Description));

			NonAccountingCognosLineGenerator lineGenerator = GetNewNonAccountingCognosLineGenerator();
			writer.Write(lineGenerator.GetLinesAsString());

			Notifications.AdvanceProgressBy(10);
		}

		protected virtual NonAccountingCognosLineGenerator GetNewNonAccountingCognosLineGenerator()
		{
			return new NonAccountingCognosLineGenerator(Factory, ExportStartDateTime);
		}

		#endregion

		readonly BusinessObjectFactory Factory;
		public readonly ZDateTime ExportStartDateTime;
		public readonly ICognosNotificationSubscriber Notifications;
	}
}
