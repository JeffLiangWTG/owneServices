using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class AddressWrapperWithIDocDocAddressCollection : GenericWrapperCollection<AddressWrapperWithIDocDocAddress>
	{
		public AddressWrapperWithIDocDocAddressCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Overrides

		#region WrapObject

		protected override DocumentWrapper WrapObject(object objectToWrap)
		{
			var addressToWrap = (OrgAddress)objectToWrap;
			return new AddressWrapperWithIDocDocAddress(addressToWrap, ContactType.All, Factory);
		}

		#endregion

		#endregion

		#region GetAddressesWithWarehousing

		public AddressWrapperWithIDocDocAddressCollection GetAddressesWithWarehousing()
		{
			var result = new AddressWrapperWithIDocDocAddressCollection(Factory);
			foreach (AddressWrapperWithIDocDocAddress address in this)
			{
				var containsAddress = result.Cast<AddressWrapperWithIDocDocAddress>().Any(a => a.SameAs(address));
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
