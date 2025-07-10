using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Organisation;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Testing
{
	[TestedType(typeof(NonPersistentOrgHeaderChecker))]
	sealed class NonPersistentOrgHeaderCheckerTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var (orgHeader, _, _) = SetupOrgHeader("54321", "12345");
			return new NonPersistentOrgHeaderChecker(Factory, orgHeader);
		}

		public void TestConstructor()
		{
			var (orgHeader, vatOrgCusCode, eoriOrgCusCode) = SetupOrgHeader("54321", "12345");

			AssertEquals("Pre-Condition VAT", "54321", orgHeader.GetVATRegistrationNumber(Core.Constants.CountryCodes.UnitedKingdom));
			AssertEquals("Pre-Condition EORI", "GB12345", orgHeader.GetEuIdentificationNumber(Core.Constants.CountryCodes.UnitedKingdom));

			var checker = new NonPersistentOrgHeaderChecker(Factory, orgHeader);

			AssertEquals("OrgHeader", orgHeader, checker.OrgHeader);
			AssertEquals("VAT OrgCusCode", vatOrgCusCode, checker.VatOrgCusCode);
			AssertEquals("VAT", "54321", checker.VAT);
			AssertEquals("Verify VAT", true, checker.VerifyVAT);
			AssertEquals("EORI OrgCusCode", eoriOrgCusCode, checker.EoriOrgCusCode);
			AssertEquals("EORI", "GB12345", checker.EORI);
			AssertEquals("Verify EORI", true, checker.VerifyEORI);
			AssertEquals("Valid for query", true, checker.IsValid);
			AssertContainsExactElementsInExactOrder("Included VAT and EORI for query", [checker.VatOrgCusCode, checker.EoriOrgCusCode], checker.CodesForQuery);
		}

		public void TestConstructor_Null()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new NonPersistentOrgHeaderChecker(Factory, null));
		}

		public void TestConstructor_WhenOrgCusCodesAreMissing()
		{
			var (orgHeader, _, _) = SetupOrgHeader();

			AssertEquals("Pre-Condition VAT", string.Empty, orgHeader.GetVATRegistrationNumber(Core.Constants.CountryCodes.UnitedKingdom));
			AssertEquals("Pre-Condition EORI", string.Empty, orgHeader.GetEuIdentificationNumber(Core.Constants.CountryCodes.UnitedKingdom));

			var checker = new NonPersistentOrgHeaderChecker(Factory, orgHeader);

			AssertEquals("OrgHeader", orgHeader, checker.OrgHeader);
			AssertNull("VAT OrgCusCode", checker.VatOrgCusCode);
			AssertEquals("VAT", string.Empty, checker.VAT);
			AssertEquals("Verify VAT", false, checker.VerifyVAT);
			AssertNull("EORI OrgCusCode", checker.EoriOrgCusCode);
			AssertEquals("EORI", string.Empty, checker.EORI);
			AssertEquals("Verify EORI", false, checker.VerifyEORI);
			AssertEquals("Invalid for query", false, checker.IsValid);
			AssertEquals("No code for query", 0, checker.CodesForQuery.Length);
		}

		(OrgHeader OrgHeader, OrgCusCode VATOrgCusCode, OrgCusCode EORIOrgCusCode) SetupOrgHeader(string vat = null, string eori = null)
		{
			var orgHeader = Factory.New<OrgHeader>();

			OrgCusCode vatOrgCusCode = null;

			if (vat != null)
			{
				vatOrgCusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, vat, Core.Constants.CountryCodes.UnitedKingdom);
			}

			OrgCusCode eoriOrgCusCode = null;

			if (eori != null)
			{
				eoriOrgCusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eori, Core.Constants.CountryCodes.UnitedKingdom);
			}

			return (orgHeader, vatOrgCusCode, eoriOrgCusCode);
		}
	}
}
