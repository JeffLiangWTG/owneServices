using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.Management.Matching
{
	public class OrganizationAddressMatchingComparator : IEqualityComparer<OrganizationAddress>
	{
		public bool Equals(OrganizationAddress x, OrganizationAddress y)
		{
			return x.OrganizationCode == y.OrganizationCode
				&& x.AddressShortCode == y.AddressShortCode
				&& x.CompanyName == y.CompanyName
				&& x.Address1 == y.Address1
				&& x.Address2 == y.Address2
				&& x.City == y.City
				&& x.State == y.State
				&& x.Postcode == y.Postcode
				&& GetCode(x.Port) == GetCode(y.Port)
				&& GetCode(x.Country) == GetCode(y.Country)
				&& x.Contact == y.Contact
				&& x.Mobile == y.Mobile
				&& x.Email == y.Email
				&& x.Phone == y.Phone
				&& x.Fax == y.Fax
				&& x.GovRegNum == y.GovRegNum
				&& GetCode(x.GovRegNumType) == GetCode(y.GovRegNumType)
				&& GetRegNos(x.RegistrationNumberCollection) == GetRegNos(y.RegistrationNumberCollection)
				&& x.UniversalNettingCode == y.UniversalNettingCode
				&& x.UniversalOfficeCode == y.UniversalOfficeCode;
		}

		public int GetHashCode(OrganizationAddress o)
		{
			if (o == null)
			{
				return 0;
			}

			var hashKey = new string[]
			{
				o.CompanyName.GetValueOrDefault(),
				o.Address1.GetValueOrDefault(),
				o.Address2.GetValueOrDefault(),
				o.City.GetValueOrDefault(),
				((ZString?)o.State).GetValueOrDefault(),
				o.Postcode.GetValueOrDefault(),
				o.Contact.GetValueOrDefault(),
				o.Mobile.GetValueOrDefault(),
				o.Email.GetValueOrDefault(),
				o.Phone.GetValueOrDefault(),
				o.Fax.GetValueOrDefault(),
				o.GovRegNum.GetValueOrDefault(),
			};

			return string.Join("|", hashKey).GetHashCode();
		}

		#region Implementation

		static string GetCode(ICodeDataObject codeSource)
		{
			return codeSource == null ? null : codeSource.Code.GetValueOrDefault();
		}

		static string GetCode(UNLOCO codeSource)
		{
			return codeSource == null ? null : codeSource.Code.GetValueOrDefault();
		}

		static string GetRegNos(List<RegistrationNumber> list)
		{
			if (list == null)
			{
				return null;
			}

			var result = new List<string>();
			foreach (var item in list)
			{
				result.Add(GetCode(item.Type) + "|" + item.Value.GetValueOrDefault().ToString() + "|" + GetCode(item.CountryOfIssue));
			}

			return string.Join("\r\n", result.ToArray());
		}

		#endregion
	}
}
