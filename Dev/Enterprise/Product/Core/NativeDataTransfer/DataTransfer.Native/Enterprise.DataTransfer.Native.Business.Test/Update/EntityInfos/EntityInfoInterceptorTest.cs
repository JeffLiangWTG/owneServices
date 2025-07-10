using Enterprise.DataTransfer.Native.Business.Responses;
using Enterprise.DataTransfer.Native.Common;
using Moq;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Update.EntityInfos
{
	public class EntityInfoInterceptorTest : TransactionedTestCase
	{
		[ExpectNoExceptions]
		public void TestUpdate()
		{
			// Arrange
			entitySet = new EntitySet("Organization");

			// Act
			interceptor.Invoke(entitySet);

			// Assert
			entityInfoHelper.Verify(
				m => m.SaveForeignCode(It.IsAny<EntityInfo>(), It.IsAny<IEntitySet>()),
				Times.Once());
			entityInfoHelper.Verify(
				m => m.SaveLocalCode(It.IsAny<EntityInfo>(), It.IsAny<IEntitySet>()),
				Times.Once());
			entityInfoHelper.Verify(
				m => m.SavePrimaryKey(It.IsAny<EntityInfo>(), It.IsAny<IEntitySet>()),
				Times.Once());
		}

		public void DummyMethod(IEntitySet source)
		{
		}

		protected override void SetUp()
		{
			base.SetUp();
			entityInfoHelper = new Mock<IEntityInfoHelper>();

			entityInfo = new EntityInfo();
			var sessionServices = new AncillaryImportServices();
			context = new UpdateContext(sessionServices, new FactoryProvider());

			setting =
				new EntityInfoSetting
				{
					EntityInfo = entityInfo
				};
			interceptor =
				new EntityInfoInterceptor(setting, sessionServices)
				{
					Function = DummyMethod,
					EntityInfoHelper = entityInfoHelper.Object
				};

			context.InterceptorSettings.Add(setting);
		}

		IEntitySet entitySet;
		IUpdateContext context;
		EntityInfoSetting setting;
		EntityInfoInterceptor interceptor;
		Mock<IEntityInfoHelper> entityInfoHelper;
		EntityInfo entityInfo;
	}
}
