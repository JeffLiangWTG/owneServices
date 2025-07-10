using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business
{
	public class BALMessageInterpreter : BaseInboundMessageInterpreter<BALProvider>
	{
		public BALMessageInterpreter(CustomsAndExciseReportInboundMessage message, BALProvider messageProvider) : base(message, messageProvider)
		{
		}

		protected override string Summary => CommonResStrings.BALMessageFriendlyName;

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (Res.GetString("DE71276A-5A74-4D86-887B-D23AC762DD04", "Total"), provider.Total);
			yield return (Res.GetString("124745AC-1736-4E0B-97DD-5EF730B2B5FD", "Cash"), provider.Cash);
			yield return (Res.GetString("98AD2CB2-3D78-4203-ABD5-951A1B44A156", "Deferred"), provider.Deferred);
		}
	}
}
