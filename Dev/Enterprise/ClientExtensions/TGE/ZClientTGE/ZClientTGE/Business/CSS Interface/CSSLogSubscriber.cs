using System;
using System.Collections.Generic;
using CargoWise.Common.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.TGE.Business
{
	[Serializable]
	public class CSSLogSubscriber : LogSubscriber
	{
		public override string[] EventTypes
		{
			get
			{
				UniqueList<string> eventList = new UniqueList<string>();
				AddEventCodes(eventList, TGEDataRegistry.Instance.CSSImportCustomsStatusCodes);
				AddEventCodes(eventList, TGEDataRegistry.Instance.CSSExportCustomsStatusCodes);
				return eventList.ToArray();
			}
		}

		public override string Name
		{
			get { return "TGECSSCustomsResponse"; }
		}

		public override string FriendlyName
		{
			get { return "TGE CSS Customs Response"; }
		}

		public override string[] TableNames
		{
			get { return new[] { CusHAWBSchema.Constants.TableName, JobDeclarationSchema.Constants.TableName, JobShipmentSchema.Constants.TableName }; }
		}

		public override bool IsClientSpecificSubscriber => true;

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			if (queuedLogs.LongLength > 0)
			{
				factory = queuedLogs[0].Factory;
				List<BusinessObject> bizoList = new List<BusinessObject>();

				foreach (IQueuedLog queuedLog in queuedLogs)
				{
					switch (queuedLog.SJ_ParentTableCode)
					{
						case CusHAWBSchema.Constants.Prefix:
							CusHAWB cusHawb = queuedLog.Factory.Load<CusHAWB>(queuedLog.SJ_ParentID);
							if (cusHawb != null && IsCustomsImportEvent(queuedLog.SJ_SE_NKEvent))
							{
								bizoList.Add(cusHawb);
							}
							break;

						case JobDeclarationSchema.Constants.Prefix:
							JobDeclaration jobDec = queuedLog.Factory.Load<JobDeclaration>(queuedLog.SJ_ParentID);
							if (jobDec != null &&
								IsCustomsExportEvent(queuedLog.SJ_SE_NKEvent) &&
								BelongsToCurrentCompany(jobDec.Branch.PK) &&
								jobDec.Shipment != null &&
								jobDec.Shipment.Consols != null &&
								HasNoDexForCurrentEntryStatus(jobDec) &&
								jobDec.JE_TransportMode == Core.Constants.TransportModes.Air &&
								jobDec.IsExport)
							{
								bizoList.Add(jobDec);
							}
							break;

						case JobShipmentSchema.Constants.Prefix:
							ForwardingShipment shipment = queuedLog.Factory.Load<ForwardingShipment>(queuedLog.SJ_ParentID);
							if (shipment != null &&
								IsCustomsExportEvent(queuedLog.SJ_SE_NKEvent) &&
								shipment.Consols != null &&
								shipment.Consols.Count > 0 &&
								!shipment.CustomsEntryNumberType.IsEmpty &&
								HasNoDexForCurrentEntryStatus(shipment) &&
								shipment.JS_TransportMode == Core.Constants.TransportModes.Air &&
								shipment.IsExport())
							{
								bizoList.Add(shipment);
							}
							break;
					}
				}

				if (bizoList.Count > 0)
				{
					Exporter.Export(bizoList, new NotificationBuffer());
				}
			}
		}

		bool IsCustomsImportEvent(ZString importEventCode)
		{
			return TGEDataRegistry.Instance.CSSImportCustomsStatusCodes.ContainsCode(importEventCode);
		}

		bool IsCustomsExportEvent(ZString exportEventCode)
		{
			return TGEDataRegistry.Instance.CSSExportCustomsStatusCodes.ContainsCode(exportEventCode);
		}

		void AddEventCodes(UniqueList<string> eventList, TGEEventRegistryBusinessObjectCollection collection)
		{
			foreach (TGEEventRegistryBusinessObject item in collection)
			{
				eventList.Add(item.Code);
			}
		}

		bool BelongsToCurrentCompany(ZGuid jobDecBranchPk)
		{
			return GlbCompany.CurrentCompany.Branches.FindByPK(jobDecBranchPk) != null;
		}

		bool HasNoDexForCurrentEntryStatus(JobDeclaration jobDec)
		{
			ZQuery filter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code);
			filter.AddToFilter(StmALogSchema.SL_Reference, String.Format(CSSExporter.DEXEventReferenceFormat, export, jobDec.Shipment.CustomsEntryNumberType + " " +
				jobDec.Shipment.CustomsEntryNumber));
			filter.AddToFilter(StmALogSchema.SL_Parent, jobDec.PK);
			return !factory.ExistsInDatabase(BusinessObjectFactory.GetTableNameFromType(typeof(StmALog)), filter);
		}

		bool HasNoDexForCurrentEntryStatus(ForwardingShipment shipment)
		{
			ZQuery filter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code);
			filter.AddToFilter(StmALogSchema.SL_Reference, String.Format(CSSExporter.DEXEventReferenceFormat, export, shipment.CustomsEntryNumberType + " " +
				shipment.CustomsEntryNumber));
			filter.AddToFilter(StmALogSchema.SL_Parent, shipment.PK);
			return !factory.ExistsInDatabase(BusinessObjectFactory.GetTableNameFromType(typeof(StmALog)), filter);
		}
		const string export = "Export";

		CSSExporter Exporter
		{
			get { return exporter ?? (exporter = new CSSExporter(factory)); }
		}
		[NonSerialized]
		CSSExporter exporter;

		[NonSerialized]
		BusinessObjectFactory factory;
	}
}
