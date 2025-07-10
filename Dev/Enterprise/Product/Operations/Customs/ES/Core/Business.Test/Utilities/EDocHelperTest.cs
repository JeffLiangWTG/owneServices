using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class EDocHelperTest : TestCaseWithFactory
	{
		public void TestChangeExistingEDocsFileNamesImport()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.MovementReferenceNumberSetter("21ES00999912345678", ZDateTime.Today);
			entryHeader.SetCSVClearanceNum("AAAAAAAAAAAAAAAA");
			var mrnCode = entryHeader.ReferenceNumber;
			var oldCSVClearance = entryHeader.CSVClearance;

			var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
			var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_AEAT_CLR.pdf", "CLR");
			var eDoc2 = docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_AEAT_CER.pdf", "CAU");
			var eDoc3 = docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_AEAT_M031.pdf", "CAU");

			CombineAssertions(() =>
			{
				var fileDictNames = EDocHelper.FileNamesDictImport(mrnCode, oldCSVClearance);
				EDocHelper.ChangeExistingEDocsFileNames(entryHeader, fileDictNames, entryHeader);

				var eDocs = docManagerInfo.GetRelatedEDocs();
				AssertEquals("Number of eDocs is correct", 3, eDocs.Count());

				var eDocNamesChanged = new List<ZString>() { mrnCode + "_I_AEAT_CLR_OLD_AAAAAAAAAAAAAAAA.pdf", mrnCode + "_I_AEAT_CER_OLD_AAAAAAAAAAAAAAAA.pdf", mrnCode + "_I_AEAT_M031.pdf" };
				AssertContainsExactElementsInAnyOrder("eDocs contains all documents with correct names", eDocNamesChanged, eDocs.Cast<IeDoc>().Select(x => x.FileName).ToList());
			});
		}

		public void TestChangeExistingEDocsFileNamesExport()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.MovementReferenceNumberSetter("21ES00999912345678", ZDateTime.Today);
			entryHeader.SetCSVClearanceNum("AAAAAAAAAAAAAAAA");
			var mrnCode = entryHeader.MovementReferenceNumber;
			var oldCSVClearance = entryHeader.CSVClearance;

			var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
			var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_E_AEAT_CLR.pdf", "CLR");
			var eDoc2 = docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_E_AEAT_t2lf.pdf", "CAU");
			var eDoc3 = docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_E_AEAT_ead.pdf", "CAU");

			CombineAssertions(() =>
			{
				var fileDictNames = EDocHelper.FileNamesDictExport(mrnCode, oldCSVClearance);
				EDocHelper.ChangeExistingEDocsFileNames(entryHeader, fileDictNames, entryHeader);

				var eDocs = docManagerInfo.GetRelatedEDocs();
				AssertEquals("Number of eDocs is correct", 3, eDocs.Count());

				var eDocNamesChanged = new List<ZString>() { mrnCode + "_E_AEAT_CLR_OLD_AAAAAAAAAAAAAAAA.pdf", mrnCode + "_E_AEAT_t2lf_OLD_AAAAAAAAAAAAAAAA.pdf", mrnCode + "_E_AEAT_ead.pdf" };
				AssertContainsExactElementsInAnyOrder("eDocs contains all documents with correct names", eDocNamesChanged, eDocs.Cast<IeDoc>().Select(x => x.FileName).ToList());
			});
		}

		public void TestChangeExistingEDocsFileNamesT2L()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.MovementReferenceNumberSetter("21ES00999912345678", ZDateTime.Today);
			entryHeader.SetCSVClearanceNum("AAAAAAAAAAAAAAAA");
			var mrnCode = entryHeader.MovementReferenceNumber;
			var oldCSVClearance = entryHeader.CSVClearance;

			var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
			var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_E_AEAT_T2L_CLR.pdf", "CLR");

			CombineAssertions(() =>
			{
				var fileDictNames = EDocHelper.FileNamesDictT2LExpeditionAmendment(mrnCode, oldCSVClearance);
				EDocHelper.ChangeExistingEDocsFileNames(entryHeader, fileDictNames, entryHeader);

				var eDocs = docManagerInfo.GetRelatedEDocs();
				AssertEquals("Number of eDocs is correct", 1, eDocs.Count());

				var eDocNamesChanged = new List<ZString>() { mrnCode + "_E_AEAT_T2L_CLR_OLD_AAAAAAAAAAAAAAAA.pdf" };
				AssertContainsExactElementsInAnyOrder("eDocs contains all documents with original names", eDocNamesChanged, eDocs.Cast<IeDoc>().Select(x => x.FileName).ToList());
			});
		}

		public void TestChangeExistingEDocsFileNamesEXS()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.MovementReferenceNumberSetter("21ES00999912345678", ZDateTime.Today);
			entryHeader.SetCSVClearanceNum("AAAAAAAAAAAAAAAA");
			var mrnCode = entryHeader.MovementReferenceNumber;
			var oldCSVClearance = entryHeader.CSVClearance;

			var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
			var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_E_AEAT_EXS_CLR.pdf", "CLR");

			CombineAssertions(() =>
			{
				var fileDictNames = EDocHelper.FileNamesDictEXS(mrnCode, oldCSVClearance);
				EDocHelper.ChangeExistingEDocsFileNames(entryHeader, fileDictNames, entryHeader);

				var eDocs = docManagerInfo.GetRelatedEDocs();
				AssertEquals("Number of eDocs is correct", 1, eDocs.Count());

				var eDocNamesChanged = new List<ZString>() { mrnCode + "_E_AEAT_EXS_CLR_OLD_AAAAAAAAAAAAAAAA.pdf" };
				AssertContainsExactElementsInAnyOrder("eDocs contains all documents with correct names", eDocNamesChanged, eDocs.Cast<IeDoc>().Select(x => x.FileName).ToList());
			});
		}

		public void TestChangeExistingEDocsFileNamesDVD()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.MovementReferenceNumberSetter("21ES00999912345678", ZDateTime.Today);
			entryHeader.SetCSVClearanceNum("AAAAAAAAAAAAAAAA");
			var mrnCode = entryHeader.MovementReferenceNumber;
			var oldCSVClearance = entryHeader.CSVClearance;

			var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
			var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_D_AEAT_CLR.pdf", "CLR");

			CombineAssertions(() =>
			{
				var fileDictNames = EDocHelper.FileNamesDictDVD(mrnCode, oldCSVClearance);
				EDocHelper.ChangeExistingEDocsFileNames(entryHeader, fileDictNames, entryHeader);

				var eDocs = docManagerInfo.GetRelatedEDocs();
				AssertEquals("Number of eDocs is correct", 1, eDocs.Count());

				var eDocNamesChanged = new List<ZString>() { mrnCode + "_D_AEAT_CLR_OLD_AAAAAAAAAAAAAAAA.pdf" };
				AssertContainsExactElementsInAnyOrder("eDocs contains all documents with correct names", eDocNamesChanged, eDocs.Cast<IeDoc>().Select(x => x.FileName).ToList());
			});
		}

		public void TestChangeExistingEDocsFileNamesExportAES()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.MovementReferenceNumberSetter("21ES00999912345678", ZDateTime.Today);
			entryHeader.SetCSVClearanceNum("AAAAAAAAAAAAAAAA");
			var mrnCode = entryHeader.MovementReferenceNumber;
			var oldCSVClearance = entryHeader.CSVClearance;

			var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
			var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_E_AEAT_CLR.pdf", "CLR");
			var eDoc2 = docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_E_AEAT_t2lf.pdf", "CAU");
			var eDoc3 = docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_E_AEAT_CLR_EXT.pdf", "CLR");
			var eDoc4 = docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_E_AEAT_ead.pdf", "EAD");

			CombineAssertions(() =>
			{
				var fileDictNames = EDocHelper.FileNamesDictExportAES(mrnCode, oldCSVClearance);
				EDocHelper.ChangeExistingEDocsFileNames(entryHeader, fileDictNames, entryHeader);

				var eDocs = docManagerInfo.GetRelatedEDocs();
				AssertEquals("Number of eDocs is correct", 4, eDocs.Count());

				var eDocNamesChanged = new List<ZString>() 
											{ mrnCode + "_E_AEAT_CLR_OLD_AAAAAAAAAAAAAAAA.pdf", 
												mrnCode + "_E_AEAT_t2lf_OLD_AAAAAAAAAAAAAAAA.pdf", 
												mrnCode + "_E_AEAT_CLR_EXT_OLD_AAAAAAAAAAAAAAAA.pdf",
												mrnCode + "_E_AEAT_ead_OLD_AAAAAAAAAAAAAAAA.pdf" };
				AssertContainsExactElementsInAnyOrder("eDocs contains all documents with correct names", eDocNamesChanged, eDocs.Cast<IeDoc>().Select(x => x.FileName).ToList());
			});
		}

		public void TestChangeExistingEDocsFileNamesT2C()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.MovementReferenceNumberSetter("21ES00999912345678", ZDateTime.Today);
			entryHeader.SetCSVClearanceNum("AAAAAAAAAAAAAAAA");
			var mrnCode = entryHeader.MovementReferenceNumber;
			var oldCSVClearance = entryHeader.CSVClearance;

			var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
			var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_AEAT_T2L_CLR.pdf", "CLR");

			CombineAssertions(() =>
			{
				var fileDictNames = EDocHelper.FileNamesDictT2C(mrnCode, oldCSVClearance);
				EDocHelper.ChangeExistingEDocsFileNames(entryHeader, fileDictNames, entryHeader);

				var eDocs = docManagerInfo.GetRelatedEDocs();
				AssertEquals("Number of eDocs is correct", 1, eDocs.Count());

				var eDocNamesChanged = new List<ZString>() { mrnCode + "_I_AEAT_T2L_CLR_OLD_AAAAAAAAAAAAAAAA.pdf" };
				AssertContainsExactElementsInAnyOrder("eDocs contains all documents with original names", eDocNamesChanged, eDocs.Cast<IeDoc>().Select(x => x.FileName).ToList());
			});
		}

		public void TestChangeExistingEDocsFileNamesT2LReception()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.MovementReferenceNumberSetter("21ES00999912345678", ZDateTime.Today);
			entryHeader.SetCSVClearanceNum("AAAAAAAAAAAAAAAA");
			var mrnCode = entryHeader.MovementReferenceNumber;
			var oldCSVClearance = entryHeader.CSVClearance;

			var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
			var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_AEAT_T2LR_CLR.pdf", "CLR");

			CombineAssertions(() =>
			{
				var fileDictNames = EDocHelper.FileNamesDictT2LReception(mrnCode, oldCSVClearance);
				EDocHelper.ChangeExistingEDocsFileNames(entryHeader, fileDictNames, entryHeader);

				var eDocs = docManagerInfo.GetRelatedEDocs();
				AssertEquals("Number of eDocs is correct", 1, eDocs.Count());

				var eDocNamesChanged = new List<ZString>() { mrnCode + "_I_AEAT_T2LR_CLR_OLD_AAAAAAAAAAAAAAAA.pdf" };
				AssertContainsExactElementsInAnyOrder("eDocs contains all documents with original names", eDocNamesChanged, eDocs.Cast<IeDoc>().Select(x => x.FileName).ToList());
			});
		}
	}
}
