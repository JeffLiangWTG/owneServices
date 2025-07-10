using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS
{
	class ResponseParameters
	{
		public ResponseParameters() { }

		public ZDateTimeOffset EventDateTime { get; set; } = ZDateTimeOffset.Now;
		public List<KeyValuePair<string, string>> EventParameters { get; } = new List<KeyValuePair<string, string>>();
	}
}
