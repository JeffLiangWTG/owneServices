using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CFIAPGAHeaderAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestProperties()
		{
			AssertEquals(typeof(CACFIAMiscCodesCollection), cfiaPGAHeader.AddInfoLookups.AirsMiscellaneous.GetType());
			AssertEquals(typeof(CACFIAEndUseCodesCollection), cfiaPGAHeader.AddInfoLookups.EndUseCodes.GetType());
		}

		public void TestPGAIndicatorList()
		{
			AssertEquals(typeof(YesNoList), cfiaPGAHeader.AddInfoLookups.PGAIndicatorList.GetType());
		}

		public void TestProgramCodesList()
		{
			AssertEquals(typeof(CFIAPGADepartmentCodes), cfiaPGAHeader.AddInfoLookups.ProgramCodesList.GetType());
		}

		public void TestCountryOfSourceLookupAndCountryOfSourceStateLookup()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_CFIAInd = "Y";
			var header = invoiceLine.CFIAPGAHeader;

			AssertEquals(typeof(RefCountryCollection), header.AddInfoLookups.CountryOfSourceLookup.GetType());
			AssertEquals(typeof(USStatesList), header.AddInfoLookups.CountryOfSourceStateLookup.GetType());
		}

		public void TestThereIsNoExceptionWhenRequirementsParentIsNull()
		{
			AssertNoExceptionThrown(() =>
			{
				_ = cfiaPGAHeader.AddInfoLookups.CountryOfSourceLookup;
				_ = cfiaPGAHeader.AddInfoLookups.CountryOfSourceStateLookup;
			});
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			cfiaPGAHeader = Factory.New<CFIAPGAHeader>();
		}
		CFIAPGAHeader cfiaPGAHeader;

		#endregion
	}
}
