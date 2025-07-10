using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Common.AU.CMR.Testing
{
	class CMRContainerUtilitiesTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetCMRContainerSize()
		{
			RefContainer container = RefContainer.New(Factory);
			container.RC_Length = 20;
			container.RC_Width = 8;
			container.RC_Height = 8;

			ZString size = utilities.GetContainerSizeCode(container);
			NUnit.Framework.Assert.That(size, Is.EqualTo(CMRContainerSizes.Codes._20X8X8).Using(CustomComparers.TypeComparison), "for a 20x8x8");

			container.RC_Height = 9.5m;
			size = utilities.GetContainerSizeCode(container);
			NUnit.Framework.Assert.That(size, Is.EqualTo(CMRContainerSizes.Codes._20X8X95).Using(CustomComparers.TypeComparison), "for a 20x8x9.5");

			container.RC_Height = 8;
			container.RC_Length = 40;
			size = utilities.GetContainerSizeCode(container);
			NUnit.Framework.Assert.That(size, Is.EqualTo(CMRContainerSizes.Codes._40X8X8).Using(CustomComparers.TypeComparison), "for a 40x8x8");

			container.RC_Height = 8.6m;
			container.RC_Length = 20;
			size = utilities.GetContainerSizeCode(container);
			NUnit.Framework.Assert.That(size, Is.EqualTo(CMRContainerSizes.Codes._20X8X8).Using(CustomComparers.TypeComparison), "for a 20x8x8.6");

			container.RC_Height = 9.6m;
			container.RC_Length = 40;
			size = utilities.GetContainerSizeCode(container);
			NUnit.Framework.Assert.That(size, Is.EqualTo(CMRContainerSizes.Codes.Other).Using(CustomComparers.TypeComparison), "for a 40x8x9.6");

			container.RC_Height = 9.6m;
			container.RC_Length = 45;
			size = utilities.GetContainerSizeCode(container);
			NUnit.Framework.Assert.That(size, Is.EqualTo(CMRContainerSizes.Codes.Other).Using(CustomComparers.TypeComparison), "for a 45x8x9.6");
		}

		[ExpectNoExceptions]
		public void TestGetCMRContainerTypeCode()
		{
			TestGetCMRContainerTypeCode("DRY", true, CMRContainerTypes.Codes.GeneralPurposeVentedAVentilatedContainerUsedToTransportCargo);
			TestGetCMRContainerTypeCode("DRY", false, CMRContainerTypes.Codes.GeneralPurposeNonVentedANonVentilatedContainerUsedToTransportCargo);
			TestGetCMRContainerTypeCode("RFG", false, CMRContainerTypes.Codes.RefrigeratedAContainerWhichIsUsedToTransportRefrigeratedCargo);
			TestGetCMRContainerTypeCode("TOP", false, CMRContainerTypes.Codes.OpenTopAContainerWithNoHardTopUsedToTransportCargoThatWouldNotNormallyFitInsideAConventionalContainer);
			TestGetCMRContainerTypeCode("FLT", false, CMRContainerTypes.Codes.FlatRackCargoSecuredOntoAFlatBaseForEaseOfLoadingAndDischarge);
			TestGetCMRContainerTypeCode("TNK", false, CMRContainerTypes.Codes.TankATypeOfVesselUsedToTransportLiquidCargo);
			TestGetCMRContainerTypeCode("MAF", false, CMRContainerTypes.Codes.MafiATypeOfWheeledTrailerOntoWhichCargoIsStrappedForTransportOnAVessel);
			TestGetCMRContainerTypeCode("FOO", false, ZString.Empty);
		}

		[ExpectNoExceptions]
		public void TestGetContainerTypeMappingOrOriginal()
		{
			NUnit.Framework.Assert.That(utilities.GetContainerTypeMappingOrOriginal("MAF"), Is.EqualTo("MAFI").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(utilities.GetContainerTypeMappingOrOriginal("FOO"), Is.EqualTo("FOO").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGetContainerTypeMappingCodeOrEmpty()
		{
			NUnit.Framework.Assert.That(utilities.GetContainerTypeMappingCodeOrEmpty("MAFI"), Is.EqualTo("MAF").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(utilities.GetContainerTypeMappingCodeOrEmpty("FOO"), Is.EqualTo(ZString.Empty));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			utilities = new CMRContainerUtilities();
		}
		CMRContainerUtilities utilities;

		[ExpectNoExceptions]
		protected void TestGetCMRContainerTypeCode(ZString enterpriseContainerType, bool hasVents, ZString expectedCMRContainerType)
		{
			RefContainer container = Factory.New<RefContainer>();
			container.RC_ContainerType = enterpriseContainerType;
			container.RC_HasVents = hasVents;
			NUnit.Framework.Assert.That(utilities.GetContainerTypeCode(container), Is.EqualTo(expectedCMRContainerType), "Container Type");
		}

		#endregion
	}
}
