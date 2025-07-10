using System.Collections.Generic;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.PBN.Messaging;

namespace Enterprise.Customs.IE.PBN.Business
{
	public class CreateAndUpdatePBNMessageInterpreter : InboundMessageInterpreter<CreateAndUpdatePBNProvider>
	{
		public CreateAndUpdatePBNMessageInterpreter(PBNInboundEDIMessage message, CreateAndUpdatePBNProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"E2DC6061-05F9-4878-9EAE-A16A66A2CC65",
			"A {0} message which contains 'Manifest Job Number' has been received for Request Report {1}.",
			message.MessageTypeWithDescription, provider.PbnID.ToString());

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (Res.GetString("A1B2C3D4-E5F6-7890-1234-56789ABCDEF8", "PBN ID"), provider.PbnID.ToString());
			yield return (Res.GetString("A1B2C3D4-E5F6-7890-1234-56789ABCDEF9", "Status"), provider.Status.ToString());
			yield return (Res.GetString("8CAF15BE-6EAA-4135-8D9C-8F4AC04275AB", "Issue"), provider.Issue.ToString());
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails()
		{
			if (provider.listValidationErrorProviders != null && provider.listValidationErrorProviders.Count > 0)
			{
				bool isSummaryAdded = false;
				foreach (var error in provider.listValidationErrorProviders)
				{
					var summary = "";
					if (!isSummaryAdded)
					{
						summary = Res.GetString("ValidationErrors", "Validation Errors");
						isSummaryAdded = true;
					}
					yield return (summary,
						new (string, string)[]
						{
							(Res.GetString("259394F5-7608-40A7-AE6F-3844524DDBA9", "Validation Error Code"), error.code.ToString()),
							(Res.GetString("FD6EB73A-0DD0-4086-BC48-2F39967126AC", "Validation Error Path"), error.path.ToString()),
							(Res.GetString("5DA31EE2-1395-47B0-ABF2-2C077D178FFE", "Validation Error Description"), error.description.ToString())
						});
				}
			}
		}
	}
}
