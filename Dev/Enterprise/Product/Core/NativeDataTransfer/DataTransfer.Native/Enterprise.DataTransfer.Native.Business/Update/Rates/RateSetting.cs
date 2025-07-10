using System.Collections.Generic;
using Enterprise.DataTransfer.Native.Business.Requests;
using Enterprise.DataTransfer.Native.Common.Interceptors;

namespace Enterprise.DataTransfer.Native.Business.Update.Rates
{
	public class RateSetting : BaseInterceptorSetting
	{
		public RateSetting(HeaderData source)
		{
			Source = source;
		}

		public HeaderData Source;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "IEnumerable string")]
		public override IEnumerable<string> EnableList
		{
			get { return new[] { "Rate" }; }
		}

		public override IEnumerable<string> DisableList
		{
			get { return System.Array.Empty<string>(); }
		}
	}
}
