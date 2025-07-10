using System.Text;

namespace Enterprise.Messaging.Integration
{
	public static class MessageEncoding
	{
		public static Encoding UTF8WithoutBOM
		{
			get { return new UTF8Encoding(false); }
		}
	}
}
