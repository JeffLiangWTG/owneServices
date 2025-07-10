using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using CargoWise.Data;
using CargoWise.IO;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class DataFileTransactionedTest : TransactionedTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWriteXml()
		{
			var sqlText = @"
				CREATE TABLE TestWriteXml_0374321AEBEF40878A6A693B5458AE3B (Id uniqueidentifier not null, CreateDate datetime)
				INSERT TestWriteXml_0374321AEBEF40878A6A693B5458AE3B VALUES (newid(), '2007-07-26 10:00:00')";
			Db.Connection.ExecuteNonQuery(sqlText);

			var dataFile = new ConcreteDataFile("", "TestWriteXml_0374321AEBEF40878A6A693B5458AE3B");
			var data = dataFile.LoadDataFromDatabase();
			string xmlContents = null;

			using (var tempDir = new TempDirectory())
			{
				var tempFilePath = Path.Combine(tempDir.DirectoryName, "TestWriteXml.xml");
				dataFile.WriteXml(data, tempFilePath, XmlWriteMode.WriteSchema);
				xmlContents = File.ReadAllText(tempFilePath);
			}

			AssertEquals("[XML Contents] CreateDate schema declared as unspecified mode\r\n" + xmlContents, true, Regex.IsMatch(xmlContents, @"name\s*=\s*""CreateDate""[^>]+DateTimeMode=""Unspecified"""));
			AssertEquals("[XML Contents] CreateDate date time value without time zone found\r\n" + xmlContents, true, Regex.IsMatch(xmlContents, "<CreateDate>2007-07-26T10:00:00<"));
			AssertEquals("[XML Contents] CreateDate date time value with time zone found\r\n" + xmlContents, false, Regex.IsMatch(xmlContents, "<CreateDate>2007-07-26T10:00:00[+]<"));
		}
	}
}
