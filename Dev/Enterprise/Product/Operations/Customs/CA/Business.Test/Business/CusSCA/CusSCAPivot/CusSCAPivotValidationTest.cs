using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusSCAPivotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCusSCAPivot()
		{
			CusSCAPivot parent = Factory.New<CusSCAPivot>();
			AssertEquals(parent.Validation.PackLine, parent);
		}

		public void TestCV_AssociatedContainerValidation()
		{
			container.CN_ContainerMode = Core.Constants.ContainerModes.Empty;
			packLine.CV_AssociatedContainer = "OCLU3213214";
			AssertHasWarning(packLine.CV_AssociatedContainerInfo, CusSCAPivotValidation.PackLineAssociatedWithAnEmptyContainer);
			container.CN_ContainerMode = Core.Constants.ContainerModes.FCL;
			packLine.CV_AssociatedContainer = ZString.Empty;
			packLine.CV_AssociatedContainer = "OCLU3213214";
			AssertNoMessageErrors(packLine.CV_AssociatedContainerInfo);
		}

		public void TestCV_PackageCountValidation()
		{
			packLine.CV_PackageCount = new ZInt(0);
			packLine.Validation.ValidateCV_PackageCount();
			AssertHasMessageErrorContaining(packLine.CV_PackageCountInfo, MandatoryValidation.YouHaveNotEntered);
			packLine.CV_PackageCount = new ZInt(1);
			AssertNoMessageErrors(packLine.CV_PackageCountInfo);
			packLine.CV_PackageCount = new ZInt(10000000);
			AssertHasMessageErrorContaining(packLine.CV_PackageCountInfo, CusSCAPivotValidation.MaximumPackageCount);
		}

		public void TestCV_PackageTypeValidation()
		{
			packLine.CV_PackageCount = new ZInt(1);
			packLine.CV_PackageType = ZString.Empty;
			packLine.Validation.ValidateCV_PackageType();
			AssertHasMessageErrorContaining(packLine.CV_PackageTypeInfo, MandatoryValidation.YouHaveNotEntered);
			packLine.CV_PackageType = "???";
			AssertHasMessageErrorContaining(packLine.CV_PackageTypeInfo, ListValidation.InvalidCodeMessageError);
			packLine.CV_PackageType = "PLT";
			AssertNoMessageErrors(packLine.CV_PackageTypeInfo);
		}

		public void TestCV_WeightValidation()
		{
			packLine.CV_PackageCount = new ZInt(1);
			packLine.CV_Weight = 0m;
			packLine.Validation.ValidateCV_Weight();
			AssertHasMessageErrorContaining(packLine.CV_WeightInfo, MandatoryValidation.YouHaveNotEntered);
			packLine.CV_Weight = 1m;
			AssertNoMessageErrors(packLine.CV_WeightInfo);
		}

		public void TestCV_WeightUQValidation()
		{
			packLine.CV_Weight = 1m;
			packLine.CV_WeightUQ = ZString.Empty;
			packLine.Validation.ValidateCV_WeightUQ();
			AssertHasMessageErrorContaining(packLine.CV_WeightUQInfo, MandatoryValidation.YouHaveNotEntered);
			packLine.CV_WeightUQ = "??";
			AssertHasMessageErrorContaining(packLine.CV_WeightUQInfo, ListValidation.InvalidCodeMessageError);
			packLine.CV_WeightUQ = "KG";
			AssertNoMessageErrors(packLine.CV_WeightUQInfo);
		}

		public void TestCV_VolumeUQValidation()
		{
			packLine.CV_Volume = 0m;
			packLine.CV_VolumeUQ = ZString.Empty;
			packLine.Validation.ValidateCV_VolumeUQ();
			AssertNoMessageErrors(packLine.CV_VolumeUQInfo);
			packLine.CV_Volume = 1m;
			packLine.CV_VolumeUQ = ZString.Empty;
			packLine.Validation.ValidateCV_VolumeUQ();
			AssertHasMessageErrorContaining(packLine.CV_VolumeUQInfo, MandatoryValidation.YouHaveNotEntered);
			packLine.CV_VolumeUQ = "??";
			AssertHasMessageErrorContaining(packLine.CV_VolumeUQInfo, ListValidation.InvalidCodeMessageError);
			packLine.CV_VolumeUQ = "M3";
			AssertNoMessageErrors(packLine.CV_VolumeUQInfo);
		}

		public void TestCV_GoodsDescriptionValidation()
		{
			packLine.CV_PackageCount = new ZInt(0);
			packLine.CV_GoodsDescription = ZString.Empty;
			packLine.Validation.ValidateCV_GoodsDescription();
			AssertNoMessageErrors(packLine.CV_GoodsDescriptionInfo);
			packLine.CV_PackageCount = new ZInt(1);
			AssertHasMessageErrorContaining(packLine.CV_GoodsDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			packLine.CV_GoodsDescription = "XXXX";
			AssertNoMessageErrors(packLine.CV_GoodsDescriptionInfo);
		}

		public void TestCheckCV_HarmonisedTariffNums()
		{
			packLine.CV_HarmonisedTariffNums = "1111111111,22,33,44,55,66";
			AssertHasMessageErrorContaining(packLine.CV_HarmonisedTariffNumsInfo, CusSCAPivotValidation.TariffWarning);
			packLine.CV_HarmonisedTariffNums = "1111111111,22,33,44,55";
			AssertNoMessageErrorContaining(packLine.CV_HarmonisedTariffNumsInfo, CusSCAPivotValidation.TariffWarning);
			packLine.CV_HarmonisedTariffNums = ZString.Empty;
			packLine.Validation.ValidateCV_HarmonisedTariffNums();
			AssertNoMessageErrorContaining(packLine.CV_HarmonisedTariffNumsInfo, CusSCAPivotValidation.TariffWarning);
			AssertHasWarningContaining(packLine.CV_HarmonisedTariffNumsInfo, CusSCAPivotValidation.TariffWarning);
			packLine.CV_HarmonisedTariffNums = "5";
			AssertHasMessageErrorContaining(packLine.CV_HarmonisedTariffNumsInfo, CusSCAPivotValidation.TariffWarning);
			packLine.CV_HarmonisedTariffNums = "1234567890";
			AssertNoMessageErrorContaining(packLine.CV_HarmonisedTariffNumsInfo, CusSCAPivotValidation.TariffWarning);
			AssertNoWarningContaining(packLine.CV_HarmonisedTariffNumsInfo, CusSCAPivotValidation.TariffWarning);
			packLine.CV_HarmonisedTariffNums = "1234.56.78 90";
			AssertNoMessageErrorContaining(packLine.CV_HarmonisedTariffNumsInfo, CusSCAPivotValidation.TariffWarning);
			packLine.CV_HarmonisedTariffNums = "12.34.56.78.90.1";
			AssertHasMessageErrorContaining(packLine.CV_HarmonisedTariffNumsInfo, CusSCAPivotValidation.TariffWarning);
		}

		CusSCAPivot packLine;
		CusSCAContainer container;
		protected override void SetUp()
		{
			base.SetUp();
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			container = oceanBill.Containers.AddNew();
			container.CN_ContainerNumber = "OCLU3213214";
			container.CN_ContainerMode = Enterprise.Core.Constants.ContainerModes.FCL;
			CusSCAHouse house = oceanBill.HouseBills.AddNew();
			packLine = house.PackLines.AddNew();
		}
	}
}
