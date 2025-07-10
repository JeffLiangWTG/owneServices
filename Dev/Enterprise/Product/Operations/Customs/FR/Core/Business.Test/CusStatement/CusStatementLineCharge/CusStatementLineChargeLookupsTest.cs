using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.FR.Business.CusStatement.Testing
{
	class CusStatementLineChargeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestChargeTypeList()
		{
			AssertEquals("Charge Type code list lookup content should match NationalFeeTypeCodeList code pair description list", new NationalFeeTypeCodeList(Factory).CodesAsString, cusStatementLineChargeLookups.ChargeTypeList.CodesAsString);
		}

		public void TestMethodOfPaymentList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "Method of Payment");

			var frImportMOP1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "1", "Cautionné", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(frImportMOP1.PK, RefCusCodeListAttributeTypes.Codes.Category, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);
			helper.CreateNewOrGetExistingCusCodeListAttribute(frImportMOP1.PK, RefCusCodeListAttributeTypes.Codes.Import, ZString.Empty);
			var frImportMOP2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "2", "Non cautionné", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(frImportMOP2.PK, RefCusCodeListAttributeTypes.Codes.Category, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);
			helper.CreateNewOrGetExistingCusCodeListAttribute(frImportMOP2.PK, RefCusCodeListAttributeTypes.Codes.Import, ZString.Empty);
			var frImportMOP3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "3", "Ai2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(frImportMOP3.PK, RefCusCodeListAttributeTypes.Codes.Category, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);
			helper.CreateNewOrGetExistingCusCodeListAttribute(frImportMOP3.PK, RefCusCodeListAttributeTypes.Codes.Import, ZString.Empty);

			var frExportMOP4 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "4", "Non perçu par la douane", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(frExportMOP4.PK, RefCusCodeListAttributeTypes.Codes.Category, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);
			helper.CreateNewOrGetExistingCusCodeListAttribute(frExportMOP4.PK, RefCusCodeListAttributeTypes.Codes.Export, ZString.Empty);
			var frExportMOP5 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "5", "Liquidation sur COD garanti", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(frExportMOP5.PK, RefCusCodeListAttributeTypes.Codes.Category, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);
			helper.CreateNewOrGetExistingCusCodeListAttribute(frExportMOP5.PK, RefCusCodeListAttributeTypes.Codes.Export, ZString.Empty);
			var frExportMOP6 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "6", "Autoliquidation ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(frExportMOP6.PK, RefCusCodeListAttributeTypes.Codes.Category, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);
			helper.CreateNewOrGetExistingCusCodeListAttribute(frExportMOP6.PK, RefCusCodeListAttributeTypes.Codes.Export, ZString.Empty);

			var esImportMOP7 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "7", "Seven", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(esImportMOP7.PK, RefCusCodeListAttributeTypes.Codes.Category, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);
			helper.CreateNewOrGetExistingCusCodeListAttribute(esImportMOP7.PK, RefCusCodeListAttributeTypes.Codes.Import, ZString.Empty);
			var itExportMOP8 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "8", "Eight", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(itExportMOP8.PK, RefCusCodeListAttributeTypes.Codes.Category, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);
			helper.CreateNewOrGetExistingCusCodeListAttribute(itExportMOP8.PK, RefCusCodeListAttributeTypes.Codes.Export, ZString.Empty);

			Factory.Save();

			statement.B2_BranchDesignation = StatementEntryTypeImpExpList.Codes.Import;
			AssertContainsExactElementsInAnyOrder("Get all import methods of payment from ref db.", new ZString[] { "1", "2", "3" }, cusStatementLineChargeLookups.MethodOfPaymentList.GetAllCodes());

			statement.B2_BranchDesignation = StatementEntryTypeImpExpList.Codes.Export;
			AssertContainsExactElementsInAnyOrder("Get all export methods of payment from ref db.", new ZString[] { "4", "5", "6" }, cusStatementLineChargeLookups.MethodOfPaymentList.GetAllCodes());
		}

		public void TestChargeGroupList()
		{
			AssertEquals("Charge Group code list lookup content should match CommunautaryChargeCodeList  code pair description list", Factory.GetCachedValue<CommunautaryChargeCodeList>().CodesAsString, cusStatementLineChargeLookups.ChargeGroupList.CodesAsString);
		}

		protected override void SetUp()
		{
			statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = StatementPeriodicityList.Codes.Day;
			charge = statement.ChargesDetail.Charges.AddNew();
			cusStatementLineChargeLookups = charge.Lookups;
		}

		CusStatementHeader statement;
		CusStatementLineCharge charge;
		CusStatementLineChargeLookups cusStatementLineChargeLookups;
	}
}
