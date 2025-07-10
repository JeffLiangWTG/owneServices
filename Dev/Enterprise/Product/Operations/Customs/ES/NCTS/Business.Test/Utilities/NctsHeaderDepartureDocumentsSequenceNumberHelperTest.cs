using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NctsHeaderDepartureDocumentsSequenceNumberHelperTest : TestCaseWithFactory
	{
		public void TestAssignSupportingDocumentsSequenceNumbers_TransitionPeriod_NoCustomsStatus()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
			{
				var doc1 = goodsItem11.SupportingDocuments.AddNew();
				doc1.CSI_Code = "9001";

				var doc2 = goodsItem11.SupportingDocuments.AddNew();
				doc2.CSI_Code = "A002";
				doc2.CSI_LineNo = 20;

				var doc3 = goodsItem12.SupportingDocuments.AddNew();
				doc3.CSI_Code = "9003";

				var doc4 = goodsItem21.SupportingDocuments.AddNew();
				doc4.CSI_Code = "Y004";

				var doc5 = nctsBill1.SupportingDocuments.AddNew();
				doc5.CSI_Code = "9005";

				var doc6 = nctsBill1.SupportingDocuments.AddNew();
				doc6.CSI_Code = "C006";

				var doc7 = nctsBill2.SupportingDocuments.AddNew();
				doc7.CSI_Code = "9007";
				doc7.CSI_LineNo = 20;

				var doc8 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				doc8.CSI_Code = "A008";

				var doc9 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				doc9.CSI_Code = "9009";

				var doc10 = nctsBill1.SupportingDocuments.AddNew();
				doc10.CSI_Code = ZString.Empty;

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: doc1 has CSI_LineNo 0", 0, doc1.CSI_LineNo);
					AssertEquals("Prereq: doc2 has CSI_LineNo 20", 20, doc2.CSI_LineNo);
					AssertEquals("Prereq: doc3 has CSI_LineNo 0", 0, doc3.CSI_LineNo);
					AssertEquals("Prereq: doc4 has CSI_LineNo 0", 0, doc4.CSI_LineNo);
					AssertEquals("Prereq: doc5 has CSI_LineNo 0", 0, doc5.CSI_LineNo);
					AssertEquals("Prereq: doc6 has CSI_LineNo 0", 0, doc6.CSI_LineNo);
					AssertEquals("Prereq: doc7 has CSI_LineNo 20", 20, doc7.CSI_LineNo);
					AssertEquals("Prereq: doc8 has CSI_LineNo 0", 0, doc8.CSI_LineNo);
					AssertEquals("Prereq: doc9 has CSI_LineNo 0", 0, doc9.CSI_LineNo);
					AssertEquals("Prereq: doc10 has CSI_LineNo 0", 0, doc10.CSI_LineNo);

					AssertContainsExactElementsInAnyOrder("Prereq: nctsHeader has 2 documents with codes", new ZString[] { "A008", "9009" }, nctsHeader.MovementHeader.SupportingDocuments.Select(x => x.CSI_Code));
					AssertContainsExactElementsInAnyOrder("Prereq: nctsBill1 has 2 documents with codes", new ZString[] { "9005", "C006", ZString.Empty }, nctsBill1.SupportingDocuments.Select(x => x.CSI_Code));
					AssertContainsExactElementsInAnyOrder("Prereq: nctsBill2 has 1 document with code", new ZString[] { "9007" }, nctsBill2.SupportingDocuments.Select(x => x.CSI_Code));
					AssertContainsExactElementsInAnyOrder("Prereq: goodsItem11 has 2 documents with codes", new ZString[] { "9001", "A002" }, goodsItem11.SupportingDocuments.Select(x => x.CSI_Code));
					AssertContainsExactElementsInAnyOrder("Prereq: goodsItem12 has 1 document with code", new ZString[] { "9003" }, goodsItem12.SupportingDocuments.Select(x => x.CSI_Code));
					AssertContainsExactElementsInAnyOrder("Prereq: goodsItem21 has 1 document with code", new ZString[] { "Y004" }, goodsItem21.SupportingDocuments.Select(x => x.CSI_Code));

					NctsHeaderDocumentsSequenceNumberHelper.AssignSupportingDocumentsSequenceNumbers(nctsHeader);

					AssertEquals("doc5 is deleted because it was in bill", true, doc5.IsDeleted);
					AssertEquals("doc6 is deleted because it was in bill", true, doc6.IsDeleted);
					AssertEquals("doc7 is deleted because it was in bill", true, doc7.IsDeleted);
					AssertEquals("doc10 is deleted because it was in bill", true, doc10.IsDeleted);
					AssertEquals("doc8 is deleted because it was in header", true, doc8.IsDeleted);
					AssertEquals("doc9 is deleted because it was in header", true, doc9.IsDeleted);

					AssertEquals("When declaration is Departure Phase5 Header has 0 documents", 0, nctsHeader.MovementHeader.SupportingDocuments.Count);
					AssertEquals("When declaration is Departure Phase5 nctsBill1 has 0 documents", 0, nctsBill1.SupportingDocuments.Count);
					AssertEquals("When declaration is Departure Phase5 nctsBill2 has 0 documents", 0, nctsBill2.SupportingDocuments.Count);
					var goodsItem11SupDocs = goodsItem11.SupportingDocuments;
					AssertContainsExactElementsInExactOrder("When declaration is Departure Phase5 goodsItem11 has 6 documents with codes", new ZString[] { ZString.Empty, "A008", "C006", "A002", "9009", "9005", "9001" }, goodsItem11SupDocs.OrderBy(x => x.CSI_LineNo).Select(x => x.CSI_Code));
					AssertContainsExactElementsInAnyOrder("When declaration is Departure Phase5 goodsItem11 has documents with LineNo set correctly", new ZInt[] { 1, 2, 3, 4, 5, 6, 7 }, goodsItem11SupDocs.Select(x => x.CSI_LineNo));
					var goodsItem12SupDocs = goodsItem12.SupportingDocuments;
					AssertContainsExactElementsInExactOrder("When declaration is Departure Phase5 goodsItem12 has 5 document with code", new ZString[] { ZString.Empty, "A008", "C006", "9009", "9005", "9003" }, goodsItem12SupDocs.OrderBy(x => x.CSI_LineNo).Select(x => x.CSI_Code));
					AssertContainsExactElementsInAnyOrder("When declaration is Departure Phase5 goodsItem12 has documents with LineNo set correctly", new ZInt[] { 1, 2, 3, 4, 5, 6 }, goodsItem12SupDocs.Select(x => x.CSI_LineNo));
					var goodsItem21SupDocs = goodsItem21.SupportingDocuments;
					AssertContainsExactElementsInExactOrder("When declaration is Departure Phase5 goodsItem21 has 3 document with code", new ZString[] { "A008", "Y004", "9009", "9007" }, goodsItem21SupDocs.OrderBy(x => x.CSI_LineNo).Select(x => x.CSI_Code));
					AssertContainsExactElementsInAnyOrder("When declaration is Departure Phase5 goodsItem21 has documents with LineNo set correctly", new ZInt[] { 1, 2, 3, 4 }, goodsItem21SupDocs.Select(x => x.CSI_LineNo));
				});
			}
		}

		public void TestAssignSupportingDocumentsSequenceNumbers_TransitionPeriod_WithCustomsStatus()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
			{
				var doc1 = goodsItem11.SupportingDocuments.AddNew();
				doc1.CSI_Code = "9001";

				var doc2 = goodsItem11.SupportingDocuments.AddNew();
				doc2.CSI_Code = "A002";

				var doc3 = goodsItem12.SupportingDocuments.AddNew();
				doc3.CSI_Code = "9003";

				var doc4 = goodsItem21.SupportingDocuments.AddNew();
				doc4.CSI_Code = "Y004";

				var doc5 = nctsBill1.SupportingDocuments.AddNew();
				doc5.CSI_Code = "9005";

				var doc6 = nctsBill1.SupportingDocuments.AddNew();
				doc6.CSI_Code = "C006";

				var doc7 = nctsBill2.SupportingDocuments.AddNew();
				doc7.CSI_Code = "9007";

				var doc8 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				doc8.CSI_Code = "A008";

				var doc9 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				doc9.CSI_Code = "9009";

				var doc10 = nctsBill1.SupportingDocuments.AddNew();
				doc10.CSI_Code = ZString.Empty;

				Factory.Save();

				doc2.CSI_LineNo = 20;
				doc7.CSI_LineNo = 20;
				nctsHeader.MovementHeader.BM_CustomsStatus = "AAA";

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: doc1 has CSI_LineNo 0", 0, doc1.CSI_LineNo);
					AssertEquals("Prereq: doc2 has CSI_LineNo 20", 20, doc2.CSI_LineNo);
					AssertEquals("Prereq: doc3 has CSI_LineNo 0", 0, doc3.CSI_LineNo);
					AssertEquals("Prereq: doc4 has CSI_LineNo 0", 0, doc4.CSI_LineNo);
					AssertEquals("Prereq: doc5 has CSI_LineNo 0", 0, doc5.CSI_LineNo);
					AssertEquals("Prereq: doc6 has CSI_LineNo 0", 0, doc6.CSI_LineNo);
					AssertEquals("Prereq: doc7 has CSI_LineNo 20", 20, doc7.CSI_LineNo);
					AssertEquals("Prereq: doc8 has CSI_LineNo 0", 0, doc8.CSI_LineNo);
					AssertEquals("Prereq: doc9 has CSI_LineNo 0", 0, doc9.CSI_LineNo);
					AssertEquals("Prereq: doc10 has CSI_LineNo 0", 0, doc10.CSI_LineNo);

					AssertContainsExactElementsInAnyOrder("Prereq: nctsHeader has 2 documents with codes", new ZString[] { "A008", "9009" }, nctsHeader.MovementHeader.SupportingDocuments.Select(x => x.CSI_Code));
					AssertContainsExactElementsInAnyOrder("Prereq: nctsBill1 has 2 documents with codes", new ZString[] { "9005", "C006" , ZString.Empty }, nctsBill1.SupportingDocuments.Select(x => x.CSI_Code));
					AssertContainsExactElementsInAnyOrder("Prereq: nctsBill2 has 1 document with code", new ZString[] { "9007" }, nctsBill2.SupportingDocuments.Select(x => x.CSI_Code));
					AssertContainsExactElementsInAnyOrder("Prereq: goodsItem11 has 2 documents with codes", new ZString[] { "9001", "A002" }, goodsItem11.SupportingDocuments.Select(x => x.CSI_Code));
					AssertContainsExactElementsInAnyOrder("Prereq: goodsItem12 has 1 document with code", new ZString[] { "9003" }, goodsItem12.SupportingDocuments.Select(x => x.CSI_Code));
					AssertContainsExactElementsInAnyOrder("Prereq: goodsItem21 has 1 document with code", new ZString[] { "Y004" }, goodsItem21.SupportingDocuments.Select(x => x.CSI_Code));

					NctsHeaderDocumentsSequenceNumberHelper.AssignSupportingDocumentsSequenceNumbers(nctsHeader);

					AssertEquals("doc5 is deleted because it was in bill", true, doc5.IsDeleted);
					AssertEquals("doc6 is deleted because it was in bill", true, doc6.IsDeleted);
					AssertEquals("doc7 is deleted because it was in bill", true, doc7.IsDeleted);
					AssertEquals("doc10 is deleted because it was in bill", true, doc10.IsDeleted);
					AssertEquals("doc8 is deleted because it was in header", true, doc8.IsDeleted);
					AssertEquals("doc9 is deleted because it was in header", true, doc9.IsDeleted);

					AssertEquals("When declaration is Departure Phase5 Header has 0 documents", 0, nctsHeader.MovementHeader.SupportingDocuments.Count);
					AssertEquals("When declaration is Departure Phase5 nctsBill1 has 0 documents", 0, nctsBill1.SupportingDocuments.Count);
					AssertEquals("When declaration is Departure Phase5 nctsBill2 has 0 documents", 0, nctsBill2.SupportingDocuments.Count);
					var goodsItem11SupDocs = goodsItem11.SupportingDocuments;
					AssertContainsExactElementsInExactOrder("When declaration is Departure Phase5 goodsItem11 has 6 documents with codes", new ZString[] { ZString.Empty, "A008", "C006", "A002", "9009", "9005", "9001" }, goodsItem11SupDocs.OrderBy(x => x.CSI_LineNo).Select(x => x.CSI_Code));
					AssertContainsExactElementsInAnyOrder("When declaration is Departure Phase5 goodsItem11 has documents with LineNo set correctly", new ZInt[] { 1, 2, 3, 4, 5, 6, 7 }, goodsItem11SupDocs.Select(x => x.CSI_LineNo));
					var goodsItem12SupDocs = goodsItem12.SupportingDocuments;
					AssertContainsExactElementsInExactOrder("When declaration is Departure Phase5 goodsItem12 has 5 document with code", new ZString[] { ZString.Empty, "A008", "C006", "9009", "9005", "9003" }, goodsItem12SupDocs.OrderBy(x => x.CSI_LineNo).Select(x => x.CSI_Code));
					AssertContainsExactElementsInAnyOrder("When declaration is Departure Phase5 goodsItem12 has documents with LineNo set correctly", new ZInt[] { 1, 2, 3, 4, 5, 6 }, goodsItem12SupDocs.Select(x => x.CSI_LineNo));
					var goodsItem21SupDocs = goodsItem21.SupportingDocuments;
					AssertContainsExactElementsInExactOrder("When declaration is Departure Phase5 goodsItem21 has 3 document with code", new ZString[] { "A008", "Y004", "9009", "9007" }, goodsItem21SupDocs.OrderBy(x => x.CSI_LineNo).Select(x => x.CSI_Code));
					AssertContainsExactElementsInAnyOrder("When declaration is Departure Phase5 goodsItem21 has documents with LineNo set correctly", new ZInt[] { 1, 2, 3, 4 }, goodsItem21SupDocs.Select(x => x.CSI_LineNo));
				});
			}
		}

		public void TestAssignSupportingDocumentsSequenceNumbers_NoTransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
			{
				var doc1 = goodsItem11.SupportingDocuments.AddNew();
				doc1.CSI_Code = "9001";

				var doc2 = goodsItem11.SupportingDocuments.AddNew();
				doc2.CSI_Code = "A002";

				var doc3 = goodsItem12.SupportingDocuments.AddNew();
				doc3.CSI_Code = "9003";

				var doc4 = goodsItem21.SupportingDocuments.AddNew();
				doc4.CSI_Code = "Y004";

				var doc5 = nctsBill1.SupportingDocuments.AddNew();
				doc5.CSI_Code = "9005";

				var doc6 = nctsBill1.SupportingDocuments.AddNew();
				doc6.CSI_Code = "C006";

				var doc7 = nctsBill2.SupportingDocuments.AddNew();
				doc7.CSI_Code = "9007";

				var doc8 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				doc8.CSI_Code = "A008";

				var doc9 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				doc9.CSI_Code = "9009";

				CombineAssertions(() =>
				{
					AssertContainsExactElementsInAnyOrder("Prereq: documents in Header have sequences 1 and 2", new ZInt[] { 1, 2 }, new ZInt[] { doc8.CSI_LineNo, doc9.CSI_LineNo });
					AssertContainsExactElementsInAnyOrder("Prereq: documents in nctsBill1 have sequences 1 and 2", new ZInt[] { 1, 2 }, new ZInt[] { doc6.CSI_LineNo, doc5.CSI_LineNo });
					AssertContainsExactElementsInAnyOrder("Prereq: documents in nctsBill1.goodsItem11 have sequences 1 and 2", new ZInt[] { 1, 2 }, new ZInt[] { doc2.CSI_LineNo, doc1.CSI_LineNo });
					AssertEquals("Prereq: doc3 has CSI_LineNo 1", 1, doc3.CSI_LineNo);
					AssertEquals("Prereq: doc4 has CSI_LineNo 1", 1, doc4.CSI_LineNo);
					AssertEquals("Prereq: doc7 has CSI_LineNo 1", 1, doc7.CSI_LineNo);

					AssertContainsExactElementsInAnyOrder("Prereq: nctsHeader has 2 documents with codes", new ZString[] { "A008", "9009" }, nctsHeader.MovementHeader.SupportingDocuments.Select(x => x.CSI_Code));
					AssertContainsExactElementsInAnyOrder("Prereq: nctsBill1 has 2 documents with codes", new ZString[] { "9005", "C006" }, nctsBill1.SupportingDocuments.Select(x => x.CSI_Code));
					AssertContainsExactElementsInAnyOrder("Prereq: nctsBill2 has 1 document with code", new ZString[] { "9007" }, nctsBill2.SupportingDocuments.Select(x => x.CSI_Code));
					AssertContainsExactElementsInAnyOrder("Prereq: goodsItem11 has 2 documents with codes", new ZString[] { "9001", "A002" }, goodsItem11.SupportingDocuments.Select(x => x.CSI_Code));
					AssertContainsExactElementsInAnyOrder("Prereq: goodsItem12 has 1 document with code", new ZString[] { "9003" }, goodsItem12.SupportingDocuments.Select(x => x.CSI_Code));
					AssertContainsExactElementsInAnyOrder("Prereq: goodsItem21 has 1 document with code", new ZString[] { "Y004" }, goodsItem21.SupportingDocuments.Select(x => x.CSI_Code));

					NctsHeaderDocumentsSequenceNumberHelper.AssignSupportingDocumentsSequenceNumbers(nctsHeader);

					AssertContainsExactElementsInAnyOrder("After, nothing is changed: documents in Header have sequences 1 and 2", new ZInt[] { 1, 2 }, new ZInt[] { doc8.CSI_LineNo, doc9.CSI_LineNo });
					AssertContainsExactElementsInAnyOrder("After, nothing is changed: documents in nctsBill1 have sequences 1 and 2", new ZInt[] { 1, 2 }, new ZInt[] { doc6.CSI_LineNo, doc5.CSI_LineNo });
					AssertContainsExactElementsInAnyOrder("After, nothing is changed: documents in nctsBill1.goodsItem11 have sequences 1 and 2", new ZInt[] { 1, 2 }, new ZInt[] { doc2.CSI_LineNo, doc1.CSI_LineNo });
					AssertEquals("After, nothing is changed: doc3 has CSI_LineNo 1", 1, doc3.CSI_LineNo);
					AssertEquals("After, nothing is changed: doc4 has CSI_LineNo 1", 1, doc4.CSI_LineNo);
					AssertEquals("After, nothing is changed: doc7 has CSI_LineNo 1", 1, doc7.CSI_LineNo);

					AssertContainsExactElementsInAnyOrder("After, nothing is changed: nctsHeader has 2 documents with codes", new ZString[] { "A008", "9009" }, nctsHeader.MovementHeader.SupportingDocuments.Select(x => x.CSI_Code));
					AssertContainsExactElementsInAnyOrder("After, nothing is changed: nctsBill1 has 2 documents with codes", new ZString[] { "9005", "C006" }, nctsBill1.SupportingDocuments.Select(x => x.CSI_Code));
					AssertContainsExactElementsInAnyOrder("After, nothing is changed: nctsBill2 has 1 document with code", new ZString[] { "9007" }, nctsBill2.SupportingDocuments.Select(x => x.CSI_Code));
					AssertContainsExactElementsInAnyOrder("After, nothing is changed: goodsItem11 has 2 documents with codes", new ZString[] { "9001", "A002" }, goodsItem11.SupportingDocuments.Select(x => x.CSI_Code));
					AssertContainsExactElementsInAnyOrder("After, nothing is changed: goodsItem12 has 1 document with code", new ZString[] { "9003" }, goodsItem12.SupportingDocuments.Select(x => x.CSI_Code));
					AssertContainsExactElementsInAnyOrder("After, nothing is changed: goodsItem21 has 1 document with code", new ZString[] { "Y004" }, goodsItem21.SupportingDocuments.Select(x => x.CSI_Code));
				});
			}
		}

		public void TestAssignAdditionalDocumentsSequenceNumbers_TransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
			{
				var doc1 = goodsItem11.AdditionalInfos.AddNew();
				doc1.CSI_Code = "9001";
				doc1.CSI_SubType = "INF";

				var doc2 = goodsItem11.AdditionalInfos.AddNew();
				doc2.CSI_Code = "9002";
				doc2.CSI_SubType = "REF";

				var doc3 = goodsItem11.AdditionalInfos.AddNew();
				doc3.CSI_Code = "9003";
				doc3.CSI_SubType = "TRA";

				var doc4 = goodsItem11.AdditionalInfos.AddNew();
				doc4.CSI_Code = "9004";
				doc4.CSI_SubType = "INF";
				doc4.CSI_LineNo = 20;

				var doc5 = goodsItem11.AdditionalInfos.AddNew();
				doc5.CSI_Code = "9005";
				doc5.CSI_SubType = "REF";
				doc5.CSI_LineNo = 20;

				var doc6 = goodsItem11.AdditionalInfos.AddNew();
				doc6.CSI_Code = "9006";
				doc6.CSI_SubType = "TRA";
				doc6.CSI_LineNo = 20;

				var doc7 = nctsBill1.AdditionalDocuments.AddNew();
				doc7.CSI_Code = "9007";
				doc7.CSI_SubType = "INF";
				doc7.CSI_LineNo = 20;

				var doc8 = nctsBill1.AdditionalDocuments.AddNew();
				doc8.CSI_Code = "9008";
				doc8.CSI_SubType = "REF";
				doc8.CSI_LineNo = 20;

				var doc9 = nctsBill1.AdditionalDocuments.AddNew();
				doc9.CSI_Code = "9009";
				doc9.CSI_SubType = "TRA";
				doc9.CSI_LineNo = 20;

				var doc10 = nctsBill1.AdditionalDocuments.AddNew();
				doc10.CSI_Code = "9010";
				doc10.CSI_SubType = "INF";

				var doc11 = nctsBill1.AdditionalDocuments.AddNew();
				doc11.CSI_Code = "9011";
				doc11.CSI_SubType = "REF";

				var doc12 = nctsBill1.AdditionalDocuments.AddNew();
				doc12.CSI_Code = "9012";
				doc12.CSI_SubType = "TRA";

				var doc13 = nctsHeader.AdditionalDocuments.AddNew();
				doc13.CSI_Code = "9013";
				doc13.CSI_SubType = "INF";

				var doc14 = nctsHeader.AdditionalDocuments.AddNew();
				doc14.CSI_Code = "9014";
				doc14.CSI_SubType = "REF";

				var doc15 = nctsHeader.AdditionalDocuments.AddNew();
				doc15.CSI_Code = "9015";
				doc15.CSI_SubType = "TRA";

				var doc16 = nctsHeader.AdditionalDocuments.AddNew();
				doc16.CSI_Code = "9016";
				doc16.CSI_SubType = "INF";

				var doc17 = nctsHeader.AdditionalDocuments.AddNew();
				doc17.CSI_Code = "9017";
				doc17.CSI_SubType = "REF";

				var doc18 = nctsHeader.AdditionalDocuments.AddNew();
				doc18.CSI_Code = "9018";
				doc18.CSI_SubType = "TRA";

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: doc1 has CSI_LineNo 0", 0, doc1.CSI_LineNo);
					AssertEquals("Prereq: doc2 has CSI_LineNo 0", 0, doc2.CSI_LineNo);
					AssertEquals("Prereq: doc3 has CSI_LineNo 0", 0, doc3.CSI_LineNo);
					AssertEquals("Prereq: doc4 has CSI_LineNo 20", 20, doc4.CSI_LineNo);
					AssertEquals("Prereq: doc5 has CSI_LineNo 20", 20, doc5.CSI_LineNo);
					AssertEquals("Prereq: doc6 has CSI_LineNo 20", 20, doc6.CSI_LineNo);
					AssertEquals("Prereq: doc7 has CSI_LineNo 20", 20, doc7.CSI_LineNo);
					AssertEquals("Prereq: doc8 has CSI_LineNo 20", 20, doc8.CSI_LineNo);
					AssertEquals("Prereq: doc9 has CSI_LineNo 20", 20, doc9.CSI_LineNo);
					AssertEquals("Prereq: doc10 has CSI_LineNo 0", 0, doc10.CSI_LineNo);
					AssertEquals("Prereq: doc11 has CSI_LineNo 0", 0, doc11.CSI_LineNo);
					AssertEquals("Prereq: doc12 has CSI_LineNo 0", 0, doc12.CSI_LineNo);
					AssertEquals("Prereq: doc13 has CSI_LineNo 0", 0, doc13.CSI_LineNo);
					AssertEquals("Prereq: doc14 has CSI_LineNo 0", 0, doc14.CSI_LineNo);
					AssertEquals("Prereq: doc15 has CSI_LineNo 0", 0, doc15.CSI_LineNo);
					AssertEquals("Prereq: doc16 has CSI_LineNo 0", 0, doc16.CSI_LineNo);
					AssertEquals("Prereq: doc17 has CSI_LineNo 0", 0, doc17.CSI_LineNo);
					AssertEquals("Prereq: doc18 has CSI_LineNo 0", 0, doc18.CSI_LineNo);

					NctsHeaderDocumentsSequenceNumberHelper.AssignAdditionalDocumentsSequenceNumbers(nctsHeader);

					AssertContainsExactElementsInAnyOrder("When declaration is Departure Phase5 documents in Header for INF have sequences 1 and 2", new ZInt[] { 1, 2 }, new ZInt[] { doc16.CSI_LineNo, doc13.CSI_LineNo });
					AssertContainsExactElementsInAnyOrder("When declaration is Departure Phase5 documents in Header for INF have sequences 1 and 2", new ZInt[] { 1, 2 }, new ZInt[] { doc17.CSI_LineNo, doc14.CSI_LineNo });
					AssertContainsExactElementsInAnyOrder("When declaration is Departure Phase5 documents in Header for INF have sequences 1 and 2", new ZInt[] { 1, 2 }, new ZInt[] { doc18.CSI_LineNo, doc15.CSI_LineNo });
					AssertContainsExactElementsInAnyOrder("When declaration is Departure Phase5 documents in nctsBill1 for INF have sequences 3 and 4", new ZInt[] { 3, 4 }, new ZInt[] { doc10.CSI_LineNo, doc7.CSI_LineNo });
					AssertContainsExactElementsInAnyOrder("When declaration is Departure Phase5 documents in nctsBill1 for REF have sequences 3 and 4", new ZInt[] { 3, 4 }, new ZInt[] { doc11.CSI_LineNo, doc8.CSI_LineNo });
					AssertContainsExactElementsInAnyOrder("When declaration is Departure Phase5 documents in nctsBill1 for TRA have sequences 3 and 4", new ZInt[] { 3, 4 }, new ZInt[] { doc12.CSI_LineNo, doc9.CSI_LineNo });
					AssertContainsExactElementsInAnyOrder("When declaration is Departure Phase5 documents in nctsBill1.GoodsItems for INF have sequences 5 and 6", new ZInt[] { 5, 6 }, new ZInt[] { doc4.CSI_LineNo, doc1.CSI_LineNo });
					AssertContainsExactElementsInAnyOrder("When declaration is Departure Phase5 documents in nctsBill1.GoodsItems for REF have sequences 5 and 6", new ZInt[] { 5, 6 }, new ZInt[] { doc5.CSI_LineNo, doc2.CSI_LineNo });
					AssertContainsExactElementsInAnyOrder("When declaration is Departure Phase5 documents in nctsBill1.GoodsItems for TRA have sequences 5 and 6", new ZInt[] { 5, 6 }, new ZInt[] { doc6.CSI_LineNo, doc3.CSI_LineNo });
				});
			}
		}

		public void TestAssignAdditionalDocumentsSequenceNumbers_NoTransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
			{
				var doc1 = goodsItem11.AdditionalInfos.AddNew();
				doc1.CSI_Code = "9001";
				doc1.CSI_SubType = "INF";

				var doc2 = goodsItem11.AdditionalInfos.AddNew();
				doc2.CSI_Code = "9002";
				doc2.CSI_SubType = "REF";

				var doc3 = goodsItem11.AdditionalInfos.AddNew();
				doc3.CSI_Code = "9003";
				doc3.CSI_SubType = "TRA";

				Factory.Save();

				var doc4 = goodsItem11.AdditionalInfos.AddNew();
				doc4.CSI_Code = "9004";
				doc4.CSI_SubType = "INF";

				var doc5 = goodsItem11.AdditionalInfos.AddNew();
				doc5.CSI_Code = "9005";
				doc5.CSI_SubType = "REF";

				var doc6 = goodsItem11.AdditionalInfos.AddNew();
				doc6.CSI_Code = "9006";
				doc6.CSI_SubType = "TRA";

				var doc7 = nctsBill1.AdditionalDocuments.AddNew();
				doc7.CSI_Code = "9007";
				doc7.CSI_SubType = "INF";

				var doc8 = nctsBill1.AdditionalDocuments.AddNew();
				doc8.CSI_Code = "9008";
				doc8.CSI_SubType = "REF";

				var doc9 = nctsBill1.AdditionalDocuments.AddNew();
				doc9.CSI_Code = "9009";
				doc9.CSI_SubType = "TRA";

				var doc10 = nctsBill1.AdditionalDocuments.AddNew();
				doc10.CSI_Code = "9010";
				doc10.CSI_SubType = "INF";

				var doc11 = nctsBill1.AdditionalDocuments.AddNew();
				doc11.CSI_Code = "9011";
				doc11.CSI_SubType = "REF";

				var doc12 = nctsBill1.AdditionalDocuments.AddNew();
				doc12.CSI_Code = "9012";
				doc12.CSI_SubType = "TRA";

				var doc13 = nctsHeader.AdditionalDocuments.AddNew();
				doc13.CSI_Code = "9013";
				doc13.CSI_SubType = "INF";

				var doc14 = nctsHeader.AdditionalDocuments.AddNew();
				doc14.CSI_Code = "9014";
				doc14.CSI_SubType = "REF";

				var doc15 = nctsHeader.AdditionalDocuments.AddNew();
				doc15.CSI_Code = "9015";
				doc15.CSI_SubType = "TRA";

				var doc16 = nctsHeader.AdditionalDocuments.AddNew();
				doc16.CSI_Code = "9016";
				doc16.CSI_SubType = "INF";

				var doc17 = nctsHeader.AdditionalDocuments.AddNew();
				doc17.CSI_Code = "9017";
				doc17.CSI_SubType = "REF";

				var doc18 = nctsHeader.AdditionalDocuments.AddNew();
				doc18.CSI_Code = "9018";
				doc18.CSI_SubType = "TRA";

				CombineAssertions(() =>
				{
					AssertContainsExactElementsInAnyOrder("Prereq: documents in Header for INF have sequences 1 and 2", new ZInt[] { 1, 2 }, new ZInt[] { doc16.CSI_LineNo, doc13.CSI_LineNo });
					AssertContainsExactElementsInAnyOrder("Prereq: documents in Header for REF have sequences 1 and 2", new ZInt[] { 1, 2 }, new ZInt[] { doc17.CSI_LineNo, doc14.CSI_LineNo });
					AssertContainsExactElementsInAnyOrder("Prereq: documents in Header for TRA have sequences 1 and 2", new ZInt[] { 1, 2 }, new ZInt[] { doc18.CSI_LineNo, doc15.CSI_LineNo });
					AssertContainsExactElementsInAnyOrder("Prereq: documents in nctsBill1 for INF have sequences 1 and 2", new ZInt[] { 1, 2 }, new ZInt[] { doc10.CSI_LineNo, doc7.CSI_LineNo });
					AssertContainsExactElementsInAnyOrder("Prereq: documents in nctsBill1 for REF have sequences 1 and 2", new ZInt[] { 1, 2 }, new ZInt[] { doc11.CSI_LineNo, doc8.CSI_LineNo });
					AssertContainsExactElementsInAnyOrder("Prereq: documents in nctsBill1 for TRA have sequences 1 and 2", new ZInt[] { 1, 2 }, new ZInt[] { doc12.CSI_LineNo, doc9.CSI_LineNo });
					AssertContainsExactElementsInAnyOrder("Prereq: documents in nctsBill1.GoodsItems for INF have sequences 1 and 2", new ZInt[] { 1, 2 }, new ZInt[] { doc4.CSI_LineNo, doc1.CSI_LineNo });
					AssertContainsExactElementsInAnyOrder("Prereq: documents in nctsBill1.GoodsItems for REF have sequences 1 and 2", new ZInt[] { 1, 2 }, new ZInt[] { doc5.CSI_LineNo, doc2.CSI_LineNo });
					AssertContainsExactElementsInAnyOrder("Prereq: documents in nctsBill1.GoodsItems for TRA have sequences 1 and 2", new ZInt[] { 1, 2 }, new ZInt[] { doc6.CSI_LineNo, doc3.CSI_LineNo });

					NctsHeaderDocumentsSequenceNumberHelper.AssignAdditionalDocumentsSequenceNumbers(nctsHeader);

					AssertContainsExactElementsInAnyOrder("After, nothing is changed: documents in Header for INF have sequences 1 and 2", new ZInt[] { 1, 2 }, new ZInt[] { doc16.CSI_LineNo, doc13.CSI_LineNo });
					AssertContainsExactElementsInAnyOrder("After, nothing is changed: documents in Header for REF have sequences 1 and 2", new ZInt[] { 1, 2 }, new ZInt[] { doc17.CSI_LineNo, doc14.CSI_LineNo });
					AssertContainsExactElementsInAnyOrder("After, nothing is changed: documents in Header for TRA have sequences 1 and 2", new ZInt[] { 1, 2 }, new ZInt[] { doc18.CSI_LineNo, doc15.CSI_LineNo });
					AssertContainsExactElementsInAnyOrder("After, nothing is changed: documents in nctsBill1 for INF have sequences 1 and 2", new ZInt[] { 1, 2 }, new ZInt[] { doc10.CSI_LineNo, doc7.CSI_LineNo });
					AssertContainsExactElementsInAnyOrder("After, nothing is changed: documents in nctsBill1 for REF have sequences 1 and 2", new ZInt[] { 1, 2 }, new ZInt[] { doc11.CSI_LineNo, doc8.CSI_LineNo });
					AssertContainsExactElementsInAnyOrder("After, nothing is changed: documents in nctsBill1 for TRA have sequences 1 and 2", new ZInt[] { 1, 2 }, new ZInt[] { doc12.CSI_LineNo, doc9.CSI_LineNo });
					AssertContainsExactElementsInAnyOrder("After, nothing is changed: documents in nctsBill1.GoodsItems for INF have sequences 1 and 2", new ZInt[] { 1, 2 }, new ZInt[] { doc4.CSI_LineNo, doc1.CSI_LineNo });
					AssertContainsExactElementsInAnyOrder("After, nothing is changed: documents in nctsBill1.GoodsItems for REF have sequences 1 and 2", new ZInt[] { 1, 2 }, new ZInt[] { doc5.CSI_LineNo, doc2.CSI_LineNo });
					AssertContainsExactElementsInAnyOrder("After, nothing is changed: documents in nctsBill1.GoodsItems for TRA have sequences 1 and 2", new ZInt[] { 1, 2 }, new ZInt[] { doc6.CSI_LineNo, doc3.CSI_LineNo });
				});
			}
		}

		public void TestAssignAdditionalDocumentsSequenceNumbers_TransitionPeriod_OnlyINF()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
			{
				var doc1 = goodsItem11.AdditionalInfos.AddNew();
				doc1.CSI_Code = "9001";
				doc1.CSI_SubType = "INF";

				var doc2 = goodsItem11.AdditionalInfos.AddNew();
				doc2.CSI_Code = "9002";
				doc2.CSI_SubType = "INF";
				doc2.CSI_LineNo = 20;

				var doc3 = goodsItem12.AdditionalInfos.AddNew();
				doc3.CSI_Code = "9003";
				doc3.CSI_SubType = "INF";

				var doc4 = goodsItem21.AdditionalInfos.AddNew();
				doc4.CSI_Code = "9004";
				doc4.CSI_SubType = "INF";

				var doc5 = nctsBill1.AdditionalDocuments.AddNew();
				doc5.CSI_Code = "9005";
				doc5.CSI_SubType = "INF";

				var doc6 = nctsBill1.AdditionalDocuments.AddNew();
				doc6.CSI_Code = "9006";
				doc6.CSI_SubType = "INF";

				var doc7 = nctsBill2.AdditionalDocuments.AddNew();
				doc7.CSI_Code = "9007";
				doc7.CSI_SubType = "INF";
				doc7.CSI_LineNo = 20;

				var doc8 = nctsHeader.AdditionalDocuments.AddNew();
				doc8.CSI_Code = "9008";
				doc8.CSI_SubType = "INF";

				var doc9 = nctsHeader.AdditionalDocuments.AddNew();
				doc9.CSI_Code = "9009";
				doc9.CSI_SubType = "INF";

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: doc1 has CSI_LineNo 0", 0, doc1.CSI_LineNo);
					AssertEquals("Prereq: doc2 has CSI_LineNo 20", 20, doc2.CSI_LineNo);
					AssertEquals("Prereq: doc3 has CSI_LineNo 0", 0, doc3.CSI_LineNo);
					AssertEquals("Prereq: doc4 has CSI_LineNo 0", 0, doc4.CSI_LineNo);
					AssertEquals("Prereq: doc5 has CSI_LineNo 0", 0, doc5.CSI_LineNo);
					AssertEquals("Prereq: doc6 has CSI_LineNo 0", 0, doc6.CSI_LineNo);
					AssertEquals("Prereq: doc7 has CSI_LineNo 20", 20, doc7.CSI_LineNo);
					AssertEquals("Prereq: doc8 has CSI_LineNo 0", 0, doc8.CSI_LineNo);
					AssertEquals("Prereq: doc9 has CSI_LineNo 0", 0, doc9.CSI_LineNo);

					NctsHeaderDocumentsSequenceNumberHelper.AssignAdditionalDocumentsSequenceNumbers(nctsHeader);

					AssertContainsExactElementsInAnyOrder("When declaration is Departure Phase5 documents in Header have sequences 1 and 2", new ZInt[] { 1, 2 }, new ZInt[] { doc8.CSI_LineNo, doc9.CSI_LineNo });

					AssertContainsExactElementsInAnyOrder("When declaration is Departure Phase5 documents in nctsBill1 have sequences 3, 4", new ZInt[] { 3, 4 }, new ZInt[] { doc5.CSI_LineNo, doc6.CSI_LineNo });
					AssertContainsExactElementsInAnyOrder("When declaration is Departure Phase5 documents in goodsItem11 have sequences 5 and 6", new ZInt[] { 5, 6 }, new ZInt[] { doc2.CSI_LineNo, doc1.CSI_LineNo });
					AssertEquals("When declaration is Departure Phase5 doc3 has CSI_LineNo 5 because it is in goodsItem12", 5, doc3.CSI_LineNo);

					AssertEquals("When declaration is Departure Phase5 doc7 has CSI_LineNo 3 because it is in nctsBill2", 3, doc7.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 doc4 has CSI_LineNo 4 because it is in goodsItem21", 4, doc4.CSI_LineNo);
				});
			}
		}

		public void TestAssignAdditionalDocumentsSequenceNumbers_TransitionPeriod_OnlyREF()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
			{
				var doc1 = goodsItem11.AdditionalInfos.AddNew();
				doc1.CSI_Code = "9001";
				doc1.CSI_SubType = "REF";

				var doc2 = goodsItem11.AdditionalInfos.AddNew();
				doc2.CSI_Code = "9002";
				doc2.CSI_SubType = "REF";
				doc2.CSI_LineNo = 20;

				var doc3 = goodsItem12.AdditionalInfos.AddNew();
				doc3.CSI_Code = "9003";
				doc3.CSI_SubType = "REF";

				var doc4 = goodsItem21.AdditionalInfos.AddNew();
				doc4.CSI_Code = "9004";
				doc4.CSI_SubType = "REF";

				var doc5 = nctsBill1.AdditionalDocuments.AddNew();
				doc5.CSI_Code = "9005";
				doc5.CSI_SubType = "REF";

				var doc6 = nctsBill1.AdditionalDocuments.AddNew();
				doc6.CSI_Code = "9006";
				doc6.CSI_SubType = "REF";

				var doc7 = nctsBill2.AdditionalDocuments.AddNew();
				doc7.CSI_Code = "9007";
				doc7.CSI_SubType = "REF";
				doc7.CSI_LineNo = 20;

				var doc8 = nctsHeader.AdditionalDocuments.AddNew();
				doc8.CSI_Code = "9008";
				doc8.CSI_SubType = "REF";

				var doc9 = nctsHeader.AdditionalDocuments.AddNew();
				doc9.CSI_Code = "9009";
				doc9.CSI_SubType = "REF";

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: doc1 has CSI_LineNo 0", 0, doc1.CSI_LineNo);
					AssertEquals("Prereq: doc2 has CSI_LineNo 20", 20, doc2.CSI_LineNo);
					AssertEquals("Prereq: doc3 has CSI_LineNo 0", 0, doc3.CSI_LineNo);
					AssertEquals("Prereq: doc4 has CSI_LineNo 0", 0, doc4.CSI_LineNo);
					AssertEquals("Prereq: doc5 has CSI_LineNo 0", 0, doc5.CSI_LineNo);
					AssertEquals("Prereq: doc6 has CSI_LineNo 0", 0, doc6.CSI_LineNo);
					AssertEquals("Prereq: doc7 has CSI_LineNo 20", 20, doc7.CSI_LineNo);
					AssertEquals("Prereq: doc8 has CSI_LineNo 0", 0, doc8.CSI_LineNo);
					AssertEquals("Prereq: doc9 has CSI_LineNo 0", 0, doc9.CSI_LineNo);

					NctsHeaderDocumentsSequenceNumberHelper.AssignAdditionalDocumentsSequenceNumbers(nctsHeader);

					AssertContainsExactElementsInAnyOrder("When declaration is Departure Phase5 documents in Header have sequences 1 and 2", new ZInt[] { 1, 2 }, new ZInt[] { doc8.CSI_LineNo, doc9.CSI_LineNo });

					AssertContainsExactElementsInAnyOrder("When declaration is Departure Phase5 documents in nctsBill1 have sequences 3, 4", new ZInt[] { 3, 4 }, new ZInt[] { doc5.CSI_LineNo, doc6.CSI_LineNo });
					AssertContainsExactElementsInAnyOrder("When declaration is Departure Phase5 documents in goodsItem11 have sequences 5 and 6", new ZInt[] { 5, 6 }, new ZInt[] { doc2.CSI_LineNo, doc1.CSI_LineNo });
					AssertEquals("When declaration is Departure Phase5 doc3 has CSI_LineNo 5 because it is in goodsItem12", 5, doc3.CSI_LineNo);

					AssertEquals("When declaration is Departure Phase5 doc7 has CSI_LineNo 3 because it is in nctsBill2", 3, doc7.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 doc4 has CSI_LineNo 4 because it is in goodsItem21", 4, doc4.CSI_LineNo);
				});
			}
		}

		public void TestAssignAdditionalDocumentsSequenceNumbers_TransitionPeriod_OnlyTRA()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
			{
				var doc1 = goodsItem11.AdditionalInfos.AddNew();
				doc1.CSI_Code = "9001";
				doc1.CSI_SubType = "TRA";

				var doc2 = goodsItem11.AdditionalInfos.AddNew();
				doc2.CSI_Code = "9002";
				doc2.CSI_SubType = "TRA";
				doc2.CSI_LineNo = 20;

				var doc3 = goodsItem12.AdditionalInfos.AddNew();
				doc3.CSI_Code = "9003";
				doc3.CSI_SubType = "TRA";

				var doc4 = goodsItem21.AdditionalInfos.AddNew();
				doc4.CSI_Code = "9004";
				doc4.CSI_SubType = "TRA";

				var doc5 = nctsBill1.AdditionalDocuments.AddNew();
				doc5.CSI_Code = "9005";
				doc5.CSI_SubType = "TRA";

				var doc6 = nctsBill1.AdditionalDocuments.AddNew();
				doc6.CSI_Code = "9006";
				doc6.CSI_SubType = "TRA";

				var doc7 = nctsBill2.AdditionalDocuments.AddNew();
				doc7.CSI_Code = "9007";
				doc7.CSI_SubType = "TRA";
				doc7.CSI_LineNo = 20;

				var doc8 = nctsHeader.AdditionalDocuments.AddNew();
				doc8.CSI_Code = "9008";
				doc8.CSI_SubType = "TRA";

				var doc9 = nctsHeader.AdditionalDocuments.AddNew();
				doc9.CSI_Code = "9009";
				doc9.CSI_SubType = "TRA";

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: doc1 has CSI_LineNo 0", 0, doc1.CSI_LineNo);
					AssertEquals("Prereq: doc2 has CSI_LineNo 20", 20, doc2.CSI_LineNo);
					AssertEquals("Prereq: doc3 has CSI_LineNo 0", 0, doc3.CSI_LineNo);
					AssertEquals("Prereq: doc4 has CSI_LineNo 0", 0, doc4.CSI_LineNo);
					AssertEquals("Prereq: doc5 has CSI_LineNo 0", 0, doc5.CSI_LineNo);
					AssertEquals("Prereq: doc6 has CSI_LineNo 0", 0, doc6.CSI_LineNo);
					AssertEquals("Prereq: doc7 has CSI_LineNo 20", 20, doc7.CSI_LineNo);
					AssertEquals("Prereq: doc8 has CSI_LineNo 0", 0, doc8.CSI_LineNo);
					AssertEquals("Prereq: doc9 has CSI_LineNo 0", 0, doc9.CSI_LineNo);

					NctsHeaderDocumentsSequenceNumberHelper.AssignAdditionalDocumentsSequenceNumbers(nctsHeader);

					AssertContainsExactElementsInAnyOrder("When declaration is Departure Phase5 documents in Header have sequences 1 and 2", new ZInt[] { 1, 2 }, new ZInt[] { doc8.CSI_LineNo, doc9.CSI_LineNo });

					AssertContainsExactElementsInAnyOrder("When declaration is Departure Phase5 documents in nctsBill1 have sequences 3, 4", new ZInt[] { 3, 4 }, new ZInt[] { doc5.CSI_LineNo, doc6.CSI_LineNo });
					AssertContainsExactElementsInAnyOrder("When declaration is Departure Phase5 documents in goodsItem11 have sequences 5 and 6", new ZInt[] { 5, 6 }, new ZInt[] { doc2.CSI_LineNo, doc1.CSI_LineNo });
					AssertEquals("When declaration is Departure Phase5 doc3 has CSI_LineNo 5 because it is in goodsItem12", 5, doc3.CSI_LineNo);

					AssertEquals("When declaration is Departure Phase5 doc7 has CSI_LineNo 3 because it is in nctsBill2", 3, doc7.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 doc4 has CSI_LineNo 4 because it is in goodsItem21", 4, doc4.CSI_LineNo);
				});
			}
		}

		public void TestResetSupportingDocumentsLineNoWhenPhase5_FromGoodsItem_TransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
			{
				var doc1 = goodsItem11.SupportingDocuments.AddNew();
				doc1.CSI_Code = "9001";
				doc1.CSI_LineNo = 1;

				var doc2 = goodsItem11.SupportingDocuments.AddNew();
				doc2.CSI_Code = "A002";
				doc2.CSI_LineNo = 2;

				var doc3 = goodsItem12.SupportingDocuments.AddNew();
				doc3.CSI_Code = "9003";
				doc3.CSI_LineNo = 3;

				var doc4 = goodsItem21.SupportingDocuments.AddNew();
				doc4.CSI_Code = "Y004";
				doc4.CSI_LineNo = 4;

				var doc5 = nctsBill1.SupportingDocuments.AddNew();
				doc5.CSI_Code = "9005";
				doc5.CSI_LineNo = 5;

				var doc6 = nctsBill1.SupportingDocuments.AddNew();
				doc6.CSI_Code = "C006";
				doc6.CSI_LineNo = 6;

				var doc7 = nctsBill2.SupportingDocuments.AddNew();
				doc7.CSI_Code = "9007";
				doc7.CSI_LineNo = 7;

				var doc8 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				doc8.CSI_Code = "A008";
				doc8.CSI_LineNo = 8;

				var doc9 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				doc9.CSI_Code = "9009";
				doc9.CSI_LineNo = 9;

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: doc1 has CSI_LineNo 1", 1, doc1.CSI_LineNo);
					AssertEquals("Prereq: doc2 has CSI_LineNo 2", 2, doc2.CSI_LineNo);
					AssertEquals("Prereq: doc3 has CSI_LineNo 3", 3, doc3.CSI_LineNo);
					AssertEquals("Prereq: doc4 has CSI_LineNo 4", 4, doc4.CSI_LineNo);
					AssertEquals("Prereq: doc5 has CSI_LineNo 5", 5, doc5.CSI_LineNo);
					AssertEquals("Prereq: doc6 has CSI_LineNo 6", 6, doc6.CSI_LineNo);
					AssertEquals("Prereq: doc7 has CSI_LineNo 7", 7, doc7.CSI_LineNo);
					AssertEquals("Prereq: doc8 has CSI_LineNo 8", 8, doc8.CSI_LineNo);
					AssertEquals("Prereq: doc9 has CSI_LineNo 9", 9, doc9.CSI_LineNo);

					NctsHeaderDocumentsSequenceNumberHelper.ResetSupportingDocumentsLineNoWhenPhase5(nctsHeader, goodsItem11);

					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc1 has CSI_LineNo 1", 1, doc1.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc2 has CSI_LineNo 2", 2, doc2.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc3 has CSI_LineNo 3", 3, doc3.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc4 has CSI_LineNo 4", 4, doc4.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc5 has CSI_LineNo 5", 5, doc5.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc6 has CSI_LineNo 6", 6, doc6.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc7 has CSI_LineNo 7", 7, doc7.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc8 has CSI_LineNo 8", 8, doc8.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc9 has CSI_LineNo 9", 9, doc9.CSI_LineNo);

					nctsHeader.MovementHeader.BM_CustomsStatus = "AAA";
					NctsHeaderDocumentsSequenceNumberHelper.ResetSupportingDocumentsLineNoWhenPhase5(nctsHeader, goodsItem11);

					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc1 has CSI_LineNo 1", 1, doc1.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc2 has CSI_LineNo 2", 2, doc2.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc3 has CSI_LineNo 3", 3, doc3.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc4 has CSI_LineNo 4", 4, doc4.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc5 has CSI_LineNo 5", 5, doc5.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc6 has CSI_LineNo 6", 6, doc6.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc7 has CSI_LineNo 7", 7, doc7.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc8 has CSI_LineNo 8", 8, doc8.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc9 has CSI_LineNo 9", 9, doc9.CSI_LineNo);

					nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
					NctsHeaderDocumentsSequenceNumberHelper.ResetSupportingDocumentsLineNoWhenPhase5(nctsHeader, goodsItem11);

					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc1 has CSI_LineNo 0", 0, doc1.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc2 has CSI_LineNo 0", 0, doc2.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc3 has CSI_LineNo 3", 3, doc3.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc4 has CSI_LineNo 4", 4, doc4.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc5 has CSI_LineNo 5", 5, doc5.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc6 has CSI_LineNo 6", 6, doc6.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc7 has CSI_LineNo 7", 7, doc7.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc8 has CSI_LineNo 8", 8, doc8.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc9 has CSI_LineNo 9", 9, doc9.CSI_LineNo);

					doc1.CSI_LineNo = 1;
					doc2.CSI_LineNo = 2;
					doc3.CSI_LineNo = 3;
					doc4.CSI_LineNo = 4;
					doc5.CSI_LineNo = 5;
					doc6.CSI_LineNo = 6;
					doc7.CSI_LineNo = 7;
					doc8.CSI_LineNo = 8;
					doc9.CSI_LineNo = 9;

					AssertEquals("Prereq2: doc1 has CSI_LineNo 1", 1, doc1.CSI_LineNo);
					AssertEquals("Prereq2: doc2 has CSI_LineNo 2", 2, doc2.CSI_LineNo);
					AssertEquals("Prereq2: doc3 has CSI_LineNo 3", 3, doc3.CSI_LineNo);
					AssertEquals("Prereq2: doc4 has CSI_LineNo 4", 4, doc4.CSI_LineNo);
					AssertEquals("Prereq2: doc5 has CSI_LineNo 5", 5, doc5.CSI_LineNo);
					AssertEquals("Prereq2: doc6 has CSI_LineNo 6", 6, doc6.CSI_LineNo);
					AssertEquals("Prereq2: doc7 has CSI_LineNo 7", 7, doc7.CSI_LineNo);
					AssertEquals("Prereq2: doc8 has CSI_LineNo 8", 8, doc8.CSI_LineNo);
					AssertEquals("Prereq2: doc9 has CSI_LineNo 9", 9, doc9.CSI_LineNo);

					nctsHeader.MovementHeader.BM_CustomsStatus = "AAA";
					NctsHeaderDocumentsSequenceNumberHelper.ResetSupportingDocumentsLineNoWhenPhase5(nctsHeader, goodsItem11, forceReset: true);

					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE but forceReset is true: doc1 has CSI_LineNo 0", 0, doc1.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE but forceReset is true: doc2 has CSI_LineNo 0", 0, doc2.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE but forceReset is true: doc3 has CSI_LineNo 3", 3, doc3.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE but forceReset is true: doc4 has CSI_LineNo 4", 4, doc4.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE but forceReset is true: doc5 has CSI_LineNo 5", 5, doc5.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE but forceReset is true: doc6 has CSI_LineNo 6", 6, doc6.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE but forceReset is true: doc7 has CSI_LineNo 7", 7, doc7.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE but forceReset is true: doc8 has CSI_LineNo 8", 8, doc8.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE but forceReset is true: doc9 has CSI_LineNo 9", 9, doc9.CSI_LineNo);
				});
			}
		}

		public void TestResetSupportingDocumentsLineNoWhenPhase5_FromGoodsItem_NoTransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
			{
				var doc1 = goodsItem11.SupportingDocuments.AddNew();
				doc1.CSI_Code = "9001";
				doc1.CSI_LineNo = 1;

				var doc2 = goodsItem11.SupportingDocuments.AddNew();
				doc2.CSI_Code = "A002";
				doc2.CSI_LineNo = 2;

				var doc3 = goodsItem12.SupportingDocuments.AddNew();
				doc3.CSI_Code = "9003";
				doc3.CSI_LineNo = 3;

				var doc4 = goodsItem21.SupportingDocuments.AddNew();
				doc4.CSI_Code = "Y004";
				doc4.CSI_LineNo = 4;

				var doc5 = nctsBill1.SupportingDocuments.AddNew();
				doc5.CSI_Code = "9005";
				doc5.CSI_LineNo = 5;

				var doc6 = nctsBill1.SupportingDocuments.AddNew();
				doc6.CSI_Code = "C006";
				doc6.CSI_LineNo = 6;

				var doc7 = nctsBill2.SupportingDocuments.AddNew();
				doc7.CSI_Code = "9007";
				doc7.CSI_LineNo = 7;

				var doc8 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				doc8.CSI_Code = "A008";
				doc8.CSI_LineNo = 8;

				var doc9 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				doc9.CSI_Code = "9009";
				doc9.CSI_LineNo = 9;

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: doc1 has CSI_LineNo 1", 1, doc1.CSI_LineNo);
					AssertEquals("Prereq: doc2 has CSI_LineNo 2", 2, doc2.CSI_LineNo);
					AssertEquals("Prereq: doc3 has CSI_LineNo 3", 3, doc3.CSI_LineNo);
					AssertEquals("Prereq: doc4 has CSI_LineNo 4", 4, doc4.CSI_LineNo);
					AssertEquals("Prereq: doc5 has CSI_LineNo 5", 5, doc5.CSI_LineNo);
					AssertEquals("Prereq: doc6 has CSI_LineNo 6", 6, doc6.CSI_LineNo);
					AssertEquals("Prereq: doc7 has CSI_LineNo 7", 7, doc7.CSI_LineNo);
					AssertEquals("Prereq: doc8 has CSI_LineNo 8", 8, doc8.CSI_LineNo);
					AssertEquals("Prereq: doc9 has CSI_LineNo 9", 9, doc9.CSI_LineNo);

					NctsHeaderDocumentsSequenceNumberHelper.ResetSupportingDocumentsLineNoWhenPhase5(nctsHeader, goodsItem11);

					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc1 has CSI_LineNo 1", 1, doc1.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc2 has CSI_LineNo 2", 2, doc2.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc3 has CSI_LineNo 3", 3, doc3.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc4 has CSI_LineNo 4", 4, doc4.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc5 has CSI_LineNo 5", 5, doc5.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc6 has CSI_LineNo 6", 6, doc6.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc7 has CSI_LineNo 7", 7, doc7.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc8 has CSI_LineNo 8", 8, doc8.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc9 has CSI_LineNo 9", 9, doc9.CSI_LineNo);

					nctsHeader.MovementHeader.BM_CustomsStatus = "AAA";
					NctsHeaderDocumentsSequenceNumberHelper.ResetSupportingDocumentsLineNoWhenPhase5(nctsHeader, goodsItem11);

					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc1 has CSI_LineNo 1", 1, doc1.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc2 has CSI_LineNo 2", 2, doc2.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc3 has CSI_LineNo 3", 3, doc3.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc4 has CSI_LineNo 4", 4, doc4.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc5 has CSI_LineNo 5", 5, doc5.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc6 has CSI_LineNo 6", 6, doc6.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc7 has CSI_LineNo 7", 7, doc7.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc8 has CSI_LineNo 8", 8, doc8.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc9 has CSI_LineNo 9", 9, doc9.CSI_LineNo);

					nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
					NctsHeaderDocumentsSequenceNumberHelper.ResetSupportingDocumentsLineNoWhenPhase5(nctsHeader, goodsItem11);

					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc1 has CSI_LineNo 1", 1, doc1.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc2 has CSI_LineNo 2", 2, doc2.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc3 has CSI_LineNo 3", 3, doc3.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc4 has CSI_LineNo 4", 4, doc4.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc5 has CSI_LineNo 5", 5, doc5.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc6 has CSI_LineNo 6", 6, doc6.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc7 has CSI_LineNo 7", 7, doc7.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc8 has CSI_LineNo 8", 8, doc8.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc9 has CSI_LineNo 9", 9, doc9.CSI_LineNo);

					nctsHeader.MovementHeader.BM_CustomsStatus = "AAA";
					NctsHeaderDocumentsSequenceNumberHelper.ResetSupportingDocumentsLineNoWhenPhase5(nctsHeader, goodsItem11, forceReset: true);

					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE and forceReset is true: doc1 has CSI_LineNo 1", 1, doc1.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE and forceReset is true: doc2 has CSI_LineNo 2", 2, doc2.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE and forceReset is true: doc3 has CSI_LineNo 3", 3, doc3.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE and forceReset is true: doc4 has CSI_LineNo 4", 4, doc4.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE and forceReset is true: doc5 has CSI_LineNo 5", 5, doc5.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE and forceReset is true: doc6 has CSI_LineNo 6", 6, doc6.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE and forceReset is true: doc7 has CSI_LineNo 7", 7, doc7.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE and forceReset is true: doc8 has CSI_LineNo 8", 8, doc8.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE and forceReset is true: doc9 has CSI_LineNo 9", 9, doc9.CSI_LineNo);
				});
			}
		}

		public void TestResetSupportingDocumentsLineNoWhenPhase5_FromBill_TransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
			{
				var doc1 = goodsItem11.SupportingDocuments.AddNew();
				doc1.CSI_Code = "9001";
				doc1.CSI_LineNo = 1;

				var doc2 = goodsItem11.SupportingDocuments.AddNew();
				doc2.CSI_Code = "A002";
				doc2.CSI_LineNo = 2;

				var doc3 = goodsItem12.SupportingDocuments.AddNew();
				doc3.CSI_Code = "9003";
				doc3.CSI_LineNo = 3;

				var doc4 = goodsItem21.SupportingDocuments.AddNew();
				doc4.CSI_Code = "Y004";
				doc4.CSI_LineNo = 4;

				var doc5 = nctsBill1.SupportingDocuments.AddNew();
				doc5.CSI_Code = "9005";
				doc5.CSI_LineNo = 5;

				var doc6 = nctsBill1.SupportingDocuments.AddNew();
				doc6.CSI_Code = "C006";
				doc6.CSI_LineNo = 6;

				var doc7 = nctsBill2.SupportingDocuments.AddNew();
				doc7.CSI_Code = "9007";
				doc7.CSI_LineNo = 7;

				var doc8 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				doc8.CSI_Code = "A008";
				doc8.CSI_LineNo = 8;

				var doc9 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				doc9.CSI_Code = "9009";
				doc9.CSI_LineNo = 9;

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: doc1 has CSI_LineNo 1", 1, doc1.CSI_LineNo);
					AssertEquals("Prereq: doc2 has CSI_LineNo 2", 2, doc2.CSI_LineNo);
					AssertEquals("Prereq: doc3 has CSI_LineNo 3", 3, doc3.CSI_LineNo);
					AssertEquals("Prereq: doc4 has CSI_LineNo 4", 4, doc4.CSI_LineNo);
					AssertEquals("Prereq: doc5 has CSI_LineNo 5", 5, doc5.CSI_LineNo);
					AssertEquals("Prereq: doc6 has CSI_LineNo 6", 6, doc6.CSI_LineNo);
					AssertEquals("Prereq: doc7 has CSI_LineNo 7", 7, doc7.CSI_LineNo);
					AssertEquals("Prereq: doc8 has CSI_LineNo 8", 8, doc8.CSI_LineNo);
					AssertEquals("Prereq: doc9 has CSI_LineNo 9", 9, doc9.CSI_LineNo);

					NctsHeaderDocumentsSequenceNumberHelper.ResetSupportingDocumentsLineNoWhenPhase5(nctsHeader, nctsBill1);

					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc1 has CSI_LineNo 1", 1, doc1.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc2 has CSI_LineNo 2", 2, doc2.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc3 has CSI_LineNo 3", 3, doc3.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc4 has CSI_LineNo 4", 4, doc4.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc5 has CSI_LineNo 5", 5, doc5.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc6 has CSI_LineNo 6", 6, doc6.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc7 has CSI_LineNo 7", 7, doc7.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc8 has CSI_LineNo 8", 8, doc8.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc9 has CSI_LineNo 9", 9, doc9.CSI_LineNo);

					nctsHeader.MovementHeader.BM_CustomsStatus = "AAA";
					NctsHeaderDocumentsSequenceNumberHelper.ResetSupportingDocumentsLineNoWhenPhase5(nctsHeader, nctsBill1);

					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc1 has CSI_LineNo 1", 1, doc1.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc2 has CSI_LineNo 2", 2, doc2.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc3 has CSI_LineNo 3", 3, doc3.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc4 has CSI_LineNo 4", 4, doc4.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc5 has CSI_LineNo 5", 5, doc5.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc6 has CSI_LineNo 6", 6, doc6.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc7 has CSI_LineNo 7", 7, doc7.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc8 has CSI_LineNo 8", 8, doc8.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc9 has CSI_LineNo 9", 9, doc9.CSI_LineNo);

					nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
					NctsHeaderDocumentsSequenceNumberHelper.ResetSupportingDocumentsLineNoWhenPhase5(nctsHeader, nctsBill1);

					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc1 has CSI_LineNo 0", 0, doc1.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc2 has CSI_LineNo 0", 0, doc2.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc3 has CSI_LineNo 0", 0, doc3.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc4 has CSI_LineNo 4", 4, doc4.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc5 has CSI_LineNo 0", 0, doc5.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc6 has CSI_LineNo 0", 0, doc6.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc7 has CSI_LineNo 7", 7, doc7.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc8 has CSI_LineNo 8", 8, doc8.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc9 has CSI_LineNo 9", 9, doc9.CSI_LineNo);

					doc1.CSI_LineNo = 1;
					doc2.CSI_LineNo = 2;
					doc3.CSI_LineNo = 3;
					doc5.CSI_LineNo = 5;
					doc6.CSI_LineNo = 6;

					AssertEquals("Prereq2: doc1 has CSI_LineNo 1", 1, doc1.CSI_LineNo);
					AssertEquals("Prereq2: doc2 has CSI_LineNo 2", 2, doc2.CSI_LineNo);
					AssertEquals("Prereq2: doc3 has CSI_LineNo 3", 3, doc3.CSI_LineNo);
					AssertEquals("Prereq2: doc4 has CSI_LineNo 4", 4, doc4.CSI_LineNo);
					AssertEquals("Prereq2: doc5 has CSI_LineNo 5", 5, doc5.CSI_LineNo);
					AssertEquals("Prereq2: doc6 has CSI_LineNo 6", 6, doc6.CSI_LineNo);
					AssertEquals("Prereq2: doc7 has CSI_LineNo 7", 7, doc7.CSI_LineNo);
					AssertEquals("Prereq2: doc8 has CSI_LineNo 8", 8, doc8.CSI_LineNo);
					AssertEquals("Prereq2: doc9 has CSI_LineNo 9", 9, doc9.CSI_LineNo);

					nctsHeader.MovementHeader.BM_CustomsStatus = "AAA";
					NctsHeaderDocumentsSequenceNumberHelper.ResetSupportingDocumentsLineNoWhenPhase5(nctsHeader, nctsBill1, forceReset: true);

					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE but forceReset is true: doc1 has CSI_LineNo 0", 0, doc1.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE but forceReset is true: doc2 has CSI_LineNo 0", 0, doc2.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE but forceReset is true: doc3 has CSI_LineNo 0", 0, doc3.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE but forceReset is true: doc4 has CSI_LineNo 4", 4, doc4.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE but forceReset is true: doc5 has CSI_LineNo 0", 0, doc5.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE but forceReset is true: doc6 has CSI_LineNo 0", 0, doc6.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE but forceReset is true: doc7 has CSI_LineNo 7", 7, doc7.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE but forceReset is true: doc8 has CSI_LineNo 8", 8, doc8.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE but forceReset is true: doc9 has CSI_LineNo 9", 9, doc9.CSI_LineNo);
				});
			}
		}

		public void TestResetSupportingDocumentsLineNoWhenPhase5_FromBill_NoTransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
			{
				var doc1 = goodsItem11.SupportingDocuments.AddNew();
				doc1.CSI_Code = "9001";
				doc1.CSI_LineNo = 1;

				var doc2 = goodsItem11.SupportingDocuments.AddNew();
				doc2.CSI_Code = "A002";
				doc2.CSI_LineNo = 2;

				var doc3 = goodsItem12.SupportingDocuments.AddNew();
				doc3.CSI_Code = "9003";
				doc3.CSI_LineNo = 3;

				var doc4 = goodsItem21.SupportingDocuments.AddNew();
				doc4.CSI_Code = "Y004";
				doc4.CSI_LineNo = 4;

				var doc5 = nctsBill1.SupportingDocuments.AddNew();
				doc5.CSI_Code = "9005";
				doc5.CSI_LineNo = 5;

				var doc6 = nctsBill1.SupportingDocuments.AddNew();
				doc6.CSI_Code = "C006";
				doc6.CSI_LineNo = 6;

				var doc7 = nctsBill2.SupportingDocuments.AddNew();
				doc7.CSI_Code = "9007";
				doc7.CSI_LineNo = 7;

				var doc8 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				doc8.CSI_Code = "A008";
				doc8.CSI_LineNo = 8;

				var doc9 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				doc9.CSI_Code = "9009";
				doc9.CSI_LineNo = 9;

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: doc1 has CSI_LineNo 1", 1, doc1.CSI_LineNo);
					AssertEquals("Prereq: doc2 has CSI_LineNo 2", 2, doc2.CSI_LineNo);
					AssertEquals("Prereq: doc3 has CSI_LineNo 3", 3, doc3.CSI_LineNo);
					AssertEquals("Prereq: doc4 has CSI_LineNo 4", 4, doc4.CSI_LineNo);
					AssertEquals("Prereq: doc5 has CSI_LineNo 5", 5, doc5.CSI_LineNo);
					AssertEquals("Prereq: doc6 has CSI_LineNo 6", 6, doc6.CSI_LineNo);
					AssertEquals("Prereq: doc7 has CSI_LineNo 7", 7, doc7.CSI_LineNo);
					AssertEquals("Prereq: doc8 has CSI_LineNo 8", 8, doc8.CSI_LineNo);
					AssertEquals("Prereq: doc9 has CSI_LineNo 9", 9, doc9.CSI_LineNo);

					NctsHeaderDocumentsSequenceNumberHelper.ResetSupportingDocumentsLineNoWhenPhase5(nctsHeader, nctsBill1);

					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc1 has CSI_LineNo 1", 1, doc1.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc2 has CSI_LineNo 2", 2, doc2.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc3 has CSI_LineNo 3", 3, doc3.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc4 has CSI_LineNo 4", 4, doc4.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc5 has CSI_LineNo 5", 5, doc5.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc6 has CSI_LineNo 6", 6, doc6.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc7 has CSI_LineNo 7", 7, doc7.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc8 has CSI_LineNo 8", 8, doc8.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc9 has CSI_LineNo 9", 9, doc9.CSI_LineNo);

					nctsHeader.MovementHeader.BM_CustomsStatus = "AAA";
					NctsHeaderDocumentsSequenceNumberHelper.ResetSupportingDocumentsLineNoWhenPhase5(nctsHeader, nctsBill1);

					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc1 has CSI_LineNo 1", 1, doc1.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc2 has CSI_LineNo 2", 2, doc2.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc3 has CSI_LineNo 3", 3, doc3.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc4 has CSI_LineNo 4", 4, doc4.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc5 has CSI_LineNo 5", 5, doc5.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc6 has CSI_LineNo 6", 6, doc6.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc7 has CSI_LineNo 7", 7, doc7.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc8 has CSI_LineNo 8", 8, doc8.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc9 has CSI_LineNo 9", 9, doc9.CSI_LineNo);

					nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
					NctsHeaderDocumentsSequenceNumberHelper.ResetSupportingDocumentsLineNoWhenPhase5(nctsHeader, nctsBill1);

					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc1 has CSI_LineNo 1", 1, doc1.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc2 has CSI_LineNo 2", 2, doc2.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc3 has CSI_LineNo 3", 3, doc3.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc4 has CSI_LineNo 4", 4, doc4.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc5 has CSI_LineNo 5", 5, doc5.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc6 has CSI_LineNo 6", 6, doc6.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc7 has CSI_LineNo 7", 7, doc7.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc8 has CSI_LineNo 8", 8, doc8.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc9 has CSI_LineNo 9", 9, doc9.CSI_LineNo);

					nctsHeader.MovementHeader.BM_CustomsStatus = "AAA";
					NctsHeaderDocumentsSequenceNumberHelper.ResetSupportingDocumentsLineNoWhenPhase5(nctsHeader, nctsBill1, forceReset: true);

					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE and forceReset is true: doc1 has CSI_LineNo 1", 1, doc1.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE and forceReset is true: doc2 has CSI_LineNo 2", 2, doc2.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE and forceReset is true: doc3 has CSI_LineNo 3", 3, doc3.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE and forceReset is true: doc4 has CSI_LineNo 4", 4, doc4.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE and forceReset is true: doc5 has CSI_LineNo 5", 5, doc5.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE and forceReset is true: doc6 has CSI_LineNo 6", 6, doc6.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE and forceReset is true: doc7 has CSI_LineNo 7", 7, doc7.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE and forceReset is true: doc8 has CSI_LineNo 8", 8, doc8.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE and forceReset is true: doc9 has CSI_LineNo 9", 9, doc9.CSI_LineNo);
				});
			}
		}

		public void TestResetSupportingDocumentsLineNoWhenPhase5_FromHeader_TransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
			{
				var doc1 = goodsItem11.SupportingDocuments.AddNew();
				doc1.CSI_Code = "9001";
				doc1.CSI_LineNo = 1;

				var doc2 = goodsItem11.SupportingDocuments.AddNew();
				doc2.CSI_Code = "A002";
				doc2.CSI_LineNo = 2;

				var doc3 = goodsItem12.SupportingDocuments.AddNew();
				doc3.CSI_Code = "9003";
				doc3.CSI_LineNo = 3;

				var doc4 = goodsItem21.SupportingDocuments.AddNew();
				doc4.CSI_Code = "Y004";
				doc4.CSI_LineNo = 4;

				var doc5 = nctsBill1.SupportingDocuments.AddNew();
				doc5.CSI_Code = "9005";
				doc5.CSI_LineNo = 5;

				var doc6 = nctsBill1.SupportingDocuments.AddNew();
				doc6.CSI_Code = "C006";
				doc6.CSI_LineNo = 6;

				var doc7 = nctsBill2.SupportingDocuments.AddNew();
				doc7.CSI_Code = "9007";
				doc7.CSI_LineNo = 7;

				var doc8 = movementHeader.SupportingDocuments.AddNew();
				doc8.CSI_Code = "A008";
				doc8.CSI_LineNo = 8;

				var doc9 = movementHeader.SupportingDocuments.AddNew();
				doc9.CSI_Code = "9009";
				doc9.CSI_LineNo = 9;

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: doc1 has CSI_LineNo 1", 1, doc1.CSI_LineNo);
					AssertEquals("Prereq: doc2 has CSI_LineNo 2", 2, doc2.CSI_LineNo);
					AssertEquals("Prereq: doc3 has CSI_LineNo 3", 3, doc3.CSI_LineNo);
					AssertEquals("Prereq: doc4 has CSI_LineNo 4", 4, doc4.CSI_LineNo);
					AssertEquals("Prereq: doc5 has CSI_LineNo 5", 5, doc5.CSI_LineNo);
					AssertEquals("Prereq: doc6 has CSI_LineNo 6", 6, doc6.CSI_LineNo);
					AssertEquals("Prereq: doc7 has CSI_LineNo 7", 7, doc7.CSI_LineNo);
					AssertEquals("Prereq: doc8 has CSI_LineNo 8", 8, doc8.CSI_LineNo);
					AssertEquals("Prereq: doc9 has CSI_LineNo 9", 9, doc9.CSI_LineNo);

					NctsHeaderDocumentsSequenceNumberHelper.ResetSupportingDocumentsLineNoWhenPhase5(nctsHeader, nctsHeader);

					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc1 has CSI_LineNo 1", 1, doc1.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc2 has CSI_LineNo 2", 2, doc2.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc3 has CSI_LineNo 3", 3, doc3.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc4 has CSI_LineNo 4", 4, doc4.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc5 has CSI_LineNo 5", 5, doc5.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc6 has CSI_LineNo 6", 6, doc6.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc7 has CSI_LineNo 7", 7, doc7.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc8 has CSI_LineNo 8", 8, doc8.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc9 has CSI_LineNo 9", 9, doc9.CSI_LineNo);

					movementHeader.BM_CustomsStatus = "AAA";
					NctsHeaderDocumentsSequenceNumberHelper.ResetSupportingDocumentsLineNoWhenPhase5(nctsHeader, nctsHeader);

					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc1 has CSI_LineNo 1", 1, doc1.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc2 has CSI_LineNo 2", 2, doc2.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc3 has CSI_LineNo 3", 3, doc3.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc4 has CSI_LineNo 4", 4, doc4.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc5 has CSI_LineNo 5", 5, doc5.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc6 has CSI_LineNo 6", 6, doc6.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc7 has CSI_LineNo 7", 7, doc7.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc8 has CSI_LineNo 8", 8, doc8.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc9 has CSI_LineNo 9", 9, doc9.CSI_LineNo);

					movementHeader.BM_CustomsStatus = "PRE";
					NctsHeaderDocumentsSequenceNumberHelper.ResetSupportingDocumentsLineNoWhenPhase5(nctsHeader, nctsHeader.MovementHeader);

					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc1 has CSI_LineNo 0", 0, doc1.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc2 has CSI_LineNo 0", 0, doc2.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc3 has CSI_LineNo 0", 0, doc3.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc4 has CSI_LineNo 0", 0, doc4.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc5 has CSI_LineNo 0", 0, doc5.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc6 has CSI_LineNo 0", 0, doc6.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc7 has CSI_LineNo 0", 0, doc7.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc8 has CSI_LineNo 0", 0, doc8.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc9 has CSI_LineNo 0", 0, doc9.CSI_LineNo);

					doc1.CSI_LineNo = 1;
					doc2.CSI_LineNo = 2;
					doc3.CSI_LineNo = 3;
					doc4.CSI_LineNo = 4;
					doc5.CSI_LineNo = 5;
					doc6.CSI_LineNo = 6;
					doc7.CSI_LineNo = 7;
					doc8.CSI_LineNo = 8;
					doc9.CSI_LineNo = 9;

					AssertEquals("Prereq2: doc1 has CSI_LineNo 1", 1, doc1.CSI_LineNo);
					AssertEquals("Prereq2: doc2 has CSI_LineNo 2", 2, doc2.CSI_LineNo);
					AssertEquals("Prereq2: doc3 has CSI_LineNo 3", 3, doc3.CSI_LineNo);
					AssertEquals("Prereq2: doc4 has CSI_LineNo 4", 4, doc4.CSI_LineNo);
					AssertEquals("Prereq2: doc5 has CSI_LineNo 5", 5, doc5.CSI_LineNo);
					AssertEquals("Prereq2: doc6 has CSI_LineNo 6", 6, doc6.CSI_LineNo);
					AssertEquals("Prereq2: doc7 has CSI_LineNo 7", 7, doc7.CSI_LineNo);
					AssertEquals("Prereq2: doc8 has CSI_LineNo 8", 8, doc8.CSI_LineNo);
					AssertEquals("Prereq2: doc9 has CSI_LineNo 9", 9, doc9.CSI_LineNo);

					movementHeader.BM_CustomsStatus = "AAA";
					NctsHeaderDocumentsSequenceNumberHelper.ResetSupportingDocumentsLineNoWhenPhase5(nctsHeader, nctsHeader.MovementHeader, forceReset: true);

					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE but forceReset is true: doc1 has CSI_LineNo 0", 0, doc1.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE but forceReset is true: doc2 has CSI_LineNo 0", 0, doc2.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE but forceReset is true: doc3 has CSI_LineNo 0", 0, doc3.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE but forceReset is true: doc4 has CSI_LineNo 0", 0, doc4.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE but forceReset is true: doc5 has CSI_LineNo 0", 0, doc5.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE but forceReset is true: doc6 has CSI_LineNo 0", 0, doc6.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE but forceReset is true: doc7 has CSI_LineNo 0", 0, doc7.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE but forceReset is true: doc8 has CSI_LineNo 0", 0, doc8.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE but forceReset is true: doc9 has CSI_LineNo 0", 0, doc9.CSI_LineNo);
				});
			}
		}

		public void TestResetSupportingDocumentsLineNoWhenPhase5_FromHeader_NoTransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
			{
				var doc1 = goodsItem11.SupportingDocuments.AddNew();
				doc1.CSI_Code = "9001";
				doc1.CSI_LineNo = 1;

				var doc2 = goodsItem11.SupportingDocuments.AddNew();
				doc2.CSI_Code = "A002";
				doc2.CSI_LineNo = 2;

				var doc3 = goodsItem12.SupportingDocuments.AddNew();
				doc3.CSI_Code = "9003";
				doc3.CSI_LineNo = 3;

				var doc4 = goodsItem21.SupportingDocuments.AddNew();
				doc4.CSI_Code = "Y004";
				doc4.CSI_LineNo = 4;

				var doc5 = nctsBill1.SupportingDocuments.AddNew();
				doc5.CSI_Code = "9005";
				doc5.CSI_LineNo = 5;

				var doc6 = nctsBill1.SupportingDocuments.AddNew();
				doc6.CSI_Code = "C006";
				doc6.CSI_LineNo = 6;

				var doc7 = nctsBill2.SupportingDocuments.AddNew();
				doc7.CSI_Code = "9007";
				doc7.CSI_LineNo = 7;

				var doc8 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				doc8.CSI_Code = "A008";
				doc8.CSI_LineNo = 8;

				var doc9 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				doc9.CSI_Code = "9009";
				doc9.CSI_LineNo = 9;

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: doc1 has CSI_LineNo 1", 1, doc1.CSI_LineNo);
					AssertEquals("Prereq: doc2 has CSI_LineNo 2", 2, doc2.CSI_LineNo);
					AssertEquals("Prereq: doc3 has CSI_LineNo 3", 3, doc3.CSI_LineNo);
					AssertEquals("Prereq: doc4 has CSI_LineNo 4", 4, doc4.CSI_LineNo);
					AssertEquals("Prereq: doc5 has CSI_LineNo 5", 5, doc5.CSI_LineNo);
					AssertEquals("Prereq: doc6 has CSI_LineNo 6", 6, doc6.CSI_LineNo);
					AssertEquals("Prereq: doc7 has CSI_LineNo 7", 7, doc7.CSI_LineNo);
					AssertEquals("Prereq: doc8 has CSI_LineNo 8", 8, doc8.CSI_LineNo);
					AssertEquals("Prereq: doc9 has CSI_LineNo 9", 9, doc9.CSI_LineNo);

					NctsHeaderDocumentsSequenceNumberHelper.ResetSupportingDocumentsLineNoWhenPhase5(nctsHeader, nctsHeader);

					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc1 has CSI_LineNo 1", 1, doc1.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc2 has CSI_LineNo 2", 2, doc2.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc3 has CSI_LineNo 3", 3, doc3.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc4 has CSI_LineNo 4", 4, doc4.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc5 has CSI_LineNo 5", 5, doc5.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc6 has CSI_LineNo 6", 6, doc6.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc7 has CSI_LineNo 7", 7, doc7.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc8 has CSI_LineNo 8", 8, doc8.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty: doc9 has CSI_LineNo 9", 9, doc9.CSI_LineNo);

					nctsHeader.MovementHeader.BM_CustomsStatus = "AAA";
					NctsHeaderDocumentsSequenceNumberHelper.ResetSupportingDocumentsLineNoWhenPhase5(nctsHeader, nctsHeader);

					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc1 has CSI_LineNo 1", 1, doc1.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc2 has CSI_LineNo 2", 2, doc2.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc3 has CSI_LineNo 3", 3, doc3.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc4 has CSI_LineNo 4", 4, doc4.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc5 has CSI_LineNo 5", 5, doc5.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc6 has CSI_LineNo 6", 6, doc6.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc7 has CSI_LineNo 7", 7, doc7.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc8 has CSI_LineNo 8", 8, doc8.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty and not PRE: doc9 has CSI_LineNo 9", 9, doc9.CSI_LineNo);

					nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
					NctsHeaderDocumentsSequenceNumberHelper.ResetSupportingDocumentsLineNoWhenPhase5(nctsHeader, nctsHeader);

					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc1 has CSI_LineNo 1", 1, doc1.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc2 has CSI_LineNo 2", 2, doc2.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc3 has CSI_LineNo 3", 3, doc3.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc4 has CSI_LineNo 4", 4, doc4.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc5 has CSI_LineNo 5", 5, doc5.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc6 has CSI_LineNo 6", 6, doc6.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc7 has CSI_LineNo 7", 7, doc7.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc8 has CSI_LineNo 8", 8, doc8.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is PRE: doc9 has CSI_LineNo 9", 9, doc9.CSI_LineNo);

					nctsHeader.MovementHeader.BM_CustomsStatus = "AAA";
					NctsHeaderDocumentsSequenceNumberHelper.ResetSupportingDocumentsLineNoWhenPhase5(nctsHeader, nctsHeader, forceReset: true);

					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE and forceReset is true: doc1 has CSI_LineNo 1", 1, doc1.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE and forceReset is true: doc2 has CSI_LineNo 2", 2, doc2.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE and forceReset is true: doc3 has CSI_LineNo 3", 3, doc3.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE and forceReset is true: doc4 has CSI_LineNo 4", 4, doc4.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE and forceReset is true: doc5 has CSI_LineNo 5", 5, doc5.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE and forceReset is true: doc6 has CSI_LineNo 6", 6, doc6.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE and forceReset is true: doc7 has CSI_LineNo 7", 7, doc7.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE and forceReset is true: doc8 has CSI_LineNo 8", 8, doc8.CSI_LineNo);
					AssertEquals("When declaration is Departure Phase5 and Customs Status is not PRE and forceReset is true: doc9 has CSI_LineNo 9", 9, doc9.CSI_LineNo);
				});
			}
		}

		public void TestResetAdditionalDocumentsLineNoWhenPhase5_FromGoodsItem_TransitionPeriod_INF()
		{
			AssertResetAdditionalDocuments_INF(true, goodsItem11);
		}

		public void TestResetAdditionalDocumentsLineNoWhenPhase5_FromGoodsItem_NoTransitionPeriod_INF()
		{
			AssertResetAdditionalDocuments_INF(false, goodsItem11);
		}

		public void TestResetAdditionalDocumentsLineNoWhenPhase5_FromBill_TransitionPeriod_INF()
		{
			AssertResetAdditionalDocuments_INF(true, nctsBill1);
		}

		public void TestResetAdditionalDocumentsLineNoWhenPhase5_FromBill_NoTransitionPeriod_INF()
		{
			AssertResetAdditionalDocuments_INF(false, nctsBill1);
		}

		public void TestResetAdditionalDocumentsLineNoWhenPhase5_FromHeader_TransitionPeriod_INF()
		{
			AssertResetAdditionalDocuments_INF(true, nctsHeader);
		}

		public void TestResetAdditionalDocumentsLineNoWhenPhase5_FromHeader_NoTransitionPeriod_INF()
		{
			AssertResetAdditionalDocuments_INF(false, nctsHeader);
		}

		void AssertResetAdditionalDocuments_INF(bool isTransitionPeriod, Integration.Customs.ICusSupportingInfoTypeSupporter parent)
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, isTransitionPeriod))
			{
				var doc1 = goodsItem11.AdditionalInfos.AddNew();
				doc1.CSI_Code = "9001";
				doc1.CSI_SubType = "INF";
				doc1.CSI_LineNo = 1;

				var doc2 = goodsItem11.AdditionalInfos.AddNew();
				doc2.CSI_Code = "A002";
				doc2.CSI_SubType = "INF";
				doc2.CSI_LineNo = 2;

				var doc3 = goodsItem11.AdditionalInfos.AddNew();
				doc3.CSI_Code = "A003";
				doc3.CSI_SubType = "REF";

				var doc4 = goodsItem11.AdditionalInfos.AddNew();
				doc4.CSI_Code = "A004";
				doc4.CSI_SubType = "TRA";
				doc4.CSI_LineNo = 4;

				var doc5 = goodsItem12.AdditionalInfos.AddNew();
				doc5.CSI_Code = "9005";
				doc5.CSI_SubType = "INF";
				doc5.CSI_LineNo = 5;

				var doc6 = goodsItem21.AdditionalInfos.AddNew();
				doc6.CSI_Code = "Y006";
				doc6.CSI_SubType = "INF";
				doc6.CSI_LineNo = 6;

				var doc7 = nctsBill1.AdditionalDocuments.AddNew();
				doc7.CSI_Code = "9007";
				doc7.CSI_SubType = "INF";
				doc7.CSI_LineNo = 7;

				var doc8 = nctsBill1.AdditionalDocuments.AddNew();
				doc8.CSI_Code = "C008";
				doc8.CSI_SubType = "INF";
				doc8.CSI_LineNo = 8;

				var doc9 = nctsBill1.AdditionalDocuments.AddNew();
				doc9.CSI_Code = "C009";
				doc9.CSI_SubType = "REF";
				doc9.CSI_LineNo = 9;

				var doc10 = nctsBill1.AdditionalDocuments.AddNew();
				doc10.CSI_Code = "C010";
				doc10.CSI_SubType = "TRA";
				doc10.CSI_LineNo = 10;

				var doc11 = nctsBill2.AdditionalDocuments.AddNew();
				doc11.CSI_Code = "9011";
				doc11.CSI_SubType = "INF";
				doc11.CSI_LineNo = 11;

				var doc12 = nctsHeader.AdditionalDocuments.AddNew();
				doc12.CSI_Code = "A012";
				doc12.CSI_SubType = "INF";
				doc12.CSI_LineNo = 12;

				var doc13 = nctsHeader.AdditionalDocuments.AddNew();
				doc13.CSI_Code = "9013";
				doc13.CSI_SubType = "INF";
				doc13.CSI_LineNo = 13;

				var doc14 = nctsHeader.AdditionalDocuments.AddNew();
				doc14.CSI_Code = "9014";
				doc14.CSI_SubType = "REF";

				var doc15 = nctsHeader.AdditionalDocuments.AddNew();
				doc15.CSI_Code = "9015";
				doc15.CSI_SubType = "TRA";
				doc15.CSI_LineNo = 15;

				doc3.CSI_LineNo = 3;
				doc14.CSI_LineNo = 14;

				CombineAssertions("Force Reset false", () =>
				{
					AssertDocumentsLinePreReq(doc1, doc2, doc3, doc4, doc5, doc6, doc7, doc8, doc9, doc10, doc11, doc12, doc13, doc14, doc15);

					NctsHeaderDocumentsSequenceNumberHelper.ResetAdditionalDocumentsLineNoWhenPhase5(nctsHeader, parent, AdditionalInfoSubTypeList.Codes.AdditionalInformation);

					AssertDocumentsLineNoWhenCustomsStatusEmptyOrNotPREAndNoForceReset(doc1, doc2, doc3, doc4, doc5, doc6, doc7, doc8, doc9, doc10, doc11, doc12, doc13, doc14, doc15);

					nctsHeader.MovementHeader.BM_CustomsStatus = "AAA";
					NctsHeaderDocumentsSequenceNumberHelper.ResetAdditionalDocumentsLineNoWhenPhase5(nctsHeader, parent, AdditionalInfoSubTypeList.Codes.AdditionalInformation);

					AssertDocumentsLineNoWhenCustomsStatusEmptyOrNotPREAndNoForceReset(doc1, doc2, doc3, doc4, doc5, doc6, doc7, doc8, doc9, doc10, doc11, doc12, doc13, doc14, doc15);

					nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
					NctsHeaderDocumentsSequenceNumberHelper.ResetAdditionalDocumentsLineNoWhenPhase5(nctsHeader, parent, AdditionalInfoSubTypeList.Codes.AdditionalInformation);

					if (isTransitionPeriod)
					{
						if (parent == goodsItem11)
						{
							AssertDocumentsLineNoWhenTransitionPeriodAndCustomsStatusPREOrForcedAndGoodsItemParent(doc1, doc2, doc3, doc4, doc5, doc6, doc7, doc8, doc9, doc10, doc11, doc12, doc13, doc14, doc15);
						}
						else if (parent == nctsBill1)
						{
							AssertDocumentsLineNoWhenTransitionPeriodAndCustomsStatusPREOrForcedAndBillParent(doc1, doc2, doc3, doc4, doc5, doc6, doc7, doc8, doc9, doc10, doc11, doc12, doc13, doc14, doc15);
						}
						else
						{
							AssertDocumentsLineNoWhenTransitionPeriodAndCustomsStatusPREOrForcedAndHeaderParent(doc1, doc2, doc3, doc4, doc5, doc6, doc7, doc8, doc9, doc10, doc11, doc12, doc13, doc14, doc15);
						}
					}
					else
					{
						AssertDocumentsLineNoWhenNoTransitionPeriod(doc1, doc2, doc3, doc4, doc5, doc6, doc7, doc8, doc9, doc10, doc11, doc12, doc13, doc14, doc15);
					}
				});

				CombineAssertions("Force Reset true", () =>
				{
					doc1.CSI_LineNo = 1;
					doc2.CSI_LineNo = 2;
					doc3.CSI_LineNo = 3;
					doc4.CSI_LineNo = 4;
					doc5.CSI_LineNo = 5;
					doc6.CSI_LineNo = 6;
					doc7.CSI_LineNo = 7;
					doc8.CSI_LineNo = 8;
					doc9.CSI_LineNo = 9;
					doc10.CSI_LineNo = 10;
					doc11.CSI_LineNo = 11;
					doc12.CSI_LineNo = 12;
					doc13.CSI_LineNo = 13;
					doc14.CSI_LineNo = 14;
					doc15.CSI_LineNo = 15;

					AssertDocumentsLinePreReq(doc1, doc2, doc3, doc4, doc5, doc6, doc7, doc8, doc9, doc10, doc11, doc12, doc13, doc14, doc15);

					nctsHeader.MovementHeader.BM_CustomsStatus = "AAA";
					NctsHeaderDocumentsSequenceNumberHelper.ResetAdditionalDocumentsLineNoWhenPhase5(nctsHeader, parent, AdditionalInfoSubTypeList.Codes.AdditionalInformation, forceReset: true);

					if (isTransitionPeriod)
					{
						if (parent == goodsItem11)
						{
							AssertDocumentsLineNoWhenTransitionPeriodAndCustomsStatusPREOrForcedAndGoodsItemParent(doc1, doc2, doc3, doc4, doc5, doc6, doc7, doc8, doc9, doc10, doc11, doc12, doc13, doc14, doc15);
						}
						else if (parent == nctsBill1)
						{
							AssertDocumentsLineNoWhenTransitionPeriodAndCustomsStatusPREOrForcedAndBillParent(doc1, doc2, doc3, doc4, doc5, doc6, doc7, doc8, doc9, doc10, doc11, doc12, doc13, doc14, doc15);
						}
						else
						{
							AssertDocumentsLineNoWhenTransitionPeriodAndCustomsStatusPREOrForcedAndHeaderParent(doc1, doc2, doc3, doc4, doc5, doc6, doc7, doc8, doc9, doc10, doc11, doc12, doc13, doc14, doc15);
						}
					}
					else
					{
						AssertDocumentsLineNoWhenNoTransitionPeriod(doc1, doc2, doc3, doc4, doc5, doc6, doc7, doc8, doc9, doc10, doc11, doc12, doc13, doc14, doc15);
					}
				});
			}
		}

		public void TestResetAdditionalDocumentsLineNoWhenPhase5_FromGoodsItem_TransitionPeriod_REF()
		{
			AssertResetAdditionalDocuments_REF(true, goodsItem11);
		}

		public void TestResetAdditionalDocumentsLineNoWhenPhase5_FromGoodsItem_NoTransitionPeriod_REF()
		{
			AssertResetAdditionalDocuments_REF(false, goodsItem11);
		}

		public void TestResetAdditionalDocumentsLineNoWhenPhase5_FromBill_TransitionPeriod_REF()
		{
			AssertResetAdditionalDocuments_REF(true, nctsBill1);
		}

		public void TestResetAdditionalDocumentsLineNoWhenPhase5_FromBill_NoTransitionPeriod_REF()
		{
			AssertResetAdditionalDocuments_REF(false, nctsBill1);
		}

		public void TestResetAdditionalDocumentsLineNoWhenPhase5_FromHeader_TransitionPeriod_REF()
		{
			AssertResetAdditionalDocuments_REF(true, nctsHeader);
		}

		public void TestResetAdditionalDocumentsLineNoWhenPhase5_FromHeader_NoTransitionPeriod_REF()
		{
			AssertResetAdditionalDocuments_REF(false, nctsHeader);
		}

		void AssertResetAdditionalDocuments_REF(bool isTransitionPeriod, Integration.Customs.ICusSupportingInfoTypeSupporter parent)
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, isTransitionPeriod))
			{
				var doc1 = goodsItem11.AdditionalInfos.AddNew();
				doc1.CSI_Code = "9001";
				doc1.CSI_SubType = "REF";
				doc1.CSI_LineNo = 1;

				var doc2 = goodsItem11.AdditionalInfos.AddNew();
				doc2.CSI_Code = "A002";
				doc2.CSI_SubType = "REF";
				doc2.CSI_LineNo = 2;

				var doc3 = goodsItem11.AdditionalInfos.AddNew();
				doc3.CSI_Code = "A003";
				doc3.CSI_SubType = "INF";
				doc3.CSI_LineNo = 3;

				var doc4 = goodsItem11.AdditionalInfos.AddNew();
				doc4.CSI_Code = "A004";
				doc4.CSI_SubType = "TRA";
				doc4.CSI_LineNo = 4;

				var doc5 = goodsItem12.AdditionalInfos.AddNew();
				doc5.CSI_Code = "9005";
				doc5.CSI_SubType = "REF";
				doc5.CSI_LineNo = 5;

				var doc6 = goodsItem21.AdditionalInfos.AddNew();
				doc6.CSI_Code = "Y006";
				doc6.CSI_SubType = "REF";
				doc6.CSI_LineNo = 6;

				var doc7 = nctsBill1.AdditionalDocuments.AddNew();
				doc7.CSI_Code = "9007";
				doc7.CSI_SubType = "REF";
				doc7.CSI_LineNo = 7;

				var doc8 = nctsBill1.AdditionalDocuments.AddNew();
				doc8.CSI_Code = "C008";
				doc8.CSI_SubType = "REF";
				doc8.CSI_LineNo = 8;

				var doc9 = nctsBill1.AdditionalDocuments.AddNew();
				doc9.CSI_Code = "C009";
				doc9.CSI_SubType = "INF";
				doc9.CSI_LineNo = 9;

				var doc10 = nctsBill1.AdditionalDocuments.AddNew();
				doc10.CSI_Code = "C010";
				doc10.CSI_SubType = "TRA";
				doc10.CSI_LineNo = 10;

				var doc11 = nctsBill2.AdditionalDocuments.AddNew();
				doc11.CSI_Code = "9011";
				doc11.CSI_SubType = "REF";
				doc11.CSI_LineNo = 11;

				var doc12 = nctsHeader.AdditionalDocuments.AddNew();
				doc12.CSI_Code = "A012";
				doc12.CSI_SubType = "REF";

				var doc13 = nctsHeader.AdditionalDocuments.AddNew();
				doc13.CSI_Code = "9013";
				doc13.CSI_SubType = "REF";

				var doc14 = nctsHeader.AdditionalDocuments.AddNew();
				doc14.CSI_Code = "9014";
				doc14.CSI_SubType = "INF";
				doc14.CSI_LineNo = 14;

				var doc15 = nctsHeader.AdditionalDocuments.AddNew();
				doc15.CSI_Code = "9015";
				doc15.CSI_SubType = "TRA";
				doc15.CSI_LineNo = 15;

				doc12.CSI_LineNo = 12;
				doc13.CSI_LineNo = 13;

				CombineAssertions("Force Reset false", () =>
				{
					AssertDocumentsLinePreReq(doc1, doc2, doc3, doc4, doc5, doc6, doc7, doc8, doc9, doc10, doc11, doc12, doc13, doc14, doc15);

					NctsHeaderDocumentsSequenceNumberHelper.ResetAdditionalDocumentsLineNoWhenPhase5(nctsHeader, parent, AdditionalInfoSubTypeList.Codes.AdditionalReference);

					AssertDocumentsLineNoWhenCustomsStatusEmptyOrNotPREAndNoForceReset(doc1, doc2, doc3, doc4, doc5, doc6, doc7, doc8, doc9, doc10, doc11, doc12, doc13, doc14, doc15);

					nctsHeader.MovementHeader.BM_CustomsStatus = "AAA";
					NctsHeaderDocumentsSequenceNumberHelper.ResetAdditionalDocumentsLineNoWhenPhase5(nctsHeader, parent, AdditionalInfoSubTypeList.Codes.AdditionalReference);

					AssertDocumentsLineNoWhenCustomsStatusEmptyOrNotPREAndNoForceReset(doc1, doc2, doc3, doc4, doc5, doc6, doc7, doc8, doc9, doc10, doc11, doc12, doc13, doc14, doc15);

					nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
					NctsHeaderDocumentsSequenceNumberHelper.ResetAdditionalDocumentsLineNoWhenPhase5(nctsHeader, parent, AdditionalInfoSubTypeList.Codes.AdditionalReference);

					if (isTransitionPeriod)
					{
						if (parent == goodsItem11)
						{
							AssertDocumentsLineNoWhenTransitionPeriodAndCustomsStatusPREOrForcedAndGoodsItemParent(doc1, doc2, doc3, doc4, doc5, doc6, doc7, doc8, doc9, doc10, doc11, doc12, doc13, doc14, doc15);
						}
						else if (parent == nctsBill1)
						{
							AssertDocumentsLineNoWhenTransitionPeriodAndCustomsStatusPREOrForcedAndBillParent(doc1, doc2, doc3, doc4, doc5, doc6, doc7, doc8, doc9, doc10, doc11, doc12, doc13, doc14, doc15);
						}
						else
						{
							AssertDocumentsLineNoWhenTransitionPeriodAndCustomsStatusPREOrForcedAndHeaderParent(doc1, doc2, doc3, doc4, doc5, doc6, doc7, doc8, doc9, doc10, doc11, doc12, doc13, doc14, doc15);
						}
					}
					else
					{
						AssertDocumentsLineNoWhenNoTransitionPeriod(doc1, doc2, doc3, doc4, doc5, doc6, doc7, doc8, doc9, doc10, doc11, doc12, doc13, doc14, doc15);
					}
				});

				CombineAssertions("Force Reset true", () =>
				{
					doc1.CSI_LineNo = 1;
					doc2.CSI_LineNo = 2;
					doc3.CSI_LineNo = 3;
					doc4.CSI_LineNo = 4;
					doc5.CSI_LineNo = 5;
					doc6.CSI_LineNo = 6;
					doc7.CSI_LineNo = 7;
					doc8.CSI_LineNo = 8;
					doc9.CSI_LineNo = 9;
					doc10.CSI_LineNo = 10;
					doc11.CSI_LineNo = 11;
					doc12.CSI_LineNo = 12;
					doc13.CSI_LineNo = 13;
					doc14.CSI_LineNo = 14;
					doc15.CSI_LineNo = 15;

					AssertDocumentsLinePreReq(doc1, doc2, doc3, doc4, doc5, doc6, doc7, doc8, doc9, doc10, doc11, doc12, doc13, doc14, doc15);

					nctsHeader.MovementHeader.BM_CustomsStatus = "AAA";
					NctsHeaderDocumentsSequenceNumberHelper.ResetAdditionalDocumentsLineNoWhenPhase5(nctsHeader, parent, AdditionalInfoSubTypeList.Codes.AdditionalReference, forceReset: true);

					if (isTransitionPeriod)
					{
						if (parent == goodsItem11)
						{
							AssertDocumentsLineNoWhenTransitionPeriodAndCustomsStatusPREOrForcedAndGoodsItemParent(doc1, doc2, doc3, doc4, doc5, doc6, doc7, doc8, doc9, doc10, doc11, doc12, doc13, doc14, doc15);
						}
						else if (parent == nctsBill1)
						{
							AssertDocumentsLineNoWhenTransitionPeriodAndCustomsStatusPREOrForcedAndBillParent(doc1, doc2, doc3, doc4, doc5, doc6, doc7, doc8, doc9, doc10, doc11, doc12, doc13, doc14, doc15);
						}
						else
						{
							AssertDocumentsLineNoWhenTransitionPeriodAndCustomsStatusPREOrForcedAndHeaderParent(doc1, doc2, doc3, doc4, doc5, doc6, doc7, doc8, doc9, doc10, doc11, doc12, doc13, doc14, doc15);
						}
					}
					else
					{
						AssertDocumentsLineNoWhenNoTransitionPeriod(doc1, doc2, doc3, doc4, doc5, doc6, doc7, doc8, doc9, doc10, doc11, doc12, doc13, doc14, doc15);
					}
				});
			}
		}

		public void TestResetAdditionalDocumentsLineNoWhenPhase5_FromGoodsItem_TransitionPeriod_TRA()
		{
			AssertResetAdditionalDocuments_TRA(true, goodsItem11);
		}

		public void TestResetAdditionalDocumentsLineNoWhenPhase5_FromGoodsItem_NoTransitionPeriod_TRA()
		{
			AssertResetAdditionalDocuments_TRA(false, goodsItem11);
		}

		public void TestResetAdditionalDocumentsLineNoWhenPhase5_FromBill_TransitionPeriod_TRA()
		{
			AssertResetAdditionalDocuments_TRA(true, nctsBill1);
		}

		public void TestResetAdditionalDocumentsLineNoWhenPhase5_FromBill_NoTransitionPeriod_TRA()
		{
			AssertResetAdditionalDocuments_TRA(false, nctsBill1);
		}

		public void TestResetAdditionalDocumentsLineNoWhenPhase5_FromHeader_TransitionPeriod_TRA()
		{
			AssertResetAdditionalDocuments_TRA(true, nctsHeader);
		}

		public void TestResetAdditionalDocumentsLineNoWhenPhase5_FromHeader_NoTransitionPeriod_TRA()
		{
			AssertResetAdditionalDocuments_TRA(false, nctsHeader);
		}

		void AssertResetAdditionalDocuments_TRA(bool isTransitionPeriod, Integration.Customs.ICusSupportingInfoTypeSupporter parent)
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, isTransitionPeriod))
			{
				var doc1 = goodsItem11.AdditionalInfos.AddNew();
				doc1.CSI_Code = "9001";
				doc1.CSI_SubType = "TRA";
				doc1.CSI_LineNo = 1;

				var doc2 = goodsItem11.AdditionalInfos.AddNew();
				doc2.CSI_Code = "A002";
				doc2.CSI_SubType = "TRA";
				doc2.CSI_LineNo = 2;

				var doc3 = goodsItem11.AdditionalInfos.AddNew();
				doc3.CSI_Code = "A003";
				doc3.CSI_SubType = "INF";
				doc3.CSI_LineNo = 3;

				var doc4 = goodsItem11.AdditionalInfos.AddNew();
				doc4.CSI_Code = "A004";
				doc4.CSI_SubType = "REF";
				doc4.CSI_LineNo = 4;

				var doc5 = goodsItem12.AdditionalInfos.AddNew();
				doc5.CSI_Code = "9005";
				doc5.CSI_SubType = "TRA";
				doc5.CSI_LineNo = 5;

				var doc6 = goodsItem21.AdditionalInfos.AddNew();
				doc6.CSI_Code = "Y006";
				doc6.CSI_SubType = "TRA";
				doc6.CSI_LineNo = 6;

				var doc7 = nctsBill1.AdditionalDocuments.AddNew();
				doc7.CSI_Code = "9007";
				doc7.CSI_SubType = "TRA";
				doc7.CSI_LineNo = 7;

				var doc8 = nctsBill1.AdditionalDocuments.AddNew();
				doc8.CSI_Code = "C008";
				doc8.CSI_SubType = "TRA";
				doc8.CSI_LineNo = 8;

				var doc9 = nctsBill1.AdditionalDocuments.AddNew();
				doc9.CSI_Code = "C009";
				doc9.CSI_SubType = "INF";
				doc9.CSI_LineNo = 9;

				var doc10 = nctsBill1.AdditionalDocuments.AddNew();
				doc10.CSI_Code = "C010";
				doc10.CSI_SubType = "REF";
				doc10.CSI_LineNo = 10;

				var doc11 = nctsBill2.AdditionalDocuments.AddNew();
				doc11.CSI_Code = "9011";
				doc11.CSI_SubType = "TRA";
				doc11.CSI_LineNo = 11;

				var doc12 = nctsHeader.AdditionalDocuments.AddNew();
				doc12.CSI_Code = "A012";
				doc12.CSI_SubType = "TRA";
				doc12.CSI_LineNo = 12;

				var doc13 = nctsHeader.AdditionalDocuments.AddNew();
				doc13.CSI_Code = "9013";
				doc13.CSI_SubType = "TRA";
				doc13.CSI_LineNo = 13;

				var doc14 = nctsHeader.AdditionalDocuments.AddNew();
				doc14.CSI_Code = "9014";
				doc14.CSI_SubType = "INF";
				doc14.CSI_LineNo = 14;

				var doc15 = nctsHeader.AdditionalDocuments.AddNew();
				doc15.CSI_Code = "9015";
				doc15.CSI_SubType = "REF";
				doc15.CSI_LineNo = 15;

				CombineAssertions("Force Reset false", () =>
				{
					AssertDocumentsLinePreReq(doc1, doc2, doc3, doc4, doc5, doc6, doc7, doc8, doc9, doc10, doc11, doc12, doc13, doc14, doc15);

					NctsHeaderDocumentsSequenceNumberHelper.ResetAdditionalDocumentsLineNoWhenPhase5(nctsHeader, parent, AdditionalInfoSubTypeList.Codes.TransportDocument);

					AssertDocumentsLineNoWhenCustomsStatusEmptyOrNotPREAndNoForceReset(doc1, doc2, doc3, doc4, doc5, doc6, doc7, doc8, doc9, doc10, doc11, doc12, doc13, doc14, doc15);

					nctsHeader.MovementHeader.BM_CustomsStatus = "AAA";
					NctsHeaderDocumentsSequenceNumberHelper.ResetAdditionalDocumentsLineNoWhenPhase5(nctsHeader, parent, AdditionalInfoSubTypeList.Codes.TransportDocument);

					AssertDocumentsLineNoWhenCustomsStatusEmptyOrNotPREAndNoForceReset(doc1, doc2, doc3, doc4, doc5, doc6, doc7, doc8, doc9, doc10, doc11, doc12, doc13, doc14, doc15);

					nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
					NctsHeaderDocumentsSequenceNumberHelper.ResetAdditionalDocumentsLineNoWhenPhase5(nctsHeader, parent, AdditionalInfoSubTypeList.Codes.TransportDocument);

					if (isTransitionPeriod)
					{
						if (parent == goodsItem11)
						{
							AssertDocumentsLineNoWhenTransitionPeriodAndCustomsStatusPREOrForcedAndGoodsItemParent(doc1, doc2, doc3, doc4, doc5, doc6, doc7, doc8, doc9, doc10, doc11, doc12, doc13, doc14, doc15);
						}
						else if (parent == nctsBill1)
						{
							AssertDocumentsLineNoWhenTransitionPeriodAndCustomsStatusPREOrForcedAndBillParent(doc1, doc2, doc3, doc4, doc5, doc6, doc7, doc8, doc9, doc10, doc11, doc12, doc13, doc14, doc15);
						}
						else
						{
							AssertDocumentsLineNoWhenTransitionPeriodAndCustomsStatusPREOrForcedAndHeaderParent(doc1, doc2, doc3, doc4, doc5, doc6, doc7, doc8, doc9, doc10, doc11, doc12, doc13, doc14, doc15);
						}
					}
					else
					{
						AssertDocumentsLineNoWhenNoTransitionPeriod(doc1, doc2, doc3, doc4, doc5, doc6, doc7, doc8, doc9, doc10, doc11, doc12, doc13, doc14, doc15);
					}
				});

				CombineAssertions("Force Reset true", () =>
				{
					doc1.CSI_LineNo = 1;
					doc2.CSI_LineNo = 2;
					doc3.CSI_LineNo = 3;
					doc4.CSI_LineNo = 4;
					doc5.CSI_LineNo = 5;
					doc6.CSI_LineNo = 6;
					doc7.CSI_LineNo = 7;
					doc8.CSI_LineNo = 8;
					doc9.CSI_LineNo = 9;
					doc10.CSI_LineNo = 10;
					doc11.CSI_LineNo = 11;
					doc12.CSI_LineNo = 12;
					doc13.CSI_LineNo = 13;
					doc14.CSI_LineNo = 14;
					doc15.CSI_LineNo = 15;

					AssertDocumentsLinePreReq(doc1, doc2, doc3, doc4, doc5, doc6, doc7, doc8, doc9, doc10, doc11, doc12, doc13, doc14, doc15);

					nctsHeader.MovementHeader.BM_CustomsStatus = "AAA";
					NctsHeaderDocumentsSequenceNumberHelper.ResetAdditionalDocumentsLineNoWhenPhase5(nctsHeader, parent, AdditionalInfoSubTypeList.Codes.TransportDocument, forceReset: true);

					if (isTransitionPeriod)
					{
						if (parent == goodsItem11)
						{
							AssertDocumentsLineNoWhenTransitionPeriodAndCustomsStatusPREOrForcedAndGoodsItemParent(doc1, doc2, doc3, doc4, doc5, doc6, doc7, doc8, doc9, doc10, doc11, doc12, doc13, doc14, doc15);
						}
						else if (parent == nctsBill1)
						{
							AssertDocumentsLineNoWhenTransitionPeriodAndCustomsStatusPREOrForcedAndBillParent(doc1, doc2, doc3, doc4, doc5, doc6, doc7, doc8, doc9, doc10, doc11, doc12, doc13, doc14, doc15);
						}
						else
						{
							AssertDocumentsLineNoWhenTransitionPeriodAndCustomsStatusPREOrForcedAndHeaderParent(doc1, doc2, doc3, doc4, doc5, doc6, doc7, doc8, doc9, doc10, doc11, doc12, doc13, doc14, doc15);
						}
					}
					else
					{
						AssertDocumentsLineNoWhenNoTransitionPeriod(doc1, doc2, doc3, doc4, doc5, doc6, doc7, doc8, doc9, doc10, doc11, doc12, doc13, doc14, doc15);
					}
				});
			}
		}

		void AssertDocumentsLinePreReq(CusSupportingInfo doc1, CusSupportingInfo doc2, CusSupportingInfo doc3, CusSupportingInfo doc4, CusSupportingInfo doc5, CusSupportingInfo doc6, CusSupportingInfo doc7,
			CusSupportingInfo doc8, CusSupportingInfo doc9, CusSupportingInfo doc10, CusSupportingInfo doc11, CusSupportingInfo doc12, CusSupportingInfo doc13, CusSupportingInfo doc14, CusSupportingInfo doc15)
		{
			AssertEquals("Prereq: doc1 has CSI_LineNo 1", 1, doc1.CSI_LineNo);
			AssertEquals("Prereq: doc2 has CSI_LineNo 2", 2, doc2.CSI_LineNo);
			AssertEquals("Prereq: doc3 has CSI_LineNo 3", 3, doc3.CSI_LineNo);
			AssertEquals("Prereq: doc4 has CSI_LineNo 4", 4, doc4.CSI_LineNo);
			AssertEquals("Prereq: doc5 has CSI_LineNo 5", 5, doc5.CSI_LineNo);
			AssertEquals("Prereq: doc6 has CSI_LineNo 6", 6, doc6.CSI_LineNo);
			AssertEquals("Prereq: doc7 has CSI_LineNo 7", 7, doc7.CSI_LineNo);
			AssertEquals("Prereq: doc8 has CSI_LineNo 8", 8, doc8.CSI_LineNo);
			AssertEquals("Prereq: doc9 has CSI_LineNo 9", 9, doc9.CSI_LineNo);
			AssertEquals("Prereq: doc10 has CSI_LineNo 10", 10, doc10.CSI_LineNo);
			AssertEquals("Prereq: doc11 has CSI_LineNo 11", 11, doc11.CSI_LineNo);
			AssertEquals("Prereq: doc12 has CSI_LineNo 12", 12, doc12.CSI_LineNo);
			AssertEquals("Prereq: doc13 has CSI_LineNo 13", 13, doc13.CSI_LineNo);
			AssertEquals("Prereq: doc14 has CSI_LineNo 14", 14, doc14.CSI_LineNo);
			AssertEquals("Prereq: doc15 has CSI_LineNo 15", 15, doc15.CSI_LineNo);
		}

		void AssertDocumentsLineNoWhenCustomsStatusEmptyOrNotPREAndNoForceReset(CusSupportingInfo doc1, CusSupportingInfo doc2, CusSupportingInfo doc3, CusSupportingInfo doc4, CusSupportingInfo doc5, CusSupportingInfo doc6,
			CusSupportingInfo doc7, CusSupportingInfo doc8, CusSupportingInfo doc9, CusSupportingInfo doc10, CusSupportingInfo doc11, CusSupportingInfo doc12, CusSupportingInfo doc13, CusSupportingInfo doc14, CusSupportingInfo doc15)
		{
			AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty or not PRE: doc1 has CSI_LineNo 1", 1, doc1.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty or not PRE: doc2 has CSI_LineNo 2", 2, doc2.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty or not PRE: doc3 has CSI_LineNo 3", 3, doc3.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty or not PRE: doc4 has CSI_LineNo 4", 4, doc4.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty or not PRE: doc5 has CSI_LineNo 5", 5, doc5.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty or not PRE: doc6 has CSI_LineNo 6", 6, doc6.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty or not PRE: doc7 has CSI_LineNo 7", 7, doc7.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty or not PRE: doc8 has CSI_LineNo 8", 8, doc8.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty or not PRE: doc9 has CSI_LineNo 9", 9, doc9.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty or not PRE: doc10 has CSI_LineNo 10", 10, doc10.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty or not PRE: doc11 has CSI_LineNo 11", 11, doc11.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty or not PRE: doc12 has CSI_LineNo 12", 12, doc12.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty or not PRE: doc13 has CSI_LineNo 13", 13, doc13.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty or not PRE: doc14 has CSI_LineNo 14", 14, doc14.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is Empty or not PRE: doc15 has CSI_LineNo 15", 15, doc15.CSI_LineNo);
		}

		void AssertDocumentsLineNoWhenTransitionPeriodAndCustomsStatusPREOrForcedAndGoodsItemParent(CusSupportingInfo doc1, CusSupportingInfo doc2, CusSupportingInfo doc3, CusSupportingInfo doc4, CusSupportingInfo doc5, CusSupportingInfo doc6,
			CusSupportingInfo doc7, CusSupportingInfo doc8, CusSupportingInfo doc9, CusSupportingInfo doc10, CusSupportingInfo doc11, CusSupportingInfo doc12, CusSupportingInfo doc13, CusSupportingInfo doc14, CusSupportingInfo doc15)
		{
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc1 has CSI_LineNo 0", 0, doc1.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc2 has CSI_LineNo 0", 0, doc2.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc3 has CSI_LineNo 3", 3, doc3.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc4 has CSI_LineNo 4", 4, doc4.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc5 has CSI_LineNo 5", 5, doc5.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc6 has CSI_LineNo 6", 6, doc6.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc7 has CSI_LineNo 7", 7, doc7.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc8 has CSI_LineNo 8", 8, doc8.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc9 has CSI_LineNo 9", 9, doc9.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc10 has CSI_LineNo 10", 10, doc10.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc11 has CSI_LineNo 11", 11, doc11.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc12 has CSI_LineNo 12", 12, doc12.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc13 has CSI_LineNo 13", 13, doc13.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc14 has CSI_LineNo 14", 14, doc14.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc15 has CSI_LineNo 15", 15, doc15.CSI_LineNo);
		}

		void AssertDocumentsLineNoWhenTransitionPeriodAndCustomsStatusPREOrForcedAndBillParent(CusSupportingInfo doc1, CusSupportingInfo doc2, CusSupportingInfo doc3, CusSupportingInfo doc4, CusSupportingInfo doc5, CusSupportingInfo doc6,
			CusSupportingInfo doc7, CusSupportingInfo doc8, CusSupportingInfo doc9, CusSupportingInfo doc10, CusSupportingInfo doc11, CusSupportingInfo doc12, CusSupportingInfo doc13, CusSupportingInfo doc14, CusSupportingInfo doc15)
		{
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc1 has CSI_LineNo 0", 0, doc1.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc2 has CSI_LineNo 0", 0, doc2.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc3 has CSI_LineNo 3", 3, doc3.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc4 has CSI_LineNo 4", 4, doc4.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc5 has CSI_LineNo 0", 0, doc5.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc6 has CSI_LineNo 6", 6, doc6.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc7 has CSI_LineNo 0", 0, doc7.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc8 has CSI_LineNo 0", 0, doc8.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc9 has CSI_LineNo 9", 9, doc9.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc10 has CSI_LineNo 10", 10, doc10.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc11 has CSI_LineNo 11", 11, doc11.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc12 has CSI_LineNo 12", 12, doc12.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc13 has CSI_LineNo 13", 13, doc13.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc14 has CSI_LineNo 14", 14, doc14.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc15 has CSI_LineNo 15", 15, doc15.CSI_LineNo);
		}

		void AssertDocumentsLineNoWhenTransitionPeriodAndCustomsStatusPREOrForcedAndHeaderParent(CusSupportingInfo doc1, CusSupportingInfo doc2, CusSupportingInfo doc3, CusSupportingInfo doc4, CusSupportingInfo doc5, CusSupportingInfo doc6,
			CusSupportingInfo doc7, CusSupportingInfo doc8, CusSupportingInfo doc9, CusSupportingInfo doc10, CusSupportingInfo doc11, CusSupportingInfo doc12, CusSupportingInfo doc13, CusSupportingInfo doc14, CusSupportingInfo doc15)
		{
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc1 has CSI_LineNo 0", 0, doc1.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc2 has CSI_LineNo 0", 0, doc2.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc3 has CSI_LineNo 3", 3, doc3.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc4 has CSI_LineNo 4", 4, doc4.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc5 has CSI_LineNo 0", 0, doc5.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc6 has CSI_LineNo 0", 0, doc6.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc7 has CSI_LineNo 0", 0, doc7.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc8 has CSI_LineNo 0", 0, doc8.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc9 has CSI_LineNo 9", 9, doc9.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc10 has CSI_LineNo 10", 10, doc10.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc11 has CSI_LineNo 0", 0, doc11.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc12 has CSI_LineNo 0", 0, doc12.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc13 has CSI_LineNo 0", 0, doc13.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc14 has CSI_LineNo 14", 14, doc14.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc15 has CSI_LineNo 15", 15, doc15.CSI_LineNo);
		}

		void AssertDocumentsLineNoWhenNoTransitionPeriod(CusSupportingInfo doc1, CusSupportingInfo doc2, CusSupportingInfo doc3, CusSupportingInfo doc4, CusSupportingInfo doc5, CusSupportingInfo doc6, CusSupportingInfo doc7,
			CusSupportingInfo doc8, CusSupportingInfo doc9, CusSupportingInfo doc10, CusSupportingInfo doc11, CusSupportingInfo doc12, CusSupportingInfo doc13, CusSupportingInfo doc14, CusSupportingInfo doc15)
		{
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc1 has CSI_LineNo 1", 1, doc1.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc2 has CSI_LineNo 2", 2, doc2.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc3 has CSI_LineNo 3", 3, doc3.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc4 has CSI_LineNo 4", 4, doc4.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc5 has CSI_LineNo 5", 5, doc5.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc6 has CSI_LineNo 6", 6, doc6.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc7 has CSI_LineNo 7", 7, doc7.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc8 has CSI_LineNo 8", 8, doc8.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc9 has CSI_LineNo 9", 9, doc9.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc10 has CSI_LineNo 10", 10, doc10.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc11 has CSI_LineNo 11", 11, doc11.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc12 has CSI_LineNo 12", 12, doc12.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc13 has CSI_LineNo 13", 13, doc13.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc14 has CSI_LineNo 14", 14, doc14.CSI_LineNo);
			AssertEquals("When declaration is Departure Phase5 but Customs Status is not Empty: doc15 has CSI_LineNo 15", 15, doc15.CSI_LineNo);
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			movementHeader = nctsHeader.MovementHeader;
			nctsBill1 = nctsHeader.Bills.AddNew();
			goodsItem11 = nctsBill1.GoodsItems.AddNew();
			goodsItem12 = nctsBill1.GoodsItems.AddNew();

			nctsBill2 = nctsHeader.Bills.AddNew();
			goodsItem21 = nctsBill2.GoodsItems.AddNew();
		}

		NctsDepartureCargoDesc goodsItem21;
		NctsDepartureCargoDesc goodsItem12;
		NctsDepartureCargoDesc goodsItem11;
		NctsBill nctsBill2;
		NctsBill nctsBill1;
		NctsHeader nctsHeader;
		NctsDepartureMovementHeader movementHeader;
	}
}
