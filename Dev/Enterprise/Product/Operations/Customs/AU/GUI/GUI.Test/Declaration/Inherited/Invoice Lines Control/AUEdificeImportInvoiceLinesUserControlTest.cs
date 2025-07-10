using Enterprise.Customs.AU.Declaration.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class AUEdificeImportInvoiceLinesUserControlTest : AUImportInvoiceLineUserControlTest
	{
		[ExpectNoExceptions]
		public override void TestInstantiation()
		{
			using (var control = new AUEdificeImportInvoiceLinesUserControl())
			{
			}
		}

		public void TestInvoiceLineGridContext()
		{
			using (var control = new AUEdificeImportInvoiceLinesUserControl())
			{
				AssertEquals("Context is set", nameof(DeclarationType.EdificeImport), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
			}
		}

		protected override JobDeclaration GetNewDeclaration()
		{
			var declaration = base.GetNewDeclaration();
			declaration.JE_ApplicationCode = "LEG";
			return declaration;
		}
	}
}
