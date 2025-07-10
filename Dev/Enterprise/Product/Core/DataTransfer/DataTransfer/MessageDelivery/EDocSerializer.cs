using System.Collections.Generic;
using System.Globalization;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.MessageDelivery
{
	class EDocSerializer : IBusinessObjectSerializer
	{
		public EDocSerializer(IValueObjectDataAdapter dataAdapter, ProcessTaskNotification action, IValueObjectExportContext context, IEDICommunicationsMode mode, EventInfoProvider eventInfo)
		{
			this.context = context;
			serializer = new ExlBusinessObjectSerializer(dataAdapter, action.PQ_MessagePurpose) { Context = context, Mode = mode };
			this.triggeringLog = eventInfo != null ? eventInfo.Event : null;
		}

		readonly IValueObjectExportContext context;
		readonly ExlBusinessObjectSerializer serializer;
		readonly StmALog triggeringLog;

		public SubStreamableStream SerializeToStream(BusinessObject businessObject)
		{
			return SerializeToStream(new[] { businessObject });
		}

		public SubStreamableStream SerializeToStream(IEnumerable<BusinessObject> businessObjects)
		{
			var eDocs = new List<BusinessObject>();

			if (triggeringLog != null)
			{
				foreach (var businessObject in businessObjects)
				{
					eDocs.AddRange(GetEDocsToDeliver(businessObject));
				}

				if (eDocs.Count > 0)
				{
					return serializer.SerializeToStream(eDocs);
				}
				else
				{
					ReportNoDocsToSerialize();
					return null;
				}
			}
			else
			{
				return null;
			}
		}

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Debug information")]
		void ReportNoDocsToSerialize()
		{
			if (context != null)
			{
				var logData = string.Format(CultureInfo.InvariantCulture, "reference '{0}', parent table '{1}'", triggeringLog.SL_Reference, triggeringLog.SL_Table);
				var shipment = triggeringLog.Master as Enterprise.Integration.Forwarding.IForwardingShipment;
				if (shipment != null)
				{
					var documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
					var documentFactory = (BusinessObjectFactory)documentFactoryProvider.GetFactory(triggeringLog.Master.Factory);
					var storageMain = documentFactory.LoadTop1<IStorageMain>(new ZQuery(StorageMainSchema.SM_ParentFK, shipment.PK)) as BusinessObject;
					if (storageMain != null)
					{
						logData += string.Format(CultureInfo.InvariantCulture, ", Document DB ID: {0}", storageMain[StorageMainSchema.Constants.SM_DB]);
					}
				}

				var docPK = StmALog.GetGuid(triggeringLog.SL_Reference);
				var extraMessage = !docPK.IsEmpty && docPK.IsValid
					? Res.GetString("FA45661E-AC0F-4E57-8B5B-F3F6DEF3C6C5", "Cannot find matched document, it may have been deleted.")
					: Res.GetString("6ABD9BC0-73CF-4AC7-A5CC-A41C73D75578", "Event reference does not contain document information, the event may have been created manually.");

				var errorMessage = string.Format(CultureInfo.InvariantCulture, "0 eDocs to deliver.\r\nTriggering event: {0}. {1}", logData, extraMessage);

				context.Add(CargoWise.ComponentModel.NotificationType.Warning, errorMessage);
			}
		}

		IEnumerable<BusinessObject> GetEDocsToDeliver(IBusiness bizObjWithEDoc)
		{
			// getting eDoc(s) which fired Action trigger 
			var eDocs = new List<BusinessObject>();

			var bizObj = bizObjWithEDoc as IStmALogParent;
			var docManagerSupport = bizObjWithEDoc as IDocManagerSupport;

			if (bizObj != null && docManagerSupport != null)
			{
				FindEDocsToDeliver(bizObj, triggeringLog, eDocs);

				foreach (var relatedBizo in bizObj.BusinessObjectsWithRelatedEvents)
				{
					FindEDocsToDeliver(relatedBizo as IStmALogParent, triggeringLog, eDocs);
				}
			}

			return eDocs;
		}

		void FindEDocsToDeliver(IStmALogParent logsParent, StmALog log, List<BusinessObject> eDocs)
		{
			var docManagerSupport = logsParent as IDocManagerSupport;
			if (logsParent != null && docManagerSupport != null)
			{
				var storageDocsPK = StmALog.GetGuid(log.SL_Reference);

				if (!storageDocsPK.IsEmpty && storageDocsPK.IsValid)
				{
					foreach (IeDoc eDocument in docManagerSupport.DocManagerInfo.AllEDocs)
					{
						if (eDocument.UniqueKey == storageDocsPK)
						{
							eDocs.Add((BusinessObject)eDocument);
							logsParent.Factory.ChildFactories.Add(eDocument.ParentMain.Factory);
							break;
						}
					}
				}
			}
		}

		#endregion
	}
}
