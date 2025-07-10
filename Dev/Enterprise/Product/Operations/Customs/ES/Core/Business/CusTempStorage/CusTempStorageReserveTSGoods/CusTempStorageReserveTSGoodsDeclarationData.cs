using System.Linq;
using CargoWise.Types;
using static Enterprise.Customs.EU.Business.TemporaryStorageHelper;

namespace Enterprise.Customs.ES.Business.CusTempStorage;

public class CusTempStorageReserveTSGoodsDeclarationData
{
	public CusTempStorageReserveTSGoodsDeclarationData(DeclarationDataToReserveTSGoods dataToReserveTSGoods, bool isLame, bool useCSI_ItemNumber)
	{
		declarationData = dataToReserveTSGoods;
		GoodsItemNumber = isLame ? 1 : useCSI_ItemNumber ? dataToReserveTSGoods.Document.CSI_ItemNumber : dataToReserveTSGoods.Document.CSI_LineNo;
	}

	public static CusTempStorageReserveTSGoodsDeclarationData New(DeclarationDataToReserveTSGoods dataToReserveTSGoods, bool isLame, bool useCSI_ItemNumber) => new(dataToReserveTSGoods, isLame, useCSI_ItemNumber);

	readonly DeclarationDataToReserveTSGoods declarationData;
	public ZInt TotalPackages => totalPackages ??= declarationData.Packages.Sum(p => p.qty);
	int? totalPackages;
	public ZDecimal TotalGrossWeight => declarationData.TotalGrossWeight;
	public ZInt GoodsItemNumber;
}
