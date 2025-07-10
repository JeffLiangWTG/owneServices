using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(WebPrintNudgeSuspendingRegistryItem))]
	sealed class WebPrintNudgeSuspendingRegistryItemTest : StronglyTypedRegistryItemTestCase<WebPrintNudgeSuspending>
	{
		protected override StronglyTypedRegistryItem<WebPrintNudgeSuspending, WebPrintNudgeSuspending> GetNewRegistryItem()
		{
			return new WebPrintNudgeSuspendingRegistryItem("a", (NoResString)"b", (NoResString)"c", (NoResString)"d", RegistryStorageFlags.System, RegistryOptions.IsOnlyForController, new WebPrintNudgeSuspending());
		}
	}
}
