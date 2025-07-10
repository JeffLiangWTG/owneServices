using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.CDS.Declaration.Testing
{
	class CusFiscalReferenceProviderTest : Business.Declaration.Testing.CusFiscalReferenceProviderTest
	{
		public new void TestRecalculateReferenceIfNeeded()
		{
			var org = Factory.New<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "TST");
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "123456789", GlbBranch.CurrentBranch.Country);

			var orgAddress = org.Addresses.AddNew();
			cusFiscalReference.CFR_OA_Owner = orgAddress.PK;
			cusFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;

			AssertEquals("Fiscal Reference is VAT", "GB123456789", cusFiscalReference.CFR_Reference);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			var cei = declaration.CustomsEntryInstructions.AddNew();
			cusFiscalReference = cei.FiscalReferences.AddNew();
		}
		CusFiscalReference cusFiscalReference;
	}
}
