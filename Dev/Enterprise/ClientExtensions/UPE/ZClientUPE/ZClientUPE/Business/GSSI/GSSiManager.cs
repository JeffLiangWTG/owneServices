using System;
using System.Collections.Generic;
using CargoWise.Common.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.BISI;
using Enterprise.Client.UPE.Business.GSSI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business.GSSi
{
	class GSSiManager
	{
		#region Instance
		public static GSSiManager Instance
		{
			get
			{
				return fInstance ?? (fInstance = new GSSiManager());
			}
		}
		[ThreadStatic]
		static GSSiManager fInstance;
		#endregion

		public void ProcessGSSIHOLDMessage(EnterpriseBusinessObject queueParent, IShipmentStatusData currentQueueLog, Func<UniqueList<ZString>> uniqueTrackingIDs, RefUNLOCO portOfFirstArrival, string triggeredBy = "")
		{
			if (currentQueueLog.ShipmentStatus == "03" && currentQueueLog.HoldReasonCode.Length == 2)
			{
				uniqueTrackingIDs().ForEach((ZString billNum) =>
				{
					CreateHoldMessage(billNum, queueParent, currentQueueLog, portOfFirstArrival, triggeredBy);
				});
			}
		}

		internal void CreateHoldMessage(ZString trackingID, EnterpriseBusinessObject logParent, IShipmentStatusData currentQueueLog, RefUNLOCO port, string triggeredBy = "")
		{
			logParent.Factory.New<GSSMessage>().EM_MessageText = new GSSiMessage(trackingID, GetBuildingID(port), currentQueueLog).ToString();

			CreateExportLog(logParent, trackingID, currentQueueLog.ShipmentStatus, currentQueueLog.HoldReasonCode, triggeredBy);
		}

		public void CreateResolutionMessage(EnterpriseBusinessObject logParent, BusinessObjectFactory factory, ZString trackingNumber, IShipmentStatusData currentQueueLog, RefUNLOCO port)
		{
			ZString buildingID = GetBuildingID(port);

			List<ZString> holdReasonCodes = new List<ZString>();

			if (logParent != null)
			{
				var queryReference = $"{trackingNumber.ToUpper()}|03|";
				var query = new ZQuery(StmALogSchema.SL_Parent, logParent.PK);
				query.AddToFilter(StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.UtcNow.AddMonths(-3));
				query.AddToFilter(StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, ZDateTime.UtcNow);
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.DataExportCode);
				query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, queryReference);

				var exportedHOLDLogs = factory.Load<StmALog>(query);

				foreach (var current in exportedHOLDLogs)
				{
					var logReference = current.SL_Reference.Split('|');
					if (logReference.Length >= 3)
					{
						holdReasonCodes.Add(logReference[2]);
					}
				}
			}

			if (holdReasonCodes.Count > 0)
			{
				foreach (var holdReasonCode in holdReasonCodes)
				{
					factory.New<GSSMessage>().EM_MessageText = new GSSiMessage(trackingNumber, buildingID, currentQueueLog, holdReasonCode).ToString();

					CreateExportLog(logParent, trackingNumber, currentQueueLog.ShipmentStatus, holdReasonCode);
				}
			}
			else if (logParent is JobDeclaration)
			{
				factory.New<GSSMessage>().EM_MessageText = new GSSiMessage(trackingNumber, buildingID, currentQueueLog).ToString();
			}
		}

		void CreateExportLog(EnterpriseBusinessObject logParent, ZString trackingNumber, ZString shipmentStatus, ZString holdReasonCode, string triggeredBy = "")
		{
			if (logParent != null)
			{
				if (String.IsNullOrEmpty(triggeredBy))
				{
					logParent.Logs.AddNew(Events.DataExport, $"{trackingNumber.ToUpper()}|{shipmentStatus}|{holdReasonCode}");
				}
				else
				{
					logParent.Logs.AddNew(Events.DataExport, $"{trackingNumber.ToUpper()}|{shipmentStatus}|{holdReasonCode}|{triggeredBy}");
				}
			}
		}
		public IShipmentStatusData GetLatestProcessLogByQueueType(ZString queueType, BusinessObjectFactory factory, ZGuid queuePK)
		{
			ZQuery shipmentStatusFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.QueueChanged.Code);
			shipmentStatusFilter.AddToFilter(StmALogSchema.SL_Parent, queuePK);
			shipmentStatusFilter.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, queueType);
			shipmentStatusFilter.OrderBy = StmALogSchema.Constants.SL_EventTime + " desc";
			return factory.LoadTop1<UPEProcessQueueLog>(shipmentStatusFilter);
		}

		ZString GetBuildingID(RefUNLOCO port)
		{
			ZString buildingID = ZString.Empty;
			if (port != null)
			{
				buildingID = UPEDataRegistry.Instance.GetBuildingIDByPortOfArrival(port.PK).ToString();

				if (string.IsNullOrEmpty(buildingID))
				{
					buildingID = UPEDataRegistry.Instance.DefaultBuildingID;
				}
			}

			return buildingID;
		}
	}
}
