using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	sealed class IdentificationTypeHelperTest : TestCaseWithFactory
	{
		public void TestGetIdentificationIndicatorListExcludingSIN_NullDec()
		{
			AssertEquals(ZString.Empty, IdentificationTypeHelper.GetIdentificationIndicatorListExcludingSIN(null).CodesAsString);
		}

		public void TestGetIdentificationIndicatorListExcludingSIN_NoCustomsOffice()
		{
			var header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			var storageDec = header.CHGOFFCusTempStorageDecs.AddNew();
			storageDec.StorageHeader.SJH_CustomsOffice = ZString.Empty;
			var list = storageDec.GetIdentificationIndicatorListExcludingSIN();
			CombineAssertions(() =>
			{
				AssertEquals("No CusOffice", "AWB, REG", list.CodesAsString);
				AssertSame("Cached", list, storageDec.GetIdentificationIndicatorListExcludingSIN());
			});
		}

		public void TestGetIdentificationIndicatorListExcludingSIN_CustomsOffice_NoRoles()
		{
			SetupCusCodesForCustomsOffice(Factory);
			var header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			var storageDec = header.CHGOFFCusTempStorageDecs.AddNew();
			storageDec.StorageHeader.SJH_CustomsOffice = CustomsOfficeCodeWithNoRoles;
			AssertEquals("REG", storageDec.GetIdentificationIndicatorListExcludingSIN().CodesAsString);
		}

		public void TestGetIdentificationIndicatorListExcludingSIN_CustomsOffice_WithDestinationAirRole()
		{
			SetupCusCodesForCustomsOffice(Factory);
			var header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			var storageDec = header.CHGOFFCusTempStorageDecs.AddNew();
			storageDec.StorageHeader.SJH_CustomsOffice = CustomsOfficeCodeWithDestinationAirRole;
			AssertEquals("AWB, REG", storageDec.GetIdentificationIndicatorListExcludingSIN().CodesAsString);
		}

		public void TestGetIdentificationIndicatorListExcludingSIN_CustomsOffice_WithDepartureAirRole()
		{
			SetupCusCodesForCustomsOffice(Factory);
			var header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			var storageDec = header.CHGOFFCusTempStorageDecs.AddNew();
			storageDec.StorageHeader.SJH_CustomsOffice = CustomsOfficeCodeWithDepartureAirRole;
			AssertEquals("REG", storageDec.GetIdentificationIndicatorListExcludingSIN().CodesAsString);
		}

		public void TestGetIdentificationIndicatorListIncludingSIN_NullDec()
		{
			AssertEquals(ZString.Empty, IdentificationTypeHelper.GetIdentificationIndicatorListIncludingSIN(null).CodesAsString);
		}

		public void TestGetIdentificationIndicatorListIncludingSIN_NoCustomsOffice()
		{
			var header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			var storageDec = header.CHGOFFCusTempStorageDecs.AddNew();
			storageDec.StorageHeader.SJH_CustomsOffice = ZString.Empty;
			var list = storageDec.GetIdentificationIndicatorListIncludingSIN();
			CombineAssertions(() =>
			{
				AssertEquals("No CusOffice", "AWB, REG, SIN", list.CodesAsString);
				AssertSame("Cached", list, storageDec.GetIdentificationIndicatorListIncludingSIN());
			});
		}

		public void TestGetIdentificationIndicatorListIncludingSIN_CustomsOffice_NoRoles()
		{
			SetupCusCodesForCustomsOffice(Factory);
			var header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			var storageDec = header.CHGOFFCusTempStorageDecs.AddNew();
			storageDec.StorageHeader.SJH_CustomsOffice = CustomsOfficeCodeWithNoRoles;
			AssertEquals("REG, SIN", storageDec.GetIdentificationIndicatorListIncludingSIN().CodesAsString);
		}

		public void TestGetIdentificationIndicatorListIncludingSIN_CustomsOffice_WithDestinationAirRole()
		{
			SetupCusCodesForCustomsOffice(Factory);
			var header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			var storageDec = header.CHGOFFCusTempStorageDecs.AddNew();
			storageDec.StorageHeader.SJH_CustomsOffice = CustomsOfficeCodeWithDestinationAirRole;
			AssertEquals("AWB, REG, SIN", storageDec.GetIdentificationIndicatorListIncludingSIN().CodesAsString);
		}

		public void TestGetIdentificationIndicatorListIncludingSIN_CustomsOffice_WithDepartureAirRole()
		{
			SetupCusCodesForCustomsOffice(Factory);
			var header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			var storageDec = header.CHGOFFCusTempStorageDecs.AddNew();
			storageDec.StorageHeader.SJH_CustomsOffice = CustomsOfficeCodeWithDepartureAirRole;
			AssertEquals("REG, SIN", storageDec.GetIdentificationIndicatorListIncludingSIN().CodesAsString);
		}

		public static void SetupCusCodesForCustomsOffice(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, CustomsOfficeCodeWithNoRoles, "Customs Office 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var destinationAir = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, CustomsOfficeCodeWithDestinationAirRole, "Customs Office 2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var destinationAttribute = helper.CreateNewOrGetExistingCusCodeListAttribute(destinationAir.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EU.Business.EuOfficeCodesTypes.Codes.OfficeOfDestination);
			helper.CreateTransportModeForCusCodeAttribute(destinationAttribute.PK, RefTransportModeList.Codes.AIR);
			helper.CreateTransportModeForCusCodeAttribute(destinationAttribute.PK, RefTransportModeList.Codes.ROA);
			var departureAir = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, CustomsOfficeCodeWithDepartureAirRole, "Customs Office 3", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var departureAttribute = helper.CreateNewOrGetExistingCusCodeListAttribute(departureAir.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EU.Business.EuOfficeCodesTypes.Codes.OfficeOfDeparture);
			helper.CreateTransportModeForCusCodeAttribute(departureAttribute.PK, RefTransportModeList.Codes.AIR);
			helper.CreateTransportModeForCusCodeAttribute(departureAttribute.PK, RefTransportModeList.Codes.SEA);
			factory.Save();
		}

		const string CustomsOfficeCodeWithDepartureAirRole = "DE002391";
		public const string CustomsOfficeCodeWithDestinationAirRole = "DE002102";
		public const string CustomsOfficeCodeWithNoRoles = "DE019004";
	}
}
