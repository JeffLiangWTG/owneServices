using System.Collections.Generic;
using CargoWise.Customs.IE.MessageDefinitions.PBN;
using CargoWise.Types;

namespace Enterprise.Customs.IE.PBN.Messaging;

public class LPBPBNProvider
{
	public LPBPBNProvider(LPBDefinition jsonObject)
	{
		messageObject = jsonObject;
		Declarations = messageObject?.Declarations != null ? [] : null;
		if (Declarations != null)
		{
			foreach (var declaration in messageObject.Declarations)
			{
				Declarations.Add(new PBNDeclarationProvider(declaration));
			}
		}
		ContactDetails = new PBNContactDetailsProvider(messageObject.ContactDetails);
	}

	public ZString PbnID => messageObject.PbnID ?? ZString.Empty;

	public ZString Status => messageObject.Status ?? ZString.Empty;

	public ZString Issue => messageObject.Issue ?? ZString.Empty;

	public ZString Direction => messageObject.Direction ?? ZString.Empty;

	public bool EmptyVehicle => messageObject.EmptyVehicle;

	public List<PBNDeclarationProvider> Declarations;

	public PBNContactDetailsProvider ContactDetails;

	readonly LPBDefinition messageObject;
}
