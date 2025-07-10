using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage
{
	public class ProducedDocumentsWritingOffProvider : IIdType
	{
		public static ProducedDocumentsWritingOffProvider New(CusSupportingInfo supportingInfo)
		{
			ProducedDocumentsWritingOffProvider result = null;
			if (supportingInfo != null)
			{
				result = new ProducedDocumentsWritingOffProvider(supportingInfo.CSI_Code, supportingInfo.CSI_ReferenceNumber);
			}
			return result;
		}

		protected ProducedDocumentsWritingOffProvider(string type, string referenceNumber)
		{
			Type = type;
			Id = referenceNumber;
		}

		public string Type { get; }

		public string Id { get; }
	}
}
