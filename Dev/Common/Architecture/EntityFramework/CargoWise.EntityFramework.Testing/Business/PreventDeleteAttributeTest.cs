using System.Data;
using CargoWise.Common;

namespace CargoWise.EntityFramework.Testing
{
	sealed class PreventDeleteAttributeTest : TestCaseWithFactory
	{
		public void TestIsTrue()
		{
			Assert(PreventDeleteAttribute.IsTrue(typeof(DummyWithPreventDeleteTrue)));
			Assert(!PreventDeleteAttribute.IsTrue(typeof(DummyWithPreventDeleteFalse)));
			Assert(PreventDeleteAttribute.IsTrue(typeof(DummyWithInheritedPreventDeleteTrue)));
			Assert(!PreventDeleteAttribute.IsTrue(typeof(DummyWithInheritedPreventDeleteFalse)));
			Assert("If PreventDelete is not on class, then you can definitely delete a class", !PreventDeleteAttribute.IsTrue(typeof(DummyWithoutPreventDelete)));
			Assert("Overriden to false", !PreventDeleteAttribute.IsTrue(typeof(DummyWithInheritedPreventDeleteTrueMultiple)));
			AssertEquals("No exceptions should be thrown", "", ErrorReporter.LastMessageReported);
		}

		[PreventDelete(true)]
		class DummyWithPreventDeleteTrue : DummyBusinessObject
		{
			public DummyWithPreventDeleteTrue(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }
		}

		[PreventDelete(false)]
		class DummyWithPreventDeleteFalse : DummyBusinessObject
		{
			public DummyWithPreventDeleteFalse(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }
		}

		class DummyWithoutPreventDelete : DummyBusinessObject
		{
			public DummyWithoutPreventDelete(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }
		}

		class DummyWithInheritedPreventDeleteTrue : DummyWithPreventDeleteTrue
		{
			public DummyWithInheritedPreventDeleteTrue(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }
		}

		class DummyWithInheritedPreventDeleteFalse : DummyWithPreventDeleteFalse
		{
			public DummyWithInheritedPreventDeleteFalse(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }
		}

		[PreventDelete(false)]
		class DummyWithInheritedPreventDeleteTrueMultiple : DummyWithPreventDeleteTrue
		{
			public DummyWithInheritedPreventDeleteTrueMultiple(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }
		}
	}
}
