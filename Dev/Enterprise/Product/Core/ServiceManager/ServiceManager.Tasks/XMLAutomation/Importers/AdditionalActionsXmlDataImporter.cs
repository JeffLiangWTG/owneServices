using System.Collections.Generic;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public struct ImportedObject
	{
		public ImportedObject(ZGuid pK, bool isInDatabase)
		{
			this.PK = pK;
			this.IsInDatabase = isInDatabase;
		}

		public readonly bool IsInDatabase;
		public readonly ZGuid PK;
	}

	public abstract class AdditionalActionsXmlDataImporter : XmlDataImporter
	{
		public AdditionalActionsXmlDataImporter(IValueObjectDataAdapter adapter)
			: base(adapter)
		{
		}

		protected override bool ImportDataToFactoryCore(TextReader dataReader, string attachmentFileName, INotifications notifications, out ITransactionParticipant[] additionalTransactionActions)
		{
			NotificationBuffer buffer = new NotificationBuffer(notifications);
			ITransactionParticipant[] newAdditionalTransactionActions;
			bool result = GetImportDataResult(dataReader, attachmentFileName, buffer, out newAdditionalTransactionActions);

			List<ITransactionParticipant> transactions = new List<ITransactionParticipant>();
			if (newAdditionalTransactionActions != null && newAdditionalTransactionActions.Length > 0)
			{
				transactions.AddRange(newAdditionalTransactionActions);
			}
			if (!buffer.ContainsNotificationType(ErrorType.ImportingDataError))
			{
				ITransactionParticipant[] extraActions = GetExtraActions(buffer, ImportedObjects);
				if (extraActions != null && extraActions.Length > 0)
				{
					transactions.AddRange(extraActions);
				}
			}
			additionalTransactionActions = transactions.ToArray();
			ImportedObjects = null;
			return result;
		}

#if DEBUG
		protected virtual
#endif
		bool GetImportDataResult(TextReader dataReader, string attachmentFileName, NotificationBuffer buffer, out ITransactionParticipant[] newAdditionalTransactionActions)
		{
			return base.ImportDataToFactoryCore(dataReader, attachmentFileName, buffer, out newAdditionalTransactionActions);
		}

		ITransactionParticipant[] GetExtraActions(NotificationBuffer notifications, ImportedObject[] importedObjects)
		{
			ITransactionParticipant[] result = GetExtraActionsCore(notifications, importedObjects);
#if DEBUG
			ExtraActionsForTest = result;
#endif
			return result;
		}

		protected virtual ITransactionParticipant[] GetExtraActionsCore(NotificationBuffer notifications, ImportedObject[] importedObjects)
		{
			return System.Array.Empty<ITransactionParticipant>();
		}

		protected override void AfterImportXml(IBusinessObjectCollection bizObjsImported, INotifications notifications)
		{
			base.AfterImportXml(bizObjsImported, notifications);
			ImportedObjects = GetImportedObjectsFromCollection(bizObjsImported);
		}

		ImportedObject[] GetImportedObjectsFromCollection(IBusinessObjectCollection bizObjsImported)
		{
			List<ImportedObject> pairList = new List<ImportedObject>();
			foreach (BusinessObject bizObj in bizObjsImported)
			{
				pairList.Add(new ImportedObject(bizObj.PK, bizObj.IsInDatabase));
			}
			return pairList.ToArray();
		}

		ImportedObject[] ImportedObjects;
#if DEBUG
		internal ITransactionParticipant[] ExtraActionsForTest;
#endif
	}
}
