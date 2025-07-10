using System.Collections.Generic;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage
{
	public class PersonProvider : IPerson
	{
		public static PersonProvider New(OrgAddress orgAddress, ZString regNo, ZString regNoType, ZString overridePhone)
		{
			PersonProvider result = null;
			if (orgAddress != null)
			{
				result = new PersonProvider(orgAddress, regNo, regNoType, overridePhone);
			}
			return result;
		}

		PersonProvider(OrgAddress orgAddress, ZString regNo, ZString regNoType, ZString overridePhone)
		{
			this.orgAddress = orgAddress;
			this.regNo = regNo;
			this.regNoType = regNoType;
			this.overridePhone = overridePhone;
		}
		readonly OrgAddress orgAddress;
		readonly ZString regNo;
		readonly ZString regNoType;
		readonly ZString overridePhone;

		public string TypeOfPerson => regNoType;

		public IFullAddress Address => address ?? (address = FullAddressProvider.New(orgAddress));
		IFullAddress address;

		public IReadOnlyCollection<IIdType> Communication => communications ?? (communications = new[] { CommunicationProvider.New(orgAddress.Header?.AllocatedContacts.GetAllocatedContact(OrgConstants.ContactAllocationType.CUS), overridePhone) });
		IReadOnlyCollection<IIdType> communications;

		public string Name => orgAddress.EffectiveCompanyName;

		public string Id => regNo;
	}
}
