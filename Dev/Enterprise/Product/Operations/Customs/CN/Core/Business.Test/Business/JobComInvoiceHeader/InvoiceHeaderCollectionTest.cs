using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(InvoiceHeaderActiveCollection))]
	class InvoiceHeaderCollectionTest : Customs.Business.Testing.BaseInvoiceHeaderCollectionTest<InvoiceHeaderActiveCollection, JobComInvoiceHeader>
	{
		public void TestSetDefaultsForNewElement()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.CostFreightWithAmpersand;
			var invoiceHeader1 = declaration.Invoices.AddNew();
			AssertEquals(CNInvoiceHeaderIncoTermList.Codes.CFR, invoiceHeader1.JZ_IncoTerm);
			AssertEquals(ConfirmationTypeList.Codes.No, invoiceHeader1.JZ_SpecialRelationshipConfirm);
			var otherCountryFilter = new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, declaration.CountryCode);
			var otherCountry = Factory.LoadTop1<RefCountry>(otherCountryFilter);
			var otherUnloco = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, otherCountry.RN_Code));
			var consignee = OrgHeader.New(Factory);
			consignee.OH_RL_NKClosestPort = otherUnloco.RL_Code;
			var consignor = OrgHeader.New(Factory);
			declaration.JE_OH_Supplier = consignor.PK;
			declaration.JE_OH_Importer = consignee.PK;
			var link = consignee.SupplierLinks.AddNew(consignor);
			link.OL_RN_NKImporterCountry = otherCountry.RN_Code;
			AssertSame(link, declaration.SupplierImporterLink);
			link.OL_RelatedParty = "Y";
			var invoiceHeader2 = declaration.Invoices.AddNew();
			AssertEquals("1", invoiceHeader2.JZ_SpecialRelationshipConfirm);
			link.OL_RelatedParty = "N";
			var invoiceHeader3 = declaration.Invoices.AddNew();
			AssertEquals("0", invoiceHeader3.JZ_SpecialRelationshipConfirm);
		}
	}
}
