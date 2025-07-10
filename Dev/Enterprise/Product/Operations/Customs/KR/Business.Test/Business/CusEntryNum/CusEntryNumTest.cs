using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusEntryNumber))]
	sealed class CusEntryNumTest : TestCaseWithFactory
	{
		[TestDate(2025, 01, 14)]
		public void TestGenerate5ULEntryNumber()
		{
			var company1 = SetGlbCompany("DKR", "6N002");
			var company2 = SetGlbCompany("TST", "12345");
			var company3 = SetGlbCompany("EMP", "");

			var entryNum1 = Factory.New<CusEntryNumber>();
			entryNum1.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNum1.Generate5ULEntryNumber(company1.PK);
			AssertEquals("6N0022500001U", entryNum1.CE_EntryNum);

			var entryNum2 = Factory.New<CusEntryNumber>();
			entryNum2.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNum2.Generate5ULEntryNumber(company1.PK);
			AssertEquals("6N0022500002U", entryNum2.CE_EntryNum);

			var entryNum_IMP = Factory.New<CusEntryNumber>();
			entryNum_IMP.CE_EntryType = KRJobMessageTypeList.Codes.Import;
			entryNum_IMP.Generate5ULEntryNumber(company1.PK);
			AssertEquals(ZString.Empty, entryNum_IMP.CE_EntryNum);

			var entryNum3 = Factory.New<CusEntryNumber>();
			entryNum3.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNum3.Generate5ULEntryNumber(company2.PK);
			AssertEquals("123452500001U", entryNum3.CE_EntryNum);

			var entryNum4 = Factory.New<CusEntryNumber>();
			entryNum4.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNum4.Generate5ULEntryNumber(company3.PK);
			AssertEquals(ZString.Empty, entryNum4.CE_EntryNum);
		}
		GlbCompany SetGlbCompany(ZString companyCode, string declarantID)
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Name = "Test Company Name " + companyCode;
			company.GC_Code = companyCode;
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, declarantID);
			return company;
		}
	}
}
