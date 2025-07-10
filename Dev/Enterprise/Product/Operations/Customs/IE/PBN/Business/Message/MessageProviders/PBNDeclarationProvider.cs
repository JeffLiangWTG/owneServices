using CargoWise.Customs.IE.MessageContracts.Interfaces.PBN;

namespace Enterprise.Customs.IE.PBN.Business
{
	public class PBNDeclarationProvider : IPBNDeclaration
	{
		public static PBNDeclarationProvider New(PBNReferenceItem declaration)
		{
			if (declaration == null)
			{
				return null;
			}
			return new PBNDeclarationProvider(declaration);
		}

		PBNDeclarationProvider(PBNReferenceItem declaration)
		{
			this.declaration = declaration;
		}
		readonly PBNReferenceItem declaration;

		public string DeclarationId => declaration.CSI_ReferenceNumber;

		public string DeclarationType => null;
	}
}
