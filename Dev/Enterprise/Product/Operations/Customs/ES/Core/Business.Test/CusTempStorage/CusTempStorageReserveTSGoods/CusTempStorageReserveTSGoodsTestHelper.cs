using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing;
public static class CusTempStorageReserveTSGoodsTestHelper
{
	public static void SetUniversalReferencePackageTypesTestData(BusinessObjectFactory factory)
	{
		var helper = new UniversalReferenceTestDataHelper(factory);
		_ = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "UN Package Types");
		_ = helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"VQ", "VQ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
		_ = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"AA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
	}

	public static CusTempStorageReserveTSGoodsRegHeader GetCusTempStorageReserveTSGoodsRegHeader(BusinessObjectFactory factory, bool isBulk = false)
	{
		var regHeader = factory.New<CusTempStorageRegHeader>();
		_ = CreateLine(regHeader, 1, "OPN", isBulk);
		_ = CreateLine(regHeader, 2, "OPN", isBulk);
		_ = CreateLine(regHeader, 3, "CLS", isBulk);
		var declarationData = CreateDeclarationData(factory, 200, 1000m, isBulk);
		return CusTempStorageReserveTSGoodsRegHeader.New(factory, regHeader, declarationData);
	}

	public static CusTempStorageReserveTSGoodsRegLine GetCusTempStorageReserveTSGoodsRegLine(BusinessObjectFactory factory)
	{
		var regHeader = factory.New<CusTempStorageRegHeader>();
		regHeader.SRH_Reference = "REF";
		var line = CreateLine(regHeader, 1);
		var declarationData = CreateDeclarationData(factory, 200, 1000m);
		var header = CusTempStorageReserveTSGoodsRegHeader.New(factory, regHeader, declarationData);
		return CusTempStorageReserveTSGoodsRegLine.New(line, header.TotalPackages, header.TotalGrossWeight);
	}

	static CusTempStorageRegLine CreateLine(CusTempStorageRegHeader regHeader, int lineNo, string customsStatus = "OPN", bool isBulk = false)
	{
		var factory = regHeader.Factory;
		var line = regHeader.CusTempStorageRegLines.AddNew();
		line.SRL_LineNumber = lineNo;
		line.SRL_CustomsStatus = customsStatus;
		line.SRL_LocationOfGoods = "Location 1";
		line.SRL_OwnerReference = "Owner Ref";
		line.SRL_PackageType = isBulk ? "VQ" : "AA";
		var item = CreateLineItem(factory, 1);
		_ = CreateLinePivot(line, item.PK, 500m);
		_ = CreateTransaction(factory, line.PK, 100, 500m, "OBL");
		return line;
	}

	static EU.TemporaryStorage.Business.CusTempStorageRegLineItem CreateLineItem(BusinessObjectFactory factory, ZInt goodsIemNumber)
	{
		var item = factory.New<EU.TemporaryStorage.Business.CusTempStorageRegLineItem>();
		item.SRI_GoodsItemNumber = goodsIemNumber;
		return item;
	}

	static CusTempStorageRegLineItemPivot CreateLinePivot(CusTempStorageRegLine line, ZGuid itemPK, ZDecimal grossWeight)
	{
		var pivot = line.RegLineItemPivots.AddNew();
		pivot.SRV_SRL_Line = line.PK;
		pivot.SRV_SRI_Item = itemPK;
		pivot.SRV_GrossWeight = grossWeight;
		return pivot as CusTempStorageRegLineItemPivot;
	}

	static CusTempStorageRegLineTransaction CreateTransaction(BusinessObjectFactory factory, ZGuid linePK, ZInt packageQuantity, ZDecimal grossWeight, string transactionType)
	{
		var transaction = factory.New<CusTempStorageRegLineTransaction>();
		transaction.SRT_SRL = linePK;
		transaction.SRT_PackageQty = packageQuantity;
		transaction.SRT_GrossWeight = grossWeight;
		transaction.SRT_TransactionType = transactionType;
		return transaction;
	}

	static CusTempStorageReserveTSGoodsDeclarationData CreateDeclarationData(BusinessObjectFactory factory, int packageQuantity, decimal grossWeight, bool isBulk = false, bool isLame = false, bool useCSI_ItemNumber = false)
	{
		var document = factory.New<TemporaryStoragePreviousDocument>();
		document.CSI_LineNo = 1;

		var packages = new List<(ZString type, ZInt qty, ZString marksOrVin, ZBool isBulk)>
		{
			("KG", packageQuantity, "Marks", isBulk)
		};

		var data = new EU.Business.TemporaryStorageHelper.DeclarationDataToReserveTSGoods()
		{
			Document = document,
			TotalGrossWeight = grossWeight,
			Packages = packages,
		};

		return CusTempStorageReserveTSGoodsDeclarationData.New(data, isLame, useCSI_ItemNumber);
	}
}
