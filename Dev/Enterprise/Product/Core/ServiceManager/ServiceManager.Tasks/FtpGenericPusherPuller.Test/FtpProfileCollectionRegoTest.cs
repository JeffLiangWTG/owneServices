using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Tasks.FTP.Testing
{
	[TestedType(typeof(FtpProfileCollection))]
	sealed class FtpProfileCollectionRegoTest : RegistryBusinessObjectCollectionTemplateTestCase<FtpProfileCollection>
	{
		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override FtpProfileCollection GetCollectionToTest()
		{
			return new FtpProfileCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new FtpProfile(Factory);
		}
	}
}
