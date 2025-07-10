using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.Intrastat.Business.Testing
{
	sealed class CusIntrastatLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCIL_Tariff()
		{
			const string atLeast8Message = "Tariff code must be at least 8 digits";
			var propertyInfo = transactionLine.CIL_TariffInfo;

			transactionLine.CIL_Tariff = "";
			AssertNoError(propertyInfo, atLeast8Message);
			ValidationTestHelper.AssertErrorIfNotEntered(propertyInfo);

			transactionLine.CIL_Tariff = "1234567";
			AssertHasError(propertyInfo, atLeast8Message);

			transactionLine.CIL_Tariff = "12345678";
			AssertNoError(propertyInfo, atLeast8Message);
		}

		public void TestCheckCIL_SupplementaryQuantityUnit()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Unit Quantities");

			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.EuropeanUnion);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "lv", eun);

			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "1", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			Factory.Save();

			var propertyInfo = transactionLine.CIL_SupplementaryQuantityUnitInfo;
			ValidationTestHelper.AssertInvalidCodeMessageError(propertyInfo, "0", "1");
		}

		public void TestCheckCIL_Region_2LetterCode()
		{
			const string messageError = "Region must be a 2 letter code";
			var propertyInfo = transactionLine.CIL_RegionInfo;

			transactionLine.CIL_Region = "";
			AssertNoError(propertyInfo, messageError);

			transactionLine.CIL_Region = "1";
			AssertHasError(propertyInfo, messageError);

			transactionLine.CIL_Region = "12";
			AssertNoError(propertyInfo, messageError);
		}

		public void TestCheckCIL_Region_ListValidation()
		{
			var transactionLineMock = Factory.NewMoq<CusIntrastatLine>();
			transactionLineMock.CallBase = true;
			transactionLineMock.Protected().Setup<CusIntrastatLineLookups>("GetNewLookups").Returns(() =>
			{
				var lookupMock = new Mock<CusIntrastatLineLookups>(transactionLineMock.Object) { CallBase = true };
				lookupMock.Protected().Setup<ICodeDescriptionPairList>("GetRegions").Returns(new CodeDescriptionPairList()
				{
					new CodeDescriptionPair("10", "Ten"),
				});
				return lookupMock.Object;
			});

			transactionLine = transactionLineMock.Object;
			var propertyInfo = transactionLine.CIL_RegionInfo;

			ValidationTestHelper.AssertInvalidCodeMessageError(propertyInfo, "11", "10");
		}

		public void TestCheckCIL_RN_NKCountryOfOrigin_2LetterCode()
		{
			const string messageError = "Country of Origin must be a 2 letter code";
			var propertyInfo = transactionLine.CIL_RN_NKCountryOfOriginInfo;

			transactionLine.CIL_RN_NKCountryOfOrigin = "";
			AssertNoError(propertyInfo, messageError);

			transactionLine.CIL_RN_NKCountryOfOrigin = "1";
			AssertHasError(propertyInfo, messageError);

			transactionLine.CIL_RN_NKCountryOfOrigin = "12";
			AssertNoError(propertyInfo, messageError);
		}

		public void TestCheckCIL_RN_NKCountryOfOrigin_ListValidation()
		{
			var propertyInfo = transactionLine.CIL_RN_NKCountryOfOriginInfo;

			ValidationTestHelper.AssertInvalidCodeMessageError(propertyInfo, "DD", "DE");
		}

		protected override void SetUp()
		{
			var helper = IntrastatTestDataHelper.New(Factory);
			transaction = helper.NewCusIntrastatHeaderWithValidData();
			transactionLine = helper.NewCusIntrastatLineWithValidData(transaction);
		}

		CusIntrastatHeader transaction;
		CusIntrastatLine transactionLine;
	}
}
