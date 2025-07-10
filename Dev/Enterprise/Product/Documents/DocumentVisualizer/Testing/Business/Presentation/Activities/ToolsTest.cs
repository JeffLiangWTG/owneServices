using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Presentation;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	[TestedType(typeof(Tools))]
	sealed class ToolsTest : CommandProviderTest
	{
		protected override ICommandProvider CreateNewModule()
		{
			var macroEvaluationContext = new Mock<IMacroEvaluationContext>();
			return new Tools(macroEvaluationContext.Object);
		}
	}
}
