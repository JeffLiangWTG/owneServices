using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class DefaultSetterForInvoiceHeaderTest : TestCaseWithFactory
	{
		public void TestDefaultForNewElementCore_UCC6()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			declaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			declaration.JE_ShipmentIncoTermPlace = "BERGERAC";
			declaration.ZG_AgreedPlaceCode = "1";

			var invoice = declaration.Invoices.AddNew();
			AssertEquals("Default JZ_IncoTerm", Core.Constants.IncoTerms.FreeOnBoard, invoice.JZ_IncoTerm);
			AssertEquals("Default JZ_IncoTermPlace", string.Empty, invoice.JZ_IncoTermPlace);
			AssertEquals("Default ZG_AgreedPlaceCode", "1", invoice.ZG_AgreedPlaceCode);
			AssertEquals("Default ZG_ValuationMethod", Enterprise.MasterFiles.Business.Customs.EU.ValuationMethodList.Codes._1, invoice.ZG_ValuationMethod);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			declaration2.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			declaration2.JE_ShipmentIncoTermPlace = "BERGERAC";
			declaration2.ZG_AgreedPlaceCode = "";

			var invoice2 = declaration2.Invoices.AddNew();
			AssertEquals("Default JZ_IncoTerm", Core.Constants.IncoTerms.FreeOnBoard, invoice2.JZ_IncoTerm);
			AssertEquals("Default JZ_IncoTermPlace", "BERGERAC", invoice2.JZ_IncoTermPlace);
			AssertEquals("Default ZG_AgreedPlaceCode", string.Empty, invoice2.ZG_AgreedPlaceCode);
			AssertEquals("Default ZG_ValuationMethod", Enterprise.MasterFiles.Business.Customs.EU.ValuationMethodList.Codes._1, invoice2.ZG_ValuationMethod);
		}

		public void TestDefaultForNewElementCore_NotUCC6()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			declaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			declaration.JE_ShipmentIncoTermPlace = "BERGERAC";
			declaration.ZG_AgreedPlaceCode = "1";

			var invoice = declaration.Invoices.AddNew();
			AssertEquals("Default JZ_IncoTerm", Core.Constants.IncoTerms.FreeOnBoard, invoice.JZ_IncoTerm);
			AssertEquals("Default JZ_IncoTermPlace", "BERGERAC", invoice.JZ_IncoTermPlace);
			AssertEquals("Default ZG_AgreedPlaceCode", "1", invoice.ZG_AgreedPlaceCode);
			AssertEquals("Default ZG_ValuationMethod", Enterprise.MasterFiles.Business.Customs.EU.ValuationMethodList.Codes._1, invoice.ZG_ValuationMethod);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			declaration2.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			declaration2.JE_ShipmentIncoTermPlace = "BERGERAC";
			declaration2.ZG_AgreedPlaceCode = "";

			var invoice2 = declaration2.Invoices.AddNew();
			AssertEquals("Default JZ_IncoTerm", Core.Constants.IncoTerms.FreeOnBoard, invoice2.JZ_IncoTerm);
			AssertEquals("Default JZ_IncoTermPlace", "BERGERAC", invoice2.JZ_IncoTermPlace);
			AssertEquals("Default ZG_AgreedPlaceCode", string.Empty, invoice2.ZG_AgreedPlaceCode);
			AssertEquals("Default ZG_ValuationMethod", Enterprise.MasterFiles.Business.Customs.EU.ValuationMethodList.Codes._1, invoice2.ZG_ValuationMethod);
		}
	}
}
