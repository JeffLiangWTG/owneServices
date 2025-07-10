using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.BR;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(ExchangeHedgeCollection))]
	public class ExchangeHedgeCollectionTest : CusSupportingInfoCollectionTest<ExchangeHedge>
	{
		public void TestReadOnlyWhenClonedFromAttached()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_JE = declaration.PK;
			entryInstruction.CEI_Description = "TEST1";

			var invHeader = declaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();

			var editableProperties = invLine.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(x => !x.ReadOnly).Select(x => x.Name);

			var licDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			licDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var licEntryInstruction = licDeclaration.CustomsEntryInstructions.AddNew();
			licEntryInstruction.CEI_JE = licDeclaration.PK;
			licEntryInstruction.CEI_Description = "TEST1";

			var licInvHeader = licDeclaration.Invoices.AddNew();
			licInvHeader.ExchangeHedgeCollection.AddNew();
			var licInvLine = licInvHeader.InvoiceLines.AddNew();
			licInvLine.JI_CEI = licEntryInstruction.PK;

			var licEntryHeader = licDeclaration.CustomsEntryHeaders.AddNew();
			licEntryHeader.CH_CEI_Instruction = licEntryInstruction.PK;
			var licEntryLine = licEntryHeader.AllEntryLines.AddNew();
			licInvLine.JI_CL = licEntryLine.PK;

			licEntryInstruction.EntryHeader.MovementReferenceNumberSetter("TST_LIC", ZDateTime.Now);
			declaration.AttachImportLicense(new[] { new ImportLicenseAttachingObject(licEntryInstruction) });

			var newInvHeader = declaration.Invoices.AddNew();
			newInvHeader.ExchangeHedgeCollection.AddNew();
			var newInvLine = invHeader.InvoiceLines.AddNew();

			var clonedInvoiceHeader = declaration.Invoices.Cast<JobComInvoiceHeader>().FirstOrDefault();
			var newInvoiceHeader = declaration.Invoices.Cast<JobComInvoiceHeader>().LastOrDefault();

			Assert("Cloned ExchangeHedgeCollection should be ReadOnly", clonedInvoiceHeader.ExchangeHedgeCollection.ReadOnly);
			Assert("New ExchangeHedgeCollection should NOT be ReadOnly", !newInvoiceHeader.ExchangeHedgeCollection.ReadOnly);
		}

		protected override Customs.Business.CusSupportingInfoCollection<ExchangeHedge> GetCusSupportingInfoCollection()
		{
			var jobComInvoice = Factory.New<JobDeclaration>().Invoices.AddNew();
			return new ExchangeHedgeCollection(jobComInvoice);
		}
	}
}
