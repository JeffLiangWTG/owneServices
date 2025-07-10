using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(ECWCCMHeaderProvider))]
	sealed class ECWCCMHeaderProviderTest : ImportHeaderProviderAbstractTest<ECWCCMHeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ECWCCMHeaderProvider(null));
		}

		public void TestCustomsAuthorisationCurrentProcedure()
		{
			var orgHeader = Factory.New<OrgHeader>();
			CombineAssertions(() =>
			{
				var cusAuthorisationHeader = orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "NUMBER1");
				var address = orgHeader.Addresses.AddNew();
				declaration.JE_OA_DeclarantAddress = address.PK;
				AssertEquals("CW1-Authorisation is valid", "NUMBER1", Provider.CustomsAuthorisationCurrentProcedure);

				cusAuthorisationHeader.CPH_EndDate = ZDate.Today.AddDays(-1);
				orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "NUMBER2");
				var newProvider = GetProvider();
				AssertEquals("CW1-Authorisation is out of date, CWP-Authorisation is valid", "NUMBER2", newProvider.CustomsAuthorisationCurrentProcedure);
			});
		}

		public void TestRepresentativeRelationshipFlag()
		{
			CombineAssertions(() =>
			{
				declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
				AssertEquals("JE_DeclarantType is 'SEL'", "0", Provider.RepresentativeRelationshipFlag);

				declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
				AssertEquals("JE_DeclarantType is 'DIR'", "1", Provider.RepresentativeRelationshipFlag);

				declaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
				AssertEquals("JE_DeclarantType is 'IND'", string.Empty, Provider.RepresentativeRelationshipFlag);
			});
		}

		public void TestCustomsAuthorisationOwner()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			var declarantAddress = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1");
			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
			var authorisationOwner = Provider.CustomsAuthorisationOwner;
			CombineAssertions(() =>
			{
				AssertEquals("EoriNumber", "GREOR1", authorisationOwner.EoriNumber);
				AssertEquals("EoriBranchSuffix", "EBS1", authorisationOwner.EoriBranchSuffix);
			});
		}

		public void TestCustomsAuthorisationOwner_Null()
		{
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			declaration.Branch.GB_OH_OrgProxy = ZGuid.Empty;
			AssertNull(Provider.CustomsAuthorisationOwner);
		}

		public void TestRepresentative_JE_DeclarantTypeIsDIR()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			var representativeAddress = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1");
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			declaration.JE_OA_Representative = representativeAddress.PK;
			var representative = Provider.Representative;
			CombineAssertions(() =>
			{
				AssertEquals("EoriNumber", "GREOR1", representative.EoriNumber);
				AssertEquals("EoriBranchSuffix", "EBS1", representative.EoriBranchSuffix);
			});
		}

		public void TestRepresentative_JE_DeclarantTypeIsDIR_Null()
		{
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			declaration.JE_OA_Representative = ZGuid.Empty;
			AssertNull(Provider.Representative);
		}

		public void TestRepresentative_JE_DeclarantTypeIsNotDIR()
		{
			var representativeAddress = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1");
			declaration.JE_OA_Representative = representativeAddress.PK;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			AssertNull(Provider.Representative);
		}

		public void TestLines()
		{
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryHeader.MergedLines.AddNew().PK;
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryHeader.MergedLines.AddNew().PK;
			AssertEquals(2, Provider.Lines.Count);
		}

		protected override IEnumerable<Expression<Func<ECWCCMHeaderProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.CustomsAuthorisationCurrentProcedure;
			yield return x => x.CustomsAuthorisationOwner;
			yield return x => x.Representative;
			yield return x => x.Lines;
		}

		protected override ECWCCMHeaderProvider GetProvider() => new ECWCCMHeaderProvider(entryHeader);
	}
}
