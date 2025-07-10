using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MailManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MailManager.MailFilters
{
	public enum MailProcessingResult { MarkSuccess, MarkFailed, Unmatch, Delete, DoNothing }
	public delegate MailProcessingResult ProcessMailItem(MailItem mail, (int index, int batchCount) position);

	public class MailBatchProcessor : ManagedBatchProcessor<MailItem>
	{
		readonly IMailFilter filter;
		readonly ProcessMailItem process;
		readonly HashSet<ZGuid> processedPks = new HashSet<ZGuid>();

		public override int BatchSize { get; }

		public MailBatchProcessor(IMailFilter filter, ProcessMailItem process, int batchSize = 50)
		{
			this.filter = Argument.NotNull(filter, nameof(filter));
			this.process = Argument.NotNull(process, nameof(process));

			BatchSize = batchSize;
		}

		public static void MarkFailed(MailItem m)
			=> m.MI_Status = MailStatus.Failed;

		public static void MarkSuccess(MailItem m)
			=> m.MI_Status = MailStatus.Processed;

		void MarkUnmatch(MailItem m)
		{
			processedPks.Add(m.PK);
			m.MI_Status = MailStatus.Queued;
		}

		MailProcessingResult ProcessSingle(MailItem item, (int, int) position)
			=> process(item, position);

		protected override void ProcessRowCore(INotifications notifications, CancellationToken token, MailItem item, BusinessObjectFactory factory, (int, int) position)
		{
			switch (ProcessSingle(item, position))
			{
				case MailProcessingResult.MarkSuccess:
					MarkSuccess(item);
					break;
				case MailProcessingResult.MarkFailed:
					MarkFailed(item);
					break;
				case MailProcessingResult.Unmatch:
					MarkUnmatch(item);
					break;
				case MailProcessingResult.Delete:
					item.Delete();
					break;
				case MailProcessingResult.DoNothing:
					break;
				default:
					throw new InvalidOperationException("Missing result.");
			}
		}

		protected override ZQuery GetQuery()
		{
			var query = filter.LoadQuery();
			if (processedPks.Count > 0)
			{
				query.AddToFilter(MailDBItemsSchema.PK, SQLComparisonOperator.NotEqual, processedPks);
			}

			return query;
		}

		protected override ZQuery GetSingularQuery(MailItem row) => new ZQuery(MailDBItemsSchema.PK, row.PK);
		protected override IList<MailItem> LoadBatchCore(BusinessObjectFactory factory, ZQuery query) => factory.Load<MailItem>(query);
		protected override void MarkRowAsBadCore(INotifications notifications, MailItem row, BusinessObjectFactory factory) => MarkFailed(row);
	}
}
