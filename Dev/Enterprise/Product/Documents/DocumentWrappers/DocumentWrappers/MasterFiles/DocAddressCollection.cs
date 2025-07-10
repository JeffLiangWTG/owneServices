using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocAddressCollection : DocumentWrapperCollection
	{
		public DocAddressCollection(BusinessObjectFactory factoryForWrapper)
			: base(factoryForWrapper)
		{
		}

		public DocAddressCollection(OrgAddressDependentCollection collectionSource, BusinessObjectFactory factoryForWrapper)
			: base(collectionSource, factoryForWrapper)
		{
		}

		public DocAddressCollection(OrgAddressCollection collectionSource, BusinessObjectFactory factoryForWrapper)
			: base(collectionSource, factoryForWrapper)
		{
		}

		public new DocAddress this[int index]
		{
			get { return (DocAddress)base[index]; }
		}

		public DocAddressCollection GetAddressesWithWareHousing()
		{
			DocAddressCollection addressesWithWareHousing = new DocAddressCollection(Factory);
			foreach (DocAddress currentAddress in this)
			{
				ZBool contained = ZBool.False;
				foreach (DocAddress add in addressesWithWareHousing)
				{
					if (((BusinessObject)currentAddress.WrappedObject).PK == ((BusinessObject)add.WrappedObject).PK)
					{
						contained = ZBool.True;
						break;
					}
				}
				if (!contained)
				{
					if (currentAddress.HasWareHousing || currentAddress.HasLoadingUnloadingConstraints)
					{
						addressesWithWareHousing.Add(currentAddress);
					}
				}
			}
			return addressesWithWareHousing;
		}
	}
}
