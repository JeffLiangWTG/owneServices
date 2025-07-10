using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class DocumentProvider : CusSupportingInfoProvider, IDocument
	{
		public DocumentProvider(CusSupportingInfo cusSupportingInfo) : base(cusSupportingInfo) { }

		public string Type => cusSupportingInfo.CSI_Code;

		public string Reference => cusSupportingInfo.CSI_ReferenceNumber;
	}
}
