using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class CusFiscalReferenceProviderTest : TestCaseWithFactory
	{
		public void TestGetNewValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var cusFiscalReference = entryInstruction.FiscalReferences.AddNew();
			AssertType<EU.Business.Declaration.CusFiscalReferenceValidation>(cusFiscalReference.Validation);

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertType<CusFiscalReferenceValidation>(cusFiscalReference.Validation);
		}

		public void TestGetNewLookups()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var cusFiscalReference = entryInstruction.FiscalReferences.AddNew();
			AssertType<EU.Business.Declaration.CusFiscalReferenceLookups>(cusFiscalReference.Lookups);

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			cusFiscalReference = entryInstruction.FiscalReferences.AddNew();
			AssertType<DeltaIECusFiscalReferenceLookups>(cusFiscalReference.Lookups);
		}

		public void TestRecalculateReferenceIfNeeded()
		{
			var declaration = Factory.New<JobDeclaration>();

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.TVA, "VATFR345", declaration.CountryCode);
			declaration.JE_OH_Importer = importer.PK;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var cusFiscalReference = entryInstruction.FiscalReferences.AddNew();
			cusFiscalReference.CFR_Code = DeltaIEFiscalReferenceCodeList.Codes.FR7;
			AssertEquals("VATFR345", cusFiscalReference.CFR_Reference);
		}
	}
}
