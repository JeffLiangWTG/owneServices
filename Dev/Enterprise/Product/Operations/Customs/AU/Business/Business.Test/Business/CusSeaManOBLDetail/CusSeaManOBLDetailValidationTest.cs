using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CusSeaManOBLDetailValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateBD_LineCargoType()
		{
			CusSeaManOBLDetail detail = GetNewDetail();

			detail.BD_LineCargoType = ZString.Empty;
			AssertHasMessageErrors("has message errors on empty", detail.BD_LineCargoTypeInfo);

			detail.BD_LineCargoType = "~~~";
			AssertHasMessageErrors("has errors when value not in list", detail.BD_LineCargoTypeInfo);

			detail.BD_LineCargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
			AssertNoMessageErrors("has no message errors with valid value", detail.BD_LineCargoTypeInfo);
			AssertNoErrors("has no errors with valid value", detail.BD_LineCargoTypeInfo);
		}

		public void TestValidateBD_ContainerNumber()
		{
			CusSeaManOBLDetail detail = GetNewDetail();

			detail.BD_ContainerNumber = ZString.Empty;

			detail.BD_LineCargoType = CMRImportCargoTypes.Codes.BreakBulk;
			AssertNoMessageErrors("should have no message errors when empty if cargo type is Break Bulk", detail.BD_ContainerNumberInfo);

			detail.BD_LineCargoType = CMRImportCargoTypes.Codes.Bulk;
			AssertNoMessageErrors("should have no message errors when empty if cargo type is Bulk", detail.BD_ContainerNumberInfo);

			detail.BD_LineCargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
			AssertHasMessageErrors("should have a message error when empty if cargo type is not breakbulk or bulk", detail.BD_ContainerNumberInfo);

			detail.BD_ContainerNumber = "ABC";
			AssertNoMessageErrors("should have no message errors when not empty", detail.BD_ContainerNumberInfo);
			AssertNoErrors("should have no errors when not empty", detail.BD_ContainerNumberInfo);
			AssertHasWarnings("has warnings when invalid container number", detail.BD_ContainerNumberInfo);

			detail.BD_ContainerNumber = "ASDF1234560";
			AssertNoWarnings("has no warnings when valid container number", detail.BD_ContainerNumberInfo);

			detail.BD_ContainerNumber = "0123456789012345678";
			AssertHasMessageErrors("should have message errors when over 17 characters", detail.BD_ContainerNumberInfo);
		}

		public void TestValidateBD_ContainerNumberMultipleBillsFCL()
		{
			const string SharedContainerNumber = "FCRU0039400";
			CusSeaManTranHead header = Factory.New<CusSeaManTranHead>();
			CusSeaManOBLHeader oBLHeader1 = header.OceanBills.AddNew();
			CusSeaManOBLDetail detail1 = oBLHeader1.Details.AddNew();

			detail1.BD_LineCargoType = Enterprise.Core.Constants.ContainerModes.FCL;
			CusSeaManOBLHeader oBLHeader2 = header.OceanBills.AddNew();
			CusSeaManOBLDetail detail2 = oBLHeader2.Details.AddNew();
			detail2.BD_LineCargoType = Enterprise.Core.Constants.ContainerModes.FCL;
			detail1.BD_ContainerNumber = SharedContainerNumber;
			detail2.BD_ContainerNumber = SharedContainerNumber;
			AssertHasMessageError(detail2.BD_ContainerNumberInfo, CusSeaManOBLDetailValidation.MultipleOceanBillsInFCLContainerMessage);
		}

		public void TestValidateBD_GoodsDescription()
		{
			CusSeaManOBLDetail detail = GetNewDetail();

			detail.BD_GoodsDescription = ZString.Empty;
			AssertHasMessageErrors("when empty", detail.BD_GoodsDescriptionInfo);

			detail.BD_GoodsDescription = "A";
			AssertHasMessageErrors("when less than two characters long", detail.BD_GoodsDescriptionInfo);

			detail.BD_GoodsDescription = "12345";
			AssertHasMessageErrors("when composed entirely of numbers", detail.BD_GoodsDescriptionInfo);

			detail.BD_GoodsDescription = "!!!!!";
			AssertHasMessageErrors("when composed entirely of special characters", detail.BD_GoodsDescriptionInfo);

			detail.BD_GoodsDescription = "Blah Blah";
			AssertNoMessageErrors(detail.BD_GoodsDescriptionInfo);
		}

		public void TestValidateBD_MarksAndNumbers()
		{
			CusSeaManOBLDetail detail = GetNewDetail();

			detail.BD_LineCargoType = CMRImportCargoTypes.Codes.LessThanContainerLoad;
			AssertHasMessageErrors("when cargo type is LCL and empty", detail.BD_MarksAndNumbersInfo);

			detail.BD_LineCargoType = CMRImportCargoTypes.Codes.BreakBulk;
			AssertHasMessageErrors("when cargo type is BreakBulk and empty", detail.BD_MarksAndNumbersInfo);

			detail.BD_LineCargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
			AssertNoMessageErrors("when cargo type is FullContainerLoad and empty", detail.BD_MarksAndNumbersInfo);

			detail.BD_MarksAndNumbers = "Blah Blah";

			detail.BD_LineCargoType = CMRImportCargoTypes.Codes.LessThanContainerLoad;
			AssertNoMessageErrors("when cargo type is LCL and not empty", detail.BD_MarksAndNumbersInfo);

			detail.BD_LineCargoType = CMRImportCargoTypes.Codes.BreakBulk;
			AssertNoMessageErrors("when cargo type is BreakBulk and not empty", detail.BD_MarksAndNumbersInfo);

			detail.BD_LineCargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
			AssertNoMessageErrors("when cargo type is FullContainerLoad and not empty", detail.BD_MarksAndNumbersInfo);
		}

		public void TestValidateBD_NoOfPacks()
		{
			CusSeaManOBLDetail detail = GetNewDetail();

			detail.Validation.ValidateBD_NoOfPacks();
			AssertHasMessageErrors("by default", detail.BD_NoOfPacksInfo);

			detail.BD_LineCargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
			AssertHasMessageErrors("when cargo type is not Bulk", detail.BD_NoOfPacksInfo);

			detail.BD_LineCargoType = CMRImportCargoTypes.Codes.Bulk;
			AssertNoMessageErrors("when cargo type is Bulk", detail.BD_NoOfPacksInfo);

			detail.BD_NoOfPacks = -1;
			AssertHasMessageErrors("when less than one", detail.BD_NoOfPacksInfo);

			detail.BD_NoOfPacks = 12345678;
			AssertHasMessageErrors("when over 7 digits", detail.BD_NoOfPacksInfo);
		}

		public void TestValidateBD_PackType()
		{
			CusSeaManOBLDetail detail = GetNewDetail();

			detail.Validation.ValidateBD_PackType();
			AssertHasMessageErrors("by default", detail.BD_PackTypeInfo);

			detail.BD_LineCargoType = CMRImportCargoTypes.Codes.Bulk;
			AssertNoMessageErrors("when cargo type is Bulk", detail.BD_PackTypeInfo);

			detail.BD_LineCargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
			AssertHasMessageErrors("when cargo type is not Bulk", detail.BD_PackTypeInfo);

			detail.BD_PackType = "~~";
			AssertHasWarnings("when value not in list", detail.BD_PackTypeInfo);

			detail.BD_PackType = CMRPackageTypes.Codes.Keg;
			AssertNoMessageErrors("when value in list", detail.BD_PackTypeInfo);
			AssertNoErrors("when value in list", detail.BD_PackTypeInfo);
		}

		public void TestValidateBD_TypeOfContainer()
		{
			CusSeaManOBLDetail detail = GetNewDetail();

			detail.Validation.ValidateBD_TypeOfContainer();
			AssertHasMessageErrors("by default", detail.BD_TypeOfContainerInfo);

			detail.BD_TypeOfContainer = CMRContainerTypes.Codes.RefrigeratedAContainerWhichIsUsedToTransportRefrigeratedCargo;
			AssertNoNotifications("when set", detail.BD_TypeOfContainerInfo);

			detail.BD_TypeOfContainer = "BLA";
			AssertHasMessageErrors("when invalid", detail.BD_TypeOfContainerInfo);

			detail.BD_LineCargoType = CMRImportCargoTypes.Codes.Bulk;
			detail.BD_TypeOfContainer = ZString.Empty;
			AssertNoNotifications("when cargo type is bulk", detail.BD_TypeOfContainerInfo);

			detail.BD_LineCargoType = CMRImportCargoTypes.Codes.BreakBulk;
			detail.Validation.ValidateBD_TypeOfContainer();
			AssertNoNotifications("when cargo type is break bulk", detail.BD_TypeOfContainerInfo);
		}

		public void TestValidateBD_ContainerSizeOrISOCode()
		{
			CusSeaManOBLDetail detail = GetNewDetail();

			detail.Validation.ValidateBD_ContainerSizeOrISOCode();
			AssertHasMessageErrors("by default", detail.BD_ContainerSizeOrISOCodeInfo);

			detail.BD_ContainerSizeOrISOCode = CMRContainerSizes.Codes._20X8X8;
			AssertNoNotifications("when set", detail.BD_ContainerSizeOrISOCodeInfo);

			detail.BD_ContainerSizeOrISOCode = "4321";
			AssertHasMessageErrors("when invalid", detail.BD_ContainerSizeOrISOCodeInfo);

			detail.BD_LineCargoType = CMRImportCargoTypes.Codes.Bulk;
			detail.BD_ContainerSizeOrISOCode = ZString.Empty;
			AssertNoNotifications("when cargo type is bulk", detail.BD_ContainerSizeOrISOCodeInfo);

			detail.BD_ContainerSizeOrISOCode = CMRImportCargoTypes.Codes.BreakBulk;
			detail.Validation.ValidateBD_ContainerSizeOrISOCode();
			AssertNoNotifications("when cargo type is break bulk", detail.BD_ContainerSizeOrISOCodeInfo);
		}

		public void TestValidateBD_GrossWeight()
		{
			CusSeaManOBLDetail detail = GetNewDetail();

			detail.BD_GrossWeight = 0;
			AssertHasMessageErrors("when 0", detail.BD_GrossWeightInfo);

			detail.BD_GrossWeight = -1;
			AssertHasMessageErrors("when less than 0", detail.BD_GrossWeightInfo);

			detail.BD_GrossWeight = 1;
			AssertNoMessageErrors("when valid", detail.BD_GrossWeightInfo);
		}

		public void TestValidateBD_GrossWeightUM()
		{
			CusSeaManOBLDetail detail = GetNewDetail();

			detail.Validation.ValidateBD_GrossWeightUM();
			AssertNoMessageErrors("by default", detail.BD_GrossWeightUMInfo);

			detail.BD_GrossWeightUM = "~~";
			AssertHasMessageErrors("when code not in list", detail.BD_GrossWeightUMInfo);

			detail.BD_GrossWeightUM = CMRGrossWeightCodes.Codes.Kilograms;
			AssertNoMessageErrors("when code in list", detail.BD_GrossWeightUMInfo);
			AssertNoErrors("when code in list", detail.BD_GrossWeightUMInfo);
		}

		public void TestValidateBD_CargoVolume()
		{
			CusSeaManOBLDetail detail = GetNewDetail();

			detail.BD_CargoVolume = 0;
			AssertHasMessageErrors("when 0", detail.BD_CargoVolumeInfo);

			detail.BD_CargoVolume = -1;
			AssertHasMessageErrors("when less than 0", detail.BD_CargoVolumeInfo);

			detail.BD_CargoVolume = 1;
			AssertNoMessageErrors("when valid", detail.BD_CargoVolumeInfo);
		}

		public void TestValidateBD_CargoVolumeUM()
		{
			CusSeaManOBLDetail detail = GetNewDetail();

			detail.Validation.ValidateBD_CargoVolumeUM();
			AssertNoMessageErrors("by default", detail.BD_CargoVolumeUMInfo);

			detail.BD_CargoVolumeUM = "~~";
			AssertHasMessageErrors("when code not in list", detail.BD_CargoVolumeUMInfo);

			detail.BD_CargoVolumeUM = CMRQuantityUnits.Codes.SuperFeet;
			AssertNoMessageErrors("when code in list", detail.BD_CargoVolumeUMInfo);
			AssertNoErrors("when code in list", detail.BD_CargoVolumeUMInfo);
		}

		public void TestValidateBD_SACIndicator()
		{
			CusSeaManOBLDetail detail = GetNewDetail();

			detail.Validation.ValidateBD_SACIndicator();
			AssertNoMessageErrors("by deault", detail.BD_SACIndicatorInfo);

			detail.BD_SACIndicator = true;

			detail.Header.BO_FreightForwarderIndicator = false;
			AssertNoNotifications("when header's freight forwarder indicator is not set and SAC true", detail.BD_SACIndicatorInfo);

			detail.Header.BO_FreightForwarderIndicator = true;
			AssertHasMessageErrors("when header's freight forwarder indicator is set and SAC true", detail.BD_SACIndicatorInfo);

			detail.BD_SACIndicator = false;

			detail.Header.BO_FreightForwarderIndicator = false;
			AssertNoNotifications("when header's freight forwarder indicator is not set and SAC false", detail.BD_SACIndicatorInfo);

			detail.Header.BO_FreightForwarderIndicator = true;
			AssertNoNotifications("when header's freight forwarder indicator is set and SAC false", detail.BD_SACIndicatorInfo);
		}

		public void TestOnlyOneBulkLine()
		{
			CusSeaManOBLHeader header = Factory.New<CusSeaManOBLHeader>();
			CusSeaManOBLDetail detail = header.Details.AddNew();
			CusSeaManOBLDetail detail2 = header.Details.AddNew();

			detail.BD_LineCargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
			detail2.BD_LineCargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
			AssertNoNotifications("on d1 when fcl", detail.BD_LineCargoTypeInfo);
			AssertNoNotifications("on d2 when fcl", detail2.BD_LineCargoTypeInfo);

			detail.BD_LineCargoType = CMRImportCargoTypes.Codes.Bulk;
			AssertNoNotifications("on d1 when d1 bulk", detail.BD_LineCargoTypeInfo);
			AssertNoNotifications("on d2 when d1 bulk", detail2.BD_LineCargoTypeInfo);

			detail2.BD_LineCargoType = CMRImportCargoTypes.Codes.Bulk;
			detail.Validation.ValidateBD_LineCargoType();
			AssertHasMessageErrors("on d1 when d1 & d2 bulk", detail.BD_LineCargoTypeInfo);
			AssertHasMessageErrors("on d2 when d1 & d2 bulk", detail2.BD_LineCargoTypeInfo);

			detail.BD_LineCargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
			detail2.Validation.ValidateBD_LineCargoType();
			AssertNoNotifications("on d1 when d1 fcl", detail.BD_LineCargoTypeInfo);
			AssertNoNotifications("on d2 when d1 fcl", detail2.BD_LineCargoTypeInfo);
		}

		public void TestOnlyOneBreakBulkLine()
		{
			CusSeaManOBLHeader header = Factory.New<CusSeaManOBLHeader>();
			CusSeaManOBLDetail detail = header.Details.AddNew();
			CusSeaManOBLDetail detail2 = header.Details.AddNew();

			detail.BD_LineCargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
			detail2.BD_LineCargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
			AssertNoNotifications("on d1 when fcl", detail.BD_LineCargoTypeInfo);
			AssertNoNotifications("on d2 when fcl", detail2.BD_LineCargoTypeInfo);

			detail.BD_LineCargoType = CMRImportCargoTypes.Codes.BreakBulk;
			AssertNoNotifications("on d1 when d1 breakbulk", detail.BD_LineCargoTypeInfo);
			AssertNoNotifications("on d2 when d1 breakbulk", detail2.BD_LineCargoTypeInfo);

			detail2.BD_LineCargoType = CMRImportCargoTypes.Codes.BreakBulk;
			detail.Validation.ValidateBD_LineCargoType();
			AssertHasMessageErrors("on d1 when d1 & d2 breakbulk", detail.BD_LineCargoTypeInfo);
			AssertHasMessageErrors("on d2 when d1 & d2 breakbulk", detail2.BD_LineCargoTypeInfo);

			detail.BD_LineCargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
			detail2.Validation.ValidateBD_LineCargoType();
			AssertNoNotifications("on d1 when d1 fcl", detail.BD_LineCargoTypeInfo);
			AssertNoNotifications("on d2 when d1 fcl", detail2.BD_LineCargoTypeInfo);
		}

		#region Implementation

		CusSeaManOBLDetail GetNewDetail()
		{
			CusSeaManOBLHeader header = Factory.New<CusSeaManOBLHeader>();
			return header.Details.AddNew();
		}

		#endregion
	}
}
