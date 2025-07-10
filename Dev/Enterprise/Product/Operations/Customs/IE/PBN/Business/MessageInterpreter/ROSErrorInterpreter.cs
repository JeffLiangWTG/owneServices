using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.PBN.Messaging;

namespace Enterprise.Customs.IE.PBN.Business;

public class ROSErrorInterpreter : InboundMessageInterpreter<ROSErrorProvider>
{
	public ROSErrorInterpreter(Enterprise.Messaging.Business.EDIMessage message, ROSErrorProvider provider) : base(message, provider) { }

	protected override string Summary => Res.GetString("CF7A6F1A-50DC-4CD3-BD3E-B77E733B1B44", "ROS Error");

	protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
	{
		var rosError = provider.ValidationErrors.Single();

		yield return (Res.GetString("D3CD3BF2-131D-433D-817D-3CEA48B60270", "Code"), rosError.code);
		yield return (Res.GetString("22B143DC-002A-465E-9009-AFC9E9519298", "Description"), rosError.description);
	}
}
