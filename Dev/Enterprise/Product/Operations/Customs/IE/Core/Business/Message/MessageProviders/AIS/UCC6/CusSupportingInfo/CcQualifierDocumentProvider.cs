using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class CcQualifierDocumentProvider : CusSupportingInfoProvider, ICcQualifierDocument
	{
		public CcQualifierDocumentProvider(CusSupportingInfo cusSupportingInfo) : base(cusSupportingInfo) { }

		public string Type => cusSupportingInfo.CSI_Code;

		public string Reference => cusSupportingInfo.CSI_ReferenceNumber;

		public string CcQualifier => null;
	}
}
