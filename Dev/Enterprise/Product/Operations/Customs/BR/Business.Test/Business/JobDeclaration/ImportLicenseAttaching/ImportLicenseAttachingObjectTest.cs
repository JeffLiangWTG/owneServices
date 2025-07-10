using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(ImportLicenseAttachingObject))]
	class ImportLicenseAttachingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var date = ZDateTime.Now;
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "Inst-1";
			var invHeader = declaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CEI = entryInstruction.PK;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			invLine.JI_CL = entryLine.PK;

			entryInstruction.EntryHeader.MovementReferenceNumberSetter("TST1", date);

			var importLicenseAttaching = new ImportLicenseAttachingObject((CusEntryInstruction)entryInstruction);
			importLicenseAttaching.ShouldAttach = true;
			importLicenseAttaching.FeeType = "F1ND";
			CombineAssertions(() =>
			{
				AssertEquals("ShouldAttach", true, importLicenseAttaching.ShouldAttach);
				AssertEquals("ImportLicenseEntryDescription", "Inst-1", importLicenseAttaching.ImportLicenseEntryDescription);
				AssertEquals("ImportLicenseEntryMRN", "TST1", importLicenseAttaching.ImportLicenseEntryMRN);
				AssertEquals("ImportLicenseEntryRegistrationDate", date, importLicenseAttaching.ImportLicenseEntryRegistrationDate);
				AssertEquals("ImportLicenseEntryStatusDescription", ZString.Empty, importLicenseAttaching.ImportLicenseEntryStatusDescription);
				AssertEquals("FeeType", "F1ND", importLicenseAttaching.FeeType);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var cusEntryInstruction = Factory.NewWithValidTestData<CusEntryInstruction>();
			return new ImportLicenseAttachingObject(cusEntryInstruction);
		}
	}
}
