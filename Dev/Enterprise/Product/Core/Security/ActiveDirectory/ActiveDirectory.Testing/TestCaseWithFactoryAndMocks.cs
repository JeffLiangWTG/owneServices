using CargoWise.ActiveDirectory;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Security.ActiveDirectory.Test
{
	public class TestCaseWithFactoryAndMocks : TestCaseWithFactory
	{
		protected ADTestHelper Helper
		{
			get { return helper ?? (helper = new ADTestHelper(Factory)); }
		}
		ADTestHelper helper;

		protected ADEntityProviderForTest ADEntityProviderSubstitution
		{
			get { return adEntityProviderSubstitution ?? (adEntityProviderSubstitution = new ADEntityProviderForTest()); }
		}
		ADEntityProviderForTest adEntityProviderSubstitution;

		protected DirectorySearcherProviderForTest DirectorySearcherProviderSubstitution => directorySearcherProviderSubstitution ?? (directorySearcherProviderSubstitution = new DirectorySearcherProviderForTest());
		DirectorySearcherProviderForTest directorySearcherProviderSubstitution;

		protected override void SetUp()
		{
			base.SetUp();
			ObjectFactory.Substitute<IDirectorySearcherProvider>(DirectorySearcherProviderSubstitution);
			ObjectFactory.Substitute<IADEntityProvider>(ADEntityProviderSubstitution);

			directorySearcherMock = new Mock<IDirectorySearcher>();
			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = directorySearcherMock.Object;
		}

		protected Mock<IDirectorySearcher> directorySearcherMock;
	}
}
