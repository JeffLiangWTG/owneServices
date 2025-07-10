using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage
{
	public class DocumentProvider : IDocument
	{
		public static DocumentProvider New(CusSupportingInfo supportingInfo, int sequenceNumber)
		{
			DocumentProvider result = null;
			if (supportingInfo != null)
			{
				result = new DocumentProvider(supportingInfo.CSI_Code, supportingInfo.CSI_ReferenceNumber, sequenceNumber.ToString());
			}
			return result;
		}

		protected DocumentProvider(string type, string referenceNumber, string sequenceNumber)
		{
			Type = type;
			Reference = referenceNumber;
			SequenceNumber = sequenceNumber;
		}

		public string Type { get; }

		public string Reference { get; }

		public string SequenceNumber { get; }
	}
}
