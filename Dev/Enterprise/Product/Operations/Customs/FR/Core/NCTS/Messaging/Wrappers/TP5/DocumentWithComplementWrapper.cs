using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class DocumentWithComplementWrapper : IDocumentWithComplement
	{
		DocumentWithComplementWrapper(CusSupportingInfo supportingInfo)
		{
			this.supportingInfo = Argument.NotNull(supportingInfo, nameof(supportingInfo));
		}

		readonly CusSupportingInfo supportingInfo;

		public static DocumentWithComplementWrapper New(CusSupportingInfo supportingInfo) => supportingInfo == null ? null : new DocumentWithComplementWrapper(supportingInfo);

		public string ComplementOfInformation => complementOfInformation ?? (complementOfInformation = supportingInfo.CSI_ReferenceNumber2);
		string complementOfInformation;

		public string Type => type ?? (type = supportingInfo.CSI_Code);
		string type;

		public string ReferenceNumber => referenceNumber ?? (referenceNumber = supportingInfo.CSI_ReferenceNumber);
		string referenceNumber;
	}
}
