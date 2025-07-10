using CargoWise.NetworkVisualisation.Integration;
using Moq;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.Business.Test
{
	public class UniqueRelationshipComparerTest : TestCase
	{
		public void TestGetHashCode_FromToIsNull()
		{
			var comparer = new UniqueRelationshipComparer();
			var x = new Entity();
			var y = new Entity();

			var r1 = new Relationship()
			{
				From = x,
				To = y
			};
			var r2 = new Relationship();
			//From and To remain null

			AssertNotEquals("HashCode for populated relationship shouldn't be 0", 0, comparer.GetHashCode(r1));

			AssertNoExceptionThrown("GetHashCode should handle empty relationships", () => comparer.GetHashCode(r2));
			AssertEquals("HashCode for empty relationship should be 0", 0, comparer.GetHashCode(r2));
		}

		public void TestGetHashCode()
		{
			var comparer = new UniqueRelationshipComparer();
			var x = new Entity();
			var y = new Entity();

			var r1 = new Relationship()
			{
				From = x,
				To = y
			};
			var r2 = new Relationship()
			{
				From = x,
				To = y
			};
			Assert("Relationships should be equal", comparer.Equals(r1, r2));
			AssertEquals("Relationships should have equal hash codes", comparer.GetHashCode(r1), comparer.GetHashCode(r2));
		}

		public void TestGetHashCode_OppositeDirection()
		{
			var comparer = new UniqueRelationshipComparer();
			var x = new Entity();
			var y = new Entity();

			var r1 = new Relationship()
			{
				From = x,
				To = y
			};
			var r2 = new Relationship()
			{
				From = y,
				To = x
			};
			Assert("Relationships should not be equal", !comparer.Equals(r1, r2));
			AssertNotEquals("Relationships should not have equal hash codes", comparer.GetHashCode(r1), comparer.GetHashCode(r2));
		}

		public void TestGetHashCode_IntegerOverflow()
		{
			var comparer = new UniqueRelationshipComparer();
			var mock = new MockRepository(MockBehavior.Loose);
			var x = mock.Create<IProposedNetworkEntity>();
			var y = mock.Create<IProposedNetworkEntity>();
			var r = new Relationship()
			{
				From = x.Object,
				To = y.Object
			};

			x.Setup(m => m.GetHashCode()).Returns(int.MaxValue);
			y.Setup(m => m.GetHashCode()).Returns(int.MaxValue - 1);

			AssertNoExceptionThrown("GetHashCode should handle overflows", () => comparer.GetHashCode(r));
		}
	}
}
