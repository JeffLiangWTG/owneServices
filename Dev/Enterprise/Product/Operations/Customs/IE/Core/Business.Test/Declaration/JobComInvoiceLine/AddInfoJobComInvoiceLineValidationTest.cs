using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.IE;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	sealed class AddInfoJobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZG_RegionOfDestination()
		{
			invoiceLine.ZG_RegionOfDestination = RegionOfDestinationList.Codes.Attica;
			AssertNoMessageErrorContaining(invoiceLine.ZG_RegionOfDestinationInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.ZG_RegionOfDestination = "@1";
			AssertHasMessageErrorContaining(invoiceLine.ZG_RegionOfDestinationInfo, ListValidation.InvalidCodeMessageError);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			instruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
		}

		CusEntryInstruction instruction;
		JobComInvoiceLine invoiceLine;
	}
}
