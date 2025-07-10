using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentScanning.DataTransfer.Universal;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.DataTransfer.Test.Universal.DocumentRequest.DocumentFilters
{
	internal class DocumentFilterFactoryTest : TestCase
	{
		#region TestCreateDocumentFilter

		public void TestCreateDocumentFilterDateUTCFrom()
		{
			AssertCreateDocumentFilterDate(DocumentFilterType.SaveDateUTCFrom, false, true);
		}

		public void TestCreateDocumentFilterDateUTCTo()
		{
			AssertCreateDocumentFilterDate(DocumentFilterType.SaveDateUTCTo, true, false);
		}

		public void AssertCreateDocumentFilterDate(DocumentFilterType filterType, bool expectedMatchEarlierDate, bool expectedMatchLaterDate)
		{
			var eDoc1 = new eDocForTest { LastEdited = new ZDateTime(2016, 7, 13, 9, 30, 0) };
			var eDoc2 = new eDocForTest { LastEdited = new ZDateTime(2016, 7, 14, 9, 30, 0) };
			var eDoc3 = new eDocForTest { LastEdited = new ZDateTime(2016, 7, 15, 9, 30, 0) };

			var filter = DocumentFilterFactory.CreateDocumentFilter(filterType);
			AssertEquals(typeof(DocumentFilterDateTime), filter.GetType());

			filter.AddFilterValue("2016-07-14T09:30");
			AssertEquals(expectedMatchEarlierDate, filter.IsMatch(eDoc1));
			AssertEquals(true, filter.IsMatch(eDoc2));
			AssertEquals(expectedMatchLaterDate, filter.IsMatch(eDoc3));
		}

		public void TestCreateDocumentFilterDocumentType()
		{
			AssertCreateDocumentFilterCode(DocumentFilterType.DocumentType, (eDoc, value) => eDoc.DocType = value);
		}

		public void TestCreateDocumentFilterCompanyCode()
		{
			AssertCreateDocumentFilterCode(DocumentFilterType.CompanyCode, (eDoc, value) => eDoc.VisibleCompanyCode = value);
		}

		public void TestCreateDocumentFilterBranchCode()
		{
			AssertCreateDocumentFilterCode(DocumentFilterType.BranchCode, (eDoc, value) => eDoc.VisibleBranchCode = value);
		}

		public void TestCreateDocumentFilterDepartmentCode()
		{
			AssertCreateDocumentFilterCode(DocumentFilterType.DepartmentCode, (eDoc, value) => eDoc.VisibleDepartmentCode = value);
		}

		void AssertCreateDocumentFilterCode(DocumentFilterType filterType, Action<eDocForTest, ZString> setEDocProperty)
		{
			var eDoc1 = new eDocForTest();
			setEDocProperty(eDoc1, "AAA");
			var eDoc2 = new eDocForTest();
			setEDocProperty(eDoc2, "BBB");

			var filter = DocumentFilterFactory.CreateDocumentFilter(filterType);
			AssertEquals(typeof(DocumentFilterCode), filter.GetType());

			filter.AddFilterValue("AAA");
			Assert(filter.IsMatch(eDoc1));
			Assert(!filter.IsMatch(eDoc2));
		}

		public void TestCreateDocumentFilterIsPublished()
		{
			var eDoc1 = new eDocForTest { IsPublished = true };
			var eDoc2 = new eDocForTest { IsPublished = false };

			var filter = DocumentFilterFactory.CreateDocumentFilter(DocumentFilterType.IsPublished);
			AssertEquals(typeof(DocumentFilterBool), filter.GetType());

			filter.AddFilterValue("False");
			Assert(!filter.IsMatch(eDoc1));
			Assert(filter.IsMatch(eDoc2));
		}

		#endregion TestCreateDocumentFilter

		public void TestGetDocumentFilters()
		{
			var filterDataObjects = new List<DocumentFilter>
			{
				new DocumentFilter { Type = DocumentFilterType.IsPublished, Value = "True" },
				new DocumentFilter { Type = DocumentFilterType.SaveDateUTCFrom, Value = "2016-07-01T00:00" },
				new DocumentFilter { Type = DocumentFilterType.SaveDateUTCTo, Value = "2016-07-01T23:59" },
				new DocumentFilter { Type = DocumentFilterType.DocumentType, Value = "AAA" },
				new DocumentFilter { Type = DocumentFilterType.DocumentType, Value = "BBB" },
				new DocumentFilter { Type = DocumentFilterType.BranchCode, Value = "CCC" },
				new DocumentFilter { Type = DocumentFilterType.BranchCode, Value = "DDD" },
				new DocumentFilter { Type = DocumentFilterType.FileName, Value = "EEE" },
				new DocumentFilter { Type = DocumentFilterType.DocumentID, Value = ZGuid.NewZGuid().ToString() },
				new DocumentFilter { Type = DocumentFilterType.RelatedEDoc, Value = "Customs Entry s1000" }
			};

			var filters = DocumentFilterFactory.GetDocumentFilters(filterDataObjects).ToList<DocumentFilterBase>();

			AssertEquals(8, filters.Count);

			AssertEquals(typeof(DocumentFilterBool), filters[0].GetType());
			AssertEquals(true, ((DocumentFilterBool)filters[0]).Value);

			AssertEquals(typeof(DocumentFilterDateTime), filters[1].GetType());
			AssertEquals(new ZDateTime(2016, 7, 1), ((DocumentFilterDateTime)filters[1]).Value);

			AssertEquals(typeof(DocumentFilterDateTime), filters[2].GetType());
			AssertEquals(new ZDateTime(2016, 7, 1, 23, 59, 00), ((DocumentFilterDateTime)filters[2]).Value);

			AssertEquals(typeof(DocumentFilterCode), filters[3].GetType());
			AssertEquals(2, ((DocumentFilterCode)filters[3]).Values.Count);
			AssertEquals("AAA", ((DocumentFilterCode)filters[3]).Values[0]);
			AssertEquals("BBB", ((DocumentFilterCode)filters[3]).Values[1]);

			AssertEquals(typeof(DocumentFilterCode), filters[4].GetType());
			AssertEquals(2, ((DocumentFilterCode)filters[4]).Values.Count);
			AssertEquals("CCC", ((DocumentFilterCode)filters[4]).Values[0]);
			AssertEquals("DDD", ((DocumentFilterCode)filters[4]).Values[1]);

			AssertEquals(typeof(DocumentFilterCode), filters[5].GetType());
			AssertEquals(typeof(DocumentFilterGuid), filters[6].GetType());

			AssertEquals(typeof(DocumentFilterRelatedEDoc), filters[7].GetType());
			AssertEquals(1, ((DocumentFilterRelatedEDoc)filters[7]).Values.Count);
		}
	}
}
