using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(Receptacle.Loader))]
	sealed class ReceptacleLoaderTest : LoaderTestCase
	{
		[ExpectNoExceptions]
		public void TestLoad()
		{
			var receptacle = loader.LoadOrCreate(manifestHeader, 1);
			AssertEquals(receptacle, loader.Load(manifestHeader, 1));
		}

		[ExpectNoExceptions]
		public void TestLoadOrCreate()
		{
			var code = loader.LoadOrCreate(manifestHeader, 1);
			AssertNotEquals("Code must be created - should not be [null]", code, default(Receptacle));
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var manifestViaNewFactory = newFactory.Load<AsycudaManifestHeader>(manifestHeader.PK);
			var receptacleViaNewFactory = loader.LoadOrCreate(manifestViaNewFactory, 1);
			AssertEquals("Code must be loaded", receptacleViaNewFactory.PK, code.PK);
		}

		protected override void SetUp()
		{
			base.SetUp();
			manifestHeader = Factory.New<AsycudaManifestHeader>();

			loader = new Receptacle.Loader(Factory);
		}
		AsycudaManifestHeader manifestHeader;
		Receptacle.Loader loader;

		protected override BusinessObject.Loader GetNewLoaderToTest() => loader;
	}
}
