using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceLineCompleteCollection))]
	public class InvoiceLineCompleteCollectionTest : EU.Business.Declaration.Testing.InvoiceLineCompleteCollectionTest
	{
		public void TestDefaultZG_CountryOfDestination()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			AssertEquals(ZString.Empty, invoiceLine.ZG_CountryOfDestination);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new InvoiceLineCompleteCollection((JobDeclaration)Declaration);

		protected override BaseJobDeclaration GetMeANewJobDeclaration()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			return dec;
		}

		protected override Customs.Business.CusEntryInstruction GetCusEntryInstruction(BaseJobDeclaration declaration)
		{
			return ((JobDeclaration)declaration).CustomsEntryInstructions.OfType<CusEntryInstruction>().FirstOrDefault();
		}
	}
}
