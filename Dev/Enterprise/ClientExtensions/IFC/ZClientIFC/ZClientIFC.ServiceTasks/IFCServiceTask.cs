using System;
using System.IO;
using System.Threading;
using CargoWise.Common.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Client.IFC.ConsolExport.ServiceTasks.IFCServiceTask.Code,
	"Export of Consols & Shipments for ESC System",
	"CSP",
	typeof(Enterprise.Client.IFC.ConsolExport.ServiceTasks.IFCServiceTask),
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "60minutes",
	ActiveByDefault = true,
	CanRunInAnyBranch = true
	)]
namespace Enterprise.Client.IFC.ConsolExport.ServiceTasks
{
	class IFCServiceTask : ServiceProviderImpl
	{
		#region Service Tasks overrides
		public override void RunTask(CancellationToken token)
		{
			Buffer = new NotificationBuffer(ServiceLogger.GetTaskNotificationSubscriber());
			if (IFCDataRegistry.Instance.FSCExportDirectory.IsEmpty || !Directory.Exists(IFCDataRegistry.Instance.FSCExportDirectory))
			{
				Buffer.Notify(new ErrorNotification(ErrorType.Error, "Export directory does not exist or invalid"));
				return;
			}
			StmALog[] logs = Factory.Load<StmALog>(LogFilter());
			HighWaterMark = ZDateTime.UtcNow;

			if (logs != null && logs.Length > 0)
			{
				foreach (StmALog log in logs)
				{
					token.ThrowIfCancellationRequested();
					if (log.SL_Table == JobShipmentSchema.Constants.TableName)
					{
						AddShipmentConsolsToList(log.SL_Parent);
					}
					else if (log.SL_Table == JobConsolSchema.Constants.TableName)
					{
						AddConsolToList(log.SL_Parent);
					}
					else if (log.SL_Table == StorageMainSchema.Constants.TableName)
					{
						DocumentFactory docFactory = new DocumentFactoryProvider().GetFactory(Factory);
						StorageMain storageMain = docFactory.Load<StorageMain>(log.SL_Parent);
						if (storageMain != null && storageMain.SM_Type == "SHP")
						{
							AddShipmentConsolsToList(storageMain.SM_ParentFK);
						}
						else if (storageMain != null && storageMain.SM_Type == "CON")
						{
							AddConsolToList(storageMain.SM_ParentFK);
						}
					}
				}

				if (ConsolList.Count > 0)
				{
					Export();
				}
			}
		}

		#endregion

		#region Implementation

		#region Export

		void Export()
		{
			ConsolXmlExporter exporter = new ConsolXmlExporter();
			foreach (ForwardingConsol consol in ConsolList)
			{
				exporter.Export(consol, Buffer);
			}
		}

		#endregion

		#region Log Filter

		protected ZQuery LogFilter()
		{
			ZQuery result = new ZQuery(StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.GreaterThan, HighWaterMark); //select logs between last hwm
			result.AddToFilter(JoinCondition.And, StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.UtcNow); //and Now.

			ZQuery tableQuery = new ZQuery(StmALogSchema.SL_Table, JobConsolSchema.Constants.TableName);
			tableQuery.AddToFilter(JoinCondition.Or, StmALogSchema.SL_Table, JobShipmentSchema.Constants.TableName);
			tableQuery.AddToFilter(JoinCondition.Or, StmALogSchema.SL_Table, StorageMainSchema.Constants.TableName);

			ZQuery eventQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AddedARecordToTheSystem.Code);
			eventQuery.AddToFilter(JoinCondition.Or, StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code);

			result.AddToFilter(eventQuery);
			result.AddToFilter(tableQuery);

			result.OrderBy = StmALogSchema.SL_PostedTimeUtc.Name;
			result.IsNoLock = false;

			return result;
		}

		#endregion

		#region Lists

		UniqueList<ForwardingConsol> ConsolList
		{
			get { return consolList ?? (consolList = new UniqueList<ForwardingConsol>()); }
		}
		UniqueList<ForwardingConsol> consolList;

		void AddConsolToList(ZGuid consolPK)
		{
			ForwardingConsol con = Factory.Load<ForwardingConsol>(consolPK);
			if (con != null)
			{
				ConsolList.Add(con);
			}
		}

		void AddShipmentConsolsToList(ZGuid shipmentPK)
		{
			ForwardingShipment ship = Factory.Load<ForwardingShipment>(shipmentPK);
			if (ship != null && ship.Consols.Count > 0)
			{
				ConsolList.AddRange(ship.Consols.ToArray<ForwardingConsol>());
			}
		}

		#endregion

		#region Factory

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		#endregion

		#region HighWaterMark

		ZDateTime HighWaterMark
		{
			get
			{
				ZDateTime regoDateTime = new ZDateTime(SystemDataRegistry.Instance.SystemLogBatchProcessHighWaterMark.Value);
				if (!regoDateTime.IsValid)
				{
					regoDateTime = ZDateTime.MinSmallDateTimeValue;
				}
				return regoDateTime;
			}
			set
			{
				if (value.IsValid)
				{
					SystemDataRegistry.Instance.SystemLogBatchProcessHighWaterMark.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToDateTime());
				}
			}
		}

		#endregion

		#endregion

		internal NotificationBuffer Buffer;

		public const string Code = "ZI1";
	}
}
