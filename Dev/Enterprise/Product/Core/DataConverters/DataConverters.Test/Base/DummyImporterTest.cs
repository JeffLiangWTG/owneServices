using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DataConverters.Testing.Base
{
	[TestedType(typeof(DummyImporter))]
	sealed internal class DummyImporterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new DummyImporter(Factory);
		}

		public void TestSetDefaults()
		{
			var importer = new DummyImporter(Factory, "Location");
			AssertEquals("Cyber2", importer.ServerName);
			AssertEquals("SYSDBA", importer.UserId);
			AssertEquals("masterkey", importer.Password);
			AssertEquals("Location", importer.DataLocation);
			AssertEquals(ZString.Empty, importer.ConnectionText);
		}

		public void TestSetConnectionString()
		{
			var importer = new DummyImporter(Factory, "Location");
			importer.ServerName = "Cyber2";
			importer.UserId = "userid";
			importer.Password = "password";
			ZString expectedString = @"Driver={INTERSOLV InterBase ODBC Driver (*.gdb)};Server=Cyber2;Database=Location;Uid=userid;Pwd=password";
			importer.SetConnectionText();
			AssertEquals(expectedString, importer.ConnectionText);
		}
	}
}
