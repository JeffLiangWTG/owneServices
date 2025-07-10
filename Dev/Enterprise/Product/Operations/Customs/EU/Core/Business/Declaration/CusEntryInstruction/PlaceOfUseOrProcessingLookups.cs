using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class PlaceOfUseOrProcessingLookups : CusGoodsLocationLookups
	{
		public PlaceOfUseOrProcessingLookups(PlaceOfUseOrProcessing parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList QualifierList
		{
			get
			{
				var result = new CusGoodsLocationQualifierList();
				result.RemoveCode(CusGoodsLocationQualifierList.Codes.PostcodeAddress);
				result.RemoveCode(CusGoodsLocationQualifierList.Codes.GnssCoordinates);

				return result;
			}
		}
	}
}
