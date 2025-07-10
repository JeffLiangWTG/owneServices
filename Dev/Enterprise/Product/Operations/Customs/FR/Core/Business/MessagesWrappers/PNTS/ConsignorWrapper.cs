using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS
{
	public class ConsignorWrapper : IConsignor
	{
		ConsignorWrapper(TemporaryStorageBill bill)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
			this.shipper = bill.Shipper;
		}
		readonly TemporaryStorageBill bill;
		readonly OrgAddress shipper;

		public IAddressParty Address => address ?? (address = AddressPartyWrapper.New(shipper));
		IAddressParty address;

		public ICollection<ICommunication> Communication => communication ?? (communication = GetCommunication());
		ICollection<ICommunication> communication;

		ICollection<ICommunication> GetCommunication()
		{
			var result = new Collection<ICommunication>();
			shipper.Header?.Contacts?.Cast<OrgContact>().Where(x => x.Allocations.Cast<OrgContactAllocation>().Any(alloc => alloc.PC_Type == OrgConstants.ContactAllocationType.CUS))?.ForEach(contact => result.Add(CommunicationWrapper.New(contact)));
			return result;
		}

		public string IdentificationNumber => identificationNumber ?? (identificationNumber = shipper.GetEuIdentificationNumber());
		string identificationNumber;

		public string Name => name ?? (name = shipper.Header?.OH_FullName);
		string name;

		public byte TypeOfPerson => typeOfPerson.Equals(ZByte.Zero) ? typeOfPerson = ZByte.ParseSafe(bill.ABL_ShipperRegNoType.ToString(), ZByte.Zero) : typeOfPerson;
		byte typeOfPerson;

		public static ConsignorWrapper New(TemporaryStorageBill bill) => bill != null && bill.Shipper != null ? new ConsignorWrapper(bill) : null;
	}
}
