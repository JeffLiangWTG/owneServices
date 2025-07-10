using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ExportCustomsManifestLinesValidationTest : ExportCustomsManifestValidationAbstractTest
	{
		public void TestValidateEL_WeightUQ()
		{
			line.EL_WeightUQ = "XX";
			AssertHasError(line.EL_WeightUQInfo, "Enter a valid selection.");

			line.EL_WeightUQ = Core.Constants.Weight.Kilograms;
			AssertNoErrors(line.EL_WeightUQInfo);

			line.EL_WeightUQ = "";
			AssertHasError(line.EL_WeightUQInfo, "Please enter a value.");
		}

		public void TestValidateEL_VolumeUQ()
		{
			line.EL_VolumeUQ = "XX";
			AssertHasError(line.EL_VolumeUQInfo, "Enter a valid selection.");

			line.EL_VolumeUQ = Core.Constants.Volume.Litre;
			AssertNoErrors(line.EL_VolumeUQInfo);

			line.EL_VolumeUQ = "";
			AssertHasError(line.EL_VolumeUQInfo, "Please enter a value.");
		}

		public void TestValidateEL_TypeOfCAN()
		{
			line.EL_TypeOfCAN = CANType.CustomsAuthorityNumber.Code;
			Assert("No Notifications", !line.EL_TypeOfCANInfo.HasNotifications());
			line.EL_TypeOfCAN = CANType.Exemptions.EXLV.Code;
			Assert("No Notifications", !line.EL_TypeOfCANInfo.HasNotifications());
			line.EL_TypeOfCAN = "XXX";
			Assert("MessageError", line.EL_TypeOfCANInfo.HasMessageErrors());
		}

		public void TestValidateEL_AirWayBill()
		{
			SetupAirEMM();
			SetupCANLine();
			line.EL_AirWayBill = ZString.Empty;
			Assert("MessageError", line.EL_AirWayBillInfo.HasMessageErrors());
			line.EL_AirWayBill = "23352334236";
			Assert("NoMessageError", !line.EL_AirWayBillInfo.HasMessageErrors());
			line.EL_AirWayBill = "2335233423";
			Assert("MessageError", line.EL_AirWayBillInfo.HasMessageErrors());
			line.EL_AirWayBill = "23352334236";
			Assert("NoMessageError", !line.EL_AirWayBillInfo.HasMessageErrors());
			line.EL_AirWayBill = "23352334235";
			Assert("MessageError", line.EL_AirWayBillInfo.HasMessageErrors());
		}

		public void TestAirWayBillRequiredForAirCTOConsolidation()
		{
			ExportCustomsManifestLines line = SetupAirCTOESM();

			line.EL_AirWayBill = ZString.Empty;
			AssertHasMessageErrors(line.EL_AirWayBillInfo);

			line.EL_AirWayBill = "12121212122";
			AssertNoNotifications(line.EL_AirWayBillInfo);

			line.EL_AirWayBill = "12345678901234567890";
			AssertNoNotifications(line.EL_AirWayBillInfo);
		}

		public void TestValidateEL_CAN()
		{
			SetupAirESM();
			SetupCANLine();
			line.EL_CAN = ZString.Empty;
			Assert("MessageError", line.EL_CANInfo.HasMessageErrors());
			line.EL_CAN = "AAAAAAMP7";
			Assert("NoMessageError", !line.EL_CANInfo.HasMessageErrors());
			line.EL_CAN = "AAAA";
			Assert("MessageError", line.EL_CANInfo.HasMessageErrors());
			line.EL_CAN = "AAAAAAAAA";
			Assert("MessageError", line.EL_CANInfo.HasMessageErrors());
			line.EL_CAN = "ZZZZZZZZZ";
			Assert("MessageError", line.EL_CANInfo.HasMessageErrors());
			SetupLowValueExemptLine();
			line.EL_CAN = ZString.Empty;
			Assert("NoMessageError", !line.EL_CANInfo.HasMessageErrors());
			line.EL_CAN = "AAAA";
			Assert("MessageError", line.EL_CANInfo.HasMessageErrors());

			header.ED_TransportMode = Core.Constants.TransportModes.Air;
			header.ED_ManifestType = AirManifestTypeList.Codes.CtoReceivalRemovalStandAlone;
			line.Validation.ValidateAll();
			AssertNoNotifications(line.EL_CANInfo);
			line.EL_CAN = ZString.Empty;
			AssertHasMessageErrors(line.EL_CANInfo);
		}

		public void TestValidateEL_NumberOfPackages()
		{
			line.EL_NumberOfPackages = -1;
			Assert("MessageErrors", line.EL_NumberOfPackagesInfo.HasMessageErrors());
			SetupAirESM();
			line.EL_NumberOfPackages = 0;
			Assert("MessageErrors", line.EL_NumberOfPackagesInfo.HasMessageErrors());
			line.EL_NumberOfPackages = 1;
			Assert("NoNotifications", !line.EL_NumberOfPackagesInfo.HasNotifications());

			SetupSeaESM();
			line.EL_NumberOfContainers = 0;
			line.EL_NumberOfPackages = 0;
			Assert("MessageErrors", line.EL_NumberOfPackagesInfo.HasMessageErrors());
			line.EL_NumberOfContainers = 1;
			line.EL_NumberOfPackages = 0;
			Assert("NoNotifications", !line.EL_NumberOfPackagesInfo.HasNotifications());

			SetupAirEMM();
			line.EL_NumberOfPackages = 0;
			Assert("MessageErrors", line.EL_NumberOfPackagesInfo.HasMessageErrors());

			SetupSeaEMM();
			line.EL_NumberOfContainers = 0;
			line.EL_NumberOfPackages = 0;
			Assert("MessageErrors", line.EL_NumberOfPackagesInfo.HasMessageErrors());
		}

		public void TestValidateEL_NumberOfContainers()
		{
			line.EL_NumberOfContainers = -1;
			Assert("MessageErrors", line.EL_NumberOfContainersInfo.HasMessageErrors());
			SetupAirESM();
			line.EL_NumberOfContainers = 1;
			Assert("MessageErrors", line.EL_NumberOfContainersInfo.HasMessageErrors());
			line.EL_NumberOfPackages = 1;
			line.EL_NumberOfContainers = 0;
			Assert("NoNotifications", !line.EL_NumberOfContainersInfo.HasNotifications());

			SetupSeaESM();
			line.EL_NumberOfPackages = 0;
			line.EL_NumberOfContainers = 0;
			Assert("MessageErrors", line.EL_NumberOfContainersInfo.HasMessageErrors());
			line.EL_NumberOfPackages = 1;
			line.EL_NumberOfContainers = 0;
			Assert("NoNotifications", !line.EL_NumberOfContainersInfo.HasNotifications());
		}

		public void TestValidateEL_GoodsDescription()
		{
			SetupAirESM();
			SetupLowValueExemptLine();
			line.EL_GoodsDescription = "DESCRIPTION";
			Assert("NoNotifications", !line.EL_GoodsDescriptionInfo.HasNotifications());
			line.EL_GoodsDescription = ZString.Empty;
			Assert("MessageErrors", line.EL_GoodsDescriptionInfo.HasMessageErrors());
		}

		public void TestValidateEL_RN_NKCountryOfDestination()
		{
			SetupAirESM();
			SetupLowValueExemptLine();
			line.EL_RN_NKCountryOfDestination = "NZ";
			Assert("NoNotifications", !line.EL_RN_NKCountryOfDestinationInfo.HasNotifications());
			line.EL_RN_NKCountryOfDestination = ZString.Empty;
			Assert("MessageErrors", line.EL_RN_NKCountryOfDestinationInfo.HasMessageErrors());
			line.EL_RN_NKCountryOfDestination = "ZZ";
			Assert("MessageErrors", line.EL_RN_NKCountryOfDestinationInfo.HasMessageErrors());
		}

		public void TestValidateEL_GoodsOwnerPartyID()
		{
			SetupSeaESM();
			SetupLowValueExemptLine();
			line.EL_GoodsOwnerPartyID = "OwnerID";
			Assert("NoNotifications", !line.EL_GoodsOwnerPartyIDInfo.HasNotifications());

			line.EL_GoodsOwnerPartyID = ZString.Empty;
			Assert("MessageErrors", line.EL_GoodsOwnerPartyIDInfo.HasMessageErrors());

			line.EL_GoodsOwner = "Owner";
			line.EL_GoodsOwnerPartyID = ZString.Empty;
			Assert("NoNotifications", !line.EL_GoodsOwnerPartyIDInfo.HasNotifications());
			SetupAirEMM();
			line.EL_GoodsOwner = ZString.Empty;
			line.EL_GoodsOwnerPartyID = ZString.Empty;
			Assert("NoNotifications", !line.EL_GoodsOwnerPartyIDInfo.HasNotifications());
		}

		public void TestValidateEL_GoodsOwner()
		{
			SetupSeaESM();
			SetupLowValueExemptLine();
			line.EL_GoodsOwner = "OwnerName";
			Assert("NoNotifications", !line.EL_GoodsOwnerInfo.HasNotifications());

			line.EL_GoodsOwner = ZString.Empty;
			Assert("MessageErrors", line.EL_GoodsOwnerInfo.HasMessageErrors());

			line.EL_GoodsOwnerPartyID = "OwnerID";
			line.EL_GoodsOwner = ZString.Empty;
			Assert("NoNotifications", !line.EL_GoodsOwnerInfo.HasNotifications());
			SetupSeaEMM();
			line.EL_GoodsOwnerPartyID = ZString.Empty;
			line.EL_GoodsOwner = ZString.Empty;
			Assert("NoNotifications", !line.EL_GoodsOwnerInfo.HasNotifications());
		}

		public void TestGoodsOwnerRequiredForAirCTOConsolidation()
		{
			ExportCustomsManifestLines line = SetupAirCTOESM();

			line.EL_GoodsOwner = ZString.Empty;
			line.EL_GoodsOwnerPartyID = ZString.Empty;
			AssertHasMessageErrors(line.EL_GoodsOwnerInfo);
			AssertHasMessageErrors(line.EL_GoodsOwnerPartyIDInfo);

			line.EL_GoodsOwner = "foo";
			line.EL_GoodsOwnerPartyID = ZString.Empty;
			AssertNoNotifications(line.EL_GoodsOwnerInfo);
			AssertNoNotifications(line.EL_GoodsOwnerPartyIDInfo);

			line.EL_GoodsOwner = ZString.Empty;
			line.EL_GoodsOwnerPartyID = "bar";
			AssertNoNotifications(line.EL_GoodsOwnerInfo);
			AssertNoNotifications(line.EL_GoodsOwnerPartyIDInfo);

			line.EL_GoodsOwner = "foo";
			line.EL_GoodsOwnerPartyID = "bar";
			AssertNoNotifications(line.EL_GoodsOwnerInfo);
			AssertNoNotifications(line.EL_GoodsOwnerPartyIDInfo);
		}

		public void TestMessageErrorIfThereIsADuplicateCAN()
		{
			SetupSeaESM();
			ExportCustomsManifestLines secondLine = header.Lines.AddNew();
			AssertEquals("Number of Lines", 2, header.Lines.Count);
			SetupCANLine();
			secondLine.EL_TypeOfCAN = CANType.CustomsAuthorityNumber.Code;
			secondLine.EL_CAN = line.EL_CAN;
			Assert("MessageErrors", secondLine.EL_CANInfo.HasMessageErrors());
			secondLine.EL_CAN = "AAAAJMLXC";
			Assert("!MessageErrors", !secondLine.EL_CANInfo.HasMessageErrors());
		}

		public void TestContingencyCANIsOK()
		{
			SetupAirESM();
			SetupCANLine();
			line.EL_TypeOfCAN = CANType.ContingencyCustomsAuthorityNumber.Code;
			line.EL_CAN = "12345678901234";
			Assert("NoMessageErrors", !line.EL_TypeOfCANInfo.HasMessageErrors());
			Assert("NoMessageErrors", !line.EL_CANInfo.HasMessageErrors());
		}

		public void TestValidateCCANThatIsTooLong()
		{
			SetupAirESM();
			SetupCANLine();
			line.EL_TypeOfCAN = CANType.ContingencyCustomsAuthorityNumber.Code;
			line.EL_CAN = "123456789012345";
			Assert("NoMessageErrors", !line.EL_TypeOfCANInfo.HasMessageErrors());
			Assert("MessageErrors", line.EL_CANInfo.HasMessageErrors());
		}

		public void TestValidateCCANThatIsTooShort()
		{
			SetupAirESM();
			SetupCANLine();
			line.EL_TypeOfCAN = CANType.ContingencyCustomsAuthorityNumber.Code;
			line.EL_CAN = "1234567890123";
			Assert("NoMessageErrors", !line.EL_TypeOfCANInfo.HasMessageErrors());
			Assert("MessageErrors", line.EL_CANInfo.HasMessageErrors());
		}

		public void TestValidateContainersIfWeChangePacks()
		{
			SetupSeaESM();
			SetupCANLine();
			line.EL_NumberOfPackages = 0;
			line.EL_NumberOfContainers = 0;
			Assert("ContainerMessageErrors", line.EL_NumberOfContainersInfo.HasMessageErrors());
			line.EL_NumberOfPackages = 1;
			Assert("NoContainerMessageErrors", !line.EL_NumberOfContainersInfo.HasMessageErrors());
		}

		public void TestValidatePacksIfWeChangeContainers()
		{
			SetupSeaESM();
			SetupCANLine();
			line.EL_NumberOfContainers = 0;
			line.EL_NumberOfPackages = 0;
			Assert("EL_NumberOfPackagesMessageErrors", line.EL_NumberOfPackagesInfo.HasMessageErrors());
			line.EL_NumberOfContainers = 1;
			Assert("NoEL_NumberOfPackagesMessageErrors", !line.EL_NumberOfPackagesInfo.HasMessageErrors());
		}

		public void TestThatWeDontErrorAContainerCountOfZeroForAirLines()
		{
			SetupAirESM();
			SetupCANLine();
			line.EL_NumberOfContainers = 0;
			line.EL_NumberOfPackages = 0;
			Assert("NoContainerMessageErrors", !line.EL_NumberOfContainersInfo.HasMessageErrors());
		}

		public void TestMessageErrorIfBlankEL_TypeOfCAN()
		{
			SetupAirESM();
			SetupCANLine();
			AssertEquals("Warnings", false, line.EL_TypeOfCANInfo.HasWarnings());
			line.EL_TypeOfCAN = ZString.Empty;
			AssertEquals("Warnings", true, line.EL_TypeOfCANInfo.HasWarnings());
		}

		ExportCustomsManifestLines SetupAirCTOESM()
		{
			var header = Factory.New<AirCTOExportCustomsManifestHeader>();
			var line = header.Lines.AddNew();

			header.ED_ManifestType = AirManifestTypeList.Codes.ConsolidationExportSubManifest;
			return line;
		}
	}
}
