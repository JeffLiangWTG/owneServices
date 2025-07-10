using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Export.Outgoing;

namespace Enterprise.Customs.BR.Business.Export
{
	public class DeclarationDeclarantProvider : IDeclarationDeclarant
	{
		public DeclarationDeclarantProvider(CusEntryInstruction entryInstruction)
		{
			this.entryInstruction = Argument.NotNull(entryInstruction, nameof(entryInstruction));
			declaration = entryInstruction.JobDeclaration;
		}

		readonly CusEntryInstruction entryInstruction;

		readonly JobDeclaration declaration;

		public string ID => (declaration != null && declaration.JE_DeclarantType == TypeOfOperationExportList.Codes._1001 ? declaration.Supplier : declaration?.DeclarantAddress?.Header)?.GetCNPJOrCPF() ?? string.Empty;

		public string ContactName => entryInstruction.JustificationContactDetailAddress?.E2_Contact;

		public string ContactEmail => entryInstruction.JustificationContactDetailAddress?.E2_Email;

		public string ContactPhone
		{
			get
			{
				var phoneResult = string.Empty;

				var contactDetailAddress = entryInstruction.JustificationContactDetailAddress;
				if (contactDetailAddress != null)
				{
					if (!contactDetailAddress.E2_Mobile.IsEmpty)
					{
						phoneResult = contactDetailAddress.E2_Mobile_FormattedLocalNumberIfLoggedInSameCountry.KeepNumericCharacters();
					}
					else if (!contactDetailAddress.E2_Phone.IsEmpty)
					{
						phoneResult = contactDetailAddress.E2_Phone_FormattedLocalNumberIfLoggedInSameCountry.KeepNumericCharacters();
					}
				}

				return phoneResult;
			}
		}
	}
}
