using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class ImporterWrapper : IImporter
	{
		ImporterWrapper(OrgHeader header, OrgAddress address)
		{
			this.header = Argument.NotNull(header, nameof(header));
			this.importerAddress = address;
		}
		readonly OrgHeader header;
		readonly OrgAddress importerAddress;

		public static ImporterWrapper New(OrgHeader header, OrgAddress importerAddress) => header == null ? null : new ImporterWrapper(header, importerAddress);

		public IAddress Address => address ?? (address = importerAddress != null && IdentificationNumber.IsNullOrEmpty() ? OrganisationAddressWrapper.New(importerAddress) : null);
		IAddress address;

		public string IdentificationNumber => identificationNumber ?? (identificationNumber = header.GetEuIdentificationNumber());
		string identificationNumber;

		public string Name => name ?? (IdentificationNumber.IsNullOrEmpty() ? name = header.OH_FullName : null);
		string name;
	}
}
