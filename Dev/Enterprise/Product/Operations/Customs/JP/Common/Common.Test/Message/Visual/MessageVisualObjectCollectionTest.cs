using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing;

[TestedType(typeof(MessageVisualObjectCollection))]
sealed class MessageVisualObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<MessageVisualObjectCollection>
{
	protected override MessageVisualObjectCollection GetCollectionToTest() => new(Factory);

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var providerMock = new Mock<IMessageContentProvider>();
		providerMock.Setup(provider => provider.Factory).Returns(Factory);
		providerMock.Setup(provider => provider.GetMessageData()).Returns(Array.Empty<byte>());
		providerMock.Setup(provider => provider.ProcedureCode).Returns("TST");

		return new MessageVisualObject(providerMock.Object);
	}
}
