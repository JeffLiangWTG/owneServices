using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class RFPNumberAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZA_RFPNumber()
		{
			rfpNumber.AddInfoValidation.ValidateZA_RFPNumber();
			AssertHasMessageErrorContaining(rfpNumber.ZA_RFPNumberInfo, MandatoryValidation.YouHaveNotEntered);

			rfpNumber.ZA_RFPNumber = "RFP1324";
			AssertNoMessageErrors(rfpNumber.ZA_RFPNumberInfo);
		}

		public void TestCheckZA_RFPLine()
		{
			rfpNumber.AddInfoValidation.ValidateZA_RFPLine();
			AssertHasMessageErrorContaining(rfpNumber.ZA_RFPLineInfo, MandatoryValidation.ValueCannotBeZero);

			rfpNumber.ZA_RFPLine = -1;
			AssertHasMessageErrorContaining(rfpNumber.ZA_RFPLineInfo, MandatoryValidation.ValueCannotBeNegative);

			rfpNumber.ZA_RFPLine = 1;
			AssertNoMessageErrors(rfpNumber.ZA_RFPLineInfo);
		}

		public void TestCheckZA_RFPNetQuantity()
		{
			rfpNumber.AddInfoValidation.ValidateZA_RFPNetQuantity();
			AssertHasMessageErrorContaining(rfpNumber.ZA_RFPNetQuantityInfo, MandatoryValidation.ValueCannotBeZero);

			rfpNumber.ZA_RFPNetQuantity = -1;
			AssertHasMessageErrorContaining(rfpNumber.ZA_RFPNetQuantityInfo, MandatoryValidation.ValueCannotBeNegative);

			rfpNumber.ZA_RFPNetQuantity = 1;
			AssertNoMessageErrors(rfpNumber.ZA_RFPNetQuantityInfo);
		}

		[TestDate(2022, 1, 18)]
		public void TestCheckZA_RFPQtyUM()
		{
			var newFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			var startDate = new ZDateTime(2022, 1, 1);
			var endDate = new ZDateTime(2022, 1, 31);
			helper.CreateNewOrGetExistingCusCodeList("AU", "EUOM", "ONZ", "OUNCE", startDate, endDate);
			newFactory.Save();

			rfpNumber.AddInfoValidation.ValidateZA_RFPQtyUM();
			AssertHasMessageErrorContaining(rfpNumber.ZA_RFPQtyUMInfo, MandatoryValidation.YouHaveNotEntered);

			rfpNumber.ZA_RFPQtyUM = "123";
			AssertHasMessageErrorContaining(rfpNumber.ZA_RFPQtyUMInfo, ListValidation.InvalidCodeMessageError);

			rfpNumber.ZA_RFPQtyUM = rfpNumber.AddInfoLookups.NetQuantityUnits[0].Code;
			AssertNoMessageErrors(rfpNumber.ZA_RFPQtyUMInfo);
		}

		public void TestCheckZA_RFPPackCount()
		{
			rfpNumber.AddInfoValidation.ValidateZA_RFPPackCount();
			AssertHasMessageErrorContaining(rfpNumber.ZA_RFPPackCountInfo, MandatoryValidation.ValueCannotBeZero);

			rfpNumber.ZA_RFPPackCount = -1;
			AssertHasMessageErrorContaining(rfpNumber.ZA_RFPPackCountInfo, MandatoryValidation.ValueCannotBeNegative);

			rfpNumber.ZA_RFPPackCount = 1;
			AssertNoMessageErrors(rfpNumber.ZA_RFPPackCountInfo);
		}

		public void TestCheckZA_RFPPackType()
		{
			rfpNumber.AddInfoValidation.ValidateZA_RFPPackType();
			AssertHasMessageErrorContaining(rfpNumber.ZA_RFPPackTypeInfo, MandatoryValidation.YouHaveNotEntered);

			rfpNumber.ZA_RFPPackType = "12";
			AssertHasMessageErrorContaining(rfpNumber.ZA_RFPPackTypeInfo, ListValidation.InvalidCodeMessageError);

			rfpNumber.ZA_RFPPackType = rfpNumber.AddInfoLookups.PackageTypes[0].Code;
			AssertNoMessageErrors(rfpNumber.ZA_RFPPackTypeInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			rfpNumber = Factory.New<RFPNumber>();
		}

		RFPNumber rfpNumber;
	}
}
