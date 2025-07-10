using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocDocAddressCollection : DocumentWrapperCollection
	{
		public DocDocAddressCollection(BusinessObjectFactory factoryForWrapper)
			: base(factoryForWrapper)
		{
		}

		public DocDocAddressCollection(MasterFiles.Business.OrgAddressDependentCollection collectionSource, BusinessObjectFactory factoryForWrapper)
			: base(collectionSource, factoryForWrapper)
		{
		}

		public new DocDocAddress this[int index]
		{
			get { return (DocDocAddress)base[index]; }
		}

		public DocDocAddressCollection GetDocAddressesWithWareHousing()
		{
			DocDocAddressCollection addressesWithWareHousing = new DocDocAddressCollection(Factory);
			foreach (DocDocAddress currentAddress in this)
			{
				ZBool contained = ZBool.False;
				foreach (DocDocAddress add in addressesWithWareHousing)
				{
					if (((BusinessObject)currentAddress.WrappedObject).PK == ((BusinessObject)add.WrappedObject).PK)
					{
						contained = ZBool.True;
						break;
					}
					else if (DocDocAddress.AreTheseDocAddressesTheSameOrgAddresses(currentAddress, add))
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
