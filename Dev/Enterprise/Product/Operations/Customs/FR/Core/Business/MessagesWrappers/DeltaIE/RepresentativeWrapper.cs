using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class RepresentativeWrapper : IRepresentative
	{
		RepresentativeWrapper(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
		}
		readonly JobDeclaration declaration;

		public IContactPerson ContactPerson => contactPerson ?? (contactPerson = ContactPersonWrapper.New(declaration.CusAgent, declaration.Branch));
		IContactPerson contactPerson;

		public string IdentificationNumber => identificationNumber ??= declaration?.Representative?.GetEuIdentificationNumber() ?? ZString.Empty;
		string identificationNumber;

		public string Status => declaration.RepresentationTypeNo;

		public static RepresentativeWrapper New(JobDeclaration declaration) => declaration == null ? null : new RepresentativeWrapper(declaration);
	}
}
