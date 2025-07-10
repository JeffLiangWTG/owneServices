using System;
using System.Reflection;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Moq;

namespace Enterprise.Customs.ES.Messaging.Testing
{
	public static class RandomGeneratorHelper
	{
		public static void MockRandomGenerator<TProvider, TObject>(XMLMessageBuilder<TProvider, TObject> xmlMessageBuilder, int randomNumber)
			where TProvider : IESEDIMessageCollectionProvider
		{
			SetRandomGenerator(xmlMessageBuilder, typeof(XMLMessageBuilder<TProvider, TObject>), GetMock(randomNumber));
		}

		public static void MockRandomGenerator(IMessageBuilderBase messageBuilder, int randomNumber)
		{
			var mockRandomGenerator = GetMock(randomNumber);
			var targetType = messageBuilder.GetType();
			while (targetType != typeof(object))
			{
				SetRandomGenerator(messageBuilder, targetType, mockRandomGenerator);
				targetType = targetType.BaseType;
			}
		}

		static IRandomGenerator GetMock(int randomNumber)
		{
			var mockRandomGenerator = new Mock<IRandomGenerator>();
			mockRandomGenerator.Setup(m => m.Generate(It.IsAny<int>())).Returns(randomNumber);
			return mockRandomGenerator.Object;
		}

		static void SetRandomGenerator(object target, Type targetType, IRandomGenerator randomGenerator)
		{
			targetType.GetField("randomGenerator", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(target, randomGenerator);
		}
	}
}
