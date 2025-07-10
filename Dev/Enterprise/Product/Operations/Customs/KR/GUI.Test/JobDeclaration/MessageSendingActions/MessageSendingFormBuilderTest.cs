using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class MessageSendingFormBuilderTest : TestCaseWithFactory
	{
		public void TestMessageSendingFormBuilders()
		{
			var builderType = typeof(MessageSendingFormBuilder);
			CombineAssertions("MessageSendingFormTest is implemented.", () =>
			{
				foreach (var type in builderType.Assembly.GetTypes().Where(x => x.Name.EndsWith(nameof(MessageSendingFormBuilder))))
				{
					if (builderType.IsAssignableFrom(type))
					{
						var unitTestName = type.Name.Split(new string[] { nameof(MessageSendingFormBuilder) }, System.StringSplitOptions.None)[0] + nameof(MessageSendingActionForm);
						Assert($"{unitTestName} does not exist.", GetType().Assembly.GetTypes().Any(x => x.Name.StartsWith(unitTestName)));
					}
				}
			});
		}
	}
}
