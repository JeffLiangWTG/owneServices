using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[CustomIndexerList(typeof(AddressTypeList))]
	public class AddressWrapperCollection : GenericWrapperCollection<AddressWrapper>
	{
		public AddressWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public AddressWrapperCollection(OrgHeader organisation, BusinessObjectFactory factory)
			: base(factory)
		{
			if (organisation != null)
			{
				Organisation = organisation;
				Load(organisation.Addresses);
			}
		}
		readonly OrgHeader Organisation;

		#region Overrides

		#region WrapObject

		protected override DocumentWrapper WrapObject(object objectToWrap)
		{
			var addressToWrap = (OrgAddress)objectToWrap;
			return new AddressWrapper(addressToWrap, ContactType.All, Factory);
		}

		#endregion

		#region GetRow

		protected override IBODocDataProvider GetRow(ZString index)
		{
			if (Organisation != null)
			{
				OrgAddress addressBO = new AddressTypeList().GetAddressUsingFallbackIfTypeCodeRecognised(Organisation, index);
				if (addressBO != null)
				{
					foreach (AddressWrapper wrapper in this)
					{
						if (addressBO == wrapper.WrappedObject)
						{
							return wrapper;
						}
					}
				}
			}
			return base.GetRow(index);
		}

		#endregion

		#endregion

		#region GetAddressesWithWarehousing

		public AddressWrapperCollection GetAddressesWithWarehousing()
		{
			var result = new AddressWrapperCollection(Factory);
			foreach (AddressWrapper address in this)
			{
				var containsAddress = result.Cast<AddressWrapper>().Any(a => a.SameAs(address));
				if (!containsAddress && (address.HasWarehousing || address.HasLoadingUnloadingConstraints))
				{
					result.Add(address);
				}
			}

			return result;
		}

		#endregion
	}
}
