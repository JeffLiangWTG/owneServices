using CargoWise.Common;
using Enterprise.Customs.EU.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	[CodeAlive("This wrapper will be used later")]
	public class CustomsOfficeWrapper : CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces.ICustomsOffice
	{
		CustomsOfficeWrapper(CusGoodsLocation goodsLocation)
		{
			this.goodsLocation = Argument.NotNull(goodsLocation, nameof(goodsLocation));
		}
		readonly CusGoodsLocation goodsLocation;

		public string ReferenceNumber => referenceNumber ?? (referenceNumber = goodsLocation.CGL_CustomsOffice);
		string referenceNumber;

		public static CustomsOfficeWrapper New(CusGoodsLocation goodsLocation) => goodsLocation == null ? null : new CustomsOfficeWrapper(goodsLocation);
	}
}
