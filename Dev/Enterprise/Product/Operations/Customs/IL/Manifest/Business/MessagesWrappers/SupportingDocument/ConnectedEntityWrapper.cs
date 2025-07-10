using System.Linq;
using CargoWise.Customs.IL.MessageDefinitions.DOC.REQ_271;
using CargoWise.Types;
using Enterprise.Customs.IL.Business.CodeDescriptionPairLists;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.IL.Business.Constants;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class ConnectedEntityWrapper : IConnectedEntity
	{
		ConnectedEntityWrapper(AsycudaBill bill, AsycudaManifestHeader header)
		{
			this.bill = bill;
			this.header = header;
		}

		internal static IConnectedEntity NewOrNull(SupportingDocument supportingDocument)
			=> supportingDocument != null && supportingDocument.Parent is AsycudaBill bill && bill.Header is AsycudaManifestHeader header
			? new ConnectedEntityWrapper(bill, header)
			: null;

		int IConnectedEntity.EntityType => int.Parse(CustomsDocumentRelatedEntity.ManifestType);
		
		string IConnectedEntity.EntityIdKey1 => header.AMA_ManifestNumber;

		string IConnectedEntity.EntityIdKey2 =>
			(string)header.AMA_TransportMode switch
			{
				TransportModes.Sea => bill.TransportDocuments.FirstOrDefault(td => td.CSI_Code == TransportDocsTypeList.Codes.IL1)?.CSI_ReferenceNumber ?? ZString.Empty,
				_ => ZString.Empty,
			};

		string IConnectedEntity.EntityIdKey3 => null;

		string IConnectedEntity.EntityIdExternalReferenceId => null;

		string IConnectedEntity.EntityPath => CustomsDocumentRelatedEntity.ManifestPath;

		readonly AsycudaBill bill;
		readonly AsycudaManifestHeader header;
	}
}
