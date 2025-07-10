using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.DbUpgrader.Shared;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class MainDbTemplateTest : TestCase
	{
		public void TestMessagesAreLoggedForDifferentStages()
		{
			// Arrange
			var expectedSubTasks = new[]
			{
				"\tCreating additional schemas",
				"\tCreating xml schemas",
				"\tCreating objects (e.g. tables)",
				"\tAll inside-transaction template database objects are done",
				"\tCreating extra objects (e.g. client-specific tables)",
			};

			var upgradeManager = new Mock<IUpgradeManager>();

			var messages = new HashSet<string>();
			upgradeManager.Setup(x => x.ShowInfoMessage(It.IsAny<string>())).Callback((string message) => messages.Add(message));

			IAuxiliaryDbCreator template = new MainDbTemplate(upgradeManager.Object, nameof(TestMessagesAreLoggedForDifferentStages));

			using (new DisposableAction(
				// Act
				() => template.CreateDropExisting(),
				// Cleanup
				() => template.Drop()))
			{
				// Assert
				AssertEquals(true, messages.IsSupersetOf(expectedSubTasks));
			}
		}
	}
}
