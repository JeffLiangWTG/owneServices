using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeList;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

[TestedType(typeof(CGMZZDatabaseValidationHelper))]
sealed class CGMZZDatabaseValidationHelperTest : TestCaseWithFactory
{
	public void TestMandatoryFields_Consignee()
	{
		var header = Factory.New<CGMAsycudaManifestHeader>();
		header.AMA_RN_NKCountry = Core.Constants.CountryCodes.India;
		var bill = header.Bills.AddNew();
		var propertyInfo = bill.ABL_ConsigneeNameInfo;

		using (bill.SuspendValidationTesting())
		{
			header.ZZValidationHelper.CheckIsMandatoryFor(propertyInfo, ManifestValidationRuleCodes.Consignee);
			Assert(!propertyInfo.HasMessageError("A Consignee is required"));
		}
	}

	public void TestMandatoryFields_EstimatedDepartureTime()
	{
		var header = Factory.New<CGMAsycudaManifestHeader>();
		header.AMA_RN_NKCountry = Core.Constants.CountryCodes.India;
		var bill = header.Bills.AddNew();
		var propertyInfo = bill.ABL_ConsigneeNameInfo;

		using (bill.SuspendValidationTesting())
		{
			header.ZZValidationHelper.CheckIsMandatoryFor(propertyInfo, ManifestValidationRuleCodes.EstimatedDepartureTime);
			Assert(!propertyInfo.HasMessageError("An Estimated Departure Time is required"));
		}
	}
}
