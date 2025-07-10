using System;
using System.Data;
using System.Linq.Expressions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using WTG.Rules.Engine;

namespace CargoWise.EntityFramework.Business.Rules.Testing
{
	sealed class BusinessObjectRuleMapperAdapterTests : TestCaseWithFactory
	{
		Expression ExpressionOf<T>(T obj)
		{
			return Expression.Constant(obj);
		}

		void AssertConversion<TIn, TOut>(BusinessObjectRuleMapperAdapter adapter, TIn obj, TOut result)
		{
			var expression = ExpressionOf(obj);
			var conversion = adapter.Convert(expression, typeof(TOut));
			var method = Expression.Lambda<Func<TOut>>(conversion).Compile();
			var message = typeof(TIn).Name + " ==> " + typeof(TOut).Name;

			AssertEquals(result, method());
		}

		public void TestConvert()
		{
			var adapter = new BusinessObjectRuleMapperAdapter();

			CombineAssertions(() =>
			{
				AssertConversion(adapter, 1, 1);
				AssertConversion(adapter, 1, 1d);
				AssertConversion(adapter, 1d, 1);
				AssertConversion(adapter, 1L, 1);
				AssertConversion(adapter, 1, 1L);

				AssertConversion(adapter, ZInt.Zero, 0);
				AssertConversion(adapter, 0, ZInt.Zero);

				AssertConversion(adapter, new ZString("BLAH"), "BLAH");
				AssertConversion(adapter, "BLAH", new ZString("BLAH"));

				var bizo = Factory.NewWithValidTestData<DummyBusinessObject>();
				AssertConversion(adapter, bizo, (BusinessObject)bizo);
				AssertConversion(adapter, (BusinessObject)bizo, bizo);
			});
		}

		public void TestGetObjectType()
		{
			var adapter = new BusinessObjectRuleMapperAdapter();

			AssertEquals("Should always return typeof(T)", typeof(int), adapter.GetObjectType(typeof(int)));
			AssertEquals("Should always return typeof(T)", typeof(ZInt), adapter.GetObjectType(typeof(ZInt)));
			AssertEquals("Should always return typeof(T)", typeof(BusinessObject), adapter.GetObjectType(typeof(BusinessObject)));
			AssertEquals("Should always return typeof(T)", typeof(DummyBusinessObject), adapter.GetObjectType(typeof(DummyBusinessObject)));
		}

		public void TestGetDependancyType_Simple()
		{
			var adapter = new BusinessObjectRuleMapperAdapter();

			AssertEquals(typeof(ZString), adapter.GetDependencyType(typeof(DummyBusinessObject), "Z0_NVarChar"));
			AssertEquals(typeof(ZDateTime), adapter.GetDependencyType(typeof(DummyBusinessObject), "RelatedDummy.RelatedDummy.RelatedDummy.Z0_Date"));
		}

		public void TestGetDependancyTypeUsesStaticPath()
		{
			var bizo = Factory.NewWithValidTestData<DummyWithOverride>();
			((DummyBusinessObject)bizo).Z0_Description = "Blah";

			var adapter = new BusinessObjectRuleMapperAdapter();
			var dependancyExpr = adapter.GetDependency(typeof(DummyBusinessObject), "Z0_Description");

			var lambda = Expression.Lambda<Func<IRuleDependency<DummyBusinessObject>>>(dependancyExpr).Compile();
			var dependency = lambda();

			RuleDependencyResult<ZString> result;
			dependency.TryGetValue(bizo, out result);

			AssertEquals("Blah", result.Value);
		}

		class DummyWithOverride : DummyBusinessObject
		{
			public DummyWithOverride(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public new ZInt Z0_Description => 0;
		}
	}
}
