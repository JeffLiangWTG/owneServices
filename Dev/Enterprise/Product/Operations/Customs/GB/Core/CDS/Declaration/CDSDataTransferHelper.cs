using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.GB.CDS.Declaration
{
	public class CDSDataTransferHelper : DataTransferHelper
	{
		protected override ZString GetLocationAtClearanceInfoForWritingUXMLCore(JobDeclaration declaration)
		{
			return declaration.JE_LocationOtherInformation;
		}

		protected override ZString? GetLocationOtherInformationFromLocationAtClearanceForReadingUXMLCore(CodeDescriptionPair35Char locationAtClearance)
		{
			return locationAtClearance?.Code?.SubstringSafe(0, JobDeclaration.Schema.JE_Calc_LocationOtherInformationCountryMaxLength + JobDeclaration.Schema.JE_Calc_LocationOtherInformationTypeMaxLength + JobDeclaration.Schema.JE_LocationQualifierMaxLength + JobDeclaration.Schema.JE_GoodsLocationMaxLength);
		}

		protected override ZString? GetLocationQualifierFromLocationAtClearanceForReadingUXMLCore(CodeDescriptionPair35Char locationAtClearance)
		{
			return locationAtClearance?.Code?.SubstringSafe(JobDeclaration.Schema.JE_Calc_LocationOtherInformationCountryMaxLength + JobDeclaration.Schema.JE_Calc_LocationOtherInformationTypeMaxLength, JobDeclaration.Schema.JE_LocationQualifierMaxLength);
		}
	}
}
