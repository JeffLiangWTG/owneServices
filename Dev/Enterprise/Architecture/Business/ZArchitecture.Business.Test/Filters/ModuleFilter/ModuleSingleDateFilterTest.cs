using System.IO;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(ModuleSingleDateFilter))]
	sealed class ModuleSingleDateFilterTest : ModuleFilterTestCase<ModuleSingleDateFilter>
	{
		#region TestProperty1Validation

		public void TestProperty1Validation()
		{
			var errorText = "You cant hug your children with atomic arms!";
			Filter.Property1Validation = null;
			Filter.Property1 = ZDateTime.Empty;

			Filter.Validation.ValidateProperty1();
			AssertNoError(Filter.Property1Info, errorText);

			Filter.Property1Validation = delegate(ZPropertyInfo info)
			{
				if (info.Value.IsEmpty)
				{
					info.AddError(errorText);
				}
			};

			Filter.Validation.ValidateProperty1();
			AssertHasError(Filter.Property1Info, errorText);
		}

		#endregion

		#region TestIsExpensiveQuery

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		#endregion

		#region TestClearResetsToDefaults

		public void TestClearResetsToDefaults()
		{
			var dateValue1 = new ZDateTime(2006, 12, 25);

			Filter.Property1 = dateValue1;

			AssertEquals("Precondition", dateValue1, Filter.Property1);

			Filter.Clear();
			AssertEquals(ZDateTime.Empty, Filter.Property1);
		}

		#endregion

		#region TestIsEmpty

		public void TestIsEmpty()
		{
			Filter.Property1 = ZDateTime.Today;
			AssertEquals(false, Filter.IsEmpty);
			Filter.Property1 = ZDateTime.Empty;
			AssertEquals(true, Filter.IsEmpty);
		}

		#endregion

		#region Testing the Query results

		#region TestQueryWithSpecifiedDates

		[TestDate(2007, 1, 1)]
		public void TestQueryWithSpecifiedDates()
		{
			ZDateTime dateToFind = TestDateAttribute.Date;
			ZDateTime dateToNotFind = TestDateAttribute.Date.AddDays(1);

			Dummy1.Z0_Date = dateToFind;
			Dummy2.Z0_Date = dateToNotFind;
			Factory.Save();

			var dummies = new DummyBusinessObjectCollection(Factory);

			Filter.Property1 = dateToFind;
			dummies.Load(Filter.Query);
			AssertCollectionContains(Dummy1, dummies);
			AssertCollectionNotContains(Dummy2, dummies);
		}

		#endregion

		#endregion

		#region TestDeserializePropertiesFromXml

		public void TestDeserializePropertiesFromXml()
		{
			var filterStripBizO = new DummyFilterStripBusinessObject();
			var filter = new DummyModuleSingleDateFilter();

			var dateValue1 = new ZDateTime(2007, 08, 09);
			filter.Property1 = dateValue1;

			filterStripBizO.AddModuleFilterForTest(filter);

			var strip = filterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = filter.Description;

			var savedFilter = filterStripBizO.SaveLayout("savedFilter");

			strip.FilterDescription = "";
			strip.Delete();

			filterStripBizO.LoadLayout(savedFilter);

			var loadedFilter = (ModuleSingleDateFilter)filterStripBizO[filter.Description];

			AssertEquals(dateValue1, loadedFilter.Property1);
		}

		#endregion

		#region TestDeserializeInvalidGuidFromXml

		public void TestDeserializeInvalidDateFromXml()
		{
			var filter = new ModuleSingleDateFilter("Filter", DummyBizoSchema.Z0_Date);

			using (var stringReader = new StringReader("<Property1>somethingInvalid</Property1>"))
			using (var xmlReader = new XmlTextReader(stringReader))
			{
				xmlReader.Read();
				filter.DeserializeProperties(xmlReader);
				AssertEquals("Nothing should happen when deserializing invalid data", ZDateTime.Empty, filter.Property1);
			}

			using (var stringReader = new StringReader("<Property1>2008-01-01 10:00:00.000</Property1>"))
			using (var xmlReader = new XmlTextReader(stringReader))
			{
				xmlReader.Read();
				filter.DeserializeProperties(xmlReader);
				AssertEquals("Valid value should be deserialised correctly", new ZDateTime(2008, 01, 01, 10, 0, 0), filter.Property1);
			}
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			Dummy1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			Dummy2 = Factory.NewWithValidTestData<DummyBusinessObject>();
		}

		DummyBusinessObject Dummy1;
		DummyBusinessObject Dummy2;

		protected override ModuleSingleDateFilter GetNewModuleFilter()
		{
			return new ModuleSingleDateFilter("moo", DummyBizoSchema.Z0_Date);
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.Dates; }
		}

		#endregion

		#region DummyModuleDateFilter

		public class DummyModuleSingleDateFilter : ModuleSingleDateFilter
		{
			public DummyModuleSingleDateFilter()
				: base("DummyDateFilter", DummyBizoSchema.Z0_Date)
			{
			}

			protected override void SerializePropertiesToXml(XmlWriter writer)
			{
				string sqlDate1 = Property1.IsValid ? Property1.SqlFormat : ZString.Empty;
				writer.WriteElementString("Property1", sqlDate1);
			}
		}

		#endregion
	}
}
