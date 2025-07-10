using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(INOrgImpAddInfo))]
sealed class INOrgImpAddInfoTest : NonPersistentBusinessObjectTestCase
{
	public void TestConstructor()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var countryData = Factory.New<OrgCountryData>();
		countryData.OV_OH_OrgHeader = orgHeader.PK;
		countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.India;
		var orgImpAddInfo = new INOrgImpAddInfo((ZPropertyInfoString)countryData.OV_ImportCustomsDefaultAddInfoInfo);
		orgImpAddInfo.ZO_TypeOfExporter = ExporterTypeList.Codes.MfgExporter;
		Factory.Save();

		orgImpAddInfo = new INOrgImpAddInfo((ZPropertyInfoString)countryData.OV_ImportCustomsDefaultAddInfoInfo);
		AssertEquals(ExporterTypeList.Codes.MfgExporter, orgImpAddInfo.ZO_TypeOfExporter);
	}

	public void TestGet()
	{
		CombineAssertions(() =>
		{
			AssertNull("Null Organisation", INOrgImpAddInfo.Get(null));

			var orgHeader = Factory.New<OrgHeader>();
			var countryData = Factory.New<OrgCountryData>();
			countryData.OV_OH_OrgHeader = orgHeader.PK;
			countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.India;
			AssertSame("Organisation with Country Data", countryData.ImpAddInfo, INOrgImpAddInfo.Get(orgHeader));
		});
	}

	public void TestLookups()
	{
		AssertType<INOrgImpAddInfoLookups>(INOrgImpAddInfo.Get(Factory.New<OrgHeader>()).Lookups);
	}

	public void TestZO_TypeOfExporter_Caption()
	{
		var resourceStringDataAttribute = INOrgImpAddInfo.Get(Factory.New<OrgHeader>()).ZO_TypeOfExporterInfo.GetAttribute<ResourceStringDataAttribute>();
		CombineAssertions(() =>
		{
			AssertEquals("ShortCaption", "Ty. Exporter", resourceStringDataAttribute.ShortCaption);
			AssertEquals("MediumCaption", "Ty. of Exporter", resourceStringDataAttribute.MediumCaption);
			AssertEquals("Caption", "Type of Exporter", resourceStringDataAttribute.Caption);
		});
	}

	public void TestZO_TypeOfImporter_Caption()
	{
		var resourceStringDataAttribute = INOrgImpAddInfo.Get(Factory.New<OrgHeader>()).ZO_TypeOfImporterInfo.GetAttribute<ResourceStringDataAttribute>();
		CombineAssertions(() =>
		{
			AssertEquals("ShortCaption", "Ty. Importer", resourceStringDataAttribute.ShortCaption);
			AssertEquals("MediumCaption", "Ty. of Importer", resourceStringDataAttribute.MediumCaption);
			AssertEquals("Caption", "Type of Importer", resourceStringDataAttribute.Caption);
		});
	}

	public void TestZO_IsDiplomat_Caption()
	{
		var resourceStringDataAttribute = INOrgImpAddInfo.Get(Factory.New<OrgHeader>()).ZO_IsDiplomatInfo.GetAttribute<ResourceStringDataAttribute>();
		AssertEquals("Caption", "Is Diplomat?", resourceStringDataAttribute.Caption);
	}

	public void TestZO_IsDiplomat_ReadOnlyAndCleanup()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		orgHeader.OH_Category = OrgConstants.Category.Government;
		orgHeader.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.India;
		var countryData = Factory.New<OrgCountryData>();
		countryData.OV_OH_OrgHeader = orgHeader.PK;
		countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.India;
		var orgImpAddInfo = new INOrgImpAddInfo((ZPropertyInfoString)countryData.OV_ImportCustomsDefaultAddInfoInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Category is GOV", false, orgImpAddInfo.ZO_IsDiplomatInfo.ReadOnly);
			orgImpAddInfo.ZO_IsDiplomat = ZBool.True;
			orgHeader.OH_Category = OrgConstants.Category.Business;
			AssertEquals("Category is not GOV, ZO_IsDiplomat Cleanup", true, orgImpAddInfo.ZO_IsDiplomatInfo.ReadOnly);
			AssertEquals("Category is not GOV, ZO_IsDiplomat Cleanup", ZBool.False, orgImpAddInfo.ZO_IsDiplomat);
		});
	}

	public void TestDefaultTypeOfImporter()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		orgHeader.OH_Category = OrgConstants.Category.Government;
		orgHeader.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.India;
		var countryData = Factory.New<OrgCountryData>();
		countryData.OV_OH_OrgHeader = orgHeader.PK;
		countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.India;
		var orgImpAddInfo = new INOrgImpAddInfo((ZPropertyInfoString)countryData.OV_ImportCustomsDefaultAddInfoInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Should default to G", ImporterTypeList.Codes.GovernmentDepartmentsBothCenterState, orgImpAddInfo.ZO_TypeOfImporter);
			orgImpAddInfo.ZO_TypeOfImporter = ZString.Empty;
			orgHeader.OH_Category = OrgConstants.Category.Business;
			orgHeader.OH_Category = OrgConstants.Category.Government;
			AssertEquals("Should not default again", ZString.Empty, orgImpAddInfo.ZO_TypeOfImporter);

			orgHeader.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			orgImpAddInfo = new INOrgImpAddInfo((ZPropertyInfoString)countryData.OV_ImportCustomsDefaultAddInfoInfo);
			AssertEquals("Should default for IN only", ZString.Empty, orgImpAddInfo.ZO_TypeOfImporter);
		});
	}

	protected override BusinessObject GetNewBusinessObject() => new INOrgImpAddInfo(Factory);
}
