using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Registry.Business
{
	public class CountryListElement : NonPersistentBusinessObject, IObsoleteValidation
	{
		public CountryListElement(CountryListCollection parent)
		{
			this.parent = parent;
		}

		public IBusinessObjectCollection CountryCollection
		{
			get { return parent.CountryCollection; }
		}

		#region CountryPK

		public ZGuid CountryPK
		{
			get { return countryPK; }
			set
			{
				SetNonPersistentPropertyValue<ZGuid>(CountryPKInfo, ref countryPK, value);
				if (!IsValidationSuspended)
				{
					ValidateCountryPK();
				}
			}
		}

		public ZPropertyInfo CountryPKInfo
		{
			get { return GetZPropertyInfo(nameof(CountryPK)); }
		}

		public void ValidateCountryPK()
		{
			CountryPKInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(CountryPKInfo);

			if (parent?.Where(c => c.CountryPK == CountryPK).Count() > 1)
			{
				CountryPKInfo.AddError(Res.GetString("222b74dc-7d2a-4d1d-aa22-5894ab6c2b22", "Remove Duplicate Countries: No Duplicates Allowed."));
			}
		}

		ZGuid countryPK;

		#endregion

		#region CountryName

		public ZString CountryName
		{
			get
			{
				var country = parent.Factory.Load<IRefCountry>(countryPK);
				return country != null ? country.RN_Desc : ZString.Empty;
			}
		}

		#endregion

		readonly CountryListCollection parent;
	}
}
