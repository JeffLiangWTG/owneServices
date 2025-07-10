using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class ProducedDocumentsProvider : IIdType
	{
		public static ProducedDocumentsProvider New(SupportingDocument supportingDocument) => new ProducedDocumentsProvider(supportingDocument.CSI_Code, supportingDocument.CSI_ReferenceNumber);

		ProducedDocumentsProvider(string type, string id)
		{
			Type = type;
			Id = id;
		}

		public string Type { get; }

		public string Id { get; }
}
}
