using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	public abstract class WarehouseGenericLineWrapperTest : GenericWrapperTest
	{
		#region Test Properties

		#region TestHoldCode

		public void TestHoldCode()
		{
			TestHoldCodeCore();
		}

		protected virtual void TestHoldCodeCore()
		{
			AssertEquals(ZString.Empty, WarehouseGenericLineWrapper.HoldCode);
		}

		#endregion

		#region TestHoldReason

		public void TestHoldReason()
		{
			TestHoldReasonCore();
		}

		protected virtual void TestHoldReasonCore()
		{
			AssertEquals(ZString.Empty, WarehouseGenericLineWrapper.HoldReason);
		}

		#endregion

		#region TestVariance

		public void TestVariance()
		{
			TestVarianceCore();
		}

		protected virtual void TestVarianceCore()
		{
			AssertEquals(ZDecimal.Zero, WarehouseGenericLineWrapper.Variance);
		}

		#endregion

		#region TestPartAttribute2Name

		public void TestPartAttribute2Name()
		{
			TestPartAttribute2NameCore();
		}

		protected virtual void TestPartAttribute2NameCore()
		{
			AssertEquals(ZString.Empty, WarehouseGenericLineWrapper.PartAttribute2Name);
		}

		#endregion

		#region TestPartAttribute3Name

		public void TestPartAttribute3Name()
		{
			TestPartAttribute3NameCore();
		}

		protected virtual void TestPartAttribute3NameCore()
		{
			AssertEquals(ZString.Empty, WarehouseGenericLineWrapper.PartAttribute3Name);
		}

		#endregion

		#region TestIsEmptyLocation

		public void TestIsEmptyLocation()
		{
			TestIsEmptyLocationCore();
		}

		protected virtual void TestIsEmptyLocationCore()
		{
			AssertEquals(ZBool.False, WarehouseGenericLineWrapper.IsEmptyLocation);
		}

		#endregion

		#region TestIsLocationEmptyAfterFinalisingPick

		public void TestIsLocationEmptyAfterFinalisingPick()
		{
			TestIsLocationEmptyAfterFinalisingPickCore();
		}

		protected virtual void TestIsLocationEmptyAfterFinalisingPickCore()
		{
			AssertEquals(ZBool.False, WarehouseGenericLineWrapper.IsLocationEmptyAfterPickFinalisation);
		}

		#endregion

		#region TestExtendedLinePriceForTotal

		public void TestExtendedLinePriceForTotal()
		{
			TestExtendedLinePriceForTotalCore();
		}

		protected virtual void TestExtendedLinePriceForTotalCore()
		{
			AssertEquals(ZDecimal.Zero, WarehouseGenericLineWrapper.ExtendedLinePriceForTotal);
		}

		#endregion

		#region TestNoOfAttribsAndAttributePrintSizeFactor

		#region TestNoOfAttribsAndAttributePrintSizeFactor

		public void TestNoOfAttribsAndAttributePrintSizeFactor()
		{
			var docWrapper = WarehouseGenericLineWrapper;
			var expectedNoOfAttribs = 0;
			var maxExpectedAttribsAndDG = 7;

			AssertEquals("Precondition", 0, docWrapper.NoOfAttribs);
			AssertEquals("Precondition", maxExpectedAttribsAndDG, docWrapper.AttributePrintSizeFactor);

			SetAttribute1();
			expectedNoOfAttribs += CanSetProductAttributesInWrapper ? 1 : 0;
			AssertEquals(expectedNoOfAttribs, docWrapper.NoOfAttribs);
			AssertEquals(maxExpectedAttribsAndDG - expectedNoOfAttribs, docWrapper.AttributePrintSizeFactor);

			SetAttribute2();
			expectedNoOfAttribs += CanSetProductAttributesInWrapper ? 1 : 0;
			AssertEquals(expectedNoOfAttribs, docWrapper.NoOfAttribs);
			AssertEquals(maxExpectedAttribsAndDG - expectedNoOfAttribs, docWrapper.AttributePrintSizeFactor);

			SetAttribute3();
			expectedNoOfAttribs += CanSetProductAttributesInWrapper ? 1 : 0;
			AssertEquals(expectedNoOfAttribs, docWrapper.NoOfAttribs);
			AssertEquals(maxExpectedAttribsAndDG - expectedNoOfAttribs, docWrapper.AttributePrintSizeFactor);

			SetExpiryDate();
			expectedNoOfAttribs += CanSetProductAttributesInWrapper ? 1 : 0;
			AssertEquals(expectedNoOfAttribs, docWrapper.NoOfAttribs);
			AssertEquals(maxExpectedAttribsAndDG - expectedNoOfAttribs, docWrapper.AttributePrintSizeFactor);

			SetPackingDate();
			expectedNoOfAttribs += CanSetProductAttributesInWrapper ? 1 : 0;
			AssertEquals(expectedNoOfAttribs, docWrapper.NoOfAttribs);
			AssertEquals(maxExpectedAttribsAndDG - expectedNoOfAttribs, docWrapper.AttributePrintSizeFactor);

			SetSerialNumber();
			expectedNoOfAttribs += CanSetProductAttributesInWrapper ? 1 : 0;
			AssertEquals(expectedNoOfAttribs, docWrapper.NoOfAttribs);
			AssertEquals(maxExpectedAttribsAndDG - expectedNoOfAttribs, docWrapper.AttributePrintSizeFactor);

			var docWrapperDG = SetProductDG();
			AssertEquals(expectedNoOfAttribs, docWrapperDG.NoOfAttribs); // DG does not include in NoOfAttribs
			expectedNoOfAttribs += CanSetProductAttributesInWrapper ? 1 : 0;
			AssertEquals(CanSetProductAttributesInWrapper, docWrapperDG.HasDG);
			AssertEquals(maxExpectedAttribsAndDG - expectedNoOfAttribs, docWrapperDG.AttributePrintSizeFactor);
		}

		bool CanSetProductAttributesInWrapper => CanSetProductAttributesInWrapperCore;

		protected virtual bool CanSetProductAttributesInWrapperCore => true;

		void SetAttribute1() => SetAttribute1Core();

		protected virtual void SetAttribute1Core()
		{
		}

		void SetAttribute2() => SetAttribute2Core();

		protected virtual void SetAttribute2Core()
		{
		}

		void SetAttribute3() => SetAttribute3Core();

		protected virtual void SetAttribute3Core()
		{
		}

		void SetExpiryDate() => SetExpiryDateCore();

		protected virtual void SetExpiryDateCore()
		{
		}

		void SetPackingDate() => SetPackingDateCore();

		protected virtual void SetPackingDateCore()
		{
		}

		void SetSerialNumber() => SetSerialNumberCore();

		protected virtual void SetSerialNumberCore()
		{
		}

		WarehouseGenericLineWrapper SetProductDG() => SetProductDGCore();

		protected virtual WarehouseGenericLineWrapper SetProductDGCore() => WarehouseGenericLineWrapper;

		#endregion

		#endregion

		#endregion

		#region Implementation

		#region TestWrapperMappingsEmpty

		public override void TestWrapperMappingsEmpty()
		{
			// Please add in alphabetical order
			var emptyWrapper = GetNewWarehouseLineWrapper(null);
			CombineAssertions(() =>
			{
				AssertEquals("AdditionalMoneys", "", emptyWrapper.AdditionalMoneys);
				AssertEquals("AttributeUnits", "", emptyWrapper.AttributeUnits);
				AssertEquals("ArrivalDate", ZDateTime.Empty, emptyWrapper.ArrivalDate);
				AssertEquals("CrossDockConsigneeAddress", "", emptyWrapper.CrossDockConsigneeAddress.CompanyName);
				AssertEquals("CrossDockOrderNumber", "", emptyWrapper.CrossDockOrderNumber);
				AssertEquals("CustomsAddInfo", "", emptyWrapper.CustomsAddInfo);
				AssertEquals("CustomsCtryOfOrigin", "", emptyWrapper.CustomsCtryOfOrigin);
				AssertEquals("CustomsEntryDate", ZDateTime.Empty, emptyWrapper.CustomsEntryDate);
				AssertEquals("CustomsEntryKey", "", emptyWrapper.CustomsEntryKey);
				AssertEquals("CustomsEntryLineNo", 0, (int)emptyWrapper.CustomsEntryLineNo);
				AssertEquals("CustomsEntryNo", "", emptyWrapper.CustomsEntryNo);
				AssertEquals("CustomsTariffItem", "", emptyWrapper.CustomsTariffItem);
				AssertEquals("CustomsTILV", 0m, emptyWrapper.CustomsTILV);
				AssertEquals("CustomsVFD", 0m, emptyWrapper.CustomsVFD);
				AssertEquals("CustomsQuantity", 0m, emptyWrapper.CustomsQuantity);
				AssertEquals("CustomsQuantityUQ", "", emptyWrapper.CustomsQuantityUQ);
				AssertEquals("CustomsSecondQuantity", 0m, emptyWrapper.CustomsSecondQuantity);
				AssertEquals("CustomsSecondUnitQty", "", emptyWrapper.CustomsSecondUnitQty);
				AssertEquals("CustomsThirdQuantity", 0m, emptyWrapper.CustomsThirdQuantity);
				AssertEquals("CustomsThirdUnitQty", "", emptyWrapper.CustomsThirdUnitQty);
				AssertEquals("ManufacturerAddress", "", emptyWrapper.ManufacturerAddress.CompanyName);
				AssertNull("DangerousGoodsSubstance", emptyWrapper.DangerousGoodsSubstance);
				AssertEquals("ExpiryDate", ZDateTime.Empty, emptyWrapper.ExpiryDate);
				AssertEquals("ExpiryDateWithLabel", "", emptyWrapper.ExpiryDateWithLabel);
				AssertNull("ExtendedLinePrice", emptyWrapper.ExtendedLinePrice);
				AssertEquals("LineComment", "", emptyWrapper.LineComment);
				AssertEquals("LineNo", "00000", emptyWrapper.LineNo);
				AssertEquals("LocationsString", "", emptyWrapper.LocationString);
				AssertEquals("LocationsString2", "", emptyWrapper.LocationString2);
				AssertEquals("PackingDate", ZDateTime.Empty, emptyWrapper.PackingDate);
				AssertEquals("PackingDateWithLabel", "", emptyWrapper.PackingDateWithLabel);
				AssertEquals("Packs", 0m, emptyWrapper.Packs);
				AssertEquals("PacksUQ", "", emptyWrapper.PacksUQ);
				AssertEquals("PalletID", "", emptyWrapper.PalletID);
				AssertEquals("PalletID2", "", emptyWrapper.PalletID2);
				AssertEquals("PartAttribute1", "", emptyWrapper.PartAttribute1);
				AssertEquals("PartAttribute1WithLabel", "", emptyWrapper.PartAttribute1WithLabel);
				AssertEquals("PartAttribute2", "", emptyWrapper.PartAttribute2);
				AssertEquals("PartAttribute2WithLabel", "", emptyWrapper.PartAttribute2WithLabel);
				AssertEquals("PartAttribute3", "", emptyWrapper.PartAttribute3);
				AssertEquals("PartAttribute3WithLabel", "", emptyWrapper.PartAttribute3WithLabel);
				AssertEquals("TrackedSerialNumber", "", emptyWrapper.TrackedSerialNumber);
				AssertEquals("TrackedSerialWithLabel", "", emptyWrapper.TrackedSerialWithLabel);
				AssertEquals("TrackedSerialBarcode", "", emptyWrapper.TrackedSerialBarcode);
				AssertEquals("PickGroup", "", emptyWrapper.PickGroup);
				AssertEquals("PickGroupNumber", ZShort.Zero, emptyWrapper.PickGroupNumber);
				AssertEquals("PickMethod", "", emptyWrapper.PickMethod);
				AssertEquals("PickPathSequence", ZInt.Zero, emptyWrapper.PickPathSequence);
				AssertEquals("PrimaryPreference", "", emptyWrapper.PrimaryPreference);
				AssertEquals("ProductCode", "", emptyWrapper.ProductCode);
				AssertEquals("ProductCodeBarcode", "", emptyWrapper.ProductCodeBarcode);
				AssertEquals("ProductDescription", "", emptyWrapper.ProductDescription);
				AssertEquals("ReasonCode", "", emptyWrapper.ReasonCode);
				AssertEquals("ReasonDescription", "", emptyWrapper.ReasonDescription);
				AssertEquals("RecommendedUnitPrice", true, emptyWrapper.RecommendedUnitPrice.IsEmpty);
				AssertEquals("RowPathSequence", ZShort.Zero, emptyWrapper.RowPathSequence);
				AssertEquals("Tariff", "", emptyWrapper.Tariff);
				AssertEquals("TransferFromWarehouseName", "", emptyWrapper.TransferFromWarehouseName);
				AssertEquals("TransferToWarehouseName", "", emptyWrapper.TransferToWarehouseName);
				AssertEquals("UnitDiscountAmount", true, emptyWrapper.UnitDiscountAmount.IsEmpty);
				AssertEquals("UnitDiscountPercent", true, emptyWrapper.UnitDiscountPercent.IsEmpty);
				AssertEquals("UnitPriceAfterDiscount", true, emptyWrapper.UnitPriceAfterDiscount.IsEmpty);
				AssertEquals("Units", 0m, emptyWrapper.Units);
				AssertEquals("UnitsGroupedPackQty", "", emptyWrapper.UnitsGroupedPackQty);
				AssertEquals("UnitsGroupedPackType", "", emptyWrapper.UnitsGroupedPackType);
				AssertEquals("UnitsGroupedQty", "", emptyWrapper.UnitsGroupedQty);
				AssertEquals("UnitsGroupedStockKeepingUnit", "", emptyWrapper.UnitsGroupedStockKeepingUnit);
				AssertEquals("UnitsGroupedUOMType", "", emptyWrapper.UnitsGroupedUOMType);
				AssertEquals("UnitsMet", true, emptyWrapper.UnitsMet.IsEmpty);
				AssertEquals("UnitsOrdered", true, emptyWrapper.UnitsOrdered.IsEmpty);
				AssertEquals("UnitsUQ", "", emptyWrapper.UnitsUQ);
				AssertEquals("CustomFields.Count", 0, emptyWrapper.CustomFields.Count);
				AssertEquals("NoOfAttribs", 0, emptyWrapper.NoOfAttribs);
				AssertEquals("AttributePrintSizeFactor", 7, emptyWrapper.AttributePrintSizeFactor);
				AssertEquals("HoldCode", "", emptyWrapper.HoldCode);
				AssertEquals("HoldReason", "", emptyWrapper.HoldReason);

				TestWrapperMappingsEmpty_NotDependentToBizOProperties(emptyWrapper);
			});
		}

		void TestWrapperMappingsEmpty_NotDependentToBizOProperties(WarehouseGenericLineWrapper emptyWrapper)
		{
			TestWrapperMappingsEmpty_DestLocationCaption(emptyWrapper);
			TestWrapperMappingsEmpty_DestPalletIDCaption(emptyWrapper);
			TestWrapperMappingsEmpty_DestWarehouseCaption(emptyWrapper);
			TestWrapperMappingsEmpty_FromLocationCaption(emptyWrapper);
			TestWrapperMappingsEmpty_FromPalletIDCaption(emptyWrapper);
			TestWrapperMappingsEmpty_FromWarehouseCaption(emptyWrapper);
		}

		protected virtual void TestWrapperMappingsEmpty_DestLocationCaption(WarehouseGenericLineWrapper emptyWrapper)
		{
			AssertEquals(nameof(emptyWrapper.DestLocationCaption), "", emptyWrapper.DestLocationCaption);
		}

		protected virtual void TestWrapperMappingsEmpty_DestPalletIDCaption(WarehouseGenericLineWrapper emptyWrapper)
		{
			AssertEquals(nameof(emptyWrapper.DestPalletIDCaption), "", emptyWrapper.DestPalletIDCaption);
		}

		protected virtual void TestWrapperMappingsEmpty_DestWarehouseCaption(WarehouseGenericLineWrapper emptyWrapper)
		{
			AssertEquals(nameof(emptyWrapper.DestWarehouseCaption), "", emptyWrapper.DestWarehouseCaption);
		}

		protected virtual void TestWrapperMappingsEmpty_FromLocationCaption(WarehouseGenericLineWrapper emptyWrapper)
		{
			AssertEquals(nameof(emptyWrapper.FromLocationCaption), "", emptyWrapper.FromLocationCaption);
		}

		protected virtual void TestWrapperMappingsEmpty_FromPalletIDCaption(WarehouseGenericLineWrapper emptyWrapper)
		{
			AssertEquals(nameof(emptyWrapper.FromPalletIDCaption), "", emptyWrapper.FromPalletIDCaption);
		}

		protected virtual void TestWrapperMappingsEmpty_FromWarehouseCaption(WarehouseGenericLineWrapper emptyWrapper)
		{
			AssertEquals(nameof(emptyWrapper.FromWarehouseCaption), "", emptyWrapper.FromWarehouseCaption);
		}

		#endregion

		#region ExpectedFieldMap

		protected sealed override string ExpectedFieldMap
		{
			get
			{
				return @"
WarehouseJobLine
======================================================================
Name                                    Type
----------------------------------------------------------------------
CrossDockConsigneeAddress               Address
ManufacturerAddress                     Address
RecommendedUnitPrice                    LabelValuePair
UnitDiscountAmount                      LabelValuePair
UnitDiscountPercent                     LabelValuePair
UnitPriceAfterDiscount                  LabelValuePair
UnitsMet                                LabelValuePair
UnitsOrdered                            LabelValuePair
UnitsPicked                             LabelValuePair
UnitsShort                              LabelValuePair
ExtendedLinePrice                       Money
Product                                 Product
DangerousGoodsSubstance                 UNDGSubstance
AdditionalMoneys                        String
ArrivalDate                             DateTime
AttributePrintSizeFactor                Int
Attributes                              MultilingualString
AttributeUnits                          String
BOMIndentation                          String
BOMLevel                                Int
ClientCode                              String
ClientName                              String
CrossDockOrderNumber                    String
CurrencySymbol                          String
CustomAttrib1                           String
CustomAttrib2                           String
CustomAttrib3                           String
CustomAttrib4                           String
CustomAttrib5                           String
CustomAttrib6                           String
CustomDate1                             DateTime
CustomDate2                             DateTime
CustomDate3                             DateTime
CustomDate4                             DateTime
CustomDate5                             DateTime
CustomDecimal1                          Decimal
CustomDecimal2                          Decimal
CustomDecimal3                          Decimal
CustomDecimal4                          Decimal
CustomDecimal5                          Decimal
CustomFlag1                             Bool
CustomFlag2                             Bool
CustomFlag3                             Bool
CustomFlag4                             Bool
CustomFlag5                             Bool
CustomsAddInfo                          String
CustomsCtryOfOrigin                     String
CustomsEntryDate                        DateTime
CustomsEntryKey                         String
CustomsEntryLineNo                      Short
CustomsEntryNo                          String
CustomsQuantity                         Decimal
CustomsQuantityUQ                       String
CustomsSecondQuantity                   Decimal
CustomsSecondUnitQty                    String
CustomsTariffItem                       String
CustomsThirdQuantity                    Decimal
CustomsThirdUnitQty                     String
CustomsTILV                             Decimal
CustomsVFD                              Decimal
CustomTextBlob1                         String
DestLocationCaption                     String
DestPalletIDCaption                     String
DestWarehouseCaption                    String
ExpectedReceiptQuantity                 Decimal
ExpiryDate                              DateTime
ExpiryDateBarcode                       String
ExpiryDateFormatted                     String
ExpiryDateWithLabel                     String
ExtraDetails                            String
FirstDate                               String
FromLocationCaption                     String
FromPalletIDCaption                     String
FromWarehouseCaption                    String
GroupedInventoryUnits                   Decimal
GroupedLineUnitsMet                     Decimal
GroupedReceivedWeight                   String
GroupedReceiveUnits                     Decimal
GroupedUnits                            Decimal
HasDG                                   Bool
HoldCode                                String
HoldReason                              String
InventoryQty                            String
InventoryStatus                         String
InventoryStatusDescription              String
IsEmptyLocation                         Bool
IsLocationEmptyAfterPickFinalisation    Bool
IsTopLevelOrEvenIndex                   String
IsTopLevelOrOddIndex                    String
LastCount                               Decimal
LeftOverAttributes                      MultilingualString
LineComment                             String
LineNo                                  String
LocationColumn                          Short
LocationLevel                           Short
LocationString                          String
LocationString2                         String
NoOfAttribs                             Int
PackingDate                             DateTime
PackingDateBarcode                      String
PackingDateFormatted                    String
PackingDateWithLabel                    String
PackQty                                 Decimal
Packs                                   Decimal
PacksUQ                                 String
PalletID                                String
PalletID2                               String
PalletIDBarcode                         String
Pallets                                 Decimal
PartAttribute1                          String
PartAttribute1Barcode                   String
PartAttribute1Name                      MultilingualString
PartAttribute1WithLabel                 MultilingualString
PartAttribute2                          String
PartAttribute2Barcode                   String
PartAttribute2Name                      MultilingualString
PartAttribute2WithLabel                 MultilingualString
PartAttribute3                          String
PartAttribute3Barcode                   String
PartAttribute3Name                      MultilingualString
PartAttribute3WithLabel                 MultilingualString
PickArea                                String
PickGroup                               String
PickGroupNumber                         Short
PickMethod                              String
PickPathSequence                        Int
PositionAfterSorting                    String
PrimaryPreference                       String
ProductBrandName                        String
ProductCode                             String
ProductCodeBarcode                      String
ProductCodeBarcodeNumber                String
ProductDescription                      String
ProductModel                            String
ReasonCode                              String
ReasonDescription                       String
ReceivedQty                             String
ReleaseUnitsAndUQ                       String
RFConfirm                               String
RFConfirmBarcode                        String
RFConfirmName                           String
RowPathSequence                         Short
SecondDate                              String
StagingAreaName                         String
Status                                  String
SubTotalUnits                           Decimal
SupplierProductDesc                     String
SystemUnits                             Decimal
Tariff                                  String
TrackedSerialBarcode                    String
TrackedSerialNumber                     String
TrackedSerialWithLabel                  String
TransferFromWarehouseName               MultilingualString
TransferToWarehouseName                 MultilingualString
Units                                   Decimal
UnitsGroupedPackQty                     String
UnitsGroupedPackType                    String
UnitsGroupedQty                         String
UnitsGroupedStockKeepingUnit            String
UnitsGroupedUOMType                     String
UnitsUQ                                 String
Variance                                Decimal
Volume                                  Decimal
VolumeUQ                                String
Weight                                  Decimal
WeightUQ                                String

CustomFields                            CustomField Collection
";
			}
		}

		#endregion

		#region Business Objects For Testing

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return WarehouseGenericLineWrapper;
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return WarehouseGenericLineWrapper;
		}

		protected WarehouseGenericLineWrapper WarehouseGenericLineWrapper
		{
			get { return warehouseGenericLineWrapper ?? (warehouseGenericLineWrapper = GetNewWarehouseLineWrapper(WhsLineBO)); }
		}
		WarehouseGenericLineWrapper warehouseGenericLineWrapper;

		protected BusinessObject WhsLineBO
		{
			get { return whsLineBO ?? (whsLineBO = GetNewBusinessObject()); }
		}
		BusinessObject whsLineBO;

		protected abstract BusinessObject GetNewBusinessObject();

		protected abstract WarehouseGenericLineWrapper GetNewWarehouseLineWrapper(BusinessObject bizO);

		#endregion

		protected WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}
		WhsTestHelperFunctions helper;

		#endregion
	}
}
