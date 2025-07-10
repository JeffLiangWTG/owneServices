using System;
using System.Data;

namespace Enterprise.BusinessObjectGenerator.Testing
{
	sealed class AutoPropertyFKTest : AutoCodeTestCase
	{
		public void TestBizObjNameWithoutNK()
		{
			AssertCountryNkPropertyGeneration(AssertBizObjNameWithoutNK);
		}

		void AssertBizObjNameWithoutNK(BusinessObjectInfo bizObjInfo, string nkColumnName, string expected)
		{
			var propertyFk = NewTestAutoPropertyFk(bizObjInfo, nkColumnName, typeof(string));
			AssertEquals($"Expected BizObjNameWithoutNK when column name = {nkColumnName}", expected, propertyFk.BizObjNameWithoutNK_Exposed);
		}

		public void TestCodeForStringRelatedBusinessObject()
		{
			AssertCountryNkPropertyGeneration(AssertCodeForStringRelatedRefCountry);
		}

		public void TestCodeForStringRelatedBusinessObjectWithTargetColumnNotEndingWithCode()
		{
			AssertSetNameNkPropertyGeneration(AssertCodeForStringRelatedRefTimeZoneSet);
		}

		public void TestIsLookupShouldBeGenerated()
		{
			const string computedColumnName = "TT_IntField";
			const string randomColumnName = "TT_RandomField";

			var bizObjInfo = AutoPropertyTest.GetBizObjInfo(new DataTable("TestTable"), masterFileReference: false);

			var computedColumn = new DataColumn(computedColumnName, typeof(int));
			computedColumn.ExtendedProperties.Add("IsComputed", "Y");
			bizObjInfo.Table.Columns.Add(computedColumn);

			var column = new DataColumn(randomColumnName, typeof(int));
			bizObjInfo.Table.Columns.Add(column);

			var autoPropertyFk = new AutoPropertyFKForTesting(bizObjInfo, column, "int", isMasterFileFK: true);
			var autoPropertyFkComputed = new AutoPropertyFKForTesting(bizObjInfo, computedColumn, "int", isMasterFileFK: true);

			Assert(autoPropertyFk.IsLookupShouldBeGenerated);
			Assert(!autoPropertyFkComputed.IsLookupShouldBeGenerated);
		}

		void AssertCodeForStringRelatedRefCountry(BusinessObjectInfo bizObjInfo, string nkColumnName, string expectedPtyName)
		{
			var propertyFk = NewTestAutoPropertyFk(bizObjInfo, nkColumnName, typeof(string), "RefCountry");
			AssertEquals(
				$"{nkColumnName} expects propety {expectedPtyName}",
				$"\t\tpublic virtual RefCountry {expectedPtyName}\r\n\t\t{{\r\n\t\t\tget {{ return (RefCountry) Factory.LoadFromNaturalKey(typeof(RefCountry), RefCountrySchema.RN_Code, {nkColumnName}); }}\r\n\t\t}}",
				propertyFk.CodeForStringRelatedBusinessObject_Exposed);
		}

		void AssertCodeForStringRelatedRefTimeZoneSet(BusinessObjectInfo bizObjInfo, string nkColumnName, string expectedPtyName)
		{
			var propertyFk = NewTestAutoPropertyFk(bizObjInfo, nkColumnName, typeof(string), "RefTimeZoneSet");
			AssertEquals(
				$"{nkColumnName} expects propety {expectedPtyName}",
				$"\t\tpublic virtual RefTimeZoneSet {expectedPtyName}\r\n\t\t{{\r\n\t\t\tget {{ return (RefTimeZoneSet) Factory.LoadFromNaturalKey(typeof(RefTimeZoneSet), RefTimeZoneSetSchema.R3_TimeZoneSetName, {nkColumnName}); }}\r\n\t\t}}",
				propertyFk.CodeForStringRelatedBusinessObject_Exposed);
		}

		void AssertCountryNkPropertyGeneration(Action<BusinessObjectInfo, string, string> assertCountryProperty)
		{
			var bizObjInfo = AutoPropertyTest.GetBizObjInfo(new DataTable("TestTable"));

			CombineAssertions(() =>
			{
				assertCountryProperty(bizObjInfo, "TT_RN_NKCountryCode", "Country");
				assertCountryProperty(bizObjInfo, "GC_RN_NKCountryCode", "Country");
				assertCountryProperty(bizObjInfo, "OA_RN_NKCountryCode", "Country");
				assertCountryProperty(bizObjInfo, "GC_RN_NKAnotherCountryCode", "AnotherCountryCode");
				assertCountryProperty(bizObjInfo, "OA_RN_NKCountryReference", "CountryReference");
			});
		}

		void AssertSetNameNkPropertyGeneration(Action<BusinessObjectInfo, string, string> assertSetNameProperty)
		{
			var bizObjInfo = AutoPropertyTest.GetBizObjInfo(new DataTable("TestTable"));

			CombineAssertions(() =>
			{
				assertSetNameProperty(bizObjInfo, "GSZ_R3_NKTimeZoneSetName", "TimeZoneSetName");
			});
		}

		AutoPropertyFKForTesting NewTestAutoPropertyFk(BusinessObjectInfo bizObjInfo, string columnName, Type columnDataType, string propertyType = null)
		{
			var column = new DataColumn(columnName, columnDataType);
			return new AutoPropertyFKForTesting(bizObjInfo, column, propertyType);
		}

		class AutoPropertyFKForTesting : AutoPropertyFK
		{
			public AutoPropertyFKForTesting(BusinessObjectInfo info, DataColumn column, string propertyType, bool isMasterFileFK = false)
				: base(info, column, maxColumnLength: 128, propertyType, isMasterFileFK)
			{
			}

			public string BizObjNameWithoutNK_Exposed => BizObjNameWithoutNK;
			public string CodeForStringRelatedBusinessObject_Exposed => CodeForStringRelatedBusinessObject;
		}
	}
}
