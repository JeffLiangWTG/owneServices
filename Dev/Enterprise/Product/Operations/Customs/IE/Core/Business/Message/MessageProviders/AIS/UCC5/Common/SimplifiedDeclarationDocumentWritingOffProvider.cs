using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	class SimplifiedDeclarationDocumentWritingOffProvider : ISimplifiedDeclarationDocumentWritingOff
	{
		public SimplifiedDeclarationDocumentWritingOffProvider(CusSupportingInfo cusSupportingInfo)
		{
			this.cusSupportingInfo = cusSupportingInfo;
		}

		public string PreviousDocumentType => cusSupportingInfo.CSI_Code;

		public string PreviousDocumentIdentifier => cusSupportingInfo.CSI_ReferenceNumber;

		public string PreviousDocumentLineId => cusSupportingInfo.CSI_LineNo.ToString();

		readonly CusSupportingInfo cusSupportingInfo;
	}
}
