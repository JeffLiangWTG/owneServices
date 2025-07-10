using System;
using System.Reflection;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class SerializableBusinessObjectTest : TestCaseWithFactory
	{
		public void TestData()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var wrappedBizo = new WrappedBusinessObject(dummy);

			AssertSame(dummy, wrappedBizo.BaseBusinessObject);
		}

		public void TestWrappBusinessObjectSerializable()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var wrappedBizo = new WrappedBusinessObject(dummy);

			var bizoType = wrappedBizo.GetType();
#pragma warning disable SYSLIB0050 // Type or member is obsolete
			Assert(bizoType.IsSerializable);
#pragma warning restore SYSLIB0050 // Type or member is obsolete

			var fieldInfo = bizoType.GetField("baseBusinessObject", BindingFlags.NonPublic | BindingFlags.Instance);
			Assert(fieldInfo.IsDefined(typeof(NonSerializedAttribute), false));
		}
	}
}
