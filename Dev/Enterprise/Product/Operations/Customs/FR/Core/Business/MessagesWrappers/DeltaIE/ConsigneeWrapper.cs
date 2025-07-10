
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class ConsigneeWrapper : IConsignee
	{
		ConsigneeWrapper(OrgAddress consigneeAddress)
		{
			this.consigneeAddress = consigneeAddress;
		}
		readonly OrgAddress consigneeAddress;

		public static ConsigneeWrapper New(OrgAddress consignee) => consignee == null ? null : new ConsigneeWrapper(consignee);

		public IAddress Address => address ?? (address = consigneeAddress != null && IdentificationNumber.IsNullOrEmpty() ? OrganisationAddressWrapper.New(consigneeAddress) : null);
		IAddress address;

		public string IdentificationNumber => identificationNumber ?? (identificationNumber = consigneeAddress.GetEuIdentificationNumber());
		string identificationNumber;

		public string Name => name ?? (IdentificationNumber.IsNullOrEmpty() ? name = consigneeAddress.Header?.OH_FullName : null);
		string name;
	}
}
