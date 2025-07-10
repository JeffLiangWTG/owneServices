using System;
using System.IO;
using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class FlatFileConverterTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentNullException))]
		public void TestImportArguments_NullValueObject()
		{
			var converter = new TestFlatFileConverter();
			converter.ImportFlatFile(null, new CsvFlatFileFormat(), new TestTextReader());
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestImportArguments_NullFlatFileFormat()
		{
			var converter = new TestFlatFileConverter();
			converter.ImportFlatFile(new TestValueObject(), null, new TestTextReader());
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestImportArguments_NullTextReader()
		{
			var converter = new TestFlatFileConverter();
			converter.ImportFlatFile(new TestValueObject(), new CsvFlatFileFormat(), null);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestExportArguments_NullTextWriter()
		{
			var converter = new TestFlatFileConverter();
			converter.ExportFlatFile(new TestValueObject(), new CsvFlatFileFormat(), null);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestExportArguments_NullFlatFileFormat()
		{
			var converter = new TestFlatFileConverter();
			converter.ExportFlatFile(new TestValueObject(), null, new TestTextWriter());
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestExportArguments_NullvalueObject()
		{
			var converter = new TestFlatFileConverter();
			converter.ExportFlatFile(null, new CsvFlatFileFormat(), new TestTextWriter());
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestInterfaceCast_Export()
		{
			IFlatFileConverter converter = new TestFlatFileConverter();
			converter.ExportFlatFile(new AnotherTestValueObject(), new CsvFlatFileFormat(), null);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestInterfaceCast_Import()
		{
			IFlatFileConverter converter = new TestFlatFileConverter();
			converter.ImportFlatFile(new AnotherTestValueObject(), new CsvFlatFileFormat(), null);
		}

		public void TestProcessOnEOF()
		{
			var tempFileLocation = Temp.GetTempFileName();
			try
			{
				using (TextWriter writer = new StreamWriter(tempFileLocation))
				{
					var exportConverter = new MockFlatFileConverter(new NotificationBuffer(), Factory);
					ITransferDataConsumer consumer = exportConverter;
					consumer.DataCollector.EOF = true;
					consumer.DataCollector.BOF = false;

					var simpleValueObject = new SimpleIValueObject();
					simpleValueObject.Value1 = "HelloHelloAsl";
					simpleValueObject.Value2 = "This row!";

					exportConverter.ExportFlatFile(simpleValueObject, new CsvFlatFileFormat(), writer);
				}

				using (var reader = new StreamReader(tempFileLocation))
				{
					var flatFileRow0 = reader.ReadLine();
					var flatFileRow1 = reader.ReadLine();
					AssertEquals("Test Row", "\"ROFLROFL\",\"This row!\"", flatFileRow0);
					AssertEquals("Footer", "\"ROFLROFL\",\"FOOTER\"", flatFileRow1);
				}
			}
			finally
			{
				File.Delete(tempFileLocation);
			}
		}

		public void TestProcessOnBOF()
		{
			var tempFileLocation = Temp.GetTempFileName();
			try
			{
				using (TextWriter writer = new StreamWriter(tempFileLocation))
				{
					var exportConverter = new MockFlatFileConverter(new NotificationBuffer(), Factory);
					var consumer = (ITransferDataConsumer)exportConverter;
					consumer.DataCollector.EOF = false;
					consumer.DataCollector.BOF = true;

					var simpleValueObject = new SimpleIValueObject();
					simpleValueObject.Value1 = "HelloHelloAsl";
					simpleValueObject.Value2 = "This row!";

					exportConverter.ExportFlatFile(simpleValueObject, new CsvFlatFileFormat(), writer);
				}

				using (TextReader reader = new StreamReader(tempFileLocation))
				{
					var flatFileRow0 = reader.ReadLine();
					var flatFileRow1 = reader.ReadLine();
					AssertEquals("Header", "\"ROFLROFL\",\"HEADER\"", flatFileRow0);
					AssertEquals("Test Row", "\"ROFLROFL\",\"This row!\"", flatFileRow1);
				}
			}
			finally
			{
				File.Delete(tempFileLocation);
			}
		}

		public void TestITransferDataConsumer()
		{
			ITransferDataConsumer converter = new MockFlatFileConverter(new NotificationBuffer(), Factory);

			AssertNotNull(converter.DataCollector);

			AssertEquals("Default value of UnitsProcessed is incorrect", 0, converter.DataCollector.UnitsProcessed);
			converter.DataCollector.UnitsProcessed = 500;
			AssertEquals("Assigned value was not returned", 500, converter.DataCollector.UnitsProcessed);

			converter.ResetDataCollector();
			Assert("Reset Failed", converter.DataCollector.UnitsProcessed != 500);
		}

		public void TestExportFlatFile()
		{
			var tempFileLocation = Temp.GetTempFileName();
			try
			{
				using (TextWriter writer = new StreamWriter(tempFileLocation))
				{
					var exportConverter = new MockFlatFileConverter(new NotificationBuffer(), Factory);

					var simpleValueObject = new SimpleIValueObject();
					simpleValueObject.Value1 = "HelloHelloAsl";
					simpleValueObject.Value2 = "This row!";

					exportConverter.ExportFlatFile(simpleValueObject, new CsvFlatFileFormat(), writer);
				}

				using (TextReader reader = new StreamReader(tempFileLocation))
				{
					var flatFileRow = reader.ReadLine();
					AssertEquals("Test Row", "\"ROFLROFL\",\"This row!\"", flatFileRow);
				}
			}
			finally
			{
				File.Delete(tempFileLocation);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportFlatFile()
		{
			using (var reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Core\DataTransfer\DataTransfer.Test\FlatFile\TestFiles\FlatFileConverterImportTestFile.txt"))
			{
				var simpleValueObject = new SimpleIValueObject();
				var importConverter = new MockFlatFileConverter(new NotificationBuffer(), Factory);
				importConverter.ImportFlatFile(simpleValueObject, new CsvFlatFileFormat(), reader);

				AssertEquals("hello", simpleValueObject.ValueCollection1[0]);
				AssertEquals("very", simpleValueObject.ValueCollection1[1]);
				AssertEquals("and", simpleValueObject.ValueCollection1[2]);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportFlatFileWithNumberOfBytesToRead()
		{
			using (var reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Core\DataTransfer\DataTransfer.Test\FlatFile\TestFiles\FlatFileConverterImportTestFileWithNoNewLines.txt"))
			{
				var simpleValueObject = new SimpleIValueObject();
				var importConverter = new MockFlatFileConverterReadTwentyBytes(new NotificationBuffer(), Factory);
				importConverter.ImportFlatFile(simpleValueObject, new CsvFlatFileFormat(), reader);

				AssertEquals("hello", simpleValueObject.ValueCollection1[0]);
				AssertEquals("you?", simpleValueObject.ValueCollection1[1]);
				AssertEquals("", simpleValueObject.ValueCollection1[2]);
				AssertEquals(",", simpleValueObject.ValueCollection1[3]);
				AssertEquals("", simpleValueObject.ValueCollection1[4]);
				AssertEquals("d\"", simpleValueObject.ValueCollection1[5]);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportFlatFileWithNumberOfBytesToReadAsZero()
		{
			using (var reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Core\DataTransfer\DataTransfer.Test\FlatFile\TestFiles\FlatFileConverterImportTestFile.txt"))
			{
				var simpleValueObject = new SimpleIValueObject();
				var importConverter = new MockFlatFileConverterReadZeroBytes(new NotificationBuffer(), Factory);
				importConverter.ImportFlatFile(simpleValueObject, new CsvFlatFileFormat(), reader);

				AssertEquals("hello", simpleValueObject.ValueCollection1[0]);
				AssertEquals("very", simpleValueObject.ValueCollection1[1]);
				AssertEquals("and", simpleValueObject.ValueCollection1[2]);
			}
		}

		sealed class TestTextReader : TextReader
		{
		}

		sealed class TestTextWriter : TextWriter
		{
			public override Encoding Encoding => throw new Exception("The method or operation is not implemented.");
		}

		sealed class TestFlatFileConverter : FlatFileConverter<TestValueObject>
		{
			public TestFlatFileConverter()
				: base(null, null)
			{
			}
		}

		sealed class TestValueObject : IValueObject
		{
			bool IValueObject.IsSpecified => throw new Exception("The method or operation is not implemented.");

			bool IValueObject.ShouldCreateElementForEmptyValue
			{
				get => throw new Exception("The method or operation is not implemented.");
				set => throw new Exception("The method or operation is not implemented.");
			}
		}

		sealed class AnotherTestValueObject : IValueObject
		{
			bool IValueObject.IsSpecified => throw new Exception("The method or operation is not implemented.");

			bool IValueObject.ShouldCreateElementForEmptyValue
			{
				get => throw new Exception("The method or operation is not implemented.");
				set => throw new Exception("The method or operation is not implemented.");
			}
		}
	}
}
