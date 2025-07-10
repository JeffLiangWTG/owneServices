using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class CusGuaranteeHeaderHelperTest : TestCaseWithFactory
	{
		public void TestLoadCusGuaranteeHeaderFromReference()
		{
			var guarantee1 = CreateGuarantee(Factory, "REF1", "ES", "IMP");
			var guarantee2 = CreateGuarantee(Factory, "REF1", "ES", "IMP");
			var guarantee3 = CreateGuarantee(Factory, "REF2", "ES", "IMP");
			var guarantee4 = CreateGuarantee(Factory, "REF1", "FR", "IMP");
			var guarantee5 = CreateGuarantee(Factory, "REF1", "ES", "TRA");
			var newFactory = new BusinessObjectFactory();
			var guarantee6 = CreateGuarantee(newFactory, "REF3", "ES", "IMP");

			var permit = Factory.NewWithValidTestData<CusPermitHeader>();
			permit.CPH_Number = "REF4";
			permit.CPH_Type = "IMP";
			permit.CPH_RN_NKCountryCode = "ES";

			CombineAssertions(() =>
			{
				AssertNotNull("When there is at least one CusGuaranteeheader with the expected values the first one is returned", CusGuaranteeHeaderHelper.LoadCusGuaranteeHeaderFromReference(Factory, "REF1", "ES", "IMP"));

				AssertEquals("When there is only one CusGuaranteeheader with the expected values it is returned (guarantee3)", guarantee3, CusGuaranteeHeaderHelper.LoadCusGuaranteeHeaderFromReference(Factory, "REF2", "ES", "IMP"));

				AssertEquals("When there is only one CusGuaranteeheader with the expected values it is returned (guarantee3)", guarantee4, CusGuaranteeHeaderHelper.LoadCusGuaranteeHeaderFromReference(Factory, "REF1", "FR", "IMP"));

				AssertEquals("When there is only one CusGuaranteeheader with the expected values it is returned (guarantee3)", guarantee5, CusGuaranteeHeaderHelper.LoadCusGuaranteeHeaderFromReference(Factory, "REF1", "ES", "TRA"));

				AssertNull("When there is no CusGuaranteeheader with the expected values method returns null (wrong factory)", CusGuaranteeHeaderHelper.LoadCusGuaranteeHeaderFromReference(Factory, "REF3", "ES", "IMP"));

				AssertNull("When there is no CusGuaranteeheader with the expected values method returns null (wrong CPH_ApplicationCode)", CusGuaranteeHeaderHelper.LoadCusGuaranteeHeaderFromReference(Factory, "REF4", "ES", "IMP"));
			});
		}
		public void TestLoadCusGuaranteeHeadersFromReferenceWithOBLTransaction()
		{
			var guarantee1 = CreateGuarantee(Factory, "REF1", "ES", "IMP", addOBLTransaction: true);
			var guarantee2 = CreateGuarantee(Factory, "REF1", "ES", "IMP", addOBLTransaction: true);
			var guarantee3 = CreateGuarantee(Factory, "REF1", "ES", "IMP", addOBLTransaction: true, withOldEndDate: true);
			var guarantee4 = CreateGuarantee(Factory, "REF1", "ES", "IMP", addOBLTransaction: true, withFutureStartDate: true);
			var guarantee5 = CreateGuarantee(Factory, "REF2", "ES", "IMP", addOBLTransaction: true);
			var guarantee6 = CreateGuarantee(Factory, "REF1", "FR", "IMP");
			var guarantee7 = CreateGuarantee(Factory, "REF1", "ES", "TRA", addOBLTransaction: true);
			var newFactory = new BusinessObjectFactory();
			var guarantee8 = CreateGuarantee(newFactory, "REF3", "ES", "IMP", addOBLTransaction: true);

			var permit = Factory.NewWithValidTestData<CusPermitHeader>();
			permit.CPH_Number = "REF4";
			permit.CPH_Type = "IMP";
			permit.CPH_RN_NKCountryCode = "ES";

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("There are 2 CusGuaranteeHeaders with OBL and correct dates with the REF1, ES, IMP", new CusGuaranteeHeader[] { guarantee1, guarantee2 }, CusGuaranteeHeaderHelper.LoadCusGuaranteeHeadersFromReferenceWithOBLTransaction(Factory, "REF1", "ES", "IMP"));

				AssertContainsExactElementsInAnyOrder("There is 1 CusGuaranteeHeader with OBL and correct dates with the REF2, ES, IMP", new CusGuaranteeHeader[] { guarantee5 }, CusGuaranteeHeaderHelper.LoadCusGuaranteeHeadersFromReferenceWithOBLTransaction(Factory, "REF2", "ES", "IMP"));

				AssertEquals("There are no CusGuaranteeHeader with OBL and correct dates with the REF1, FR, IMP", 0, CusGuaranteeHeaderHelper.LoadCusGuaranteeHeadersFromReferenceWithOBLTransaction(Factory, "REF1", "FR", "IMP").Length);

				AssertContainsExactElementsInAnyOrder("There is 1 CusGuaranteeHeader with OBL and correct dates with the REF1, ES, TRA", new CusGuaranteeHeader[] { guarantee7 }, CusGuaranteeHeaderHelper.LoadCusGuaranteeHeadersFromReferenceWithOBLTransaction(Factory, "REF1", "ES", "TRA"));

				AssertEquals("There are no CusGuaranteeHeader with OBL and correct dates with the REF3, ES, IMP (wrong factory)", 0, CusGuaranteeHeaderHelper.LoadCusGuaranteeHeadersFromReferenceWithOBLTransaction(Factory, "REF3", "ES", "IMP").Length);

				AssertEquals("There are no CusGuaranteeHeader with OBL and correct dates with the REF4, FR, IMP (wrong CPH_ApplicationCode)", 0, CusGuaranteeHeaderHelper.LoadCusGuaranteeHeadersFromReferenceWithOBLTransaction(Factory, "REF4", "ES", "IMP").Length);
			});
		}

		CusGuaranteeHeader CreateGuarantee(BusinessObjectFactory factory, string reference, string countryCode, string type, bool addOBLTransaction = false, bool withFutureStartDate = false, bool withOldEndDate = false)
		{
			var guaranteeHeader = factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Number = reference;
			guaranteeHeader.CPH_Type = type;
			guaranteeHeader.CPH_RN_NKCountryCode = countryCode;
			guaranteeHeader.CPH_StartDate = withFutureStartDate ? ZDate.Today.AddDays(10) : ZDate.Today.AddDays(-10);
			if (addOBLTransaction)
			{
				AddOBLTransaction(guaranteeHeader, 1000m);
			}

			if (withOldEndDate)
			{
				guaranteeHeader.CPH_EndDate = ZDate.Today.AddDays(-10);
			}
			return guaranteeHeader;
		}

		void AddOBLTransaction(CusGuaranteeHeader guaranteeHeader, ZDecimal value)
		{
			var transaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transaction.CPL_Reference = "OPENING";
			transaction.CPL_TranValue = value;
			transaction.CPL_Comment = "OPENING";
			transaction.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
		}
	}
}
