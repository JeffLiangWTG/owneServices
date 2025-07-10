using System;
using CargoWise.EntityFramework;
using Enterprise.Dash.Integration;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Dash.Business
{
	public class DashParseRequestService : IDashParseRequestService
	{
		readonly BusinessObjectFactory factory;

		public DashParseRequestService(BusinessObjectFactory businessObjectFactory)
		{
			this.factory = businessObjectFactory;
		}

		public IParseRequest Create(IeDoc storageDoc)
		{
			var newMessage = factory.New<EDocsShipamaxMessage>();
			newMessage.EM_LinkUniqueID = storageDoc.UniqueKey;
			newMessage.EM_LinkTable = AutoStorageDocs.Schema.TableName;
			newMessage.EM_ApplicationReference = storageDoc.ParentMain.PK.ToString();

			return newMessage;
		}

		public bool Exists(Guid storageDocPk)
		{
			var query = new ZDBOnlyQuery(typeof(EDocsShipamaxMessage))
						.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, storageDocPk)
						.AddToFilter(EDIMessageSchema.EM_IsActive, true);

			var exists = factory.ExistsInDatabase(AutoEDIMessage.Schema.TableName, query);

			return exists;
		}

		public IParseRequest Get(Guid storageDocPk)
		{
			var query = new ZDBOnlyQuery(typeof(EDocsShipamaxMessage))
						.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, storageDocPk)
						.AddToFilter(EDIMessageSchema.EM_IsActive, true);
			query.ReLoadExistingRows = true;
			var parseRequest = factory.LoadTop1<EDocsShipamaxMessage>(query);

			return parseRequest;
		}

		public void Cancel(IParseRequest parseRequest)
		{
			var edocsShipamaxMessage = (EDocsShipamaxMessage)parseRequest;
			edocsShipamaxMessage.EM_IsActive = false;
		}
	}
}
