using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CUSRESMessageHelper))]
	sealed class CUSRESMessageHelperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestBGM()
		{
			var cusres = Factory.New<B3Message>();
			cusres.EM_MessageText = CCSEntryResponseMessage;
			var testHelper = CUSRESMessageHelper.New(cusres);
			AssertEquals("BT00001", testHelper.DocumentMessageName);
			AssertEquals(ZString.Empty, testHelper.UniqueReferenceNumber);

			cusres.EM_MessageText = CCSEntryFunctionalExceptionMessage;
			testHelper = CUSRESMessageHelper.New(cusres);
			AssertEquals("B9999999", testHelper.DocumentMessageName);
			AssertEquals(ZString.Empty, testHelper.UniqueReferenceNumber);

			cusres.EM_MessageText = EmptyCCSEntryResponseMessage;
			testHelper = CUSRESMessageHelper.New(cusres);
			AssertEquals(ZString.Empty, testHelper.DocumentMessageName);
			AssertEquals(ZString.Empty, testHelper.UniqueReferenceNumber);

			cusres.EM_MessageText = EmptyCCSEntryFunctionalExceptionMessage;
			testHelper = CUSRESMessageHelper.New(cusres);
			AssertEquals(ZString.Empty, testHelper.DocumentMessageName);
			AssertEquals(ZString.Empty, testHelper.UniqueReferenceNumber);
		}

		public void TestDTM()
		{
			var cusres = Factory.New<B3Message>();
			cusres.EM_MessageText = CCSEntryResponseMessage;
			var testHelper = CUSRESMessageHelper.New(cusres);
			AssertEquals(ZDateTime.Invalid, testHelper.DocumentMessageDateTime);
			AssertEquals(new ZDateTime(2018, 10, 19), testHelper.DocumentMessageDate);

			cusres.EM_MessageText = CCSEntryFunctionalExceptionMessage;
			testHelper = CUSRESMessageHelper.New(cusres);
			AssertEquals(new ZDateTime(2018, 10, 19, 13, 10, 00), testHelper.DocumentMessageDateTime);
			AssertEquals(ZDateTime.Invalid, testHelper.DocumentMessageDate);

			cusres.EM_MessageText = EmptyCCSEntryResponseMessage;
			testHelper = CUSRESMessageHelper.New(cusres);
			AssertEquals(ZDateTime.Invalid, testHelper.DocumentMessageDateTime);
			AssertEquals(new ZDateTime(2018, 10, 19), testHelper.DocumentMessageDate);

			cusres.EM_MessageText = EmptyCCSEntryFunctionalExceptionMessage;
			testHelper = CUSRESMessageHelper.New(cusres);
			AssertEquals(new ZDateTime(2018, 10, 19, 13, 10, 00), testHelper.DocumentMessageDateTime);
			AssertEquals(ZDateTime.Invalid, testHelper.DocumentMessageDate);
		}

		public void TestFreeTextErrors()
		{
			var cusres = Factory.New<B3Message>();
			cusres.EM_MessageText = CCSEntryFunctionalExceptionMessage;
			var testHelper = CUSRESMessageHelper.New(cusres);
			AssertEquals(2, testHelper.FreeTextErrors.Count);

			cusres.EM_MessageText = EmptyCCSEntryFunctionalExceptionMessage;
			testHelper = CUSRESMessageHelper.New(cusres);
			AssertEquals(1, testHelper.FreeTextErrors.Count);
		}

		public void TestDetailSectionMessages()
		{
			var cusres = Factory.New<EDIMessage>();
			cusres.EM_MessageText = CCSEntryResponseMessage;
			var testHelper = CUSRESMessageHelper.New(cusres);
			AssertEquals(2, testHelper.DetailSectionMessages.Count);

			cusres.EM_MessageText = EmptyCCSEntryResponseMessage;
			testHelper = CUSRESMessageHelper.New(cusres);
			AssertEquals(1, testHelper.DetailSectionMessages.Count);
		}

		public void TestAccountSecurityNumbers()
		{
			var cusres = Factory.New<EDIMessage>();
			cusres.EM_MessageText = CCSEntryResponseMessage;
			var testHelper = CUSRESMessageHelper.New(cusres);
			AssertEquals(1, testHelper.AccountSecurityNumbers.Count);
			AssertEquals("102070000125487", testHelper.AccountSecurityNumbers[0]);

			cusres.EM_MessageText = EmptyCCSEntryResponseMessage;
			testHelper = CUSRESMessageHelper.New(cusres);
			AssertEquals(1, testHelper.AccountSecurityNumbers.Count);
			AssertEquals(ZString.Empty, testHelper.AccountSecurityNumbers[0]);
		}

		public void TestApplicableReferenceNumbers()
		{
			var cusres = Factory.New<EDIMessage>();
			cusres.EM_MessageText = CCSEntryResponseMessage;
			var testHelper = CUSRESMessageHelper.New(cusres);
			AssertEquals(2, testHelper.ApplicableReferenceNumbers.Count);
			AssertContainsExactElementsInAnyOrder(new List<ZString> { "AN00001", "AN00002" }, testHelper.ApplicableReferenceNumbers);

			cusres.EM_MessageText = EmptyCCSEntryResponseMessage;
			testHelper = CUSRESMessageHelper.New(cusres);
			AssertEquals(1, testHelper.ApplicableReferenceNumbers.Count);
		}

		public void TestApplicationErrorInformations()
		{
			var cusres = Factory.New<EDIMessage>();
			cusres.EM_MessageText = CCSEntryResponseMessage;
			var testHelper = CUSRESMessageHelper.New(cusres);
			AssertEquals(2, testHelper.ApplicationErrorInformations.Count);

			cusres.EM_MessageText = EmptyCCSEntryResponseMessage;
			testHelper = CUSRESMessageHelper.New(cusres);
			AssertEquals(1, testHelper.ApplicationErrorInformations.Count);
		}

		public void TestTotalNumbers()
		{
			var cusres = Factory.New<EDIMessage>();
			cusres.EM_MessageText = CCSEntryResponseMessage;
			var testHelper = CUSRESMessageHelper.New(cusres);
			AssertEquals(18, testHelper.TotalNumberOfTransactions);
			AssertEquals(20, testHelper.NumberOfValidTransactions);
			AssertEquals(35, testHelper.NumberOfInValidTransactions);

			cusres.EM_MessageText = EmptyCCSEntryResponseMessage;
			testHelper = CUSRESMessageHelper.New(cusres);
			AssertEquals(0, testHelper.TotalNumberOfTransactions);
			AssertEquals(0, testHelper.NumberOfValidTransactions);
			AssertEquals(0, testHelper.NumberOfInValidTransactions);
		}

		const string CCSEntryResponseMessage = "UNH+1+CUSRES:S:99B:UN'BGM+:::BT00001++9'DTM+137:20181019:102'RFF+ABP:102070000125487'ERP+:IN00001'ERP+:IN00002'RFF+ABO:AN00001'RFF+ABO:AN00002'ERC+EMN00001'ERC+EMN00002'DOC+961'CST++18+20+35";

		const string CCSEntryFunctionalExceptionMessage = "UNH+1+CUSRES:S:99B:UN'BGM+:::B9999999++11'DTM+137:201810191310:203'GIS+14'ERP+2:1:29'FTX+AAO+++FREE TEXT 11:FREE TEXT 12'FTX+AAO+++FREE TEXT 21:FREE TEXT 22";

		const string EmptyCCSEntryResponseMessage = "UNH+1+CUSRES:S:99B:UN'BGM+++9'DTM+137:20181019:102'RFF+ABP'ERP'RFF+ABO'ERC'DOC+961'CST++0+0+0";

		const string EmptyCCSEntryFunctionalExceptionMessage = "UNH+1+CUSRES:S:99B:UN'BGM+++11'DTM+137:201810191310:203'GIS+14'ERP+2:1:29'FTX+AAO";

		protected override BusinessObject GetNewBusinessObject()
		{
			var testMessage = Factory.NewWithValidTestData<EDIMessage>();
			testMessage.EM_MessageText = CCSEntryResponseMessage;
			return CUSRESMessageHelper.New(testMessage);
		}
	}
}
