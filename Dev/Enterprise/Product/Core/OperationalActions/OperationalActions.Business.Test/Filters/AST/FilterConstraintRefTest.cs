using System.Collections.Generic;
using System.Text;
using Enterprise.Services.OperationalActions.Support;
using Moq;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.AST.Testing
{
	internal sealed class FilterConstraintRefTest : TestCase
	{
		public void TestCollectConstraints()
		{
			var list = new List<string>();
			var constraint1 = new FilterConstraintRef("nameA");
			var constraint2 = new FilterConstraintRef("nameA");
			var constraint3 = new FilterConstraintRef("nameB");
			constraint1.CollectConstraints(list);
			AssertContainsExactElementsInAnyOrder(new string[] { "nameA" }, list);
			constraint2.CollectConstraints(list);
			AssertContainsExactElementsInAnyOrder(new string[] { "nameA" }, list);
			constraint3.CollectConstraints(list);
			AssertContainsExactElementsInAnyOrder(new string[] { "nameA", "nameB" }, list);
		}

		public void TestEvaluate()
		{
			var provider = new Mock<IFilterValueProvider>();
			var constraint = new Mock<IFilterConstraint>();
			provider.Setup(m => m.GetConstraint("name")).Returns(constraint.Object);
			constraint.Setup(m => m.GetValue()).Returns("value");
			AssertEquals("value", new FilterConstraintRef("name").Evaluate(provider.Object));
			provider.Setup(m => m.GetConstraint("name")).Returns((IFilterConstraint)null);
			AssertEquals(null, new FilterConstraintRef("name").Evaluate(provider.Object));
		}

		public void TestGetValuesForRequirement()
		{
			var inRequirement = new FilterRequirement("Blat") { "Value1" };
			var outRequirement1 = new FilterConstraintRef("Blat").GetValuesForRequirement(inRequirement);
			var outRequirement2 = new FilterConstraintRef("blat").GetValuesForRequirement(inRequirement);
			var outRequirement3 = new FilterConstraintRef("Bob").GetValuesForRequirement(inRequirement);
			CombineAssertions(delegate
			{
				AssertEquals("Constraint Name Unchanged", "Blat", inRequirement.ConstraintName);
				AssertContainsExactElementsInAnyOrder("Values Unchanged", new string[] { "Value1" }, outRequirement1);
				AssertSame("1) Exact Match", inRequirement, outRequirement1);
				AssertSame("2) Case-insensitive Match", inRequirement, outRequirement2);
				AssertNotEquals("3) No Match", inRequirement, outRequirement3);
				AssertEquals("3) Constraint Name", "Blat", outRequirement1.ConstraintName);
				AssertContainsExactElementsInAnyOrder("3) Values", System.Array.Empty<string>(), outRequirement3);
			});
		}

		public void TestAppend()
		{
			var constraint = new FilterConstraintRef("name");
			var builder = new StringBuilder();
			constraint.Append(builder);
			AssertEquals("name", builder.ToString());
		}

		public void TestAddConstraintPrefix()
		{
			IFilterOperand result = new FilterConstraintRef("blaticus").AddConstraintPrefix("bob");
			AssertType(typeof(FilterConstraintRef), result);
			var filterRef = (FilterConstraintRef)result;
			AssertEquals("bob.blaticus", filterRef.ConstraintName);
		}

		public void TestRemoveConstraintPrefix()
		{
			CombineAssertions(delegate
			{
				var list = new[] { new
				{
				Original = "bob.fread.blaticus", Rem = "bob", Expected = "fread.blaticus"
				}

				, new
				{
				Original = "bob+fread.blaticus", Rem = "bob", Expected = "fread.blaticus"
				}

				, new
				{
				Original = "bob.fread.blaticus", Rem = "bob.fread", Expected = "blaticus"
				}

				, new
				{
				Original = "bob+fread.blaticus", Rem = "bob+fread", Expected = "blaticus"
				}

				, new
				{
				Original = "bob.fread.blaticus", Rem = "bob+fread", Expected = "blaticus"
				}

				, new
				{
				Original = "bob+fread.blaticus", Rem = "bob.fread", Expected = "blaticus"
				}

				, };
				foreach (var set in list)
				{
					string message = string.Format("'{0}' - '{1}' => '{2}'", set.Original, set.Rem, set.Expected);
					IFilterOperand result = new FilterConstraintRef(set.Original).RemoveConstraintPrefix(set.Rem);
					FilterConstraintRef filterRef = result as FilterConstraintRef;
					if (filterRef == null)
					{
						AssertType(message, typeof(FilterConstraintRef), result);
					}
					else
					{
						AssertEquals(message, set.Expected, filterRef.ConstraintName);
					}
				}
			});
		}
	}
}
