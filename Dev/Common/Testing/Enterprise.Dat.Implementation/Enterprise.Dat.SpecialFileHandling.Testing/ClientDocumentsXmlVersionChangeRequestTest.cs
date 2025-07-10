using Dat.Integration.VersionControl;
using Moq;
using NUnit.Framework;

namespace Enterprise.Dat.SpecialFileHandling.Testing
{
	internal class ClientDocumentsXmlVersionChangeRequestTest : TestCase
	{
		public void TestDeletionChangeType()
		{
			var fakePath = "Enterprise/ClientExtensions/_UAConstantsGenerator_/Documents/fakeFile_Documents.xml";
			var change = new MockPendingChange(TfsChangeType.Delete, fakePath);
			var workspace = new Mock<IWorkspaceAccess>(MockBehavior.Strict);
			var changeRequest = new ClientDocumentsXmlVersionChangeRequest();
			AssertNoExceptionThrown(() => changeRequest.UpdateForDATCheckin(workspace.Object, change));
		}
	}
}
