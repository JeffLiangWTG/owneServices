using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class DFOPGAHeaderAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestProgramCodesList()
		{
			AssertEquals(typeof(DFOPGADepartmentCodes), header.AddInfoLookups.ProgramCodesList.GetType());
		}

		public void TestCategoryList()
		{
			AssertEquals(typeof(DFOProductCategories), header.AddInfoLookups.CategoryList.GetType());
			header.CA_ABIProgramInd = YesNoList.Codes.Yes;
			AssertEquals(12, header.AddInfoLookups.CategoryList.Count);
		}

		public void TestDirectionList()
		{
			AssertEquals(typeof(DFOPGADirections), header.AddInfoLookups.DirectionList.GetType());
		}

		public void TestScientificNames()
		{
			AssertEquals(typeof(DFOScientificNames), header.AddInfoLookups.ScientificNames.GetType());
		}

		public void TestCommonNameCodes()
		{
			AssertEquals(typeof(DFOCommonNameCodes), header.AddInfoLookups.CommonNameCodes.GetType());
		}

		public void TestCommissionLists()
		{
			AssertEquals(typeof(DFOCommissionList), header.AddInfoLookups.CommissionList.GetType());
			header.CA_TTPProgramInd = YesNoList.Codes.Yes;
			AssertEquals(4, header.AddInfoLookups.CommissionList.Count);
		}

		public void TestCountryOfOriginsLookup()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_DFOInd = "Y";
			var header = invoiceLine.DFOPGAHeader;

			AssertEquals(typeof(RefCountryCollection), header.AddInfoLookups.CountryOfOriginsLookup.GetType());
		}

		public void TestThereIsNoExceptionWhenRequirementsParentIsNull()
		{
			AssertNoExceptionThrown(() =>
			{
				_ = header.AddInfoLookups.CountryOfOriginsLookup;
			});
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<DFOPGAHeader>();
		}
		DFOPGAHeader header;

		#endregion
	}
}
