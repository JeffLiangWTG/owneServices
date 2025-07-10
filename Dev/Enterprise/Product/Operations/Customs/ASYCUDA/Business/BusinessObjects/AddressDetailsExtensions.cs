using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public static class AddressDetailsExtensions
	{
		/// <summary>
		/// Copies the given address to the given organization.
		/// If the organization is null, then nothing is copied.
		/// Note: The contact field is not copied.
		/// </summary>
		/// <param name="address">The address to copy.</param>
		/// <param name="org">The organization to copy address to.</param>
		/// <exception cref="ArgumentNullException">If the address is null.</exception>
		public static void CopyTo(this IAddressDetails address, OrgHeader org)
		{
			if (address == null)
			{
				// It is wrong to try to copy null, regardless of the destination.
				throw new ArgumentNullException(nameof(address));
			}

			if (org == null)
			{
				return;
			}

			org.OH_FullName = address.CompanyName;

			OrgAddress orgAddress = org.MainAddress;
			if (orgAddress != null)
			{
				orgAddress.OA_Phone = address.Phone;
				orgAddress.OA_Fax = address.Fax;
				orgAddress.OA_Email = address.Email;
				orgAddress.OA_Address1 = address.AddressLine1;
				orgAddress.OA_Address2 = address.AddressLine2;
				orgAddress.OA_City = address.City;
				orgAddress.OA_State = address.State;
				orgAddress.OA_PostCode = address.PostCode;
				orgAddress.OA_RN_NKCountryCode = address.Country;
			}
		}

		/// <summary>
		/// Returns true if the given address is null or all its fields are empty; otherwise, false.
		/// </summary>
		public static bool AreEmpty(this IAddressDetails address)
		{
			if (address == null)
			{
				return true;
			}

			return
				address.CompanyName.IsEmpty &&
				address.ContactName.IsEmpty &&
				address.Phone.IsEmpty &&
				address.Fax.IsEmpty &&
				address.Email.IsEmpty &&
				address.AddressLine1.IsEmpty &&
				address.AddressLine2.IsEmpty &&
				address.City.IsEmpty &&
				address.State.IsEmpty &&
				address.PostCode.IsEmpty &&
				address.Country.IsEmpty;
		}
	}
}
