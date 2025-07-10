using System;
using System.IO;
using System.Text;
using CargoWise.Data.Testing;
using CargoWise.IO;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class BinaryStreamsTest : StreamsTest<ZBlob, Stream>
	{
		protected override SchemaColumn Column
		{
			get { return DummyBizoSchema.Z0_VarBinaryMax; }
		}

		protected override ZBlob FieldValue(DummyBusinessObject dummy)
		{
			return dummy.Z0_VarBinaryMax;
		}

		protected override void SetSource(DummyBusinessObject dummy, string filePath)
		{
			dummy.SetZ0_VarBinaryMaxSource(new FileStreamSource(filePath));
		}

		protected override Stream GetReader(DummyBusinessObject dummy)
		{
			return dummy.GetZ0_VarBinaryMaxReader();
		}

		protected override Stream OpenFile(string file)
		{
			return File.OpenRead(file);
		}

		protected override TempFile CreateTestFile(int size)
		{
			var random = new Random();
			var tempFile = TempFile.New();
			using (var stream = File.OpenWrite(tempFile.Filename))
			{
				for (int i = 0; i < size; i++)
				{
					unchecked
					{
						stream.WriteByte((byte)random.Next());
					}
				}
			}
			return tempFile;
		}

		public void TestChangeValue()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			using (var tempFile = CreateTestFile(3210))
			{
				SetSource(dummy, tempFile.Filename);
				Factory.Save();
			}
			using (var tempFile = CreateTestFile(3010))
			{
				SetSource(dummy, tempFile.Filename);
				Assert(dummy.HasChanges);
				Factory.Save();

				BusinessObjectFactory factory2 = new BusinessObjectFactory();
				dummy = factory2.Load<DummyBusinessObject>(dummy.PK);
				using (var dbStream = GetReader(dummy))
				using (var fileStream = OpenFile(tempFile.Filename))
				{
					Assert("Streams should contain the same data", dbStream.ContainsTheSameDataAs(fileStream));
				}
			}
		}
	}

	sealed class ImageStreamsTest : StreamsTest<ZBlob, Stream>
	{
		protected override SchemaColumn Column
		{
			get { return DummyBizoSchema.Z0_VarBinaryMax; }
		}

		protected override ZBlob FieldValue(DummyBusinessObject dummy)
		{
			return dummy.Z0_VarBinaryMax;
		}

		protected override void SetSource(DummyBusinessObject dummy, string filePath)
		{
			dummy.SetZ0_VarBinaryMaxSource(new FileStreamSource(filePath));
		}

		protected override Stream GetReader(DummyBusinessObject dummy)
		{
			return dummy.GetZ0_VarBinaryMaxReader();
		}

		protected override Stream OpenFile(string file)
		{
			return File.OpenRead(file);
		}

		protected override TempFile CreateTestFile(int size)
		{
			var random = new Random();
			var tempFile = TempFile.New();
			using (var stream = File.OpenWrite(tempFile.Filename))
			{
				for (int i = 0; i < size; i++)
				{
					unchecked
					{
						stream.WriteByte((byte)random.Next());
					}
				}
			}
			return tempFile;
		}
	}

	sealed class VarCharStreamsTest : StreamsTest<ZString, TextReader>
	{
		protected override SchemaColumn Column
		{
			get { return DummyBizoSchema.Z0_VarCharMax; }
		}

		protected override ZString FieldValue(DummyBusinessObject dummy)
		{
			return dummy.Z0_VarCharMax;
		}

		protected override void SetSource(DummyBusinessObject dummy, string filePath)
		{
			dummy.SetZ0_VarCharMaxSource(new FileTextReaderSource(filePath, Encoding.ASCII));
		}

		protected override TextReader GetReader(DummyBusinessObject dummy)
		{
			return dummy.GetZ0_VarCharMaxReader();
		}

		protected override TextReader OpenFile(string file)
		{
			return new StreamReader(File.OpenRead(file), Encoding.ASCII);
		}

		protected override TempFile CreateTestFile(int size)
		{
			char[] chars = new char[] { 'a', 'b', 'c', '1', '2', '3' };
			var tempFile = TempFile.New();
			using (var writer = new StreamWriter(File.OpenWrite(tempFile.Filename), Encoding.ASCII))
			{
				for (int i = 0; i < size; i++)
				{
					writer.Write(chars[i % chars.Length]);
				}
			}
			return tempFile;
		}
	}

	sealed class NVarCharStreamsTest : StreamsTest<ZString, TextReader>
	{
		protected override SchemaColumn Column
		{
			get { return DummyBizoSchema.Z0_NVarCharMax; }
		}

		protected override ZString FieldValue(DummyBusinessObject dummy)
		{
			return dummy.Z0_NVarCharMax;
		}

		protected override void SetSource(DummyBusinessObject dummy, string filePath)
		{
			dummy.SetZ0_NVarCharMaxSource(new FileTextReaderSource(filePath, Encoding.UTF8));
		}

		protected override TextReader GetReader(DummyBusinessObject dummy)
		{
			return dummy.GetZ0_NVarCharMaxReader();
		}

		protected override TextReader OpenFile(string file)
		{
			return new StreamReader(File.OpenRead(file), Encoding.UTF8);
		}

		protected override TempFile CreateTestFile(int size)
		{
			char[] chars = new char[] { 'a', 'b', 'c', 'd', '\x00E8', '\x0391', '\x0392', '\x0393', '1', '2', '3' };
			var tempFile = TempFile.New();
			using (var writer = new StreamWriter(File.OpenWrite(tempFile.Filename), Encoding.UTF8))
			{
				for (int i = 0; i < size; i++)
				{
					writer.Write(chars[i % chars.Length]);
				}
			}
			return tempFile;
		}

		public void TestClearReaderSource()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			using (var tempFile = CreateTestFile(500))
			{
				dummy.SetZ0_NVarCharMaxSource(new FileTextReaderSource(tempFile.Filename, Encoding.UTF8));
				dummy.Z0_NVarCharMax = ZString.Empty;
			}
			AssertEquals(ZString.Empty, dummy.Z0_NVarCharMax);
			Factory.Save();
			AssertEquals(ZString.Empty, dummy.Z0_NVarCharMax);
		}

		public void TestReaderSourceAfterSave()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.SetZ0_NVarCharMaxSource(new OnlyReadableOnceTextReaderSource());
			Factory.Save();
			using (var reader = dummy.GetZ0_NVarCharMaxReader())
			{
				AssertEquals("You can only read this once", reader.ReadToEnd());
			}
		}

		class OnlyReadableOnceTextReaderSource : ITextReaderSource
		{
			bool readOnce;
			public TextReader GetReader(bool closeUnderlyingStream = true)
			{
				if (readOnce)
				{
					throw new Exception("You can only read this once");
				}
				readOnce = true;
				return new StringReader("You can only read this once");
			}
		}
	}

	sealed class VarCharWithCloseStreamsTest : StreamsTest<ZString, TextReader>
	{
		protected override SchemaColumn Column
		{
			get { return DummyBizoSchema.Z0_VarCharMax; }
		}

		protected override ZString FieldValue(DummyBusinessObject dummy)
		{
			return dummy.Z0_VarCharMax;
		}

		protected override void SetSource(DummyBusinessObject dummy, string filePath)
		{
			dummy.SetZ0_VarCharMaxSource(new FileTextReaderSource(filePath, Encoding.ASCII));
		}

		protected override TextReader GetReader(DummyBusinessObject dummy)
		{
			return dummy.GetZ0_VarCharMaxReader(true);
		}

		protected override TextReader OpenFile(string file)
		{
			return new StreamReader(File.OpenRead(file), Encoding.ASCII);
		}

		protected override TempFile CreateTestFile(int size)
		{
			char[] chars = new char[] { 'a', 'b', 'c', '1', '2', '3' };
			var tempFile = TempFile.New();
			using (var writer = new StreamWriter(File.OpenWrite(tempFile.Filename), Encoding.ASCII))
			{
				for (int i = 0; i < size; i++)
				{
					writer.Write(chars[i % chars.Length]);
				}
			}
			return tempFile;
		}
	}

	sealed class TextStreamsTest : StreamsTest<ZString, TextReader>
	{
		protected override SchemaColumn Column
		{
			get { return DummyBizoSchema.Z0_VarCharMax; }
		}

		protected override ZString FieldValue(DummyBusinessObject dummy)
		{
			return dummy.Z0_VarCharMax;
		}

		protected override void SetSource(DummyBusinessObject dummy, string filePath)
		{
			dummy.SetZ0_VarCharMaxSource(new FileTextReaderSource(filePath, Encoding.ASCII));
		}

		protected override TextReader GetReader(DummyBusinessObject dummy)
		{
			return dummy.GetZ0_VarCharMaxReader();
		}

		protected override TextReader OpenFile(string file)
		{
			return new StreamReader(File.OpenRead(file), Encoding.ASCII);
		}

		protected override TempFile CreateTestFile(int size)
		{
			char[] chars = new char[] { 'a', 'b', 'c', '1', '2', '3' };
			var tempFile = TempFile.New();
			using (var writer = new StreamWriter(File.OpenWrite(tempFile.Filename), Encoding.ASCII))
			{
				for (int i = 0; i < size; i++)
				{
					writer.Write(chars[i % chars.Length]);
				}
			}
			return tempFile;
		}
	}

	sealed class NTextStreamsTest : StreamsTest<ZString, TextReader>
	{
		protected override SchemaColumn Column
		{
			get { return DummyBizoSchema.Z0_NVarCharMax; }
		}

		protected override ZString FieldValue(DummyBusinessObject dummy)
		{
			return dummy.Z0_NVarCharMax;
		}

		protected override void SetSource(DummyBusinessObject dummy, string filePath)
		{
			dummy.SetZ0_NVarCharMaxSource(new FileTextReaderSource(filePath, Encoding.UTF8));
		}

		protected override TextReader GetReader(DummyBusinessObject dummy)
		{
			return dummy.GetZ0_NVarCharMaxReader();
		}

		protected override TextReader OpenFile(string file)
		{
			return new StreamReader(File.OpenRead(file), Encoding.UTF8);
		}

		protected override TempFile CreateTestFile(int size)
		{
			char[] chars = new char[] { 'a', 'b', 'c', 'd', '\x00E8', '\x0391', '\x0392', '\x0393', '1', '2', '3' };
			var tempFile = TempFile.New();
			using (var writer = new StreamWriter(File.OpenWrite(tempFile.Filename), Encoding.UTF8))
			{
				for (int i = 0; i < size; i++)
				{
					writer.Write(chars[i % chars.Length]);
				}
			}
			return tempFile;
		}
	}

	abstract class StreamsTest<ZType, ReaderType> : TestCaseWithFactory
		where ZType : IZType
		where ReaderType : IDisposable
	{
		protected abstract SchemaColumn Column { get; }
		protected abstract ZType FieldValue(DummyBusinessObject dummy);
		protected abstract void SetSource(DummyBusinessObject dummy, string file);
		protected abstract ReaderType GetReader(DummyBusinessObject dummy);
		protected abstract ReaderType OpenFile(string file);
		protected abstract TempFile CreateTestFile(int size);

		public void TestEmptyValue()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			Factory.Save();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			dummy = factory2.Load<DummyBusinessObject>(dummy.PK);
			Assert(!LazyLoading.LoadRequired(dummy.Row[Column.Name]));
			Assert(FieldValue(dummy).IsEmpty);
		}

		public void TestStreamWriteAndRead4Bytes()
		{
			TestStreamWriteAndRead(4, false);
		}

		public void TestStreamWriteAndRead40Bytes()
		{
			TestStreamWriteAndRead(40, false);
		}

		public void TestStreamWriteAndRead4K()
		{
			TestStreamWriteAndRead(4444, true);
		}

		public void TestStreamWriteAndRead444K()
		{
			TestStreamWriteAndRead(444444, true);
		}

		[SnailTest, RequiresLargeLogFile]
		public void TestStreamWriteAndRead111M()
		{
			TestStreamWriteAndRead(111111111, true);
		}

		protected void TestStreamWriteAndRead(int size, bool shouldLazyLoad)
		{
			var dummy = Factory.New<DummyBusinessObject>();
			using (var tempFile = CreateTestFile(size))
			{
				SetSource(dummy, tempFile.Filename);
				Factory.Save();

				BusinessObjectFactory factory2 = new BusinessObjectFactory();
				dummy = factory2.Load<DummyBusinessObject>(dummy.PK);
				AssertEquals(shouldLazyLoad, dummy.BlobFieldsNeedLoadingExposedForTest(Column));
				using (var dbStream = GetReader(dummy))
				using (var fileStream = OpenFile(tempFile.Filename))
				{
					if (dbStream is Stream)
					{
						Assert("Streams should contain the same data", (dbStream as Stream).ContainsTheSameDataAs((fileStream as Stream)));
					}
					else
					{
						Assert("Readers should contain the same data", (dbStream as TextReader).ContainsTheSameDataAs((fileStream as TextReader)));
					}
				}
			}
		}
	}
}
