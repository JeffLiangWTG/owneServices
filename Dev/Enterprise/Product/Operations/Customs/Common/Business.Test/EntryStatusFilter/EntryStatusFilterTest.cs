using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Common.Testing
{
	[TestedType(typeof(EntryStatusFilter))]
	sealed class EntryStatusFilterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestEntryStatusFilter_ThrowsArgumentException()
		{
			AssertExceptionThrown<NullReferenceException>(() => new EntryStatusFilter("Entry Status", null, () => new CodeDescriptionPairList()));
			AssertExceptionThrown<ArgumentException>(() => new EntryStatusFilter("Entry Status", delegate { return new ZQuery(); }, null));
		}

		[ExpectNoExceptions]
		public void TestDefaultValues()
		{
			NUnit.Framework.Assert.That(Filter.Property, Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(Filter.FilterType, Is.EqualTo(EntryStatusFilterTypeList.Codes.Any).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestClear()
		{
			Filter.Property = "RL1";
			Filter.FilterType = EntryStatusFilterTypeList.Codes.All;

			Filter.Clear();

			NUnit.Framework.Assert.That(Filter.Property, Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(Filter.FilterType, Is.EqualTo(EntryStatusFilterTypeList.Codes.Any).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestIsEmpty()
		{
			Filter.Property = ZString.Empty;
			Filter.FilterType = ZString.Empty;

			NUnit.Framework.Assert.That(Filter.IsEmpty, Is.EqualTo(true));

			Filter.Property = "RL1";

			NUnit.Framework.Assert.That(Filter.IsEmpty, Is.EqualTo(false));

			Filter.Property = ZString.Empty;
			Filter.FilterType = EntryStatusFilterTypeList.Codes.Any;

			NUnit.Framework.Assert.That(Filter.IsEmpty, Is.EqualTo(true));

			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;

			NUnit.Framework.Assert.That(Filter.IsEmpty, Is.EqualTo(false));

			Filter.Property = ZString.Empty;
			Filter.FilterType = EntryStatusFilterTypeList.Codes.All;

			NUnit.Framework.Assert.That(Filter.IsEmpty, Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestComparisonOperators()
		{
			Filter.FilterType = EntryStatusFilterTypeList.Codes.All;

			NUnit.Framework.Assert.That(Filter.ComparisonOperator_List.GetAllCodes(), Is.EquivalentTo(new[]
				{
					ModuleTextFilter.ComparisonConstants.Exact,
				}));

			Filter.FilterType = EntryStatusFilterTypeList.Codes.Any;

			NUnit.Framework.Assert.That(Filter.ComparisonOperator_List.GetAllCodes(), Is.EquivalentTo(new[]
				{
					ModuleTextFilter.ComparisonConstants.Exact,
					ModuleTextFilter.ComparisonConstants.NotEqual,
					ModuleTextFilter.ComparisonConstants.IsBlank,
					ModuleTextFilter.ComparisonConstants.IsNotBlank
				}));
		}

		[ExpectNoExceptions]
		public void TestSerialization()
		{
			Filter.FilterType = EntryStatusFilterTypeList.Codes.All;
			Filter.Property = "RL1";
			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;

			using (var writer = new StringWriter())
			using (var xmlWriter = new XmlTextWriter(writer))
			{
				xmlWriter.Formatting = Formatting.Indented;

				xmlWriter.WriteStartElement("Filter");
				((IXmlSerializable)Filter).WriteXml(xmlWriter);
				xmlWriter.WriteEndElement();
				xmlWriter.Flush();

				NUnit.Framework.Assert.That(writer.ToString(), CustomConstraints.MultilineASCIIEquals(SampleXML));
			}
		}

		[ExpectNoExceptions]
		public void TestDeserialization()
		{
			using (StringReader reader = new StringReader(SampleXML))
			using (XmlTextReader xmlReader = new XmlTextReader(reader))
			{
				xmlReader.WhitespaceHandling = WhitespaceHandling.None;
				xmlReader.MoveToContent();
				xmlReader.ReadStartElement("Filter");
				((IXmlSerializable)Filter).ReadXml(xmlReader);
				xmlReader.ReadEndElement();
			}

			CombineAssertions(delegate
			{
				NUnit.Framework.Assert.That(Filter.FilterType, Is.EqualTo("All").Using(CustomComparers.TypeComparison), "FilterType");
				NUnit.Framework.Assert.That(Filter.Property, Is.EqualTo("RL1").Using(CustomComparers.TypeComparison), "Property");
				NUnit.Framework.Assert.That(Filter.ComparisonOperator, Is.EqualTo(ModuleTextFilter.ComparisonConstants.Exact).Using(CustomComparers.TypeComparison), "ComparisonOperator");
			});
		}

		[ExpectNoExceptions]
		public void TestQueryDelegateParameters_PropertyOnly()
		{
			var testVal = ZString.Empty;

			var filter = new EntryStatusFilter("Test Entry Status", (ZString str) => { testVal = str;
					return new ZQuery();
				},
				() => new CodeDescriptionPairList(), false, false);

			filter.Property = "RL1";

			Factory.Load<DummyBusinessObject>(filter.Query);

			NUnit.Framework.Assert.That(testVal, Is.EqualTo("RL1").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestQueryDelegateParameters_All()
		{
			var statusArg = ZString.Empty;
			var comparisonOperator = SQLComparisonOperator.NotSpecified;
			var filterType = ZString.Empty;

			var entryStatusFilter = new EntryStatusFilter("Test Entry Status", (SQLComparisonOperator @operator, ZString type, ZString status) => {
					comparisonOperator = @operator;
					statusArg = status;
					filterType = type;
					return new ZQuery();
				},
				() => new CodeDescriptionPairList(), true, true);

			entryStatusFilter.Property = "RL1";
			entryStatusFilter.FilterType = EntryStatusFilterTypeList.Codes.Any;
			entryStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;

			Factory.Load<DummyBusinessObject>(entryStatusFilter.Query);

			NUnit.Framework.Assert.That(statusArg, Is.EqualTo("RL1").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(comparisonOperator, Is.EqualTo(SQLComparisonOperator.NotEqual));
			NUnit.Framework.Assert.That(filterType, Is.EqualTo(EntryStatusFilterTypeList.Codes.Any).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestQueryDelegateParameters_ComparisonOperatorAndProperty()
		{
			var statusArg = ZString.Empty;
			var comparisonOperator = SQLComparisonOperator.NotSpecified;

			var entryStatusFilter = new EntryStatusFilter("Test Entry Status", (SQLComparisonOperator @operator, ZString status) => {
					comparisonOperator = @operator;
					statusArg = status;
					return new ZQuery();
				},
				() => new CodeDescriptionPairList(), true, false);

			entryStatusFilter.Property = "RL1";
			entryStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;

			Factory.Load<DummyBusinessObject>(entryStatusFilter.Query);

			NUnit.Framework.Assert.That(statusArg, Is.EqualTo("RL1").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(comparisonOperator, Is.EqualTo(SQLComparisonOperator.NotEqual));
		}

		[ExpectNoExceptions]
		public void TestShowFilterElementProperties()
		{
			NUnit.Framework.Assert.That(Filter.ShowComparisonOperator, Is.EqualTo(true));
			NUnit.Framework.Assert.That(Filter.ShowFilterType, Is.EqualTo(true));

			Filter.ShowFilterType = false;

			NUnit.Framework.Assert.That(Filter.ShowFilterType, Is.EqualTo(false));

			var entryStatusFilter = new EntryStatusFilter("Test Entry Status", (SQLComparisonOperator @operator, ZString type, ZString status) => new ZQuery(),
				() => new CodeDescriptionPairList(), false, false);

			NUnit.Framework.Assert.That(entryStatusFilter.ShowComparisonOperator, Is.EqualTo(false));
			NUnit.Framework.Assert.That(entryStatusFilter.ShowFilterType, Is.EqualTo(false));
		}

		EntryStatusFilter Filter => filter ??= (EntryStatusFilter)GetNewBusinessObject();
		EntryStatusFilter filter;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new EntryStatusFilter("Entry Status",
				delegate { return new ZQuery(); },
				() => new CodeDescriptionPairList());
		}

		const string SampleXML =
			@"<Filter>
  <Comparer>exact</Comparer>
  <Property>RL1</Property>
  <FilterType>All</FilterType>
</Filter>";
	}
}

