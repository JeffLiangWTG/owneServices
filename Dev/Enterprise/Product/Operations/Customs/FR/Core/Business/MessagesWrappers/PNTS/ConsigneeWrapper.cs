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
	public class ConsigneeWrapper : IConsignee
	{
		ConsigneeWrapper(TemporaryStorageBill bill)
		{
			this.bill = bill;
			consignee = bill.Consignee;
		}
		readonly TemporaryStorageBill bill;
		readonly OrgAddress consignee;

		public IAddressParty Address => address ?? (address = AddressPartyWrapper.New(consignee));
		IAddressParty address;

		public ICollection<ICommunication> Communication => communication ?? (communication = GetCommunication());
		ICollection<ICommunication> communication;

		ICollection<ICommunication> GetCommunication()
		{
			var result = new Collection<ICommunication>();
			consignee.Header?.Contacts?.Cast<OrgContact>().Where(x => x.Allocations.Cast<OrgContactAllocation>().Any(alloc => alloc.PC_Type == OrgConstants.ContactAllocationType.CUS))?.ForEach(contact => result.Add(CommunicationWrapper.New(contact)));
			return result;
		}

		public string IdentificationNumber => identificationNumber ?? (identificationNumber = consignee.GetEuIdentificationNumber());
		string identificationNumber;

		public string Name => name ?? (name = consignee.Header.OH_FullName);
		string name;

		public byte TypeOfPerson => typeOfPerson.Equals(ZByte.Zero) ? typeOfPerson = ZByte.ParseSafe(bill.ABL_ConsigneeRegNoType.ToString(), ZByte.Zero) : typeOfPerson;
		byte typeOfPerson;

		public static ConsigneeWrapper New(TemporaryStorageBill bill) => bill != null && bill.Consignee != null ? new ConsigneeWrapper(bill) : null;
	}
}
