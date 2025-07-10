using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class DepartureMeansOfTransportCollectionComparerTest : TestCase
{
	public void TestEquals_BothNull()
	{
		IReadOnlyCollection<DepartureMeansOfTransportWrapper> x = null;
		IReadOnlyCollection<DepartureMeansOfTransportWrapper> y = null;

		AssertEquals(true, comparer.Equals(x, y));
	}

	public void TestEquals_OneNul()
	{
		IReadOnlyCollection<DepartureMeansOfTransportWrapper> x = null;
		IReadOnlyCollection<DepartureMeansOfTransportWrapper> y = new List<DepartureMeansOfTransportWrapper>();

		AssertEquals(false, comparer.Equals(x, y));
	}

	public void TestEquals_DifferentCounts()
	{
		var x = new List<DepartureMeansOfTransportWrapper>
		{
			DepartureMeansOfTransportWrapper.NewOrNull("1", "ID1", "IT")
		};

		var y = new List<DepartureMeansOfTransportWrapper>();

		AssertEquals(false, comparer.Equals(x, y));
	}

	public void TestEquals_SameElements()
	{
		var x = new List<DepartureMeansOfTransportWrapper>
		{
			DepartureMeansOfTransportWrapper.NewOrNull("1", "ID1", "IT"),
			DepartureMeansOfTransportWrapper.NewOrNull("2", "ID2", "DE")
		};

		var y = new List<DepartureMeansOfTransportWrapper>
		{
			DepartureMeansOfTransportWrapper.NewOrNull("1", "ID1", "IT"),
			DepartureMeansOfTransportWrapper.NewOrNull("2", "ID2", "DE")
		};

		AssertEquals(true, comparer.Equals(x, y));
	}

	public void TestEquals_DifferentElements()
	{
		var x = new List<DepartureMeansOfTransportWrapper>
		{
			DepartureMeansOfTransportWrapper.NewOrNull("1", "ID1", "IT")
		};

		var y = new List<DepartureMeansOfTransportWrapper>
		{
			DepartureMeansOfTransportWrapper.NewOrNull("2", "ID2", "DE")
		};

		AssertEquals(false, comparer.Equals(x, y));
	}

	public void TestGetHashCode_SameElements()
	{
		var x = new List<DepartureMeansOfTransportWrapper>
		{
			DepartureMeansOfTransportWrapper.NewOrNull("1", "ID1", "IT"),
			DepartureMeansOfTransportWrapper.NewOrNull("2", "ID2", "DE")
		};

		var y = new List<DepartureMeansOfTransportWrapper>
		{
			DepartureMeansOfTransportWrapper.NewOrNull("1", "ID1", "IT"),
			DepartureMeansOfTransportWrapper.NewOrNull("2", "ID2", "DE")
		};

		AssertEquals(comparer.GetHashCode(x), comparer.GetHashCode(y));
	}

	public void TestGetHashCode_DifferentElements()
	{
		var x = new List<DepartureMeansOfTransportWrapper>
		{
			DepartureMeansOfTransportWrapper.NewOrNull("1", "ID1", "IT")
		};

		var y = new List<DepartureMeansOfTransportWrapper>
		{
			DepartureMeansOfTransportWrapper.NewOrNull("2", "ID2", "CA")
		};

		AssertNotEquals(comparer.GetHashCode(x), comparer.GetHashCode(y));
	}

	protected override void SetUp()
	{
		base.SetUp();
		comparer = new DepartureMeansOfTransportCollectionComparer();
	}

	DepartureMeansOfTransportCollectionComparer comparer;
}
