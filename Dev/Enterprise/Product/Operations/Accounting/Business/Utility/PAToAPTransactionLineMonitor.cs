using System.Collections.Generic;
using System.Diagnostics;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business
{
	public class PAToAPTransactionLineMonitor : IService
	{
		public PAToAPTransactionLineMonitor(AccTransactionHeader header)
		{
			this.headerPK = header.PK;
		}

		public static void Create(AccTransactionHeader header)
		{
			if (!AccountingConfigurationRegistry.Instance.EnableTransactionLineMonitor.Value)
			{
				return;
			}

			if (header.AH_Ledger != LedgerTypes.TransactionsPendingAllocation ||
				header.AH_TransactionType != TransactionTypes.InvoicePendingAllocation)
			{
				return;
			}

			var factory = header.Factory;
			var result = factory.ServiceContainer.GetService<PAToAPTransactionLineMonitor>();

			if (result == null)
			{
				var monitor = new PAToAPTransactionLineMonitor(header);
				factory.ServiceContainer.AddService(monitor);
			}
		}

		public static PAToAPTransactionLineMonitor GetInstance(AccTransactionHeader header)
		{
			var service = header.Factory.ServiceContainer.GetService<PAToAPTransactionLineMonitor>();
			return (service != null && service.IsApplicable(header)) ? service : null;
		}

		public void RecordLineCount(string log,  int lineCount)
		{
			AddLineCount(log, lineCount);
		}

		public void RecordLineItemChange(CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded)
			{
				addLineCount++;
			}

			if (e.ItemRemoved)
			{
				removeLineCount++;
				lastLineItemRemoveStackTrace = new StackTrace().ToString();
			}
		}

		public string GetInfo()
		{
			var strBuilder = new ZStringBuilder();
			lineCountHistory.ForEach(x => strBuilder.Append(x.Log + ": " + x.Count));

			var lineCountInfo = strBuilder.IsEmpty ? "N/A" : strBuilder.ToStringWithNewLineBetweenAppends();

			return
$@"Last 10 counts of lines:
{lineCountInfo}

Stack trace of last line remove:
{lastLineItemRemoveStackTrace ?? "N/A"}

Total calls of adding line: {addLineCount}
Total calls of removing line: {removeLineCount}
";
		}

		bool IsApplicable(AccTransactionHeader header) => header.PK == headerPK;

		void AddLineCount(string log, int lineCount)
		{
			if (lineCountHistory.Count >= MaxQueueSize)
			{
				lineCountHistory.Dequeue();
			}

			lineCountHistory.Enqueue((log, lineCount));
		}

		const int MaxQueueSize = 10;
		readonly ZGuid headerPK;
		string lastLineItemRemoveStackTrace;
		int addLineCount, removeLineCount;
		readonly Queue<(string Log, int Count)> lineCountHistory = new Queue<(string Log, int Count)>(MaxQueueSize);
	}
}
