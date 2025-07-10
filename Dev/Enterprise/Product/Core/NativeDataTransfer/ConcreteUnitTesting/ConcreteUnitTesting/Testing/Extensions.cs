using System.IO;
using System.Text;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting
{
	internal static class Extensions
	{
		public static string ImportNativeXmlReturningLog(this string incomingXmlText)
		{
			string actualLog;
			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(incomingXmlText)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);
				actualLog = manager.GetLogs();
			}
			return actualLog;
		}
	}
}
