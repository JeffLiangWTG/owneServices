using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	[TestedType(typeof(CustomCommandsProvider))]
	sealed class CustomCommandsProviderTest : CommandProviderTest
	{
		protected override ICommandProvider CreateNewModule()
		{
			var supporter = new Mock<IVisualizableDocumentSupporter>();
			supporter
				.Setup(s => s.GetCustomCommands(It.IsAny<string>()))
				.Returns(System.Array.Empty<ICommand>());

			return new CustomCommandsProvider(supporter.Object, DataContext.ShippingOrder);
		}
	}
}
