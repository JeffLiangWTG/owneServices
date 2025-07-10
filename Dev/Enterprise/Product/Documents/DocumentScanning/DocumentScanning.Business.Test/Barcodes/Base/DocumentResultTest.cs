using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	[TestedType(typeof(DocumentResult))]
	public class DocumentResultTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new DocumentResult(new DocumentFactoryProvider().GetFactory(Factory));
		}

		public void TestFilePath()
		{
			string filename = Path.Combine(Env.TempPath, "blah.txt");
			DocumentResult result = new DocumentResult(MasterFactory, filename);
			AssertEquals(filename, result.FilePath);
		}

		public void TestMasterFactory()
		{
			DocumentResult result = new DocumentResult(MasterFactory);
			AssertNotNull("Master factory shouldn't be null", result.MasterFactory);
			AssertEquals("Master factory should be the same instance passed in", MasterFactory, result.MasterFactory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			MasterFactory = new DocumentFactoryProvider().GetFactory(Factory);
		}

		DocumentFactory MasterFactory;
	}
}
