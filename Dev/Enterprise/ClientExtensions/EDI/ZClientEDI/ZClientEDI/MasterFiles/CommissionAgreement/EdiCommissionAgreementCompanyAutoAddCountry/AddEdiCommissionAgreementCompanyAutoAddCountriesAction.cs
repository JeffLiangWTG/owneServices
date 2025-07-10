using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class AddEdiCommissionAgreementCompanyAutoAddCountriesAction : NonPersistentBusinessObject
	{
		public AddEdiCommissionAgreementCompanyAutoAddCountriesAction(EdiCommissionAgreementCustomization customization, params ZGuid[] databasePks)
			: base(customization.Factory)
		{
			this.customization = customization;
			this.databasePks = databasePks;
		}

		#region Fields

		readonly EdiCommissionAgreementCustomization customization;
		readonly ZGuid[] databasePks;

		#endregion

		public void Execute()
		{
			foreach (var databasePk in databasePks)
			{
				var existingCompanyCountries = new HashSet<ZString>();
				foreach (var companyCountry in customization.CompanyAutoAddCountries.Where(x => x.EPC_LD == databasePk))
				{
					existingCompanyCountries.Add(companyCountry.EPC_RN_NKCountry);
				}

				foreach (AddEdiCommissionAgreementCompanyAutoAddCountryItem item in Items)
				{
					if (!existingCompanyCountries.Contains(item.CountryCode))
					{
						customization.CompanyAutoAddCountries.AddNew(databasePk, item.CountryCode);
					}
				}
			}
		}

		#region Related Business Objects

		#region Items

		public AddEdiCommissionAgreementCompanyAutoAddCountryItemCollection Items
		{
			get
			{
				if (items == null)
				{
					items = new AddEdiCommissionAgreementCompanyAutoAddCountryItemCollection(Factory);
					RegisterEditableChildObject(items);
				}

				return items;
			}
		}
		AddEdiCommissionAgreementCompanyAutoAddCountryItemCollection items;

		#endregion

		#endregion
	}
}
