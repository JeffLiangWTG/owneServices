using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Duimp.Outgoing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Duimp
{
	public class IdentificationProvider : IIdentification
	{
		IdentificationProvider(CusEntryInstruction entryInstruction)
		{
			this.entryInstruction = Argument.NotNull(entryInstruction, nameof(entryInstruction));
			declaration = Argument.NotNull(entryInstruction.JobDeclaration, nameof(entryInstruction.JobDeclaration));
		}

		public static IdentificationProvider New(CusEntryInstruction entryInstruction) => entryInstruction == null ? null : new IdentificationProvider(entryInstruction);

		readonly CusEntryInstruction entryInstruction;
		readonly JobDeclaration declaration;

		public string AdditionalInformation => entryInstruction.AdditionalInformationConcatenated;

		public string ImporterRegistrationNumber => declaration.Importer?.GetCNPJOrCPF();

		public string ImporterNumberType
		{
			get
			{
				return declaration.Importer?.PrimaryRegistrationNumber?.NumberType.ToString() switch
				{
					BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ => Constants.ImporterRegistrationNumberTypes.CNPJ,
					BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration => Constants.ImporterRegistrationNumberTypes.CPF,
					_ => null,
				};
			}
		}
	}
}
