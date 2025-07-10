using System.IO;
using System.Reflection;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	public static class CARMStatementOfAccountMessageTestHelper
	{
		public static ZString GetCARMStatementOfAccount_LEMessageText()
		{
			var text = ZString.Empty;
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.Customs.CA.Business.Test.MessageProcessors.ResponseMessageProcessors.CACustoms.TestFiles.CARMStatementOfAccount_LE.txt"))
			using (var sr = new StreamReader(stream))
			{
				text = sr.ReadToEnd();
			}
			return text;
		}

		public static ZString GetCARMStatementOfAccount_PTMessageText()
		{
			var text = ZString.Empty;
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.Customs.CA.Business.Test.MessageProcessors.ResponseMessageProcessors.CACustoms.TestFiles.CARMStatementOfAccount_PT.txt"))
			using (var sr = new StreamReader(stream))
			{
				text = sr.ReadToEnd();
			}
			return text;
		}

		public static ZString GetCARMStatementOfAccount_PAMessageText()
		{
			var text = ZString.Empty;
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.Customs.CA.Business.Test.MessageProcessors.ResponseMessageProcessors.CACustoms.TestFiles.CARMStatementOfAccount_PA.txt"))
			using (var sr = new StreamReader(stream))
			{
				text = sr.ReadToEnd();
			}
			return text;
		}
	}
}
