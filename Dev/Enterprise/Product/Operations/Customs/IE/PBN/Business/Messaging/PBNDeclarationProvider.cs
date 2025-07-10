using CargoWise.Customs.IE.MessageDefinitions.PBN;
using CargoWise.Types;

namespace Enterprise.Customs.IE.PBN.Messaging
{
	public class PBNDeclarationProvider
	{
		public PBNDeclarationProvider(PBNDeclaration pbnDeclaration)
		{
			jsonObject = pbnDeclaration;
		}

		public ZString DeclarationId => jsonObject.DeclarationId ?? ZString.Empty;

		public ZString DeclarationType => jsonObject.DeclarationType ?? ZString.Empty;

		readonly PBNDeclaration jsonObject;
	}
}
