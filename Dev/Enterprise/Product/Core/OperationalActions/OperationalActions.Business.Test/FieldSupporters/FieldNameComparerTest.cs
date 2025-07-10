using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business
{
	internal sealed class FieldNameComparerTest : TestCase
	{
		public void TestCompare()
		{
			AssertFieldNamesEqual("js_uniqueconsignref", "JS_UNIQUECONSIGNREF", "JS_UniqueConsignRef");
			AssertFieldNamesEqual("Booking.JS_UniqueConsignRef", "Booking+JS_UniqueConsignRef");
			AssertFieldNamesOrder("Alpha", "Beta");
			AssertFieldNamesOrder("Alpha", "AlphaBeta");
		}

		#region Implementation
		void AssertFieldNamesEqual(string name1, params string[] otherNames)
		{
			foreach (string name2 in otherNames)
			{
				AssertFieldNamesEqual(name1, name2);
			}
		}

		void AssertFieldNamesEqual(string name1, string name2)
		{
			AssertEquals(string.Format("Compare('{0}', '{1}') == 0", name1, name2), 0, FieldNameComparer.Compare(name1, name2));
			AssertEquals(string.Format("Compare('{0}', '{1}') == 0", name2, name1), 0, FieldNameComparer.Compare(name2, name1));
			AssertEquals(string.Format("HashCode('{0}') == HashCode('{1}')", name1, name2), FieldNameComparer.GetHashCode(name1), FieldNameComparer.GetHashCode(name2));
		}

		void AssertFieldNamesOrder(string firstName, string secondName)
		{
			int result;
			if ((result = FieldNameComparer.Compare(firstName, secondName)) >= 0)
			{
				HtmlFail(Html(string.Format("Compare('{0}', '{1}') < 0", firstName, secondName)) + "</br>" + HtmlFormatBadValue(result));
			}

			if ((result = FieldNameComparer.Compare(secondName, firstName)) <= 0)
			{
				HtmlFail(Html(string.Format("Compare('{0}', '{1}') > 0", secondName, firstName)) + "</br>" + HtmlFormatBadValue(result));
			}

			Assert(true);
		}
		#endregion
	}
}
