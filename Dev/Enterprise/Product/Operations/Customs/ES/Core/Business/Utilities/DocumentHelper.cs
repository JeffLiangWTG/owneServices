using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ES.Business
{
	public static class DocumentHelper
	{
		public static ZBool IsSameDocumentReference(CusSupportingInfo document, ZString requestReference) => document.CSI_ReferenceNumber == requestReference;

		public static ZString GetDocumentByTransportType(ZString transportType) => ESConstants.DocumentTypes.DocumentCodeByTransportMode.ContainsKey(transportType) ?
			ESConstants.DocumentTypes.DocumentCodeByTransportMode[transportType] : ZString.Empty;

		public static ZString GetDsdtMRNNumberFormat(ZString dsdtMRN) => string.Format("{0}{1}{2}", dsdtMRN.SubstringSafe(6, 4), dsdtMRN.SubstringSafe(1, 1), dsdtMRN.SubstringSafe(11, 6));
	}
}
