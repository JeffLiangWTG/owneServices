using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	[ModuleID(ModuleId.PrintQueue)]
	public class StmPrintQueueCollection : BusinessObjectCollection<StmPrintQueue>, Enterprise.Integration.DocumentEngine.IStmPrintQueueCollection
	{
		public StmPrintQueueCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public StmPrintQueueCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public static StmPrintQueueCollection GetPrintersVisibleToCurrentUser(BusinessObjectFactory factory, bool hideUnallowed, bool onlyOnline = false)
		{
			var result = new StmPrintQueueCollection(factory);

			var filter = new ZQuery(StmPrintQueueSchema.SQ_AllowPrinting, ZBool.True);

			if (onlyOnline)
			{
				filter.AddToFilter(StmPrintQueueSchema.SQ_QueueDeleted, DBNull.Value);
			}

			result = new StmPrintQueueCollection(factory, filter);
			result.Load();

			if (hideUnallowed)
			{
				for (int i = (result.Count - 1); i >= 0; i--)
				{
					if (!result[i].IsPrintAllowed)
					{
						result.Remove(result[i]);
					}
				}
			}

			result.Sort(StmPrintQueueSchema.Constants.SQ_DisplayName, ListSortDirection.Ascending);

			return result;
		}

		static string GetQualifiedQueueName(StmPrintQueue queue)
		{
			return @"\\" + queue.SQ_ServerName + @"\" + queue.SQ_QueueName;
		}

		public CodeDescriptionPairList GetOnlinePrinterNames()
		{
			Dictionary<string, List<StmPrintQueue>> queuesByName = new Dictionary<string, List<StmPrintQueue>>();
			foreach (StmPrintQueue printer in this)
			{
				if (printer.IsOnline)
				{
					string name = printer.SQ_DisplayName + (printer.IsPrintAllowed ? "" : (NoResString)" (No Rights)");
					List<StmPrintQueue> listForName;
					if (queuesByName.TryGetValue(name, out listForName))
					{
						listForName.Add(printer);
					}
					else
					{
						queuesByName[name] = new List<StmPrintQueue>(new StmPrintQueue[] { printer });
					}
				}
			}

			CodeDescriptionPairList result = new CodeDescriptionPairList();
			foreach (KeyValuePair<string, List<StmPrintQueue>> queueName in queuesByName)
			{
				List<StmPrintQueue> queuesWithTheSameName = queueName.Value;
				if (queuesWithTheSameName.Count == 1)
				{
					result.AddPair(queuesWithTheSameName[0].PK, queueName.Key, queuesWithTheSameName[0].SQ_ServerName);
				}
				else
				{
					foreach (StmPrintQueue queue in queuesWithTheSameName)
					{
						result.AddPair(queue.PK, queueName.Key + " (" + GetQualifiedQueueName(queue) + ")", queue.SQ_ServerName);
					}
				}
			}

			result.Sort();
			return result;
		}

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy()
		{
			return new StmPrintQueueCollectionFetchStrategy(this);
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX notifications, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(notifications, selectedBusinessObject);

			if (!((StmPrintQueue)selectedBusinessObject).SQ_AllowPrinting)
			{
				notifications.Add(Res.GetString("33e3e941-eab6-4612-9921-8c87e3a08842", "This printer is inactive and cannot be used."));
			}
		}
	}
}
