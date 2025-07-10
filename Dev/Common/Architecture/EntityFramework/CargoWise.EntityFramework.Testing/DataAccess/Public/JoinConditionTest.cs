using System.Linq;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class JoinConditionTest : TestCase
	{
		public void TestDeepClone()
		{
			JoinCondition andCondition = JoinCondition.And;
			IFilterPart clonedAnd = andCondition.DeepClone();
			Assert(!object.ReferenceEquals(clonedAnd, andCondition));
			AssertEquals(((JoinCondition)clonedAnd).Text, andCondition.Text);
		}

		public void TestGetSimplifiedVersion()
		{
			AssertEquals(JoinCondition.And, ((IFilterPart)JoinCondition.And).GetSimplifiedVersion(null)[0]);
		}

		public void TestBlobFilters()
		{
			IFilterPart filterPart = JoinCondition.And;
			AssertEquals(0, filterPart.BlobFilters.Count());
		}

		public void TestEquals()
		{
			AssertEquals(true, JoinCondition.And.Equals(JoinCondition.And));
			AssertEquals(false, JoinCondition.And.Equals(JoinCondition.Or));
			AssertEquals(false, JoinCondition.And.Equals(null));
			AssertEquals(false, JoinCondition.And.Equals(3));
		}

		public void TestEqualityOperator()
		{
#pragma warning disable 1718
			AssertEquals(true, JoinCondition.And == JoinCondition.And);
			AssertEquals(true, JoinCondition.Or == JoinCondition.Or);
			AssertEquals(false, JoinCondition.Or == JoinCondition.And);
#pragma warning restore 1718
		}

		public void TestInEqualityOperator()
		{
#pragma warning disable 1718
			AssertEquals(false, JoinCondition.And != JoinCondition.And);
			AssertEquals(false, JoinCondition.Or != JoinCondition.Or);
			AssertEquals(true, JoinCondition.Or != JoinCondition.And);
#pragma warning restore 1718
		}

		public void TestHasParameters()
		{
			AssertEquals(false, JoinCondition.And.HasParameters);
		}

		#region HasComparisonOperator

		public void TestHasComparisonOperator()
		{
			var joinCondition = JoinCondition.And;
			AssertEquals(joinCondition.HasComparisonOperatorLike, false);
		}

		#endregion
	}
}
