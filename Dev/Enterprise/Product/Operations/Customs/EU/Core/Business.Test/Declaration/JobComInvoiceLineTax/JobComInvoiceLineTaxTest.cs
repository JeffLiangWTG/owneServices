using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceLineTax))]
	class JobComInvoiceLineTaxTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetMoP()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "Method of Payment");

			var gBExportMOP = helper.CreateNewOrGetExistingCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "X", "Daniel Test Export", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(gBExportMOP.PK, RefCusCodeListAttributeTypes.Codes.Category, UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);
			helper.CreateNewOrGetExistingCusCodeListAttribute(gBExportMOP.PK, RefCusCodeListAttributeTypes.Codes.Export, ZString.Empty);

			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "EXP";
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var tax = invoiceLine.Taxes.AddNew();
			tax.JLT_Type = "ANY";
			AssertEquals("X", tax.JLT_MethodOfPayment);
		}
	}
}
