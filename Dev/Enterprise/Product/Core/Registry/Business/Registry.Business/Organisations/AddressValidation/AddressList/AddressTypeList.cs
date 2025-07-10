using System;
using System.Collections;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Registry.Business
{
	public class AddressTypeList : AddressValidationList<DocAddressType>
	{
		public AddressTypeList()
		{
			BuildList();
		}

		protected override IEnumerable GetList()
		{
			return Enum.GetValues(typeof(DocAddressType));
		}

		protected override string GetItemToString(DocAddressType item)
		{
			return item.ToString();
		}
	}
}
