using System.IO;
using System.Reflection;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	public static class CARMDailyNoticeMessageTestHelper
	{
		public static ZString GetCARMDailyNoticeMessageText()
		{
			var text = ZString.Empty;
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.Customs.CA.Business.Test.MessageProcessors.ResponseMessageProcessors.CACustoms.TestFiles.CARMDailyNotice.txt"))
			using (var sr = new StreamReader(stream))
			{
				text = sr.ReadToEnd();
			}
			return text;
		}

		public static ZString GetCARMDailyNoticeImporterMessageText()
		{
			var text = ZString.Empty;
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.Customs.CA.Business.Test.MessageProcessors.ResponseMessageProcessors.CACustoms.TestFiles.CARMDailyNoticeImporter.txt"))
			using (var sr = new StreamReader(stream))
			{
				text = sr.ReadToEnd();
			}
			return text;
		}

		public static ZString GetCARMDailyNoticeBrokerMessageText()
		{
			var text = ZString.Empty;
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.Customs.CA.Business.Test.MessageProcessors.ResponseMessageProcessors.CACustoms.TestFiles.CARMDailyNoticeBroker.txt"))
			using (var sr = new StreamReader(stream))
			{
				text = sr.ReadToEnd();
			}
			return text;
		}

		public static ZString GetCARMDailyNoticeBrokerMessageTextWithPartyAccountElementIsNotPresent()
		{
			var text = ZString.Empty;
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.Customs.CA.Business.Test.MessageProcessors.ResponseMessageProcessors.CACustoms.TestFiles.CARMDailyNoticeBrokerWithPartyAccountElementIsNotPresent.txt"))
			using (var sr = new StreamReader(stream))
			{
				text = sr.ReadToEnd();
			}
			return text;
		}
	}
}
