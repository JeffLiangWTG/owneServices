using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(WebPrintNudgeRegistryItem))]
	sealed class WebPrintNudgeRegistryItemTest : StronglyTypedRegistryItemTestCase<WebPrintNudge>
	{
		protected override StronglyTypedRegistryItem<WebPrintNudge, WebPrintNudge> GetNewRegistryItem()
		{
			return new WebPrintNudgeRegistryItem("a", (NoResString)"b", (NoResString)"c", (NoResString)"d", RegistryStorageFlags.System, RegistryOptions.IsOnlyForController, new WebPrintNudge { EnableIPAddress = true });
		}
	}
}
