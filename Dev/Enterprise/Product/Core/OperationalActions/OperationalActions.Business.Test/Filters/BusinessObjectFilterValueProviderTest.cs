using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Services.OperationalActions.Support.Testing;
using Moq;

namespace Enterprise.Services.OperationalActions.Business
{
	internal sealed class BusinessObjectFilterValueProviderTest : TestCaseWithFactory
	{
		public void TestDirectValue()
		{
			IFilterConstraint constraint = Provider.GetConstraint("Z0_Code");
			AssertNotNull(constraint);
			Provider.Source = Dummy1;
			AssertEquals("BOB", constraint.GetValue());
			Provider.Source = Dummy2;
			AssertEquals("MIK", constraint.GetValue());
		}

		public void TestValueViaScalar()
		{
			var mock1 = New<DummyBusinessObjectWithDocumentSupport>("mock1");
			mock1.Setup(m => m.Other).Returns(Dummy1);
			var mock2 = New<DummyBusinessObjectWithDocumentSupport>("mock2");
			mock2.Setup(m => m.Other).Returns(Dummy2);
			IFilterConstraint constraint = Provider.GetConstraint("Other.Z0_Code");
			AssertNotNull(constraint);
			Provider.Source = mock1.Object;
			AssertEquals("BOB", constraint.GetValue());
			Provider.Source = mock2.Object;
			AssertEquals("MIK", constraint.GetValue());
		}

		public void TestValueViaCollection()
		{
			// it's not possible to get a value via a collection, so always return null (unmatchable) as the value.
			var mock = New<DummyBusinessObjectWithDocumentSupport>("mock1");
			mock.Setup(m => m.Collection).Returns(NewDummyChildCollection(Dummy1));
			IFilterConstraint constraint = Provider.GetConstraint("Collection.Z0_Code");
			AssertNotNull(constraint);
			Provider.Source = mock.Object;
			AssertEquals(null, constraint.GetValue());
		}

		public void TestValueViaEmptyScalar()
		{
			var mock1 = New<DummyBusinessObjectWithDocumentSupport>("mock1");
			mock1.Setup(m => m.Other).Returns((DummyBusinessObjectWithDocumentSupport)null);
			IFilterConstraint constraint = Provider.GetConstraint("Other.Z0_Code");
			AssertNotNull(constraint);
			Provider.Source = mock1.Object;
			AssertEquals("", constraint.GetValue());
		}

		public void TestValueViaInvalidScalar()
		{
			AssertEquals(null, Provider.GetConstraint("Blaticus"));
		}

		#region Implementation
		BusinessObjectFilterValueProvider Provider
		{
			get
			{
				return provider ?? (provider = new BusinessObjectFilterValueProvider(typeof(DummyBusinessObjectWithDocumentSupport)));
			}
		}

		BusinessObjectFilterValueProvider provider;
		DummyBusinessObjectWithDocumentSupport Dummy1
		{
			get
			{
				if (dummy1 == null)
				{
					dummy1 = Factory.New<DummyBusinessObjectWithDocumentSupport>();
					dummy1.Z0_Code = "BOB";
				}

				return dummy1;
			}
		}

		DummyBusinessObjectWithDocumentSupport dummy1;
		DummyBusinessObjectWithDocumentSupport Dummy2
		{
			get
			{
				if (dummy2 == null)
				{
					dummy2 = Factory.New<DummyBusinessObjectWithDocumentSupport>();
					dummy2.Z0_Code = "MIK";
				}

				return dummy2;
			}
		}

		DummyBusinessObjectWithDocumentSupport dummy2;
		Mock<BizObjT> New<BizObjT>(string name)
			where BizObjT : BusinessObject
		{
			var result = Factory.NewMoq<BizObjT>();
			result.Name = name;
			if (mocks == null)
			{
				mocks = new List<Mock>();
			}

			mocks.Add(result);
			return result;
		}

		DummyChildBusinessObjectCollection NewDummyChildCollection(params DummyBusinessObjectWithDocumentSupport[] children)
		{
			var collection = new DummyChildBusinessObjectCollection(Factory);
			collection.AddRange(children);
			return collection;
		}

		List<Mock> mocks;
		protected override void TearDown()
		{
			base.TearDown();
			if (mocks != null)
			{
				foreach (var mock in mocks)
				{
					mock.VerifyAll();
				}
			}
		}
		#endregion
	}
}
