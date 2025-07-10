using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	public class CusFiscalReferenceProviderTest : TestCaseWithFactory
	{
		public void TestRecalculateReferenceIfNeeded()
		{
			var org = Factory.New<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "TST");
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "123456789", GlbBranch.CurrentBranch.Country);
			var orgAddress = org.Addresses.AddNew();
			cusFiscalReference.CFR_OA_Owner = orgAddress.PK;
			cusFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;

			AssertEquals("Fiscal Reference is Eori", "GBTST", cusFiscalReference.CFR_Reference);
		}

		public void TestRecalculateReferenceIfNeeded_NoOrg()
		{
			var org = Factory.New<OrgHeader>();
			var orgAddress = org.Addresses.AddNew();
			cusFiscalReference.CFR_OA_Owner = orgAddress.PK;
			cusFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;

			AssertEquals("Fiscal Reference", ZString.Empty, cusFiscalReference.CFR_Reference);

			cusFiscalReference.CFR_Reference = "GBTEST";
			cusFiscalReference.CFR_OA_Owner = Guid.Empty;

			AssertEquals("Fiscal Reference", "GBTEST", cusFiscalReference.CFR_Reference);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;

			var cei = declaration.CustomsEntryInstructions.AddNew();
			cusFiscalReference = cei.FiscalReferences.AddNew();
		}
		CusFiscalReference cusFiscalReference;
	}
}
