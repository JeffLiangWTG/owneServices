using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class AsycudaAdditionalInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeList()
		{
			var factory = Factory;
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Israel);
			helper.CreateCusCodeType(code: "ADDIN", desc: "ADDIN", dataGrouping: Core.Constants.CountryCodes.Israel);
			var code1 = helper.CreateCusCodeList(dataGroupingCode: Core.Constants.CountryCodes.Israel, codeType: "ADDIN", code: "code1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var code2 = helper.CreateCusCodeList(dataGroupingCode: Core.Constants.CountryCodes.Israel, codeType: "ADDIN", code: "code2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var code3 = helper.CreateCusCodeList(dataGroupingCode: Core.Constants.CountryCodes.Israel, codeType: "ADDIN", code: "code3", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var code4 = helper.CreateCusCodeList(dataGroupingCode: Core.Constants.CountryCodes.Israel, codeType: "ADDIN", code: "code4", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var code12 = helper.CreateCusCodeList(dataGroupingCode: Core.Constants.CountryCodes.Israel, codeType: "ADDIN", code: "12", description: "Cancellation", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(code1.PK, "Level", "Bill");
			helper.CreateCusCodeListAttribute(code2.PK, "Level", "Item");
			helper.CreateCusCodeListAttribute(code3.PK, "Level", "Bill");
			helper.CreateCusCodeListAttribute(code3.PK, "Level", "Item");
			helper.CreateCusCodeListAttribute(code12.PK, "Level", "Bill");

			factory.Save();

			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var addInfo = bill.AdditionalInfos.AddNew();
			var codeList = addInfo.Lookups.CodeList as CodeDescriptionPairList;
			AssertEquals(3, codeList.Count);
			AssertEquals("12, code1, code3", codeList.CodesAsString);
			AssertEquals("Action Code", codeList.GetDescriptionFromCode("12"));

			var packItem = bill.PackedItems.AddNew();
			addInfo = packItem.AdditionalInfos.AddNew();
			codeList = addInfo.Lookups.CodeList as CodeDescriptionPairList;
			AssertEquals(2, codeList.Count);
			AssertEquals("code2, code3", codeList.CodesAsString);
		}

		public void TestReferenceNumberList()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var addInfo = bill.AdditionalInfos.AddNew();
			addInfo.CSI_Code = Constants.AsycudaAdditionalInfoCodes.UNLOCO;
			AssertType<RefUNLOCOCollection>(addInfo.Lookups.ReferenceNumberList);

			addInfo.CSI_Code = Constants.AsycudaAdditionalInfoCodes.ExporterTypeID;
			AssertType<ILStatementCodeListForExporterIDType>(addInfo.Lookups.ReferenceNumberList);

			addInfo.CSI_Code = Constants.AsycudaAdditionalInfoCodes.NightStop;
			AssertType<ILStatementCodeList>(addInfo.Lookups.ReferenceNumberList);

			addInfo.CSI_Code = Constants.AsycudaAdditionalInfoCodes.IsCooling;
			AssertType<ILStatementCodeList>(addInfo.Lookups.ReferenceNumberList);

			addInfo.CSI_Code = Constants.AsycudaAdditionalInfoCodes.MultipleDeals;
			AssertType<ILStatementCodeList>(addInfo.Lookups.ReferenceNumberList);

			addInfo.CSI_Code = Constants.AsycudaAdditionalInfoCodes.CargoType;
			AssertType<ILStatementCodeListForCargoType>(addInfo.Lookups.ReferenceNumberList);

			addInfo.CSI_Code = Constants.AsycudaAdditionalInfoCodes.ActionCode;
			AssertType<ILStatementCodeListForActionCode>(addInfo.Lookups.ReferenceNumberList);

			addInfo.CSI_Code = "";
			AssertNull(addInfo.Lookups.ReferenceNumberList);
		}

		public void TestDescriptionList()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var billItem = header.Bills.AddNew().PackedItems.AddNew();
			var addInfo = billItem.AdditionalInfos.AddNew();
			addInfo.CSI_Code = Constants.AsycudaAdditionalInfoCodes.IsDirectDelivery;
			AssertType<ILContentListForIsDirectDelivery>(addInfo.Lookups.DescriptionList);

			addInfo.CSI_Code = "";
			AssertNull(addInfo.Lookups.DescriptionList);
		}
	}
}
