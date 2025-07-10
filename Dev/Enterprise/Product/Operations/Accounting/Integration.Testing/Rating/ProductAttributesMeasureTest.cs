using NUnit.Framework;

namespace Enterprise.Accounting.Integration.Testing
{
	public class ProductAttributesMeasureTest : TestCase
	{
		public void TestProductAttributesMeasure()
		{
			ProductAttributesMeasure prodAttribs1 = new ProductAttributesMeasure("A1", "B1", "C1");
			ProductAttributesMeasure prodAttribs2 = new ProductAttributesMeasure("A1", "B1");
			ProductAttributesMeasure prodAttribs3 = new ProductAttributesMeasure("A1");
			ProductAttributesMeasure prodAttribs4 = new ProductAttributesMeasure();
			ProductAttributesMeasure prodAttribs5 = new ProductAttributesMeasure("A1", "B1", "C1");
			ProductAttributesMeasure prodAttribs6 = new ProductAttributesMeasure();
			ProductAttributesMeasure prodAttribs7 = new ProductAttributesMeasure("A1", "", "C1");
			ProductAttributesMeasure prodAttribs8 = new ProductAttributesMeasure("A1", "B2", "C1");

			Assert(prodAttribs1.Equals(prodAttribs1));
			Assert(!prodAttribs1.Equals(null));
			Assert(!prodAttribs1.Equals(prodAttribs2));
			Assert(!prodAttribs1.Equals(prodAttribs3));
			Assert(!prodAttribs1.Equals(prodAttribs4));
			Assert(prodAttribs1.Equals(prodAttribs5));
			Assert(prodAttribs4.Equals(prodAttribs6));

			AssertEquals(0, prodAttribs4.GetHashCode());
			AssertNotEquals(0, prodAttribs1.GetHashCode());
			Assert(prodAttribs1.GetHashCode() == prodAttribs5.GetHashCode());

			Assert(!prodAttribs1.IsEmpty);
			Assert(prodAttribs4.IsEmpty);

			AssertEquals(-1, prodAttribs1.GetSimilarity(prodAttribs2));
			AssertEquals(6, prodAttribs2.GetSimilarity(prodAttribs1));

			AssertEquals(-1, prodAttribs1.GetSimilarity(prodAttribs3));
			AssertEquals(4, prodAttribs3.GetSimilarity(prodAttribs1));

			AssertEquals(-1, prodAttribs1.GetSimilarity(prodAttribs4));
			AssertEquals(0, prodAttribs4.GetSimilarity(prodAttribs1));
			AssertEquals(9, prodAttribs4.GetSimilarity(prodAttribs6));

			AssertEquals(9, prodAttribs1.GetSimilarity(prodAttribs5));
			AssertEquals(9, prodAttribs5.GetSimilarity(prodAttribs1));

			AssertEquals(-1, prodAttribs1.GetSimilarity(prodAttribs7));
			AssertEquals(5, prodAttribs7.GetSimilarity(prodAttribs1));

			AssertEquals(-1, prodAttribs1.GetSimilarity(prodAttribs8));
			AssertEquals(-1, prodAttribs8.GetSimilarity(prodAttribs1));
		}

		public void TestProductAttributesMeasureWithNullAttribute()
		{
			var prodAttribs = new ProductAttributesMeasure("A1", "B1", "C1");
			var prodAttribsSame = new ProductAttributesMeasure("A1", "B1", "C1");
			var prodAttribsWithNull = new ProductAttributesMeasure("A1", null, "C1");
			var prodAttribsNull = new ProductAttributesMeasure(null, null, null);
			AssertEquals(0, prodAttribs.CompareTo(prodAttribsSame));
			AssertEquals(1, prodAttribs.CompareTo(prodAttribsWithNull));
			AssertEquals(0, prodAttribsNull.CompareTo(prodAttribsNull));
			AssertEquals(1, prodAttribsWithNull.CompareTo(prodAttribsNull));
			AssertEquals(1, prodAttribs.CompareTo(null));
		}
	}
}
