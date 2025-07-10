using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	sealed class DefaultSetterForInvoiceHeaderTest : TestCaseWithFactory
	{
		public void TestDefaultJZ_IncoTerm()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			Assert("Precondition: INCO term on shipment is emtpy.", declaration.JE_ShipmentIncoTerm.IsEmpty);

			var invoice1 = declaration.Invoices.AddNew();
			Assert("INCO term on invoice header is emtpy as the one on parent is emtpy.", invoice1.JZ_IncoTerm.IsEmpty);

			declaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			var invoice2 = declaration.Invoices.AddNew();
			AssertEquals("INCO term on invoice header should copy from parent.", Core.Constants.IncoTerms.FreeOnBoard, invoice2.JZ_IncoTerm);

			declaration.JE_MessageType = "EXS";
			var invoice3 = declaration.Invoices.AddNew();
			Assert("INCO term on invoice header is emtpy as parent is EXS.", invoice3.JZ_IncoTerm.IsEmpty);

			declaration.JE_MessageType = "REX";
			var invoice4 = declaration.Invoices.AddNew();
			Assert("INCO term on invoice header is emtpy as parent is REX.", invoice4.JZ_IncoTerm.IsEmpty);
		}
	}
}
