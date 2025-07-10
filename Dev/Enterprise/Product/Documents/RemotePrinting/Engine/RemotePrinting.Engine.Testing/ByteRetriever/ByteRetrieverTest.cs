using System.IO;

namespace Enterprise.RemotePrinting.Engine.Testing
{
	using NUnit.Framework;

	public class ByteRetrieverTest : TestCase
	{
		public void TestGetFileAsBytes()
		{
			string tempFile = string.Empty;
			try
			{
				byte[] bytesToSave = new byte[] { 1, 2, 3, 4, 5 };
				tempFile = TempForTest.GetTempFileName();

				using (FileStream stream = File.OpenWrite(tempFile))
				{
					stream.Write(bytesToSave, 0, bytesToSave.Length);
				}

				byte[] retrievedBytes = ByteRetriever.GetFileAsBytes(tempFile);

				AssertEquals(bytesToSave, retrievedBytes);
			}
			finally
			{
				DeleteIfExists(tempFile);
			}
		}
	}
}
