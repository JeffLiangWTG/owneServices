using System;
using CargoWise.Application;

namespace CargoWise.EntityFramework.Testing
{
	sealed class BusinessObjectCreationSourceStackTest : TestCaseWithDummy
	{
		public void TestCanPushCanPop()
		{
			var source1 = new Moq.Mock<IBusinessObjectCreationSource>();
			source1.Setup(x => x.CreationSourceCode).Returns("AAA");
			var source2 = new Moq.Mock<IBusinessObjectCreationSource>();
			source2.Setup(x => x.CreationSourceCode).Returns("BBB");

			var stack = new BusinessObjectCreationSourceStack();
			var disposable1 = stack.PushCreationSource(source1.Object);
			AssertEquals(source1.Object, stack.CurrentSource);

			var disposable2 = stack.PushCreationSource(source2.Object);
			AssertEquals(source2.Object, stack.CurrentSource);

			disposable2.Dispose();
			AssertEquals(source1.Object, stack.CurrentSource);

			disposable1.Dispose();
			AssertEquals(null, stack.CurrentSource);
		}

		public void TestOutOfOrderDispose_ThrowException()
		{
			var source1 = new Moq.Mock<IBusinessObjectCreationSource>();
			source1.Setup(x => x.CreationSourceCode).Returns("AAA");
			var source2 = new Moq.Mock<IBusinessObjectCreationSource>();
			source2.Setup(x => x.CreationSourceCode).Returns("BBB");

			var stack = new BusinessObjectCreationSourceStack();
			var disposable1 = stack.PushCreationSource(source1.Object);
			var disposable2 = stack.PushCreationSource(source2.Object);

			AssertExceptionThrown<InvalidOperationException>(
				"Disposable1 got disposed before Disposable2 - exception should be seen",
				"The pushing and popping of IBusinessObjectCreationSources is out of order",
				() => disposable1.Dispose());
		}

		public void TestIsPresentInObjectFactory()
		{
			var instanceFromObjectFactory = ObjectFactory.Get<IBusinessObjectCreationSourceStack>();

			var source = new Moq.Mock<IBusinessObjectCreationSource>();
			source.Setup(x => x.CreationSourceCode).Returns("AAA");

			instanceFromObjectFactory.PushCreationSource(source.Object).Dispose();
			Assert("It fails if there's an exception", true);
		}
	}
}
