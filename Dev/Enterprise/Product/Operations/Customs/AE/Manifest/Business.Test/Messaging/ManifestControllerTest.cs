using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

sealed class ManifestControllerTest : TestCaseWithFactory
{
	[ExpectNoExceptions]
	public void TestInterchangeSegmentProvider()
		=> NUnit.Framework.Assert.That(new ManifestController().InterchangeSegmentProvider, NUnit.Framework.Is.TypeOf<ManifestInterchangeSegmentProvider>());

	[ExpectNoExceptions]
	public void TestMessageAttacheeProvider()
		=> NUnit.Framework.Assert.That(new ManifestController().MessageAttacheeProvider, NUnit.Framework.Is.TypeOf<ManifestMessageBillProvider>());
}
