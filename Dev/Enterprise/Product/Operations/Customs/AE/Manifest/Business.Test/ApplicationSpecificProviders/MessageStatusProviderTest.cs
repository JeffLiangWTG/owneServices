using System;
using Enterprise.Customs.ASYCUDA.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

sealed class MessageStatusProviderTest : ASYCUDA.Business.Testing.MessageStatusProviderTest
{
	[ExpectNoExceptions]
	public override void TestAllowOriginalMessage()
	{
		AssertUponHasManifestBeenAcceptedByCustoms((s, p) => s.AllowOriginalMessage(p), false);
	}

	[ExpectNoExceptions]
	public override void TestAllowModificationMessage()
	{
		AssertUponHasManifestBeenAcceptedByCustoms((s, p) => s.AllowModificationMessage(p), true);
	}

	[ExpectNoExceptions]
	public override void TestAllowCancellationMessage()
	{
		AssertUponHasManifestBeenAcceptedByCustoms((s, p) => s.AllowCancellationMessage(p), true);
	}

	[ExpectNoExceptions]
	public override void TestAllowManifestCancellationMessage()
	{
		NUnit.Framework.Assert.That(GetMessageStatusProvider().AllowManifestCancellationMessage(Mock.Of<IMessageParent>()), Is.EqualTo(false));
	}

	[ExpectNoExceptions]
	public override void TestHasManifestBeenAcceptedByCustoms()
	{
		AssertUponHasManifestBeenAcceptedByCustoms((s, p) => s.HasManifestBeenAcceptedByCustoms(p), true);
	}

	[ExpectNoExceptions]
	public override void TestHasManifestBeenSubmittedToCustoms()
	{
		AssertUponHasManifestBeenAcceptedByCustoms((s, p) => s.HasManifestBeenSubmittedToCustoms(p), true);
	}

	[ExpectNoExceptions]
	public override void TestMessageStatusCanBeReset()
	{
		NUnit.Framework.Assert.That(GetMessageStatusProvider().MessageStatusCanBeReset(Mock.Of<IMessageParent>()), Is.EqualTo(false));
	}

	protected override ASYCUDA.Business.MessageStatusProvider GetMessageStatusProvider() => new MessageStatusProvider();

	void AssertUponHasManifestBeenAcceptedByCustoms(Func<MessageStatusProvider, IMessageParent, bool> testMethod, bool expectedValueOnAccepted) => CombineAssertions(() =>
	{
		var messageStatusProvider = (MessageStatusProvider)GetMessageStatusProvider();

		var mock = new Mock<IMessageParent>();
		mock.Setup(x => x.HasCustomsNumbers).Returns(true);
		mock.Setup(x => x.IsCustomsCleared).Returns(false);

		NUnit.Framework.Assert.That(testMethod(messageStatusProvider, mock.Object), Is.EqualTo(expectedValueOnAccepted), "HasCustomsNumbers&& !IsCustomsCleared");

		mock.Setup(x => x.HasCustomsNumbers).Returns(false);
		mock.Setup(x => x.IsCustomsCleared).Returns(true);
		NUnit.Framework.Assert.That(testMethod(messageStatusProvider,  mock.Object), Is.EqualTo(expectedValueOnAccepted), "!HasCustomsNumbers && IsCustomsCleared");

		mock.Setup(x => x.HasCustomsNumbers).Returns(false);
		mock.Setup(x => x.IsCustomsCleared).Returns(false);
		NUnit.Framework.Assert.That(testMethod(messageStatusProvider,  mock.Object), Is.EqualTo(!expectedValueOnAccepted), "!HasCustomsNumbers && !IsCustomsCleared");
	});
}
