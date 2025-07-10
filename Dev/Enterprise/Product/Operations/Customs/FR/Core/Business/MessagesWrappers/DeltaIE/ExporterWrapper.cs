using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class ExporterWrapper : IExporter
	{
		ExporterWrapper(OrgAddress exporterAddress)
		{
			this.exporterAddress = exporterAddress;
		}
		readonly OrgAddress exporterAddress;

		public static ExporterWrapper New(OrgAddress exporterAddress) => exporterAddress == null ? null : new ExporterWrapper(exporterAddress);

		public IAddress Address => address ?? (address = exporterAddress != null && IdentificationNumber.IsNullOrEmpty() ? OrganisationAddressWrapper.New(exporterAddress) : null);
		IAddress address;

		public string IdentificationNumber => identificationNumber ?? (identificationNumber = exporterAddress.GetEuIdentificationNumber());
		string identificationNumber;

		public string Name => name ?? (IdentificationNumber.IsNullOrEmpty() ? name = exporterAddress.Header?.OH_FullName : null);
		string name;
	}
}
