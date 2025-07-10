using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public abstract class DataTransferHelper
	{
		public ZString GetLocationAtClearanceInfoForWritingUXML(JobDeclaration declaration) => GetLocationAtClearanceInfoForWritingUXMLCore(declaration);
		protected abstract ZString GetLocationAtClearanceInfoForWritingUXMLCore(JobDeclaration declaration);

		public ZString? GetLocationOtherInformationFromLocationAtClearanceForReadingUXML(CodeDescriptionPair35Char locationAtClearance) => GetLocationOtherInformationFromLocationAtClearanceForReadingUXMLCore(locationAtClearance);
		protected virtual ZString? GetLocationOtherInformationFromLocationAtClearanceForReadingUXMLCore(CodeDescriptionPair35Char locationAtClearance) => null;

		public ZString? GetLocationQualifierFromLocationAtClearanceForReadingUXML(CodeDescriptionPair35Char locationAtClearance) => GetLocationQualifierFromLocationAtClearanceForReadingUXMLCore(locationAtClearance);
		protected virtual ZString? GetLocationQualifierFromLocationAtClearanceForReadingUXMLCore(CodeDescriptionPair35Char locationAtClearance) => null;

		public ZString? GetLocationOfGoodsFromLocationAtClearanceForReadingUXML(CodeDescriptionPair35Char locationAtClearance) => GetLocationOfGoodsFromLocationAtClearanceForReadingUXMLCore(locationAtClearance);
		protected virtual ZString? GetLocationOfGoodsFromLocationAtClearanceForReadingUXMLCore(CodeDescriptionPair35Char locationAtClearance) => null;
	}
}
