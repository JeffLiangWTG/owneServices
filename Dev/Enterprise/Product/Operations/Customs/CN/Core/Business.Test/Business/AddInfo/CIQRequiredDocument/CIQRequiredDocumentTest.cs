using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CIQRequiredDocument))]
	class CIQRequiredDocumentTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<CIQRequiredDocument>
	{
		public void TestSupportsNotes()
		{
			AssertEquals("SupportsNotes should be false", false, ((CIQRequiredDocument)BusinessObject).SupportsNotes);
		}

		public void TestXC_DocumentTypeChangesAndReadonlys()
		{
			var testItem = Factory.New<JobDeclaration>().CustomsEntryInstructions.AddNew().CIQRequiredDocuments.AddNew();
			testItem.XC_NumberOfOriginals = 1;
			testItem.XC_NumberOfCopies = 2;
			testItem.XC_DocumentType = "20";
			AssertEquals(new ZInt(1), testItem.XC_NumberOfOriginals);
			AssertEquals(new ZInt(2), testItem.XC_NumberOfCopies);
			Assert(!testItem.XC_NumberOfOriginalsInfo.ReadOnly);
			Assert(!testItem.XC_NumberOfCopiesInfo.ReadOnly);
			testItem.XC_DocumentType = "24";
			AssertEquals(ZInt.Zero, testItem.XC_NumberOfOriginals);
			AssertEquals(ZInt.Zero, testItem.XC_NumberOfCopies);
			Assert(testItem.XC_NumberOfOriginalsInfo.ReadOnly);
			Assert(testItem.XC_NumberOfCopiesInfo.ReadOnly);
		}

		public void TestXC_DocumentName()
		{
			var testItem = Factory.New<JobDeclaration>().CustomsEntryInstructions.AddNew().CIQRequiredDocuments.AddNew();
			testItem.XC_DocumentType = "13";
			AssertEquals("数量证书", testItem.XC_DocumentName);
			testItem.XC_DocumentType = "17";
			AssertEquals("动物卫生证书", testItem.XC_DocumentName);
			testItem.XC_DocumentType = "XX";
			AssertEquals(ZString.Empty, testItem.XC_DocumentName);
		}

		public void TestRequestOfNotIssuing()
		{
			var testItem = Factory.New<JobDeclaration>().CustomsEntryInstructions.AddNew().CIQRequiredDocuments.AddNew();
			testItem.XC_DocumentType = "20";
			Assert(!testItem.RequestOfNotIssuing);
			testItem.XC_DocumentType = "XX";
			Assert(!testItem.RequestOfNotIssuing);
			testItem.XC_DocumentType = "24";
			Assert(testItem.RequestOfNotIssuing);
		}

		protected override IEnumerable<CIQRequiredDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;

			yield return declaration.CustomsEntryInstructions.AddNew().CIQRequiredDocuments.AddNew();
		}
	}
}
