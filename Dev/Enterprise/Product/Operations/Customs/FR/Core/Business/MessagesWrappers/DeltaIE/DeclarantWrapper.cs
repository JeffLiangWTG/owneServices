using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class DeclarantWrapper : IDeclarant
	{
		DeclarantWrapper(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
		}

		public DeclarantWrapper(bool isForOperationalAction)
		{
			this.isForOperationalAction = isForOperationalAction;
		}

		protected readonly JobDeclaration declaration;
		readonly bool isForOperationalAction;

		public IAddress Address => address ?? (address = OrgAddress != null && IdentificationNumber.IsNullOrEmpty() ? OrganisationAddressWrapper.New(OrgAddress) : null);
		IAddress address;

		public IContactPerson ContactPerson => contactPerson ?? (contactPerson = (isForOperationalAction || (declaration?.JE_GS_NKCusAgent.IsEmpty ?? true)) ? null : ContactPersonWrapper.New(declaration.CusAgent, declaration.Branch));
		IContactPerson contactPerson;

		public string IdentificationNumber => identificationNumber ?? (identificationNumber = OrgAddress?.GetEuIdentificationNumber() ?? string.Empty);
		string identificationNumber;

		public string Name => name ?? (IdentificationNumber.IsNullOrEmpty() ? name = OrgHeader?.OH_FullName : null);
		string name;

		OrgAddress OrgAddress => isForOperationalAction ? (GlbBranch.CurrentBranch?.OrgProxy ?? GlbCompany.CurrentCompany?.OrgProxy).MainAddress : declaration?.Declarant;

		OrgHeader OrgHeader => OrgAddress?.Header;

		public static DeclarantWrapper New(JobDeclaration declaration) => declaration == null ? null : new DeclarantWrapper(declaration);

		public static DeclarantWrapper New(bool isForOperationalAction) => new DeclarantWrapper(isForOperationalAction);
	}
}
