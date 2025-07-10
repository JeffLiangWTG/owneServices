using System;
using Enterprise.AlwaysOn.Setup;
using NUnit.Framework;

namespace Enterprise.AlwaysOn.Testing
{
	class DatabaseFileTest : TestCase
	{
		public void TestAttributes()
		{
			var dbFile = new DatabaseFile("DataFile01", "D:\\Physical\\Name.mdf", "D");
			AssertEquals("LogicalName", "DataFile01", dbFile.LogicalName);
			AssertEquals("PhysicalName", "D:\\Physical\\Name.mdf", dbFile.PhysicalName);
			AssertEquals("Type", DatabaseFile.FileType.Data, dbFile.Type);
		}

		public void TestConstructWithInvalidFileType()
		{
#if NETFRAMEWORK
			var expectedMessage = "Invalid file type [Q]. Valid values are [D, L].\r\nParameter name: type";
#else
			var expectedMessage = "Invalid file type [Q]. Valid values are [D, L]. (Parameter 'type')";
#endif

			AssertExceptionThrown(
				typeof(ArgumentException),
				expectedMessage,
				() => new DatabaseFile("name", "path", "Q"));
		}
	}
}
