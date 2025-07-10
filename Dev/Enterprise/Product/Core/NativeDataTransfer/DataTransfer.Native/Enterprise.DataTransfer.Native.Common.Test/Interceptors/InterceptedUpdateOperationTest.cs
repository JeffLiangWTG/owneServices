using System;
using Enterprise.DataTransfer.Native.Common.Operations;
using Moq;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Common.Interceptors
{
	public class InterceptedUpdateOperationTest : TransactionedTestCase
	{
		[ExpectNoExceptions]
		public void TestInterceptor_Disable()
		{
			entitySet = new EntitySet("Organization");
			setting.SetupGet(s => s.Enable).Returns(false);

			interceptedOperation.Update(entitySet);

			interceptor.Verify(i => i.Invoke(It.IsAny<IEntitySet>()), Times.Never);
			updateOperation.Verify(u => u.Update(It.IsAny<IEntitySet>()), Times.Once);

			mocks.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestInterceptor_Enable()
		{
			entitySet = new EntitySet("Organization");

			setting.Setup(s => s.Interceptor).Returns(interceptor.Object);
			setting.Setup(s => s.Enable).Returns(true);
			setting.Setup(s => s.EnableList).Returns(Array.Empty<String>());
			setting.Setup(s => s.DisableList).Returns(Array.Empty<String>());

			interceptedOperation.Update(entitySet);

			interceptor.Verify(i => i.Invoke(It.IsAny<IEntitySet>()), Times.Once);
			mocks.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestInterceptor_Filter_Entity_In_DisableList()
		{
			entitySet = new EntitySet("Organization");

			setting.Setup(s => s.Enable).Returns(true);
			setting.Setup(s => s.DisableList).Returns(new String[] { "Organization" });

			interceptedOperation.Update(entitySet);

			interceptor.Verify(i => i.Invoke(It.IsAny<IEntitySet>()), Times.Never);
			updateOperation.Verify(u => u.Update(It.IsAny<IEntitySet>()), Times.Once);

			mocks.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestInterceptor_Filter_Entity_Not_In_EnableList()
		{
			entitySet = new EntitySet("Organization");

			setting.Setup(s => s.Enable).Returns(true);
			setting.Setup(s => s.EnableList).Returns(new String[] { "Order" });
			setting.Setup(s => s.DisableList).Returns(Array.Empty<String>());

			interceptedOperation.Update(entitySet);

			interceptor.Verify(i => i.Invoke(It.IsAny<IEntitySet>()), Times.Never);
			updateOperation.Verify(u => u.Update(It.IsAny<IEntitySet>()), Times.Once);

			mocks.VerifyAll();
		}

		protected override void SetUp()
		{
			base.SetUp();
			mocks = new MockRepository(MockBehavior.Loose);
			updateOperation = mocks.Create<IUpdateOperation>();

			setting = mocks.Create<IInterceptorSetting>();
			interceptor = mocks.Create<IInterceptor<IEntitySet>>();

			context = SetUpContext();

			interceptedOperation = new InterceptedUpdateOperation(context)
									{
										UpdateOperation = updateOperation.Object
									};
		}

		EntityContext SetUpContext()
		{
			var result = new EntityContext(new AncillaryImportServices(), new FactoryProvider());
			result.InterceptorSettings.Add(setting.Object);
			return result;
		}

		MockRepository mocks;
		Mock<IUpdateOperation> updateOperation;
		IEntitySet entitySet;
		IEntityContext context;
		Mock<IInterceptorSetting> setting;
		Mock<IInterceptor<IEntitySet>> interceptor;
		InterceptedUpdateOperation interceptedOperation;
	}
}
