using System.Collections;
using System.IO;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DataTransfer.IO.Testing
{
	sealed class FileLineIteratorTest : TestCase
	{
		public void TestFile()
		{
			using (TempFile file = TempFile.New())
			{
				using (StreamWriter writer = new StreamWriter(file.Filename))
				{
					writer.WriteLine("LINE1");
					writer.WriteLine("LINE2");
					writer.Flush();
				}

				using (FileLineIterator read = new FileLineIterator(file.Filename))
				{
					ArrayList lines = new ArrayList();
					foreach (string line in read)
					{
						lines.Add(line);
					}

					AssertEquals("LINE1", lines[0]);
					AssertEquals("LINE2", lines[1]);
				}
			}
		}

		public void TestStream()
		{
			using (Stream file = new MemoryStream())
			{
				StreamWriter writer = new StreamWriter(file);
				writer.WriteLine("LINE1");
				writer.WriteLine("LINE2");
				writer.Flush();

				file.Position = 0;

				FileLineIterator read = new FileLineIterator(file);

				ArrayList lines = new ArrayList();
				foreach (string line in read)
				{
					lines.Add(line);
				}

				AssertEquals("LINE1", lines[0]);
				AssertEquals("LINE2", lines[1]);
			}
		}
	}
}
