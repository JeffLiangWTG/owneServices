using System.IO;

namespace CargoWise.BuildTools
{
	public static class CurrentDatDbBackupPrefix
	{
		public static string GetValue()
		{
			using (var stream = typeof(CurrentDatDbBackupPrefix).Assembly.GetManifestResourceStream("CargoWise.BuildTools.CurrentDatDbBackupPrefix.txt"))
			using (var reader = new StreamReader(stream))
			{
				return reader.ReadToEnd();
			}
		}
	}
}
