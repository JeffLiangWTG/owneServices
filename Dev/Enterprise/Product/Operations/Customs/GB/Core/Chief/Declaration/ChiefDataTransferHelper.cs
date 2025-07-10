using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.GB.Chief.Declaration
{
	public class ChiefDataTransferHelper : DataTransferHelper
	{
		protected override ZString GetLocationAtClearanceInfoForWritingUXMLCore(JobDeclaration declaration)
		{
			return declaration.JE_LocationOfGoods;
		}

		protected override ZString? GetLocationOfGoodsFromLocationAtClearanceForReadingUXMLCore(CodeDescriptionPair35Char locationAtClearance)
		{
			return locationAtClearance?.Code?.PadRight(JobDeclaration.Schema.JE_CHIEF_GoodsLocationMaxLength).Left(JobDeclaration.Schema.JE_CHIEF_GoodsLocationMaxLength);
		}
	}
}
