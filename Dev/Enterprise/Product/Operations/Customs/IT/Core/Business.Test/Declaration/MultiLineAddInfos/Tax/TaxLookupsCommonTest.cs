using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class TaxLookupsCommonTest : TestCaseWithFactory
{
	public void TestTypeListContainsVatExemptionCodes()
	{
		SetUpMock(Factory.New<ICanBeImportOrExportDummyBizObj>());

		CombineAssertions(() =>
		{
			var typeList = new TaxLookupsCommon(euTaxMock.Object).TypeList;
			const string vatExemption406 = UniversalReferenceConstants.RefCusRateCodes.ItalianCustomsVatExemptionCode406;
			const string vatExemption407 = UniversalReferenceConstants.RefCusRateCodes.ItalianCustomsVatExemptionCode407;
			Assert($"Should contain {vatExemption406}", typeList.ContainsCode(vatExemption406));
			Assert($"Should contain {vatExemption407}", typeList.ContainsCode(vatExemption407));
		});
	}

	public void TestRateTypesForImport()
	{
		declaration.JE_MessageType = "IMP";

		SetUpMock(Factory.New<ICanBeImportDummyBizObj>());

		CombineAssertions(() =>
		{
			var typeList = new TaxLookupsCommon(euTaxMock.Object).TypeList;
			Assert("Should contain MiscellaneousImportExport", typeList.ContainsCode("111"));
			Assert("Should contain MiscellaneousNotVatableImportExport", typeList.ContainsCode("222"));
			Assert("Should not contain Miscellanous only for Export", !typeList.ContainsCode("333"));
		});
	}

	public void TestRateTypesForExport()
	{
		declaration.JE_MessageType = "EXP";

		SetUpMock(Factory.New<ICanBeExportDummyBizObj>());

		CombineAssertions(() =>
		{
			var typeList = new TaxLookupsCommon(euTaxMock.Object).TypeList;
			Assert("Should contain MiscellaneousImportExport", typeList.ContainsCode("111"));
			Assert("Should contain MiscellaneousNotVatableImportExport", typeList.ContainsCode("222"));
			Assert("Should contain Miscellanous only for Export", typeList.ContainsCode("333"));

			AssertTypeListDoNotContainCodesForRateTypeDuty(typeList);
		});
	}

	public void TestRateTypeForUcc6Export()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			SetUpMock(entryLine);

			CombineAssertions(() =>
			{
				var typeList = new TaxLookupsCommon(euTaxMock.Object).TypeList;
				Assert("Should contain Codes with Rate Type DTY", typeList.ContainsCode("234"));
				Assert("Should contain Codes with Rate Type ADD", typeList.ContainsCode("987"));
				Assert("Should contain Codes with Rate Type CVD", typeList.ContainsCode("989"));
			});
		}
	}

	static void AssertTypeListDoNotContainCodesForRateTypeDuty(RateCodeDescriptionPairList typeList)
	{
		Assert("Should not contain codes with Rate type DTY", !typeList.ContainsCode("234"));
		Assert("Should not contain codes with Rate type ADD", !typeList.ContainsCode("987"));
		Assert("Should not contain codes with Rate type CVD", !typeList.ContainsCode("989"));
	}

	void SetUpMock(ICanBeImportOrExport bizO)
	{
		euTaxMock = new Mock<IEuTax>();
		euTaxMock.Setup(m => m.ImportExportParent).Returns(bizO);
		euTaxMock.Setup(m => m.Factory).Returns(Factory);
		euTaxMock.Setup(m => m.CountryCode).Returns("IT");
	}
	Mock<IEuTax> euTaxMock;

	protected override void SetUp()
	{
		base.SetUp();

		var refDataHelper = new UniversalReferenceTestDataHelper(Factory);

		var eunDataGrouping = refDataHelper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
		refDataHelper.CreateNewOrGetExistingDataGrouping("IT", parent: eunDataGrouping);

		var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		var ordVat = refDataHelper.CreateTaxOrFee("ORD", 0.2m, currentCountryCode, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));

		var mieRateType = refDataHelper.CreateCusRateType(dataGroupingCode: "IT", rateType: "MIE", description: "Miscellaneous Import Export");
		var mnbRateType = refDataHelper.CreateCusRateType(dataGroupingCode: "IT", rateType: "MNB", description: "Miscellaneous Not VATable Import Export");
		var moeRateType = refDataHelper.CreateCusRateType(dataGroupingCode: "IT", rateType: "MOE", description: "Miscellaneous only for Export");
		var dutyRateType = refDataHelper.CreateCusRateType(dataGroupingCode: "IT", rateType: "DTY", description: "Duty");
		var antiDumpingDutyRateType = refDataHelper.CreateCusRateType(dataGroupingCode: "IT", rateType: "ADD", description: "Anti Dumping Duty");
		var countervailingDutyRateType = refDataHelper.CreateCusRateType(dataGroupingCode: "IT", rateType: "CVD", description: "Countervailing Duty");

		refDataHelper.CreateCusRateCode(Factory, zy1RateCode: "111", mieRateType.PK);
		refDataHelper.CreateCusRateCode(Factory, zy1RateCode: "222", mnbRateType.PK);
		refDataHelper.CreateCusRateCode(Factory, zy1RateCode: "333", moeRateType.PK);

		refDataHelper.CreateCusRateCode(Factory, zy1RateCode: "234", dutyRateType.PK);
		refDataHelper.CreateCusRateCode(Factory, zy1RateCode: "987", antiDumpingDutyRateType.PK);
		refDataHelper.CreateCusRateCode(Factory, zy1RateCode: "989", countervailingDutyRateType.PK);

		Factory.Save();

		declaration = Factory.New<JobDeclaration>();
	}
	JobDeclaration declaration;

	#region DummyBizObjects

	class ICanBeImportOrExportDummyBizObj : DummyBusinessObject, ICanBeImportOrExport
	{
		public ICanBeImportOrExportDummyBizObj(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
		public ZBool IsImport => IsImportCore;
		protected virtual ZBool IsImportCore => false;
		public ZBool IsExport => IsExportCore;
		protected virtual ZBool IsExportCore => false;
		public string Level => throw new System.NotImplementedException();
		public string TrueCountryCode => throw new System.NotImplementedException();
		public string DataGroupingCode => Core.Constants.CountryCodes.Italy;
		public void ValidatePreviousDocuments() => throw new System.NotImplementedException();
	}

	class ICanBeImportDummyBizObj : ICanBeImportOrExportDummyBizObj
	{
		public ICanBeImportDummyBizObj(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
		protected override ZBool IsImportCore => true;
	}

	class ICanBeExportDummyBizObj : ICanBeImportOrExportDummyBizObj
	{
		public ICanBeExportDummyBizObj(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
		protected override ZBool IsExportCore => true;
	}

	#endregion
}
