using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.SystemMerge.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.DataTransfer.SystemMerge.Xml
{
	public class SysMergeEdocsXmlValueObjectSerializer : XmlValueObjectSerializer
	{
		public SysMergeEdocsXmlValueObjectSerializer(Type valueObjectType)
			: base(valueObjectType)
		{
		}

		protected override BusinessObject CreateOrUpdateFromValueObject(IValueObjectDataAdapter dataAdapter, IBusinessObjectCollection collection, IValueObject valueObject, IValueObjectImportContext context)
		{
			BusinessObject topLevelBizObj;
			try
			{
				topLevelBizObj = dataAdapter.CreateOrUpdateFromValueObject(valueObject, context);
				SaveIfNotInTheDatabaseAndNotify(topLevelBizObj, context);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				topLevelBizObj = null;
				context.Notify(new ErrorNotification(ErrorType.Error, ex.Message));
			}

			return topLevelBizObj;
		}

		/// <summary>
		/// Only Save Factory and Notify if the Top Level Business Object is a new imported one
		/// (as opposed to an existing one, which has been loaded and skipped from import)
		/// </summary>
		void SaveIfNotInTheDatabaseAndNotify(BusinessObject topLevelBizObj, IValueObjectImportContext context)
		{
			if (!topLevelBizObj.IsInDatabase)
			{
				try
				{
					SaveAndNotify(topLevelBizObj, context);
					context.Notify(SysMergeValueObjectHelper.GetImportedSuccessfullyNotification(GetDocumentDisplayInfo(topLevelBizObj)));
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					string message = String.Format("{0}\r\n\r\n{1}", GetDocumentDisplayInfo(topLevelBizObj), ex.Message);
					throw new Exception(message, ex);
				}
			}
		}

		/// <summary>
		/// Saves and add a create business object notification in order to increment the count of imported top level objects.
		/// </summary>
		void SaveAndNotify(BusinessObject topLevelBizObj, IValueObjectImportContext context)
		{
			BusinessObjectCreatedOrUpdatedNotification createdBizObjNotification = new BusinessObjectCreatedOrUpdatedNotification(topLevelBizObj);
			DataTransferTransactionCoordinator.SaveFactory(((NumberedBusinessObjectFactory)topLevelBizObj.Factory).MasterFactory);
			context.Notify(createdBizObjNotification);
		}

		string GetDocumentDisplayInfo(BusinessObject topLevelBizObj)
		{
			StorageDocsForDataTransfer doc = (StorageDocsForDataTransfer)topLevelBizObj;
			string result = Res.GetString("024a03d3-c7c9-4303-9d35-880aebdd6a9b", "Document: [({0}) - {1} - {2}]",
				doc.PK.ToString(), doc.SC_Desc, doc.SC_FileName);
			return result;
		}
	}
}
