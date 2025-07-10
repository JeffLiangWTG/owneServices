using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.LogDocumentRenderer.Testing
{
	[TestedType(typeof(LogDocumentRendererRegistryItem))]
	public sealed class DocBuilderThemeRegistryItemTest : StronglyTypedRegistryItemTestCase<LogDocumentRendererRegistry>
	{
		protected override StronglyTypedRegistryItem<LogDocumentRendererRegistry, LogDocumentRendererRegistry> GetNewRegistryItem()
		{
			return new LogDocumentRendererRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", new LogDocumentRendererRegistry());
		}
	}
}
