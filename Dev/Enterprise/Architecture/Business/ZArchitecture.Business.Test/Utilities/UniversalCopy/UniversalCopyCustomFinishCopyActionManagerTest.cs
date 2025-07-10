using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business.Utilities.UniversalCopy;
using Moq;

namespace Enterprise.ZArchitecture.Business.Test.Utilities.UniversalCopy
{
	sealed class UniversalCopyCustomFinishCopyActionManagerTest : TestCaseWithFactory
	{
		public void TestPerformUniversalCopyCustomFinishCopyActions_CallsAllActionsFromClassesInObjectFactoryInPriorityOrder()
		{
			// Arrange
			var mockAction1 = new Mock<IUniversalCopyCustomFinishCopyAction>();
			var mockAction2 = new Mock<IUniversalCopyCustomFinishCopyAction>();
			var priorityOrder = new List<int>();
			mockAction1.Setup(entity => entity.FinishCopyAction(It.IsAny<Dictionary<object, object>>()))
				.Callback<Dictionary<object, object>>(entities => priorityOrder.Add(1));
			mockAction1.Setup(entity => entity.Priority).Returns(1);
			mockAction2.Setup(entity => entity.FinishCopyAction(It.IsAny<Dictionary<object, object>>()))
				.Callback<Dictionary<object, object>>(entities => priorityOrder.Add(2));
			mockAction2.Setup(entity => entity.Priority).Returns(2);
			var copiedEntities = new Dictionary<object, object>() { { 1, 2 } };

			// Act
			using (ObjectFactory.Substitute("UniversalCopyCustomFinishCopyActionsList", new ListObject() { mockAction1.Object, mockAction2.Object }))
			{
				var universalCopyCustomFinishCopyActionManager = new UniversalCopyCustomFinishCopyActionManager();

				universalCopyCustomFinishCopyActionManager.PerformUniversalCopyCustomFinishCopyActions(copiedEntities);
			}

			// Assert
			mockAction1.Verify(action => action.FinishCopyAction(copiedEntities), Times.Once);
			mockAction2.Verify(action => action.FinishCopyAction(copiedEntities), Times.Once);

			AssertEquals(2, priorityOrder.Count);
			AssertEquals("Excecuted first priority first", 1, priorityOrder[0]);
			AssertEquals("Excecuted second priority last", 2, priorityOrder[1]);
		}
	}
}
