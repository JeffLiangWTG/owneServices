using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class PersonPayingCustomsDutyWrapper : IPersonPayingCustomsDuty
	{
		PersonPayingCustomsDutyWrapper(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
		}
		readonly JobDeclaration declaration;

		public string IdentificationNumber => identificationNumber ?? (identificationNumber = declaration.DocAddresses?.Cast<JobDocAddress>().FirstOrDefault(x => x.E2_AddressType == "DFP")?.Organisation?.GetEuIdentificationNumber() ?? string.Empty);
		string identificationNumber;

		public static PersonPayingCustomsDutyWrapper New(JobDeclaration declaration) => declaration == null ? null : new PersonPayingCustomsDutyWrapper(declaration);
	}
}
