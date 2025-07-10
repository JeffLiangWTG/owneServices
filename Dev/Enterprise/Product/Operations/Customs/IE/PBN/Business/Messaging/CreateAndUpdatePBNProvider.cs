using System.Collections.Generic;
using CargoWise.Customs.IE.MessageDefinitions.PBN;
using CargoWise.Types;

namespace Enterprise.Customs.IE.PBN.Messaging;

public class CreateAndUpdatePBNProvider
{
	public CreateAndUpdatePBNProvider(CreateAndUpdatePBNMessageDefinition jsonObject)
	{
		messageObject = jsonObject;
		if (jsonObject?.ValidationErrors != null)
		{
			listValidationErrorProviders = new List<PBNValidationErrorsProvider>();
			foreach (var validationError in jsonObject.ValidationErrors)
			{
				listValidationErrorProviders.Add(new PBNValidationErrorsProvider(validationError));
			}
		}
	}

	readonly CreateAndUpdatePBNMessageDefinition messageObject;

	public ZString Status => messageObject.Status ?? ZString.Empty;

	public ZString PbnID => messageObject.PbnID ?? ZString.Empty;

	public ZString Issue => messageObject.Issue ?? ZString.Empty;

	public List<PBNValidationErrorsProvider> listValidationErrorProviders;
}
