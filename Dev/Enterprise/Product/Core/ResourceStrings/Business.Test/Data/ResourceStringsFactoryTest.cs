using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.Business.Testing
{
	sealed class ResourceStringsFactoryTest : TestCase
	{
		public void TestStringOperators()
		{
			var mockData = ResourceStringsFactory.GetMockSource(Res.DefaultLanguage);
			mockData.Put("abc", new ResourceStringData("abc", string.Empty, string.Empty, "abc", string.Empty));
			mockData.Put("bcd", new ResourceStringData("bcd", string.Empty, string.Empty, "bcd", string.Empty));
			mockData.Put("def", new ResourceStringData("def", string.Empty, string.Empty, "def", string.Empty));
			AssertQueryResult(new ZQuery(HelpDataStringSchema.HD_Caption, SQLComparisonOperator.Contains, "BC"), ResourceStringsFactory.Lookup(Res.DefaultLanguage, "abc"), ResourceStringsFactory.Lookup(Res.DefaultLanguage, "bcd"));
			AssertQueryResult(new ZQuery(HelpDataStringSchema.HD_Caption, SQLComparisonOperator.DoesNotStartWith, "AB"), ResourceStringsFactory.Lookup(Res.DefaultLanguage, "bcd"), ResourceStringsFactory.Lookup(Res.DefaultLanguage, "def"));
			AssertQueryResult(new ZQuery(HelpDataStringSchema.HD_Caption, SQLComparisonOperator.EndsWith, "CD"), ResourceStringsFactory.Lookup(Res.DefaultLanguage, "bcd"));
			AssertQueryResult(new ZQuery(HelpDataStringSchema.HD_Caption, SQLComparisonOperator.Equal, "ABC"), ResourceStringsFactory.Lookup(Res.DefaultLanguage, "abc"));
			AssertQueryResult(new ZQuery(HelpDataStringSchema.HD_Caption, SQLComparisonOperator.NotContains, "BC"), ResourceStringsFactory.Lookup(Res.DefaultLanguage, "def"));
			AssertQueryResult(new ZQuery(HelpDataStringSchema.HD_Caption, SQLComparisonOperator.NotEqual, "ABC"), ResourceStringsFactory.Lookup(Res.DefaultLanguage, "bcd"), ResourceStringsFactory.Lookup(Res.DefaultLanguage, "def"));
			AssertQueryResult(new ZQuery(HelpDataStringSchema.HD_Caption, SQLComparisonOperator.StartsWith, "AB"), ResourceStringsFactory.Lookup(Res.DefaultLanguage, "abc"));
		}

		public void TestStringOperators_ZSQLInFilter()
		{
			var mockData = ResourceStringsFactory.GetMockSource(Res.DefaultLanguage);
			mockData.Put("nono", new ResourceStringData("nono", string.Empty, string.Empty, "no", "no"));
			mockData.Put("yesno", new ResourceStringData("yesno", string.Empty, string.Empty, "yes", "no"));
			mockData.Put("noyes", new ResourceStringData("noyes", string.Empty, string.Empty, "no", "yes"));
			mockData.Put("yesyes", new ResourceStringData("yesyes", string.Empty, string.Empty, "yes", "yes"));
			mockData.Put("aaabbb", new ResourceStringData("aaabbb", string.Empty, string.Empty, "aaa", "bbb"));

			var query = new ZQuery(HelpDataStringSchema.HD_Caption, "yes");
			var fullDescriptionQuery = new ZQuery(HelpDataStringSchema.HD_FullDescription, new ZString[] { "yes", "no" });
			query.AddToFilter(fullDescriptionQuery);
			AssertQueryResult(query, ResourceStringsFactory.Lookup(Res.DefaultLanguage, "yesno"), ResourceStringsFactory.Lookup(Res.DefaultLanguage, "yesyes"));
		}

		public void TestJoinOperators()
		{
			var mockData = ResourceStringsFactory.GetMockSource(Res.DefaultLanguage);
			mockData.Put("nono", new ResourceStringData("nono", string.Empty, string.Empty, "no", "no"));
			mockData.Put("yesno", new ResourceStringData("yesno", string.Empty, string.Empty, "yes", "no"));
			mockData.Put("noyes", new ResourceStringData("noyes", string.Empty, string.Empty, "no", "yes"));
			mockData.Put("yesyes", new ResourceStringData("yesyes", string.Empty, string.Empty, "yes", "yes"));
			ZQuery q = new ZQuery(HelpDataStringSchema.HD_Caption, "yes");
			q.AddToFilter(HelpDataStringSchema.HD_FullDescription, "yes");
			AssertQueryResult(q, ResourceStringsFactory.Lookup(Res.DefaultLanguage, "yesyes"));
			q = new ZQuery(HelpDataStringSchema.HD_Caption, "yes");
			q.AddToFilter(JoinCondition.Or, HelpDataStringSchema.HD_FullDescription, "yes");
			AssertQueryResult(q, ResourceStringsFactory.Lookup(Res.DefaultLanguage, "yesyes"), ResourceStringsFactory.Lookup(Res.DefaultLanguage, "yesno"), ResourceStringsFactory.Lookup(Res.DefaultLanguage, "noyes"));
		}

		public void TestCheckout()
		{
			var eNG = ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English);
			eNG.Put("code", new ResourceStringData("code", "caption"));

			var item = new HelpDataString();
			item.HD_Language = Core.SharedConstants.Languages.French;
			item.HD_Code = "code";
			item.HD_Caption = "sous-titre";
			ResourceStringsFactory.Save("TST", item);
			AssertQueryResult(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true), item);
			ResourceStringsFactory.UndoCheckout(item);
			AssertQueryResult(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
		}

		public void TestSaveDoesNotOverwriteExternalChanges()
		{
			var eNG = ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English);
			eNG.Put("one", new ResourceStringData("one", "one"));
			eNG.Put("two", new ResourceStringData("two", "two"));
			string deltaFile = ((ResourcesDeltaSource)((SimpleResourceStringCache)((MultiLevelResourceStringCache)ResourceStringsFactory.GetResourceStringCache(Core.SharedConstants.Languages.French)).Caches[0]).Source).FilePath;
			var item1 = new HelpDataString();
			item1.HD_Language = Core.SharedConstants.Languages.French;
			item1.HD_Code = "one";
			item1.HD_Caption = "un";
			ResourceStringsFactory.Save("TST", item1);
			var fileContents = File.ReadAllBytes(deltaFile);
			ResourceStringsFactory.UndoCheckout(item1);
			AssertQueryResult(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
			File.WriteAllBytes(deltaFile, fileContents);
			var item2 = new HelpDataString();
			item2.HD_Language = Core.SharedConstants.Languages.French;
			item2.HD_Code = "two";
			item2.HD_Caption = "deux";
			ResourceStringsFactory.Save("TST", item2);
			AssertQueryResult(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true), item1, item2);
		}

		void AssertQueryResult(ZQuery query, params HelpDataString[] expected)
		{
			AssertContainsExactElementsInAnyOrder<HelpDataString>(new HelpDataStringComparer(), expected, ResourceStringsFactory.Load(query));
		}

		class HelpDataStringComparer : IEqualityComparer<HelpDataString>
		{
			public bool Equals(HelpDataString x, HelpDataString y)
			{
				return
					x.HD_Code == y.HD_Code &&
					x.HD_Caption == y.HD_Caption &&
					x.HD_FullDescription == y.HD_FullDescription &&
					x.HD_ShortCaption == y.HD_ShortCaption &&
					x.HD_MidCaption == y.HD_MidCaption;
			}

			public int GetHashCode(HelpDataString obj)
			{
				return obj.HD_Code.GetHashCode() ^ obj.HD_Caption.GetHashCode() ^ obj.HD_FullDescription.GetHashCode();
			}
		}

		protected override void SetUp()
		{
			mockSources = ResourceStringsFactory.MockSources();
			base.SetUp();
		}

		protected override void TearDown()
		{
			if (mockSources != null)
			{
				mockSources.Dispose();
			}
			base.TearDown();
		}

		IDisposable mockSources;
	}
}
