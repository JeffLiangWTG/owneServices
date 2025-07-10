using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration;
using Enterprise.DocumentScanning.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentScanning.Services
{
	public interface IEDocsService
	{
		EDocUpdateResult AddOrUpdateEDoc(string tablePrefix, Guid businessObjectPk, EDocDetail eDocDetail, byte[] contents, bool includeUnpublished, ZGuid? contactPK = null);
		void DeleteEDoc(Guid eDocPK, int databaseNumber, bool includeUnpublished, ZGuid? contactPK = null);
		void DeliverEDoc(Guid eDocPK, int databaseNumber, DeliveryInstructionsBase deliveryInstructions, bool includeUnpublished, ZGuid? contactPK = null);
		int GetEDocCount(Guid entityPK, ZGuid? contactPK = null);
		EDocDetail[] GetEDocDetails(Guid parentPK, bool includeDeleted, bool includeUnpublished, ZGuid? contactPK = null);
		EDocImageData GetEDocImageData(Guid eDocPK, int databaseNumber, bool includeUnpublished, ZGuid? contactPK = null);
		string GetReferenceType(string entityDocManagerCode);
		IEnumerable<RefDocType> GetRefDocTypes(string referenceType, bool includeUnpublished, ZGuid? contactPK = null);
		bool IsFileAcceptable(string fileName);
		bool IsFileAcceptable(Stream fileData, out string apparentFileType);
	}
}
