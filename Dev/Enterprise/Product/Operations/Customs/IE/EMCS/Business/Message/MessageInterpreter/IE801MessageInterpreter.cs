using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.EMCS.Messaging;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public sealed class IE801MessageInterpreter : InboundMessageInterpreter<IIE801>
	{
		public IE801MessageInterpreter(EMCSInboundEDIMessage message, IIE801 provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("94C68423-A391-4D7B-83F8-007B12C3F999", "Electronic Administrative Document received.");

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails() => Enumerable.Empty<(string, string)>();

		protected override IEnumerable<string> GetDescriptions()
		{
			yield return $"e-AD Number : {provider.MrnNumber} ";
		}
	}
}
