using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class OrganizationWrapper : IOrganization
	{
		public OrganizationWrapper(OrgHeader orgHeader, string role = null)
		{
			this.orgHeader = Argument.NotNull(orgHeader, nameof(orgHeader));
			this.Role = role;
		}

		readonly OrgHeader orgHeader;

		public static OrganizationWrapper New(OrgHeader orgHeader) => orgHeader == null ? null : new OrganizationWrapper(orgHeader);

		public static OrganizationWrapper New(OrgHeader orgHeader, string role) => orgHeader == null ? null : new OrganizationWrapper(orgHeader, role);

		public string IdentificationNumber => identificationNumber ?? (identificationNumber = orgHeader.MainAddress.GetEORI().Left(17));
		string identificationNumber;

		public string Name => name ?? (name = orgHeader.OH_FullName);
		string name;

		public IAddress Address => address ?? (address = AddressWrapper.New(orgHeader.MainAddress));
		IAddress address;

		public string Role { get; }
	}
}
