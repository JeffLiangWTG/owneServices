using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public static class AsycudaTransportDocumentInfoCollectionExtensions
	{
		public static void EnsureTransportDocumentType(this IAsycudaTransportDocumentInfoCollection collection, ZString documentType, ZString documentNumber)
		{
			var transportDocument = collection.Cast<AsycudaTransportDocumentInfo>().FirstOrDefault(s => s.CSI_Code == documentType);

			if (documentNumber.IsEmpty)
			{
				if (transportDocument is not null)
				{
					collection.RemoveAndDelete(transportDocument);
				}
				return;
			}

			if (transportDocument is null)
			{
				transportDocument = collection.AddNew();
				transportDocument.CSI_Code = documentType;
			}

			transportDocument.CSI_ReferenceNumber = documentNumber;
		}
	}
}
