using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusEntryNumCollection))]
	sealed class CusEntryNumCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAddNewCusEntryNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("Pre-Condition: entryNum does not exist", false, entryHeader.EntryNumbers.Cast<CusEntryNumber>().Any());
			entryHeader.EntryNumbers.AddNew();
			var entryNum = entryHeader.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault();
			AssertNotNull("EntryNum has been added", entryNum);
			AssertEquals("Country has been set as KR", "KR", entryNum.CE_RN_NKCountryCode);
		}
		public void TestChangeIssueDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryNum = entryHeader.EntryNumbers.AddNew();
			entryNum.CE_EntryType = "EXP";

			entryNum.CE_IssueDate = new ZDateTime(2022, 10, 10);
			AssertEquals(entryNum.CE_IssueDate, declaration.GetEntryIssueDate(entryHeader.PK));

			entryNum.CE_IssueDate = ZDateTime.Today;
			AssertEquals(entryNum.CE_IssueDate, declaration.GetEntryIssueDate(entryHeader.PK));
		}

		public void TestCusEntryNumberParent()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("Pre-Condition: entryNum does not exist", false, entryHeader.EntryNumbers.Cast<CusEntryNumber>().Any());
			entryHeader.EntryNumbers.AddNew();
			var entryNum = entryHeader.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault();
			AssertEquals(entryHeader, entryNum.Parent);
			AssertNull(entryHeader.CusEntryNumber);

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(entryHeader.RegistryCompanyPK, Guid.Empty, Guid.Empty, "6N002");
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var entryLoaded = factory2.Load<CusEntryHeader>(entryHeader.PK);
			AssertEquals(2, entryLoaded.EntryNumbers.Count);
			AssertEquals(entryLoaded, entryLoaded.EntryNumbers[0].Parent);
			AssertEquals(entryLoaded, entryLoaded.EntryNumbers[1].Parent);
			AssertNotNull(entryHeader.CusEntryNumber);
		}

		public void TestGetOrCreateCusEntryNum()
		{
			var entry = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			AssertEquals("Pre-condition: CusEntryNum does not exist", 0, entry.EntryNumbers.Count);

			entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._008);
			AssertEquals("CusEntryNum exists", 1, entry.EntryNumbers.Count);
			var entryNum = entry.EntryNumbers.Cast<CusEntryNumber>().Single();
			AssertEquals(ElectronicDocumentTypeList.Codes._008, entryNum.CE_EntryType);
			AssertEquals(entry.PK, entryNum.CE_ParentID);
			AssertEquals(entry.TableName, entryNum.CE_ParentTable);
		}

		public void TestGetCusEntryNumWithMatchingVersionNumber()
		{
			var entry = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			AssertEquals("Pre-condition: CusEntryNum does not exist", 0, entry.EntryNumbers.Count);
			AssertNull(entry.EntryNumbers.GetCusEntryNumWithMatchingVersionNumber(ElectronicDocumentTypeList.Codes._5UA, "2"));

			var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5UA);
			AssertNull(entry.EntryNumbers.GetCusEntryNumWithMatchingVersionNumber(ElectronicDocumentTypeList.Codes._5UA, "2"));

			entryNum.CE_EntryLineReference = "2";
			entryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalSent;
			AssertNotNull(entry.EntryNumbers.GetCusEntryNumWithMatchingVersionNumber(ElectronicDocumentTypeList.Codes._5UA, "2"));
		}

		public void TestGetCusEntryNumWithMaxVersionNumber()
		{
			var entry = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			AssertEquals("Pre-condition: CusEntryNum does not exist", 0, entry.EntryNumbers.Count);
			AssertNull(entry.EntryNumbers.GetCusEntryNumWithMaxVersionNumber(ElectronicDocumentTypeList.Codes._5UA));

			var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5UA);
			entryNum.CE_EntryLineReference = "1";
			AssertEquals(entryNum, entry.EntryNumbers.GetCusEntryNumWithMaxVersionNumber(ElectronicDocumentTypeList.Codes._5UA));

			var entryNum2 = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5UA);
			entryNum2.CE_EntryLineReference = "2";
			AssertEquals(entryNum2, entry.EntryNumbers.GetCusEntryNumWithMaxVersionNumber(ElectronicDocumentTypeList.Codes._5UA));
		}

		public void TestUpdateCusEntryNumIfExists()
		{
			var entry = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();

			entry.EntryNumbers.UpdateCusEntryNumIfExists(ElectronicDocumentTypeList.Codes._008, CusEntryNumber.Schema.CE_IssueDate, new ZDateTime(2021, 1, 1));
			AssertNull("CusEntryNum does not exist", entry.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._008));

			entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._008);
			AssertEquals(ZDateTime.Empty, entry.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._008).CE_IssueDate);

			entry.EntryNumbers.UpdateCusEntryNumIfExists(ElectronicDocumentTypeList.Codes._008, CusEntryNumber.Schema.CE_IssueDate, new ZDateTime(2021, 01, 01));
			AssertEquals(new ZDateTime(2021, 1, 1), entry.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._008).CE_IssueDate);
		}

		public void TestStringIndex()
		{
			var entry = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			var entryNum1 = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._008);
			var entryNum2 = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5AC);

			AssertEquals(entryNum1, entry.EntryNumbers["\"008\""]);
			AssertEquals(entryNum2, entry.EntryNumbers["\"5AC\""]);
			AssertNull(entry.EntryNumbers["\"AAA\""]);
		}
		public void TestEntryNumStatusDescription()
		{
			var entry = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5BD);

			entryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalSent;
			AssertEquals(CustomsMessageStatusTypeList.Descriptions.OriginalSent, entryNum.EntryStatusDescription);

			entryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			AssertEquals(CustomsMessageStatusTypeList.Descriptions.OriginalAccepted, entryNum.EntryStatusDescription);
		}
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusEntryNumCollection(Entry);
		}

		CusEntryHeader Entry => entry ?? (entry = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew());
		CusEntryHeader entry;
	}
}
