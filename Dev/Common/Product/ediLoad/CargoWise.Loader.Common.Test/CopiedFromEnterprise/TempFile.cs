using System;
using System.IO;
using CargoWise.Common;
using NUnit.Framework;

namespace CargoWise.Loader.Common
{
	public sealed class TempFile : IDisposable
	{
		readonly string fileName;

		TempFile(string fileName)
		{
			if (string.IsNullOrEmpty(fileName))
			{
				throw new ArgumentException("Value cannot be null or empty.", nameof(fileName));
			}

			this.fileName = fileName;
		}

		public string FileName
		{
			get { return fileName; }
		}

		public void Dispose()
		{
			if (File.Exists(FileName))
			{
				File.SetAttributes(FileName, FileAttributes.Normal);
				File.Delete(FileName);
			}
		}

		public static TempFile New()
		{
			string tempFileName = TempForTest.GetTempFileName();
			return new TempFile(tempFileName);
		}

		public override string ToString()
		{
			return FileName;
		}

		public static implicit operator string(TempFile tempFile)
		{
			Argument.NotNull(tempFile, nameof(tempFile));
			return tempFile.ToString();
		}
	}
}

#region Test

namespace CargoWise.Loader.Common.Testing
{
	using NUnit.Framework;
	class TempFileTest : TestCase
	{
		public void TestFileCreatedAndDeleted()
		{
			string fileName;
			using (TempFile temp = TempFile.New())
			{
				fileName = temp;
				AssertEquals(fileName, temp.FileName);
				Assert(File.Exists(temp));
				File.SetAttributes(temp.FileName, FileAttributes.ReadOnly);
			}
			Assert(!File.Exists(fileName));
		}

		public void TestFileDeletedBeforeDispose()
		{
			using (TempFile tempFile = TempFile.New())
			{
				File.Delete(tempFile.FileName);
				Assert("File no longer exists, and dispose should not cause a problem.", !File.Exists(tempFile.FileName));
			}
		}
	}
}

#endregion