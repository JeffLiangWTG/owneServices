using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusReconEntryNumCollection))]
	sealed class CusReconEntryNumCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAddNewCusEntryNumber()
		{
			AssertEquals("Pre-Condition: entryNum does not exist", false, Declaration.EntryNumbers.Cast<CusEntryNumber>().Any());
			Declaration.EntryNumbers.AddNew();
			var entryNum = Declaration.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault();
			AssertNotNull("EntryNum has been added", entryNum);
			AssertEquals("Country has been set as KR", "KR", entryNum.CE_RN_NKCountryCode);
		}

		public void TestGetOrCreateCusEntryNum()
		{
			AssertEquals("Pre-condition: CusEntryNum does not exist", 0, Declaration.EntryNumbers.Count);

			Declaration.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5UL);
			AssertEquals("CusEntryNum exists", 1, Declaration.EntryNumbers.Count);
			var entryNum = Declaration.EntryNumbers.Cast<CusEntryNumber>().Single();
			AssertEquals(ElectronicDocumentTypeList.Codes._5UL, entryNum.CE_EntryType);
			AssertEquals(Declaration.PK, entryNum.CE_ParentID);
			AssertEquals(Declaration.TableName, entryNum.CE_ParentTable);
		}

		public void TestGetCusEntryNumWithMatchingVersionNumber()
		{
			AssertEquals("Pre-condition: CusEntryNum does not exist", 0, Declaration.EntryNumbers.Count);
			AssertNull(Declaration.EntryNumbers.GetCusEntryNumWithMatchingVersionNumber(ElectronicDocumentTypeList.Codes._5UL, "2"));

			var entryNum = Declaration.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5UL);
			AssertNull(Declaration.EntryNumbers.GetCusEntryNumWithMatchingVersionNumber(ElectronicDocumentTypeList.Codes._5UL, "2"));

			entryNum.CE_EntryLineReference = "2";
			entryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalSent;
			AssertNotNull(Declaration.EntryNumbers.GetCusEntryNumWithMatchingVersionNumber(ElectronicDocumentTypeList.Codes._5UL, "2"));
		}

		public void TestGetCusEntryNumWithMaxVersionNumber()
		{
			AssertEquals("Pre-condition: CusEntryNum does not exist", 0, Declaration.EntryNumbers.Count);
			AssertNull(Declaration.EntryNumbers.GetCusEntryNumWithMaxVersionNumber(ElectronicDocumentTypeList.Codes._5UL));

			var entryNum = Declaration.EntryNumbers.AddNew();
			entryNum.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNum.CE_EntryLineReference = "1";
			AssertEquals(entryNum, Declaration.EntryNumbers.GetCusEntryNumWithMaxVersionNumber(ElectronicDocumentTypeList.Codes._5UL));

			var entryNum2 = Declaration.EntryNumbers.AddNew();
			entryNum2.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNum2.CE_EntryLineReference = "2";
			AssertEquals(entryNum2, Declaration.EntryNumbers.GetCusEntryNumWithMaxVersionNumber(ElectronicDocumentTypeList.Codes._5UL));
		}

		public void TestUpdateCusEntryNumIfExists()
		{
			Declaration.EntryNumbers.UpdateCusEntryNumIfExists(ElectronicDocumentTypeList.Codes._5UL, CusEntryNumber.Schema.CE_IssueDate, new ZDateTime(2021, 1, 1));
			AssertNull("CusEntryNum does not exist", Declaration.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5UL));

			Declaration.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5UL);
			AssertEquals(ZDateTime.Empty, Declaration.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5UL).CE_IssueDate);

			Declaration.EntryNumbers.UpdateCusEntryNumIfExists(ElectronicDocumentTypeList.Codes._5UL, CusEntryNumber.Schema.CE_IssueDate, new ZDateTime(2021, 01, 01));
			AssertEquals(new ZDateTime(2021, 1, 1), Declaration.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5UL).CE_IssueDate);
		}

		public void TestStringIndex()
		{
			var entryNum1 = Declaration.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5UL);
			var entryNum2 = Declaration.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5UO);

			AssertEquals(entryNum1, Declaration.EntryNumbers["\"5UL\""]);
			AssertEquals(entryNum2, Declaration.EntryNumbers["\"5UO\""]);
			AssertNull(Declaration.EntryNumbers["\"AAA\""]);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusReconEntryNumCollection(Declaration);
		}

		CusReconDeclaration Declaration => declaration ??= Factory.New<CusReconDeclaration>();
		CusReconDeclaration declaration;
	}
}
