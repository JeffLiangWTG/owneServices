using MimeKit;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1171
	{
		public void BadCode()
		{
			var message = new MimeMessage();
			// CW1171 Do Not Use MimeMessage.ToString(), instead use Encoding.UTF8.GetString(MimeMessage.GetData())
			message.ToString();
		}
	}
}
