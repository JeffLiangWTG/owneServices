using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	sealed class LightweightBizoBaseTest : TestCaseWithFactory
	{
		public void TestGetHeavy()
		{
			var heavy = Factory.New<DummyBusinessObject>();
			Factory.Save();

			var light = new LightweightBizo<DummyBusinessObject>(heavy);
			var actual = light.GetHeavy(Factory);

			var factory2 = new BusinessObjectFactory();
			var actual2 = light.GetHeavy(factory2);
			var actual2again = light.GetHeavy(factory2);

			Assert(object.ReferenceEquals(heavy, actual));

			AssertEquals(heavy.PK, actual2.PK);
			Assert(object.ReferenceEquals(actual2, actual2again));
			AssertArrayEqualsByElements(((INeedRow)heavy).Row.ItemArray, ((INeedRow)actual2).Row.ItemArray);
			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(DummyBizoSchema.Constants.TableName, 0);
			AssertDbHits(expectedDbHits, factory2);
		}

		public class DummyBizoTypeDecider : TypeDecider
		{
			public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory) => typeof(DummyBizoDerived);
			public override Type GetTypeForNew() => typeof(DummyBizoDerived);
			public override Type GetTypeForBinding() => typeof(DummyBizoDerived);
		}

		public class DummyBizoBase : DummyBusinessObject
		{
			public readonly static new DummyBizoTypeDecider TypeDecider = new DummyBizoTypeDecider();
			public DummyBizoBase(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
		}

		public class DummyBizoDerived : DummyBizoBase
		{
			public DummyBizoDerived(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
		}

		public void TestGetHeavyForDerivedType()
		{
			Factory.RefreshEnabled = false;
			var heavy = Factory.New<DummyBizoDerived>();
			Factory.Save();

			var light = new LightweightBizo<DummyBizoBase>(heavy);
			light.DiscardAnyFactoryReferenceForTest();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var bizoPreLoadedIn2 = factory2.Load<DummyBizoBase>(heavy.PK);
			var actual2 = light.GetHeavy(factory2);
			var actual2again = light.GetHeavy(factory2);

			AssertEquals(heavy.PK, actual2.PK);
			AssertType<DummyBizoDerived>(actual2);
			Assert(object.ReferenceEquals(actual2, actual2again));
			AssertArrayEqualsByElements(
				((INeedRow)heavy).Row.ItemArray.Where(o => o.GetType().Name != "SqlGeography").ToArray(),
				((INeedRow)actual2).Row.ItemArray.Where(o => o.GetType().Name != "SqlGeography").ToArray());

			var heavyGeos = from object obj in ((INeedRow)heavy).Row.ItemArray
							where obj.GetType().Name == "SqlGeography"
							select obj.ToString();
			var actual2Geos = from object obj in ((INeedRow)actual2).Row.ItemArray
							  where obj.GetType().Name == "SqlGeography"
							  select obj.ToString();
			AssertArrayEqualsByElements(
				heavyGeos.ToArray(),
				actual2Geos.ToArray());
		}

		public void TestConstructor_BizoNotInDatabase()
		{
			Factory.RefreshEnabled = false;
			var heavy = Factory.New<DummyBusinessObject>();
			heavy.Z0_AnotherDecimal = 1.23m;
			heavy.Z0_Description = "some text";
			var light = new LightweightBizo<DummyBusinessObject>(heavy);
			var actual = light.GetHeavy(new BusinessObjectFactory() { RefreshEnabled = false });
			Assert(object.ReferenceEquals(heavy, actual));
			Assert("given factory is ignored", object.ReferenceEquals(Factory, actual.Factory));
			light.DiscardAnyFactoryReferenceForTest();
			var actual2 = light.GetHeavy(new BusinessObjectFactory() { RefreshEnabled = false });
			Assert("new object in new factory", !object.ReferenceEquals(heavy, actual2));
			AssertEquals(1.23m, actual2.Z0_AnotherDecimal);
			AssertEquals("some text", actual2.Z0_Description);
		}

		public void TestTryGetHeavyFromAnotherThreadNoErrorReport()
		{
			var heavy = Factory.New<DummyBusinessObject>();
			var light = new LightweightBizo<DummyBusinessObject>(heavy);
			ErrorReporter.Clear();

			var thread = new Thread(new ThreadStart(() =>
			{
				light.TryGetHeavy();
			}));

			thread.Start();
			thread.Join();

			AssertNull(ErrorReporter.LastExceptionReported);
			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
		}
	}
}
