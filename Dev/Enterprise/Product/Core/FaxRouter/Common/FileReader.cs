using System;
using System.IO;

namespace Enterprise.FaxRouter
{
	public static class FileReader
	{
		public static byte[] ReadFile(String filePath)
		{
			using (FileStream fs = File.OpenRead(filePath))
			{
				byte[] result = new Byte[fs.Length];
				int bytesRead = 0;
				while (bytesRead < result.Length)
				{
					bytesRead += fs.Read(result, bytesRead, result.Length - bytesRead);
				}
				return result;
			}
		}
	}
}
