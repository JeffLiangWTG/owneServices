using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.GB.Declaration.Testing
{
	public class AddInfoJobComInvoiceLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMethodOfPaymentList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);

			var gb = helper.CreateNewOrGetExistingDataGrouping("GB", "United Kingdom");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, "Customs Declaration Service", gb);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "Method of Payment");

			var gBImportMOPCDS = helper.CreateNewOrGetExistingCusCodeList("CDS", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "A", "Immediate payment by cash or equivalent (Paper declarations)", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeListAttribute(gBImportMOPCDS.PK, RefCusCodeListAttributeTypes.Codes.Category, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);
			helper.CreateNewOrGetExistingCusCodeListAttribute(gBImportMOPCDS.PK, RefCusCodeListAttributeTypes.Codes.Import, ZString.Empty);

			var gBImportMOP = helper.CreateNewOrGetExistingCusCodeList("GB", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "B", "Should not appear", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeListAttribute(gBImportMOP.PK, RefCusCodeListAttributeTypes.Codes.Category, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);
			helper.CreateNewOrGetExistingCusCodeListAttribute(gBImportMOP.PK, RefCusCodeListAttributeTypes.Codes.Import, ZString.Empty);

			var gBExportMOP = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "L", "CAP Export Licence", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeListAttribute(gBExportMOP.PK, RefCusCodeListAttributeTypes.Codes.Category, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);
			helper.CreateNewOrGetExistingCusCodeListAttribute(gBExportMOP.PK, RefCusCodeListAttributeTypes.Codes.Export, ZString.Empty);

			var dEMOP = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "D", "Andere (z.B. Abbuchung vom Konto eines Zollagenten)", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeListAttribute(dEMOP.PK, RefCusCodeListAttributeTypes.Codes.Category, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);

			var iTMOP = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "T", "Garanzia sul conto dello spedizioniere doganale", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeListAttribute(iTMOP.PK, RefCusCodeListAttributeTypes.Codes.Category, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AssertEquals("A", invLine.AddInfoLookups.MethodOfPaymentList.CodesAsString);
		}
	}
}
