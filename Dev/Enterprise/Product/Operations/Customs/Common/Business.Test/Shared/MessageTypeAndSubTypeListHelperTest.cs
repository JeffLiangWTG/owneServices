using NUnit.Framework;

namespace Enterprise.Customs.Common.Testing
{
	class MessageTypeAndSubTypeListHelperTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestGetMessageTypeAndSubTypeListProvider()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(MessageTypeAndSubTypeListHelper.GetMessageTypeAndSubTypeListProvider(Core.Constants.CountryCodes.Australia).GetType().FullName, Is.EqualTo("Enterprise.Customs.Business.MessageTypeAndSubTypeListProvider"));
				NUnit.Framework.Assert.That(MessageTypeAndSubTypeListHelper.GetMessageTypeAndSubTypeListProvider(Core.Constants.CountryCodes.NewZealand).GetType().FullName, Is.EqualTo("Enterprise.Customs.Business.MessageTypeAndSubTypeListProvider"));
				NUnit.Framework.Assert.That(MessageTypeAndSubTypeListHelper.GetMessageTypeAndSubTypeListProvider(Core.Constants.CountryCodes.Canada).GetType().FullName, Is.EqualTo("Enterprise.Customs.Business.MessageTypeAndSubTypeListProvider"));
				NUnit.Framework.Assert.That(MessageTypeAndSubTypeListHelper.GetMessageTypeAndSubTypeListProvider(Core.Constants.CountryCodes.UnitedStates).GetType().FullName, Is.EqualTo("Enterprise.Customs.US.Business.MessageTypeAndSubTypeListProvider"));
			});
		}
	}
}
