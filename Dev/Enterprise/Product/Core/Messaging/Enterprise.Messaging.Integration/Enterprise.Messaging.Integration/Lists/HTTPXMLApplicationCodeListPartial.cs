using System.Collections.Generic;

namespace Enterprise.Messaging.Integration
{
	partial class HTTPXMLApplicationCodeList
	{
		public static IEnumerable<string> GetCodes()
		{
			return new List<string>
			{
				Codes.NDQ,
				Codes.UDQ
			};
		}
	}
}
