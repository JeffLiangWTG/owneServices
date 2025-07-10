using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Dash.Integration;
using Enterprise.Dash.Integration.Services;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Dash.Business.Services
{
	public sealed class DashEDocsService(IShipamaxService shipamaxService) : IDashEDocsService
	{
		readonly IShipamaxService shipamaxService = shipamaxService;

		public DashEDocsDetails GetDashEDocsDetails(ZGuid docMainId, ZGuid docId, ZString docToken)
		{
			var referencedStorageMain = MasterFactory.Load<StorageMain>(new ZGuid(docMainId)) ?? throw new DashException($"Can't find StorageMain record with ID: {docMainId}");

			try
			{
				var changes = shipamaxService.CheckEDocsChanges(docId.ToGuid(), docToken);
				var documentChanges = GetDocumentChangesText(changes);

				var shipamaxMessage = GetEDocsShipamaxMessage(docId) ?? throw new DashException("Can't find related EDocsShipamaxMessage");

				return new DashEDocsDetails
				{
					DocId = docId.ToGuid(),
					DocMainId = docMainId.ToGuid(),
					RelatedEntityId = referencedStorageMain.SM_ParentFK.ToGuid(),
					RelatedEntityTypeCode = referencedStorageMain.SM_Type,
					RelatedBranchId = shipamaxMessage.Branch.PK.ToGuid(),
					RelatedDepartmentId = shipamaxMessage.Department.PK.ToGuid(),
					DocumentChanges = documentChanges
				};
			}
			catch (ShipamaxServiceException ex)
			{
				throw new DashException(ex.Message);
			}
		}

		List<string> GetDocumentChangesText(IEnumerable<ShipamaxEDocsChange> changes)
		{
			var result = new List<string>();

			foreach (var change in changes)
			{
				result.Add($"Field: {change.Field}, Old Value: {change.OldValue}, New Value: {change.NewValue}");
			}

			return result;
		}

		EDocsShipamaxMessage GetEDocsShipamaxMessage(ZGuid eDocId)
		{
			var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, eDocId);
			query.AddToFilter(EDIMessageSchema.EM_IsActive, true);
			var shipamaxMessage = MasterFactory.LoadTop1<EDocsShipamaxMessage>(query);

			return shipamaxMessage;
		}

		#region Master Factory

		DocumentFactory MasterFactory => masterFactory ??= new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());

		DocumentFactory masterFactory;

		#endregion
	}
}
