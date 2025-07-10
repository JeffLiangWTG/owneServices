using System.Linq;
using Enterprise.Core.Environment;
using Enterprise.Core.Modules;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class ModuleListingSubsetRegisterTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestInitializeSecurityCheckpoints()
		{
			// Arrange
			var securityMock = new Mock<IZSecurity>();
			securityMock.Setup(security => security.AddCheckPoint(It.IsAny<CheckpointLookupKey>(), It.IsAny<ISecurityCheckpoint>())).Returns(true);

			// Act
			DummyModuleListingSubset.InitializeDummyModule.Value = true;
			ModuleListingSubsetRegister.InitializeSecurityCheckpoints(securityMock.Object);

			// Assert
			securityMock.Verify(s => s.AddCheckPoint(It.Is<CheckpointLookupKey>(k => k.Code == "Dummy"), It.IsAny<ISecurityCheckpoint>()), Times.Once);
		}

		public void TestInitializeModuleTree()
		{
			// Arrange
			var securityMock = new Mock<IZSecurity>();
			var checkpointMock = new Mock<ISecurityCheckpoint>();
			var displayText = (NoResString)"Text";

			var categories = new ModuleTreeCategories(
				new("Saltar", displayText, checkpointMock.Object),
				new("Funcionar", displayText, checkpointMock.Object),
				new("Administrar", displayText, checkpointMock.Object),
				new("Mantener", displayText, checkpointMock.Object));

			// Act
			DummyModuleListingSubset.InitializeDummyModule.Value = true;
			ModuleListingSubsetRegister.InitializeModuleTree(categories, securityMock.Object);

			// Assert
			AssertContainsExactElementsInAnyOrder([("Dummy", (NoResString)"Added in DummyModuleListingSubset")], categories.Jump.Sections.Values.Cast<ModuleSection>().Select(s => (s.Name, s.DisplayText)));
		}
	}
}
