using CargoWise.Types;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using static Enterprise.Customs.GB.CDS.Constants;

namespace Enterprise.Customs.GB.CDS
{
	public class SpecificCircumstanceIndicatorCodeList : SpecificCircumstanceIndicatorList
	{
		public static string MapToSpecificCircumstanceCode(ZString specificCircumstance)
		{
			var result = string.Empty;
			switch (specificCircumstance)
			{
				case Codes.ExpressConsignmentsA20:
					result = SpecificCircumstanceCodes.A20;
					break;
				default:
					result = specificCircumstance;
					break;
			}

			return result;
		}
	}
}
