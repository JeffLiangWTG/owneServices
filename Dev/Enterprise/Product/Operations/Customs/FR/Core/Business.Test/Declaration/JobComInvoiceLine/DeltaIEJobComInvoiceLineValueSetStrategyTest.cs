using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class DeltaIEJobComInvoiceLineValueSetStrategyTest : TestCaseWithFactory
	{
		public void TestSettingProcedureDefaultsCustomsGuaranteeNumber()
		{
			new UniversalReferenceTestDataHelper(Factory).CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE);

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "IMPORTER";
			importer.OH_FullName = "The importer";
			importer.OH_RL_NKClosestPort = "FRPAR";
			importer.MainAddress.Address1 = "ImporterAddress";
			importer.MainAddress.OA_Code = "ImporterAddress";
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "IGUA", importer.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "ImporterAddress", ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.France);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes.DIR;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Procedure = ZString.Empty;
			AssertEquals("Customs Guarantee Number should be empty when there is no guarantee consuming procedure in declaration invoice lines.", ZString.Empty, declaration.JE_CustomsGuaranteeNumber);

			var procedure = Factory.New<RefCusProcedure>();
			procedure.ZZ6_ProcedureCode = "53";
			procedure.ZZ6_PreviousProcedureCode = "53";
			procedure.ZZ6_Concession = "D07";
			procedure.ZZ6_ShipmentType = JobMessageTypeList.Codes.Import;
			procedure.ZZ6_IsGuaranteeConsumed = Universal.CodeDescriptionPairLists.YesNoList.Codes.Yes;
			procedure.ZZ6_ZZZ_NKDataGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Procedure = "5353D07";
			AssertEquals("Customs Guarantee Number should be defaulted when there is a guarantee consuming procedure in declaration invoice lines.", "IGUA", declaration.JE_CustomsGuaranteeNumber);
		}
	}
}
