using CargoWise.Types;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.FR.Messaging.Interfaces.CIN;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.CIN
{
	public class CINLineWrapper : ICINLine
	{
		public CINLineWrapper(CusTempStorageLine line)
		{
			this.line = line;
		}
		public ZGuid PK => line.PK;

		public ZString Type => line.TSL_OwnerReferenceType;

		public ZString ReferenceNumber => line.TSL_OwnerReferenceNumber;

		public ZInt NoPieces => line.TSL_PackageQty;
		public ZInt PreviousNoPieces => (ZInt)line.TSL_PackageQtyInfo.OriginalValue;

		public ZInt TotalNoPieces => line.TSL_PackageQty;

		public ZDecimal Mass => ConvertedWeight;
		public ZDecimal PreviousMass => ConvertedPreviousWeight;

		public ZDecimal TotalMass => ConvertedWeight;

		ZDecimal ConvertedWeight => Core.Constants.Weight.Convert(line.TSL_GrossWeight, line.TSL_GrossWeightUQ, Core.Constants.Weight.Kilograms);
		ZDecimal ConvertedPreviousWeight => Core.Constants.Weight.Convert((ZDecimal)line.TSL_GrossWeightInfo.OriginalValue, (ZString)line.TSL_GrossWeightUQInfo.OriginalValue, Core.Constants.Weight.Kilograms);

		public ZString DescriptionOfGoods => line.TSL_GoodsDescription;

		public bool IsAirwayBill => Type == OwnerReferenceTypeList.Codes.ORT_AWB;

		readonly CusTempStorageLine line;
	}
}
