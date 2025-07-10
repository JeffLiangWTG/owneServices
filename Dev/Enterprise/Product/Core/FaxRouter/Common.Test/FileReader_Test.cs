using System.IO;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.FaxRouter
{
	sealed class FileReader_Test : TestCase
	{
		public void TestReadFile()
		{
			using (TempFile temp = TempFile.New())
			{
				using (FileStream tempStream = File.Create(temp.Filename))
				{
					tempStream.WriteByte(42);
					tempStream.WriteByte(69);
					tempStream.WriteByte(96);
					tempStream.WriteByte(7);
				}

				byte[] actual = FileReader.ReadFile(temp.Filename);
				AssertEquals((byte)42, actual[0]);
				AssertEquals((byte)69, actual[1]);
				AssertEquals((byte)96, actual[2]);
				AssertEquals((byte)7, actual[3]);
			}
		}
	}
}
