using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.Testing
{
	public class TaxLookupsCommonTest : TestCaseWithFactory
	{
		public void TestGetMOPList_ImportOrExportParent()
		{
			SetupMOPList();
			var importDeclaration = Factory.New<JobDeclaration>();
			importDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertContainsExactElementsInAnyOrder("Get all import methods of payment from ref db.", new ZString[] { "1", "2", "3" }, TaxLookupsCommon.GetMOPList(Factory, Core.Constants.CountryCodes.Latvia, importDeclaration).GetAllCodes());

			var exportDeclaration = Factory.New<JobDeclaration>();
			exportDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertContainsExactElementsInAnyOrder("Get all export methods of payment from ref db.", new ZString[] { "4", "5", "6" }, TaxLookupsCommon.GetMOPList(Factory, Core.Constants.CountryCodes.Latvia, exportDeclaration).GetAllCodes());

			var miscDeclaration = Factory.New<JobDeclaration>();
			miscDeclaration.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
			AssertContainsExactElementsInAnyOrder("Get all methods of payment from ref db.", new ZString[] { "1", "2", "3", "4", "5", "6" }, TaxLookupsCommon.GetMOPList(Factory, Core.Constants.CountryCodes.Latvia, miscDeclaration).GetAllCodes());
		}

		public void TestGetMOPList_ImportOrExport()
		{
			SetupMOPList();
			AssertContainsExactElementsInAnyOrder("Get all import methods of payment from ref db.", new ZString[] { "1", "2", "3" }, TaxLookupsCommon.GetMOPList(Factory, Core.Constants.CountryCodes.Latvia, true, false).GetAllCodes());
			AssertContainsExactElementsInAnyOrder("Get all export methods of payment from ref db.", new ZString[] { "4", "5", "6" }, TaxLookupsCommon.GetMOPList(Factory, Core.Constants.CountryCodes.Latvia, false, true).GetAllCodes());
			AssertContainsExactElementsInAnyOrder("Get all methods of payment from ref db.", new ZString[] { "1", "2", "3", "4", "5", "6" }, TaxLookupsCommon.GetMOPList(Factory, Core.Constants.CountryCodes.Latvia, false, false).GetAllCodes());
		}

		public void TestMOPList_WhenCodeTypeIs104IM()
		{
			Setup104IMList();
			var declaration = Factory.New<JobDeclaration>();
			var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			var entryLineFee = entryLine.Fees.AddNew();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			{
				AssertContainsExactElementsInAnyOrder("(104IM) Get all methods of payment from ref db.", new ZString[] { "A", "B", "C", "D" }, new TaxLookupsCommon(entryLineFee).MOPList.GetAllCodes());
			}
		}

		void SetupMOPList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "Method of Payment");

			var frImportMOP1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "1", "One", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(frImportMOP1.PK, RefCusCodeListAttributeTypes.Codes.Category, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);
			helper.CreateNewOrGetExistingCusCodeListAttribute(frImportMOP1.PK, RefCusCodeListAttributeTypes.Codes.Import, ZString.Empty);
			var frImportMOP2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "2", "Two", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(frImportMOP2.PK, RefCusCodeListAttributeTypes.Codes.Category, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);
			helper.CreateNewOrGetExistingCusCodeListAttribute(frImportMOP2.PK, RefCusCodeListAttributeTypes.Codes.Import, ZString.Empty);
			var frImportMOP3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "3", "Three", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(frImportMOP3.PK, RefCusCodeListAttributeTypes.Codes.Category, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);
			helper.CreateNewOrGetExistingCusCodeListAttribute(frImportMOP3.PK, RefCusCodeListAttributeTypes.Codes.Import, ZString.Empty);

			var frExportMOP4 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "4", "Four", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(frExportMOP4.PK, RefCusCodeListAttributeTypes.Codes.Category, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);
			helper.CreateNewOrGetExistingCusCodeListAttribute(frExportMOP4.PK, RefCusCodeListAttributeTypes.Codes.Export, ZString.Empty);
			var frExportMOP5 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "5", "Five", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(frExportMOP5.PK, RefCusCodeListAttributeTypes.Codes.Category, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);
			helper.CreateNewOrGetExistingCusCodeListAttribute(frExportMOP5.PK, RefCusCodeListAttributeTypes.Codes.Export, ZString.Empty);
			var frExportMOP6 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "6", "Six", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(frExportMOP6.PK, RefCusCodeListAttributeTypes.Codes.Category, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);
			helper.CreateNewOrGetExistingCusCodeListAttribute(frExportMOP6.PK, RefCusCodeListAttributeTypes.Codes.Export, ZString.Empty);

			var esImportMOP7 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "7", "Seven", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(esImportMOP7.PK, RefCusCodeListAttributeTypes.Codes.Category, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);
			helper.CreateNewOrGetExistingCusCodeListAttribute(esImportMOP7.PK, RefCusCodeListAttributeTypes.Codes.Import, ZString.Empty);
			var itExportMOP8 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "8", "Eight", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(itExportMOP8.PK, RefCusCodeListAttributeTypes.Codes.Category, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);
			helper.CreateNewOrGetExistingCusCodeListAttribute(itExportMOP8.PK, RefCusCodeListAttributeTypes.Codes.Export, ZString.Empty);

			Factory.Save();
		}

		void Setup104IMList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CCIMethodOfPayment, "CCI Method of Payment");
			var eun104IM1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CCIMethodOfPayment, "A", "Payment in cash", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var eun104IM2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CCIMethodOfPayment, "B", "Payment by credit card", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var eun104IM3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CCIMethodOfPayment, "C", "Payment by cheque", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var eun104IM4 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CCIMethodOfPayment, "D", "Other (e. g. direct debit to agent's cash account)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
		}
	}
}
