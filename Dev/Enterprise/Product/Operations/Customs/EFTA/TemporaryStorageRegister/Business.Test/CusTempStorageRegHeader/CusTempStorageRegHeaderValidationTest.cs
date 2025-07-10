using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.Testing;

[TestedType(typeof(CusTempStorageRegHeaderValidation))]
sealed class CusTempStorageRegHeaderValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckSRH_CustomsOffice()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var euDataGrouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
		_ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: euDataGrouping);
		_ = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "MainCustomsOffice");
		_ = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE1", "Germany", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();

		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
		{
			var header = Factory.New<CusTempStorageRegHeader>();
			header.SRH_CustomsOffice = "GB1";
			header.Validation.ValidateSRH_CustomsOffice();
			AssertHasMessageErrorContaining(header.SRH_CustomsOfficeInfo, ListValidation.InvalidCodeMessageError);

			header.SRH_CustomsOffice = "DE1";
			header.Validation.ValidateSRH_CustomsOffice();
			AssertNoMessageErrorContaining(header.SRH_CustomsOfficeInfo, ListValidation.InvalidCodeMessageError);
		}
	}
}
