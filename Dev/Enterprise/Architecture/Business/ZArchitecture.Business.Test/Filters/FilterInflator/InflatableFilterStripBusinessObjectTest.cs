using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing;

sealed class InflatableFilterStripBusinessObjectTest : TestCase
{
	public void TestCorrectInheritance() => Assert(typeof(FilterStripBusinessObject).IsAssignableFrom(typeof(InflatableFilterStripBusinessObject)));

	public void TestGetModuleFiltersReturnsExpectedFilters()
	{
		var mockInflatableFilterStripBusinessObject = new Mock<InflatableFilterStripBusinessObject>();
		mockInflatableFilterStripBusinessObject.Protected()
			.Setup<List<IFilterInflator>>("GetFilterInflators")
			.Returns(CreateMockedFilterInflators("Consignee", "Presenter", "Tariff"));

		var filterCollection = mockInflatableFilterStripBusinessObject.Object.GetModuleFilters();
		AssertEquals(3, filterCollection.Count());

		var filterDesc = new HashSet<string>(filterCollection.Select(f => f.Description.ToString()));
		Assert(filterDesc.Contains("Tariff"));
		Assert(filterDesc.Contains("Consignee"));
		Assert(filterDesc.Contains("Presenter"));
	}

	public void TestGetModuleFiltersWithFewNullInflators()
	{
		var filterInflators = CreateMockedFilterInflators("Consignee", "Presenter", "Tariff");
		filterInflators.AddRange([null, null]);

		var mockInflatableFilterStripBusinessObject = new Mock<InflatableFilterStripBusinessObject>();
		mockInflatableFilterStripBusinessObject.Protected()
			.Setup<List<IFilterInflator>>("GetFilterInflators")
			.Returns(filterInflators);

		var filterCollection = mockInflatableFilterStripBusinessObject.Object.GetModuleFilters();
		AssertEquals("Null filter inflators should not contribute to filter collection",3, filterCollection.Count());

		var filterDesc = new HashSet<string>(filterCollection.Select(f => f.Description.ToString()));
		Assert(filterDesc.Contains("Tariff"));
		Assert(filterDesc.Contains("Consignee"));
		Assert(filterDesc.Contains("Presenter"));
	}

	public void TestGetModuleFiltersWithOnlyNullInflators()
	{
		var mockInflatableFilterStripBusinessObject = new Mock<InflatableFilterStripBusinessObject>();
		mockInflatableFilterStripBusinessObject.Protected()
			.Setup<List<IFilterInflator>>("GetFilterInflators")
			.Returns([null, null, null]);

		var filterCollection = mockInflatableFilterStripBusinessObject.Object.GetModuleFilters();
		AssertEquals("Filter list containing only null inflators should result in empty module filters", 0, filterCollection.Count());
	}

	public void TestGetModuleFiltersWithDuplicateDescriptions()
	{
		var mockInflatableFilterStripBusinessObject = new Mock<InflatableFilterStripBusinessObject>();
		mockInflatableFilterStripBusinessObject.Protected()
			.Setup<List<IFilterInflator>>("GetFilterInflators")
			.Returns(CreateMockedFilterInflators("Country", "Country", "Country"));

		AssertExceptionThrown<ArgumentException>(
			"Duplicate filter description must throw an exception",
			"A filter already exists with the description 'Country'; filter descriptions must be unique.",
			() => mockInflatableFilterStripBusinessObject.Object.GetModuleFilters());
	}

	public void TestModuleFiltersWithEmptyFilterList()
	{
		var mockInflatableFilterStripBusinessObject = new Mock<InflatableFilterStripBusinessObject>();
		mockInflatableFilterStripBusinessObject.Protected()
			.Setup<List<IFilterInflator>>("GetFilterInflators")
			.Returns([]);

		var filterCollection = mockInflatableFilterStripBusinessObject.Object.GetModuleFilters();
		AssertEquals("Empty filter list should result in empty module filters", 0, filterCollection.Count());
	}

	public void TestModuleFiltersWithNullFilterList()
	{
		var mockInflatableFilterStripBusinessObject = new Mock<InflatableFilterStripBusinessObject>();
		mockInflatableFilterStripBusinessObject.Protected()
			.Setup<List<IFilterInflator>>("GetFilterInflators")
			.Returns((List<IFilterInflator>)null);

		var filterCollection = mockInflatableFilterStripBusinessObject.Object.GetModuleFilters();
		AssertEquals("Null filter list should result in empty module filters", 0, filterCollection.Count());
	}

	List<IFilterInflator> CreateMockedFilterInflators(params string[] descriptions)
		=> descriptions.Select(desc => CreateMockFilterInflator(desc).Object).ToList();

	Mock<IFilterInflator> CreateMockFilterInflator(string desc)
	{
		var mockFilterInflator = new Mock<IFilterInflator>();
		mockFilterInflator
			.Setup(i => i.InflateFilter(It.IsAny<ModuleFilterCollection>()))
			.Callback<ModuleFilterCollection>(c => c.AddTextFilter(desc, DummyBizoSchema.Z0_Description));
		return mockFilterInflator;
	}
}
